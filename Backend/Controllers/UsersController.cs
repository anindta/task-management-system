using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Data;
using TaskManagerAPI.Models;
using TaskManagerAPI.DTOs;

namespace TaskManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            // Kita return object custom biar Role yang muncul Namanya, bukan ID doang
            var users = await _context.Users
                .Include(u => u.Role)
                .Select(u => new {
                    u.Id,
                    u.Username,
                    u.Email,
                    Role = u.Role != null ? u.Role.Name : "No Role" // Ambil nama rolenya
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound("User tidak ditemukan");
            return user;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> CreateUser(CreateUserDto request)
        {
            if (_context.Users.Any(u => u.Username == request.Username))
                return BadRequest("Username sudah terdaftar.");

            // --- PERBAIKAN: Cari Role ID ---
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == request.Role);
            if (role == null) return BadRequest($"Role '{request.Role}' tidak ditemukan di database.");
            // ------------------------------

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                RoleId = role.Id, // Pakai ID
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password) 
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("User tidak ditemukan");

            user.Username = request.Username;
            user.Email = request.Email;
            
            // --- PERBAIKAN: Update Role ---
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == request.Role);
            if (role != null)
            {
                user.RoleId = role.Id;
            }
            // -----------------------------

            if (!string.IsNullOrEmpty(request.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Data user berhasil diupdate" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("User tidak ditemukan");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User berhasil dihapus" });
        }
    }
}