using SQLite;

namespace SafetySentinel.Models
{
    [Table("avoidance_items")]
    public class AvoidanceItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int RadiusMeters { get; set; }
        public string Reason { get; set; } = "";
        public int RiskLevel { get; set; }
        public string TimeRestriction { get; set; } = "";
        public bool Active { get; set; } = true;
        public long CreatedAt { get; set; }
    }
}
