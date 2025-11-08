using System.Text.RegularExpressions;
using PhoneNumbers;

namespace TextCheckIn.Core.Helpers;

/// <summary>
/// Helper class for phone number operations including masking
/// </summary>
public static class PhoneNumberHelper
{
    private static readonly PhoneNumberUtil _phoneNumberUtil = PhoneNumberUtil.GetInstance();

    /// <summary>
    /// Masks a phone number, showing only the last 4 digits
    /// </summary>
    /// <param name="phoneNumber">The phone number to mask</param>
    /// <returns>Masked phone number (e.g., "(XXX) XXX-1234") or original if invalid</returns>
    public static string MaskPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return string.Empty;

        try
        {
            // Try to parse the phone number
            var parsedNumber = _phoneNumberUtil.Parse(phoneNumber, "US");
            
            // Format it to national format
            var formattedNumber = _phoneNumberUtil.Format(parsedNumber, PhoneNumberFormat.NATIONAL);
            
            // Extract the last 4 digits
            var digits = Regex.Replace(formattedNumber, @"\D", "");
            if (digits.Length >= 4)
            {
                var lastFour = digits.Substring(digits.Length - 4);
                
                // Return masked format
                return $"(XXX) XXX-{lastFour}";
            }
            
            // If we can't extract 4 digits, mask the entire number
            return "(XXX) XXX-XXXX";
        }
        catch
        {
            // If parsing fails, try basic masking
            var cleanedNumber = Regex.Replace(phoneNumber, @"\D", "");
            
            if (cleanedNumber.Length >= 4)
            {
                var lastFour = cleanedNumber.Substring(cleanedNumber.Length - 4);
                return $"(XXX) XXX-{lastFour}";
            }
            
            // Return fully masked if we can't parse it
            return "(XXX) XXX-XXXX";
        }
    }

    /// <summary>
    /// Validates if a phone number is valid
    /// </summary>
    /// <param name="phoneNumber">The phone number to validate</param>
    /// <param name="regionCode">The region code (default: US)</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidPhoneNumber(string? phoneNumber, string regionCode = "US")
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        try
        {
            var parsedNumber = _phoneNumberUtil.Parse(phoneNumber, regionCode);
            return _phoneNumberUtil.IsValidNumber(parsedNumber);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Formats a phone number to E164 format
    /// </summary>
    /// <param name="phoneNumber">The phone number to format</param>
    /// <param name="regionCode">The region code (default: US)</param>
    /// <returns>E164 formatted phone number or original if invalid</returns>
    public static string FormatToE164(string? phoneNumber, string regionCode = "US")
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return string.Empty;

        try
        {
            var parsedNumber = _phoneNumberUtil.Parse(phoneNumber, regionCode);
            return _phoneNumberUtil.Format(parsedNumber, PhoneNumberFormat.E164);
        }
        catch
        {
            return phoneNumber;
        }
    }
}
