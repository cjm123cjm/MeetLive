using Microsoft.AspNetCore.Http;

namespace MeetLive.Services.IService.Dtos.Inputs
{
    /// <summary>
    /// 修改用户信息输入参数
    /// </summary>
    public class UpdateUserInfoInput
    {
        /// <summary>
        /// 头像
        /// </summary>
        public IFormFile? Avatar { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; } = null!;
        /// <summary>
        /// 性别
        /// </summary>
        public int Sex { get; set; }
    }
}
