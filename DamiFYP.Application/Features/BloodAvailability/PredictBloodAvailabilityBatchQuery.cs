using DamiFYP.Infrastructure.BloodAvailability;
using MediatR;

namespace DamiFYP.Application.Features.BloodAvailability;

public class PredictBloodAvailabilityBatchQuery : IRequest<List<BloodAvailabilityPredictionViewModel>>
{
    public List<BloodAvailabilityPredictionInput> Predictions { get; set; } = new();
}

public class PredictBloodAvailabilityBatchQueryHandler
    : IRequestHandler<PredictBloodAvailabilityBatchQuery, List<BloodAvailabilityPredictionViewModel>>
{
    private readonly IBloodAvailabilityServiceClient _client;

    public PredictBloodAvailabilityBatchQueryHandler(IBloodAvailabilityServiceClient client)
    {
        _client = client;
    }

    public async Task<List<BloodAvailabilityPredictionViewModel>> Handle(PredictBloodAvailabilityBatchQuery request,
        CancellationToken cancellationToken)
    {
        var requestDtos = request.Predictions.Select(prediction => prediction.ToDto()).ToList();
        var results = await _client.PredictBatchAsync(requestDtos, cancellationToken);
        return results.Select(result => new BloodAvailabilityPredictionViewModel
        {
            Availability = result.Availability,
            Probabilities = result.Probabilities
        }).ToList();
    }
}
