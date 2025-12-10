using MeetLive.Services.Domain.Entities;
using MeetLive.Services.IService.Dtos;
using MeetLive.Services.IService.Dtos.Inputs;
using MeetLive.Services.IService.Enums;
using MeetLive.Services.IService.Interfaces;
using MeetLive.Services.WebSocket.Message;
using Microsoft.AspNetCore.Mvc;

namespace MeetLive.Services.Api.Controllers
{
    /// <summary>
    /// 联系人
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserContactController : BaseController
    {
        private readonly IUserContactService _userContactService;
        private readonly IMessageHandler _messageHandler;

        public UserContactController(
            IUserContactService userContactService,
            IMessageHandler messageHandler)
        {
            _userContactService = userContactService;
            _messageHandler = messageHandler;
        }

        /// <summary>
        /// 搜索联系人(根据邮箱来搜索)
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResponseDto> SearchContact(string email)
        {
            var data = await _userContactService.SearchContactAsync(email);

            return new ResponseDto(data);
        }

        /// <summary>
        /// 申请好友
        /// </summary>
        /// <param name="receiveUserId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> ContactApply(long receiveUserId)
        {
            UserContactApply userContactApply = new UserContactApply
            {
                ApplyUserId = LoginUserId,
                ReceiveUserId = receiveUserId
            };

            var data = await _userContactService.SaveUserContactAsync(userContactApply);
            if (data == 0)
            {
                //发送消息给对方
                MessageSendDto<object> messageSendDto = new MessageSendDto<object>
                {
                    MessageSendType = MessageSendTypeEnum.USER,
                    ReceiveUserId = receiveUserId.ToString(),
                    SendUserId = LoginUserId.ToString(),
                    MessageType = MessageTypeEnum.USER_CONTACT_APPLY
                };
                _messageHandler.SendMessage(messageSendDto);
            }

            return new ResponseDto(data);
        }

        /// <summary>
        /// 处理好友请求
        /// </summary>
        /// <param name="applyAsyncInput"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> DealWithApply(DealWithApplyAsyncInput applyAsyncInput)
        {
            await _userContactService.DealWithApplyAsync(applyAsyncInput);

            //发送消息，告诉申请人已处理
            _messageHandler.SendMessage(new MessageSendDto<object>
            {
                MessageSendType = MessageSendTypeEnum.USER,
                MessageType = MessageTypeEnum.USER_CONTACT_DEAL_WITH,
                ReceiveUserId = applyAsyncInput.ApplyUserId.ToString(),
                MessageContent = applyAsyncInput.Status,
                SendUserNickName = NickName
            });

            return new ResponseDto();
        }

        /// <summary>
        /// 加载联系人
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResponseDto> LoadContactUser()
        {
            var data = await _userContactService.LoadContactApplyAsync();

            return new ResponseDto(data);
        }

        /// <summary>
        /// 加载申请信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResponseDto> LoadContactApply()
        {
            var data = await _userContactService.LoadContactApplyAsync();

            return new ResponseDto(data);
        }

        /// <summary>
        /// 有多少条申请
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResponseDto> LoadContactApplyDealWithCount()
        {
            var data = await _userContactService.LoadContactApplyDealWithCountAsync();

            return new ResponseDto(data);
        }

        /// <summary>
        /// 删除/拉黑联系人
        /// </summary>
        /// <param name="deleteContactInput"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> DeleteContact(DeleteContactInput deleteContactInput)
        {
            await _userContactService.DeleteContactAsync(deleteContactInput);

            return new ResponseDto();
        }
    }
}
