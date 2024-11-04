using System.ComponentModel.DataAnnotations;

namespace Connect.Models
{
    public class CreateMemberViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]      
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters long and at most {1} characters long.", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }

        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be 10 digits.")]
        public string Phone { get; set; }
        public int? WardId { get; set; }
     
        public IFormFile ProfilePhoto { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ProfilePhoto == null)
            {
                yield return new ValidationResult("Profile Photo is required.", new[] { nameof(ProfilePhoto) });
            }

            if (Role != "Admin" && !WardId.HasValue)
            {
                yield return new ValidationResult("Ward ID is required unless the role is System Admin", new[] { nameof(WardId) });
            }
        }
    }
}
