using System;
using System.Configuration;

namespace QL_PhienDangNhapNguoiDung.Services
{
    /// <summary>
    /// Central application configuration.
    /// SQL credentials are kept outside source code in App.config.
    /// Environment variables can override local configuration when deployed.
    /// </summary>
    public static class AppConfig
    {
        public static readonly string SQL_CONNECTION =
            ReadConnectionString("SqlConnection", "QL_SESSION_SQL_CONNECTION");

        public static readonly string REDIS_CONNECTION =
            ReadSetting("RedisConnection", "QL_SESSION_REDIS_CONNECTION", "127.0.0.1:6379");

        public static readonly int REDIS_DATABASE =
            ReadIntSetting("RedisDatabase", "QL_SESSION_REDIS_DATABASE", 4);

        public static string? CurrentSessionId { get; set; }

        private static string ReadConnectionString(string configName, string environmentName)
        {
            string? environmentValue = Environment.GetEnvironmentVariable(environmentName);
            if (!string.IsNullOrWhiteSpace(environmentValue))
                return environmentValue;

            string? configuredValue = ConfigurationManager.ConnectionStrings[configName]?.ConnectionString;
            if (!string.IsNullOrWhiteSpace(configuredValue))
                return configuredValue!;

            throw new ConfigurationErrorsException(
                $"Missing connection string '{configName}'. Configure App.config or {environmentName}.");
        }

        private static string ReadSetting(string configName, string environmentName, string fallback)
        {
            string? environmentValue = Environment.GetEnvironmentVariable(environmentName);
            if (!string.IsNullOrWhiteSpace(environmentValue))
                return environmentValue;

            return ConfigurationManager.AppSettings[configName] ?? fallback;
        }

        private static int ReadIntSetting(string configName, string environmentName, int fallback)
        {
            string value = ReadSetting(configName, environmentName, fallback.ToString());
            return int.TryParse(value, out int result) ? result : fallback;
        }
    }
}
