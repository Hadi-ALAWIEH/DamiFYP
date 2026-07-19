using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using DamiFYP.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace DamiFYP.Application.Features.DonationRequests;

public class GetDonationRequestQuery : IRequest<DonationRequestViewModel>
{
    public int Id { get; set; }
}

public class GetDonationRequestQueryHandler : IRequestHandler<GetDonationRequestQuery, DonationRequestViewModel>
{
    private readonly DamiContext _context;
    private readonly IMapper _mapper;

    public GetDonationRequestQueryHandler(DamiContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DonationRequestViewModel> Handle(GetDonationRequestQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.DonationRequests
            .Include(dr => dr.Matches)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null) return null!; // let controller handle nulls (will cause 204/500 depending)

        return _mapper.Map<DonationRequestViewModel>(entity);

        // return new DonationRequestViewModel
        // {
        //     Id = entity.Id,
        //     UserId = entity.UserId,
        //     BloodTypeName = entity.BloodTypeName.ToString(),
        //     Quantity = entity.Quantity,
        //     Latitude = entity.Latitude,
        //     Longitude = entity.Longitude,
        //     Urgency = entity.UrgencyLevel,
        //     Status = entity.Status,
        //     CreatedAt = entity.CreatedAt,
        //     NeededByDate = entity.NeededByDate
        // };
    }
}

