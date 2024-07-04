using System.ComponentModel.DataAnnotations;

namespace Lafatkotob.ViewModels
{
    public class UpdateUserSettingModel
    {
        [StringLength(15)]

        public string Name { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string UserName { get; set; }

        public string About { get; set; }
        public string City { get; set; }
        public string UserId { get; set; }

    }
}
