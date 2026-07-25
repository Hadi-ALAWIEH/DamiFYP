using DamiFYP.Application.Helpers;
using MediatR;

public class GetUserProfileQuery : IRequest<UserProfile>{
}


public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfile>{
    private readonly ICurrentUserProfileService _profileService;

    public GetUserProfileQueryHandler(ICurrentUserProfileService profileService)
    {
        _profileService = profileService;
    }

    public async Task<UserProfile> Handle(GetUserProfileQuery request, CancellationToken cancellationToken) =>
        await _profileService.GetCurrentAsync();
}
