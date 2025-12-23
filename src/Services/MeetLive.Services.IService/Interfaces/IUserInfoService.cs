using MeetLive.Services.IService.Dtos.Inputs;
using MeetLive.Services.IService.Dtos.Outputs;

namespace MeetLive.Services.IService.Interfaces
{
    public interface IUserInfoService
    {
        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="registerInput"></param>
        /// <returns></returns>
        Task RegisterAsync(RegisterInput registerInput);

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="loginInput"></param>
        /// <returns></returns>
        Task<LoginResponseDto> LoginAsync(LoginInput loginInput);

        /// <summary>
        /// 修改用户信息
        /// </summary>
        /// <param name="updateUserInfoInput"></param>
        /// <returns></returns>
        Task UpdateUserInfoAsync(UpdateUserInfoInput updateUserInfoInput);

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="updatePasswordInput"></param>
        /// <returns></returns>
        Task UpdatePasswordAsync(UpdatePasswordInput updatePasswordInput);
    }
}
