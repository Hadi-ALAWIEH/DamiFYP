using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using DamiFYP.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace DamiFYP.Application.Features.DonationRequests;

public class GetAllDonationRequestsQuery : IRequest<List<DonationRequestViewModel>> { }

public class GetAllDonationRequestsQueryHandler : IRequestHandler<GetAllDonationRequestsQuery, List<DonationRequestViewModel>>
{
    private readonly DamiContext _context;

    public GetAllDonationRequestsQueryHandler(DamiContext context)
    {
        _context = context;
    }

    public async Task<List<DonationRequestViewModel>> Handle(GetAllDonationRequestsQuery request, CancellationToken cancellationToken)
    {
        var entities = await _context.DonationRequests.AsNoTracking().ToListAsync(cancellationToken);
        return entities.Select(entity => new DonationRequestViewModel
        {
            Id = entity.Id,
            UserId = entity.UserId,
            BloodTypeName = entity.BloodTypeName.ToString(),
            Quantity = entity.Quantity,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            UrgencyLevel = entity.UrgencyLevel,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            NeededByDate = entity.NeededByDate
        }).ToList();
    }
}

