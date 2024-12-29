using System;

namespace CryptoTrader.Common.Models
{
    public class UserSession
    {
        // Kullanıcı bilgileri
        public string Username { get; private set; }
        public DateTime LoginTime { get; private set; }
        public string TimeZone { get; private set; } = "UTC";

        // Singleton instance
        private static UserSession _current;
        public static UserSession Current
        {
            get
            {
                if (_current == null)
                    throw new InvalidOperationException("Session not initialized");
                return _current;
            }
        }

        // Session başlatma
        public static void Initialize(string username)
        {
            _current = new UserSession
            {
                Username = username,
                LoginTime = DateTime.UtcNow
            };
        }

        // Kullanıcı bilgilerini alma
        public string GetUserInfo()
        {
            return $"Current Date and Time (UTC): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}\n" +
                   $"Current User's Login: {Username}";
        }

        // Session kontrol
        public bool IsSessionValid()
        {
            return (DateTime.UtcNow - LoginTime).TotalHours < 24;
        }
    }
}