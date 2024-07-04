using Lafatkotob.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lafatkotob.Initialization
{
    public class Badgeinitialization
    {
        public static async Task SeedBadges(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var badges = new List<Badge>
            {
                new Badge { BadgeName="ReviewBronze" ,Description="received 5 positive reviews"},
                new Badge { BadgeName="ReviewSilver" ,Description="received 20 positive reviews"},
                new Badge { BadgeName="ReviewGold" ,Description="received 50 positive reviews"},

                new Badge { BadgeName="ParticipationBronze" ,Description="signed up for 1 event"},
                new Badge { BadgeName="ParticipationSilver" ,Description="signed up for 3 events"},
                new Badge { BadgeName="ParticipationGold" ,Description="signed up for 10 events"},

                new Badge { BadgeName="TradingBronze" ,Description="traded 1 book"},
                new Badge { BadgeName="TradingSilver" ,Description="traded 5 books"},
                new Badge { BadgeName="TradingGold" ,Description="traded 10 books"},

              
            };

            foreach (var badge in badges)
            {
                var badgeExist = await context.Badges.AnyAsync(b => b.BadgeName == badge.BadgeName);
                if (!badgeExist)
                {
                    context.Badges.Add(badge);
                }

            }
            await context.SaveChangesAsync();
        }
    }
}
