using System.Text.Json.Serialization;

namespace TaskManagerAPI.Models
{
    public class RoleMenu
    {
        public int Id { get; set; }
        
        public int RoleId { get; set; }
        [JsonIgnore]
        public Role? Role { get; set; }

        public int MenuId { get; set; }
        public Menu? Menu { get; set; }
    }
}