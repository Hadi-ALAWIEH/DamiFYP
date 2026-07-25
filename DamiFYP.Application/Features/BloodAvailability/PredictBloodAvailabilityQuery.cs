using DamiFYP.Domain.Models;
using DamiFYP.Infrastructure.BloodAvailability;
using MediatR;

namespace DamiFYP.Application.Features.BloodAvailability;

public class BloodAvailabilityPredictionInput
{
    public string Hospital { get; set; } = string.Empty;
    public string HospitalSize { get; set; } = string.Empty;
    public string Month { get; set; } = string.Empty;
    public BloodTypeName BloodType { get; set; }
    public int OpeningStock { get; set; }
    public int DonationsReceived { get; set; }
    public int BloodRequests { get; set; }
    public bool HolidaySeason { get; set; }
    public bool SummerSeason { get; set; }

    public BloodAvailabilityPredictionRequestDto ToDto() => new(
        Hospital,
        HospitalSize,
        Month,
        BloodType.ToBloodTypeCode(),
        OpeningStock,
        DonationsReceived,
        BloodRequests,
        HolidaySeason,
        SummerSeason);
}

public class PredictBloodAvailabilityQuery : BloodAvailabilityPredictionInput, IRequest<BloodAvailabilityPredictionViewModel> { }

public class PredictBloodAvailabilityQueryHandler
    : IRequestHandler<PredictBloodAvailabilityQuery, BloodAvailabilityPredictionViewModel>
{
    private readonly IBloodAvailabilityServiceClient _client;

    public PredictBloodAvailabilityQueryHandler(IBloodAvailabilityServiceClient client)
    {
        _client = client;
    }

    public async Task<BloodAvailabilityPredictionViewModel> Handle(PredictBloodAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _client.PredictAsync(request.ToDto(), cancellationToken);
        return new BloodAvailabilityPredictionViewModel
        {
            Availability = result.Availability,
            Probabilities = result.Probabilities
        };
    }
}
