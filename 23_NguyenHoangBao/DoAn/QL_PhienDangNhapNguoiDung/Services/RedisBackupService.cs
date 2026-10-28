using System;
using System.IO;
using System.Text;
using StackExchange.Redis;

namespace QL_PhienDangNhapNguoiDung.Services
{
    /// <summary>
    /// Export/import logical Redis data của ứng dụng.
    /// Mỗi dòng lưu key và payload DUMP ở dạng Base64, kèm TTL còn lại.
    /// Cách này giữ được nhiều kiểu dữ liệu Redis (Hash, Set, List, String...)
    /// mà không phụ thuộc vào redis-cli trên máy chạy ứng dụng.
    /// </summary>
    public class RedisBackupService
    {
        private const string Header = "# Redis Session Manager backup v1";
        private readonly IDatabase _database;

        public RedisBackupService()
        {
            _database = RedisHelper.Instance.Db;
        }

        /// <summary>
        /// Export các key trong database hiện tại ra file text an toàn để lưu trữ.
        /// Không dùng KEYS trong ứng dụng; IServer.Keys sử dụng SCAN khi server hỗ trợ.
        /// </summary>
        public int Export(string filePath, string pattern = "*")
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Đường dẫn file backup không hợp lệ.", nameof(filePath));

            var multiplexer = _database.Multiplexer;
            var endpoint = multiplexer.GetEndPoints()[0];
            var server = multiplexer.GetServer(endpoint);
            int count = 0;

            using (var writer = new StreamWriter(filePath, false, new UTF8Encoding(false)))
            {
                writer.WriteLine(Header);
                writer.WriteLine("# keyBase64|ttlMilliseconds (-1 = persistent)|dumpBase64");

                foreach (RedisKey key in server.Keys(database: _database.Database, pattern: pattern))
                {
                    byte[]? dump = _database.KeyDump(key);
                    if (dump == null || dump.Length == 0)
                        continue;

                    TimeSpan? ttl = _database.KeyTimeToLive(key);
                    long ttlMilliseconds = ttl.HasValue
                        ? Math.Max(1L, (long)ttl.Value.TotalMilliseconds)
                        : -1L;

                    string encodedKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(key.ToString()));
                    string encodedDump = Convert.ToBase64String(dump);
                    writer.WriteLine($"{encodedKey}|{ttlMilliseconds}|{encodedDump}");
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Import backup và thay thế các key trùng tên; các key khác trong database
        /// được giữ nguyên. Chỉ nên import file do chính Export tạo ra.
        /// </summary>
        public int Import(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Không tìm thấy file backup Redis.", filePath);

            int count = 0;
            using (var reader = new StreamReader(filePath, Encoding.UTF8, true))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    string[] parts = line.Split('|');
                    if (parts.Length != 3)
                        throw new InvalidDataException("File backup không đúng định dạng v1.");

                    string key = Encoding.UTF8.GetString(Convert.FromBase64String(parts[0]));
                    if (!long.TryParse(parts[1], out long ttlMilliseconds))
                        throw new InvalidDataException("TTL trong file backup không hợp lệ.");

                    byte[] dump = Convert.FromBase64String(parts[2]);
                    _database.KeyDelete(key);

                    TimeSpan expiry = ttlMilliseconds < 0
                        ? TimeSpan.Zero
                        : TimeSpan.FromMilliseconds(Math.Max(1L, ttlMilliseconds));
                    _database.KeyRestore(key, dump, expiry);
                    count++;
                }
            }

            return count;
        }
    }
}
