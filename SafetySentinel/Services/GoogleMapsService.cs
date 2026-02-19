using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace SafetySentinel.Services
{
    /// <summary>
    /// Uses Google Places and Directions APIs to resolve actual road names and
    /// travel times for exit routes from the HVR's home location.
    /// Fully dynamic — works for any country in the world.
    /// Only activated when a Google Maps API key is present in Settings.
    /// </summary>
    public class GoogleMapsService
    {
        private readonly HttpClient _http = new();
        private readonly string _apiKey;

        public GoogleMapsService(string apiKey)
        {
            _apiKey = apiKey;
        }

        /// <summary>
        /// Returns formatted route data for injection into the intelligence brief prompt.
        /// Silently returns null if Google API is unavailable or returns no results.
        /// </summary>
        public async Task<string?> GetExitRouteSummary(double lat, double lon, string countryCode, string homeCity)
        {
            if (string.IsNullOrEmpty(_apiKey) || (lat == 0 && lon == 0)) return null;

            var sb = new StringBuilder();
            sb.AppendLine("GOOGLE MAPS EXIT ROUTE DATA (verified road names — use these verbatim in the Evacuation Routes section):");
            bool anyRoute = false;

            // ---- International airports (Places Nearby Search) ----
            var airports = await FindNearestAirports(lat, lon);
            foreach (var airport in airports.Take(2))
            {
                var route = await GetDirections(lat, lon, airport.Lat, airport.Lon, airport.Name);
                if (route != null) { sb.AppendLine(route); anyRoute = true; }
            }

            // ---- Land border crossings (dynamic Places search) ----
            var borders = await FindBorderCrossings(lat, lon);
            foreach (var border in borders.Take(3))
            {
                var route = await GetDirections(lat, lon, border.Lat, border.Lon, border.Name);
                if (route != null) { sb.AppendLine(route); anyRoute = true; }
            }

            return anyRoute ? sb.ToString().TrimEnd() : null;
        }

        // ---- Private helpers ----

        private async Task<List<(string Name, double Lat, double Lon)>> FindNearestAirports(double lat, double lon)
        {
            try
            {
                // Nearby Search scoped to airport type, 350km radius
                var url = "https://maps.googleapis.com/maps/api/place/nearbysearch/json" +
                          $"?location={lat},{lon}&radius=350000&type=airport&key={_apiKey}";
                var json = JObject.Parse(await _http.GetStringAsync(url));

                return (json["results"] as JArray ?? new JArray())
                    .Where(r =>
                    {
                        var name = r["name"]?.ToString() ?? "";
                        // Only include airports with "International" in the name
                        return name.Contains("International", StringComparison.OrdinalIgnoreCase)
                            || name.Contains("Internasionaal", StringComparison.OrdinalIgnoreCase);
                    })
                    .Select(r => (
                        Name: r["name"]!.ToString(),
                        Lat:  r["geometry"]!["location"]!["lat"]!.Value<double>(),
                        Lon:  r["geometry"]!["location"]!["lng"]!.Value<double>()
                    ))
                    .Where(a => a.Lat != 0)
                    .OrderBy(a => ApproxDistanceKm(lat, lon, a.Lat, a.Lon))
                    .ToList();
            }
            catch { return new(); }
        }

        private async Task<List<(string Name, double Lat, double Lon)>> FindBorderCrossings(double lat, double lon)
        {
            var results = new List<(string Name, double Lat, double Lon)>();

            // Run two complementary searches and merge, deduplicating by proximity
            var queries = new[]
            {
                "international border crossing",
                "port of entry immigration customs"
            };

            foreach (var query in queries)
            {
                try
                {
                    // Text Search within 800km — large radius to cover users in big countries like USA/Canada/Australia
                    var url = "https://maps.googleapis.com/maps/api/place/textsearch/json" +
                              $"?query={Uri.EscapeDataString(query)}" +
                              $"&location={lat},{lon}&radius=800000&key={_apiKey}";
                    var json = JObject.Parse(await _http.GetStringAsync(url));

                    foreach (var r in json["results"] as JArray ?? new JArray())
                    {
                        var name = r["name"]?.ToString() ?? "";
                        var rLat = r["geometry"]?["location"]?["lat"]?.Value<double>() ?? 0;
                        var rLon = r["geometry"]?["location"]?["lng"]?.Value<double>() ?? 0;
                        if (rLat == 0) continue;

                        // Filter: must look like a border crossing or port of entry
                        if (!IsBorderRelated(name)) continue;

                        // Deduplicate: skip if we already have a result within 30km
                        bool duplicate = results.Any(existing =>
                            ApproxDistanceKm(rLat, rLon, existing.Lat, existing.Lon) < 30);
                        if (!duplicate)
                            results.Add((name, rLat, rLon));
                    }
                }
                catch { /* silently continue */ }
            }

            return results
                .OrderBy(b => ApproxDistanceKm(lat, lon, b.Lat, b.Lon))
                .ToList();
        }

        private static bool IsBorderRelated(string name)
        {
            var lower = name.ToLower();
            return lower.Contains("border") || lower.Contains("crossing") ||
                   lower.Contains("port of entry") || lower.Contains("immigration") ||
                   lower.Contains("customs") || lower.Contains("checkpoint") ||
                   lower.Contains("frontier") || lower.Contains("boundary") ||
                   lower.Contains("poste frontière") || lower.Contains("grenzübergang") ||
                   lower.Contains("frontera") || lower.Contains("grens") ||
                   lower.Contains("posto de fronteira");
        }

        private async Task<string?> GetDirections(
            double originLat, double originLon,
            double destLat,   double destLon,
            string destName)
        {
            try
            {
                var url = "https://maps.googleapis.com/maps/api/directions/json" +
                          $"?origin={originLat},{originLon}" +
                          $"&destination={destLat},{destLon}" +
                          $"&key={_apiKey}";
                var json = JObject.Parse(await _http.GetStringAsync(url));

                var routes = json["routes"] as JArray;
                if (routes == null || routes.Count == 0) return null;

                var route   = routes[0];
                var leg      = route["legs"]?[0];
                var summary  = route["summary"]?.ToString() ?? "";
                var distance = leg?["distance"]?["text"]?.ToString() ?? "";
                var duration = leg?["duration"]?["text"]?.ToString() ?? "";

                // Extract road identifiers from step instructions (N1, R21, A3, M4, I-95, US-1, etc.)
                var roads = new LinkedList<string>();
                foreach (var step in (route["legs"]?[0]?["steps"] as JArray ?? new JArray()).Take(20))
                {
                    var raw   = step["html_instructions"]?.ToString() ?? "";
                    var clean = Regex.Replace(raw, "<.*?>", " ").Trim();
                    // Match road codes: N1, R21, A3, M4, I-95, US-1, B1234, etc.
                    foreach (Match m in Regex.Matches(clean, @"\b([A-Z]{1,2}-?\d{1,4})\b"))
                    {
                        if (!roads.Contains(m.Value) && roads.Count < 6)
                            roads.AddLast(m.Value);
                    }
                }

                var roadCodes = roads.Count > 0 ? $" ({string.Join(", ", roads.Take(5))})" : "";
                var via       = !string.IsNullOrEmpty(summary) ? $" via {summary}{roadCodes}" : roadCodes;
                return $"  -> {destName}: {distance} / approx. {duration}{via}";
            }
            catch { return null; }
        }

        private static double ApproxDistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            double dLat = (lat2 - lat1) * Math.PI / 180;
            double dLon = (lon2 - lon1) * Math.PI / 180;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }
    }
}
