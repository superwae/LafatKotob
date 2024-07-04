using Lafatkotob.Services.BooksInWishlistsService;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lafatkotob.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksInWishlistsController : Controller
    {
        private readonly IBooksInWishlistsService _booksInWishlistsService;
        public BooksInWishlistsController(IBooksInWishlistsService booksInWishlistsService)
        {
            _booksInWishlistsService = booksInWishlistsService;
        }
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllBooksInWishlists()
        {
            var booksInWishlists = await _booksInWishlistsService.GetAll();
            if(booksInWishlists == null) return BadRequest();
            return Ok(booksInWishlists);
        }
        [HttpGet("getbyid")]
        public async Task<IActionResult> GetBooksInWishlistsById(int booksInWishlistsId)
        {
            var booksInWishlists = await _booksInWishlistsService.GetById(booksInWishlistsId);
            if (booksInWishlists == null) return BadRequest();
            return Ok(booksInWishlists);
        }

        [HttpPost("post")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public async Task<IActionResult> PostBooksInWishlists(BookInWishlistsModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _booksInWishlistsService.Post(model);
            return Ok();
        }
        [HttpPost("postall")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PostAllBooksInWishlists(BookInWishlistsModel model, string UserId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var BooksInWishlist=await _booksInWishlistsService.PostAll(model, UserId);
            return Ok(BooksInWishlist.Data);
        }

        [HttpDelete("delete")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public async Task<IActionResult> DeleteBooksInWishlists(int booksInWishlistsId)
        {
            var booksInWishlists = await _booksInWishlistsService.Delete(booksInWishlistsId);
            if (booksInWishlists == null) return BadRequest();
            return Ok(booksInWishlists);
        }
        [HttpPut("update")]

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public async Task<IActionResult> UpdateBooksInWishlists(BookInWishlistsModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _booksInWishlistsService.Update(model);
            return Ok();
        }


      
    }
}
