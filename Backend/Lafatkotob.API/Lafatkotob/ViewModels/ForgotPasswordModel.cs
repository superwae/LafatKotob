using System.ComponentModel.DataAnnotations;

namespace Lafatkotob.ViewModels
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
