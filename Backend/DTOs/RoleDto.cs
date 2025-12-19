namespace TaskManagerAPI.DTOs
{
    public class CreateRoleDto
    {
        public string Name { get; set; } = string.Empty;
        public List<int> MenuIds { get; set; } = new(); // List ID menu yang dipilih
    }

    public class RoleDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> MenuLabels { get; set; } = new(); // Untuk ditampilkan di tabel
        public List<int> MenuIds { get; set; } = new(); // Untuk mengisi checkbox saat Edit
    }
}