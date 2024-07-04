using Lafatkotob.Services.BookGenreService;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lafatkotob.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookGenreController : Controller
    {
        private readonly IBookGenreServices _bookGenreService;
        public BookGenreController(IBookGenreServices bookGenreService)
        {
            _bookGenreService = bookGenreService;
        }
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllBookGenres()
        {
            var bookGenres = await _bookGenreService.GetAll();
            if(bookGenres == null) return BadRequest();
            return Ok(bookGenres);
        }
        [HttpGet("getbyid")]
        public async Task<IActionResult> GetBookGenreById(int bookGenreId)
        {
            var bookGenre = await _bookGenreService.GetById(bookGenreId);
            if (bookGenre == null) return BadRequest();
            return Ok(bookGenre);
        }
        [HttpPost("post")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public async Task<IActionResult> PostBookGenre(BookGenreModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _bookGenreService.Post(model);
            return Ok();
        }
        [HttpDelete("delete")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public async Task<IActionResult> DeleteBookGenre(int bookGenreId)
        {
            var bookGenre = await _bookGenreService.Delete(bookGenreId);
            if (bookGenre == null) return BadRequest();
            return Ok(bookGenre);
        }
        [HttpPut("update")]

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public async Task<IActionResult> UpdateBookGenre(BookGenreModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _bookGenreService.Update(model);
            return Ok();
        }
      
    }
}
