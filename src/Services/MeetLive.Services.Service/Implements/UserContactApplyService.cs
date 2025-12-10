using MeetLive.Services.Domain.Entities;
using MeetLive.Services.Domain.IRepository;
using MeetLive.Services.IService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetLive.Services.Service.Implements
{
    public class UserContactApplyService : IUserContactApplyService
    {
        private readonly IUserContactApplyRepository _contactApplyRepository;

        public UserContactApplyService(IUserContactApplyRepository contactApplyRepository)
        {
            _contactApplyRepository = contactApplyRepository;
        }

        public async Task SaveUserContactAsync(UserContactApply userContactApply)
        {
            var apply = await _contactApplyRepository.SelectByApplyUserIdAndReceiveUserIdAsync(userContactApply.ApplyUserId, userContactApply.ReceiveUserId);
            if (apply != null)
            {

            }
        }
    }
}
