using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Lafatkotob.Entities;

namespace Lafatkotob
{
    public class ApplicationDbContext : IdentityDbContext 
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookGenre> BookGenres { get; set; }
        public DbSet<BookPostComment> BookPostComments { get; set; }
        public DbSet<BookPostLike> BookPostLikes { get; set; }
        public DbSet<BooksInWishlists> BooksInWishlists { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationsUser> ConversationsUsers { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<History> History { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationUser> NotificationUsers { get; set; }
        public DbSet<UserBadge> UserBadges { get; set; }
        public DbSet<UserEvent> UserEvents { get; set; }
        public DbSet<UserLike> UserLikes { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }
        public DbSet<UserReview> UserReviews { get; set; }
        public DbSet<WishedBook> WishedBooks { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<UserVote> UserVotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
