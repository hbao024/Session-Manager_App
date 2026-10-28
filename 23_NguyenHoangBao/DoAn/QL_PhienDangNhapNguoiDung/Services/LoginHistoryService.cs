using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using QL_PhienDangNhapNguoiDung.Models;

namespace QL_PhienDangNhapNguoiDung.Services
{
    /// <summary>
    /// Quản lý lịch sử đăng nhập trong SQL Server
    /// </summary>
    public class LoginHistoryService
    {
        private readonly string _connStr;

        public LoginHistoryService(string connectionString)
        {
            _connStr = connectionString;
        }

        /// <summary>
        /// Ghi bản ghi đăng nhập mới, trả về ID vừa tạo
        /// </summary>
        public int LogLogin(int? userId, string username, string? sessionId, string? ip, string status, string? note = null)
        {
            string query = @"INSERT INTO LOGIN_HISTORY (user_id, username, session_id, ip_address, status, note)
                             OUTPUT INSERTED.id
                             VALUES (@userId, @username, @sessionId, @ip, @status, @note)";
            using SqlConnection cn = new SqlConnection(_connStr);
            cn.Open();
            using SqlCommand cmd = new SqlCommand(query, cn);
            cmd.Parameters.AddWithValue("@userId",    userId.HasValue ? userId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@username",  username);
            cmd.Parameters.AddWithValue("@sessionId", sessionId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ip",        ip ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@status",    status);
            cmd.Parameters.AddWithValue("@note",      note ?? (object)DBNull.Value);
            return (int)cmd.ExecuteScalar();
        }

        /// <summary>
        /// Cập nhật thời gian đăng xuất
        /// </summary>
        public void LogLogout(string sessionId)
        {
            using SqlConnection cn = new SqlConnection(_connStr);
            cn.Open();
            using SqlCommand cmd = new SqlCommand(
                "UPDATE LOGIN_HISTORY SET logout_time = GETDATE() WHERE session_id = @sid AND logout_time IS NULL", cn);
            cmd.Parameters.AddWithValue("@sid", sessionId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Cập nhật đăng xuất cho nhiều session cùng lúc (đăng xuất tất cả)
        /// </summary>
        public void LogLogoutMany(IEnumerable<string> sessionIds)
        {
            using SqlConnection cn = new SqlConnection(_connStr);
            cn.Open();
            foreach (var sid in sessionIds)
            {
                using SqlCommand cmd = new SqlCommand(
                    "UPDATE LOGIN_HISTORY SET logout_time = GETDATE() WHERE session_id = @sid AND logout_time IS NULL", cn);
                cmd.Parameters.AddWithValue("@sid", sid);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Lấy lịch sử đăng nhập của một user (mới nhất trước)
        /// </summary>
        public List<LoginHistoryModel> GetByUser(int userId, int top = 50)
        {
            var list = new List<LoginHistoryModel>();
            string query = $@"SELECT TOP {top} id, user_id, username, session_id, login_time, logout_time, ip_address, status, note
                              FROM LOGIN_HISTORY
                              WHERE user_id = @userId
                              ORDER BY login_time DESC";
            using SqlConnection cn = new SqlConnection(_connStr);
            cn.Open();
            using SqlCommand cmd = new SqlCommand(query, cn);
            cmd.Parameters.AddWithValue("@userId", userId);
            using SqlDataReader r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(MapReader(r));
            return list;
        }

        /// <summary>
        /// Lấy toàn bộ lịch sử đăng nhập (cho admin)
        /// </summary>
        public List<LoginHistoryModel> GetAll(int top = 200)
        {
            var list = new List<LoginHistoryModel>();
            string query = $@"SELECT TOP {top} id, user_id, username, session_id, login_time, logout_time, ip_address, status, note
                              FROM LOGIN_HISTORY ORDER BY login_time DESC";
            using SqlConnection cn = new SqlConnection(_connStr);
            cn.Open();
            using SqlCommand cmd = new SqlCommand(query, cn);
            using SqlDataReader r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(MapReader(r));
            return list;
        }

        private LoginHistoryModel MapReader(SqlDataReader r) => new LoginHistoryModel
        {
            Id         = r.GetInt32(0),
            UserId     = r.IsDBNull(1) ? null : r.GetInt32(1),
            Username   = r.GetString(2),
            SessionId  = r.IsDBNull(3) ? null : r.GetString(3),
            LoginTime  = r.GetDateTime(4),
            LogoutTime = r.IsDBNull(5) ? null : r.GetDateTime(5),
            IpAddress  = r.IsDBNull(6) ? null : r.GetString(6),
            Status     = r.GetString(7),
            Note       = r.IsDBNull(8) ? null : r.GetString(8)
        };
    }
}
