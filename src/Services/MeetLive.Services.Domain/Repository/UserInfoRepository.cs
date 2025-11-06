using MeetLive.Services.Domain.Entities;
using MeetLive.Services.Domain.IRepository;
using Microsoft.Extensions.Logging;

namespace MeetLive.Services.Domain.Repository
{
    public class UserInfoRepository : BaseRepository<UserInfo>, IUserInfoRepository
    {
        protected UserInfoRepository(MeetLiveDbContext context, ILogger logger) : base(context, logger)
        {
        }
    }
}
