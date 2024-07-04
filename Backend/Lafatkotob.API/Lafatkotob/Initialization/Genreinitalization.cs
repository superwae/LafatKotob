using Lafatkotob.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Lafatkotob.Initialization
{
    public class GenresInitialization
    {
        public static async Task SeedGenres(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var genres = new List<Genre>
            {
                new Genre { Name = "History" },
                new Genre { Name = "Romance" },
                new Genre { Name = "Science Fiction" },
                new Genre { Name = "Fantasy" },
                new Genre { Name = "Thriller" },
                new Genre { Name = "Young Adult" },
                new Genre { Name = "Children" },
                new Genre { Name = "Science" },
                new Genre { Name = "Horror" },
                new Genre { Name = "Nonfiction" },
                new Genre { Name = "Health" },
                new Genre { Name = "Travel" },
                new Genre { Name = "Cooking" },
                new Genre { Name = "Art" },
                new Genre { Name = "Religion" },
                new Genre { Name = "Philosophy" },
                new Genre { Name = "Education" },
                new Genre { Name = "Politics" },
                new Genre { Name = "Business" },
                new Genre { Name = "Technology" },
                new Genre { Name = "True Crime" },
                new Genre { Name = "Drama" },
                new Genre { Name = "Adventure" },
                new Genre { Name = "Nature" },
                new Genre { Name = "Humor" },
                new Genre { Name = "Lifestyle" },
                new Genre { Name = "Economics" },
                new Genre { Name = "Astronomy" },
                new Genre { Name = "Linguistics" },
                new Genre { Name = "Literature" },
                new Genre { Name = "Short Story" },
                new Genre { Name = "Novel" },
                new Genre { Name = "Medicine" },
                new Genre { Name = "Psychology" },
                new Genre { Name = "Anime" },
                new Genre { Name = "Poetry" },
                new Genre { Name = "Sports" },
                new Genre { Name = "Comics" }

            };

            foreach (var genre in genres)
            {
                var genreExist = await context.Genres.AnyAsync(g => g.Name == genre.Name);
                if (!genreExist)
                {
                    context.Genres.Add(genre);
                }

            }
            await context.SaveChangesAsync();
        }
    }
}
