using DamiFYP.Application.Features.DonationRequests;
using DamiFYP.Application.Helpers;
using DamiFYP.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

        var donationPosts = await _context.DonationPosts
            .Where(dp => dp.DamiUserId == currentUserProfile.UserId)
            .Select(dp => new DonationPostViewModel
            {
                DonationPostId          = dp.Id,
                BloodTypeName           = dp.BloodTypeName.ToString(),
                Quantity                = dp.Quantity,
                DonorAddress            = dp.Address ?? "",
                Latitude                = dp.Latitude,
                Longitude               = dp.Longitude,
                Status                  = dp.Status,
                IsMatched               = _context.Matches.Any(m => m.DonationPostId == dp.Id),
                ReceivedFeedbackRating  = _context.DonorFeedbacks
                    .Where(f => _context.Matches.Any(m => m.DonationPostId == dp.Id && m.DonationRequestId == f.DonationRequestId))
                    .Select(f => (int?)f.Rating)
                    .FirstOrDefault(),
                ReceivedFeedbackComment = _context.DonorFeedbacks
                    .Where(f => _context.Matches.Any(m => m.DonationPostId == dp.Id && m.DonationRequestId == f.DonationRequestId))
                    .Select(f => f.Comment)
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        return donationPosts;
    }
}