namespace Twitchbot.Authentication.ModelsIn
{
    public class AuthenticationModel
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string ClientId { get; set; }
        public string UserName { get; set; }
    }
}