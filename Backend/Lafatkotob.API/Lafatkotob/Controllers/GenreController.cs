using Lafatkotob.Services.GenreService;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lafatkotob.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenreController : Controller
    {
        private readonly IGenreService _genreService;
        public GenreController(IGenreService genreService)
        {
            _genreService = genreService;
        }
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllGenres()
        {
            var genres = await _genreService.GetAll();
            if(genres == null) return BadRequest();
            return Ok(genres);
        }
        [HttpGet("getbyid")]
        public async Task<IActionResult> GetGenreById(int genreId)
        {
            var genre = await _genreService.GetById(genreId);
            if (genre == null) return BadRequest();
            return Ok(genre);
        }
        [HttpPost("post")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,Roles ="Admin")]

        public async Task<IActionResult> PostGenre(GenreModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _genreService.Post(model);
            return Ok();
        }
        [HttpDelete("delete")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]

        public async Task<IActionResult> DeleteGenre(int genreId)
        {
            var genre = await _genreService.Delete(genreId);
            if (genre == null) return BadRequest();
            return Ok(genre);
        }
        [HttpPut("update")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]

        public async Task<IActionResult> UpdateGenre(GenreModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _genreService.Update(model);
            return Ok();
        }
       
    }
}
