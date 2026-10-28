using System.Collections.Generic;

namespace QL_PhienDangNhapNguoiDung.Models
{
    public sealed class LoginRiskResult
    {
        public int Score { get; set; }
        public string Level { get; set; } = "Low";
        public bool RequiresMfa { get; set; }
        public bool Blocked { get; set; }
        public bool IsNewIp { get; set; }
        public bool IsNewDevice { get; set; }
        public List<string> Reasons { get; } = new List<string>();

        public string ReasonText => string.Join("; ", Reasons);
    }
}
