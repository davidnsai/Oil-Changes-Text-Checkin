namespace TextCheckIn.Core.Models.Domain;

public class CustomerData
{
    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required string Email { get; set; }

    public required string PhoneNumber { get; set; }

    public ContactPreference PreferredContact { get; set; } = ContactPreference.SMS;

    public bool AcceptsMarketing { get; set; } = false;
}

public enum ContactPreference
{
    SMS,

    Email,

    Phone
}
