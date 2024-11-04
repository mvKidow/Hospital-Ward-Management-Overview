using CareConnect.Models;
using Connect.Models;
using Microsoft.AspNetCore.Mvc;

namespace Connect.Repositories.Account
{
    public interface IUserRepository
    {
        Task<User> AuthenticateUserAsync(string email, string password);
        Task<User> GetUserByIdAsync(int userId);
        Task<User> GetCurrentUserAsync(int userId);
        Task<User> GetUserByEmailAsync(string email);
        Task SavePasswordResetTokenAsync(int userId, string token);
        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
        Task SaveOtpAsync(int userId, string otp);
        Task<bool> VerifyOtpAsync(string email, string otp);
        Task<bool> ResetPasswordAsync(string email, string newPassword);
        Task SendEmailAsync(EmailModel emailModel);
        Task<User> GetUserProfileAsync(int userId);
        Task<bool> UpdateUserAsync(User user);
        Task<IEnumerable<UserWard>> GetUserWardsAsync(int userId);
    }
}
