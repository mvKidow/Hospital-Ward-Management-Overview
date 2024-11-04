using CareConnect.Models;
using System.ComponentModel.DataAnnotations;

namespace Connect.Models
{
    public class OtpToken
    {
        public int OtpTokenId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(6)]
        public string Otp { get; set; }

        public DateTime ExpiryDate { get; set; }

        public User User { get; set; }
    }
}
