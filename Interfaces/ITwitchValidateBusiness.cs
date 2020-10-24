
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Twitchbot.Common.Base.Models;
using Twitchbot.Common.Models.Domain.Models;
using Twitchbot.Services.Authentication.ModelsIn;

namespace Twitchbot.Services.Authentication.Interfaces
{
    public interface ITwitchValidateBusiness
    {
        Task<HttpResultModel<AuthenticationModel>> ValidateAuth(IReadOnlyList<TwitchReadModel> readModelList, CancellationToken cancellationToken);
        Task<IReadOnlyList<TwitchReadModel>> ReadTwitchModel(string clientId, CancellationToken cancellationToken);
    }
}
