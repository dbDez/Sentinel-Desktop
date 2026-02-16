using SQLite;

namespace SafetySentinel.Models
{
    [Table("crime_hotspots")]
    public class CrimeHotspot
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int RadiusMeters { get; set; }
        public string CrimeType { get; set; } = "";
        public int Severity { get; set; }
        public string TimePattern { get; set; } = "";
        public string Precinct { get; set; } = "";
        public string LocationName { get; set; } = "";
        public long LastIncidentDate { get; set; }
        public int IncidentCount90d { get; set; }
        public string CountryCode { get; set; } = "ZA";
        public bool Active { get; set; } = true;
        public long UpdatedAt { get; set; }
    }
}
