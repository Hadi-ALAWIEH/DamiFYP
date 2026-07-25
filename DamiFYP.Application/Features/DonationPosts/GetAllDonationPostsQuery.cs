using DamiFYP.Application.Features.DonationRequests;
using DamiFYP.Application.Helpers;
using DamiFYP.Persistence.Contexts;
using MediatR;

public class GetCurrentUserDonationPostsQuery : IRequest<List<DonationPostViewModel>>
{
}


public class
    GetCurrentUserDonationPostsQueryHandler : IRequestHandler<GetCurrentUserDonationPostsQuery, List<DonationPostViewModel>>
{
    private readonly ICurrentUserProfileService _profileService;
    private readonly DamiContext _context;

    public GetCurrentUserDonationPostsQueryHandler(ICurrentUserProfileService profileService, DamiContext context)
    {
        _profileService = profileService;
        _context = context;
    }

    public async Task<List<DonationPostViewModel>> Handle(GetCurrentUserDonationPostsQuery request,
        CancellationToken cancellationToken)
    {
        var currentUserProfile = await _profileService.GetCurrentAsync(cancellationToken);
        var donationPosts = _context.DonationPosts.Where(dp => dp.DamiUserId == currentUserProfile.UserId).ToList();

        return donationPosts.Select(dp => new DonationPostViewModel()
        {
            BloodTypeName = dp.BloodTypeName.ToString(),
            Quantity = dp.Quantity,
        }).ToList();
    }
}