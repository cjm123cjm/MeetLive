using System.ComponentModel.DataAnnotations;

namespace MeetLive.Services.IService.Dtos.Inputs
{
    /// <summary>
    /// 处理申请消息输入参数
    /// </summary>
    public class DealWithApplyAsyncInput
    {
        /// <summary>
        /// 申请人id
        /// </summary>
        public long ApplyUserId { get; set; }
        /// <summary>
        /// 1-同意,2-拒绝,3-拉黑
        /// </summary>
        [Range(1, 3, ErrorMessage = "状态只能是1、2、3")]
        public int Status { get; set; }
    }
}
