using Lafatkotob.Entities;
using Lafatkotob.Services.UserEventService;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lafatkotob.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserEventController : Controller
    {
        private readonly IUserEventService _userEventService;
        public UserEventController(IUserEventService userEventService)
        {
            _userEventService = userEventService;
        }
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllUserEvents()
        {
            var userEvents = await _userEventService.GetAll();
            if(userEvents == null) return BadRequest();
            return Ok(userEvents);
        }
        [HttpGet("getbyid")]
        public async Task<IActionResult> GetUserEventById(int userEventId)
        {
            var userEvent = await _userEventService.GetById(userEventId);
            if (userEvent == null) return BadRequest();
            return Ok(userEvent);
        }

        [HttpGet("GetUserEvent")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Premium,Admin,User")]
        public async Task<IActionResult> GetUserEvent(string UserId,int EventId)
        {

            var userEvent = await _userEventService.GetUserEvent(UserId, EventId);
            if (userEvent == null) return BadRequest();
            return Ok(userEvent);
        }


        [HttpPost("PostUserEvent")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,Roles = "Premium,Admin,User")]
        public async Task<IActionResult> PostUserEvent(UserEventModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _userEventService.Post(model);
            return Ok();
        }
        [HttpDelete("DeleteUserEvent")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteUserEvent(int userEventId)
        {
            var userEvent = await _userEventService.Delete(userEventId);
            if (userEvent == null) return BadRequest();
            return Ok(userEvent);
        }
        [HttpDelete("DeleteUserEventByUserId")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteUserEventByUserId(int EventId, string UserId)
        {
            var userEvent = await _userEventService.DeleteUserEventByUserId(EventId, UserId);
            if (userEvent == null) return BadRequest();
            return Ok(userEvent);
        }

        [HttpPut("update")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Premium,Admin")]
        public async Task<IActionResult> UpdateUserEvent(UserEventModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _userEventService.Update(model);
            return Ok();
        }
        
    }
}
