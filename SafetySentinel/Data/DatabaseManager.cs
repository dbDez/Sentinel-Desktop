using SafetySentinel.Models;
using SQLite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SafetySentinel.Data
{
    public class DatabaseManager : IDisposable
    {
        private readonly SQLiteConnection _db;
        private readonly string _dbPath;

        public DatabaseManager()
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SafetySentinel");
            Directory.CreateDirectory(folder);
            _dbPath = Path.Combine(folder, "sentinel.db");

            _db = new SQLiteConnection(_dbPath);
            CreateTables();
        }

        private void CreateTables()
        {
            _db.CreateTable<UserProfile>();
            _db.CreateTable<CountryProfile>();
            _db.CreateTable<AlertCategory>();
            _db.CreateTable<CrimeHotspot>();
            _db.CreateTable<DailyScore>();
            _db.CreateTable<ExecutiveBrief>();
            _db.CreateTable<WatchlistItem>();
            _db.CreateTable<ExitPlanItem>();
            _db.CreateTable<ThreatEvent>();
            _db.CreateTable<AvoidanceItem>();
            _db.CreateTable<Source>();
            _db.CreateTable<GeofenceAlert>();
            _db.CreateTable<ActionItem>();
            _db.CreateTable<PersonalAlert>();
            _db.CreateTable<ExcludedLocation>();
            _db.CreateTable<ApiUsageRecord>();

            // Migrations: add new columns to existing watchlist table
            try { _db.Execute("ALTER TABLE watchlist ADD COLUMN City TEXT NOT NULL DEFAULT ''"); } catch { }
            try { _db.Execute("ALTER TABLE watchlist ADD COLUMN StateProvince TEXT NOT NULL DEFAULT ''"); } catch { }
            try { _db.Execute("ALTER TABLE watchlist ADD COLUMN ExitPlan INTEGER NOT NULL DEFAULT 0"); } catch { }
            try { _db.Execute("ALTER TABLE watchlist ADD COLUMN ContinentAdded INTEGER NOT NULL DEFAULT 0"); } catch { }
            // Migration: add ApiMonthlyBudget to user_profile
            try { _db.Execute("ALTER TABLE user_profile ADD COLUMN ApiMonthlyBudget REAL NOT NULL DEFAULT 0"); } catch { }
            // Migration: add ApiBalanceSetAt to user_profile
            try { _db.Execute("ALTER TABLE user_profile ADD COLUMN ApiBalanceSetAt INTEGER NOT NULL DEFAULT 0"); } catch { }
            // Migration: add WatchlistSnapshot to executive_briefs
            try { _db.Execute("ALTER TABLE executive_briefs ADD COLUMN WatchlistSnapshot TEXT NOT NULL DEFAULT ''"); } catch { }
            // Migration: add HotspotId to threat_events
            try { _db.Execute("ALTER TABLE threat_events ADD COLUMN HotspotId INTEGER NOT NULL DEFAULT 0"); } catch { }

            // Remove old hardcoded exit plan seed data (new plans are AI-generated per watchlist entry)
            _db.Execute("DELETE FROM exit_plan_items WHERE PlanName LIKE 'Plan A:%' OR PlanName LIKE 'Plan B:%' OR PlanName LIKE 'Plan C:%'");

            SeedIfEmpty();
        }

        private void SeedIfEmpty()
        {
            if (_db.Table<CountryProfile>().Count() == 0)
            {
                _db.InsertAll(SeedData.GetCountries());
            }

            if (_db.Table<AlertCategory>().Count() == 0)
            {
                _db.InsertAll(SeedData.GetAlertCategories());
            }

            if (_db.Table<CrimeHotspot>().Count() == 0)
            {
                _db.InsertAll(SeedData.GetSouthAfricaHotspots());
            }

            // Exit plans are no longer seeded — they are AI-generated per watchlist entry on scan
        }

        public string GetDatabasePath() => _dbPath;

        #region Profile

        public UserProfile GetProfile()
        {
            var profile = _db.Table<UserProfile>().FirstOrDefault();
            return profile ?? new UserProfile();
        }

        public void SaveProfile(UserProfile profile)
        {
            profile.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (profile.Id == 0)
                _db.Insert(profile);
            else
                _db.Update(profile);
        }

        #endregion

        #region Countries

        public List<CountryProfile> GetAllCountries()
        {
            return _db.Table<CountryProfile>().ToList();
        }

        public void SaveCountry(CountryProfile country)
        {
            country.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (country.Id == 0)
                _db.Insert(country);
            else
                _db.Update(country);
        }

        #endregion

        #region Hotspots

        public List<CrimeHotspot> GetActiveHotspots()
        {
            return _db.Table<CrimeHotspot>().Where(h => h.Active).ToList();
        }

        #endregion

        #region Threat Events

        public void SaveThreatEvent(ThreatEvent e)
        {
            e.CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (e.Id == 0)
                _db.Insert(e);
            else
                _db.Update(e);
        }

        public List<ThreatEvent> GetThreatEventsForHotspot(int hotspotId)
        {
            return _db.Table<ThreatEvent>()
                .Where(e => e.HotspotId == hotspotId)
                .OrderByDescending(e => e.Timestamp)
                .ToList();
        }

        public void DeleteThreatEventsForHotspot(int hotspotId)
        {
            _db.Execute("DELETE FROM threat_events WHERE HotspotId = ?", hotspotId);
        }

        /// <summary>Returns the CreatedAt timestamp of the most recent fetch for this hotspot, or 0 if none.</summary>
        public long GetHotspotIncidentsFetchedAt(int hotspotId)
        {
            return _db.Table<ThreatEvent>()
                .Where(e => e.HotspotId == hotspotId)
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => e.CreatedAt)
                .FirstOrDefault();
        }

        #endregion

        #region Alert Categories

        public List<AlertCategory> GetAlertCategories()
        {
            return _db.Table<AlertCategory>().ToList();
        }

        public void SaveAlertCategory(AlertCategory cat)
        {
            _db.Update(cat);
        }

        #endregion

        #region Briefs

        public ExecutiveBrief? GetLatestBrief()
        {
            return _db.Table<ExecutiveBrief>()
                .OrderByDescending(b => b.BriefDateTicks)
                .FirstOrDefault();
        }

        public List<ExecutiveBrief> GetAllBriefs()
        {
            return _db.Table<ExecutiveBrief>()
                .OrderByDescending(b => b.BriefDateTicks)
                .ToList();
        }

        public void SaveBrief(ExecutiveBrief brief)
        {
            brief.CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _db.Insert(brief);
        }

        public void UpdateBrief(ExecutiveBrief brief)
        {
            _db.Update(brief);
        }

        public void DeleteBrief(int id)
        {
            _db.Delete<ExecutiveBrief>(id);
        }

        #endregion

        #region Watchlist

        public List<WatchlistItem> GetWatchlist()
        {
            return _db.Table<WatchlistItem>().ToList();
        }

        public void AddWatchlistItem(WatchlistItem item)
        {
            item.AddedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _db.Insert(item);
        }

        public void RemoveWatchlistItem(int id)
        {
            _db.Delete<WatchlistItem>(id);
        }

        public void UpdateWatchlistItem(WatchlistItem item)
        {
            _db.Update(item);
        }

        #endregion

        #region Avoidance Items

        public List<AvoidanceItem> GetAvoidanceItems()
        {
            return _db.Table<AvoidanceItem>().Where(a => a.Active).ToList();
        }

        #endregion

        #region Excluded Locations

        public List<ExcludedLocation> GetExcludedLocations()
        {
            return _db.Table<ExcludedLocation>().ToList();
        }

        public void AddExcludedLocation(ExcludedLocation item)
        {
            item.AddedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _db.Insert(item);
        }

        public void RemoveExcludedLocation(int id)
        {
            _db.Delete<ExcludedLocation>(id);
        }

        public bool IsExplicitlyExcluded(string countryCode)
        {
            return _db.Table<ExcludedLocation>()
                .Any(e => e.CountryCode == countryCode);
        }

        #endregion

        #region Exit Plans

        public List<ExitPlanItem> GetExitPlanByName(string planName)
        {
            return _db.Table<ExitPlanItem>()
                .Where(e => e.PlanName == planName)
                .OrderBy(e => e.SortOrder)
                .ToList();
        }

        public void SaveExitPlanItem(ExitPlanItem item)
        {
            _db.Update(item);
        }

        public void InsertExitPlanItem(ExitPlanItem item)
        {
            item.CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _db.Insert(item);
        }

        public void ClearExitPlanByName(string planName)
        {
            _db.Execute("DELETE FROM exit_plan_items WHERE PlanName = ?", planName);
        }

        public List<string> GetDistinctPlanNames()
        {
            return _db.Table<ExitPlanItem>()
                .Select(e => e.PlanName)
                .Distinct()
                .ToList();
        }

        #endregion

        #region Daily Scores

        public void SaveDailyScore(DailyScore score)
        {
            score.CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _db.Insert(score);
        }

        public List<DailyScore> GetDailyScores(string countryCode)
        {
            return _db.Table<DailyScore>()
                .Where(d => d.CountryCode == countryCode)
                .OrderByDescending(d => d.Date)
                .ToList();
        }

        #endregion

        #region Personal Alerts

        public List<PersonalAlert> GetActivePersonalAlerts()
        {
            return _db.Table<PersonalAlert>().Where(a => a.Active).ToList();
        }

        public void AddPersonalAlert(PersonalAlert alert)
        {
            alert.CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _db.Insert(alert);
        }

        public void RemovePersonalAlert(int id)
        {
            _db.Delete<PersonalAlert>(id);
        }

        #endregion

        #region Export / Import

        public string ExportAllDataToJson()
        {
            var data = new
            {
                profile = GetProfile(),
                countries = GetAllCountries(),
                alertCategories = GetAlertCategories(),
                hotspots = _db.Table<CrimeHotspot>().ToList(),
                briefs = GetAllBriefs(),
                watchlist = GetWatchlist(),
                exitPlans = _db.Table<ExitPlanItem>().ToList(),
                dailyScores = _db.Table<DailyScore>().ToList(),
                avoidanceItems = _db.Table<AvoidanceItem>().ToList(),
                exportDate = DateTime.UtcNow
            };
            return JsonConvert.SerializeObject(data, Formatting.Indented);
        }

        public string ExportForMobile()
        {
            var data = new
            {
                format = "sentinel-mobile",
                version = 1,
                profile = GetProfile(),
                countries = GetAllCountries(),
                hotspots = GetActiveHotspots(),
                watchlist = GetWatchlist(),
                exitPlans = _db.Table<ExitPlanItem>().ToList(),
                avoidanceItems = GetAvoidanceItems(),
                latestBrief = GetLatestBrief(),
                exportDate = DateTime.UtcNow
            };
            return JsonConvert.SerializeObject(data, Formatting.Indented);
        }

        public (int countries, int hotspots, int watchlist, int avoidance, int exitPlans) ImportFromSentinel(string json)
        {
            var data = Newtonsoft.Json.Linq.JObject.Parse(json);
            int countries = 0, hotspots = 0, watchlist = 0, avoidance = 0, exitPlans = 0;

            if (data["countries"] != null)
            {
                var items = data["countries"]!.ToObject<List<CountryProfile>>() ?? new();
                foreach (var item in items)
                {
                    var existing = _db.Table<CountryProfile>()
                        .FirstOrDefault(c => c.CountryCode == item.CountryCode);
                    if (existing != null)
                    {
                        item.Id = existing.Id;
                        _db.Update(item);
                    }
                    else
                    {
                        item.Id = 0;
                        _db.Insert(item);
                    }
                    countries++;
                }
            }

            if (data["hotspots"] != null)
            {
                var items = data["hotspots"]!.ToObject<List<CrimeHotspot>>() ?? new();
                foreach (var item in items)
                {
                    item.Id = 0;
                    _db.Insert(item);
                    hotspots++;
                }
            }

            if (data["watchlist"] != null)
            {
                var items = data["watchlist"]!.ToObject<List<WatchlistItem>>() ?? new();
                foreach (var item in items)
                {
                    item.Id = 0;
                    _db.Insert(item);
                    watchlist++;
                }
            }

            if (data["avoidanceItems"] != null)
            {
                var items = data["avoidanceItems"]!.ToObject<List<AvoidanceItem>>() ?? new();
                foreach (var item in items)
                {
                    item.Id = 0;
                    _db.Insert(item);
                    avoidance++;
                }
            }

            if (data["exitPlans"] != null)
            {
                var items = data["exitPlans"]!.ToObject<List<ExitPlanItem>>() ?? new();
                foreach (var item in items)
                {
                    item.Id = 0;
                    _db.Insert(item);
                    exitPlans++;
                }
            }

            if (data["profile"] != null)
            {
                var imported = data["profile"]!.ToObject<UserProfile>();
                if (imported != null)
                {
                    var existing = GetProfile();
                    if (existing.Id > 0)
                    {
                        imported.Id = existing.Id;
                        _db.Update(imported);
                    }
                    else
                    {
                        imported.Id = 0;
                        _db.Insert(imported);
                    }
                }
            }

            return (countries, hotspots, watchlist, avoidance, exitPlans);
        }

        #endregion

        #region API Usage

        public void AddApiUsage(ApiUsageRecord record)
        {
            _db.Insert(record);
        }

        /// <summary>Returns total spending for the current calendar month.</summary>
        public decimal GetMonthlySpend()
        {
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            return _db.Table<ApiUsageRecord>()
                .Where(r => r.Timestamp >= startOfMonth)
                .ToList()
                .Sum(r => r.Cost);
        }

        /// <summary>Returns total spending across all time.</summary>
        public decimal GetTotalSpend()
        {
            return _db.Table<ApiUsageRecord>().ToList().Sum(r => r.Cost);
        }

        /// <summary>Returns total spending since the given date (used for balance-based tracking).</summary>
        public decimal GetSpendSince(DateTime from)
        {
            return _db.Table<ApiUsageRecord>()
                .Where(r => r.Timestamp >= from)
                .ToList()
                .Sum(r => r.Cost);
        }

        #endregion

        public void Dispose()
        {
            _db?.Close();
            _db?.Dispose();
        }
    }
}
