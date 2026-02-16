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
        public string Reason { get; set; } = "";
        public int AlertThreshold { get; set; } = 60;
        public int ChangeThreshold { get; set; } = 10;
        public bool NotifyOnChange { get; set; } = true;
        public long AddedAt { get; set; }
    }
}
