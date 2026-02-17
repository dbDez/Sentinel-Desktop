using SQLite;
using System;

namespace SafetySentinel.Models
{
    [Table("personal_alerts")]
    public class PersonalAlert
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Description { get; set; } = "";
        public bool Active { get; set; } = true;
        public long CreatedAt { get; set; }

        [Ignore]
        public DateTime CreatedDate =>
            DateTimeOffset.FromUnixTimeMilliseconds(CreatedAt).LocalDateTime;

        [Ignore]
        public string DisplayText => $"{Description}  ({CreatedDate:yyyy-MM-dd})";
    }
}
