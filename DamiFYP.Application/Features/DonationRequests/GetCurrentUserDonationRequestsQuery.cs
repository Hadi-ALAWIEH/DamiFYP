using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using DamiFYP.Application.Helpers;
using DamiFYP.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace DamiFYP.Application.Features.DonationRequests;

public class GetCurrentUserDonationRequestsQuery : IRequest<List<DonationRequestViewModel>> { }

public class GetCurrentUserDonationRequestsQueryHandler : IRequestHandler<GetCurrentUserDonationRequestsQuery, List<DonationRequestViewModel>>
{
    private readonly DamiContext _context;
    private readonly ICurrentUserProfileService _currentUserProfileService;

    public GetCurrentUserDonationRequestsQueryHandler(DamiContext context, ICurrentUserProfileService currentUserProfileService)
    {
        _context = context;
        _currentUserProfileService = currentUserProfileService;
    }

    public async Task<List<DonationRequestViewModel>> Handle(GetCurrentUserDonationRequestsQuery request, CancellationToken cancellationToken)
    {
        // Get the current user's profile
        var userProfile = await _currentUserProfileService.GetCurrentAsync(cancellationToken);

        if (userProfile == null)
        {
            return new List<DonationRequestViewModel>();
        }

        // Fetch all donation requests for the current user, including feedback via nav property.
        return await _context.DonationRequests
            .AsNoTracking()
            .Where(x => x.DamiUserId == userProfile.UserId)
            .Select(x => new DonationRequestViewModel
            {
                Id              = x.Id,
                DamiUserId      = x.DamiUserId,
                BloodTypeName   = x.BloodTypeName.ToString(),
                Quantity        = x.Quantity,
                Latitude        = x.Latitude,
                Longitude       = x.Longitude,
                Address         = x.Address,
                Urgency         = x.Urgency,
                Status          = x.Status,
                CreatedAt       = x.CreatedAt,
                NeededByDate    = x.NeededByDate,
                FeedbackRating  = x.DonorFeedback != null ? (int?)x.DonorFeedback.Rating  : null,
                FeedbackComment = x.DonorFeedback != null ? x.DonorFeedback.Comment        : null,
                HasFeedback     = x.DonorFeedback != null,
            })
            .ToListAsync(cancellationToken);
    }
}

