using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using TextCheckIn.Data.Configuration;
using TextCheckIn.Data.Helpers;
using TextCheckIn.Data.OmniX.Models;
using TextCheckIn.Data.OmniX.Repositories.Abstractions;

namespace TextCheckIn.Data.OmniX.Repositories;

public class OminXRepository : IOmniXRepository
{
    private readonly OmniXSettings _omniXSettings;

    private readonly HttpClient _httpClient;

    public OminXRepository(OmniXSettings omniXSettings, HttpClient httpClient)
    {
        _omniXSettings = omniXSettings;
        _httpClient = httpClient;
    }

    public async Task<ServiceRecommendation?> GetServiceRecommendationsAsync(GetServiceRecommendationsByLicensePlateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var requestBody = JsonSerializer.Serialize(request, JsonSerializerDefaultOptions.Options);
        var requestContent = new StringContent(requestBody, Encoding.UTF8, MediaTypeNames.Application.Json);

        var url = $"{_omniXSettings.BaseUrl}/services/recommended"; // TODO: Change this to use actual URL path
        var response = await _httpClient.PostAsync(url, requestContent);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ServiceRecommendation>();
    }

    public async Task<ServiceRecommendation?> GetServiceRecommendationsAsync(GetServiceRecommendationsByVinRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var requestBody = JsonSerializer.Serialize(request, JsonSerializerDefaultOptions.Options);
        var requestContent = new StringContent(requestBody, Encoding.UTF8, MediaTypeNames.Application.Json);

        var url = $"{_omniXSettings.BaseUrl}/services/recommended"; // TODO: Change this to use actual URL path
        var response = await _httpClient.PostAsync(url, requestContent);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ServiceRecommendation>();
    }
}