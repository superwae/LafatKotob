using Lafatkotob.Entities;
using Lafatkotob.Services;
using Lafatkotob.Services.ConversationService;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lafatkotob.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConversationController : Controller
    {
       
        private readonly IConversationService _conversationService;
        public ConversationController(IConversationService conversationService)
        {
                _conversationService = conversationService;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAllConversations()
        {
            var conversations = await _conversationService.GetAll();
            if(conversations == null) return BadRequest();
            return Ok(conversations);
        }

        [HttpGet("getbyid")]
        public async Task<IActionResult> GetConversationById(int conversationId)
        {
            var conversation = await _conversationService.GetById(conversationId);
            if (conversation == null) return BadRequest();
            return Ok(conversation);
        }

        [HttpPost("post")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public async Task<IActionResult> PostConversation(ConversationModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _conversationService.Post(model);
            return Ok();
        }

        [HttpDelete("delete")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public async Task<IActionResult> DeleteConversation(int conversationId)
        {
            var conversation = await _conversationService.Delete(conversationId);
            if (conversation == null) return BadRequest();
            return Ok(conversation);
        }

        [HttpPut("update")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateConversation(ConversationModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _conversationService.Update(model);
            return Ok();
        }

        [HttpGet("getconversationsforuser")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetConversationsForUser(string userId)
        {
            var conversations = await _conversationService.GetConversationsForUser(userId);
            if (conversations.Success == false) return Ok(conversations.Message);
            return Ok(conversations.Data);
        }



        [HttpGet("{conversationId}/messages")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<ServiceResponse<List<Message>>>> GetAllMessagesByConversationId(int conversationId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var messages = await _conversationService.GetAllMessagesByConversationIdAsync(conversationId, pageNumber, pageSize);
            if (messages.Success == false) return Ok(messages.Message);
            return Ok(messages.Data);
        }

        [HttpPost("NewConversation")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> NewConversation(ConversationWithIdsModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var response = await _conversationService.PostNewConversation(model);

            return Ok(response.Data);
        }
        [HttpGet("ConversationCountWithUnreadMessages")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ConversationCountWithUnreadMessages(string userId)
        {
            var response = await _conversationService.ConversationCountWithUnreadMessages(userId);
            return Ok(response.Data);
        }


    }
}
