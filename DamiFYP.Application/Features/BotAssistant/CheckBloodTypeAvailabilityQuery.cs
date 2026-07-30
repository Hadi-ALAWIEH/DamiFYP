using DamiFYP.Domain.Models;
using DamiFYP.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DamiFYP.Application.Features.BotAssistant;

// Simple, real-time "is this blood type available" check, backed directly by
// our own DB (DonationPosts + registered donors) — NOT the ML forecast in
// Features/BloodAvailability, which predicts future stock from hospital
// operational stats the bot doesn't have access to.
//
// "Available" here means: donor posts for this blood type that have not
// already been matched to a request, plus a count of registered donors of
// this type who currently have themselves marked as available.
public class CheckBloodTypeAvailabilityQuery : IRequest<BloodTypeAvailabilityViewModel>
{
    public BloodTypeName BloodTypeName { get; set; }
}

public class BloodTypeAvailabilityViewModel
{
    public string BloodTypeName { get; set; } = string.Empty;
    public int UnmatchedDonationPostCount { get; set; }
    public int TotalPledgedQuantity { get; set; }
    public int AvailableDonorCount { get; set; }
}

public class CheckBloodTypeAvailabilityQueryHandler
    : IRequestHandler<CheckBloodTypeAvailabilityQuery, BloodTypeAvailabilityViewModel>
{
    private readonly DamiContext _context;

    public CheckBloodTypeAvailabilityQueryHandler(DamiContext context)
    {
        _context = context;
    }

    public async Task<BloodTypeAvailabilityViewModel> Handle(CheckBloodTypeAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        var unmatchedPosts = await _context.DonationPosts
            .Where(dp => dp.BloodTypeName == request.BloodTypeName)
            .Where(dp => !dp.Matches.Any())
            .ToListAsync(cancellationToken);

        var availableDonorCount = await _context.BloodTypes
            .Where(bt => bt.BloodTypeName == request.BloodTypeName)
            .Where(bt => bt.DamiUser.IsAvailable)
            .CountAsync(cancellationToken);

        return new BloodTypeAvailabilityViewModel
        {
            BloodTypeName = request.BloodTypeName.ToString(),
            UnmatchedDonationPostCount = unmatchedPosts.Count,
            TotalPledgedQuantity = unmatchedPosts.Sum(p => p.Quantity ?? 0),
            AvailableDonorCount = availableDonorCount
        };
    }
}
