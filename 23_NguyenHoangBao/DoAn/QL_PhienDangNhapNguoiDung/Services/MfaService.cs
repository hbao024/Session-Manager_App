using System;
using System.Security.Cryptography;
using StackExchange.Redis;

namespace QL_PhienDangNhapNguoiDung.Services
{
    /// <summary>
    /// Minimal RFC 6238-compatible TOTP service for administrator MFA.
    /// The secret is kept in the configured Redis database for this demo project.
    /// </summary>
    public sealed class MfaService
    {
        private const int TimeStepSeconds = 30;
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        private readonly IDatabase _cache;

        public MfaService()
        {
            _cache = RedisHelper.Instance.Db;
        }

        public string GetOrCreateSecret(int userId)
        {
            string key = SecretKey(userId);
            RedisValue current = _cache.StringGet(key);
            if (current.HasValue && !string.IsNullOrWhiteSpace(current.ToString()))
                return current.ToString();

            byte[] bytes = new byte[20];
            using (var random = RandomNumberGenerator.Create())
                random.GetBytes(bytes);

            string secret = Base32Encode(bytes);
            _cache.StringSet(key, secret);
            return secret;
        }

        public bool IsEnabled(int userId)
        {
            return _cache.StringGet(EnabledKey(userId)) == "1" &&
                   _cache.KeyExists(SecretKey(userId));
        }

        public bool Enable(int userId, string code)
        {
            string secret = GetOrCreateSecret(userId);
            if (!VerifyCode(secret, code)) return false;

            _cache.StringSet(EnabledKey(userId), "1");
            return true;
        }

        public void Disable(int userId)
        {
            _cache.KeyDelete(EnabledKey(userId));
        }

        public bool VerifyCode(int userId, string code)
        {
            RedisValue secret = _cache.StringGet(SecretKey(userId));
            return secret.HasValue && VerifyCode(secret.ToString(), code);
        }

        public string GetCurrentCodeForDemo(int userId)
        {
            RedisValue secret = _cache.StringGet(SecretKey(userId));
            return secret.HasValue ? GenerateCode(secret.ToString(), GetCounter()) : string.Empty;
        }

        private static bool VerifyCode(string secret, string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            string normalized = code.Trim();
            long counter = GetCounter();

            for (long offset = -1; offset <= 1; offset++)
            {
                if (GenerateCode(secret, counter + offset) == normalized)
                    return true;
            }
            return false;
        }

        private static string GenerateCode(string secret, long counter)
        {
            byte[] key = Base32Decode(secret);
            byte[] message = BitConverter.GetBytes(counter);
            if (BitConverter.IsLittleEndian) Array.Reverse(message);

            using (var hmac = new HMACSHA1(key))
            {
                byte[] hash = hmac.ComputeHash(message);
                int offset = hash[hash.Length - 1] & 0x0F;
                int binary = ((hash[offset] & 0x7F) << 24)
                           | ((hash[offset + 1] & 0xFF) << 16)
                           | ((hash[offset + 2] & 0xFF) << 8)
                           | (hash[offset + 3] & 0xFF);
                return (binary % 1000000).ToString("D6");
            }
        }

        private static long GetCounter()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() / TimeStepSeconds;
        }

        private static string Base32Encode(byte[] bytes)
        {
            int buffer = 0;
            int bitsLeft = 0;
            var result = new System.Text.StringBuilder();

            foreach (byte value in bytes)
            {
                buffer = (buffer << 8) | value;
                bitsLeft += 8;
                while (bitsLeft >= 5)
                {
                    bitsLeft -= 5;
                    result.Append(Alphabet[(buffer >> bitsLeft) & 31]);
                }
                buffer &= bitsLeft == 0 ? 0 : (1 << bitsLeft) - 1;
            }

            if (bitsLeft > 0)
                result.Append(Alphabet[(buffer << (5 - bitsLeft)) & 31]);
            return result.ToString();
        }

        private static byte[] Base32Decode(string value)
        {
            string normalized = value.Trim().ToUpperInvariant().TrimEnd('=');
            var result = new System.Collections.Generic.List<byte>();
            int buffer = 0;
            int bitsLeft = 0;

            foreach (char character in normalized)
            {
                int index = Alphabet.IndexOf(character);
                if (index < 0) continue;
                buffer = (buffer << 5) | index;
                bitsLeft += 5;
                if (bitsLeft >= 8)
                {
                    bitsLeft -= 8;
                    result.Add((byte)((buffer >> bitsLeft) & 0xFF));
                    buffer &= bitsLeft == 0 ? 0 : (1 << bitsLeft) - 1;
                }
            }
            return result.ToArray();
        }

        private static string SecretKey(int userId) => $"mfa:user:{userId}:secret";
        private static string EnabledKey(int userId) => $"mfa:user:{userId}:enabled";
    }
}
