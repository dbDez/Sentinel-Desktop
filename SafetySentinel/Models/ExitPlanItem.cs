using SQLite;
using System;

namespace SafetySentinel.Models
{
    [Table("exit_plan_items")]
    public class ExitPlanItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string PlanName { get; set; } = "";
        public string Category { get; set; } = "";
        public int SortOrder { get; set; }
        public string TaskTitle { get; set; } = "";
        public string TaskDescription { get; set; } = "";
        public bool Completed { get; set; }
        [Column("CompletedDate")]
        public long? CompletedDateTicks { get; set; }
        public long CreatedAt { get; set; }

        [Ignore]
        public DateTime? CompletedDate
        {
            get => CompletedDateTicks.HasValue
                ? DateTimeOffset.FromUnixTimeMilliseconds(CompletedDateTicks.Value).LocalDateTime
                : null;
            set => CompletedDateTicks = value.HasValue
                ? new DateTimeOffset(value.Value).ToUnixTimeMilliseconds()
                : null;
        }
    }
}
