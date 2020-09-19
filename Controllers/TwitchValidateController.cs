using System.Linq;
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
    public class TwitchValidateController : ControllerBase
    {
        private readonly ILogger<TwitchValidateController> _logger;
        private readonly TwitchValidateBusiness _validateBusiness;

        public TwitchValidateController(ILogger<TwitchValidateController> logger,
            TwitchValidateBusiness validateBusiness)
        {
            _logger = logger;
            _validateBusiness = validateBusiness;
        }

        [HttpGet("/twitch/validate")]
        public async Task<ActionResult<HttpResultModel<AuthenticationModel>>> Get(CancellationToken cancellationToken, string clientId)
        {
            _logger.LogDebug("Get validate authentication");

            var readModelList = await _validateBusiness.ReadTwitchModel(cancellationToken, clientId);

            if (!readModelList.Any())
                return NotFound();

            return await _validateBusiness.ValidateAuth(readModelList, cancellationToken);
        }
    }
}