using AutoMapper;
using Twitchbot.Common.Base.Dao;
using Twitchbot.Common.Models.Data;
using Twitchbot.Common.Models.Data.Entities;
using Twitchbot.Common.Models.Domain.Models;

namespace Twitchbot.Services.Authentication.Dao
{
    public class TwitchDao : BaseDao<Twitch, TwitchReadModel, TwitchCreateModel, TwitchUpdateModel>
    {
        public TwitchDao(TwitchbotContext dataContext, IMapper mapper) : base(dataContext, mapper) { }
    }
}