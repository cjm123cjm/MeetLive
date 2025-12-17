using MeetLive.Services.IService.Dtos;
using MeetLive.Services.IService.Dtos.Inputs;
using MeetLive.Services.IService.Interfaces;
using MeetLive.Services.IService.Options;
using MeetLive.Services.WebSocket.Message;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MeetLive.Services.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ChatMessageController : ControllerBase
    {
        private readonly IMeetingChatMessageService _chatMessageService;
        private readonly IMessageHandler _messageHandler;
        private readonly IOptions<FolderPath> _options;
        private readonly ILogger<ChatMessageController> _logger;

        public ChatMessageController(
            IMeetingChatMessageService chatMessageService,
            IMessageHandler messageHandler,
            IOptions<FolderPath> options,
            ILogger<ChatMessageController> logger)
        {
            _chatMessageService = chatMessageService;
            _messageHandler = messageHandler;
            _options = options;
            _logger = logger;
        }

        /// <summary>
        /// 加载聊天消息
        /// </summary>
        /// <param name="chatMessageQuery"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResponseDto> LoadMessages(ChatMessageQuery chatMessageQuery)
        {
            var data = await _chatMessageService.LoadChatMessagesAsync(chatMessageQuery);

            return new ResponseDto(data);
        }

        /// <summary>
        /// 加载历史聊天消息
        /// </summary>
        /// <param name="meetingId">会议id</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResponseDto> LoadHistoryMessages(ChatMessageQuery chatMessageQuery)
        {
            var data = await _chatMessageService.LoadHistoryMessagesAsync(chatMessageQuery);

            return new ResponseDto(data);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="sendMessageInput"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> SendMessage(SendMessageInput sendMessageInput)
        {
            var data = await _chatMessageService.SendMessageAsync(sendMessageInput);

            if (data.Item2.Count > 0)
            {
                foreach (var item in data.Item2)
                {
                    _messageHandler.SendMessage(item);
                }
            }
            return new ResponseDto(data.Item1);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="uploadFileInput"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> UploadFile(UploadFileInput uploadFileInput)
        {
            var data = await _chatMessageService.UploadFileAsync(uploadFileInput);
            if (data != null)
            {
                _messageHandler.SendMessage(data);
            }

            return new ResponseDto();
        }

        /// <summary>
        /// 预览文件
        /// </summary>
        /// <param name="resourceInput"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetResource(ResourceInput resourceInput)
        {
            //xxx/年月日/
            string month = resourceInput.SendTime.ToString("yyyyMM");

            string filePath = "";
            string fileName = resourceInput.MessageId + "";
            if (resourceInput.Thumbnail)
            {
                fileName += "_thumb.jpg";
                filePath = Path.Combine(_options.Value.PhysicalPath, month, resourceInput.MeetingId.ToString(), fileName);
            }
            else
            {
                if (resourceInput.FileType == 0)
                {
                    fileName += ".jpg";
                }
                else if (resourceInput.FileType == 1)
                {
                    fileName += ".mp4";
                }
                else
                {

                }
                filePath = Path.Combine(_options.Value.PhysicalPath, month, resourceInput.MeetingId.ToString(), fileName);
            }

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            // 获取Range请求头
            var rangeHeader = Request.Headers["Range"].FirstOrDefault();

            // 根据文件类型设置响应头
            var fileInfo = new FileInfo(filePath);

            if (resourceInput.Thumbnail || resourceInput.FileType == 0)
            {
                // 图片文件 - 直接返回完整文件
                var imageBytes = await System.IO.File.ReadAllBytesAsync(filePath);

                Response.ContentType = "image/jpeg";
                Response.ContentLength = imageBytes.Length;
                await Response.Body.WriteAsync(imageBytes, 0, imageBytes.Length);
            }
            else if (resourceInput.FileType == 1)
            {
                // 视频文件 - 支持断点续传
                var contentType = "video/mp4";
                await WriteVideoFileWithRangeSupport(filePath, rangeHeader, contentType);
            }

            // 直接返回文件流，不需要ResponseDto
            return Ok();
        }

        /// <summary>
        /// 支持Range请求的视频文件输出
        /// </summary>
        [NonAction]
        private async Task WriteVideoFileWithRangeSupport(string filePath, string rangeHeader, string contentType)
        {
            var fileInfo = new FileInfo(filePath);
            long fileSize = fileInfo.Length;

            if (string.IsNullOrEmpty(rangeHeader))
            {
                // 第一次请求，返回完整文件信息
                Response.ContentType = contentType;
                Response.ContentLength = fileSize;
                Response.Headers.Add("Accept-Ranges", "bytes");

                await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                await fileStream.CopyToAsync(Response.Body);
            }
            else
            {
                // 处理Range请求
                var range = rangeHeader.Replace("bytes=", "");
                var ranges = range.Split('-');

                long start = 0;
                long end = fileSize - 1;

                if (ranges.Length > 0 && !string.IsNullOrEmpty(ranges[0]))
                    start = long.Parse(ranges[0]);

                if (ranges.Length > 1 && !string.IsNullOrEmpty(ranges[1]))
                    end = long.Parse(ranges[1]);

                long contentLength = end - start + 1;

                // 设置部分内容响应头
                Response.StatusCode = 206; // Partial Content
                Response.ContentType = contentType;
                Response.ContentLength = contentLength;
                Response.Headers.Add("Content-Range", $"bytes {start}-{end}/{fileSize}");
                Response.Headers.Add("Accept-Ranges", "bytes");

                // 读取指定范围的数据
                await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                fileStream.Seek(start, SeekOrigin.Begin);

                var buffer = new byte[81920]; // 80KB缓冲区
                long bytesRemaining = contentLength;

                while (bytesRemaining > 0)
                {
                    int bytesRead = await fileStream.ReadAsync(buffer, 0,
                        (int)Math.Min(buffer.Length, bytesRemaining));

                    if (bytesRead == 0)
                        break;

                    await Response.Body.WriteAsync(buffer, 0, bytesRead);
                    bytesRemaining -= bytesRead;
                }
            }
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="resourceInput"></param>
        /// <returns></returns>
        [HttpGet("download")]
        public async Task<IActionResult> DownloadFile(ResourceInput resourceInput)
        {
            try
            {
                // 构建文件路径（复用预览接口的逻辑）
                string month = resourceInput.SendTime.ToString("yyyyMM");
                string filePath = "";
                string fileName = resourceInput.MessageId + "";
                string originalFileName = ""; // 原始文件名，用于下载时显示

                // 根据业务需求，这里可能需要从数据库获取原始文件名
                originalFileName = "download";

                if (resourceInput.Thumbnail)
                {
                    fileName += "_thumb.jpg";
                    filePath = Path.Combine(_options.Value.PhysicalPath, month,
                        resourceInput.MeetingId.ToString(), fileName);
                    originalFileName += "_thumb.jpg";
                }
                else
                {
                    if (resourceInput.FileType == 0)
                    {
                        fileName += ".jpg";
                        originalFileName += ".jpg";
                    }
                    else if (resourceInput.FileType == 1)
                    {
                        fileName += ".mp4";
                        originalFileName += ".mp4";
                    }
                    else
                    {
                        return BadRequest(new { Code = 400, Message = "不支持的文件类型" });
                    }
                    filePath = Path.Combine(_options.Value.PhysicalPath, month,
                        resourceInput.MeetingId.ToString(), fileName);
                }

                // 检查文件是否存在
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { Code = 404, Message = "文件不存在" });
                }

                // 获取文件信息
                var file = new FileInfo(filePath);

                // 设置下载响应头
                var contentType = GetContentType(file.Extension);
                var encodedFileName = Uri.EscapeDataString(originalFileName); // URL编码文件名

                Response.Headers.Add("Content-Disposition",
                    $"attachment; filename=\"{originalFileName}\"; filename*=UTF-8''{encodedFileName}");
                Response.ContentType = contentType;
                Response.ContentLength = file.Length;

                // 添加缓存控制头（可选）
                Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                Response.Headers.Add("Pragma", "no-cache");
                Response.Headers.Add("Expires", "0");

                // 流式传输文件
                await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var buffer = new byte[81920]; // 80KB缓冲区

                int bytesRead;
                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await Response.Body.WriteAsync(buffer, 0, bytesRead);

                    // 确保及时刷新缓冲区
                    await Response.Body.FlushAsync();
                }

                return new EmptyResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文件下载失败");
                return StatusCode(500, new { Code = 500, Message = $"文件下载失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 获取文件内容类型
        /// </summary>
        [NonAction]
        private string GetContentType(string extension)
        {
            return extension.ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".mp4" => "video/mp4",
                ".avi" => "video/x-msvideo",
                ".mov" => "video/quicktime",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                _ => "application/octet-stream"
            };
        }

    }
}
