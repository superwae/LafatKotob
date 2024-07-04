using System.ComponentModel.DataAnnotations;

namespace Lafatkotob.ViewModel
{
    public class RegisterModel
    {
        [Required]
        [StringLength(15)]
        public string Name { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{8,}$", ErrorMessage = "Password must be at least 8 characters and contain at least one letter and one number.")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmNewPassword { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Compare("Email")]
        public string ConfirmNewEmail { get; set; }
        [Required]
        public string UserName { get; set; }
        public string ProfilePictureUrl { get; set; }
        [Required]
        public DateTime DTHDate { get; set; }
        [Required]
        public string City { get; set; }
        public string About { get; set; }
       


}
}
