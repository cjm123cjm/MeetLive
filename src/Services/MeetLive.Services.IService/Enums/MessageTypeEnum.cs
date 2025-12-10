namespace MeetLive.Services.IService.Enums
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MessageTypeEnum
    {
        /// <summary>
        /// 链接ws获取消息
        /// </summary>
        INIT,

        /// <summary>
        /// 加入房间
        /// </summary>
        ADD_MEETING_ROOM,

        /// <summary>
        /// 发送peer
        /// </summary>
        PEER,

        /// <summary>
        /// 退出房间
        /// </summary>
        EXIT_MEETING_ROOM,

        /// <summary>
        /// 结束会议
        /// </summary>
        FINIS_MEETING,

        /// <summary>
        /// 文本消息
        /// </summary>
        CHAT_TEXT_MESSAGE,

        /// <summary>
        /// 媒体消息
        /// </summary>
        CHAT_MEDIA_MESSAGE,

        /// <summary>
        /// 媒体消息更新
        /// </summary>
        CHAT_MEDIA_MEAASGE_UPDATE,

        /// <summary>
        /// 好友申请消息
        /// </summary>
        USER_CONTACT_APPLY,

        /// <summary>
        /// 邀请入会
        /// </summary>
        INVITE_MEMBER_MEETING,

        /// <summary>
        /// 强制下线
        /// </summary>
        FORCE_OFF_LIVE,

        /// <summary>
        /// 用户视频改变
        /// </summary>
        MEETING_USER_VIDEO_CHANGE,

        /// <summary>
        /// 处理好友请求
        /// </summary>
        USER_CONTACT_DEAL_WITH
    }
}
