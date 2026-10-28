using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;
using QL_PhienDangNhapNguoiDung.Helpers;
using QL_PhienDangNhapNguoiDung.Models;

namespace QL_PhienDangNhapNguoiDung.Services
{
    /// <summary>
    /// Quản lý toàn bộ vòng đời phiên đăng nhập trong Redis
    /// </summary>
    public class SessionManager
    {
        private readonly IDatabase _cache;
        private readonly SessionEventService _eventService;

        // Thời gian sống của phiên: 30 phút không hoạt động → hết hạn
        private static readonly TimeSpan SESSION_TTL = TimeSpan.FromMinutes(30);
        // Thời gian khóa tài khoản khi đăng nhập sai quá nhiều lần
        private static readonly TimeSpan LOCK_TTL = TimeSpan.FromMinutes(15);
        // Ngưỡng số lần đăng nhập sai trước khi bị khóa
        private const int MAX_FAILED_ATTEMPTS = 5;
        // Giữ tối đa 20 mục log gần nhất trong Redis List
        private const int MAX_LOGIN_LOG = 20;

        public SessionManager()
        {
            _cache = RedisHelper.Instance.Db;
            _eventService = new SessionEventService();
        }

        // ─────────────────────────────────────────────────────────
        //  TẠO PHIÊN MỚI
        // ─────────────────────────────────────────────────────────

        /// <summary>
        /// Tạo phiên đăng nhập mới, lưu vào Redis Hash + Set
        /// </summary>
        public string CreateSession(
            UserModel user,
            string ipAddress,
            LoginRiskResult? risk = null,
            string? deviceId = null)
        {
            string sessionId = Guid.NewGuid().ToString();
            string sessionKey = $"session:{sessionId}";
            string userSessionsKey = $"user:{user.UserId}:sessions";
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Lưu thông tin phiên vào Redis Hash
            _cache.HashSet(sessionKey, new HashEntry[]
            {
                new HashEntry("user_id",       user.UserId.ToString()),
                new HashEntry("username",      user.Username),
                new HashEntry("full_name",     user.FullName),
                new HashEntry("email",         user.Email),
                new HashEntry("role",          user.Role),
                new HashEntry("login_time",    now),
                new HashEntry("last_activity", now),
                new HashEntry("ip_address",    ipAddress),
                new HashEntry("device_id",     deviceId ?? string.Empty),
                new HashEntry("risk_score",    (risk?.Score ?? 0).ToString()),
                new HashEntry("risk_level",    risk?.Level ?? "Low")
            });

            // Đặt TTL cho phiên (30 phút)
            _cache.KeyExpire(sessionKey, SESSION_TTL);

            // Thêm sessionId vào Set của user (quản lý đa thiết bị)
            _cache.SetAdd(userSessionsKey, sessionId);

            // Ghi log đăng nhập vào Redis List
            string logEntry = $"[{now}] IP:{ipAddress} - LOGIN SUCCESS";
            _cache.ListLeftPush($"user:{user.UserId}:login_log", logEntry);
            _cache.ListTrim($"user:{user.UserId}:login_log", 0, MAX_LOGIN_LOG - 1);
            _cache.KeyExpire($"user:{user.UserId}:login_log", TimeSpan.FromDays(7));

            _eventService.Publish(new SessionEvent
            {
                EventType = "LOGIN_SUCCESS",
                UserId = user.UserId,
                Username = user.Username,
                SessionId = sessionId,
                IpAddress = ipAddress,
                RiskScore = risk?.Score ?? 0,
                RiskLevel = risk?.Level ?? "Low",
                Reason = risk?.ReasonText ?? string.Empty
            });

            return sessionId;
        }

        // ─────────────────────────────────────────────────────────
        //  XÁC THỰC PHIÊN
        // ─────────────────────────────────────────────────────────

        /// <summary>
        /// Kiểm tra phiên còn hợp lệ không và gia hạn TTL nếu còn
        /// </summary>
        public SessionInfo? ValidateAndRefresh(string sessionId)
        {
            string key = $"session:{sessionId}";

            if (!_cache.KeyExists(key))
                return null;

            // Gia hạn TTL (rolling expiration)
            _cache.KeyExpire(key, SESSION_TTL);

            // Cập nhật last_activity
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _cache.HashSet(key, "last_activity", now);

            return GetSessionInfo(sessionId);
        }

        /// <summary>
        /// Đọc thông tin phiên từ Redis (không gia hạn TTL)
        /// </summary>
        public SessionInfo? GetSessionInfo(string sessionId)
        {
            string key = $"session:{sessionId}";
            var entries = _cache.HashGetAll(key);
            if (entries.Length == 0) return null;

            var dict = entries.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());
            var ttl = _cache.KeyTimeToLive(key);

            return new SessionInfo
            {
                SessionId    = sessionId,
                UserId       = int.Parse(dict.GetValueOrDefault("user_id", "0")),
                Username     = dict.GetValueOrDefault("username", ""),
                FullName     = dict.GetValueOrDefault("full_name", ""),
                Email        = dict.GetValueOrDefault("email", ""),
                Role         = dict.GetValueOrDefault("role", ""),
                LoginTime    = DateTime.Parse(dict.GetValueOrDefault("login_time", DateTime.Now.ToString())),
                LastActivity = DateTime.Parse(dict.GetValueOrDefault("last_activity", DateTime.Now.ToString())),
                IpAddress    = dict.GetValueOrDefault("ip_address", ""),
                DeviceId     = dict.GetValueOrDefault("device_id", ""),
                RiskScore    = int.Parse(dict.GetValueOrDefault("risk_score", "0")),
                RiskLevel    = dict.GetValueOrDefault("risk_level", "Low"),
                TtlSeconds   = (long?)ttl?.TotalSeconds
            };
        }

        // ─────────────────────────────────────────────────────────
        //  ĐĂNG XUẤT
        // ─────────────────────────────────────────────────────────

        /// <summary>
        /// Đăng xuất một phiên cụ thể
        /// </summary>
        public bool Logout(string sessionId)
        {
            var info = GetSessionInfo(sessionId);
            if (info == null) return false;

            _eventService.Publish(new SessionEvent
            {
                EventType = "LOGOUT",
                UserId = info.UserId,
                Username = info.Username,
                SessionId = sessionId,
                IpAddress = info.IpAddress
            });

            // Xóa Hash phiên
            _cache.KeyDelete($"session:{sessionId}");
            // Xóa khỏi Set của user
            _cache.SetRemove($"user:{info.UserId}:sessions", sessionId);

            return true;
        }

        /// <summary>
        /// Đăng xuất TẤT CẢ phiên của một user
        /// </summary>
        public int LogoutAll(int userId)
        {
            string userSessionsKey = $"user:{userId}:sessions";
            var sessionIds = _cache.SetMembers(userSessionsKey);
            int count = 0;

            foreach (var sid in sessionIds)
            {
                var info = GetSessionInfo(sid.ToString());
                if (info != null)
                {
                    _eventService.Publish(new SessionEvent
                    {
                        EventType = "FORCE_LOGOUT",
                        UserId = info.UserId,
                        Username = info.Username,
                        SessionId = sid.ToString(),
                        IpAddress = info.IpAddress
                    });
                }
                _cache.KeyDelete($"session:{sid}");
                count++;
            }
            _cache.KeyDelete(userSessionsKey);
            return count;
        }

        // ─────────────────────────────────────────────────────────
        //  QUẢN LÝ ĐA THIẾT BỊ
        // ─────────────────────────────────────────────────────────

        /// <summary>
        /// Lấy danh sách tất cả phiên đang hoạt động của một user
        /// </summary>
        public List<SessionInfo> GetUserActiveSessions(int userId)
        {
            string userSessionsKey = $"user:{userId}:sessions";
            var sessionIds = _cache.SetMembers(userSessionsKey);
            var result = new List<SessionInfo>();

            foreach (var sid in sessionIds)
            {
                var info = GetSessionInfo(sid.ToString());
                if (info != null)
                    result.Add(info);
                else
                    // Xóa session đã hết hạn khỏi Set
                    _cache.SetRemove(userSessionsKey, sid);
            }
            return result;
        }

        /// <summary>
        /// Lấy tổng số phiên đang online (tất cả user)
        /// </summary>
        public int GetTotalOnlineCount()
        {
            // Quét tất cả key "session:*" trong Redis
            var server = RedisHelper.Instance.Db.Multiplexer.GetServer(
                RedisHelper.Instance.Db.Multiplexer.GetEndPoints()[0]);
            return server.Keys(database: _cache.Database, pattern: "session:*").Count();
        }

        public int GetFailedAttemptCount(string username)
        {
            string? value = _cache.StringGet($"login_failed:{username}");
            return int.TryParse(value, out int count) ? count : 0;
        }

        // ─────────────────────────────────────────────────────────
        //  CHỐNG BRUTE-FORCE
        // ─────────────────────────────────────────────────────────

        /// <summary>
        /// Tăng số lần đăng nhập sai. Trả về số lần sai hiện tại.
        /// </summary>
        public long IncrementFailedAttempts(string username)
        {
            string key = $"login_failed:{username}";
            long count = _cache.StringIncrement(key);
            // Đặt TTL 15 phút kể từ lần sai đầu tiên
            if (count == 1)
                _cache.KeyExpire(key, LOCK_TTL);
            return count;
        }

        /// <summary>
        /// Kiểm tra tài khoản có bị khóa không (đăng nhập sai >= 5 lần)
        /// </summary>
        public bool IsLocked(string username)
        {
            string key = $"login_failed:{username}";
            string? val = _cache.StringGet(key);
            if (val == null) return false;
            return long.TryParse(val, out long count) && count >= MAX_FAILED_ATTEMPTS;
        }

        /// <summary>
        /// Lấy thời gian còn lại của lệnh khóa (giây)
        /// </summary>
        public long GetLockRemainingSeconds(string username)
        {
            var ttl = _cache.KeyTimeToLive($"login_failed:{username}");
            return (long)(ttl?.TotalSeconds ?? 0);
        }

        /// <summary>
        /// Xóa trạng thái đăng nhập sai sau khi đăng nhập thành công
        /// </summary>
        public void ClearFailedAttempts(string username)
        {
            _cache.KeyDelete($"login_failed:{username}");
        }

        // ─────────────────────────────────────────────────────────
        //  LỊCH SỬ ĐĂNG NHẬP (REDIS LIST)
        // ─────────────────────────────────────────────────────────

        /// <summary>
        /// Lấy log đăng nhập gần đây của user từ Redis
        /// </summary>
        public List<string> GetRecentLoginLog(int userId, int count = 10)
        {
            var entries = _cache.ListRange($"user:{userId}:login_log", 0, count - 1);
            return entries.Select(e => e.ToString()).ToList();
        }
    }
}
