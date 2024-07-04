using Lafatkotob.Services.BookPostLikeServices;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lafatkotob.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookPostLikeController : Controller
    {
        private readonly IBookPostLikeService _bookPostLikeService;
        public BookPostLikeController(IBookPostLikeService bookPostLikeService)
        {
            _bookPostLikeService = bookPostLikeService;
        }
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllBookPostLikes()
        {
            var bookPostLikes = await _bookPostLikeService.GetAll();
            if(bookPostLikes == null) return BadRequest();
            return Ok(bookPostLikes);
        }
        [HttpGet("getbyid")]
        public async Task<IActionResult> GetBookPostLikeById([FromQuery] string userId, [FromQuery] int bookId)
        {
            var model = new AddBookPostLikeModel { UserId = userId, BookId = bookId };
            var bookPostLike = await _bookPostLikeService.GetById(model);
            if (bookPostLike == null) return BadRequest();
            return Ok(bookPostLike);
        }

      


        [HttpPost("post")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public async Task<IActionResult> PostBookPostLike(AddBookPostLikeModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _bookPostLikeService.Post(model);
            return Ok();
        }
        [HttpDelete("delete")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public async Task<IActionResult> DeleteBookPostLike([FromQuery] string userId, [FromQuery] int bookId)
        {
            var model = new AddBookPostLikeModel { UserId = userId, BookId = bookId };
            var bookPostLike = await _bookPostLikeService.Delete(model);
            if (bookPostLike == null) return BadRequest();
            return Ok(bookPostLike);
        }
        [HttpPut("update")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public async Task<IActionResult> UpdateBookPostLike(BookPostLikeModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _bookPostLikeService.Update(model);
            return Ok();
        }

        [HttpGet("checkBulkLikes")]
        public async Task<IActionResult> CheckBulkLikes(string userId, [FromQuery(Name = "bookIds")] List<int> bookIds)
        {
            var results = await _bookPostLikeService.CheckBulkLikes(userId, bookIds);
            if (results == null) return BadRequest();
            return Ok(results.Data);
        }





    }
}
