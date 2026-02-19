using SQLite;

namespace SafetySentinel.Models
{
    [Table("watchlist")]
    public class WatchlistItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string CountryCode { get; set; } = "";
        public string CountryName { get; set; } = "";
        public string City { get; set; } = "";
        public string StateProvince { get; set; } = "";
        public string Reason { get; set; } = "";
        public int AlertThreshold { get; set; } = 60;
        public int ChangeThreshold { get; set; } = 10;
        public bool NotifyOnChange { get; set; } = true;
        public long AddedAt { get; set; }
        public bool ExitPlan { get; set; } = false;
        public bool ContinentAdded { get; set; } = false;

        [Ignore]
        public string ExitPlanLabel => ExitPlan ? "✓ EXIT" : "";

        [Ignore]
        public string DisplayText =>
            string.IsNullOrEmpty(City)
                ? $"{CountryName} ({CountryCode})"
                : string.IsNullOrEmpty(StateProvince)
                    ? $"{CountryName} — {City} ({CountryCode})"
                    : $"{CountryName} — {City}, {StateProvince} ({CountryCode})";

        public override string ToString() => DisplayText;
    }
}
