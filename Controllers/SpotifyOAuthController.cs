using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Twitchbot.Common.Base.Models;
using Twitchbot.Services.Authentication.Business;
using Twitchbot.Services.Authentication.ModelsIn;
using Twitchbot_Sample.Authentication.ModelsIn;

namespace Twitchbot.Services.Authentication.Controllers
{
    [Route("api/Authentication")]
    public class SpotifyOAuthController : ControllerBase
    {
        private readonly ILogger<SpotifyOAuthController> _logger;
        private readonly SpotifyOAuthBusiness _oAuthBusiness;

        public SpotifyOAuthController(ILogger<SpotifyOAuthController> logger,
            SpotifyOAuthBusiness oAuthBusiness)
        {
            _logger = logger;
            _oAuthBusiness = oAuthBusiness;
        }

        [HttpPost("/spotify/oauth")]
        public async Task<ActionResult<HttpResultModel<AuthenticationModel>>> Post(CancellationToken cancellationToken, string code, string clientId)
        {
            _logger.LogDebug("Post oauth");

            return await _oAuthBusiness.PostOAuth(code, clientId, cancellationToken);
        }

        [HttpGet("/spotify/oauth")]
        public async Task<ActionResult<HttpResultModel<SpotifyModel>>> Get(CancellationToken cancellationToken, string clientId)
        {
            _logger.LogDebug("Get oauth");

            return await _oAuthBusiness.GetOAuth(clientId, cancellationToken);
        }
    }
}