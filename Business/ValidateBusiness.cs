using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Twitchbot.Authentication.Dao;
using Twitchbot.Authentication.ModelsIn;
using Twitchbot.Authentication.ModelsOut;
using Twitchbot.Base.Client;
using Twitchbot.Base.Models;
using Twitchbot.Models.Domain.Models;

namespace Twitchbot.Authentication.Business
{
    public class ValidateBusiness
    {
        private readonly ClientBase _client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ValidateBusiness> _logger;
        private readonly string _clientId;
        private readonly string _secretId;
        private readonly IStringLocalizer<ValidateBusiness> _localizer;
        private readonly TwitchDao _twitchDao;

        public ValidateBusiness(IConfiguration configuration, ILogger<ValidateBusiness> logger, ClientBase clientBase,
            IStringLocalizer<ValidateBusiness> localizer, TwitchDao twitchDao)
        {
            _configuration = configuration;
            _logger = logger;
            _clientId = _configuration["TwitchClientId"];
            _secretId = _configuration["TwitchSecretId"];
            _client = clientBase;
            _localizer = localizer;
            _twitchDao = twitchDao;
        }

        internal async Task<HttpResultModel<AuthenticationModel>> ValidateAuth(IReadOnlyList<TwitchReadModel> readModelList, CancellationToken cancellationToken)
        {
            var user = readModelList.First();
            var resultAuthentication = new HttpResultModel<AuthenticationModel>();

            if (user.ClientId != null && user.Token != null && user.RefreshToken != null)
            {
                var resultValidate = await ValidateTwitchToken(user.Token);
                if (resultValidate.Result)
                {
                    var model = new AuthenticationModel
                    {
                        AccessToken = user.Token,
                        RefreshToken = user.RefreshToken,
                        ClientId = resultValidate.Model.ClientId,
                        UserName = resultValidate.Model.Login
                    };
                    resultAuthentication.PerformResult(true, "", _localizer["Connexion réussie."], model);
                }
                else
                {
                    var resultRefresh = await RefreshTwitchToken(user.RefreshToken);
                    if (resultRefresh.Result)
                    {
                        resultValidate = await ValidateTwitchToken(resultRefresh.Model.AccessToken);
                        if (resultValidate.Result)
                        {
                            var model = new AuthenticationModel
                            {
                                AccessToken = resultRefresh.Model.AccessToken,
                                RefreshToken = resultRefresh.Model.RefreshToken,
                                ClientId = resultValidate.Model.ClientId,
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
                await UpdateTwitchModel(user, resultAuthentication.Model, cancellationToken);
            }

            return resultAuthentication;
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

            await _twitchDao.UpdateModel(user.Id, twitchUpdateModel, cancellationToken);
        }

        internal async Task<IReadOnlyList<TwitchReadModel>> ReadTwitchModel(CancellationToken cancellationToken, string clientId)
        {
            return await _twitchDao.QueryModel(x => x.ClientId == clientId, cancellationToken);
        }

        private async Task<HttpResultModel<TwitchRefreshTokenModel>> RefreshTwitchToken(string refreshToken)
        {
            _logger.LogInformation("Refresh du token {0}", refreshToken);

            var url = _configuration["Url:Twitch:RefreshToken"].Replace("{refreshToken}", refreshToken).Replace("{clientId}", _clientId).Replace("{secretId}", _secretId);
            var result = await _client.PerformRequest<object, TwitchRefreshTokenModel>(url, HttpMethod.Post);

            return result;
        }

        private async Task<HttpResultModel<TwitchValidateTokenModel>> ValidateTwitchToken(string token)
        {
            _logger.LogInformation("Validation du token {0}", token);

            var url = _configuration["Url:Twitch:ValidateToken"];
            var headers = new Dictionary<string, string> { { "Authorization", token } };
            var result = await _client.PerformRequest<object, TwitchValidateTokenModel>(url, HttpMethod.Get, null, headers);

            return result;
        }
    }
}