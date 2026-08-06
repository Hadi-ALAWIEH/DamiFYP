using System.Net.Http.Json;
using System.Text.Json;

namespace DamiFYP.Infrastructure.FaceVerification;

// Typed HttpClient for the small Python FastAPI face-verification service
// (see /face-verification-service at the repo root). Field names are
// snake_case on the wire - same convention as BloodAvailabilityServiceClient.
//
// This is the concrete implementation of IFaceVerificationService (also in
// this namespace - see IFaceVerificationService.cs) that
// SubmitVerificationCommandHandler depends on - it's what actually re-runs
// the pose check server-side instead of trusting the frontend's own claim.
public sealed class FaceVerificationServiceClient : IFaceVerificationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly HttpClient _httpClient;

    public FaceVerificationServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<FaceVerificationResult> VerifyAsync(
        IReadOnlyList<VerificationPoseFrame> frames, CancellationToken cancellationToken = default)
    {
        var request = new FaceVerificationRequestDto(
            frames.Select(f => new FaceVerificationFrameDto(f.Pose, Convert.ToBase64String(f.ImageData))).ToList());

        using var response = await _httpClient.PostAsJsonAsync("/verify", request, JsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new FaceVerificationServiceException(
                $"Face verification service returned {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
        }

        var dto = (await response.Content.ReadFromJsonAsync<FaceVerificationResponseDto>(JsonOptions,
            cancellationToken))!;

        return new FaceVerificationResult
        {
            Passed = dto.Passed,
            FailureReason = dto.FailureReason
        };
    }
}
