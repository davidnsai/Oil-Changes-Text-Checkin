using System.Threading.Tasks;

namespace TextCheckIn.Core.Services.Interfaces
{
    public interface ISmsService
    {
        Task<bool> SendSmsAsync(string phoneNumber, string message, string firstName = "", string lastName = "");

        Task<bool> SendOtpAsync(string phoneNumber, string otpCode, string firstName = "", string lastName = "");
    }
}
