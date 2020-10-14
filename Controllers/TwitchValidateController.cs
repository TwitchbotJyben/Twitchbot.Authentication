using System.Linq;
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

        [HttpPost("/twitch/validation")]
        public async Task<ActionResult<HttpResultModel<AuthenticationModel>>> Post(CancellationToken cancellationToken, string clientId)
        {
            _logger.LogDebug("Post validate authentication");

            var readModelList = await _validateBusiness.ReadTwitchModel(cancellationToken, clientId);

            if (!readModelList.Any())
                return NotFound();

            return await _validateBusiness.ValidateAuth(readModelList, cancellationToken);
        }
    }
}