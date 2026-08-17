using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using DamiFYP.Persistence.Contexts;
using DamiFYP.Domain.Models;
using DamiFYP.Application.Helpers;
using DamiFYP.Infrastructure.Email;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DamiFYP.Application.Features.DonationRequests;

public class ConfirmDonationCommand : IRequest<Unit>
{
    public long DonationRequestId { get; set; }
}

public class ConfirmDonationCommandHandler : IRequestHandler<ConfirmDonationCommand, Unit>
{
    private readonly DamiContext _context;
    private readonly ICurrentUserProfileService _currentUserProfileService;
    private readonly IEmailService _emailService;
    private readonly IHubContext<DamiHub> _hubContext;

    public ConfirmDonationCommandHandler(
        DamiContext context,
        ICurrentUserProfileService currentUserProfileService,
        IEmailService emailService,
        IHubContext<DamiHub> hubContext)
    {
        _context = context;
        _currentUserProfileService = currentUserProfileService;
        _emailService = emailService;
        _hubContext = hubContext;
    }

    public async Task<Unit> Handle(ConfirmDonationCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _currentUserProfileService.GetCurrentAsync(cancellationToken);

        var donationRequest = await _context.DonationRequests
            .FirstOrDefaultAsync(r => r.Id == request.DonationRequestId, cancellationToken);

        if (donationRequest == null) return Unit.Value;
        if (donationRequest.DamiUserId != currentUser!.UserId) throw new UnauthorizedAccessException();
        if (donationRequest.Status != DonationRequestStatus.Matched)
            throw new InvalidOperationException("Only matched requests can be confirmed.");

        // Resolve the matched donor post via the Match record
        var match = await _context.Matches
            .Include(m => m.DonationPost)
                .ThenInclude(dp => dp.DamiUser)
            .FirstOrDefaultAsync(m => m.DonationRequestId == request.DonationRequestId, cancellationToken);

        // Mark both sides as completed
        donationRequest.Status = DonationRequestStatus.Completed;

        if (match?.DonationPost != null)
            match.DonationPost.Status = DonationPostStatus.Completed;

        // Award urgency-weighted points to the donor
        if (match?.DonationPost?.DamiUser is { } donor)
        {
            var points = donationRequest.Urgency switch
            {
                DonationRequestUrgency.Low    => 1,
                DonationRequestUrgency.Medium => 2,
                DonationRequestUrgency.High   => 3,
                _                             => 1,
            };

            var badge = await _context.DamiBadges
                .FirstOrDefaultAsync(b => b.DamiUserId == donor.Id, cancellationToken);

            if (badge == null)
            {
                badge = new DamiBadge { DamiUserId = donor.Id };
                _context.DamiBadges.Add(badge);
            }

            badge.DonationPoints += points;
            badge.Tier = badge.DonationPoints switch
            {
                >= 35 => BadgeTier.Hero,
                >= 15 => BadgeTier.Guardian,
                >= 5  => BadgeTier.Contributor,
                >= 1  => BadgeTier.Helper,
                _     => BadgeTier.Newcomer,
            };
            badge.LastDonationAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Bust the in-memory cache so the donor's next profile fetch
            // returns the updated badge tier immediately.
            await _currentUserProfileService.InvalidateAsync(donor.KeyCloakId);

            var bloodType = donationRequest.BloodTypeName.ToString();

            // Notify donor
            await _hubContext.Clients
                .Group(SignalRGroups.ForUser(donor.Id))
                .SendAsync("DonationConfirmed", new
                {
                    seekerName = currentUser.Name,
                    bloodType,
                    pointsEarned = points,
                    newTier      = badge.Tier.ToString(),
                    totalPoints  = badge.DonationPoints,
                }, cancellationToken);

            await _emailService.SendEmailAsync(
                donor.Email,
                donor.Name,
                "Your donation was confirmed — thank you!",
                $"Hi {donor.Name},\n\n" +
                $"{currentUser.Name} has confirmed that your blood donation ({bloodType}) was successfully received.\n\n" +
                $"You earned {points} point{(points > 1 ? "s" : "")} and are now a '{badge.Tier}' donor.\n\n" +
                $"Total points: {badge.DonationPoints}.\n\n" +
                $"Thank you for saving a life!",
                cancellationToken);
        }
        else
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
