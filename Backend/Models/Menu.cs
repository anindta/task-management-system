namespace TaskManagerAPI.Models
{
    public class Menu
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Utk coding, misal: "view_users"
        public string Label { get; set; } = string.Empty; // Utk tampilan, misal: "Manajemen User"
        public string Icon { get; set; } = "ri-circle-line";
    }
}