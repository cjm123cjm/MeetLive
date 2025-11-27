using MeetLive.Services.Domain.Entities;
using MeetLive.Services.Domain.IRepository;

namespace MeetLive.Services.Domain.Repository
{
    public class MeetingInfoRepository : BaseRepository<MeetingInfo>, IMeetingInfoRepository
    {
        public MeetingInfoRepository(MeetLiveDbContext context) : base(context)
        {
        }
    }
}
