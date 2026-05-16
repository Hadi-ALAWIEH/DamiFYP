using MediatR;

namespace DamiFYP.Application.Features.BloodType;

public class GetAllBloodTypesQuery : IRequest<List<BloodTypeViewModel>>
{

}