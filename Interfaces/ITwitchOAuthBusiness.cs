using System.Threading;
using System.Threading.Tasks;
using Twitchbot.Common.Base.Models;
using Twitchbot.Services.Authentication.ModelsIn;

namespace Twitchbot.Services.Authentication.Interfaces
{
    public interface ITwitchOAuthBusiness
    {
        Task<HttpResultModel<AuthenticationModel>> PostOAuth(string code, CancellationToken cancellationToken);
    }
}