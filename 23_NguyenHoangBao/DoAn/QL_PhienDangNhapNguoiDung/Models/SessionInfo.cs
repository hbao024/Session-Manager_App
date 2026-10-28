using System;

namespace QL_PhienDangNhapNguoiDung.Models
{
    /// <summary>
    /// Model chứa thông tin một phiên đăng nhập đang hoạt động (đọc từ Redis)
    /// </summary>
    public class SessionInfo
    {
        public string SessionId { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime LoginTime { get; set; }
        public DateTime LastActivity { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; } = "Low";
        public long? TtlSeconds { get; set; }  // Thời gian còn lại (giây)
    }
}
