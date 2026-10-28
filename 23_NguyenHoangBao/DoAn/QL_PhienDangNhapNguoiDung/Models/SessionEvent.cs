using System;

namespace QL_PhienDangNhapNguoiDung.Models
{
    public sealed class SessionEvent
    {
        public string EventType { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
