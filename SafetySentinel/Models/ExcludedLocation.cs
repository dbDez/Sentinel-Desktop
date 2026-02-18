using SQLite;

namespace SafetySentinel.Models
{
    [Table("excluded_locations")]
    public class ExcludedLocation
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string CountryCode { get; set; } = "";
        public string CountryName { get; set; } = "";
        public string Reason { get; set; } = "";
        public long AddedAt { get; set; }
    }
}
