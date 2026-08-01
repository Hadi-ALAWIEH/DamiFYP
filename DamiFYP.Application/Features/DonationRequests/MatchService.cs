using DamiFYP.Persistence.Contexts;
using DamiFYP.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using DamiFYP.Application.Features.Conversations;
using DamiFYP.Application.Helpers;
using DamiFYP.Infrastructure.Email;
using Microsoft.AspNetCore.SignalR;

namespace DamiFYP.Application.Features.DonationRequests;

// Simple matching service. Keeps logic minimal:
// - returns compatible donation post candidates to the UI
// - creates Match, Conversation and ConversationParticipants only after user picks one
public class MatchService : IMatchService
{
    private readonly DamiContext _context;
    private readonly ICurrentUserProfileService _currentUserProfileService;
    private readonly IMapper _mapper;
    private readonly IHubContext<DamiHub> _hubContext;
    private readonly IEmailService _emailService;

    public MatchService(DamiContext context, ICurrentUserProfileService currentUserProfileService, IMapper mapper,
        IHubContext<DamiHub> hubContext, IEmailService emailService)
    {
        _context = context;
        _currentUserProfileService = currentUserProfileService;
        _mapper = mapper;
        _hubContext = hubContext;
        _emailService = emailService;
    }

    public async Task<DonationRequestMatchCandidatesViewModel> GetCandidatesAsync(long donationRequestId,
        CancellationToken cancellationToken)
    {
        var currentUserId = (await _currentUserProfileService.GetCurrentAsync(cancellationToken)).UserId;

        var request = await _context.DonationRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(r =>
                    r.Id == donationRequestId && r.DamiUserId == currentUserId,
                cancellationToken);

        var requestViewModel = _mapper.Map<DonationRequestViewModel>(request);

        if (request == null)
            return new DonationRequestMatchCandidatesViewModel();

        // Donor posts already matched to this request — used to flag IsMatched
        // below so a donor who was confirmed in an earlier session still shows
        // as "Matched" (not a fresh "Confirm Match" button) after a reload.
        var matchedPostIds = await _context.Matches
            .AsNoTracking()
            .Where(m => m.DonationRequestId == donationRequestId)
            .Select(m => m.DonationPostId)
            .ToListAsync(cancellationToken);

        var donationPostViewModels = await _context.DonationPosts
            .AsNoTracking()
            .Where(p => p.BloodTypeName == request.BloodTypeName)
            .Where(p => p.Quantity >= request.Quantity)
            .Include(donationPost => donationPost.DamiUser)
            .Select(p => new DonationPostViewModel
            {
                DonationPostId = p.Id,
                DonorUserId = p.DamiUserId,
                DonorName = p.DamiUser.Name,
                DonorAddress = "",
                BloodTypeName = p.BloodTypeName.ToString(),
                Quantity = p.Quantity,
                IsMatched = matchedPostIds.Contains(p.Id)
            })
            .ToListAsync(cancellationToken);


        return new DonationRequestMatchCandidatesViewModel()
        {
            DonationRequest = requestViewModel, Candidates = donationPostViewModels
        };
    }

    public async Task ConfirmMatch(long donationRequestId, long donationPostId, CancellationToken cancellationToken)
    {
        // A given donor can only be matched to a given request once. This is the
        // authoritative, persisted guard (backed by the Matches table itself,
        // not a derived status flag) — re-submitting "Confirm Match" for the
        // same (request, donor) pair is a no-op instead of creating a second
        // Match/Conversation and re-sending the "you've been matched" email.
        var alreadyMatched = await _context.Matches
            .AsNoTracking()
            .AnyAsync(m => m.DonationPostId == donationPostId && m.DonationRequestId == donationRequestId,
                cancellationToken);

        if (alreadyMatched)
            return;

        var request = await _context.DonationRequests
            .FirstOrDefaultAsync(r => r.Id == donationRequestId, cancellationToken);

        var post = await _context.DonationPosts
            .FirstOrDefaultAsync(p => p.Id == donationPostId, cancellationToken);

        if (request == null || post == null)
            return;

        using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);

        var match = new Match
        {
            DonationPostId = post.Id,
            DonationRequestId = request.Id
        };

        _context.Matches.Add(match);
        await _context.SaveChangesAsync(cancellationToken);

        var conversation = new Conversation
        {
            MatchId = match.Id
        };

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync(cancellationToken);

        _context.ConversationParticipants.AddRange(
            new ConversationParticipant { ConversationId = conversation.Id, DamiUserId = request.DamiUserId },
            new ConversationParticipant { ConversationId = conversation.Id, DamiUserId = post.DamiUserId });

        await _context.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        await NotifyBothPartiesAsync(conversation.Id, match.Id, request.DamiUserId, post.DamiUserId,
            cancellationToken);
    }

    // Pushes a "ConversationStarted" SignalR event to both matched users' personal
    // groups (joined in DamiHub.OnConnectedAsync) so their clients can open the new
    // chat immediately instead of waiting for the next GetAllConversations poll.
    // Also emails both parties, since they might not be online to receive the
    // SignalR event. Runs after the transaction commits, so it only fires once
    // the match is durable.
    private async Task NotifyBothPartiesAsync(long conversationId, long matchId, long seekerUserId,
        long donorUserId, CancellationToken cancellationToken)
    {
        var users = await _context.DamiUsers
            .AsNoTracking()
            .Where(user => user.Id == seekerUserId || user.Id == donorUserId)
            .Select(user => new { user.Id, user.Name, user.Email })
            .ToDictionaryAsync(user => user.Id, cancellationToken);

        var seekerName = users.TryGetValue(seekerUserId, out var seeker) ? seeker.Name : string.Empty;
        var seekerEmail = users.TryGetValue(seekerUserId, out var seekerInfo) ? seekerInfo.Email : string.Empty;
        var donorName = users.TryGetValue(donorUserId, out var donor) ? donor.Name : string.Empty;
        var donorEmail = users.TryGetValue(donorUserId, out var donorInfo) ? donorInfo.Email : string.Empty;

        await _hubContext.Clients.Group(SignalRGroups.ForUser(seekerUserId)).SendAsync("ConversationStarted",
            new ConversationStartedNotification
            {
                ConversationId = conversationId,
                MatchId = matchId,
                OtherUserId = donorUserId,
                OtherUserName = donorName
            }, cancellationToken);

        await _hubContext.Clients.Group(SignalRGroups.ForUser(donorUserId)).SendAsync("ConversationStarted",
            new ConversationStartedNotification
            {
                ConversationId = conversationId,
                MatchId = matchId,
                OtherUserId = seekerUserId,
                OtherUserName = seekerName
            }, cancellationToken);

        await SendMatchEmailsAsync(seekerName, seekerEmail, donorName, donorEmail, cancellationToken);
    }

    // Sends the "you've been matched" notice to both the seeker and the donor.
    private async Task SendMatchEmailsAsync(string seekerName, string seekerEmail, string donorName,
        string donorEmail, CancellationToken cancellationToken)
    {
        const string subject = "You've been matched!";

        await _emailService.SendEmailAsync(seekerEmail, seekerName, subject,
            $"Hi {seekerName},\n\nYou've been matched with {donorName}. Open the app to start chatting and arrange your donation.",
            cancellationToken);

        await _emailService.SendEmailAsync(donorEmail, donorName, subject,
            $"Hi {donorName},\n\nYou've been matched with {seekerName}. Open the app to start chatting and arrange the donation.",
            cancellationToken);
    }
}