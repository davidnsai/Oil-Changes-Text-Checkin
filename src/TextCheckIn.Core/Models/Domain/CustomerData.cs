namespace TextCheckIn.Core.Models.Domain;

/// <summary>
/// Represents customer information collected during check-in
/// </summary>
public class CustomerData
{
    /// <summary>
    /// Customer's first name
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// Customer's last name
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// Customer's email address
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Customer's phone number
    /// </summary>
    public required string PhoneNumber { get; set; }

    /// <summary>
    /// Preferred contact method
    /// </summary>
    public ContactPreference PreferredContact { get; set; } = ContactPreference.SMS;

    /// <summary>
    /// Whether customer wants to receive promotional offers
    /// </summary>
    public bool AcceptsMarketing { get; set; } = false;
}

/// <summary>
/// Customer contact preferences
/// </summary>
public enum ContactPreference
{
    /// <summary>
    /// Contact via SMS/text message
    /// </summary>
    SMS,

    /// <summary>
    /// Contact via email
    /// </summary>
    Email,

    /// <summary>
    /// Contact via phone call
    /// </summary>
    Phone
}
