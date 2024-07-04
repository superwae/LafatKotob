using Lafatkotob.Services.NotificationService;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lafatkotob.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        [HttpGet("getall")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetAllNotifications()
        {
            var notifications = await _notificationService.GetAll();
            if(notifications == null) return BadRequest();
            return Ok(notifications);
        }
        [HttpGet("getbyid")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetNotificationById([FromQuery]  int id)
        {
            var notification = await _notificationService.GetById(id);
            if (notification == null) return BadRequest();
            return Ok(notification);
        }
        [HttpPost("post")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PostNotification(NotificationModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _notificationService.Post(model);
            return Ok();
        }
        [HttpDelete("delete")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {
            var notification = await _notificationService.Delete(notificationId);
            if (notification == null) return BadRequest();
            return Ok(notification);
        }
        [HttpPut("update")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateNotification([FromBody] NotificationModel model)
        {
            var response = await _notificationService.Update(model);
            if (response.Success)
            {
                return Ok(response.Data);
            }
            return BadRequest(response.Message);
        }
       
    }
}
