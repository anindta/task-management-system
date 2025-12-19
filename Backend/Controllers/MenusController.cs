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
    [Authorize(Roles = "Admin")] // WAJIB ADMIN
    public class MenusController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MenusController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GET: api/Menus (Lihat semua permission yang tersedia)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Menu>>> GetMenus()
        {
            return await _context.Menus.ToListAsync();
        }

        // 2. POST: api/Menus (Tambah permission baru)
        [HttpPost]
        public async Task<ActionResult<Menu>> CreateMenu(MenuDto request)
        {
            // Validasi: Kode Name tidak boleh kembar
            if (_context.Menus.Any(m => m.Name == request.Name))
                return BadRequest($"Menu dengan kode '{request.Name}' sudah ada.");

            var menu = new Menu
            {
                Name = request.Name,   // Contoh: "view_reports"
                Label = request.Label,  // Contoh: "Laporan"
                Icon = request.Icon  // Contoh: "ri-file-chart-line"
            };

            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Permission/Menu berhasil dibuat", data = menu });
        }

        // 3. PUT: api/Menus/5 (Edit Label/Nama)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMenu(int id, MenuDto request)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (menu == null) return NotFound("Menu tidak ditemukan");

            menu.Name = request.Name;
            menu.Label = request.Label;
            menu.Icon = request.Icon;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Menu berhasil diupdate" });
        }

        // 4. DELETE: api/Menus/5 (Hapus Permission)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenu(int id)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (menu == null) return NotFound("Menu tidak ditemukan");

            // VALIDASI PENTING: 
            // Jangan hapus menu kalau sudah dipakai di Role apapun!
            bool isUsed = await _context.RoleMenus.AnyAsync(rm => rm.MenuId == id);
            if (isUsed)
            {
                return BadRequest("Gagal hapus: Permission ini sedang digunakan oleh Role tertentu. Hapus centang di Role dulu.");
            }

            _context.Menus.Remove(menu);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Menu berhasil dihapus" });
        }
    }
}