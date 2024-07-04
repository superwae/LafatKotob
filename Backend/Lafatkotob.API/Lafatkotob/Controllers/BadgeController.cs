using Lafatkotob.Services;
using Lafatkotob.Services.BadgeService;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Lafatkotob.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] 
    public class BadgeController : Controller
    {
      
           private readonly IBadgeService _badgeService;
        public BadgeController(IBadgeService badgeService)
        {
                _badgeService = badgeService;
        }
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllBadges()
        {
            var badges = await _badgeService.GetAll();
            if(badges == null) return BadRequest();
            return Ok(badges);
        }
        [HttpGet("getbyid")]
        public async Task<IActionResult> GetBadgeById(int badgeId)
        {
            var badge = await _badgeService.GetById(badgeId);
            if (badge == null) return BadRequest();
            return Ok(badge);
        }

        [HttpGet("GetAllBadgesByUser")]
        public async Task<IActionResult> GetAllBadgesByUser(string userId)
        {
                var badge = await _badgeService.GetAllBadgesByUser(userId);
                if (badge == null) return NotFound("User not found.");
                return Ok(badge.Data);
            
        }

        [HttpPost("post")]
        public async Task<IActionResult> PostBadge(BadgeModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _badgeService.Post(model);
            return Ok();
        }
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteBadge(int badgeId)
        {
            var badge = await _badgeService.Delete(badgeId);
            if (badge == null) return BadRequest();
            return Ok(badge);
        }
        [HttpPut("update")]
        public async Task<IActionResult> UpdateBadge(BadgeModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _badgeService.Update(model);
            return Ok();
        }



           
        
    }
}
