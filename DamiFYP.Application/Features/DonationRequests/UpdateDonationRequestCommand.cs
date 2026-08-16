using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using DamiFYP.Persistence.Contexts;
using DamiFYP.Domain.Models;
using DamiFYP.Application.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DamiFYP.Application.Features.DonationRequests;

public class UpdateDonationRequestCommand : IRequest<DonationRequestViewModel?>
{
    public long Id { get; set; }
    public string? BloodTypeName { get; set; }
    public int? Quantity { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
    public DonationRequestUrgency Urgency { get; set; }
    public DateTime? NeededByDate { get; set; }
}

public class UpdateDonationRequestCommandHandler : IRequestHandler<UpdateDonationRequestCommand, DonationRequestViewModel?>
{
    private readonly DamiContext _context;
    private readonly ICurrentUserProfileService _currentUserProfileService;

    public UpdateDonationRequestCommandHandler(DamiContext context, ICurrentUserProfileService currentUserProfileService)
    {
        _context = context;
        _currentUserProfileService = currentUserProfileService;
    }

    public async Task<DonationRequestViewModel?> Handle(UpdateDonationRequestCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _currentUserProfileService.GetCurrentAsync(cancellationToken);
        var entity = await _context.DonationRequests
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null) return null;
        if (entity.DamiUserId != currentUser!.UserId) throw new UnauthorizedAccessException();
        if (entity.Status != DonationRequestStatus.Pending)
            throw new InvalidOperationException("Request can only be edited while pending.");

        if (!string.IsNullOrWhiteSpace(request.BloodTypeName) && Enum.TryParse<BloodTypeName>(request.BloodTypeName, out var bt))
            entity.BloodTypeName = bt;
        if (request.Quantity.HasValue) entity.Quantity = request.Quantity;
        if (request.Latitude.HasValue) entity.Latitude = request.Latitude;
        if (request.Longitude.HasValue) entity.Longitude = request.Longitude;
        if (request.Address != null) entity.Address = request.Address;
        entity.Urgency = request.Urgency;
        entity.NeededByDate = request.NeededByDate;

        await _context.SaveChangesAsync(cancellationToken);

        return new DonationRequestViewModel
        {
            Id          = entity.Id,
            DamiUserId  = entity.DamiUserId,
            BloodTypeName = entity.BloodTypeName.ToString(),
            Quantity    = entity.Quantity,
            Latitude    = entity.Latitude,
            Longitude   = entity.Longitude,
            Address     = entity.Address,
            Urgency     = entity.Urgency,
            Status      = entity.Status,
            CreatedAt   = entity.CreatedAt,
            NeededByDate = entity.NeededByDate,
        };
    }
}
