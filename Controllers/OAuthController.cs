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
    public class OAuthController : ControllerBase
    {
        private readonly ILogger<OAuthController> _logger;
        private readonly OAuthBusiness _oAuthBusiness;

        public OAuthController(ILogger<OAuthController> logger,
            OAuthBusiness oAuthBusiness)
        {
            _logger = logger;
            _oAuthBusiness = oAuthBusiness;
        }

        [HttpGet("/oauth")]
        public async Task<ActionResult<HttpResultModel<AuthenticationModel>>> Get(CancellationToken cancellationToken, string code)
        {
            _logger.LogDebug("Get oauth");

            return await _oAuthBusiness.GetOAuth(code, cancellationToken);
        }
    }
}