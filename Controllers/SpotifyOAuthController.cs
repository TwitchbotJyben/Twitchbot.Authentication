using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Twitchbot.Authentication.Business;
using Twitchbot.Authentication.ModelsIn;
using Twitchbot.Base.Models;

namespace Twitchbot.Authentication.Controllers
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

        [HttpGet("/spotify/oauth")]
        public async Task<ActionResult<HttpResultModel<AuthenticationModel>>> Get(CancellationToken cancellationToken, string code, string clientId)
        {
            _logger.LogDebug("Get oauth");

            return await _oAuthBusiness.GetOAuth(code, clientId, cancellationToken);
        }
    }
}