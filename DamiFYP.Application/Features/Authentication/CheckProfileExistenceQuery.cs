using DamiFYP.Application.Helpers;
using DamiFYP.Domain.Models;
using MediatR;
namespace DamiFYP.Application.Features.Authentication;

public class CheckProfileExistenceQuery : IRequest<CheckProfileExistenceViewModel>
{

}

public class CheckProfileExistenceViewModel
{
    public bool Completed { get; set; }
}

public class CheckProfileExistenceQueryHandler : IRequestHandler<CheckProfileExistenceQuery, CheckProfileExistenceViewModel>
{
    private readonly ICurrentUserProfileService _profileService;

    public CheckProfileExistenceQueryHandler(ICurrentUserProfileService profileService)
    {
        _profileService = profileService;
    }

    public async Task<CheckProfileExistenceViewModel> Handle(CheckProfileExistenceQuery request, CancellationToken cancellationToken)
    {
        var profile = await _profileService.GetCurrentAsync(cancellationToken);
        return new()
            {
                // "Completed" is the single gate the frontend routes on (main.tsx / App.tsx).
                // It now requires BOTH the onboarding form having been saved (Name is no
                // longer the bootstrap placeholder) AND identity verification having passed -
                // a user who filled the form but hasn't verified yet must still be routed
                // back to onboarding, not into the app.
                Completed = profile.Name != "Pending Profile"
                            && profile.VerificationStatus == VerificationStatus.Verified
            };
    }
}
