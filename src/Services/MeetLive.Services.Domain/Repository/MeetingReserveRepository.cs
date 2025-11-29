using MeetLive.Services.Domain.Entities;
using MeetLive.Services.Domain.IRepository;

namespace MeetLive.Services.Domain.Repository
{
    public class MeetingReserveRepository : BaseRepository<MeetingReserve>, IMeetingReserveRepository
    {
        public MeetingReserveRepository(MeetLiveDbContext context) : base(context)
        {
        }
    }
}
