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
    public class SpotifyOAuthBusiness
    {
        private readonly ClientBase _client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SpotifyOAuthBusiness> _logger;
        private readonly IStringLocalizer<SpotifyOAuthBusiness> _localizer;
        private readonly SpotifyDao _spotifyDao;
        private readonly TwitchDao _twitchDao;

        public SpotifyOAuthBusiness(IConfiguration configuration, ILogger<SpotifyOAuthBusiness> logger, ClientBase clientBase,
            IStringLocalizer<SpotifyOAuthBusiness> localizer, SpotifyDao spotifyDao, TwitchDao twitchDao)
        {
            _configuration = configuration;
            _logger = logger;
            _client = clientBase;
            _localizer = localizer;
            _spotifyDao = spotifyDao;
            _twitchDao = twitchDao;
        }

        internal async Task<HttpResultModel<AuthenticationModel>> GetOAuth(string code, string clientId, CancellationToken cancellationToken)
        {
            var resultAuthentication = new HttpResultModel<AuthenticationModel>();

            var resultOAuth = await PostOAuth(code);
            if (resultOAuth.Result)
            {
                resultAuthentication.PerformResult(true, "", _localizer["Connexion réussie."], null);
            }
            else
            {
                resultAuthentication.PerformResult(false, "", _localizer["Connexion impossible."], null);
            }

            if (resultAuthentication.Result)
            {
                var readTwitchModel = await _twitchDao.QueryModel(x => x.ClientId == clientId, cancellationToken);
                if (readTwitchModel.Any())
                {
                    var userId = readTwitchModel.First().UserId;

                    var model = new AuthenticationModel
                    {
                        AccessToken = resultOAuth.Model.AccessToken,
                        RefreshToken = resultOAuth.Model.RefreshToken
                    };

                    resultAuthentication.PerformResult(true, "", _localizer["Connexion réussie."], model);

                    var readSpotifyModel = await _spotifyDao.QueryModel(x => x.UserId == userId, cancellationToken);

                    if (readSpotifyModel.Any())
                    {
                        var spotify = readSpotifyModel.First();
                        await UpdateSpotifyModel(spotify.Id, userId, resultAuthentication.Model, cancellationToken);
                    }
                    else
                    {
                        await CreateSpotifyModel(userId, resultAuthentication.Model, cancellationToken);
                    }
                }
                else
                {
                    resultAuthentication.PerformResult(false, "", _localizer["Utilisateur inconnu."], null);
                }

            }

            return resultAuthentication;
        }

        private async Task UpdateSpotifyModel(int id, int userId, AuthenticationModel model, CancellationToken cancellationToken)
        {
            var updateModel = new SpotifyUpdateModel
            {
                RefreshToken = model.RefreshToken,
                Token = model.AccessToken,
                UserId = userId
            };

            await _spotifyDao.UpdateModel(id, updateModel, cancellationToken);
        }

        private async Task CreateSpotifyModel(int userId, AuthenticationModel authenticationModel, CancellationToken cancellationToken)
        {
            var createModel = new SpotifyCreateModel()
            {
                RefreshToken = authenticationModel.RefreshToken,
                Token = authenticationModel.AccessToken,
                UserId = userId
            };

            await _spotifyDao.CreateModel(createModel, cancellationToken);
        }

        private async Task<HttpResultModel<SpotifyOAuthModel>> PostOAuth(string code)
        {
            _logger.LogInformation("Post OAuth {0}", code);

            var nvc = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("redirect_uri", _configuration["ApiParams:Spotify:RedirectUrl"]),
                    new KeyValuePair<string, string>("client_id", _configuration["ApiParams:Spotify:ClientId"]),
                    new KeyValuePair<string, string>("client_secret", _configuration["ApiParams:Spotify:SecretId"])
                };
            var formUrlEncodedContent = new FormUrlEncodedContent(nvc);
            var url = _configuration["ApiUrl:Spotify:OAuth"];
            var result = await _client.PerformRequest<SpotifyOAuthModel>(url, HttpMethod.Post, formUrlEncodedContent);

            return result;
        }
    }
}