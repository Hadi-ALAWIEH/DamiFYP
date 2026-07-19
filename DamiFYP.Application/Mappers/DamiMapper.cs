using AutoMapper;
using DamiFYP.Application.Features.BloodType;
using DamiFYP.Application.Features.DonationRequests;
using DamiFYP.Domain.Models;

namespace DamiFYP.Application.Mappers;

public class DamiMapper : Profile
{
    public DamiMapper()
    {
        AddGlobalIgnore("Item");
        CreateMap<BloodType, BloodTypeViewModel>()
            .ForMember(model => model.Id,
                opts => opts.MapFrom(model => model.Id))
            .ForMember(model => model.Description,
                opts => opts.MapFrom(model => model.BloodTypeName));

        CreateMap<DonationRequest, DonationRequestViewModel>()
            .ForMember(model => model.BloodTypeName,
                opts => opts.MapFrom((model => model.BloodTypeName.ToString())));
    }
}