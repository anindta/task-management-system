using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTOs;
using TaskManagerAPI.Models;

namespace TaskManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(RegisterDto request)
        {
            if (_context.Users.Any(u => u.Username == request.Username))
            {
                return BadRequest("Username sudah terdaftar.");
            }

            // Cari Role
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == request.Role);
            if (role == null)
            {
                // Fallback ke Employee jika role tidak ditemukan
                role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Employee");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Password = passwordHash,
                RoleId = role?.Id ?? 3 // Default ID 3 jika null
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registrasi berhasil!" });
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<ActionResult<object>> Login(LoginDto request)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null) return BadRequest("User tidak ditemukan.");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return BadRequest("Password salah.");
            }

            string token = CreateToken(user);

            // Return token lengkap
            return Ok(new { 
                token = token, 
                role = user.Role?.Name, 
                userId = user.Id,
                username = user.Username 
            });
        }

        // GET: api/Auth/my-menus
        [HttpGet("my-menus")]
        [Authorize]
        public async Task<ActionResult> GetMyMenus()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null) return Unauthorized();

            var userId = int.Parse(userIdStr);

            var user = await _context.Users
                .Include(u => u.Role)
                    .ThenInclude(r => r.RoleMenus) // Lewat tabel junction
                        .ThenInclude(rm => rm.Menu)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return Unauthorized();
            if (user.Role == null) return Ok(new List<string>());

            // Kita ambil Icon juga disini
            var menus = user.Role.RoleMenus.Select(rm => new
            {
                rm.Menu.Id,
                rm.Menu.Name,
                rm.Menu.Label,
                rm.Menu.Icon // Tambahan Icon
            }).ToList();

            return Ok(menus);
        }

        // --- FITUR BARU: UPDATE PROFILE ---
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null) return Unauthorized();

            var userId = int.Parse(userIdStr);
            
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.Username = request.Username;
            user.Email = request.Email;

            await _context.SaveChangesAsync();
            
            // Perbaikan syntax agar tidak error koma
            return Ok(new { 
                message = "Profil berhasil diperbarui", 
                username = user.Username, 
                email = user.Email 
            });
        }

        // --- FITUR BARU: GANTI PASSWORD ---
        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null) return Unauthorized();

            var userId = int.Parse(userIdStr);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // Cek Password Lama
            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password))
            {
                return BadRequest("Password lama salah.");
            }

            // Hash & Simpan Password Baru
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password berhasil diubah." });
        }

        // Fungsi Helper Token
        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "Employee")
            };

            // Menggunakan JwtSettings:Key sesuai kode aslimu
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("JwtSettings:Key").Value!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}