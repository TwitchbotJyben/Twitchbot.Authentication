using AutoMapper;
using Twitchbot.Base.Dao;
using Twitchbot.Models.Data;
using Twitchbot.Models.Data.Entities;
using Twitchbot.Models.Domain.Models;

namespace Twitchbot.Authentication.Dao
{
    public class SpotifyDao : BaseDao<Spotify, SpotifyReadModel, SpotifyCreateModel, SpotifyUpdateModel>
    {
        public SpotifyDao(TwitchbotContext dataContext, IMapper mapper) : base(dataContext, mapper) { }
    }
}