using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Twitchbot.Common.Base.Models;
using Twitchbot.Services.Authentication.Interfaces;
using Twitchbot.Services.Authentication.ModelsIn;

namespace Twitchbot.Services.Authentication.Controllers
{
    [Route("api/Authentication")]
    public class SpotifyOAuthController : ControllerBase
    {
        private readonly ILogger<SpotifyOAuthController> _logger;
        private readonly ISpotifyOAuthBusiness _oAuthBusiness;

        public SpotifyOAuthController(ILogger<SpotifyOAuthController> logger, ISpotifyOAuthBusiness oAuthBusiness)
        {
            _logger = logger;
            _oAuthBusiness = oAuthBusiness;
        }

        [HttpPost("/spotify/oauth")]
        public async Task<ActionResult<HttpResultModel<AuthenticationModel>>> Post(string code, string clientId, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Post oauth");

            return await _oAuthBusiness.PostOAuth(code, clientId, cancellationToken).ConfigureAwait(false);
        }

        [HttpGet("/spotify/oauth")]
        public async Task<ActionResult<HttpResultModel<SpotifyModel>>> Get(string clientId, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Get oauth");

            return await _oAuthBusiness.GetOAuth(clientId, cancellationToken).ConfigureAwait(false);
        }
    }
}