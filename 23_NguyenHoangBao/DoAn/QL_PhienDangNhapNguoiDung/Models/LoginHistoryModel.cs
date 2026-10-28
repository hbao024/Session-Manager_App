
using System;

namespace QL_PhienDangNhapNguoiDung.Models
{

    /// <summary>
    /// Model lịch sử đăng nhập (đọc từ SQL Server)
    /// </summary>
    public class LoginHistoryModel
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? SessionId { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string? IpAddress { get; set; }
        public string Status { get; set; } = string.Empty;   // success | failed | locked
        public string? Note { get; set; }

        // Tính thời gian online
        public string Duration
        {
            get
            {
                if (LogoutTime == null) return "Đang online";
                var span = LogoutTime.Value - LoginTime;
                return $"{(int)span.TotalMinutes} phút";
            }
        }
    }
}
