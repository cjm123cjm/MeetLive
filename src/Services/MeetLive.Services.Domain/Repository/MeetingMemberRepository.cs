using MeetLive.Services.Domain.Entities;
using MeetLive.Services.Domain.IRepository;

namespace MeetLive.Services.Domain.Repository
{
    public class MeetingMemberRepository : BaseRepository<MeetingMember>, IMeetingMemberRepository
    {
        public MeetingMemberRepository(MeetLiveDbContext context) : base(context)
        {
        }
    }
}
