using SQLite;

namespace SafetySentinel.Models
{
    [Table("country_profiles")]
    public class CountryProfile
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string CountryCode { get; set; } = "";
        public string CountryName { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int OverallSafetyScore { get; set; }
        public int PhysicalSecurity { get; set; }
        public int PoliticalStability { get; set; }
        public int EconomicFreedom { get; set; }
        public int DigitalSovereignty { get; set; }
        public int HealthEnvironment { get; set; }
        public int SocialCohesion { get; set; }
        public int MobilityExit { get; set; }
        public int Infrastructure { get; set; }
        public string CbdcStatus { get; set; } = "none";
        public int CashFreedomScore { get; set; }
        public string AiIntegrationMandate { get; set; } = "none";
        public int SurveillanceScore { get; set; }
        public int GenocideStage { get; set; }
        public int EsimAvailable { get; set; }
        public int InternetFreedomScore { get; set; }
        public long UpdatedAt { get; set; }
    }
}
