using AutoMapper;
using Twitchbot.Base.Dao;
using Twitchbot.Models.Data;
using Twitchbot.Models.Data.Entities;
using Twitchbot.Models.Domain.Models;

namespace Twitchbot.Authentication.Dao
{
    public class UsersDao : BaseDao<Users, UsersReadModel, UsersCreateModel, UsersUpdateModel>
    {
        public UsersDao(TwitchbotContext dataContext, IMapper mapper) : base(dataContext, mapper) { }
    }
}