namespace TextCheckIn.Core.Models.Configuration;

/// <summary>
/// Configuration settings for omniX integration
/// </summary>
public class OmniXConfiguration
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "OmniX";

    /// <summary>
    /// Whether to use mock service instead of real omniX API
    /// </summary>
    public bool UseMockService { get; set; } = true;

    /// <summary>
    /// omniX API base URL
    /// </summary>
    public string? ApiUrl { get; set; }

    /// <summary>
    /// omniX API authentication key
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Webhook signature validation secret
    /// </summary>
    public string? WebhookSecret { get; set; }

    /// <summary>
    /// API request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum webhook payload size in bytes
    /// </summary>
    public long MaxWebhookPayloadSize { get; set; } = 1048576; // 1MB

    /// <summary>
    /// Whether to validate webhook signatures
    /// </summary>
    public bool EnableSignatureValidation { get; set; } = true;

    /// <summary>
    /// Maximum age of webhook timestamp in minutes (replay attack prevention)
    /// </summary>
    public int MaxWebhookAgeMinutes { get; set; } = 5;

    /// <summary>
    /// Mock service configuration
    /// </summary>
    public MockServiceConfiguration Mock { get; set; } = new();
}

/// <summary>
/// Configuration for mock omniX service
/// </summary>
public class MockServiceConfiguration
{
    /// <summary>
    /// Artificial delay in milliseconds for mock responses
    /// </summary>
    public int DelayMs { get; set; } = 500;

    /// <summary>
    /// Probability of returning "vehicle not found" (0.0 to 1.0)
    /// </summary>
    public double NotFoundProbability { get; set; } = 0.1;

    /// <summary>
    /// Probability of returning partial vehicle data (0.0 to 1.0)
    /// </summary>
    public double PartialDataProbability { get; set; } = 0.2;

    /// <summary>
    /// Probability of API error simulation (0.0 to 1.0)
    /// </summary>
    public double ErrorProbability { get; set; } = 0.05;

    /// <summary>
    /// Whether to generate realistic service recommendations
    /// </summary>
    public bool GenerateRealisticRecommendations { get; set; } = true;

    /// <summary>
    /// Default store ID for mock responses
    /// </summary>
    public string DefaultStoreId { get; set; } = "303";
}
