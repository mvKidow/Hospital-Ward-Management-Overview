using Connect.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Connect.Repositories.Account;
using CareConnect.Models;
using Microsoft.Extensions.Hosting;

namespace Connect.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AccountController> _logger;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AccountController(
            IUserRepository userRepository,
            ILogger<AccountController> logger,
            IWebHostEnvironment hostEnvironment)
        {
            _userRepository = userRepository;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewData["HideLayout"] = true;
            return View();
        }

        [HttpGet("profile-photo/{userId}")]
        public async Task<IActionResult> GetProfilePhoto(int userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user?.ProfilePhoto != null && user.ProfilePhoto.Length > 0)
                {
                    return File(user.ProfilePhoto, "image/jpeg");
                }

                // Return default image
                var defaultImagePath = Path.Combine(_hostEnvironment.WebRootPath, "img", "default-profile.jpg");
                if (System.IO.File.Exists(defaultImagePath))
                {
                    return File(System.IO.File.ReadAllBytes(defaultImagePath), "image/jpeg");
                }

                // If even the default image is missing, return a 404
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile photo for user {UserId}", userId);
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            ViewData["HideLayout"] = true;
            if (ModelState.IsValid)
            {
                var user = await _userRepository.AuthenticateUserAsync(model.Email, model.Password);
                if (user != null)
                {
                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("FullName", $"{user.Title} {user.Name} {user.Surname}".Trim())
                };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                        });

                    return RedirectToAction("Index", GetControllerByRole(user.Role));
                }
                ModelState.AddModelError(string.Empty, "Wrong password or Email try again!");
            }
            return View(model);
        }


        public async Task<IActionResult> Logout()
        {

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
        public IActionResult ForgotPassword()
        {
            ViewData["HideLayout"] = true;
            var model = new ForgotPasswordViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model)
        {

            ViewData["HideLayout"] = true;
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Please enter a valid email address." });
                }

                var user = await _userRepository.GetUserByEmailAsync(model.Email);
                if (user == null)
                {
                    _logger.LogInformation("Password reset attempted for non-existent email: {Email}", model.Email);
                    return Json(new { success = true, message = "If the email exists in our system, you will receive an OTP shortly." });
                }

                var otp = GenerateSecureOtp();
                await _userRepository.SaveOtpAsync(user.UserId, otp);

                var emailModel = new EmailModel
                {
                    To = user.Email,
                    Subject = "Password Reset OTP",
                    Body = $"{otp}. It will expire in 10 minutes."
                };

                await _userRepository.SendEmailAsync(emailModel);
                _logger.LogInformation("OTP sent successfully to {Email}", user.Email);

                return Json(new { success = true, message = "OTP has been sent to your email." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ForgotPassword for email: {Email}", model.Email);
                return Json(new { success = false, message = "An error occurred while processing your request. Please try again later." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpViewModel model)
        {
            ViewData["HideLayout"] = true;
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Please provide a valid OTP." });
                }

                var isValidOtp = await _userRepository.VerifyOtpAsync(model.Email, model.Otp);
                if (isValidOtp)
                {
                    var token = Guid.NewGuid().ToString("N");
                    var user = await _userRepository.GetUserByEmailAsync(model.Email);
                    if (user != null)
                    {
                        await _userRepository.SavePasswordResetTokenAsync(user.UserId, token);
                        return Json(new
                        {
                            success = true,
                            redirectUrl = Url.Action("ResetPassword", "Account", new { email = model.Email, token = token })
                        });
                    }
                }

                return Json(new { success = false, message = "Invalid or expired OTP. Please try again." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyOtp for email: {Email}", model.Email);
                return Json(new { success = false, message = "An error occurred while verifying the OTP. Please try again later." });
            }
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            ViewData["HideLayout"] = true;
            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };
            return View(model);
        }
          

        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            ViewData["HideLayout"] = true;
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please ensure all fields are filled correctly."
                    });
                }

                var result = await _userRepository.ResetPasswordAsync(
                    model.Email,
                    model.Token,
                    model.NewPassword);

                if (result)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Your password has been reset successfully.",
                        redirectUrl = Url.Action("Login", "Account")
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "Invalid or expired reset token. Please try again."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ResetPassword for email: {Email}", model.Email);
                return Json(new
                {
                    success = false,
                    message = "An error occurred while resetting your password. Please try again later."
                });
            }
        }

        private string GenerateSecureOtp()
        {
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                var random = BitConverter.ToUInt32(bytes, 0);
                return (random % 900000 + 100000).ToString(); 
            }
        }
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            ViewData["HideLayout"] = true;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = await _userRepository.GetUserByIdAsync(int.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new EditProfileViewModel
            {
                UserId = user.UserId,
                Title = user.Title,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                Phone = user.Phone,
                CurrentProfilePhoto = user.ProfilePhoto
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            ViewData["HideLayout"] = true;
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please check your input and try again.";
                return View("EditProfile", model);
            }

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null || int.Parse(userId) != model.UserId)
                {
                    return RedirectToAction("Profile");
                }

                var user = await _userRepository.GetUserByIdAsync(model.UserId);
                if (user == null)
                {
                    return NotFound();
                }

                // Update user properties
                user.Title = model.Title;
                user.Name = model.Name;
                user.Surname = model.Surname;
                user.Email = model.Email;
                user.Phone = model.Phone;

                // Handle profile photo upload
                if (model.ProfilePhoto != null && model.ProfilePhoto.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.ProfilePhoto.CopyToAsync(memoryStream);
                        user.ProfilePhoto = memoryStream.ToArray();
                    }
                }

                await _userRepository.UpdateUserAsync(user);

                // Update claims if necessary
                var identity = User.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    var fullNameClaim = identity.FindFirst("FullName");
                    if (fullNameClaim != null)
                    {
                        identity.RemoveClaim(fullNameClaim);
                        identity.AddClaim(new Claim("FullName", $"{user.Title} {user.Name} {user.Surname}".Trim()));
                    }
                }

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", model.UserId);
                TempData["ErrorMessage"] = "An error occurred while updating your profile. Please try again.";
                return View("EditProfile", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            ViewData["HideLayout"] = true;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return RedirectToAction("Index");
            }

            var user = await _userRepository.GetUserByIdAsync(int.Parse(userId));
            return View(user);
        }

        private string GetControllerByRole(string role)
        {
            switch (role)
            {
                case "Admin":
                    return "Admin";
                case "Doctor":
                    return "Doctor";
                case "Nurse":
                    return "Nurse";
                case "WardAdmin":
                    return "WardAdmin";
                default:
                    return "Home";
            }
        }  

       
    }
}
