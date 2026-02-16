using SafetySentinel.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SafetySentinel.Services
{
    /// <summary>
    /// Calculates threat scores. All scores are THREAT-based: higher = more dangerous (0-100).
    /// </summary>
    public class ThreatScoringEngine
    {
        // Domain weights for overall threat calculation
        private static readonly Dictionary<string, double> DomainWeights = new()
        {
            ["physical"] = 0.20,
            ["political"] = 0.15,
            ["economic"] = 0.15,
            ["digital"] = 0.10,
            ["health"] = 0.10,
            ["social"] = 0.10,
            ["mobility"] = 0.10,
            ["infrastructure"] = 0.10
        };

        /// <summary>
        /// Calculate overall threat score from individual domain scores.
        /// All scores are threat-based (higher = worse).
        /// </summary>
        public int CalculateOverallScore(CountryProfile country)
        {
            double weighted =
                country.PhysicalSecurity * DomainWeights["physical"] +
                country.PoliticalStability * DomainWeights["political"] +
                country.EconomicFreedom * DomainWeights["economic"] +
                country.DigitalSovereignty * DomainWeights["digital"] +
                country.HealthEnvironment * DomainWeights["health"] +
                country.SocialCohesion * DomainWeights["social"] +
                country.MobilityExit * DomainWeights["mobility"] +
                country.Infrastructure * DomainWeights["infrastructure"];

            // Genocide stage amplifier: escalate overall score for high genocide stages
            if (country.GenocideStage >= 6)
                weighted = Math.Min(100, weighted * 1.3);
            else if (country.GenocideStage >= 4)
                weighted = Math.Min(100, weighted * 1.15);

            return (int)Math.Round(Math.Clamp(weighted, 0, 100));
        }

        /// <summary>
        /// Calculate hijacking risk based on proximity to hotspots and vehicle type.
        /// Returns 0-100 threat score (higher = more dangerous).
        /// </summary>
        public int CalculateHijackingRisk(double lat, double lon, string vehicleType, List<CrimeHotspot> hotspots)
        {
            if (hotspots == null || hotspots.Count == 0) return 0;

            double maxRisk = 0;
            foreach (var hotspot in hotspots.Where(h => h.CrimeType.Contains("Hijacking", StringComparison.OrdinalIgnoreCase)))
            {
                double distanceKm = HaversineDistance(lat, lon, hotspot.Latitude, hotspot.Longitude);
                double radiusKm = hotspot.RadiusMeters / 1000.0;

                // Risk decays with distance — inside radius = full risk, decays to zero at 3x radius
                double proximityFactor;
                if (distanceKm <= radiusKm)
                    proximityFactor = 1.0;
                else if (distanceKm <= radiusKm * 3)
                    proximityFactor = 1.0 - ((distanceKm - radiusKm) / (radiusKm * 2));
                else
                    proximityFactor = 0;

                double risk = hotspot.Severity * proximityFactor;
                maxRisk = Math.Max(maxRisk, risk);
            }

            // Vehicle type modifier — luxury/SUV vehicles are higher risk
            double vehicleMod = vehicleType?.ToLower() switch
            {
                "suv" or "4x4" => 1.2,
                "luxury" => 1.3,
                "sedan" => 1.0,
                "hatchback" => 0.9,
                "bakkie" or "truck" => 1.1,
                _ => 1.0
            };

            return (int)Math.Round(Math.Clamp(maxRisk * vehicleMod, 0, 100));
        }

        /// <summary>
        /// Get threat level label from threat score. Higher score = worse.
        /// </summary>
        public static string GetThreatLevel(int threatScore)
        {
            return threatScore switch
            {
                >= 76 => "RED",
                >= 51 => "ORANGE",
                >= 26 => "YELLOW",
                _ => "GREEN"
            };
        }

        private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth radius in km
            double dLat = ToRad(lat2 - lat1);
            double dLon = ToRad(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private static double ToRad(double deg) => deg * Math.PI / 180;
    }
}
