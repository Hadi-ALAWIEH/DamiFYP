using DamiFYP.Application.Helpers;
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
                Completed = profile.Name != "Pending Profile"
            };
    }
}

