using Lafatkotob.Services.ConversationsUserService;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lafatkotob.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConversationsUserController : Controller
    {
        private readonly IConversationsUserService _conversationsUserService;
        public ConversationsUserController(IConversationsUserService conversationsUserService)
        {
            _conversationsUserService = conversationsUserService;
        }
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllConversationsUsers()
        {
            var conversationsUsers = await _conversationsUserService.GetAll();
            if(conversationsUsers == null) return BadRequest();
            return Ok(conversationsUsers);
        }
        [HttpGet("getbyid")]
        public async Task<IActionResult> GetConversationsUserById(int conversationsUserId)
        {
            var conversationsUser = await _conversationsUserService.GetById(conversationsUserId);
            if (conversationsUser == null) return BadRequest();
            return Ok(conversationsUser);
        }

        [HttpPost("postWithUsers")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PostConversationsUser(CreateConversationModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var response =await _conversationsUserService.PostWithUsers(model);
            return Ok(response.Data);
        }

        [HttpDelete("delete")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteConversationsUser(int conversationsUserId)
        {
            var conversationsUser = await _conversationsUserService.Delete(conversationsUserId);
            if (conversationsUser == null) return BadRequest();
            return Ok(conversationsUser);
        }
        [HttpPut("update")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateConversationsUser(ConversationsUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _conversationsUserService.Update(model);
            return Ok();
        }
       
    }
}
