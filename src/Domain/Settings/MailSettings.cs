﻿#nullable disable

namespace Domain.Settings
{
    public class MailSettings
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public string EmailAddress { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }
}
