using System.Text.Json.Serialization;

namespace TaskManagerAPI.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Relasi: Satu project punya banyak task
        [JsonIgnore]
        public List<TaskItem>? Tasks { get; set; }
    }
}