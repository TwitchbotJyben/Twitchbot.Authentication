using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Twitchbot.Common.Base.Client;
using Twitchbot.Common.Base.Models;
using Twitchbot.Common.Models.Domain.Models;
using Twitchbot.Services.Authentication.Dao;
using Twitchbot.Services.Authentication.ModelsIn;
using Twitchbot.Services.Authentication.ModelsOut;
using Twitchbot_Sample.Authentication.ModelsIn;

namespace Twitchbot.Services.Authentication.Business
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

        internal async Task<ActionResult<HttpResultModel<SpotifyModel>>> GetOAuth(string clientId, CancellationToken cancellationToken)
        {
            var resultAuthentication = new HttpResultModel<SpotifyModel>();
            var readTwitchModel = await _twitchDao.QueryModel(x => x.ClientId == clientId, cancellationToken);

            if (readTwitchModel.Any())
            {
                var userId = readTwitchModel.First().UserId;
                var readSpotifyModel = await ReadSpotifyModel(userId, cancellationToken);

                if (readSpotifyModel.Any())
                {
                    var spotify = readSpotifyModel.First();

                    if (IsTokenValid(spotify))
                    {
                        resultAuthentication.PerformResult(true, "", _localizer["Informations de connexion Spotify récupérées."], new SpotifyModel() { Token = spotify.Token });
                    }
                    else
                    {
                        await RefreshToken(resultAuthentication, userId, spotify, cancellationToken);
                    }
                }
                else
                {
                    resultAuthentication.PerformResult(false, "", _localizer["Informations de connexion Spotify inconnues. Veuillez demander un nouveau token."], null);
                }
            }
            else
            {
                resultAuthentication.PerformResult(false, "", _localizer["Utilisateur inconnu."], null);
            }

            return resultAuthentication;
        }

        internal async Task<HttpResultModel<AuthenticationModel>> PostOAuth(string code, string clientId, CancellationToken cancellationToken)
        {
            var resultAuthentication = new HttpResultModel<AuthenticationModel>();

            var resultOAuth = await PostOAuth(code);
            if (!resultOAuth.Result)
            {
                resultAuthentication.PerformResult(false, "", _localizer["Connexion impossible."], null);
                return resultAuthentication;
            }

            var readTwitchModel = await _twitchDao.QueryModel(x => x.ClientId == clientId, cancellationToken);
            if (readTwitchModel.Any())
            {
                var userId = readTwitchModel.First().UserId;

                var model = new AuthenticationModel
                {
                    AccessToken = resultOAuth.Model.AccessToken,
                    RefreshToken = resultOAuth.Model.RefreshToken,
                    ExpiresIn = resultOAuth.Model.ExpiresIn,
                    Time = DateTime.Now
                };

                var readSpotifyModel = await ReadSpotifyModel(userId, cancellationToken);

                if (readSpotifyModel.Any())
                {
                    var spotify = readSpotifyModel.First();
                    await UpdateSpotifyModel(spotify.Id, userId, model, cancellationToken);
                }
                else
                {
                    await CreateSpotifyModel(userId, model, cancellationToken);
                }

                resultAuthentication.PerformResult(true, "", _localizer["Connexion réussie."], model);
            }
            else
            {
                resultAuthentication.PerformResult(false, "", _localizer["Utilisateur inconnu."], null);
            }

            return resultAuthentication;
        }

        private async Task<IReadOnlyList<SpotifyReadModel>> ReadSpotifyModel(int userId, CancellationToken cancellationToken)
        {
            return await _spotifyDao.QueryModel(x => x.UserId == userId, cancellationToken);
        }

        private async Task UpdateSpotifyModel(int id, int userId, AuthenticationModel model, CancellationToken cancellationToken)
        {
            var updateModel = new SpotifyUpdateModel
            {
                Id = model.Id,
                RefreshToken = model.RefreshToken,
                Token = model.AccessToken,
                UserId = userId,
                ExpiresIn = model.ExpiresIn,
                Time = model.Time
            };

            await _spotifyDao.UpdateModel(id, updateModel, cancellationToken);
        }

        private async Task CreateSpotifyModel(int userId, AuthenticationModel authenticationModel, CancellationToken cancellationToken)
        {
            var createModel = new SpotifyCreateModel()
            {
                RefreshToken = authenticationModel.RefreshToken,
                Token = authenticationModel.AccessToken,
                UserId = userId,
                ExpiresIn = authenticationModel.ExpiresIn,
                Time = authenticationModel.Time
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

        private async Task<HttpResultModel<SpotifyOAuthModel>> PostRefreshOAuth(string token)
        {
            _logger.LogInformation("Post RefreshOAuth {0}", token);

            var nvc = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("refresh_token", token),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("client_id", _configuration["ApiParams:Spotify:ClientId"]),
                    new KeyValuePair<string, string>("client_secret", _configuration["ApiParams:Spotify:SecretId"])
                };

            var formUrlEncodedContent = new FormUrlEncodedContent(nvc);
            var url = _configuration["ApiUrl:Spotify:OAuth"];
            var result = await _client.PerformRequest<SpotifyOAuthModel>(url, HttpMethod.Post, formUrlEncodedContent);

            return result;
        }

        private static bool IsTokenValid(SpotifyReadModel spotify)
        {
            return spotify.ExpiresIn.HasValue && spotify.Time > DateTime.Now.AddSeconds(spotify.ExpiresIn.Value);
        }

        private async Task RefreshToken(HttpResultModel<SpotifyModel> resultAuthentication, int userId, SpotifyReadModel spotify, CancellationToken cancellationToken)
        {
            var resultOAuth = await PostRefreshOAuth(spotify.RefreshToken);
            if (resultOAuth.Result)
            {
                var model = new AuthenticationModel()
                {
                    Id = spotify.Id,
                    AccessToken = resultOAuth.Model.AccessToken,
                    RefreshToken = spotify.RefreshToken,
                    ExpiresIn = resultOAuth.Model.ExpiresIn,
                    Time = DateTime.Now
                };

                await UpdateSpotifyModel(spotify.Id, userId, model, cancellationToken);

                resultAuthentication.PerformResult(true, "", _localizer["Informations de connexion Spotify récupérées."], new SpotifyModel() { Token = spotify.Token });
            }
            else
            {
                resultAuthentication.PerformResult(false, "", _localizer["Impossible de refraichir le token."], null);
            }
        }
    }
}