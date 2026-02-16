using SQLite;

namespace SafetySentinel.Models
{
    [Table("daily_scores")]
    public class DailyScore
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public long Date { get; set; }
        public string CountryCode { get; set; } = "";
        public string Domain { get; set; } = "";
        public int Score { get; set; }
        public double Score7dAvg { get; set; }
        public double Score30dAvg { get; set; }
        public double Score90dAvg { get; set; }
        public double RateOfChange { get; set; }
        public long CreatedAt { get; set; }
    }
}
