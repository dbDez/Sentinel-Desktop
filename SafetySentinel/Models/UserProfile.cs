using SQLite;

namespace SafetySentinel.Models
{
    [Table("user_profile")]
    public class UserProfile
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string IdNumber { get; set; } = "";
        public string SocialSecurityNumber { get; set; } = "";
        public int Age { get; set; }
        public string Ethnicity { get; set; } = "";
        public string Gender { get; set; } = "Male";
        public string CurrentCountry { get; set; } = "ZA";
        public string CurrentCity { get; set; } = "";
        public double HomeLatitude { get; set; }
        public double HomeLongitude { get; set; }
        public string DestinationCountry { get; set; } = "US";
        public string DestinationCity { get; set; } = "";
        public string VehicleType { get; set; } = "Sedan";
        public string VehicleMake { get; set; } = "";
        public string VehicleModel { get; set; } = "";
        public string Values { get; set; } = "";
        public string HealthApproach { get; set; } = "";
        public string Skills { get; set; } = "";
        public string ImmigrationStatus { get; set; } = "";
        public long UpdatedAt { get; set; }
    }
}
