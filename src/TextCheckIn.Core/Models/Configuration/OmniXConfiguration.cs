namespace TextCheckIn.Core.Models.Configuration;

public class OmniXConfiguration
{
    public const string SectionName = "OmniX";

    public string? ApiUrl { get; set; }

    public string? ApiKey { get; set; }

    public string? WebhookSecret { get; set; }

    public int TimeoutSeconds { get; set; } = 30;

    public long MaxWebhookPayloadSize { get; set; } = 1048576;

    public bool EnableSignatureValidation { get; set; } = true;

    public int MaxWebhookAgeMinutes { get; set; } = 5;
}
