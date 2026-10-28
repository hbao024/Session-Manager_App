using System;
using StackExchange.Redis;

namespace QL_PhienDangNhapNguoiDung.Services
{
    /// <summary>
    /// Quản lý kết nối Redis theo mẫu Singleton — chỉ tạo 1 kết nối duy nhất
    /// </summary>
    public sealed class RedisHelper
    {
        private static RedisHelper? _instance;
        private static readonly object _lock = new object();

        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        private RedisHelper(string connectionString = "127.0.0.1:6379")
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
            // Chỉ định rõ database để ứng dụng và Redis Insight luôn xem cùng một nơi.
            _db = _redis.GetDatabase(AppConfig.REDIS_DATABASE);
        }

        /// <summary>
        /// Lấy instance duy nhất (thread-safe)
        /// </summary>
        public static RedisHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new RedisHelper(AppConfig.REDIS_CONNECTION);
                    }
                }
                return _instance;
            }
        }

        public IDatabase Db => _db;

        /// <summary>
        /// Kiểm tra kết nối Redis có hoạt động không
        /// </summary>
        public bool IsConnected()
        {
            try
            {
                _db.Ping();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
