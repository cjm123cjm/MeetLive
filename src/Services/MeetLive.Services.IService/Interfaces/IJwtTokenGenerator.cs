using MeetLive.Services.IService.Dtos.Outputs;

namespace MeetLive.Services.IService.Interfaces
{
    /// <summary>
    /// 生成token
    /// </summary>
    public interface IJwtTokenGenerator
    {
        /// <summary>
        /// 生成token
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns></returns>
        string GenerateToken(UserInfoDto userDto);
    }
}
