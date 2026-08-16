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

public class CancelDonationRequestCommand : IRequest<Unit>
{
    public long Id { get; set; }
}

public class CancelDonationRequestCommandHandler : IRequestHandler<CancelDonationRequestCommand, Unit>
{
    private readonly DamiContext _context;
    private readonly ICurrentUserProfileService _currentUserProfileService;
    private readonly IEmailService _emailService;
    private readonly IHubContext<DamiHub> _hubContext;

    public CancelDonationRequestCommandHandler(
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

    public async Task<Unit> Handle(CancelDonationRequestCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _currentUserProfileService.GetCurrentAsync(cancellationToken);
        var entity = await _context.DonationRequests
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null) return Unit.Value;
        if (entity.DamiUserId != currentUser!.UserId) throw new UnauthorizedAccessException();
        if (entity.Status != DonationRequestStatus.Matched)
            throw new InvalidOperationException("Only matched requests can be cancelled.");

        // Find the matched donor before changing status
        var match = await _context.Matches
            .Include(m => m.DonationPost)
                .ThenInclude(dp => dp.DamiUser)
            .FirstOrDefaultAsync(m => m.DonationRequestId == request.Id, cancellationToken);

        entity.Status = DonationRequestStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);

        if (match?.DonationPost?.DamiUser is { } donor)
        {
            var bloodType = entity.BloodTypeName.ToString();

            await _emailService.SendEmailAsync(
                donor.Email,
                donor.Name,
                "Blood donation request cancelled",
                $"Hi {donor.Name},\n\n" +
                $"The blood donation request you were matched with ({currentUser.Name}, {bloodType}) has been cancelled by the seeker.\n\n" +
                $"Your donation post is now available to others again.\n\n" +
                $"Thank you for your willingness to help.",
                cancellationToken);

            await _hubContext.Clients
                .Group($"user-{donor.Id}")
                .SendAsync("RequestCancelled", new { seekerName = currentUser.Name, bloodType }, cancellationToken);
        }

        return Unit.Value;
    }
}
