namespace MeetLive.Services.IService.Dtos
{
    /// <summary>
    /// 统一返回结果
    /// </summary>
    public class ResponseDto
    {
        /// <summary>
        /// 结果
        /// </summary>
        public object? Result { get; set; }
        /// <summary>
        /// 状态码
        /// </summary>
        public int Code { get; set; } = 200;
        /// <summary>
        /// 输出消息
        /// </summary>
        public string Message { get; set; } = string.Empty;
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; } = true;

        public ResponseDto()
        {

        }
        public ResponseDto(object? Result)
        {
            this.Result = Result;
        }

        public ResponseDto(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }
    }
}
