namespace TaskManagerAPI.DTOs
{
    public class MenuDto
    {
        public string Name { get; set; } = string.Empty; // Kode unik, misal: "view_reports"
        public string Label { get; set; } = string.Empty; // Tampilan, misal: "Laporan Keuangan"
        public string Icon { get; set; } = string.Empty;
    }
}