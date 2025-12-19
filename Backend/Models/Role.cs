namespace TaskManagerAPI.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Contoh: "Admin", "ProjectManager"

        // Relasi: Satu role punya banyak akses menu
        public List<RoleMenu> RoleMenus { get; set; } = new();
    }
}