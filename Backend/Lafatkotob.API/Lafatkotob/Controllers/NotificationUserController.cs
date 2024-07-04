using Lafatkotob.Services.NotificationUserService;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lafatkotob.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class NotificationUserController : Controller
    {
        private readonly INotificationUserService _notificationUserService;
        public NotificationUserController(INotificationUserService notificationUserService)
        {
            _notificationUserService = notificationUserService;
        }
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllNotificationUsers()
        {
            var notificationUsers = await _notificationUserService.GetAll();
            if(notificationUsers == null) return BadRequest();
            return Ok(notificationUsers);
        }
        [HttpGet("getbyid")]
        public async Task<IActionResult> GetNotificationUserById(int notificationUserId)
        {
            var notificationUser = await _notificationUserService.GetById(notificationUserId);
            if (notificationUser == null) return BadRequest();
            return Ok(notificationUser);
        }
        [HttpGet("getbyUserid")]
        public async Task<IActionResult> GetAllNotificationByUserId(string userid)
        {
            var notificationUsers = await _notificationUserService.GetByUserId(userid);
            if (notificationUsers == null) return BadRequest();
            return Ok(notificationUsers);
        }
        [HttpGet("getbyUseridFalse")]
        public async Task<IActionResult> GetFalseNotificationByUserId(string userid)
        {
            var notificationUsers = await _notificationUserService.GetByUserIdFalse(userid);
            if (notificationUsers == null) return BadRequest();
            return Ok(notificationUsers);
        }

        [HttpGet("getbyUseridFive")]
        public async Task<IActionResult> GetFiveNotificationByUserId(string userid)
        {
            var notificationUsers = await _notificationUserService.GetByUserIdFive(userid);
            if (notificationUsers == null) return BadRequest();
            return Ok(notificationUsers);
        }
        [HttpPost("post")]
        public async Task<IActionResult> PostNotificationUser(NotificationUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _notificationUserService.Post(model);
            return Ok();
        }
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteNotificationUser(int notificationUserId)
        {
            var notificationUser = await _notificationUserService.Delete(notificationUserId);
            if (notificationUser == null) return BadRequest();
            return Ok(notificationUser);
        }
        [HttpPut("update")]
        public async Task<IActionResult> UpdateNotificationUser(NotificationUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _notificationUserService.Update(model);
            return Ok();
        }
        
    }
}
