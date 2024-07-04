using Lafatkotob.Services.WishedBookService;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lafatkotob.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class WishedBookController : Controller
    {
        private readonly IWishedBookService _wishedBookService;
        public WishedBookController(IWishedBookService wishedBookService)
        {
            _wishedBookService = wishedBookService;
        }
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllWishedBooks()
        {
            var wishedBooks = await _wishedBookService.GetAll();
            if(wishedBooks == null) return BadRequest();
            return Ok(wishedBooks);
        }
        [HttpGet("getbyid")]
        public async Task<IActionResult> GetWishedBookById(int wishedBookId)
        {
            var wishedBook = await _wishedBookService.GetById(wishedBookId);
            if (wishedBook == null) return BadRequest();
            return Ok(wishedBook);
        }
        [HttpPost("post")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PostWishedBook(WishedBookModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _wishedBookService.Post(model);
            return Ok();
        }
        [HttpDelete("delete")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteWishedBook(int wishedBookId)
        {
            var wishedBook = await _wishedBookService.Delete(wishedBookId);
            if (wishedBook == null) return BadRequest();
            return Ok(wishedBook);
        }
        [HttpPut("update")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateWishedBook(WishedBookModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _wishedBookService.Update(model);
            return Ok();
        }
        
    }
}
