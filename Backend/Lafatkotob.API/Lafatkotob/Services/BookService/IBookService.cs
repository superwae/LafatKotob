using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Lafatkotob.Services.BookService
{
    public interface IBookService
    {
        Task<BookWithUserModel> GetById(int id);
        Task<List<BookModel>> GetBooksByUserName(string username, int pageNumber, int pageSize);
        Task<List<BookModel>> GetAll(int pageNumber, int pageSize);
        Task<List<BookWithUserModel>> GetAllWithUserInfo(int pageNumber, int pageSize);

        Task<ServiceResponse<BookModel>> Delete(int id);
        Task<ServiceResponse<BookModel>> DeleteByAdmin(int id);
        Task<ServiceResponse<BookModel>> Report(int id, string userId);
        Task<GetGenreForUserModel> GetGenresForUser(string userId);
        Task<ServiceResponse<BookModel>> Post(RegisterBook model, IFormFile imageFile);
        Task<ServiceResponse<BookWithUserModel>> RegisterBookWithGenres(RegisterBookWithGenres model, IFormFile imageFile);
        Task<ServiceResponse<UpdateBookModel>> Update(int id, UpdateBookModel model, IFormFile imageFile = null);
        Task<ServiceResponse<List<BookModel>>> GetBooksFilteredByGenres(List<int> genreIds, int pageNumber, int pageSize);
        Task<ServiceResponse<List<GenreModel>>> GetGenresByBookId(int bookId);
        Task<ServiceResponse<List<Book>>> SearchBooks(string query, int pageNumber, int pageSize);
        Task<ServiceResponse<Book>> UpdateBookWithGenres(RegisterBookWithGenres model, IFormFile imageFile);
        Task<ServiceResponse<Book>> SellBook(int bookId);
        Task<ServiceResponse<List<BookModel>>> GetBooksByIsbn(List<string> isbnList);
    }
}
