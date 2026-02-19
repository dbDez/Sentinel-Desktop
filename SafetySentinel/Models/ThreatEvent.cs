using SQLite;

namespace SafetySentinel.Models
{
    [Table("threat_events")]
    public class ThreatEvent
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public long Timestamp { get; set; }
        public string Domain { get; set; } = "";
        public string Category { get; set; } = "";
        public int Severity { get; set; }
        public string Title { get; set; } = "";
        public string Summary { get; set; } = "";
        public string SourceUrl { get; set; } = "";
        public int SourceCredibility { get; set; }
        public string CountryCode { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public long ExpiresAt { get; set; }
        public long CreatedAt { get; set; }
        /// <summary>Links this event to a CrimeHotspot. 0 = not linked.</summary>
        public int HotspotId { get; set; }
    }
}
