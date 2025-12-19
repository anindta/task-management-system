namespace TaskManagerAPI.Models
{
    public enum TaskPriority
    {
        Low = 0,
        Medium = 1,
        High = 2
    }
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        
        public TaskStatusEnum Status { get; set; } // Menggunakan Enum yang kita buat di langkah 1

        // --- RELASI (Foreign Keys) ---

        // 1. Relasi ke Project (Wajib ada projectnya)
        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        // 2. Relasi ke User (Opsional, boleh belum ada yang ngerjain/null)
        public int? AssignedUserId { get; set; }
        public User? AssignedUser { get; set; }
        public string? CompletionNote { get; set; }
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    }
}