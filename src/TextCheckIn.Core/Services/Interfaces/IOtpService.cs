using System;
using System.Threading.Tasks;

namespace TextCheckIn.Core.Services.Interfaces
{
    public interface IOtpService
    {
        Task<string> GenerateOtpAsync(string phoneNumber);

        Task<bool> ValidateOtpAsync(string phoneNumber, string otpCode);

        Task<int?> GetRemainingAttemptsAsync(string phoneNumber);

        Task ClearOtpAsync(string phoneNumber);

        Task<bool> IsInCooldownAsync(string phoneNumber);

        Task<int?> GetCooldownRemainingSecondsAsync(string phoneNumber);
    }
}
