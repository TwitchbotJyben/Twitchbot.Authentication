using System;
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
    public class TwitchOAuthBusiness : ITwitchOAuthBusiness
    {
        private readonly IApiClient _client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TwitchOAuthBusiness> _logger;
        private readonly string _clientId;
        private readonly string _secretId;
        private readonly IStringLocalizer<TwitchOAuthBusiness> _localizer;
        private readonly TwitchDao _twitchDao;
        private readonly UsersDao _usersDao;

        public TwitchOAuthBusiness(IConfiguration configuration, ILogger<TwitchOAuthBusiness> logger, IApiClient clientBase,
            IStringLocalizer<TwitchOAuthBusiness> localizer, TwitchDao twitchDao, UsersDao usersDao)
        {
            _configuration = configuration;
            _logger = logger;
            _clientId = _configuration["ApiParams:Twitch:ClientId"];
            _secretId = _configuration["ApiParams:Twitch:SecretId"];
            _client = clientBase;
            _localizer = localizer;
            _twitchDao = twitchDao;
            _usersDao = usersDao;
        }

        public async Task<HttpResultModel<AuthenticationModel>> PostOAuth(string code, CancellationToken cancellationToken)
        {
            var resultAuthentication = new HttpResultModel<AuthenticationModel>();

            var resultOAuth = await PostOAuth(code).ConfigureAwait(false);
            if (resultOAuth.Result)
            {
                var resultValidate = await ValidateTwitchToken(resultOAuth.Model.AccessToken).ConfigureAwait(false);
                if (resultValidate.Result)
                {
                    var model = new AuthenticationModel
                    {
                        AccessToken = resultOAuth.Model.AccessToken,
                        RefreshToken = resultOAuth.Model.RefreshToken,
                        ClientId = Guid.NewGuid().ToString(),
                        UserName = resultValidate.Model.Login
                    };
                    resultAuthentication.PerformResult(true, "", _localizer["Connexion réussie."], model);
                }
                else
                {
                    resultAuthentication.PerformResult(false, "", _localizer["Connexion impossible."], null);
                }
            }
            else
            {
                resultAuthentication.PerformResult(false, "", _localizer["Connexion impossible."], null);
            }

            if (resultAuthentication.Result && resultAuthentication.Model != null)
            {
                var readModelList = await _twitchDao.QueryModel(x => x.ClientId == resultAuthentication.Model.ClientId || x.UserUsers.Name == resultAuthentication.Model.UserName, cancellationToken).ConfigureAwait(false);
                if (readModelList.Any())
                {
                    var user = readModelList.First();
                    await UpdateTwitchModel(user, resultAuthentication.Model, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    var resultCreateUsersModel = await CreateUserModel(resultAuthentication.Model, cancellationToken).ConfigureAwait(false);
                    await CreateTwitchModel(resultAuthentication.Model, resultCreateUsersModel, cancellationToken).ConfigureAwait(false);
                }
            }

            return resultAuthentication;
        }

        private async Task<UsersReadModel> CreateUserModel(AuthenticationModel authenticationModel, CancellationToken cancellationToken)
        {
            var createModel = new UsersCreateModel()
            {
                Name = authenticationModel.UserName
            };

            return await _usersDao.CreateModel(createModel, cancellationToken).ConfigureAwait(false);
        }

        private async Task CreateTwitchModel(AuthenticationModel authenticationModel, UsersReadModel usersReadModel, CancellationToken cancellationToken)
        {
            var createModel = new TwitchCreateModel()
            {
                ClientId = authenticationModel.ClientId,
                RefreshToken = authenticationModel.RefreshToken,
                Token = authenticationModel.AccessToken,
                UserId = usersReadModel.Id
            };

            await _twitchDao.CreateModel(createModel, cancellationToken).ConfigureAwait(false);
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

        private async Task<HttpResultModel<TwitchOAuthModel>> PostOAuth(string code)
        {
            _logger.LogInformation("Post OAuth {0}", code);

            var url = _configuration["ApiUrl:Twitch:OAuth"].Replace("{code}", code).Replace("{clientId}", _clientId).Replace("{secretId}", _secretId).Replace("{redirect_url}", _configuration["ApiParams:Twitch:RedirectUrl"]);
            var result = await _client.PerformRequest<TwitchOAuthModel>(url, HttpMethod.Post).ConfigureAwait(false);

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