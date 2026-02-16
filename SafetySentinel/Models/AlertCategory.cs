using SQLite;

namespace SafetySentinel.Models
{
    [Table("alert_categories")]
    public class AlertCategory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Domain { get; set; } = "";
        public string Category { get; set; } = "";
        public string Description { get; set; } = "";
        public bool Enabled { get; set; } = true;
        public int SeverityWeight { get; set; }
        public string ScanFrequency { get; set; } = "daily";
    }
}
