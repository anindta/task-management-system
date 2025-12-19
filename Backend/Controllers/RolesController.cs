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
    [Authorize(Roles = "Admin")] // HANYA ADMIN
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GET: api/Roles (Ambil semua role + menu aksesnya)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDetailDto>>> GetRoles()
        {
            var roles = await _context.Roles
                .Include(r => r.RoleMenus)
                .ThenInclude(rm => rm.Menu)
                .ToListAsync();

            // Mapping ke DTO biar rapi JSON-nya
            var result = roles.Select(r => new RoleDetailDto
            {
                Id = r.Id,
                Name = r.Name,
                MenuLabels = r.RoleMenus.Select(rm => rm.Menu!.Label).ToList(),
                MenuIds = r.RoleMenus.Select(rm => rm.MenuId).ToList()
            });

            return Ok(result);
        }

        // 2. GET: api/Roles/menus (Ambil daftar menu yg tersedia untuk pilihan Checkbox)
        [HttpGet("menus")]
        public async Task<ActionResult<IEnumerable<Menu>>> GetAvailableMenus()
        {
            return await _context.Menus.ToListAsync();
        }

        // 3. POST: api/Roles (Buat Role Baru)
        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleDto request)
        {
            if (_context.Roles.Any(r => r.Name == request.Name))
                return BadRequest("Nama role sudah ada.");

            // 1. Simpan Role
            var newRole = new Role { Name = request.Name };
            _context.Roles.Add(newRole);
            await _context.SaveChangesAsync(); // Save dulu biar dapet ID

            // 2. Simpan Permission (Looping MenuIds)
            if (request.MenuIds != null && request.MenuIds.Any())
            {
                foreach (var menuId in request.MenuIds)
                {
                    _context.RoleMenus.Add(new RoleMenu 
                    { 
                        RoleId = newRole.Id, 
                        MenuId = menuId 
                    });
                }
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Role berhasil dibuat", roleId = newRole.Id });
        }

        // 4. PUT: api/Roles/5 (Update Role & Permission)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, CreateRoleDto request)
        {
            var role = await _context.Roles
                .Include(r => r.RoleMenus)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null) return NotFound("Role tidak ditemukan");

            // Update Nama
            role.Name = request.Name;

            // Update Permission: Hapus semua akses lama, ganti yang baru
            _context.RoleMenus.RemoveRange(role.RoleMenus);
            
            if (request.MenuIds != null)
            {
                foreach (var menuId in request.MenuIds)
                {
                    _context.RoleMenus.Add(new RoleMenu 
                    { 
                        RoleId = role.Id, 
                        MenuId = menuId 
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Role & Permission berhasil diupdate" });
        }

        // 5. DELETE: api/Roles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            // Validasi: Jangan hapus Role yang sedang dipakai User!
            bool isUsed = await _context.Users.AnyAsync(u => u.RoleId == id);
            if (isUsed)
            {
                return BadRequest("Gagal hapus: Role ini sedang dipakai oleh User.");
            }

            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound();

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Role berhasil dihapus" });
        }
    }
}