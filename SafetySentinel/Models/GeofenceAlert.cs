using SQLite;

namespace SafetySentinel.Models
{
    [Table("geofence_alerts")]
    public class GeofenceAlert
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int HotspotId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string AlertType { get; set; } = "";
        public long Timestamp { get; set; }
        public string UserAction { get; set; } = "";
    }
}
