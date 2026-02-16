using SQLite;

namespace SafetySentinel.Models
{
    [Table("sources")]
    public class Source
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public string SourceType { get; set; } = "";
        public int ReliabilityScore { get; set; }
        public string BiasNotes { get; set; } = "";
        public long LastAccessed { get; set; }
    }
}
