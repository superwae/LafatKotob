using Lafatkotob.Services.UserBadgeService;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lafatkotob.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class UserBadgeController : Controller
    {
        private readonly IUserBadgeService _userBadgeService;
        public UserBadgeController(IUserBadgeService userBadgeService)
        {
            _userBadgeService = userBadgeService;
        }
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllUserBadges()
        {
            var userBadges = await _userBadgeService.GetAll();
            if(userBadges == null) return BadRequest();
            return Ok(userBadges);
        }
        [HttpGet("getbyid")]
        public async Task<IActionResult> GetUserBadgeById(int userBadgeId)
        {
            var userBadge = await _userBadgeService.GetById(userBadgeId);
            if (userBadge == null) return BadRequest();
            return Ok(userBadge);
        }
        [HttpPost("post")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PostUserBadge(UserBadgeModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _userBadgeService.Post(model);
            return Ok();
        }
        [HttpDelete("delete")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteUserBadge(int userBadgeId)
        {
            var userBadge = await _userBadgeService.Delete(userBadgeId);
            if (userBadge == null) return BadRequest();
            return Ok(userBadge);
        }
        [HttpPut("update")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateUserBadge(UserBadgeModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _userBadgeService.Update(model);
            return Ok();
        }
        
    }
}
