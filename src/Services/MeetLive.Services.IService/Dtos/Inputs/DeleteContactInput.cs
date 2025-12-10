using System.ComponentModel.DataAnnotations;

namespace MeetLive.Services.IService.Dtos.Inputs
{
    /// <summary>
    /// 删除联系人输入参数
    /// </summary>
    public class DeleteContactInput
    {
        /// <summary>
        /// 联系人id
        /// </summary>
        public long ContactId { get; set; }
        /// <summary>
        /// 状态:2-删除,3-拉黑
        /// </summary>
        [Range(2, 3)]
        public int Status { get; set; }
    }
}
