namespace MeetLive.Services.Domain.IRepository
{
    public interface IDapperOptions
    {
        bool UseIndependentConnectionForQueries { get; }
        int DefaultCommandTimeout { get; }
        bool EnableDetailedLogging { get; }
    }

    public class DapperOptions : IDapperOptions
    {
        public bool UseIndependentConnectionForQueries { get; set; } = true;
        public int DefaultCommandTimeout { get; set; } = 30;
        public bool EnableDetailedLogging { get; set; } = true;
    }
}
