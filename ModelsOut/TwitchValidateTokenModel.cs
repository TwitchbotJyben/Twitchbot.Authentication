using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Twitchbot.Authentication.ModelsOut
{
    [DataContract]
    public class TwitchValidateTokenModel
    {
        [DataMember(Name = "client_id")]
        public string ClientId { get; set; }

        [DataMember(Name = "login")]
        public string Login { get; set; }

        [DataMember(Name = "scopes")]
        public IList<object> Scopes { get; set; }

        [DataMember(Name = "user_id")]
        public string UserId { get; set; }

        [DataMember(Name = "expires_in")]
        public int ExpiresIn { get; set; }
    }
}