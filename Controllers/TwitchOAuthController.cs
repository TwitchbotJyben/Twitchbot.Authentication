using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Twitchbot.Common.Base.Models;
using Twitchbot.Services.Authentication.Business;
using Twitchbot.Services.Authentication.ModelsIn;

namespace Twitchbot.Services.Authentication.Controllers
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

        [HttpPost("/twitch/oauth")]
        public async Task<ActionResult<HttpResultModel<AuthenticationModel>>> Post(string code, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Post oauth");

            return await _oAuthBusiness.PostOAuth(code, cancellationToken).ConfigureAwait(false);
        }
    }
}