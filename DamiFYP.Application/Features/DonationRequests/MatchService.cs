using DamiFYP.Persistence.Contexts;
using DamiFYP.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using DamiFYP.Application.Helpers;

namespace DamiFYP.Application.Features.DonationRequests;

// Simple matching service. Keeps logic minimal:
// - returns compatible donation post candidates to the UI
// - creates Match, Conversation and ConversationParticipants only after user picks one
public class MatchService : IMatchService
{
    private readonly DamiContext _context;
    private readonly ICurrentUserProfileService _currentUserProfileService;
    private readonly IMapper _mapper;

    public MatchService(DamiContext context, ICurrentUserProfileService currentUserProfileService, IMapper mapper)
    {
        _context = context;
        _currentUserProfileService = currentUserProfileService;
        _mapper = mapper;
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
                Quantity = p.Quantity
            })
            .ToListAsync(cancellationToken);


        return new DonationRequestMatchCandidatesViewModel()
        {
            DonationRequest = requestViewModel, Candidates = donationPostViewModels
        };
    }

    public async Task ConfirmMatch(long donationRequestId, long donationPostId, CancellationToken cancellationToken)
    {
        var exists = await _context.Matches
            .AsNoTracking()
            .AnyAsync(m => m.DonationPostId == donationPostId && m.DonationRequestId == donationRequestId,
                cancellationToken);

        if (exists)
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
    }
}