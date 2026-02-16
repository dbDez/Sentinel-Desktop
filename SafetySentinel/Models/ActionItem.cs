using SQLite;

namespace SafetySentinel.Models
{
    [Table("action_items")]
    public class ActionItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Priority { get; set; } = "Medium";
        public string Category { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public long Deadline { get; set; }
        public bool Completed { get; set; }
        public long CreatedAt { get; set; }
    }
}
