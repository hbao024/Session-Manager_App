using System;
using System.Security.Cryptography;
using System.Text;

namespace QL_PhienDangNhapNguoiDung.Helpers
{
    /// <summary>
    /// Helper mã hóa mật khẩu bằng SHA256
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// Băm mật khẩu bằng SHA256 — trả về chuỗi hex lowercase
        /// </summary>
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        /// <summary>
        /// So sánh mật khẩu nhập vào với hash đã lưu
        /// </summary>
        public static bool Verify(string password, string hash)
        {
            return HashPassword(password) == hash.ToLower();
        }
    }
}
