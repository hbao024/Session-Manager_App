using System;
using StackExchange.Redis;
using QL_PhienDangNhapNguoiDung.Models;

namespace QL_PhienDangNhapNguoiDung.Services
{
    /// <summary>
    /// Calculates an explainable risk score for each login attempt.
    /// </summary>
    public sealed class RiskAssessmentService
    {
        private readonly IDatabase _cache;
        private readonly SessionManager _sessionManager;

        public RiskAssessmentService()
        {
            _cache = RedisHelper.Instance.Db;
            _sessionManager = new SessionManager();
        }

        public LoginRiskResult Assess(UserModel user, string ipAddress, string deviceId)
        {
            var result = new LoginRiskResult();
            string ipKey = $"user:{user.UserId}:known_ips";
            string deviceKey = $"user:{user.UserId}:known_devices";

            bool hasKnownIps = _cache.SetLength(ipKey) > 0;
            bool hasKnownDevices = _cache.SetLength(deviceKey) > 0;
            result.IsNewIp = hasKnownIps && !_cache.SetContains(ipKey, ipAddress);
            result.IsNewDevice = hasKnownDevices && !_cache.SetContains(deviceKey, deviceId);

            if (result.IsNewIp)
            {
                result.Score += 30;
                result.Reasons.Add("IP chưa từng sử dụng");
            }

            if (result.IsNewDevice)
            {
                result.Score += 20;
                result.Reasons.Add("Thiết bị chưa từng sử dụng");
            }

            int activeSessions = _sessionManager.GetUserActiveSessions(user.UserId).Count;
            if (activeSessions >= 2)
            {
                result.Score += 20;
                result.Reasons.Add("Có nhiều session đang hoạt động");
            }

            int failedAttempts = _sessionManager.GetFailedAttemptCount(user.Username);
            if (failedAttempts > 0)
            {
                int points = Math.Min(20, failedAttempts * 5);
                result.Score += points;
                result.Reasons.Add($"Có {failedAttempts} lần đăng nhập sai gần đây");
            }

            if (DateTime.Now.Hour < 6 || DateTime.Now.Hour >= 23)
            {
                result.Score += 10;
                result.Reasons.Add("Đăng nhập ngoài khung giờ thông thường");
            }

            result.Level = result.Score >= 60
                ? "High"
                : result.Score >= 30 ? "Medium" : "Low";
            result.RequiresMfa = result.Score >= 30 &&
                string.Equals(user.Role, "admin", StringComparison.OrdinalIgnoreCase);
            result.Blocked = result.Score >= 70;
            return result;
        }

        public void RememberLogin(UserModel user, string ipAddress, string deviceId)
        {
            _cache.SetAdd($"user:{user.UserId}:known_ips", ipAddress);
            _cache.SetAdd($"user:{user.UserId}:known_devices", deviceId);
            _cache.KeyExpire($"user:{user.UserId}:known_ips", TimeSpan.FromDays(180));
            _cache.KeyExpire($"user:{user.UserId}:known_devices", TimeSpan.FromDays(180));
        }
    }
}
