using System.Text.Json.Serialization;

namespace TaskManagerAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // --- UPDATE DI SINI ---
        // Bukan string lagi, tapi ID
        public int RoleId { get; set; } 
        public Role? Role { get; set; }
        // ----------------------

        [JsonIgnore] 
        public List<TaskItem>? AssignedTasks { get; set; }
    }
}