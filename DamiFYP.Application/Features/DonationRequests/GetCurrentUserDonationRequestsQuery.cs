using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
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
    private readonly IMapper _mapper;


    public GetCurrentUserDonationRequestsQueryHandler(DamiContext context, ICurrentUserProfileService currentUserProfileService, IMapper mapper)
    {
        _context = context;
        _currentUserProfileService = currentUserProfileService;
        _mapper = mapper;
    }

    public async Task<List<DonationRequestViewModel>> Handle(GetCurrentUserDonationRequestsQuery request, CancellationToken cancellationToken)
    {
        // Get the current user's profile
        var userProfile = await _currentUserProfileService.GetCurrentAsync(cancellationToken);

        if (userProfile == null)
        {
            return new List<DonationRequestViewModel>();
        }

        // Fetch all donation requests for the current user
        var entities = await _context.DonationRequests
            .AsNoTracking()
            .Where(x => x.DamiUserId == userProfile.UserId)
            .ToListAsync(cancellationToken);

        return entities.Select(entity => _mapper.Map<DonationRequestViewModel>(entity)).ToList();
        // return entities.Select(entity => new DonationRequestViewModel
        // {
        //     Id = entity.Id,
        //     UserId = entity.UserId,
        //     BloodTypeName = entity.BloodTypeName.ToString(),
        //     Quantity = entity.Quantity,
        //     Latitude = entity.Latitude,
        //     Longitude = entity.Longitude,
        //     Urgency = entity.Urgency,
        //     Status = entity.Status,
        //     CreatedAt = entity.CreatedAt,
        //     NeededByDate = entity.NeededByDate
        // }).ToList();
    }
}

