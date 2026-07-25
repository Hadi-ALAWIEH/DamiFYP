using DamiFYP.Infrastructure.BloodAvailability;
using MediatR;

namespace DamiFYP.Application.Features.BloodAvailability;

public class GetBloodAvailabilityMetadataQuery : IRequest<BloodAvailabilityMetadataViewModel> { }

public class GetBloodAvailabilityMetadataQueryHandler
    : IRequestHandler<GetBloodAvailabilityMetadataQuery, BloodAvailabilityMetadataViewModel>
{
    private readonly IBloodAvailabilityServiceClient _client;

    public GetBloodAvailabilityMetadataQueryHandler(IBloodAvailabilityServiceClient client)
    {
        _client = client;
    }

    public async Task<BloodAvailabilityMetadataViewModel> Handle(GetBloodAvailabilityMetadataQuery request,
        CancellationToken cancellationToken)
    {
        var metadata = await _client.GetMetadataAsync(cancellationToken);
        return new BloodAvailabilityMetadataViewModel
        {
            Hospitals = metadata.Hospitals,
            HospitalSizes = metadata.HospitalSizes,
            Months = metadata.Months,
            BloodTypes = metadata.BloodTypes,
            TargetClasses = metadata.TargetClasses,
            TestAccuracy = metadata.TestAccuracy
        };
    }
}
