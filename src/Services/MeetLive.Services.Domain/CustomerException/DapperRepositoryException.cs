namespace MeetLive.Services.Domain.CustomerException
{
    /// <summary>
    /// Dapper 仓储自定义异常
    /// </summary>
    public class DapperRepositoryException : Exception
    {
        public DapperRepositoryException(string message) : base(message) { }
        public DapperRepositoryException(string message, Exception innerException) : base(message, innerException) { }
    }
}
