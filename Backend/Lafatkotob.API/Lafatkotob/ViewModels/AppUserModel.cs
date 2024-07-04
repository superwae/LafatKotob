using Lafatkotob.Entities;

namespace Lafatkotob.ViewModels
{
    public class AppUserModel
    {
        public string Id { get; set; }
        public bool IsDeleted { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Email { get; set; }
        public DateTime DateJoined { get; set; }
        public DateTime LastLogin { get; set; }
        public string ProfilePicture { get; set; }
        public string About { get; set; }
        public DateTime DTHDate { get; set; }
        public int? HistoryId { get; set; }
        public int? UpVotes { get; set; }

        public int Age => DateTime.Today.Year - DTHDate.Year - (DateTime.Today.DayOfYear < DTHDate.DayOfYear ? 1 : 0);

 
    }
}
