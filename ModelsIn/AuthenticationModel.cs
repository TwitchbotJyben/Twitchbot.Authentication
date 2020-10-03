using System;

namespace Twitchbot.Services.Authentication.ModelsIn
{
    public class AuthenticationModel
    {
        public int Id { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string ClientId { get; set; }
        public string UserName { get; set; }
        public int ExpiresIn { get; set; }
        public DateTime Time { get; set; }
    }
}