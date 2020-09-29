using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Twitchbot.Common.Base.Client;
using Twitchbot.Common.Base.Models;
using Twitchbot.Common.Models.Domain.Models;
using Twitchbot.Services.Authentication.Dao;
using Twitchbot.Services.Authentication.ModelsIn;
using Twitchbot.Services.Authentication.ModelsOut;

namespace Twitchbot.Services.Authentication.Business
{
    public class TwitchOAuthBusiness
    {
        private readonly ClientBase _client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TwitchOAuthBusiness> _logger;
        private readonly string _clientId;
        private readonly string _secretId;
        private readonly IStringLocalizer<TwitchOAuthBusiness> _localizer;
        private readonly TwitchDao _twitchDao;
        private readonly UsersDao _usersDao;

        public TwitchOAuthBusiness(IConfiguration configuration, ILogger<TwitchOAuthBusiness> logger, ClientBase clientBase,
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

        internal async Task<HttpResultModel<AuthenticationModel>> PostOAuth(string code, CancellationToken cancellationToken)
        {
            var resultAuthentication = new HttpResultModel<AuthenticationModel>();

            var resultOAuth = await PostOAuth(code);
            if (resultOAuth.Result)
            {
                var resultValidate = await ValidateTwitchToken(resultOAuth.Model.AccessToken);
                if (resultValidate.Result)
                {
                    var model = new AuthenticationModel
                    {
                        AccessToken = resultOAuth.Model.AccessToken,
                        RefreshToken = resultOAuth.Model.RefreshToken,
                        ClientId = resultValidate.Model.ClientId,
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
                var readModelList = await _twitchDao.QueryModel(x => x.ClientId == resultAuthentication.Model.ClientId || x.UserUsers.Name == resultAuthentication.Model.UserName, cancellationToken);
                if (readModelList.Any())
                {
                    var user = readModelList.First();
                    await UpdateTwitchModel(user, resultAuthentication.Model, cancellationToken);
                }
                else
                {
                    var resultCreateUsersModel = await CreateUserModel(resultAuthentication.Model, cancellationToken);
                    await CreateTwitchModel(resultAuthentication.Model, resultCreateUsersModel, cancellationToken);
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

            return await _usersDao.CreateModel(createModel, cancellationToken);
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

            await _twitchDao.CreateModel(createModel, cancellationToken);
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

        private async Task<HttpResultModel<TwitchOAuthModel>> PostOAuth(string code)
        {
            _logger.LogInformation("Post OAuth {0}", code);

            var url = _configuration["ApiUrl:Twitch:OAuth"].Replace("{code}", code).Replace("{clientId}", _clientId).Replace("{secretId}", _secretId).Replace("{redirect_url}", _configuration["ApiParams:Twitch:RedirectUrl"]);
            var result = await _client.PerformRequest<TwitchOAuthModel>(url, HttpMethod.Post);

            return result;
        }

        private async Task<HttpResultModel<TwitchValidateTokenModel>> ValidateTwitchToken(string token)
        {
            _logger.LogInformation("Validation du token {0}", token);

            var url = _configuration["ApiUrl:Twitch:ValidateToken"];
            var headers = new Dictionary<string, string> { { "Authorization", token } };
            var result = await _client.PerformRequest<TwitchValidateTokenModel>(url, HttpMethod.Get, null, headers);

            return result;
        }
    }
}