namespace TaskManagerAPI.DTOs
{
    // Untuk Create User baru oleh Admin
    public class CreateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Employee";
    }

    // Untuk Update User (Password opsional, kalau kosong berarti gak diganti)
    public class UpdateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; } // Boleh null
        public string Role { get; set; } = string.Empty;
    }
}