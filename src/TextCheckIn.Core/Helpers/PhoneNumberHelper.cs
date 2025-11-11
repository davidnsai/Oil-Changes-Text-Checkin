using System.Text.RegularExpressions;
using PhoneNumbers;

namespace TextCheckIn.Core.Helpers;

public static partial class PhoneNumberHelper
{
    private static readonly PhoneNumberUtil _phoneNumberUtil = PhoneNumberUtil.GetInstance();

    [GeneratedRegex(@"\D")]
    private static partial Regex NonDigitRegex();

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
            var digits = NonDigitRegex().Replace(formattedNumber, "");
            if (digits.Length >= 4)
            {
                var lastFour = digits[^4..];
                
                // Return masked format
                return $"(XXX) XXX-{lastFour}";
            }
            
            // If we can't extract 4 digits, mask the entire number
            return "(XXX) XXX-XXXX";
        }
        catch
        {
            // If parsing fails, try basic masking
            var cleanedNumber = NonDigitRegex().Replace(phoneNumber, "");
            
            if (cleanedNumber.Length >= 4)
            {
                var lastFour = cleanedNumber[^4..];
                return $"(XXX) XXX-{lastFour}";
            }
            
            // Return fully masked if we can't parse it
            return "(XXX) XXX-XXXX";
        }
    }

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
