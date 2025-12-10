using MeetLive.Services.Common.RedisUtil;
using MeetLive.Services.IService.Dtos.Outputs;

namespace MeetLive.Services.Service
{
    /// <summary>
    /// reids组件
    /// </summary>
    public class RedisComponent
    {
        /// <summary>
        /// 获取会议所有成员
        /// </summary>
        /// <param name="meetingId">会议id</param>
        /// <returns></returns>
        public static List<MeetingMemberDto> GetMeetingMemberList(string meetingId)
        {
            return CacheManager.HashValues<MeetingMemberDto>(RedisKeyPrefix.Redis_Key_Meeting_Root + meetingId).ToList();
        }

        /// <summary>
        /// 获取会议某一个成员
        /// </summary>
        /// <param name="meetingId">会议id</param>
        /// <param name="userId">用户id</param>
        /// <returns></returns>
        public static MeetingMemberDto GetMeetingMember(string meetingId, string userId)
        {
            return CacheManager.HashGet<MeetingMemberDto>(RedisKeyPrefix.Redis_Key_Meeting_Root + meetingId, userId);
        }

        /// <summary>
        /// 设置会议人员
        /// </summary>
        /// <param name="meetingId"></param>
        /// <param name="userId"></param>
        /// <param name="meetingMemberDto"></param>
        public static void SetMeetingMember(string meetingId, string userId, MeetingMemberDto meetingMemberDto)
        {
            CacheManager.HashSet(RedisKeyPrefix.Redis_Key_Meeting_Root + meetingId.ToString(), userId.ToString(), meetingMemberDto);
        }

        /// <summary>
        /// 根据会议id清空人员
        /// </summary>
        /// <param name="meetingId">会议id</param>
        public static void RemoveAllMeetingMember(string meetingId)
        {
            CacheManager.HashKeyDelete(meetingId);
        }

        /// <summary>
        /// 保存用户信息
        /// </summary>
        /// <param name="userDto"></param>
        /// <param name="token"></param>
        public static void SetUserInfo(UserInfoDto userDto, string token)
        {
            CacheManager.Set(RedisKeyPrefix.Redis_Key_Ws_Token + token, userDto, TimeSpan.FromDays(1));
            CacheManager.Set(RedisKeyPrefix.Redis_Key_Ws_Token_UserId + userDto.UserId, userDto, TimeSpan.FromDays(1));
        }

        /// <summary>
        /// 更新用户消息
        /// </summary>
        /// <param name="userDto"></param>
        public static void UpdateUserInfoByUserId(UserInfoDto userDto)
        {
            CacheManager.Set(RedisKeyPrefix.Redis_Key_Ws_Token_UserId + userDto.UserId, userDto, TimeSpan.FromDays(1));
        }

        /// <summary>
        /// 根据token获取用户信息
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static UserInfoDto? GtetUserInfoByToken(string token)
        {
            return CacheManager.Get<UserInfoDto>(RedisKeyPrefix.Redis_Key_Ws_Token + token);
        }

        /// <summary>
        /// 根据用户id获取用户信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static UserInfoDto? GetUserInfoByUserId(string userId)
        {
            return CacheManager.Get<UserInfoDto>(RedisKeyPrefix.Redis_Key_Ws_Token_UserId + userId);
        }

        /// <summary>
        /// 添加邀请信息,5分钟过期
        /// </summary>
        /// <param name="meetingId"></param>
        /// <param name="userId"></param>
        public static void AddInviteInfo(string meetingId, string userId)
        {
            CacheManager.GetOrSet(RedisKeyPrefix.Redis_Key_Invite_Member + userId + meetingId, () => meetingId, TimeSpan.FromMinutes(5));
        }

        /// <summary>
        /// 获取邀请信息
        /// </summary>
        /// <param name="meetingId"></param>
        /// <param name="userId"></param>
        public static string GetInviteInfo(string meetingId, string userId)
        {
            return CacheManager.Get<string>(RedisKeyPrefix.Redis_Key_Invite_Member + userId + meetingId);
        }
    }
}
