using System;

namespace QL_PhienDangNhapNguoiDung.Models
{
    /// <summary>
    /// Model chứa thông tin tài khoản người dùng (đọc từ SQL Server)
    /// </summary>
    public class UserModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}
