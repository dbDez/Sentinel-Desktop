using SQLite;
using System;

namespace SafetySentinel.Models
{
    [Table("api_usage")]
    public class ApiUsageRecord
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Model { get; set; } = "";
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
        public decimal Cost { get; set; }
        /// <summary>brief | chat | smartselect | exitplan | resolve</summary>
        public string CallType { get; set; } = "";
    }
}
