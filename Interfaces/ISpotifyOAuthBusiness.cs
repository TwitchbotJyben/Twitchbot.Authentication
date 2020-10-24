using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Twitchbot.Common.Base.Models;
using Twitchbot.Services.Authentication.ModelsIn;

namespace Twitchbot.Services.Authentication.Interfaces
{
    public interface ISpotifyOAuthBusiness
    {
        Task<ActionResult<HttpResultModel<SpotifyModel>>> GetOAuth(string clientId, CancellationToken cancellationToken);
        Task<HttpResultModel<AuthenticationModel>> PostOAuth(string code, string clientId, CancellationToken cancellationToken);
    }
}