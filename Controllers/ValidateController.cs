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
    public class ValidateController : ControllerBase
    {
        private readonly ILogger<ValidateController> _logger;
        private readonly ValidateBusiness _validateBusiness;

        public ValidateController(ILogger<ValidateController> logger,
            ValidateBusiness validateBusiness)
        {
            _logger = logger;
            _validateBusiness = validateBusiness;
        }

        [HttpGet("/validate")]
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