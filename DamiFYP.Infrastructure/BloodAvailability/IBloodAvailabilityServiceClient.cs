namespace DamiFYP.Infrastructure.BloodAvailability;

public interface IBloodAvailabilityServiceClient
{
    Task<BloodAvailabilityMetadataDto> GetMetadataAsync(CancellationToken cancellationToken = default);

    Task<BloodAvailabilityPredictionResponseDto> PredictAsync(BloodAvailabilityPredictionRequestDto request,
        CancellationToken cancellationToken = default);

    Task<List<BloodAvailabilityPredictionResponseDto>> PredictBatchAsync(
        List<BloodAvailabilityPredictionRequestDto> requests, CancellationToken cancellationToken = default);
}
