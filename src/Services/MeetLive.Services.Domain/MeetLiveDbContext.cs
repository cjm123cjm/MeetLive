using MeetLive.Services.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeetLive.Services.Domain
{
    public class MeetLiveDbContext : DbContext
    {
        public MeetLiveDbContext(DbContextOptions<MeetLiveDbContext> options) : base(options)
        {

        }

        public DbSet<UserInfo> UserInfos { get; set; }
        public DbSet<MeetingInfo> MeetingInfos { get; set; }
        public DbSet<MeetingMember> MeetingMembers { get; set; }
        public DbSet<MeetingReserve> MeetingReserves { get; set; }
        public DbSet<MeetingReserveMember> MeetingReserveMembers { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserInfo>(t =>
            {
                t.HasKey(m => m.UserId);
                t.HasIndex(m => m.Email).IsUnique();
            });
        }
    }
}
