using System.ComponentModel.DataAnnotations;

namespace Lafatkotob.ViewModel
{
    public class UpdateUserModel
    {
        [StringLength(15)]
        
        public string Name { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string UserName { get; set; }

        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; }

        public string ProfilePictureUrl { get; set; }

        public string About { get; set; }
        public string City { get; set; }

    }
}
