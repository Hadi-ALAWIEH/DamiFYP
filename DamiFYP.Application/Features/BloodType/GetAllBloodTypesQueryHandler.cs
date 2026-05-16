using AutoMapper;
using DamiFYP.Extensions;
using DamiFYP.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DamiFYP.Application.Features.BloodType;

public class GetAllBloodTypesQueryHandler : IRequestHandler<GetAllBloodTypesQuery, List<BloodTypeViewModel>>
{
    private readonly DamiContext _context;
    private readonly IMapper _mapper;

    public GetAllBloodTypesQueryHandler(DamiContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // public async Task<List<BloodTypeViewModel>> Handle(GetAllBloodTypesQuery request,
    //     CancellationToken cancellationToken)
    // {
    //     IEnumerable<Domain.Models.BloodType> bloodTypes = await _context.BloodTypes.ToListAsync(cancellationToken);
    //     var isNegative = false;
    //     var things = _mapper.Map<List<Domain.Models.BloodType>, List<BloodTypeViewModel>>(
    //         bloodTypes
    //             .WhereIf(isNegative, types => types.Where(type => type.Id == 2)).ToList()
    //     );
    //
    //     return things;
    // }

    public async Task<List<BloodTypeViewModel>> Handle(GetAllBloodTypesQuery request,
            CancellationToken cancellationToken)
        => _mapper.Map<List<BloodTypeViewModel>>(await _context.BloodTypes.ToListAsync(cancellationToken));

    // public async Task<List<BloodTypeViewModel>> Handle(GetAllBloodTypesQuery request, CancellationToken cancellationToken)
    // {
    //     IEnumerable<Domain.Models.BloodType> bloodTypes = await _context.BloodTypes.ToListAsync(cancellationToken);
    //     var isNegative = false;
    //     var things = _mapper.Map<List<Domain.Models.BloodType>, List<BloodTypeViewModel>>(
    //         bloodTypes
    //             .WhereIf(isNegative, types => types.Where(type => type.Id == 1)).ToList()
    //     );
    //
    //     return things;
    // }
}