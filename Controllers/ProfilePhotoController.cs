using Connect.Repositories.Account;
using Microsoft.AspNetCore.Mvc;

namespace Connect.Controllers
{
    public class ProfilePhotoController : Controller
    {

        private readonly IUserRepository _userRepository;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ILogger<ProfilePhotoController> _logger;

        public ProfilePhotoController(
            IUserRepository userRepository,
            IWebHostEnvironment hostEnvironment,
            ILogger<ProfilePhotoController> logger)
        {
            _userRepository = userRepository;
            _hostEnvironment = hostEnvironment;
            _logger = logger;
        }

        [HttpGet("api/profile-photo/{userId}")]
        public async Task<IActionResult> GetProfilePhoto(int userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);

                // Check if user exists and has a profile photo
                if (user?.ProfilePhoto != null && user.ProfilePhoto.Length > 0)
                {
                    // Set cache headers
                    Response.Headers.Add("Cache-Control", "public, max-age=3600"); // Cache for 1 hour
                    return File(user.ProfilePhoto, "image/jpeg");
                }

                // Return default image
                var defaultImagePath = Path.Combine(_hostEnvironment.WebRootPath, "img", "default-profile.jpg");
                if (System.IO.File.Exists(defaultImagePath))
                {
                    Response.Headers.Add("Cache-Control", "public, max-age=86400"); // Cache for 24 hours
                    return File(System.IO.File.ReadAllBytes(defaultImagePath), "image/jpeg");
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile photo for user {UserId}", userId);
                return NotFound();
            }
        }
    }
}
