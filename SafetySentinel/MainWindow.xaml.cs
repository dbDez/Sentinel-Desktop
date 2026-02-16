using SafetySentinel.Data;
using SafetySentinel.Models;
using SafetySentinel.Services;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
                LoadLatestBrief();
                LoadWatchlist();
                LoadExitPlans();
                LoadBriefHistory();
                LoadSettings();

                // Update header
                UpdateThreatLevelHeader();
                CountryCount.Text = $"{_countries.Count} countries monitored";
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

        private async Task InitializeGlobe()
        {
            try
            {
                var env = await CoreWebView2Environment.CreateAsync();
                await GlobeWebView.EnsureCoreWebView2Async(env);

                string globePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WebContent", "globe.html");
                if (File.Exists(globePath))
                {
                    // Listen for GLOBE_READY message from JavaScript
                    var globeReadyTcs = new TaskCompletionSource<bool>();
                    GlobeWebView.CoreWebView2.WebMessageReceived += (_, args) =>
                    {
                        if (args.TryGetWebMessageAsString() == "GLOBE_READY")
                        {
                            globeReadyTcs.TrySetResult(true);
                        }
                    };

                    GlobeWebView.CoreWebView2.Navigate(new Uri(globePath).AbsoluteUri);

                    // Wait for the globe to signal readiness (base texture loaded), with a fallback timeout
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

        private async Task LoadGlobeData()
        {
            try
            {
                // Send country data to globe
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
                    genocide = c.GenocideStage, cashFreedom = c.CashFreedomScore
                });
                string countryJson = JsonConvert.SerializeObject(countryData);
                await GlobeWebView.ExecuteScriptAsync($"loadCountryData({countryJson})");

                // Send hotspot data
                var hotspotData = _hotspots.Select(h => new
                {
                    lat = h.Latitude, lng = h.Longitude,
                    name = h.LocationName, type = h.CrimeType,
                    severity = h.Severity, radius = h.RadiusMeters,
                    incidents = h.IncidentCount90d
                });
                string hotspotJson = JsonConvert.SerializeObject(hotspotData);
                await GlobeWebView.ExecuteScriptAsync($"loadHotspotData({hotspotJson})");

                // Set globe to home position from profile
                if (_profile.HomeLatitude != 0 || _profile.HomeLongitude != 0)
                {
                    await GlobeWebView.ExecuteScriptAsync(
                        $"setHomePosition({_profile.HomeLatitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {_profile.HomeLongitude.ToString(System.Globalization.CultureInfo.InvariantCulture)})");
                }
                else if (!string.IsNullOrEmpty(_profile.CurrentCountry))
                {
                    // Fall back to country coordinates when home lat/lon not set
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

          #region Profile Tab

          private void LoadProfileToUI()
          {
              ProfileName.Text = _profile.FullName;
              ProfileIdNumber.Text = _profile.IdNumber;
              ProfileAge.Text = _profile.Age.ToString();
              ProfileEthnicity.Text = _profile.Ethnicity;
              SelectComboItem(ProfileGender, _profile.Gender);
              ProfileCountry.Text = _profile.CurrentCountry;
              ProfileCity.Text = _profile.CurrentCity;
              ProfileLat.Text = _profile.HomeLatitude.ToString();
              ProfileLon.Text = _profile.HomeLongitude.ToString();
              ProfileDestCountry.Text = _profile.DestinationCountry;
              ProfileDestCity.Text = _profile.DestinationCity;
              ProfileImmigration.Text = _profile.ImmigrationStatus;
              SelectComboItem(ProfileVehicleType, _profile.VehicleType);
              ProfileVehicleMakeModel.Text = $"{_profile.VehicleMake} {_profile.VehicleModel}".Trim();
              ProfileSkills.Text = _profile.Skills;
              ProfileValues.Text = _profile.Values;
              ProfileHealth.Text = _profile.HealthApproach;
          }

          private void SaveProfile_Click(object sender, RoutedEventArgs e)
          {
              _profile.FullName = ProfileName.Text;
              _profile.IdNumber = ProfileIdNumber.Text;
              int.TryParse(ProfileAge.Text, out int age); _profile.Age = age;
              _profile.Ethnicity = ProfileEthnicity.Text;
              _profile.Gender = (ProfileGender.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Male";
              _profile.CurrentCountry = ProfileCountry.Text;
              _profile.CurrentCity = ProfileCity.Text;
              double.TryParse(ProfileLat.Text, out double lat); _profile.HomeLatitude = lat;
              double.TryParse(ProfileLon.Text, out double lon); _profile.HomeLongitude = lon;
              _profile.DestinationCountry = ProfileDestCountry.Text;
              _profile.DestinationCity = ProfileDestCity.Text;
              _profile.ImmigrationStatus = ProfileImmigration.Text;
              _profile.VehicleType = (ProfileVehicleType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Sedan";
              var makeModel = ProfileVehicleMakeModel.Text.Split(' ', 2);
              _profile.VehicleMake = makeModel.Length > 0 ? makeModel[0] : "";
              _profile.VehicleModel = makeModel.Length > 1 ? makeModel[1] : "";
              _profile.Skills = ProfileSkills.Text;
              _profile.Values = ProfileValues.Text;
              _profile.HealthApproach = ProfileHealth.Text;

              _db.SaveProfile(_profile);
              UpdateStatus("Profile saved.");
              MessageBox.Show("Profile saved successfully.", "Safety Sentinel", MessageBoxButton.OK, MessageBoxImage.Information);
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

          #region Brief Tab

          private void LoadLatestBrief()
          {
              var brief = _db.GetLatestBrief();
              if (brief != null)
              {
                  BriefContent.Text = brief.Content;
                  BriefDateText.Text = $"Generated: {brief.BriefDate:yyyy-MM-dd HH:mm}";
              }
              else
              {
                  BriefContent.Text = "No intelligence brief generated yet.\n\n1. Fill in your Profile\n2. Add your Claude API key in Settings\n3. Click 🔍 Scan Now";
              }
          }

          #endregion

          #region Watchlist Tab

          private void LoadWatchlist()
          {
              WatchlistView.ItemsSource = _db.GetWatchlist();
              AvoidanceView.ItemsSource = _db.GetAvoidanceItems();
          }

          private void AddWatchlist_Click(object sender, RoutedEventArgs e)
          {
              var code = WatchCountryCode.Text.Trim().ToUpper();
              var reason = WatchReason.Text.Trim();
              if (string.IsNullOrEmpty(code)) { MessageBox.Show("Enter a country code."); return; }

              var country = _countries.FirstOrDefault(c => c.CountryCode == code);
              _db.AddWatchlistItem(new WatchlistItem
              {
                  CountryCode = code,
                  CountryName = country?.CountryName ?? code,
                  Reason = reason,
                  AlertThreshold = 60,
                  ChangeThreshold = 10
              });
              WatchCountryCode.Text = "";
              WatchReason.Text = "";
              LoadWatchlist();
              UpdateStatus($"Added {code} to watchlist.");
          }

          private void RemoveWatchlist_Click(object sender, RoutedEventArgs e)
          {
              if (WatchlistView.SelectedItem is WatchlistItem item)
              {
                  _db.RemoveWatchlistItem(item.Id);
                  LoadWatchlist();
              }
          }

          #endregion

          #region Exit Plans Tab

          private void LoadExitPlans()
          {
              ExitPlanSelector.SelectedIndex = 0;
              LoadExitPlanForSelection();
          }

          private void ExitPlanSelector_Changed(object sender, SelectionChangedEventArgs e)
          {
              LoadExitPlanForSelection();
          }

          private void LoadExitPlanForSelection()
          {
              if (ExitPlanList == null || ExitPlanSelector == null) return;
              if (ExitPlanSelector.SelectedItem is ComboBoxItem item)
              {
                  string planName = item.Content.ToString()!;
                  var items = _db.GetExitPlanByName(planName);
                  ExitPlanList.ItemsSource = items;
                  int done = items.Count(i => i.Completed);
                  int total = items.Count;
                  ExitPlanProgress.Text = total > 0 ? $"{done}/{total} complete ({done * 100 / total}%)" : "No tasks";
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
              if (ExitPlanSelector.SelectedItem is ComboBoxItem item)
              {
                  string planName = item.Content.ToString()!;
                  var items = _db.GetExitPlanByName(planName);
                  var text = $"EXIT PLAN: {planName}\n" + string.Join("\n",
                      items.Select(i => $"[{(i.Completed ? "✓" : " ")}] {i.Category}: {i.TaskTitle} — {i.TaskDescription}"));
                  Clipboard.SetText(text);
                  UpdateStatus("Exit plan copied to clipboard.");
              }
          }

          #endregion

          #region History Tab

          private void LoadBriefHistory()
          {
              BriefHistoryList.ItemsSource = _db.GetAllBriefs();
          }

          private void BriefHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
          {
              if (BriefHistoryList.SelectedItem is ExecutiveBrief brief)
                  HistoryBriefContent.Text = brief.Content;
          }

          #endregion

          #region Settings Tab

          private void LoadSettings()
          {
              try
              {
                  var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
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

                  // Configure intelligence service
                  _intel.Configure(SettingsApiKey.Password, (SettingsModel.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "");

                  // Auto-start scheduler if enabled
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
                  var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
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
                  // Re-configure from current UI fields in case settings were changed
                  _intel.Configure(SettingsApiKey.Password, (SettingsModel.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "");

                  UpdateStatus("🔍 Generating intelligence brief...");
                  var brief = await _intel.GenerateDailyBrief(_profile, _countries, _hotspots);
                  if (brief != null)
                  {
                      BriefContent.Text = brief.Content;
                      BriefDateText.Text = $"Generated: {brief.BriefDate:yyyy-MM-dd HH:mm}";

                      // Try to parse and update scores
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
                        UpdateStatus($"Brief generated: {brief.OverallThreatLevel} | {DateTime.Now:HH:mm}");
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Scan error: {ex.Message}");
                    MessageBox.Show($"Scan failed: {ex.Message}", "Safety Sentinel", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
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

            private void UpdateThreatLevelHeader()
            {
                var home = _countries.FirstOrDefault(c => c.CountryCode == _profile.CurrentCountry);
                if (home != null)
                {
                    string level = ThreatScoringEngine.GetThreatLevel(home.OverallSafetyScore);
                    ThreatLevelText.Text = level;
                    ThreatLevelBadge.Background = level switch
                    {
                        "GREEN" => FindResource("AccentGreenBrush") as System.Windows.Media.SolidColorBrush,
                        "YELLOW" => FindResource("AccentYellowBrush") as System.Windows.Media.SolidColorBrush,
                        "ORANGE" => FindResource("AccentOrangeBrush") as System.Windows.Media.SolidColorBrush,
                        "RED" => FindResource("AccentBrush") as System.Windows.Media.SolidColorBrush,
                        _ => FindResource("AccentGreenBrush") as System.Windows.Media.SolidColorBrush
                    };

                    // Hijacking risk
                    int hijackRisk = _scoring.CalculateHijackingRisk(
                        _profile.HomeLatitude, _profile.HomeLongitude,
                        _profile.VehicleType, _hotspots);
                    string hijackLevel = hijackRisk switch { < 25 => "LOW", < 50 => "MODERATE", < 75 => "HIGH", _ => "CRITICAL" };
                    HijackRiskText.Text = $"{hijackLevel} ({hijackRisk})";
                    HijackRiskBadge.Background = hijackLevel switch
                    {
                        "LOW" => FindResource("AccentGreenBrush") as System.Windows.Media.SolidColorBrush,
                        "MODERATE" => FindResource("AccentYellowBrush") as System.Windows.Media.SolidColorBrush,
                        "HIGH" => FindResource("AccentOrangeBrush") as System.Windows.Media.SolidColorBrush,
                        _ => FindResource("AccentBrush") as System.Windows.Media.SolidColorBrush
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
