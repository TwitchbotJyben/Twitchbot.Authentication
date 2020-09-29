using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Twitchbot.Services.Authentication.ModelsOut
{
    [DataContract]
    public class TwitchOAuthModel
    {
        [DataMember(Name = "access_token")]
        public string AccessToken { get; set; }

        [DataMember(Name = "refresh_token")]
        public string RefreshToken { get; set; }

        [DataMember(Name = "expires_in")]
        public int ExpiresIn { get; set; }

        [DataMember(Name = "scope")]
        public IList<object> Scope { get; set; }

        [DataMember(Name = "token_type")]
        public string TokenType { get; set; }
    }
}