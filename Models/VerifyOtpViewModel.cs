using System.ComponentModel.DataAnnotations;

namespace Connect.Models
{
    public class VerifyOtpViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        [Display(Name = "OTP")]
        public string Otp { get; set; }
    }
}
