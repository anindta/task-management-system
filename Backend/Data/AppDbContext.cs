using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Models;

namespace TaskManagerAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        
        // --- TAMBAHAN BARU ---
        public DbSet<Role> Roles { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<RoleMenu> RoleMenus { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seeding Data (Data Awal)
            
            // 1. Daftar Menu yang tersedia di sistem
            modelBuilder.Entity<Menu>().HasData(
                new Menu { Id = 1, Name = "dashboard", Label = "Dashboard" },
                new Menu { Id = 2, Name = "kanban", Label = "Kanban Board" },
                new Menu { Id = 3, Name = "users", Label = "User Management" },
                new Menu { Id = 4, Name = "roles", Label = "Role & Permission" }
            );

            // 2. Daftar Role Awal
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "ProjectManager" },
                new Role { Id = 3, Name = "Employee" }
            );

            // 3. Mapping: Admin boleh akses SEMUA (1,2,3,4)
            modelBuilder.Entity<RoleMenu>().HasData(
                new RoleMenu { Id = 1, RoleId = 1, MenuId = 1 },
                new RoleMenu { Id = 2, RoleId = 1, MenuId = 2 },
                new RoleMenu { Id = 3, RoleId = 1, MenuId = 3 },
                new RoleMenu { Id = 4, RoleId = 1, MenuId = 4 }
            );

            // 4. Mapping: Employee cuma boleh akses Kanban (2)
            modelBuilder.Entity<RoleMenu>().HasData(
                new RoleMenu { Id = 5, RoleId = 3, MenuId = 2 }
            );
        }
    }
}