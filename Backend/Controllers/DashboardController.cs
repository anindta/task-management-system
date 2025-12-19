using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTOs;
using TaskManagerAPI.Models;

namespace TaskManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStatDto>> GetStats()
        {
            // Ambil ID User yang sedang login
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null) return Unauthorized();
            
            var userId = int.Parse(userIdStr);

            // 1. Hitung Total Proyek (Global)
            var totalProjects = await _context.Projects.CountAsync();

            // 2. Hitung Total User (Global)
            var totalUsers = await _context.Users.CountAsync();

            // 3. Hitung Tugas MILIK SAYA (Personal)
            // Status: 0=Todo, 1=Progress, 2=Done
            var myTasks = await _context.Tasks
                .Where(t => t.AssignedUserId == userId)
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            // Mapping hasil group by ke DTO
            var stats = new DashboardStatDto
            {
                TotalProjects = totalProjects,
                TotalUsers = totalUsers,
                MyTodo = myTasks.FirstOrDefault(x => x.Status == TaskStatusEnum.Todo)?.Count ?? 0,
                MyProgress = myTasks.FirstOrDefault(x => x.Status == TaskStatusEnum.OnProgress)?.Count ?? 0,
                MyDone = myTasks.FirstOrDefault(x => x.Status == TaskStatusEnum.Done)?.Count ?? 0
            };

            return Ok(stats);
        }
    }
}