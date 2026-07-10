using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using DamiFYP.Persistence.Contexts;
using DamiFYP.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DamiFYP.Application.Features.DonationRequests;

public class UpdateDonationRequestCommand : IRequest<DonationRequestViewModel>
{
    public int Id { get; set; }
    public long? UserId { get; set; }
    public string? BloodTypeName { get; set; }
    public int? Quantity { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? UrgencyLevel { get; set; }
    public string? Status { get; set; }
    public DateTime? NeededByDate { get; set; }
}

public class UpdateDonationRequestCommandHandler : IRequestHandler<UpdateDonationRequestCommand, DonationRequestViewModel>
{
    private readonly DamiContext _context;

    public UpdateDonationRequestCommandHandler(DamiContext context)
    {
        _context = context;
    }

    public async Task<DonationRequestViewModel> Handle(UpdateDonationRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.DonationRequests.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (entity == null) return null!;

        if (request.UserId.HasValue) entity.UserId = request.UserId.Value;
        if (!string.IsNullOrWhiteSpace(request.BloodTypeName) && Enum.TryParse<BloodTypeName>(request.BloodTypeName, out var bt))
            entity.BloodTypeName = bt;
        if (request.Quantity.HasValue) entity.Quantity = request.Quantity;
        if (request.Latitude.HasValue) entity.Latitude = request.Latitude;
        if (request.Longitude.HasValue) entity.Longitude = request.Longitude;
        if (request.UrgencyLevel != null) entity.UrgencyLevel = request.UrgencyLevel;
        if (request.Status != null) entity.Status = request.Status;
        entity.NeededByDate = request.NeededByDate;

        await _context.SaveChangesAsync(cancellationToken);

        return new DonationRequestViewModel
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
        };
    }
}


