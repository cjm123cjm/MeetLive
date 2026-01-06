using MeetLive.Services.Domain.Entities;
using MeetLive.Services.Domain.IRepository;

namespace MeetLive.Services.Domain.Repository
{
    public class AppUpdateRepository : BaseRepository<AppUpdate>, IAppUpdateRepository
    {
        public AppUpdateRepository(MeetLiveDbContext context) : base(context)
        {
        }
    }
}
