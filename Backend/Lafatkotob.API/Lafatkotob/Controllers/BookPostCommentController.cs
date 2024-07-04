using Lafatkotob.Services.BookPostCommentService;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lafatkotob.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookPostCommentController : Controller
    {
        private readonly IBookPostCommentServices _bookPostCommentService;
        public BookPostCommentController(IBookPostCommentServices bookPostCommentService)
        {
            _bookPostCommentService = bookPostCommentService;
        }
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllBookPostComments()
        {
            var bookPostComments = await _bookPostCommentService.GetAll();
            if(bookPostComments == null) return BadRequest();
            return Ok(bookPostComments);
        }
        [HttpGet("getbyid")]
        public async Task<IActionResult> GetBookPostCommentById(int bookPostCommentId)
        {
            var bookPostComment = await _bookPostCommentService.GetById(bookPostCommentId);
            if (bookPostComment == null) return BadRequest();
            return Ok(bookPostComment);
        }
        [HttpPost("post")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public async Task<IActionResult> PostBookPostComment(BookPostCommentModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _bookPostCommentService.Post(model);
            return Ok();
        }
        [HttpDelete("delete")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public async Task<IActionResult> DeleteBookPostComment(int bookPostCommentId)
        {
            var bookPostComment = await _bookPostCommentService.Delete(bookPostCommentId);
            if (bookPostComment == null) return BadRequest();
            return Ok(bookPostComment);
        }
        [HttpPut("update")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public async Task<IActionResult> UpdateBookPostComment(BookPostCommentModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _bookPostCommentService.Update(model);
            return Ok();
        }
       
    }
}
