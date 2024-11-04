using CareConnect.Models;
using Connect.Data;
using Connect.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Net.Mail;
using System.Net;

namespace Connect.Repositories.Account
{
    public class UserRepository : IUserRepository
    {
        private readonly CareConnectDbContext _context;
        private readonly IDbConnection _connection;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(
            ILogger<UserRepository> logger,
            IConfiguration configuration,
            CareConnectDbContext context,
            IDbConnection connection)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public async Task<User> AuthenticateUserAsync(string email, string password)
        {
            try
            {
                _logger.LogInformation("Attempting to authenticate user: {Email}", email);

                var parameters = new { Email = email, Password = password };
                var user = await _connection.QueryFirstOrDefaultAsync<User>(
                    "sp_AuthenticateUser",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                if (user != null)
                {
                    _logger.LogInformation("User authenticated successfully: {Email}", email);
                    _logger.LogInformation("Profile photo present: {HasPhoto}", user.ProfilePhoto != null);

                    // If you need to load additional data that might not be in the stored procedure
                    return await LoadUserDetailsAsync(user);
                }
                else
                {
                    _logger.LogWarning("Authentication failed for user: {Email}", email);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating user: {Email}", email);
                throw;
            }
        }

   

        private async Task<User> LoadUserDetailsAsync(User user)
        {
            // If the stored procedure didn't return all necessary data,
            // load it here from the context
            if (user.ProfilePhoto == null)
            {
                var fullUser = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == user.UserId);

                if (fullUser?.ProfilePhoto != null)
                {
                    user.ProfilePhoto = fullUser.ProfilePhoto;
                    _logger.LogInformation("Loaded profile photo from database for user: {Email}", user.Email);
                }
            }

            return user;
        }

        public async Task SaveOtpAsync(int userId, string otp)
        {
            var parameters = new { UserId = userId, Otp = otp, ExpiryDate = DateTime.UtcNow.AddMinutes(10) };
            await _connection.ExecuteAsync("sp_SaveOtp",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> VerifyOtpAsync(string email, string otp)
        {
            var parameters = new { Email = email, Otp = otp };
            var result = await _connection.ExecuteScalarAsync<int>("sp_VerifyOtp",
                parameters,
                commandType: CommandType.StoredProcedure);
            return result > 0;
        }

        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            var parameters = new { Email = email, NewPassword = newPassword };
            var result = await _connection.ExecuteScalarAsync<int>("sp_ResetPassword",
                parameters,
                commandType: CommandType.StoredProcedure);
            return result > 0;
        }

        public async Task SendEmailAsync(EmailModel emailModel)
        {
            if (string.IsNullOrEmpty(emailModel?.To))
            {
                throw new ArgumentException("Email recipient is required");
            }

            try
            {
                using (var client = new SmtpClient())
                {
                    var host = _configuration["Email:Host"] ?? throw new InvalidOperationException("SMTP host not configured");
                    var port = int.Parse(_configuration["Email:Port"] ?? "587");
                    var systemEmail = _configuration["Email:SystemEmail"] ?? throw new InvalidOperationException("System email not configured");
                    var password = _configuration["Email:SystemEmailPassword"] ?? throw new InvalidOperationException("System email password not configured");

                    client.Host = host;
                    client.Port = port;
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(systemEmail, password);
                    client.Timeout = 10000;

                    using (var message = new MailMessage())
                    {
                        message.From = new MailAddress(systemEmail, _configuration["Email:SystemName"] ?? "CareConnect");
                        message.Subject = emailModel.Subject;
                        message.Body = GetFormattedEmailBody(emailModel.Body);
                        message.IsBodyHtml = true;
                        message.To.Add(emailModel.To);

                        await SendWithRetryAsync(client, message, emailModel.To);
                    }
                }
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP error occurred while sending email to {Email}. Error: {ErrorMessage}",
                    emailModel.To, smtpEx.Message);
                throw new Exception($"SMTP error: {smtpEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}. Error: {ErrorMessage}",
                    emailModel.To, ex.Message);
                throw new Exception("An error occurred while sending the email. Please try again later.");
            }
        }

        private async Task SendWithRetryAsync(SmtpClient client, MailMessage message, string recipient)
        {
            int maxRetries = 3;
            int currentTry = 0;

            while (currentTry < maxRetries)
            {
                try
                {
                    await client.SendMailAsync(message);
                    _logger.LogInformation("Email sent successfully to {Email} on attempt {Attempt}",
                        recipient, currentTry + 1);
                    return;
                }
                catch (SmtpException ex) when (currentTry < maxRetries - 1)
                {
                    currentTry++;
                    _logger.LogWarning(ex, "SMTP error on attempt {Attempt}. Retrying...", currentTry);
                    await Task.Delay(1000 * currentTry); // Exponential backoff
                }
            }

            throw new Exception($"Failed to send email after {maxRetries} attempts");
        }

        private string GetFormattedEmailBody(string content)
        {
            return $@"
            <html>
            <body style='font-family: Arial, sans-serif; padding: 20px;'>
                <div style='max-width: 600px; margin: 0 auto; background-color: #f9f9f9; padding: 20px; border-radius: 5px;'>
                    <h2 style='color: #333;'>Password Reset Request</h2>
                    <p>Your CareConnect OTP is:</p>
                    <h1 style='color: #007bff; font-size: 32px; letter-spacing: 2px;'>{content}</h1>
                    <p style='color: #666;'>If you didn't request this password reset, contact CareConnect support team.</p>
                </div>
            </body>
            </html>";
        }

      

        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<User> GetCurrentUserAsync(int userId)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task SavePasswordResetTokenAsync(int userId, string token)
        {
            var parameters = new { UserId = userId, Token = token, ExpiryDate = DateTime.UtcNow.AddHours(24) };
            await _connection.ExecuteAsync("sp_SavePasswordResetToken",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var parameters = new { Email = email, Token = token, NewPassword = newPassword };
            var result = await _connection.ExecuteScalarAsync<int>("sp_ResetPassword",
                parameters,
                commandType: CommandType.StoredProcedure);
            return result > 0;
        }
        public async Task<User> GetUserProfileAsync(int userId)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                return await connection.QuerySingleOrDefaultAsync<User>(
                    "sp_GetUserProfile",
                    new { UserId = userId },
                    commandType: CommandType.StoredProcedure
                );
            }
        }
        public async Task<IEnumerable<UserWard>> GetUserWardsAsync(int userId)
        {
            try
            {
                var userWards = await _context.UserWards
                    .Include(uw => uw.Ward)
                    .Where(uw => uw.UserId == userId)
                    .ToListAsync();

                if (!userWards.Any())
                {
                    _logger.LogInformation($"No ward assignments found for user {userId}");
                    return Enumerable.Empty<UserWard>();
                }

                _logger.LogInformation($"Retrieved {userWards.Count} ward assignments for user {userId}");
                return userWards;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving ward assignments for user {userId}");
                throw;
            }
        }
        public async Task<bool> UpdateUserAsync(User user)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                var result = await connection.ExecuteAsync(
                    "sp_UpdateUserProfile",
                    new
                    {
                        user.UserId,
                        user.Title,
                        user.Name,
                        user.Surname,
                        user.Email,
                        user.Phone,
                        user.ProfilePhoto
                    },
                    commandType: CommandType.StoredProcedure
                );
                return result > 0;
            }
        }

    }
}

