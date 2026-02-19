using SQLite;
using System;

namespace SafetySentinel.Models
{
    [Table("executive_briefs")]
    public class ExecutiveBrief
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Column("BriefDate")]
        public long BriefDateTicks { get; set; }
        public string BriefType { get; set; } = "daily";
        public string OverallThreatLevel { get; set; } = "GREEN";
        public string Content { get; set; } = "";
        public string ActionItems { get; set; } = "";
        public string ChatHistory { get; set; } = "";
        /// <summary>JSON array of CountryCode strings watched when this brief was generated.</summary>
        public string WatchlistSnapshot { get; set; } = "";
        public long CreatedAt { get; set; }

        [Ignore]
        public DateTime BriefDate
        {
            get => DateTimeOffset.FromUnixTimeMilliseconds(BriefDateTicks).LocalDateTime;
            set => BriefDateTicks = new DateTimeOffset(value).ToUnixTimeMilliseconds();
        }

        [Ignore]
        public string DisplayText => $"{BriefDate:yyyy-MM-dd HH:mm} | {OverallThreatLevel}";
    }
}
