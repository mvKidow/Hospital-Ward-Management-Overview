using System.ComponentModel.DataAnnotations;

namespace Connect.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
