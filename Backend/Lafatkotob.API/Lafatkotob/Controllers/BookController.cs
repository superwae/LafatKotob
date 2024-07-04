using Lafatkotob.Services.BookService;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Lafatkotob.Hubs;
using Azure.Core;

namespace Lafatkotob.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IHubContext<ChatHub> _hubContext;

        public BookController(IBookService bookService, IHubContext<ChatHub> hubContext)
        {
            _bookService = bookService;
            _hubContext = hubContext;
        }


        [HttpGet("getall")]
        public async Task<IActionResult> GetAllBooks(int pageNumber, int pageSize)
        {
            var books = await _bookService.GetAll( pageNumber,  pageSize);
            if (books == null) return BadRequest();
            return Ok(books);
        }
        [HttpGet("GetGenresForUser")]
        public async Task<IActionResult> GetGenresForUser(string userId)
        {
            var genres = await _bookService.GetGenresForUser(userId);
            if (genres == null) return BadRequest();
            return Ok(genres);
        }
        [HttpGet("GetAllWithUserInfo")]
        public async Task<IActionResult> GetAllWithUserInfo(int pageNumber, int pageSize)
        {
            var books = await _bookService.GetAllWithUserInfo(pageNumber, pageSize);
            if (books == null) return BadRequest();
            return Ok(books);
        }


        [HttpGet("{bookId}")]
        public async Task<IActionResult> GetBookById(int bookId)
        {
            var book = await _bookService.GetById(bookId);
            if (book == null) return NotFound();
            return Ok(book);
        }
        public class ReportBookRequest
        {
            public int Id { get; set; }
            public string UserId { get; set; }
        }
        [HttpPost("ReportBook")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ReportBook([FromBody] ReportBookRequest request)
        {
            var book = await _bookService.Report(request.Id, request.UserId);
            if (book == null) return BadRequest();
            return Ok(book);
        }

        [HttpGet("GetBooksByUserName")]
        public async Task<IActionResult> GetBooksByUserId([FromQuery] string username,  int pageNumber = 1,  int pageSize = 20)
        {
            var books = await _bookService.GetBooksByUserName(username, pageNumber, pageSize);
           
            return Ok(books);
        }


        [HttpPost("post")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PostBook([FromForm] RegisterBook model, IFormFile imageFile)
        {   
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (imageFile != null && !imageFile.ContentType.StartsWith("image/"))
            {
                return BadRequest("Only image files are allowed.");
            }
            var result = await _bookService.Post(model, imageFile);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }






        [HttpDelete("delete")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public async Task<IActionResult> DeleteBook(int bookId)
        {

            var book = await _bookService.Delete(bookId);
            if (book == null) return BadRequest();
            await _hubContext.Clients.All.SendAsync("BookDeleted", bookId);
            return Ok(book);
        }
        [HttpDelete("DeleteByAdmin")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]

        public async Task<IActionResult> DeleteByAdmin(int bookId)
        {

            var book = await _bookService.DeleteByAdmin(bookId);
            if (book == null) return BadRequest();
            await _hubContext.Clients.All.SendAsync("BookDeleted", bookId);
            return Ok(book);
        }

        [HttpPut("update/{bookId}"), DisableRequestSizeLimit]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateBook(int bookId, [FromForm] UpdateBookModel model, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();

            }
            if (imageFile != null && !imageFile.ContentType.StartsWith("image/"))
            {
                return BadRequest("Only image files are allowed.");
            }
            var result = await _bookService.Update(bookId, model, imageFile);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }


        [HttpGet("filter")]
        public async Task<IActionResult> GetBooksByGenres([FromQuery] List<int> genreIds,  int pageNumber = 1,int pageSize = 10)
        {
            if (genreIds == null || !genreIds.Any())
            {
                return BadRequest("No genre IDs provided.");
            }

            var books = await _bookService.GetBooksFilteredByGenres(genreIds, pageNumber, pageSize);
            if (books == null || !books.Success)
            {
                return BadRequest(books.Message);
            }

            return Ok(books.Data);
        }

        [HttpGet("{bookId}/genres")]
        public async Task<IActionResult> GetGenresForBook(int bookId)
        {
            if (bookId <= 0)
            {
                return BadRequest("Invalid book ID.");
            }

            var genres = await _bookService.GetGenresByBookId(bookId);

            if (genres.Data == null || !genres.Data.Any())
            {
                return NotFound(genres.Message);
            }

            return Ok(genres.Data);
        }
        [HttpGet("search")]
        public async Task<IActionResult> SearchBooks([FromQuery] string query, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query is required.");
            }

            var books = await _bookService.SearchBooks(query, pageNumber, pageSize);
            if (!books.Success)
            {
                return BadRequest(books.Message);
            }

            return Ok(books.Data);
        }

        [HttpPost("GetBooksByIsbn")]
        public async Task<IActionResult> GetBooksByIsbn([FromBody] List<string> isbn)
        {
            if (isbn == null || !isbn.Any())
            {
                return BadRequest("No ISBNs provided.");
            }

            var books = await _bookService.GetBooksByIsbn(isbn);
            if (books == null) return BadRequest(books.Message);

            return Ok(books.Data);
        }




        [HttpPut("UpdateBookWithGenres")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateBookWithGenres([FromForm] RegisterBookWithGenres model, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _bookService.UpdateBookWithGenres(model, imageFile);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            await _hubContext.Clients.All.SendAsync("BookUpdated", new
            {
                id = result.Data.Id,
                title = result.Data.Title,
                author = result.Data.Author,
                description = result.Data.Description,
                coverImage = result.Data.CoverImage,
                userId = result.Data.UserId,
                historyId = result.Data.HistoryId,
                publicationDate = result.Data.PublicationDate,
                isbn = result.Data.ISBN,
                pageCount = result.Data.PageCount,
                condition = result.Data.Condition,
                status = result.Data.Status,
                type = result.Data.Type,
                partnerUserId = result.Data.PartnerUserId,
                language = result.Data.Language,
                addedDate = result.Data.AddedDate,
            });
          
           
            return Ok();
        }


        [HttpPut("SellBook")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> SellBook([FromQuery] int BookId)
        {
            var result = await _bookService.SellBook(BookId);
            if (result == null) return BadRequest();
            await _hubContext.Clients.All.SendAsync("BookUpdated", new
            {
                id = result.Data.Id,
                title = result.Data.Title,
                author = result.Data.Author,
                description = result.Data.Description,
                coverImage = result.Data.CoverImage,
                userId = result.Data.UserId,
                historyId = result.Data.HistoryId,
                publicationDate = result.Data.PublicationDate,
                isbn = result.Data.ISBN,
                pageCount = result.Data.PageCount,
                condition = result.Data.Condition,
                status = result.Data.Status,
                type = result.Data.Type,
                partnerUserId = result.Data.PartnerUserId,
                language = result.Data.Language,
                addedDate = result.Data.AddedDate,
            });

            return Ok();
        }

        [HttpPost("PostBookWithGenres")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> RegisterBookWithGenres([FromForm] RegisterBookWithGenres model, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (imageFile != null && !imageFile.ContentType.StartsWith("image/"))
            {
                return BadRequest("Only image files are allowed.");
            }


            // The model now includes GenreIds, make sure your service layer knows how to handle it.
            var result = await _bookService.RegisterBookWithGenres(model, imageFile);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            await _hubContext.Clients.All.SendAsync("BookAdded", new
            {
                id = result.Data.Id,
                title = result.Data.Title,
                author = result.Data.Author,
                description = result.Data.Description,
                coverImage = result.Data.CoverImage,
                userId = result.Data.UserId,
                historyId = result.Data.HistoryId,
                publicationDate = result.Data.PublicationDate,
                isbn = result.Data.ISBN,
                pageCount = result.Data.PageCount,
                condition = result.Data.Condition,
                status = result.Data.Status,
                type = result.Data.Type,
                partnerUserId = result.Data.PartnerUserId,
                language = result.Data.Language,
                addedDate = result.Data.AddedDate,
                userName = result.Data.UserName,
                userProfilePicture = result.Data.UserProfilePicture,
            });
            return Ok(result.Data);
        }



    }
}
