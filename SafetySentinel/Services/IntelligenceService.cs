using SafetySentinel.Data;
using SafetySentinel.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SafetySentinel.Services
{
    public class IntelligenceService : IDisposable
    {
        private readonly DatabaseManager _db;
        private readonly HttpClient _http = new();
        private string _apiKey = "";
        private string _model = "claude-sonnet-4-20250514";

        public IntelligenceService(DatabaseManager db)
        {
            _db = db;
        }

        public void Configure(string apiKey, string model)
        {
            _apiKey = apiKey;
            if (!string.IsNullOrWhiteSpace(model))
                _model = model;
        }

        public async Task<ExecutiveBrief?> GenerateDailyBrief(
            UserProfile profile, List<CountryProfile> countries, List<CrimeHotspot> hotspots)
        {
            if (string.IsNullOrEmpty(_apiKey))
                throw new InvalidOperationException("API key not configured. Set it in Settings.");

            var homeCountry = countries.FirstOrDefault(c => c.CountryCode == profile.CurrentCountry);
            var destCountry = countries.FirstOrDefault(c => c.CountryCode == profile.DestinationCountry);
            var watchlist = _db.GetWatchlist();

            string systemPrompt = BuildSystemPrompt(profile);
            string userPrompt = BuildBriefRequest(profile, homeCountry, destCountry, hotspots, watchlist);

            var requestBody = new
            {
                model = _model,
                max_tokens = 4096,
                system = systemPrompt,
                messages = new[]
                {
                    new { role = "user", content = userPrompt }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages")
            {
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("x-api-key", _apiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");

            var response = await _http.SendAsync(request);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API error {response.StatusCode}: {responseJson}");

            var result = JObject.Parse(responseJson);
            var content = result["content"]?[0]?["text"]?.ToString() ?? "No response content.";

            // Determine threat level from content
            string threatLevel = "YELLOW";
            if (content.Contains("RED", StringComparison.OrdinalIgnoreCase)) threatLevel = "RED";
            else if (content.Contains("ORANGE", StringComparison.OrdinalIgnoreCase)) threatLevel = "ORANGE";
            else if (content.Contains("GREEN", StringComparison.OrdinalIgnoreCase)) threatLevel = "GREEN";

            var brief = new ExecutiveBrief
            {
                BriefDate = DateTime.Now,
                BriefType = "daily",
                OverallThreatLevel = threatLevel,
                Content = content,
                ActionItems = ""
            };

            _db.SaveBrief(brief);
            return brief;
        }

        /// <summary>
        /// Parse threat scores from Claude's response text.
        /// Looks for patterns like "physical_security: 72" or "Physical Security: 72/100"
        /// All scores are THREAT-based (higher = more dangerous).
        /// </summary>
        public Dictionary<string, int>? ParseScoresFromResponse(string content)
        {
            var scores = new Dictionary<string, int>();
            var domains = new[]
            {
                "physical_security", "political_stability", "economic_freedom",
                "digital_sovereignty", "health_environment", "social_cohesion",
                "mobility_exit", "infrastructure", "genocide_stage"
            };

            foreach (var domain in domains)
            {
                // Try pattern: domain_name: NN or domain_name: NN/100
                var searchTerms = new[]
                {
                    domain,
                    domain.Replace("_", " "),
                    domain.Replace("_", " ").ToUpper()
                };

                foreach (var term in searchTerms)
                {
                    int idx = content.IndexOf(term, StringComparison.OrdinalIgnoreCase);
                    if (idx >= 0)
                    {
                        // Find the number after the colon
                        int colonIdx = content.IndexOf(':', idx + term.Length);
                        if (colonIdx >= 0 && colonIdx < idx + term.Length + 5)
                        {
                            string afterColon = content.Substring(colonIdx + 1, Math.Min(10, content.Length - colonIdx - 1)).Trim();
                            string numStr = new string(afterColon.TakeWhile(c => char.IsDigit(c)).ToArray());
                            if (int.TryParse(numStr, out int score))
                            {
                                scores[domain] = Math.Clamp(score, 0, 100);
                                break;
                            }
                        }
                    }
                }
            }

            return scores.Count > 0 ? scores : null;
        }

        private static string BuildSystemPrompt(UserProfile profile)
        {
            return $@"You are SENTINEL — Safety & Environmental Navigation Through Intelligence & Neurological Early-warning Logic.

You are a personal threat intelligence analyst for a high-value resource (HVR).
Your sole mission is protecting this individual's safety, freedom, financial security, and quality of life.

SUBJECT PROFILE:
- Name: {profile.FullName}
- Age: {profile.Age}, Gender: {profile.Gender}, Ethnicity: {profile.Ethnicity}
- Current Location: {profile.CurrentCity}, {profile.CurrentCountry}
- Home Coordinates: {profile.HomeLatitude}, {profile.HomeLongitude}
- Destination: {profile.DestinationCity}, {profile.DestinationCountry}
- Immigration Status: {profile.ImmigrationStatus}
- Vehicle: {profile.VehicleMake} {profile.VehicleModel} ({profile.VehicleType})
- Core Values: {profile.Values}
- Health Approach: {profile.HealthApproach}
- Skills: {profile.Skills}

SCORING MODEL:
All scores are THREAT-BASED on a 0-100 scale where higher = more dangerous.
- 0-25: GREEN (Low threat)
- 26-50: YELLOW (Moderate threat)
- 51-75: ORANGE (High threat)
- 76-100: RED (Critical threat)

BRIEF FORMAT:
Start the brief with a header block:

═══════════════════════════════════════════════════
  SENTINEL DAILY BRIEF — [Date]
  OVERALL THREAT LEVEL: [GREEN/YELLOW/ORANGE/RED]
═══════════════════════════════════════════════════

For the THREAT DASHBOARD section, render each domain using DOS-style heatmap bars.
Use █ for filled blocks and ░ for empty blocks. Each bar has exactly 20 blocks total.
The number of filled █ blocks = score / 5 (rounded).
After the bar, show a risk label, the threat level percentage, and a change indicator showing the direction and magnitude of change from the previous assessment.

Use ↑ (up arrow) for increases and ↓ (down arrow) for decreases, followed by the number of points changed.
If no change, use — (em dash) to indicate stable.

The column header should say ""Threat Level"" (NOT ""Score"").

Risk labels based on score:
  0-10:  Minimal Risk
  11-25: Fairly Safe
  26-40: Moderate Risk
  41-55: Elevated Risk
  56-70: High Risk
  71-85: Severe Risk
  86-100: Extreme Risk

Example format:
  Physical Security:   [████████████████░░░░] Severe Risk   Threat Level: 78%  ↑+3
  Political Stability: [██████░░░░░░░░░░░░░░] Moderate Risk Threat Level: 30%  ↓-2
  Economic Freedom:    [████████░░░░░░░░░░░░] Elevated Risk Threat Level: 42%  —

Use this exact format for all 8 threat domains plus a HIJACKING RISK line.

ANALYSIS PRINCIPLES:
- Distinguish between noise and signal — don't alarm unnecessarily
- Use the 10 Stages of Genocide framework for demographic threat assessment
- Weight sources by credibility (academic > government > media > social)
- Track rate of change, not just absolute levels
- Consider second-order effects
- Always maintain at least 3 actionable exit scenarios
- Flag any metric that crosses a predefined threshold

At the END of your brief, provide updated THREAT scores in this exact format:
THREAT_SCORES:
physical_security: [0-100]
political_stability: [0-100]
economic_freedom: [0-100]
digital_sovereignty: [0-100]
health_environment: [0-100]
social_cohesion: [0-100]
mobility_exit: [0-100]
infrastructure: [0-100]
genocide_stage: [0-10]";
        }

        private static string BuildBriefRequest(
            UserProfile profile, CountryProfile? home, CountryProfile? dest,
            List<CrimeHotspot> hotspots, List<WatchlistItem> watchlist)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Generate a SENTINEL DAILY BRIEF for {DateTime.Now:yyyy-MM-dd}.");
            sb.AppendLine();

            if (home != null)
            {
                sb.AppendLine($"HOME COUNTRY ({home.CountryName} - {home.CountryCode}):");
                sb.AppendLine($"  Current Threat Scores (0-100, higher=worse):");
                sb.AppendLine($"  Physical Security: {home.PhysicalSecurity}");
                sb.AppendLine($"  Political Stability: {home.PoliticalStability}");
                sb.AppendLine($"  Economic Freedom: {home.EconomicFreedom}");
                sb.AppendLine($"  Digital Sovereignty: {home.DigitalSovereignty}");
                sb.AppendLine($"  Health Environment: {home.HealthEnvironment}");
                sb.AppendLine($"  Social Cohesion: {home.SocialCohesion}");
                sb.AppendLine($"  Mobility/Exit: {home.MobilityExit}");
                sb.AppendLine($"  Infrastructure: {home.Infrastructure}");
                sb.AppendLine($"  Genocide Stage: {home.GenocideStage}/10");
                sb.AppendLine($"  CBDC Status: {home.CbdcStatus}");
                sb.AppendLine($"  Surveillance Score: {home.SurveillanceScore}");
                sb.AppendLine();
            }

            if (dest != null)
            {
                sb.AppendLine($"DESTINATION COUNTRY ({dest.CountryName} - {dest.CountryCode}):");
                sb.AppendLine($"  Overall Threat: {dest.OverallSafetyScore}");
                sb.AppendLine($"  Genocide Stage: {dest.GenocideStage}/10");
                sb.AppendLine($"  CBDC Status: {dest.CbdcStatus}");
                sb.AppendLine();
            }

            if (hotspots.Count > 0)
            {
                sb.AppendLine($"CRIME HOTSPOTS NEAR HVR ({hotspots.Count} active):");
                foreach (var h in hotspots.Take(10))
                {
                    sb.AppendLine($"  - {h.LocationName}: {h.CrimeType}, Severity {h.Severity}, {h.IncidentCount90d} incidents/90d");
                }
                sb.AppendLine();
            }

            if (watchlist.Count > 0)
            {
                sb.AppendLine("WATCHLIST COUNTRIES:");
                foreach (var w in watchlist)
                {
                    sb.AppendLine($"  - {w.CountryName} ({w.CountryCode}): {w.Reason}");
                }
                sb.AppendLine();
            }

            sb.AppendLine(@"Please provide the brief in this structure:

═══════════════════════════════════════════════════
  SENTINEL DAILY BRIEF — [Today's Date]
  OVERALL THREAT LEVEL: [level]
═══════════════════════════════════════════════════

1. CRITICAL ALERTS (immediate action items, if any)

2. THREAT DASHBOARD
   Use the DOS-style heatmap bars (20 blocks each, █ filled / ░ empty).
   After each bar show: risk label, Threat Level percentage, and a change arrow (↑+N / ↓-N / —):
   Physical Security:   [████████████████░░░░] Severe Risk   Threat Level: 78%  ↑+3
   Political Stability: [██████░░░░░░░░░░░░░░] Moderate Risk Threat Level: 30%  ↓-2
   ... etc for all 8 domains plus Hijacking Risk

3. SA SITUATIONAL AWARENESS
4. DESTINATION MONITORING
5. FINANCIAL SOVEREIGNTY WATCH
6. TECHNOLOGY & PRIVACY
7. PATTERN ANALYSIS
8. ACTIONABLE RECOMMENDATIONS

End with updated THREAT_SCORES block.");

            return sb.ToString();
        }

        public void Dispose()
        {
            _http?.Dispose();
        }
    }
}
