using System.Net.Http.Json;
using System.Text.Json;

namespace DamiFYP.Infrastructure.BloodAvailability;

// Typed HttpClient for the Python FastAPI blood-availability prediction service.
// Field names are snake_case on the wire; the naming policy below maps our
// PascalCase DTO properties to/from that shape without per-property attributes.
public sealed class BloodAvailabilityServiceClient : IBloodAvailabilityServiceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly HttpClient _httpClient;

    public BloodAvailabilityServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BloodAvailabilityMetadataDto> GetMetadataAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("/metadata", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<BloodAvailabilityMetadataDto>(JsonOptions,
            cancellationToken))!;
    }

    public async Task<BloodAvailabilityPredictionResponseDto> PredictAsync(
        BloodAvailabilityPredictionRequestDto request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/predict", request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<BloodAvailabilityPredictionResponseDto>(JsonOptions,
            cancellationToken))!;
    }

    public async Task<List<BloodAvailabilityPredictionResponseDto>> PredictBatchAsync(
        List<BloodAvailabilityPredictionRequestDto> requests, CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.PostAsJsonAsync("/predict/batch", requests, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<List<BloodAvailabilityPredictionResponseDto>>(JsonOptions,
            cancellationToken))!;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new BloodAvailabilityServiceException(
            $"Blood availability service returned {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
    }
}
