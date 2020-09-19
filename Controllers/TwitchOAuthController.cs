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
    public class TwitchOAuthController : ControllerBase
    {
        private readonly ILogger<TwitchOAuthController> _logger;
        private readonly TwitchOAuthBusiness _oAuthBusiness;

        public TwitchOAuthController(ILogger<TwitchOAuthController> logger,
            TwitchOAuthBusiness oAuthBusiness)
        {
            _logger = logger;
            _oAuthBusiness = oAuthBusiness;
        }

        [HttpGet("/twitch/oauth")]
        public async Task<ActionResult<HttpResultModel<AuthenticationModel>>> Get(CancellationToken cancellationToken, string code)
        {
            _logger.LogDebug("Get oauth");

            return await _oAuthBusiness.GetOAuth(code, cancellationToken);
        }
    }
}