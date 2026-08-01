using DamiFYP.Application.Helpers;
using DamiFYP.Domain.Models;
using DamiFYP.Infrastructure.Email;
using DamiFYP.Persistence.Contexts;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

public class CreateDonationPostCommand : IRequest
{
    public BloodTypeName BloodTypeName { get; set; }
    public int? Quantity { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

// Pushed over SignalR (event "DonationPostMatch") to every seeker whose pending
// donation request this new post satisfies, so their client can pop up a
// "a donor just posted a match for your request!" alert in real time.
public class DonationPostMatchNotification
{
    public long DonationPostId { get; set; }
    public long DonationRequestId { get; set; }
    public long DonorUserId { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public string? BloodTypeName { get; set; }
    public int? Quantity { get; set; }
    public string? DonorAddress { get; set; }
    public double? DonorLatitude { get; set; }
    public double? DonorLongitude { get; set; }
}

public class CreateDonationPostCommandHandler : IRequestHandler<CreateDonationPostCommand>
{
    private readonly DamiContext _context;
    private readonly ICurrentUserProfileService _currentUserProfileService;
    private readonly IHubContext<DamiHub> _hubContext;
    private readonly IEmailService _emailService;

    public CreateDonationPostCommandHandler(DamiContext context, ICurrentUserProfileService currentUserProfileService,
        IHubContext<DamiHub> hubContext, IEmailService emailService)
    {
        _context = context;
        _currentUserProfileService = currentUserProfileService;
        _hubContext = hubContext;
        _emailService = emailService;
    }

    public async Task Handle(CreateDonationPostCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _currentUserProfileService.GetCurrentAsync(cancellationToken);

        var post = new DonationPost()
        {
            BloodTypeName = request.BloodTypeName,
            Quantity      = request.Quantity,
            Address       = request.Address,
            Latitude      = request.Latitude,
            Longitude     = request.Longitude,
            DamiUserId    = currentUser.UserId
        };

        _context.DonationPosts.Add(post);
        await _context.SaveChangesAsync(cancellationToken);

        await NotifyMatchingSeekersAsync(post, currentUser.Name, cancellationToken);
    }

    // Finds every still-pending donation request this new post satisfies (same
    // blood type, enough quantity) and lets each seeker know right away - both
    // over SignalR for anyone currently online, and by email as a fallback for
    // anyone who isn't, mirroring how MatchService notifies a confirmed match.
    private async Task NotifyMatchingSeekersAsync(DonationPost post, string donorName,
        CancellationToken cancellationToken)
    {
        var matchingSeekers = await _context.DonationRequests
            .AsNoTracking()
            .Where(r => r.Status == DonationRequestStatus.Pending)
            .Where(r => r.BloodTypeName == post.BloodTypeName)
            .Where(r => r.Quantity <= post.Quantity)
            .Select(r => new { RequestId = r.Id, r.DamiUserId, r.DamiUser.Name, r.DamiUser.Email })
            .ToListAsync(cancellationToken);

        foreach (var seeker in matchingSeekers)
        {
            var notification = new DonationPostMatchNotification
            {
                DonationPostId = post.Id,
                DonationRequestId = seeker.RequestId,
                DonorUserId = post.DamiUserId,
                DonorName = donorName,
                BloodTypeName = post.BloodTypeName.ToString(),
                Quantity = post.Quantity,
                DonorAddress = post.Address,
                DonorLatitude = post.Latitude,
                DonorLongitude = post.Longitude
            };

            await _hubContext.Clients.Group(SignalRGroups.ForUser(seeker.DamiUserId))
                .SendAsync("DonationPostMatch", notification, cancellationToken);

            await _emailService.SendEmailAsync(seeker.Email, seeker.Name, "A matching donor just posted!",
                $"Hi {seeker.Name},\n\n{donorName} just posted a {notification.BloodTypeName} donation that matches your request. Open the app to review and confirm the match.",
                cancellationToken);
        }
    }
}