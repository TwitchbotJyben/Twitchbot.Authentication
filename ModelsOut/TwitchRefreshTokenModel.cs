using System.Runtime.Serialization;

namespace Twitchbot.Authentication.ModelsOut
{
    [DataContract]
    public class TwitchRefreshTokenModel
    {
        [DataMember(Name = "access_token")]
        public string AccessToken { get; set; }

        [DataMember(Name = "refresh_token")]
        public string RefreshToken { get; set; }
    }
}