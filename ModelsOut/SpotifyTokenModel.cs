using System;
using System.Runtime.Serialization;

namespace Twitchbot.Services.Authentication.ModelsOut
{
    [DataContract]
    public class SpotifyTokenModel
    {
        [DataMember(Name = "grant_type")]
        public string GrantType { get; set; }

        [DataMember(Name = "code")]
        public string Code { get; set; }

        [DataMember(Name = "redirect_uri")]
        public Uri RedirectUri { get; set; }

        [DataMember(Name = "client_id")]
        public string ClientId { get; set; }

        [DataMember(Name = "client_secret")]
        public string ClientSecret { get; set; }
    }
}