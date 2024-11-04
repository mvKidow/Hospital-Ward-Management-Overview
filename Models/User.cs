using Connect.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    public class User
    {

        [Key]
        public int UserId { get; set; }
        [Required, MaxLength(50)]
        public string Name { get; set; }
        [Required, MaxLength(50)]
        public string Surname { get; set; }
        [Required, EmailAddress, MaxLength(50)]
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Required, MinLength(8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [Phone]
        public string Phone { get; set; }
        [Required]
        public string Role { get; set; }
        [Required]
        public string Title { get; set; }
        public string Status { get; set; }
        [ForeignKey("WardId")]
        public int? WardId { get; set; }
        public Ward Ward { get; set; }
        public ICollection<UserWard> UserWards { get; set; }
        public byte[]? ProfilePhoto { get; set; } 
        public ICollection<PasswordResetToken> PasswordResetTokens { get; set; }
        public ICollection<OtpToken> OtpTokens { get; set; }

    }

   
}
