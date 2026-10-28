using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using QL_PhienDangNhapNguoiDung.Helpers;
using QL_PhienDangNhapNguoiDung.Models;

namespace QL_PhienDangNhapNguoiDung.Services
{
    /// <summary>
    /// Xử lý xác thực người dùng và quản lý tài khoản từ SQL Server
    /// </summary>
    public class UserService
    {
        private readonly string _connStr;

        public UserService(string connectionString)
        {
            _connStr = connectionString;
        }

        /// <summary>
        /// Xác thực đăng nhập — trả về UserModel nếu đúng, null nếu sai
        /// </summary>
        public UserModel? Authenticate(string username, string password)
        {
            string hash = PasswordHelper.HashPassword(password);
            string query = @"SELECT user_id, username, email, full_name, role, is_active, created_at, last_login
                             FROM USERS
                             WHERE username = @username AND password_hash = @hash";

            using SqlConnection cn = new SqlConnection(_connStr);
            cn.Open();
            using SqlCommand cmd = new SqlCommand(query, cn);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@hash", hash);

            using SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new UserModel
                {
                    UserId   = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Email    = reader.GetString(2),
                    FullName = reader.GetString(3),
                    Role     = reader.GetString(4),
                    IsActive = reader.GetBoolean(5),
                    CreatedAt = reader.GetDateTime(6),
                    LastLogin = reader.IsDBNull(7) ? null : reader.GetDateTime(7)
                };
            }
            return null;
        }

        /// <summary>
        /// Lấy danh sách tất cả người dùng (cho admin)
        /// </summary>
        public List<UserModel> GetAllUsers()
        {
            var list = new List<UserModel>();
            string query = @"SELECT user_id, username, email, full_name, role, is_active, created_at, last_login
                             FROM USERS ORDER BY created_at DESC";

            using SqlConnection cn = new SqlConnection(_connStr);
            cn.Open();
            using SqlCommand cmd = new SqlCommand(query, cn);
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new UserModel
                {
                    UserId    = reader.GetInt32(0),
                    Username  = reader.GetString(1),
                    Email     = reader.GetString(2),
                    FullName  = reader.GetString(3),
                    Role      = reader.GetString(4),
                    IsActive  = reader.GetBoolean(5),
                    CreatedAt = reader.GetDateTime(6),
                    LastLogin = reader.IsDBNull(7) ? null : reader.GetDateTime(7)
                });
            }
            return list;
        }

        /// <summary>
        /// Lấy thông tin user theo ID
        /// </summary>
        public UserModel? GetUserById(int userId)
        {
            string query = @"SELECT user_id, username, email, full_name, role, is_active, created_at, last_login
                             FROM USERS WHERE user_id = @id";
            using SqlConnection cn = new SqlConnection(_connStr);
            cn.Open();
            using SqlCommand cmd = new SqlCommand(query, cn);
            cmd.Parameters.AddWithValue("@id", userId);
            using SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new UserModel
                {
                    UserId    = reader.GetInt32(0),
                    Username  = reader.GetString(1),
                    Email     = reader.GetString(2),
                    FullName  = reader.GetString(3),
                    Role      = reader.GetString(4),
                    IsActive  = reader.GetBoolean(5),
                    CreatedAt = reader.GetDateTime(6),
                    LastLogin = reader.IsDBNull(7) ? null : reader.GetDateTime(7)
                };
            }
            return null;
        }

        /// <summary>
        /// Cập nhật last_login sau khi đăng nhập thành công
        /// </summary>
        public void UpdateLastLogin(int userId)
        {
            using SqlConnection cn = new SqlConnection(_connStr);
            cn.Open();
            using SqlCommand cmd = new SqlCommand(
                "UPDATE USERS SET last_login = GETDATE() WHERE user_id = @id", cn);
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Kích hoạt / vô hiệu hóa tài khoản (admin)
        /// </summary>
        public void SetUserActive(int userId, bool isActive)
        {
            using SqlConnection cn = new SqlConnection(_connStr);
            cn.Open();
            using SqlCommand cmd = new SqlCommand(
                "UPDATE USERS SET is_active = @active WHERE user_id = @id", cn);
            cmd.Parameters.AddWithValue("@active", isActive ? 1 : 0);
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Thêm tài khoản mới (admin)
        /// </summary>
        public void AddUser(string username, string password, string email, string fullName, string role)
        {
            string hash = PasswordHelper.HashPassword(password);
            string query = @"INSERT INTO USERS (username, password_hash, email, full_name, role)
                             VALUES (@username, @hash, @email, @fullName, @role)";
            using SqlConnection cn = new SqlConnection(_connStr);
            cn.Open();
            using SqlCommand cmd = new SqlCommand(query, cn);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@hash", hash);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@fullName", fullName);
            cmd.Parameters.AddWithValue("@role", role);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Cập nhật thông tin tài khoản. Để trống mật khẩu nếu không muốn đổi.
        /// </summary>
        public void UpdateUser(int userId, string email, string fullName, string role, string? newPassword = null)
        {
            string query = string.IsNullOrWhiteSpace(newPassword)
                ? @"UPDATE USERS
                   SET email = @email, full_name = @fullName, role = @role
                   WHERE user_id = @id"
                : @"UPDATE USERS
                   SET email = @email, full_name = @fullName, role = @role,
                       password_hash = @hash
                   WHERE user_id = @id";

            using SqlConnection cn = new SqlConnection(_connStr);
            cn.Open();
            using SqlCommand cmd = new SqlCommand(query, cn);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@fullName", fullName);
            cmd.Parameters.AddWithValue("@role", role);
            cmd.Parameters.AddWithValue("@id", userId);
            if (!string.IsNullOrWhiteSpace(newPassword))
                cmd.Parameters.AddWithValue("@hash", PasswordHelper.HashPassword(newPassword!));
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Xóa tài khoản và lịch sử đăng nhập liên quan trong một transaction.
        /// </summary>
        public void DeleteUser(int userId)
        {
            using SqlConnection cn = new SqlConnection(_connStr);
            cn.Open();
            using SqlTransaction tx = cn.BeginTransaction();
            try
            {
                using (SqlCommand history = new SqlCommand(
                    "DELETE FROM LOGIN_HISTORY WHERE user_id = @id", cn, tx))
                {
                    history.Parameters.AddWithValue("@id", userId);
                    history.ExecuteNonQuery();
                }

                using (SqlCommand user = new SqlCommand(
                    "DELETE FROM USERS WHERE user_id = @id", cn, tx))
                {
                    user.Parameters.AddWithValue("@id", userId);
                    if (user.ExecuteNonQuery() == 0)
                        throw new InvalidOperationException("Không tìm thấy tài khoản cần xóa.");
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        public void ChangePassword(int userId, string newPassword)
        {
            string hash = PasswordHelper.HashPassword(newPassword);
            using SqlConnection cn = new SqlConnection(_connStr);
            cn.Open();
            using SqlCommand cmd = new SqlCommand(
                "UPDATE USERS SET password_hash = @hash WHERE user_id = @id", cn);
            cmd.Parameters.AddWithValue("@hash", hash);
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.ExecuteNonQuery();
        }
    }
}
