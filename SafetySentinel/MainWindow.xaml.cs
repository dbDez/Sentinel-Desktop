using SafetySentinel.Data;
using SafetySentinel.Models;
using SafetySentinel.Services;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Win32;

namespace SafetySentinel
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseManager _db = new();
        private readonly IntelligenceService _intel;
        private readonly ThreatScoringEngine _scoring = new();
        private readonly ScanScheduler _scheduler = new();
        private readonly DispatcherTimer _clockTimer = new();
        private UserProfile _profile = new();
        private List<CountryProfile> _countries = new();
        private List<CrimeHotspot> _hotspots = new();

        // Chat state
        private bool _briefChatOpen = false;
        private bool _historyChatOpen = false;
        private readonly List<(string role, string text)> _briefChatHistory = new();
        private readonly List<(string role, string text)> _historyChatHistory = new();
        private int _currentBriefId = -1; // tracks which brief the chat belongs to

        // Scan progress
        private static readonly string[] _scanStages = new[]
        {
            "Initiating secure connection...",
            "Researching current threat landscape...",
            "Scanning personal alert conditions...",
            "Analyzing country threat profiles...",
            "Cross-referencing watchlist entries...",
            "Evaluating genocide risk indicators...",
            "Assessing evacuation readiness...",
            "Waiting for intelligence analysis...",
            "Processing threat assessments...",
            "Compiling intelligence brief...",
            "Finalizing threat dashboard..."
        };

        // Geocoding
        private static readonly HttpClient _geocodeHttp = new();

        public MainWindow()
        {
            InitializeComponent();
            _intel = new IntelligenceService(_db);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] MainWindow initialized.");
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Window_Loaded fired.");
                UpdateStatus("Loading database...");

                // Load data
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Loading profile...");
                _profile = _db.GetProfile();
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Loading countries...");
                _countries = _db.GetAllCountries();
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Loading hotspots...");
                _hotspots = _db.GetActiveHotspots();
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Data loaded: {_countries.Count} countries, {_hotspots.Count} hotspots.");

                // Populate UI
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Populating UI...");
                LoadProfileToUI();
                LoadAlertCategories();
                LoadPersonalAlerts();
                LoadLatestBrief();
                LoadWatchlist();
                LoadExitPlans();
                LoadBriefHistory();
                LoadSettings();

                // Update header
                UpdateThreatLevelHeader();
                UpdateCountryCount();
                UpdateApiUsageDisplay();
                SettingsDbPath.Text = $"Database: {_db.GetDatabasePath()}";
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] DB path: {_db.GetDatabasePath()}");

                // Clock timer
                _clockTimer.Interval = TimeSpan.FromSeconds(1);
                _clockTimer.Tick += (_, _) => HeaderTime.Text = DateTime.Now.ToString("HH:mm:ss | ddd dd MMM yyyy");
                _clockTimer.Start();
                HeaderTime.Text = DateTime.Now.ToString("HH:mm:ss | ddd dd MMM yyyy");

                // Initialize WebView2 for globe
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Initializing WebView2 globe...");
                await InitializeGlobe();
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Globe initialized.");

                // Setup scheduler events
                _scheduler.OnDailyBriefDue += async () => await Dispatcher.InvokeAsync(async () => await RunScan());
                _scheduler.OnStatusUpdate += msg => Dispatcher.Invoke(() => SchedulerStatus.Text = msg);

                UpdateStatus("Ready. Configure API key in Settings, then click Scan Now.");
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✓ Startup complete — Ready.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] STARTUP ERROR: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                MessageBox.Show($"Startup error: {ex.Message}", "Safety Sentinel", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Globe

        private async Task InitializeGlobe()
        {
            try
            {
                var env = await CoreWebView2Environment.CreateAsync();
                await GlobeWebView.EnsureCoreWebView2Async(env);

                string globePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WebContent", "globe.html");
                if (File.Exists(globePath))
                {
                    var globeReadyTcs = new TaskCompletionSource<bool>();
                    GlobeWebView.CoreWebView2.WebMessageReceived += (_, args) =>
                    {
                        if (args.TryGetWebMessageAsString() == "GLOBE_READY")
                            globeReadyTcs.TrySetResult(true);
                    };

                    GlobeWebView.CoreWebView2.Navigate(new Uri(globePath).AbsoluteUri);

                    var timeoutTask = Task.Delay(8000);
                    await Task.WhenAny(globeReadyTcs.Task, timeoutTask);

                    await LoadGlobeData();
                }
                else
                {
                    UpdateStatus("Globe HTML not found — check WebContent/globe.html");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"WebView2 error: {ex.Message}. Install WebView2 Runtime.");
            }
        }

        private async Task LoadGlobeData(ExecutiveBrief? briefContext = null)
        {
            try
            {
                // Determine which countries are "active" (in the watchlist for this brief)
                HashSet<string> activeCodes;
                string briefLabel;

                if (briefContext != null && !string.IsNullOrEmpty(briefContext.WatchlistSnapshot))
                {
                    var snapshotCodes = JsonConvert.DeserializeObject<List<string>>(briefContext.WatchlistSnapshot) ?? new();
                    activeCodes = new HashSet<string>(snapshotCodes, StringComparer.OrdinalIgnoreCase);
                    briefLabel = $"Threat Zones for Intelligence Report dated {briefContext.BriefDate:dd MMM yyyy} at {briefContext.BriefDate:HH:mm} hrs";
                }
                else
                {
                    // Live view: use current watchlist + always include home country
                    var watchlistCodes = _db.GetWatchlist().Select(w => w.CountryCode).ToList();
                    activeCodes = new HashSet<string>(watchlistCodes, StringComparer.OrdinalIgnoreCase);
                    var latestBrief = _db.GetAllBriefs().OrderByDescending(b => b.BriefDate).FirstOrDefault();
                    briefLabel = latestBrief != null
                        ? $"Threat Zones for Intelligence Report dated {latestBrief.BriefDate:dd MMM yyyy} at {latestBrief.BriefDate:HH:mm} hrs"
                        : "Threat Zones — no intelligence brief yet";
                }

                // Always include home country in the active set
                if (!string.IsNullOrEmpty(_profile.CurrentCountry))
                    activeCodes.Add(_profile.CurrentCountry);

                var countryData = _countries.Select(c => new
                {
                    code = c.CountryCode, name = c.CountryName,
                    lat = c.Latitude, lng = c.Longitude,
                    overall = c.OverallSafetyScore,
                    physical = c.PhysicalSecurity, political = c.PoliticalStability,
                    economic = c.EconomicFreedom, digital = c.DigitalSovereignty,
                    health = c.HealthEnvironment, social = c.SocialCohesion,
                    mobility = c.MobilityExit, infrastructure = c.Infrastructure,
                    cbdc = c.CbdcStatus, surveillance = c.SurveillanceScore,
                    genocide = c.GenocideStage, cashFreedom = c.CashFreedomScore,
                    active = activeCodes.Contains(c.CountryCode)
                });
                string countryJson = JsonConvert.SerializeObject(countryData);
                await GlobeWebView.ExecuteScriptAsync($"loadCountryData({countryJson})");

                // Brief date label
                string escapedLabel = briefLabel.Replace("'", "\\'");
                await GlobeWebView.ExecuteScriptAsync($"setBriefLabel('{escapedLabel}')");

                var hotspotData = _hotspots.Select(h => new
                {
                    lat = h.Latitude, lng = h.Longitude,
                    name = h.LocationName, type = h.CrimeType,
                    severity = h.Severity, radius = h.RadiusMeters,
                    incidents = h.IncidentCount90d
                });
                string hotspotJson = JsonConvert.SerializeObject(hotspotData);
                await GlobeWebView.ExecuteScriptAsync($"loadHotspotData({hotspotJson})");

                if (_profile.HomeLatitude != 0 || _profile.HomeLongitude != 0)
                {
                    await GlobeWebView.ExecuteScriptAsync(
                        $"setHomePosition({_profile.HomeLatitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {_profile.HomeLongitude.ToString(System.Globalization.CultureInfo.InvariantCulture)})");
                }
                else if (!string.IsNullOrEmpty(_profile.CurrentCountry))
                {
                    var homeCountry = _countries.FirstOrDefault(c => c.CountryCode == _profile.CurrentCountry);
                    if (homeCountry != null)
                    {
                        await GlobeWebView.ExecuteScriptAsync(
                            $"setHomePosition({homeCountry.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {homeCountry.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)})");
                    }
                }
            }
            catch { /* Globe not ready yet */ }
        }

        #endregion

        #region Tab Sounds

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource is TabControl)
            {
                Task.Run(() => SoundGenerator.PlayTabClick());
                GlobeControlsBar.Visibility = MainTabControl.SelectedIndex == 0
                    ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion

        #region Profile Tab

        private void LoadProfileToUI()
        {
            ProfileName.Text = _profile.FullName;
            ProfileIdNumber.Text = _profile.IdNumber;
            ProfileAge.Text = _profile.Age.ToString();
            ProfileEthnicity.Text = _profile.Ethnicity;
            SelectComboItem(ProfileGender, _profile.Gender);
            ProfileStreetAddress.Text = _profile.StreetAddress;
            ProfileSuburb.Text = _profile.Suburb;
            ProfileCity.Text = _profile.CurrentCity;
            ProfileCountry.Text = _profile.CurrentCountry;
            ProfilePostalCode.Text = _profile.PostalCode;
            ProfileLat.Text = _profile.HomeLatitude.ToString("F6");
            ProfileLon.Text = _profile.HomeLongitude.ToString("F6");
            ProfileImmigration.Text = _profile.ImmigrationStatus;
            SelectComboItem(ProfileVehicleType, _profile.VehicleType);
            ProfileVehicleMakeModel.Text = $"{_profile.VehicleMake} {_profile.VehicleModel}".Trim();
            ProfileSkills.Text = _profile.Skills;
            ProfileValues.Text = _profile.Values;
            ProfileHealth.Text = _profile.HealthApproach;
        }

        private async void SaveProfile_Click(object sender, RoutedEventArgs e)
        {
            _profile.FullName = ProfileName.Text;
            _profile.IdNumber = ProfileIdNumber.Text;
            int.TryParse(ProfileAge.Text, out int age); _profile.Age = age;
            _profile.Ethnicity = ProfileEthnicity.Text;
            _profile.Gender = (ProfileGender.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Male";
            _profile.StreetAddress = ProfileStreetAddress.Text;
            _profile.Suburb = ProfileSuburb.Text;
            _profile.CurrentCity = ProfileCity.Text;
            _profile.CurrentCountry = ProfileCountry.Text;
            _profile.PostalCode = ProfilePostalCode.Text;
            _profile.ImmigrationStatus = ProfileImmigration.Text;
            _profile.VehicleType = (ProfileVehicleType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Sedan";
            var makeModel = ProfileVehicleMakeModel.Text.Split(' ', 2);
            _profile.VehicleMake = makeModel.Length > 0 ? makeModel[0] : "";
            _profile.VehicleModel = makeModel.Length > 1 ? makeModel[1] : "";
            _profile.Skills = ProfileSkills.Text;
            _profile.Values = ProfileValues.Text;
            _profile.HealthApproach = ProfileHealth.Text;

            // Geocode the address using Google API
            var googleKey = SettingsGoogleKey.Text;
            if (!string.IsNullOrEmpty(googleKey))
            {
                try
                {
                    UpdateStatus("Geocoding address...");
                    var addressParts = new[]
                    {
                        _profile.StreetAddress, _profile.Suburb,
                        _profile.CurrentCity, _profile.PostalCode,
                        _profile.CurrentCountry
                    }.Where(s => !string.IsNullOrWhiteSpace(s));

                    var fullAddress = string.Join(", ", addressParts);
                    if (!string.IsNullOrEmpty(fullAddress))
                    {
                        var (lat, lon) = await GeocodeAddress(fullAddress, googleKey);
                        if (lat != 0 || lon != 0)
                        {
                            _profile.HomeLatitude = lat;
                            _profile.HomeLongitude = lon;
                            ProfileLat.Text = lat.ToString("F6");
                            ProfileLon.Text = lon.ToString("F6");
                        }
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Geocoding failed: {ex.Message}");
                }
            }
            else
            {
                // No Google key — use manually entered lat/lon
                double.TryParse(ProfileLat.Text, out double lat); _profile.HomeLatitude = lat;
                double.TryParse(ProfileLon.Text, out double lon); _profile.HomeLongitude = lon;
            }

            _db.SaveProfile(_profile);
            UpdateStatus("Profile saved.");
            MessageBox.Show("Profile saved successfully.", "Safety Sentinel", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static async Task<(double lat, double lon)> GeocodeAddress(string address, string apiKey)
        {
            var encoded = Uri.EscapeDataString(address);
            var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={encoded}&key={apiKey}";
            var response = await _geocodeHttp.GetStringAsync(url);
            var json = Newtonsoft.Json.Linq.JObject.Parse(response);

            var location = json["results"]?[0]?["geometry"]?["location"];
            if (location != null)
            {
                double lat = location["lat"]?.ToObject<double>() ?? 0;
                double lon = location["lng"]?.ToObject<double>() ?? 0;
                return (lat, lon);
            }
            return (0, 0);
        }

        #endregion

        #region Alerts Tab

        private void LoadAlertCategories()
        {
            var cats = _db.GetAlertCategories();
            AlertCategoriesList.ItemsSource = cats;
            AlertCountText.Text = $"{cats.Count(c => c.Enabled)} of {cats.Count} enabled";
        }

        private void AlertToggle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.DataContext is AlertCategory cat)
            {
                _db.SaveAlertCategory(cat);
                LoadAlertCategories();
            }
        }

        #endregion

        #region Personal Alerts

        private void LoadPersonalAlerts()
        {
            PersonalAlertsList.ItemsSource = _db.GetActivePersonalAlerts();
        }

        private void AddPersonalAlert_Click(object sender, RoutedEventArgs e)
        {
            var desc = PersonalAlertInput.Text.Trim();
            if (string.IsNullOrEmpty(desc)) return;

            _db.AddPersonalAlert(new PersonalAlert { Description = desc });
            PersonalAlertInput.Text = "";
            LoadPersonalAlerts();
            UpdateStatus("Personal alert added.");
        }

        private void RemovePersonalAlert_Click(object sender, RoutedEventArgs e)
        {
            if (PersonalAlertsList.SelectedItem is PersonalAlert alert)
            {
                _db.RemovePersonalAlert(alert.Id);
                LoadPersonalAlerts();
                UpdateStatus("Personal alert removed.");
            }
        }

        #endregion

        #region Brief Tab

        private void LoadLatestBrief()
        {
            var brief = _db.GetLatestBrief();
            if (brief != null)
            {
                BriefContent.Text = brief.Content;
                BriefDateText.Text = $"Generated: {brief.BriefDate:yyyy-MM-dd HH:mm}";
                _currentBriefId = brief.Id;
                LoadChatHistoryFromBrief(brief);
            }
            else
            {
                BriefContent.Text = "No intelligence brief generated yet.\n\n1. Fill in your Profile\n2. Add your Claude API key in Settings\n3. Click Scan Now";
                _currentBriefId = -1;
            }
        }

        /// <summary>
        /// Load persisted chat history from the brief's ChatHistory JSON field.
        /// </summary>
        private void LoadChatHistoryFromBrief(ExecutiveBrief brief)
        {
            _briefChatHistory.Clear();
            BriefChatMessages.Document.Blocks.Clear();
            if (!string.IsNullOrWhiteSpace(brief.ChatHistory))
            {
                try
                {
                    var entries = JsonConvert.DeserializeObject<List<ChatEntry>>(brief.ChatHistory);
                    if (entries != null)
                    {
                        foreach (var entry in entries)
                        {
                            _briefChatHistory.Add((entry.Role, entry.Text));
                            AddChatMessage(BriefChatMessages,
                                entry.Role == "user" ? "Asset" : "Agent",
                                entry.Text,
                                isUser: entry.Role == "user");
                        }
                    }
                }
                catch { /* ignore corrupt data */ }
            }
        }

        /// <summary>
        /// Persist the current brief chat history to the database.
        /// </summary>
        private void SaveBriefChatHistory()
        {
            if (_currentBriefId <= 0 || _briefChatHistory.Count == 0) return;
            try
            {
                var briefs = _db.GetAllBriefs();
                var brief = briefs.FirstOrDefault(b => b.Id == _currentBriefId);
                if (brief != null)
                {
                    var entries = _briefChatHistory.Select(h => new ChatEntry { Role = h.role, Text = h.text }).ToList();
                    brief.ChatHistory = JsonConvert.SerializeObject(entries);
                    _db.UpdateBrief(brief);
                }
            }
            catch { /* best effort */ }
        }

        private class ChatEntry { public string Role { get; set; } = ""; public string Text { get; set; } = ""; }

        #endregion

        #region Brief Chat

        private void BriefChatToggle_Click(object sender, RoutedEventArgs e)
        {
            if (!_briefChatOpen)
                OpenChatPanel(BriefChatTranslate, ref _briefChatOpen);
            else
                CloseChatPanel(BriefChatTranslate, ref _briefChatOpen);
        }

        private void BriefChatClose_Click(object sender, RoutedEventArgs e)
        {
            CloseChatPanel(BriefChatTranslate, ref _briefChatOpen);
            SaveBriefChatHistory();
        }

        private void BriefChatInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                BriefChatSend_Click(sender, e);
        }

        private async void BriefChatSend_Click(object sender, RoutedEventArgs e)
        {
            var msg = BriefChatInput.Text.Trim();
            if (string.IsNullOrEmpty(msg)) return;

            BriefChatInput.Text = "";
            AddChatMessage(BriefChatMessages, "Asset", msg, isUser: true);
            _briefChatHistory.Add(("user", msg));

            // Show thinking indicator immediately
            var thinkingPara = AddThinkingIndicator(BriefChatMessages);

            try
            {
                BriefChatSend.IsEnabled = false;
                var briefContent = BriefContent.Text;
                var response = await _intel.ChatAboutBrief(msg, briefContent, _briefChatHistory);
                RemoveThinkingIndicator(BriefChatMessages, thinkingPara);
                AddChatMessage(BriefChatMessages, "Agent", response, isUser: false);
                _briefChatHistory.Add(("assistant", response));
                SaveBriefChatHistory(); // auto-persist after each exchange
                UpdateApiUsageDisplay();
            }
            catch (Exception ex)
            {
                RemoveThinkingIndicator(BriefChatMessages, thinkingPara);
                AddChatMessage(BriefChatMessages, "Agent", $"Error: {ex.Message}", isUser: false);
            }
            finally
            {
                BriefChatSend.IsEnabled = true;
            }
        }

        #endregion

        #region History Tab

        private void LoadBriefHistory()
        {
            var briefs = _db.GetAllBriefs();
            BriefHistoryList.ItemsSource = briefs;
            LoadGlobeBriefSelector(briefs);
        }

        private void LoadGlobeBriefSelector(List<ExecutiveBrief>? briefs = null)
        {
            briefs ??= _db.GetAllBriefs();
            GlobeBriefSelector.SelectionChanged -= GlobeBriefSelector_Changed;
            GlobeBriefSelector.Items.Clear();
            GlobeBriefSelector.Items.Add(new ComboBoxItem { Content = "Current watchlist (live)", Tag = (ExecutiveBrief?)null });
            foreach (var b in briefs.OrderByDescending(x => x.BriefDate))
            {
                GlobeBriefSelector.Items.Add(new ComboBoxItem
                {
                    Content = $"{b.BriefDate:dd MMM yyyy HH:mm}  [{b.OverallThreatLevel}]",
                    Tag = b
                });
            }
            GlobeBriefSelector.SelectedIndex = 0;
            GlobeBriefSelector.SelectionChanged += GlobeBriefSelector_Changed;
        }

        private async void GlobeBriefSelector_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (GlobeBriefSelector.SelectedItem is not ComboBoxItem item) return;
            var brief = item.Tag as ExecutiveBrief;
            await LoadGlobeData(brief);
        }

        private void BriefHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BriefHistoryList.SelectedItem is ExecutiveBrief brief)
                HistoryBriefContent.Text = brief.Content;
        }

        #endregion

        #region History Chat

        private void HistoryChatToggle_Click(object sender, RoutedEventArgs e)
        {
            if (!_historyChatOpen)
                OpenChatPanel(HistoryChatTranslate, ref _historyChatOpen);
            else
                CloseChatPanel(HistoryChatTranslate, ref _historyChatOpen);
        }

        private void HistoryChatClose_Click(object sender, RoutedEventArgs e)
        {
            CloseChatPanel(HistoryChatTranslate, ref _historyChatOpen);
        }

        private void HistoryChatInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                HistoryChatSend_Click(sender, e);
        }

        private async void HistoryChatSend_Click(object sender, RoutedEventArgs e)
        {
            var msg = HistoryChatInput.Text.Trim();
            if (string.IsNullOrEmpty(msg)) return;

            HistoryChatInput.Text = "";
            AddChatMessage(HistoryChatMessages, "Asset", msg, isUser: true);
            _historyChatHistory.Add(("user", msg));

            // Show thinking indicator immediately
            var thinkingPara = AddThinkingIndicator(HistoryChatMessages);

            try
            {
                HistoryChatSend.IsEnabled = false;
                var allBriefs = _db.GetAllBriefs();
                var response = await _intel.ChatAboutHistory(msg, allBriefs, _historyChatHistory);
                RemoveThinkingIndicator(HistoryChatMessages, thinkingPara);
                AddChatMessage(HistoryChatMessages, "Agent", response, isUser: false);
                _historyChatHistory.Add(("assistant", response));
                UpdateApiUsageDisplay();
            }
            catch (Exception ex)
            {
                RemoveThinkingIndicator(HistoryChatMessages, thinkingPara);
                AddChatMessage(HistoryChatMessages, "Agent", $"Error: {ex.Message}", isUser: false);
            }
            finally
            {
                HistoryChatSend.IsEnabled = true;
            }
        }

        #endregion

        #region Chat Helpers

        private void OpenChatPanel(TranslateTransform transform, ref bool isOpen)
        {
            isOpen = true;
            Task.Run(() => SoundGenerator.PlaySwooshOpen());
            double panelHeight = 300;
            if (transform == BriefChatTranslate) panelHeight = BriefChatPanel.Height;
            else if (transform == HistoryChatTranslate) panelHeight = HistoryChatPanel.Height;

            var anim = new DoubleAnimation(panelHeight, 0, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            transform.BeginAnimation(TranslateTransform.YProperty, anim);
        }

        private void CloseChatPanel(TranslateTransform transform, ref bool isOpen)
        {
            isOpen = false;
            Task.Run(() => SoundGenerator.PlaySwooshClose());
            // Determine the panel that owns this transform so we slide by its actual height
            double panelHeight = 300;
            if (transform == BriefChatTranslate) panelHeight = BriefChatPanel.Height;
            else if (transform == HistoryChatTranslate) panelHeight = HistoryChatPanel.Height;

            var anim = new DoubleAnimation(0, panelHeight, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            transform.BeginAnimation(TranslateTransform.YProperty, anim);
        }

        private static readonly string[] _thinkingPhrases = new[]
        {
            "Thinking...", "Doing research...", "Getting information...",
            "Analyzing data...", "Consulting sources...", "Cross-referencing intelligence...",
            "Assessing threats...", "Compiling findings..."
        };

        private Paragraph? _briefThinkingParagraph;
        private Paragraph? _historyThinkingParagraph;

        private Paragraph AddThinkingIndicator(RichTextBox chatBox)
        {
            var para = new Paragraph { Margin = new Thickness(0, 4, 0, 0) };
            var labelRun = new Run("Agent: ")
            {
                Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0xff, 0x88)),
                FontWeight = FontWeights.Bold
            };
            var thinkingRun = new Run("Thinking...")
            {
                Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0xff, 0x88)),
                FontStyle = FontStyles.Italic
            };
            para.Inlines.Add(labelRun);
            para.Inlines.Add(thinkingRun);
            chatBox.Document.Blocks.Add(para);
            chatBox.ScrollToEnd();

            // Animate opacity pulsing
            var pulseAnim = new DoubleAnimation(1.0, 0.3, TimeSpan.FromMilliseconds(600))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase()
            };
            thinkingRun.BeginAnimation(UIElement.OpacityProperty, pulseAnim);

            // Cycle phrases
            int phraseIdx = 0;
            var phraseTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.5) };
            phraseTimer.Tick += (_, _) =>
            {
                phraseIdx = (phraseIdx + 1) % _thinkingPhrases.Length;
                thinkingRun.Text = _thinkingPhrases[phraseIdx];
            };
            phraseTimer.Start();
            para.Tag = phraseTimer;

            if (chatBox == BriefChatMessages) _briefThinkingParagraph = para;
            else _historyThinkingParagraph = para;

            return para;
        }

        private void RemoveThinkingIndicator(RichTextBox chatBox, Paragraph para)
        {
            if (para.Tag is DispatcherTimer timer)
                timer.Stop();
            chatBox.Document.Blocks.Remove(para);
        }

        private static void AddChatMessage(RichTextBox chatBox, string label, string text, bool isUser)
        {
            var labelColor = isUser
                ? Color.FromRgb(0xfb, 0xbf, 0x24)
                : Color.FromRgb(0x00, 0xff, 0x88);

            var para = new Paragraph { Margin = new Thickness(0, 4, 0, 0) };
            para.Inlines.Add(new Run($"{label}: ")
            {
                Foreground = new SolidColorBrush(labelColor),
                FontWeight = FontWeights.Bold
            });
            para.Inlines.Add(new Run(text)
            {
                Foreground = new SolidColorBrush(Color.FromRgb(0xe5, 0xe7, 0xeb))
            });
            chatBox.Document.Blocks.Add(para);
            chatBox.ScrollToEnd();
        }

        private void ChatInput_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility(sender as TextBox);
        }

        private void ChatInput_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility(sender as TextBox);
        }

        private void ChatInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholderVisibility(sender as TextBox);
        }

        private void UpdatePlaceholderVisibility(TextBox? textBox)
        {
            if (textBox == null) return;
            TextBlock? placeholder = null;
            if (textBox == BriefChatInput) placeholder = BriefChatPlaceholder;
            else if (textBox == HistoryChatInput) placeholder = HistoryChatPlaceholder;
            if (placeholder != null)
                placeholder.Visibility = string.IsNullOrEmpty(textBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        // ===== Resize thumb handlers =====
        private void BriefChatResizeThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var newHeight = BriefChatPanel.Height - e.VerticalChange;
            if (newHeight >= BriefChatPanel.MinHeight && newHeight <= BriefChatPanel.MaxHeight)
                BriefChatPanel.Height = newHeight;
        }

        private void HistoryChatResizeThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var newHeight = HistoryChatPanel.Height - e.VerticalChange;
            if (newHeight >= HistoryChatPanel.MinHeight && newHeight <= HistoryChatPanel.MaxHeight)
                HistoryChatPanel.Height = newHeight;
        }

        #endregion

        #region Watchlist Tab

        // View model for countries in the exclusion list (computed, not persisted)
        private class CountryViewItem
        {
            public string CountryName { get; set; } = "";
            public string CountryCode { get; set; } = "";
            public string City { get; set; } = "";
            public string StateProvince { get; set; } = "";
            public string Reason { get; set; } = "";
        }

        private bool _suppressContinentEvents = false;

        private void UpdateContinentCheckboxes(HashSet<string> watchedCodes)
        {
            _suppressContinentEvents = true;
            var map = new (string continent, CheckBox cb)[]
            {
                ("Africa",        CbAfrica),
                ("Asia",          CbAsia),
                ("Europe",        CbEurope),
                ("North America", CbNorthAmerica),
                ("South America", CbSouthAmerica),
                ("Oceania",       CbOceania),
            };
            foreach (var (continent, cb) in map)
            {
                if (!SeedData.ContinentCountryCodes.TryGetValue(continent, out var codes)) continue;
                int watched = codes.Count(c => watchedCodes.Contains(c));
                cb.IsChecked = watched == 0 ? false : watched == codes.Length ? true : (bool?)null;
            }
            _suppressContinentEvents = false;
        }

        private void ContinentChecked(object sender, RoutedEventArgs e)
        {
            if (_suppressContinentEvents) return;
            ProcessContinentAdd((string)((CheckBox)sender).Tag);
        }

        private void ContinentUnchecked(object sender, RoutedEventArgs e)
        {
            if (_suppressContinentEvents) return;
            ProcessContinentRemove((string)((CheckBox)sender).Tag);
        }

        private void ContinentIndeterminate(object sender, RoutedEventArgs e)
        {
            if (_suppressContinentEvents) return;
            // User clicked a fully-checked box → it went to indeterminate → skip to unchecked
            var cb = (CheckBox)sender;
            _suppressContinentEvents = true;
            cb.IsChecked = false;
            _suppressContinentEvents = false;
            ProcessContinentRemove((string)cb.Tag);
        }

        private void ProcessContinentAdd(string continent)
        {
            if (!SeedData.ContinentCountryCodes.TryGetValue(continent, out var codes)) return;
            var existingCodes = _db.GetWatchlist().Select(w => w.CountryCode)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            int added = 0;
            foreach (var code in codes)
            {
                if (existingCodes.Contains(code)) continue;
                if (!SeedData.AllWorldCountries.TryGetValue(code, out var name)) continue;
                _db.AddWatchlistItem(new WatchlistItem
                {
                    CountryCode = code,
                    CountryName = name,
                    ContinentAdded = true,
                    AlertThreshold = 60,
                    ChangeThreshold = 10
                });
                added++;
            }
            LoadWatchlist();
            LoadExitPlans();
            UpdateStatus($"Added {added} {continent} countries to watchlist.");
        }

        private void ProcessContinentRemove(string continent)
        {
            if (!SeedData.ContinentCountryCodes.TryGetValue(continent, out var codes)) return;
            var codesSet = new HashSet<string>(codes, StringComparer.OrdinalIgnoreCase);
            var allInContinent = _db.GetWatchlist().Where(w => codesSet.Contains(w.CountryCode)).ToList();
            if (!allInContinent.Any()) { LoadWatchlist(); return; }

            // Warn about countries with exit plan tasks
            var exitPlanOnes = allInContinent.Where(w => w.ExitPlan || _db.GetExitPlanByName(w.DisplayText).Any()).ToList();
            if (exitPlanOnes.Count > 0)
            {
                var names = string.Join(", ", exitPlanOnes.Take(3).Select(w => w.CountryName));
                if (exitPlanOnes.Count > 3) names += $" and {exitPlanOnes.Count - 3} more";
                var ans = MessageBox.Show(
                    $"Warning: {names} ha{(exitPlanOnes.Count == 1 ? "s" : "ve")} exit plan data (tasks or destination flag).\n\n" +
                    $"Removing from watchlist will also remove from Exit Plans.\n\nContinue removing {continent}?",
                    "Exit Plan Data Found", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (ans == MessageBoxResult.No) { LoadWatchlist(); return; }
            }

            var manualOnes = allInContinent.Where(w => !w.ContinentAdded).ToList();
            if (manualOnes.Count > 0)
            {
                var names = string.Join(", ", manualOnes.Take(3).Select(w => w.CountryName));
                if (manualOnes.Count > 3) names += $" and {manualOnes.Count - 3} more";
                var ans = MessageBox.Show(
                    $"{manualOnes.Count} {continent} countr{(manualOnes.Count == 1 ? "y was" : "ies were")} added manually ({names}).\n\n" +
                    $"[Yes] Remove ALL {continent} countries\n[No] Keep manually added, remove continent-added only\n[Cancel] Do nothing",
                    "Remove Continent", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (ans == MessageBoxResult.Cancel) { LoadWatchlist(); return; }
                if (ans == MessageBoxResult.No)
                {
                    foreach (var item in allInContinent.Where(w => w.ContinentAdded))
                        _db.RemoveWatchlistItem(item.Id);
                    LoadWatchlist(); LoadExitPlans();
                    UpdateStatus($"Removed continent-added {continent} countries. Manually added ones kept.");
                    return;
                }
            }
            foreach (var item in allInContinent)
                _db.RemoveWatchlistItem(item.Id);
            LoadWatchlist();
            LoadExitPlans();
            UpdateStatus($"Removed {allInContinent.Count} {continent} countries from watchlist.");
        }

        private void LoadWatchlist()
        {
            var watchlist = _db.GetWatchlist();
            WatchlistView.ItemsSource = watchlist;

            var watchedCodes = watchlist.Select(w => w.CountryCode)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            UpdateContinentCheckboxes(watchedCodes);

            // Exclusion list = all world countries NOT on watchlist
            var excluded = SeedData.AllWorldCountries
                .Where(kvp => !watchedCodes.Contains(kvp.Key))
                .OrderBy(kvp => kvp.Value)
                .Select(kvp => new CountryViewItem { CountryName = kvp.Value, CountryCode = kvp.Key })
                .ToList();
            ExclusionView.ItemsSource = excluded;

            UpdateCountryCount();
        }

        // --- Watchlist context menu ---

        private void WatchCtx_AddCities(object sender, RoutedEventArgs e)
        {
            if (WatchlistView.SelectedItem is not WatchlistItem item) return;
            var input = ShowInputDialog("Add Cities",
                $"Cities for {item.CountryName} (separate with commas or /):", item.City);
            if (input == null) return;
            item.City = NormalizeList(input);
            _db.UpdateWatchlistItem(item);
            LoadWatchlist();
        }

        private void WatchCtx_AddState(object sender, RoutedEventArgs e)
        {
            if (WatchlistView.SelectedItem is not WatchlistItem item) return;
            var input = ShowInputDialog("Add State/Province",
                $"State/Province for {item.CountryName}:", item.StateProvince);
            if (input == null) return;
            item.StateProvince = input.Trim();
            _db.UpdateWatchlistItem(item);
            LoadWatchlist();
        }

        private void WatchCtx_AddReason(object sender, RoutedEventArgs e)
        {
            if (WatchlistView.SelectedItem is not WatchlistItem item) return;
            var input = ShowInputDialog("Watchlist Reason",
                $"Why are you watching {item.CountryName}?", item.Reason);
            if (input == null) return;
            item.Reason = input.Trim();
            _db.UpdateWatchlistItem(item);
            LoadWatchlist();
        }

        private void WatchCtx_ToggleExitPlan(object sender, RoutedEventArgs e)
        {
            if (WatchlistView.SelectedItem is not WatchlistItem item) return;
            item.ExitPlan = !item.ExitPlan;
            _db.UpdateWatchlistItem(item);
            LoadWatchlist();
            LoadExitPlans();
            UpdateStatus($"{item.CountryName} {(item.ExitPlan ? "added to" : "removed from")} exit plan destinations.");
        }

        private void WatchCtx_MoveToExcluded(object sender, RoutedEventArgs e)
        {
            if (WatchlistView.SelectedItem is not WatchlistItem item) return;
            _db.RemoveWatchlistItem(item.Id);
            LoadWatchlist();
            LoadExitPlans();
            UpdateStatus($"{item.CountryName} moved to excluded list.");
        }

        // --- Exclusion list context menu ---

        private void ExclCtx_AddToWatchlist(object sender, RoutedEventArgs e)
        {
            if (ExclusionView.SelectedItem is not CountryViewItem item) return;
            _db.AddWatchlistItem(new WatchlistItem
            {
                CountryCode = item.CountryCode,
                CountryName = item.CountryName,
                ContinentAdded = false,
                AlertThreshold = 60,
                ChangeThreshold = 10
            });
            LoadWatchlist();
            UpdateStatus($"{item.CountryName} added to watchlist.");
        }

        // --- Double-click handlers ---

        private void WatchlistView_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (WatchlistView.SelectedItem is not WatchlistItem item) return;
            ShowWatchlistEditDialog(item);
        }

        private void ShowWatchlistEditDialog(WatchlistItem item)
        {
            var dialog = new Window
            {
                Title = $"Edit — {item.CountryName}",
                Width = 500, SizeToContent = SizeToContent.Height,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = new SolidColorBrush(Color.FromRgb(0x0a, 0x0e, 0x17)),
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false
            };
            var grid = new Grid { Margin = new Thickness(16) };
            for (int i = 0; i < 10; i++)
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            void AddRow(int row, string label, UIElement control)
            {
                var lbl = new TextBlock
                {
                    Text = label, VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x9c, 0xa3, 0xaf)),
                    FontFamily = new FontFamily("Segoe UI"), Margin = new Thickness(0, 0, 8, 8)
                };
                Grid.SetRow(lbl, row); Grid.SetColumn(lbl, 0); grid.Children.Add(lbl);
                Grid.SetRow(control, row); Grid.SetColumn(control, 1); grid.Children.Add(control);
            }

            TextBox MakeTextBox(string value) => new TextBox
            {
                Text = value,
                Background = new SolidColorBrush(Color.FromRgb(0x11, 0x18, 0x27)),
                Foreground = new SolidColorBrush(Color.FromRgb(0xe5, 0xe7, 0xeb)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x1e, 0x29, 0x3b)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(6, 4, 6, 4),
                FontFamily = new FontFamily("Segoe UI"),
                CaretBrush = new SolidColorBrush(Color.FromRgb(0x00, 0xff, 0x88)),
                Margin = new Thickness(0, 0, 0, 8)
            };

            var tbCity = MakeTextBox(item.City);
            tbCity.ToolTip = "Separate multiple cities with commas";
            var tbState = MakeTextBox(item.StateProvince);
            var tbReason = MakeTextBox(item.Reason);
            var cbExitPlan = new CheckBox
            {
                IsChecked = item.ExitPlan,
                Foreground = new SolidColorBrush(Color.FromRgb(0xe5, 0xe7, 0xeb)),
                FontFamily = new FontFamily("Segoe UI"),
                Content = "Flag as Exit Plan destination",
                Margin = new Thickness(0, 0, 0, 8),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Country name (read-only)
            var tbCountry = MakeTextBox($"{item.CountryName} ({item.CountryCode})");
            tbCountry.IsReadOnly = true;
            tbCountry.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0xff, 0x88));

            AddRow(0, "Country:", tbCountry);
            AddRow(1, "Cities:", tbCity);
            AddRow(2, "State/Province:", tbState);
            AddRow(3, "Reason:", tbReason);
            Grid.SetRow(cbExitPlan, 4); Grid.SetColumn(cbExitPlan, 1); grid.Children.Add(cbExitPlan);

            var btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 8, 0, 0)
            };
            Grid.SetRow(btnPanel, 5); Grid.SetColumnSpan(btnPanel, 2); grid.Children.Add(btnPanel);

            bool saved = false;
            var btnSave = MakeDialogButton("Save", "#1a3a2a", "#00ff88");
            var btnCancel = MakeDialogButton("Cancel", "#1a1f2e", "#9ca3af");
            btnPanel.Children.Add(btnSave);
            btnPanel.Children.Add(btnCancel);

            btnSave.Click += (_, _) =>
            {
                item.City = NormalizeList(tbCity.Text);
                item.StateProvince = tbState.Text.Trim();
                item.Reason = tbReason.Text.Trim();
                item.ExitPlan = cbExitPlan.IsChecked == true;
                saved = true;
                dialog.Close();
            };
            btnCancel.Click += (_, _) => dialog.Close();
            dialog.KeyDown += (_, ke) => { if (ke.Key == System.Windows.Input.Key.Escape) dialog.Close(); };

            dialog.Content = grid;
            dialog.ShowDialog();

            if (saved)
            {
                _db.UpdateWatchlistItem(item);
                LoadWatchlist();
                LoadExitPlans();
            }
        }

        /// <summary>Create a styled dialog button matching the app's dark theme.</summary>
        private static Button MakeDialogButton(string text, string bgHex, string fgHex)
        {
            Color ParseHex(string hex) => (Color)ColorConverter.ConvertFromString(hex);
            var btn = new Button
            {
                Content = text,
                Margin = new Thickness(0, 0, 8, 0),
                Background = new SolidColorBrush(ParseHex(bgHex)),
                Foreground = new SolidColorBrush(ParseHex(fgHex)),
                BorderBrush = new SolidColorBrush(ParseHex(fgHex)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(16, 6, 16, 6),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            // Override the default WPF button template to prevent system-color hover highlight
            var factory = new FrameworkElementFactory(typeof(Border));
            factory.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            factory.SetBinding(Border.BorderBrushProperty, new System.Windows.Data.Binding("BorderBrush") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            factory.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
            factory.SetBinding(Border.PaddingProperty, new System.Windows.Data.Binding("Padding") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            var cp = new FrameworkElementFactory(typeof(ContentPresenter));
            cp.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            cp.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            factory.AppendChild(cp);
            var template = new ControlTemplate(typeof(Button)) { VisualTree = factory };
            btn.Template = template;
            return btn;
        }

        private void ExclusionView_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ExclusionView.SelectedItem is not CountryViewItem item) return;
            _db.AddWatchlistItem(new WatchlistItem
            {
                CountryCode = item.CountryCode,
                CountryName = item.CountryName,
                ContinentAdded = false,
                AlertThreshold = 60,
                ChangeThreshold = 10
            });
            LoadWatchlist();
            UpdateStatus($"{item.CountryName} added to watchlist.");
        }

        // --- Smart Select ---

        private async void SmartSelect_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SettingsApiKey.Password))
            {
                MessageBox.Show("Please set your API key in Settings to use Smart Select.", "Safety Sentinel");
                return;
            }
            var criteria = ShowInputDialog("Smart Select",
                "Describe the countries you want to watch:\n(e.g. \"politically stable, English-speaking, strong rule of law\")",
                "", multiLine: true);
            if (string.IsNullOrEmpty(criteria)) return;

            // Ensure API key is fresh
            _intel.Configure(SettingsApiKey.Password, (SettingsModel.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "");
            SmartSelectBtn.IsEnabled = false;
            UpdateStatus("Smart Select: consulting AI...");
            try
            {
                var codes = await _intel.SmartSelectCountries(criteria);
                if (codes.Count == 0)
                {
                    MessageBox.Show("AI returned no matching countries. Try different criteria.", "Safety Sentinel");
                    return;
                }
                var existing = _db.GetWatchlist().Select(w => w.CountryCode)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                int added = 0;
                var shortCriteria = criteria.Length > 40 ? criteria[..40] + "…" : criteria;
                foreach (var code in codes)
                {
                    if (existing.Contains(code)) continue;
                    if (!SeedData.AllWorldCountries.TryGetValue(code, out var name)) continue;
                    _db.AddWatchlistItem(new WatchlistItem
                    {
                        CountryCode = code,
                        CountryName = name,
                        Reason = $"Smart Select: {shortCriteria}",
                        ContinentAdded = false,
                        AlertThreshold = 60,
                        ChangeThreshold = 10
                    });
                    added++;
                }
                LoadWatchlist();
                LoadExitPlans();
                UpdateApiUsageDisplay();
                UpdateStatus($"Smart Select: added {added} countries to watchlist.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Smart Select failed: {ex.Message}", "Safety Sentinel");
                UpdateStatus("Smart Select failed.");
            }
            finally
            {
                SmartSelectBtn.IsEnabled = true;
            }
        }

        // --- Helpers ---

        private static string NormalizeList(string input) =>
            string.Join(", ", input
                .Split(new[] { ',', '/', '\\' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => p.Length > 0));

        private static string? ShowInputDialog(string title, string prompt, string defaultValue, bool multiLine = false)
        {
            string? result = null;
            var dialog = new Window
            {
                Title = title,
                Width = 500,
                Height = multiLine ? 290 : 185,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = new SolidColorBrush(Color.FromRgb(0x0a, 0x0e, 0x17)),
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false
            };
            var grid = new Grid { Margin = new Thickness(16) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = multiLine ? new GridLength(1, GridUnitType.Star) : GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var lbl = new TextBlock
            {
                Text = prompt,
                Foreground = new SolidColorBrush(Color.FromRgb(0x9c, 0xa3, 0xaf)),
                FontFamily = new FontFamily("Segoe UI"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8)
            };
            Grid.SetRow(lbl, 0);

            var tb = new TextBox
            {
                Text = defaultValue,
                Background = new SolidColorBrush(Color.FromRgb(0x11, 0x18, 0x27)),
                Foreground = new SolidColorBrush(Color.FromRgb(0xe5, 0xe7, 0xeb)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x1e, 0x29, 0x3b)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(6, 6, 6, 6),
                FontFamily = new FontFamily("Segoe UI"),
                Margin = new Thickness(0, 0, 0, 10),
                CaretBrush = new SolidColorBrush(Color.FromRgb(0x00, 0xff, 0x88)),
                AcceptsReturn = multiLine,
                TextWrapping = multiLine ? TextWrapping.Wrap : TextWrapping.NoWrap,
                VerticalAlignment = multiLine ? VerticalAlignment.Stretch : VerticalAlignment.Top,
                VerticalScrollBarVisibility = multiLine ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled
            };
            if (!string.IsNullOrEmpty(defaultValue)) tb.SelectAll();
            Grid.SetRow(tb, 1);

            var btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            var btnOk = MakeDialogButton("OK", "#1a3a2a", "#00ff88");
            var btnCancel = MakeDialogButton("Cancel", "#1a1f2e", "#9ca3af");
            btnPanel.Children.Add(btnOk);
            btnPanel.Children.Add(btnCancel);
            Grid.SetRow(btnPanel, 2);

            btnOk.Click += (_, _) => { result = tb.Text; dialog.Close(); };
            btnCancel.Click += (_, _) => { result = null; dialog.Close(); };
            if (!multiLine)
            {
                tb.KeyDown += (_, ke) =>
                {
                    if (ke.Key == System.Windows.Input.Key.Return) { result = tb.Text; dialog.Close(); }
                };
            }
            dialog.KeyDown += (_, ke) =>
            {
                if (ke.Key == System.Windows.Input.Key.Escape) dialog.Close();
            };

            grid.Children.Add(lbl);
            grid.Children.Add(tb);
            grid.Children.Add(btnPanel);
            dialog.Content = grid;
            dialog.ShowDialog();
            return result;
        }

        #endregion

        #region Exit Plans Tab

        private void LoadExitPlans()
        {
            if (ExitPlanCountriesView == null) return;
            var exitCountries = _db.GetWatchlist().Where(w => w.ExitPlan).ToList();
            ExitPlanCountriesView.ItemsSource = exitCountries;

            if (!exitCountries.Any())
            {
                ExitPlanTitle.Text = "← Right-click a country in the Watchlist and choose 'Toggle Exit Plan Destination'";
                ExitPlanList.ItemsSource = null;
                ExitPlanProgress.Text = "";
            }
            else
            {
                // keep current selection if still valid
                LoadExitPlanForSelection();
            }
        }

        private void ExitPlanCountry_Selected(object sender, SelectionChangedEventArgs e)
        {
            LoadExitPlanForSelection();
        }

        private void LoadExitPlanForSelection()
        {
            if (ExitPlanList == null || ExitPlanCountriesView == null) return;

            if (ExitPlanCountriesView.SelectedItem is WatchlistItem item)
            {
                ExitPlanTitle.Text = item.DisplayText;
                var tasks = _db.GetExitPlanByName(item.DisplayText);
                if (tasks.Count == 0)
                {
                    ExitPlanList.ItemsSource = null;
                    ExitPlanProgress.Text = "Requirements for the selected destinations will be populated during the next scan.";
                }
                else
                {
                    ExitPlanList.ItemsSource = tasks;
                    int done = tasks.Count(t => t.Completed);
                    ExitPlanProgress.Text = $"{done}/{tasks.Count} complete ({done * 100 / tasks.Count}%)";
                }
            }
            else
            {
                ExitPlanTitle.Text = "← Select a destination";
                ExitPlanList.ItemsSource = null;
                ExitPlanProgress.Text = "";
            }
        }

        private void ExitPlanCheck_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.DataContext is ExitPlanItem planItem)
            {
                planItem.CompletedDate = planItem.Completed ? DateTime.UtcNow : null;
                _db.SaveExitPlanItem(planItem);
                LoadExitPlanForSelection();
            }
        }

        private void CopyExitPlan_Click(object sender, RoutedEventArgs e)
        {
            if (ExitPlanCountriesView.SelectedItem is not WatchlistItem item)
            {
                MessageBox.Show("Select an exit plan destination first.", "Safety Sentinel");
                return;
            }
            var tasks = _db.GetExitPlanByName(item.DisplayText);
            if (tasks.Count == 0)
            {
                MessageBox.Show("No tasks to copy yet. Run a scan to generate exit plan tasks.", "Safety Sentinel");
                return;
            }
            var text = $"EXIT PLAN: {item.DisplayText}\n" + string.Join("\n",
                tasks.Select(t => $"[{(t.Completed ? "✓" : " ")}] {t.Category}: {t.TaskTitle} — {t.TaskDescription}"));
            Clipboard.SetText(text);
            UpdateStatus("Exit plan copied to clipboard.");
        }

        #endregion

        #region Settings Tab

        /// <summary>
        /// Returns the persistent settings file path in %APPDATA%\SafetySentinel.
        /// Falls back to the build directory copy only as an initial seed.
        /// </summary>
        private static string GetSettingsPath()
        {
            var appDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SafetySentinel");

            if (!Directory.Exists(appDataDir))
                Directory.CreateDirectory(appDataDir);

            var userPath = Path.Combine(appDataDir, "appsettings.json");

            // Seed from build directory if user copy doesn't exist yet
            if (!File.Exists(userPath))
            {
                var defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                if (File.Exists(defaultPath))
                    File.Copy(defaultPath, userPath);
            }

            return userPath;
        }

        private void LoadSettings()
        {
            try
            {
                var configPath = GetSettingsPath();
                if (!File.Exists(configPath)) return;

                var json = File.ReadAllText(configPath);
                var config = Newtonsoft.Json.Linq.JObject.Parse(json);
                var sentinel = config["Sentinel"];
                if (sentinel == null) return;

                SettingsApiKey.Password = sentinel["ApiKey"]?.ToString() ?? "";
                SelectComboItem(SettingsModel, sentinel["Model"]?.ToString() ?? "claude-sonnet-4-20250514");
                SettingsGoogleKey.Text = sentinel["GoogleMapsApiKey"]?.ToString() ?? "";
                SettingsScanInterval.Text = sentinel["QuickScanIntervalHours"]?.ToString() ?? "2";
                SettingsBriefTime.Text = sentinel["DailyBriefTime"]?.ToString() ?? "08:00";
                SettingsAutoStart.IsChecked = sentinel["AutoStartScheduler"]?.ToObject<bool>() ?? false;
                SettingsApiMonthlyBudget.Text = _profile.ApiMonthlyBudget.ToString("F2");

                _intel.Configure(SettingsApiKey.Password, (SettingsModel.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "");

                if (SettingsAutoStart.IsChecked == true && !string.IsNullOrEmpty(SettingsApiKey.Password))
                {
                    int.TryParse(SettingsScanInterval.Text, out int hours);
                    if (hours < 1) hours = 2;
                    var briefTimeParts = SettingsBriefTime.Text.Split(':');
                    int.TryParse(briefTimeParts[0], out int h);
                    int.TryParse(briefTimeParts.Length > 1 ? briefTimeParts[1] : "0", out int m);
                    _scheduler.Start(TimeSpan.FromHours(hours), new TimeSpan(h, m, 0));
                }
            }
            catch (Exception ex) { UpdateStatus($"Settings load error: {ex.Message}"); }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var configPath = GetSettingsPath();
                var config = new
                {
                    Sentinel = new
                    {
                        ApiKey = SettingsApiKey.Password,
                        Model = (SettingsModel.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "claude-sonnet-4-20250514",
                        QuickScanIntervalHours = int.TryParse(SettingsScanInterval.Text, out int h) ? h : 2,
                        DailyBriefTime = SettingsBriefTime.Text,
                        GoogleMapsApiKey = SettingsGoogleKey.Text,
                        AutoStartScheduler = SettingsAutoStart.IsChecked ?? false,
                        DefaultHomeCountry = "ZA",
                        DefaultDestinationCountry = "US",
                        ThresholdYellow = 50,
                        ThresholdOrange = 30,
                        ThresholdRed = 15
                    }
                };
                File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
                _intel.Configure(SettingsApiKey.Password, (SettingsModel.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "");

                // Save credit balance to profile; record timestamp so spend is tracked from this point
                if (double.TryParse(SettingsApiMonthlyBudget.Text, out double budget))
                {
                    bool balanceChanged = Math.Abs(budget - _profile.ApiMonthlyBudget) > 0.001;
                    _profile.ApiMonthlyBudget = budget;
                    if (balanceChanged && budget > 0)
                        _profile.ApiBalanceSetAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    _db.SaveProfile(_profile);
                }
                UpdateApiUsageDisplay();

                UpdateStatus("Settings saved.");
                MessageBox.Show("Settings saved successfully.", "Safety Sentinel", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { MessageBox.Show($"Error saving settings: {ex.Message}"); }
        }

        #endregion

        #region Scanning

        private async void ScanNow_Click(object sender, RoutedEventArgs e)
        {
            await RunScan();
        }

        private async Task RunScan()
        {
            try
            {
                _intel.Configure(SettingsApiKey.Password, (SettingsModel.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "");

                // Warn user if watchlist is large (high token/credit usage)
                var watchlistForScan = _db.GetWatchlist();
                if (watchlistForScan.Count > 20)
                {
                    var answer = MessageBox.Show(
                        $"You have {watchlistForScan.Count} countries on your watchlist.\n\n" +
                        "Gathering detailed intelligence on this many countries will use significantly more " +
                        "API credits and may take longer to complete.\n\n" +
                        "Would you like to proceed?",
                        "Large Watchlist — High Credit Usage",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
                    if (answer == MessageBoxResult.No) return;
                }

                UpdateStatus("Generating intelligence brief...");

                // Show progress bar and switch to Brief tab
                ScanProgressPanel.Visibility = Visibility.Visible;
                ScanStageText.Text = "Establishing secure connection...";
                ScanProgressPercent.Text = "0%";
                ScanProgressFill.Width = 0;

                // Switch to Brief tab immediately and clear old content
                MainTabControl.SelectedIndex = 1;
                BriefContent.Text = "";
                BriefDateText.Text = "Streaming live intelligence brief...";

                // Create a progress reporter that updates the UI from the streaming API
                int progressPhase = 0; // 0=init, 1=searching, 2=writing
                int searchCount = 0;
                var scanProgress = new Progress<string>(status =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ScanStageText.Text = status;

                        // Calculate progress based on phase
                        double percent;
                        if (status.StartsWith("Sending") || status.StartsWith("Establishing"))
                        {
                            percent = 5;
                        }
                        else if (status.Contains("source") && status.Contains("...") && !status.StartsWith("Analyzing"))
                        {
                            progressPhase = 1;
                            // Extract source number from "... (source N)"
                            var srcIdx = status.IndexOf("source ");
                            if (srcIdx >= 0)
                            {
                                var numStr = new string(status.Substring(srcIdx + 7).TakeWhile(char.IsDigit).ToArray());
                                if (int.TryParse(numStr, out int num))
                                    searchCount = num;
                            }
                            percent = 10 + (searchCount * 4); // 10-50% for up to 10 searches
                        }
                        else if (status.StartsWith("Analyzing"))
                        {
                            percent = 10 + (searchCount * 4) + 2;
                        }
                        else if (status.StartsWith("Compiling threat"))
                        {
                            progressPhase = 2;
                            percent = 55;
                        }
                        else if (status.StartsWith("Building") || status.StartsWith("Drafting") 
                              || status.StartsWith("Compiling") || status.StartsWith("Assembling"))
                        {
                            // Parse the percentage from the status if available
                            var pctIdx = status.IndexOf('~');
                            if (pctIdx >= 0)
                            {
                                var pctStr = new string(status.Substring(pctIdx + 1).TakeWhile(c => char.IsDigit(c)).ToArray());
                                if (int.TryParse(pctStr, out int p))
                                    percent = p;
                                else
                                    percent = 60;
                            }
                            else
                                percent = 60;
                        }
                        else if (status.Contains("secured") || status.Contains("complete"))
                        {
                            percent = 100;
                        }
                        else
                        {
                            percent = progressPhase switch { 0 => 5, 1 => 30, 2 => 70, _ => 50 };
                        }

                        percent = Math.Min(percent, 100);
                        ScanProgressPercent.Text = $"{(int)percent}%";

                        var parentBorder = ScanProgressFill.Parent as FrameworkElement;
                        if (parentBorder != null)
                        {
                            double maxWidth = parentBorder.ActualWidth;
                            ScanProgressFill.Width = maxWidth * percent / 100.0;
                        }
                    });
                });

                // Run actual API call with streaming + progress + live text
                var personalAlerts = _db.GetActivePersonalAlerts();
                var brief = await _intel.GenerateDailyBrief(_profile, _countries, _hotspots, personalAlerts, scanProgress,
                    textDelta => Dispatcher.Invoke(() =>
                    {
                        BriefContent.Text += textDelta;
                        SoundGenerator.PlayTelexTick(textDelta);
                    }));

                // Stop typewriter sounds now that streaming is done
                SoundGenerator.StopTypewriter();

                // Complete the progress bar to 100%
                ScanStageText.Text = "Intelligence brief secured.";
                ScanProgressPercent.Text = "100%";
                await Dispatcher.InvokeAsync(() =>
                {
                    var parentBorder = ScanProgressFill.Parent as FrameworkElement;
                    if (parentBorder != null)
                        ScanProgressFill.Width = parentBorder.ActualWidth;
                }, DispatcherPriority.Render);

                await Task.Delay(1500);
                ScanProgressPanel.Visibility = Visibility.Collapsed;
                ScanProgressFill.Width = 0;

                if (brief != null)
                {
                    // Brief text already streamed live — just update metadata
                    BriefContent.Text = brief.Content; // ensure final complete text
                    BriefDateText.Text = $"Generated: {brief.BriefDate:yyyy-MM-dd HH:mm}";

                    var scores = _intel.ParseScoresFromResponse(brief.Content);
                    if (scores != null)
                    {
                        var home = _countries.FirstOrDefault(c => c.CountryCode == _profile.CurrentCountry);
                        if (home != null)
                        {
                            if (scores.ContainsKey("physical_security")) home.PhysicalSecurity = scores["physical_security"];
                            if (scores.ContainsKey("political_stability")) home.PoliticalStability = scores["political_stability"];
                            if (scores.ContainsKey("economic_freedom")) home.EconomicFreedom = scores["economic_freedom"];
                            if (scores.ContainsKey("digital_sovereignty")) home.DigitalSovereignty = scores["digital_sovereignty"];
                            if (scores.ContainsKey("health_environment")) home.HealthEnvironment = scores["health_environment"];
                            if (scores.ContainsKey("social_cohesion")) home.SocialCohesion = scores["social_cohesion"];
                            if (scores.ContainsKey("mobility_exit")) home.MobilityExit = scores["mobility_exit"];
                            if (scores.ContainsKey("infrastructure")) home.Infrastructure = scores["infrastructure"];
                            if (scores.ContainsKey("genocide_stage")) home.GenocideStage = scores["genocide_stage"];
                            home.OverallSafetyScore = _scoring.CalculateOverallScore(home);
                            _db.SaveCountry(home);
                        }
                    }

                    UpdateThreatLevelHeader();
                    LoadBriefHistory();
                    await LoadGlobeData();

                    // Generate exit plan tasks only for watchlist entries flagged as Exit Plan destinations
                    var exitPlanItems = _db.GetWatchlist().Where(w => w.ExitPlan).ToList();
                    foreach (var wItem in exitPlanItems)
                    {
                        var planName = wItem.DisplayText;
                        if (!_db.GetExitPlanByName(planName).Any())
                        {
                            await _intel.GenerateExitPlanTasks(wItem, scanProgress);
                        }
                    }
                    Dispatcher.Invoke(LoadExitPlans);

                    UpdateStatus($"Brief generated: {brief.OverallThreatLevel} | {DateTime.Now:HH:mm}");
                    UpdateApiUsageDisplay();
                }
            }
            catch (Exception ex)
            {
                ScanProgressPanel.Visibility = Visibility.Collapsed;
                ScanProgressFill.Width = 0;

                // Parse API errors into user-friendly messages
                string friendlyMessage;
                if (ex.Message.Contains("rate_limit_error") || ex.Message.Contains("TooManyRequests") || ex.Message.Contains("429"))
                {
                    friendlyMessage = "Rate limit exceeded — the API is receiving too many requests.\n\n" +
                                      "Please wait 1-2 minutes before scanning again.\n\n" +
                                      "Tip: Longer models like Opus use more tokens per minute.\n" +
                                      "Consider switching to a faster model in Settings.";
                    UpdateStatus("Rate limited — wait 1-2 minutes before retrying.");
                }
                else if (ex.Message.Contains("authentication") || ex.Message.Contains("401") || ex.Message.Contains("invalid_api_key"))
                {
                    friendlyMessage = "Invalid API key.\n\nPlease check your Anthropic API key in Settings.";
                    UpdateStatus("Authentication failed — check API key in Settings.");
                }
                else if (ex.Message.Contains("insufficient_quota") || ex.Message.Contains("billing") || ex.Message.Contains("402"))
                {
                    friendlyMessage = "Insufficient API credits.\n\nPlease check your Anthropic account billing at console.anthropic.com.";
                    UpdateStatus("Billing issue — check Anthropic account credits.");
                }
                else if (ex.Message.Contains("overloaded") || ex.Message.Contains("529"))
                {
                    friendlyMessage = "Anthropic's servers are currently overloaded.\n\nPlease try again in a few minutes.";
                    UpdateStatus("API overloaded — try again in a few minutes.");
                }
                else if (ex.Message.Contains("not_found") || ex.Message.Contains("404"))
                {
                    friendlyMessage = "The selected model was not found.\n\nPlease select a different model in Settings.";
                    UpdateStatus("Model not found — select a different model in Settings.");
                }
                else if (ex.Message.Contains("Timeout") || ex.Message.Contains("TaskCanceled"))
                {
                    friendlyMessage = "The request timed out.\n\nThe API took too long to respond. Please try again.";
                    UpdateStatus("Request timed out — try again.");
                }
                else if (ex.Message.Contains("API key not configured"))
                {
                    friendlyMessage = "No API key configured.\n\nGo to the Settings tab and enter your Anthropic API key.";
                    UpdateStatus("No API key — configure in Settings tab.");
                }
                else
                {
                    friendlyMessage = $"An unexpected error occurred:\n\n{ex.Message}";
                    UpdateStatus($"Scan error: {ex.Message}");
                }

                MessageBox.Show(friendlyMessage, "SENTINEL — Scan Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task AnimateScanProgress(CancellationToken ct)
        {
            try
            {
                for (int i = 0; i < _scanStages.Length && !ct.IsCancellationRequested; i++)
                {
                    int stageIndex = i;
                    await Dispatcher.InvokeAsync(() =>
                    {
                        ScanStageText.Text = _scanStages[stageIndex];
                        double percent = (double)(stageIndex + 1) / _scanStages.Length * 90;
                        ScanProgressPercent.Text = $"{(int)percent}%";

                        var parentBorder = ScanProgressFill.Parent as FrameworkElement;
                        if (parentBorder != null)
                        {
                            double maxWidth = parentBorder.ActualWidth;
                            ScanProgressFill.Width = maxWidth * percent / 100;
                        }
                    }, DispatcherPriority.Render);

                    int delay = Random.Shared.Next(1000, 3000);
                    await Task.Delay(delay, ct);
                }

                // Hold at 90% while waiting for API response
                while (!ct.IsCancellationRequested)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        ScanStageText.Text = "Awaiting intelligence analysis...";
                    });
                    await Task.Delay(2000, ct);
                }
            }
            catch (TaskCanceledException) { /* Expected when scan completes */ }
        }

        #endregion

        #region Export / Import

        private void ExportJson_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog { Filter = "JSON files|*.json", FileName = $"SafetySentinel_backup_{DateTime.Now:yyyyMMdd}.json" };
            if (dlg.ShowDialog() == true)
            {
                File.WriteAllText(dlg.FileName, _db.ExportAllDataToJson());
                UpdateStatus($"Exported to {dlg.FileName}");
                MessageBox.Show("Data exported successfully.", "Safety Sentinel");
            }
        }

        private void ExportMobile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog { Filter = "Sentinel files|*.sentinel", FileName = $"SafetySentinel_backup_{DateTime.Now:yyyyMMdd}.sentinel" };
            if (dlg.ShowDialog() == true)
            {
                File.WriteAllText(dlg.FileName, _db.ExportForMobile());
                UpdateStatus($"Mobile export saved to {dlg.FileName}");
                MessageBox.Show("Mobile data exported. Copy .sentinel file to USB or phone.", "Safety Sentinel");
            }
        }

        private void ImportSentinel_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "Sentinel files|*.sentinel|JSON files|*.json|All files|*.*" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    string json = File.ReadAllText(dlg.FileName);
                    var result = _db.ImportFromSentinel(json);
                    _countries = _db.GetAllCountries();
                    _hotspots = _db.GetActiveHotspots();
                    _profile = _db.GetProfile();
                    LoadProfileToUI();
                    LoadWatchlist();
                    LoadExitPlans();
                    _ = LoadGlobeData();
                    UpdateStatus($"Imported: {result.countries} countries, {result.hotspots} hotspots, {result.watchlist} watchlist, {result.avoidance} zones, {result.exitPlans} tasks");
                    MessageBox.Show($"Import complete!\n\nCountries: {result.countries}\nHotspots: {result.hotspots}\nWatchlist: {result.watchlist}\nAvoidance zones: {result.avoidance}\nExit plan tasks: {result.exitPlans}", "Safety Sentinel");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Import failed: {ex.Message}", "Safety Sentinel", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Helpers

        private void UpdateStatus(string msg) { StatusText.Text = msg; }

        private void UpdateCountryCount()
        {
            var watchCount = _db.GetWatchlist().Count;
            var totalWorld = Data.SeedData.AllWorldCountries.Count;
            var notWatched = totalWorld - watchCount;
            CountryCount.Text = watchCount > 0
                ? $"{watchCount} watching · {notWatched} not watched"
                : $"{totalWorld} world countries";
        }

        private void UpdateApiUsageDisplay()
        {
            var balance = _profile.ApiMonthlyBudget;
            if (balance > 0)
            {
                // Track spend since the user last set their balance
                var balanceSetDate = _profile.ApiBalanceSetAt > 0
                    ? DateTimeOffset.FromUnixTimeMilliseconds(_profile.ApiBalanceSetAt).UtcDateTime
                    : DateTime.MinValue;
                var spentSince = _db.GetSpendSince(balanceSetDate);
                var remaining = (decimal)balance - spentSince;
                ApiCreditsLabel.Text = "CREDITS REMAINING";
                ApiCreditsAmount.Text = remaining >= 0
                    ? $"${remaining:F2} of ${balance:F2}"
                    : $"${Math.Abs(remaining):F2} over balance";
                ApiCreditsAmount.Foreground = new SolidColorBrush(remaining < 0
                    ? Color.FromRgb(0xef, 0x44, 0x44)
                    : remaining < (decimal)(balance * 0.2)
                        ? Color.FromRgb(0xfb, 0xbf, 0x24)
                        : Color.FromRgb(0x00, 0xff, 0x88));
            }
            else
            {
                var monthlySpend = _db.GetMonthlySpend();
                ApiCreditsLabel.Text = "API SPEND THIS MONTH";
                ApiCreditsAmount.Text = $"${monthlySpend:F4}";
                ApiCreditsAmount.Foreground = new SolidColorBrush(Color.FromRgb(0x9c, 0xa3, 0xaf));
            }
        }

        private void TopupCredits_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://console.anthropic.com/settings/billing",
                UseShellExecute = true
            });
        }

        private void UpdateThreatLevelHeader()
        {
            var home = _countries.FirstOrDefault(c => c.CountryCode == _profile.CurrentCountry);
            if (home != null)
            {
                string level = ThreatScoringEngine.GetThreatLevel(home.OverallSafetyScore);
                ThreatLevelText.Text = level;
                ThreatLevelBadge.Background = level switch
                {
                    "GREEN" => FindResource("AccentGreenBrush") as SolidColorBrush,
                    "YELLOW" => FindResource("AccentYellowBrush") as SolidColorBrush,
                    "ORANGE" => FindResource("AccentOrangeBrush") as SolidColorBrush,
                    "RED" => FindResource("AccentBrush") as SolidColorBrush,
                    _ => FindResource("AccentGreenBrush") as SolidColorBrush
                };

                int hijackRisk = _scoring.CalculateHijackingRisk(
                    _profile.HomeLatitude, _profile.HomeLongitude,
                    _profile.VehicleType, _hotspots);
                string hijackLevel = hijackRisk switch { < 25 => "LOW", < 50 => "MODERATE", < 75 => "HIGH", _ => "CRITICAL" };
                HijackRiskText.Text = $"{hijackLevel} ({hijackRisk})";
                HijackRiskBadge.Background = hijackLevel switch
                {
                    "LOW" => FindResource("AccentGreenBrush") as SolidColorBrush,
                    "MODERATE" => FindResource("AccentYellowBrush") as SolidColorBrush,
                    "HIGH" => FindResource("AccentOrangeBrush") as SolidColorBrush,
                    _ => FindResource("AccentBrush") as SolidColorBrush
                };
            }
        }

        private static void SelectComboItem(ComboBox combo, string value)
        {
            foreach (ComboBoxItem item in combo.Items)
            {
                if (item.Content?.ToString() == value)
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
            if (combo.Items.Count > 0) combo.SelectedIndex = 0;
        }

        protected override void OnClosed(EventArgs e)
        {
            _scheduler.Dispose();
            _intel.Dispose();
            _db.Dispose();
            base.OnClosed(e);
        }

        #endregion
    }
}
