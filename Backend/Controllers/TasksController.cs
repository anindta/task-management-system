using Microsoft.AspNetCore.Authorization; // Wajib ada buat Security
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // Buat baca isi Token (Siapa yg login)
using TaskManagerAPI.Data;
using TaskManagerAPI.Models;

namespace TaskManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // <--- GEMBOK 1: User wajib Login (punya Token) buat akses controller ini
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GET: api/Tasks
        // Logic: Admin/PM liat semua, Employee cuma liat punya sendiri
        // GET: api/Tasks?projectId=1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks([FromQuery] int? projectId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var query = _context.Tasks
                .Include(t => t.AssignedUser) // Include User biar avatarnya muncul
                .Include(t => t.Project)
                .AsQueryable();

            // --- FILTER PROJECT (PENTING) ---
            // Kalau projectId dikirim (misal: /api/Tasks?projectId=1), filter datanya.
            // Kalau null, JANGAN kembalikan apa-apa (biar kanban gak bingung), atau kembalikan semua.
            // Kita pilih: Filter ketat.
            if (projectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == projectId.Value);
            }

            if (userRole == "Employee")
            {
                query = query.Where(t => t.AssignedUserId == userId);
            }

            return await query.ToListAsync();
        }

        // 2. POST: api/Tasks
        // Logic: Cuma Admin & ProjectManager yang boleh bikin tugas
        [HttpPost]
        [Authorize(Roles = "Admin,ProjectManager")] // <--- GEMBOK 2: Employee dilarang masuk sini
        public async Task<ActionResult<TaskItem>> CreateTask(TaskItem task)
        {
            if (task.Deadline == DateTime.MinValue) task.Deadline = DateTime.Now.AddDays(7);

            // Validasi Project (Default 1)
            if (task.ProjectId == 0) task.ProjectId = 1;

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTasks), new { id = task.Id }, task);
        }

        // 3. PUT: api/Tasks/5/status
        // Logic: Employee cuma boleh geser tugasnya sendiri
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] int newStatus)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            // Validasi input status (0, 1, 2)
            if (!Enum.IsDefined(typeof(TaskStatusEnum), newStatus))
            {
                return BadRequest("Status tidak valid");
            }

            task.Status = (TaskStatusEnum)newStatus;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Status berhasil diupdate", newStatus = task.Status });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskItem task)
        {
            if (id != task.Id) return BadRequest();

            var existingTask = await _context.Tasks.FindAsync(id);
            if (existingTask == null) return NotFound();

            // Validasi: Cuma Admin, PM, atau Pemilik tugas yang boleh edit
            // (Bisa tambahkan logic security RBAC di sini kalau mau ketat)

            existingTask.Title = task.Title;
            existingTask.Description = task.Description;
            existingTask.Deadline = task.Deadline;
            existingTask.AssignedUserId = task.AssignedUserId;
            // Status tidak diupdate disini, tapi di endpoint khusus status

            await _context.SaveChangesAsync();
            return Ok(new { message = "Tugas berhasil diupdate" });
        }

        // --- TAMBAHAN BARU: Delete Tugas ---
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tugas berhasil dihapus" });
        }
        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteTask(int id, [FromBody] string note)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            task.Status = TaskStatusEnum.Done; // Paksa jadi Done
            task.CompletionNote = note;        // Simpan catatan

            await _context.SaveChangesAsync();
            return Ok(new { message = "Tugas selesai", note = task.CompletionNote });
        }
    }
}