namespace TaskManagerAPI.DTOs
{
    public class DashboardStatDto
    {
        public int TotalProjects { get; set; }
        public int TotalUsers { get; set; }
        
        // Statistik Tugas User yang Login
        public int MyTodo { get; set; }
        public int MyProgress { get; set; }
        public int MyDone { get; set; }
    }
}