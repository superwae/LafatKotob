using System.ComponentModel.DataAnnotations;

namespace Lafatkotob.ViewModels
{
    public class ChangePasswordModel
    {
        [Required]

        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{8,}$", ErrorMessage = "Password must be at least 8 characters and contain at least one letter and one number.")]

        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        [Required]

        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{8,}$", ErrorMessage = "Password must be at least 8 characters and contain at least one letter and one number.")]

        public string OldPassword { get; set; }

        [Required]

        public string UserId { get; set; }
    }
}
