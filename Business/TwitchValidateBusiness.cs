using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Twitchbot.Common.Base.Interfaces;
using Twitchbot.Common.Base.Models;
using Twitchbot.Common.Models.Domain.Models;
using Twitchbot.Services.Authentication.Dao;
using Twitchbot.Services.Authentication.Interfaces;
using Twitchbot.Services.Authentication.ModelsIn;
using Twitchbot.Services.Authentication.ModelsOut;

namespace Twitchbot.Services.Authentication.Business
{
    public class TwitchValidateBusiness : ITwitchValidateBusiness
    {
        private readonly IApiClient _client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TwitchValidateBusiness> _logger;
        private readonly string _clientId;
        private readonly string _secretId;
        private readonly IStringLocalizer<TwitchValidateBusiness> _localizer;
        private readonly TwitchDao _twitchDao;

        public TwitchValidateBusiness(IConfiguration configuration, ILogger<TwitchValidateBusiness> logger, IApiClient clientBase,
            IStringLocalizer<TwitchValidateBusiness> localizer, TwitchDao twitchDao)
        {
            _configuration = configuration;
            _logger = logger;
            _clientId = _configuration["ApiParams:Twitch:ClientId"];
            _secretId = _configuration["ApiParams:Twitch:SecretId"];
            _client = clientBase;
            _localizer = localizer;
            _twitchDao = twitchDao;
        }

        public async Task<HttpResultModel<AuthenticationModel>> ValidateAuth(IReadOnlyList<TwitchReadModel> readModelList, CancellationToken cancellationToken)
        {
            var user = readModelList.First();
            var resultAuthentication = new HttpResultModel<AuthenticationModel>();

            if (user.ClientId != null && user.Token != null && user.RefreshToken != null)
            {
                var resultValidate = await ValidateTwitchToken(user.Token).ConfigureAwait(false);
                if (resultValidate.Result)
                {
                    var model = new AuthenticationModel
                    {
                        AccessToken = user.Token,
                        RefreshToken = user.RefreshToken,
                        ClientId = user.ClientId,
                        UserName = resultValidate.Model.Login
                    };
                    resultAuthentication.PerformResult(true, "", _localizer["Connexion réussie."], model);
                }
                else
                {
                    var resultRefresh = await RefreshTwitchToken(user.RefreshToken).ConfigureAwait(false);
                    if (resultRefresh.Result)
                    {
                        resultValidate = await ValidateTwitchToken(resultRefresh.Model.AccessToken).ConfigureAwait(false);
                        if (resultValidate.Result)
                        {
                            var model = new AuthenticationModel
                            {
                                AccessToken = resultRefresh.Model.AccessToken,
                                RefreshToken = resultRefresh.Model.RefreshToken,
                                ClientId = user.ClientId,
                                UserName = resultValidate.Model.Login
                            };
                            resultAuthentication.PerformResult(true, "", _localizer["Connexion réussie."], model);
                        }
                    }
                    else
                    {
                        resultAuthentication.PerformResult(false, _localizer["Authentification expirée, merci de vous reconnecter à Twitch."], "", null);
                    }
                }
            }
            else
            {
                resultAuthentication.PerformResult(false, _localizer["Merci de vous connecter à Twitch pour accéder au site."], "", null);
            }

            if (resultAuthentication.Result && resultAuthentication.Model != null)
            {
                await UpdateTwitchModel(user, resultAuthentication.Model, cancellationToken).ConfigureAwait(false);
            }

            return resultAuthentication;
        }

        public async Task<IReadOnlyList<TwitchReadModel>> ReadTwitchModel(string clientId, CancellationToken cancellationToken)
        {
            return await _twitchDao.QueryModel(x => x.ClientId == clientId, cancellationToken).ConfigureAwait(false);
        }

        private async Task UpdateTwitchModel(TwitchReadModel user, AuthenticationModel authenticationModel, CancellationToken cancellationToken)
        {
            var twitchUpdateModel = new TwitchUpdateModel
            {
                ClientId = user.ClientId,
                Token = authenticationModel.AccessToken,
                RefreshToken = authenticationModel.RefreshToken,
                Id = user.Id,
                UserId = user.UserId
            };

            await _twitchDao.UpdateModel(user.Id, twitchUpdateModel, cancellationToken).ConfigureAwait(false);
        }

        private async Task<HttpResultModel<TwitchRefreshTokenModel>> RefreshTwitchToken(string refreshToken)
        {
            _logger.LogInformation("Refresh du token {0}", refreshToken);

            var url = _configuration["ApiUrl:Twitch:RefreshToken"].Replace("{refreshToken}", refreshToken).Replace("{clientId}", _clientId).Replace("{secretId}", _secretId);
            var result = await _client.PerformRequest<TwitchRefreshTokenModel>(url, HttpMethod.Post).ConfigureAwait(false);

            return result;
        }

        private async Task<HttpResultModel<TwitchValidateTokenModel>> ValidateTwitchToken(string token)
        {
            _logger.LogInformation("Validation du token {0}", token);

            var url = _configuration["ApiUrl:Twitch:ValidateToken"];
            var headers = new Dictionary<string, string> { { "OAuth", token } };
            var result = await _client.PerformRequest<TwitchValidateTokenModel>(url, HttpMethod.Get, null, headers).ConfigureAwait(false);

            return result;
        }
    }
}