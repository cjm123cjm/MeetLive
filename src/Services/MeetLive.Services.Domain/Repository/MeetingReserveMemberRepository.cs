using MeetLive.Services.Domain.Entities;
using MeetLive.Services.Domain.IRepository;

namespace MeetLive.Services.Domain.Repository
{
    public class MeetingReserveMemberRepository : BaseRepository<MeetingReserveMember>, IMeetingReserveMemberRepository
    {
        public MeetingReserveMemberRepository(MeetLiveDbContext context) : base(context)
        {
        }
    }
}
