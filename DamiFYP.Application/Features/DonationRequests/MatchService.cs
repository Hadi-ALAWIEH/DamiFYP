using DamiFYP.Persistence.Contexts;
using DamiFYP.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DamiFYP.Application.Features.DonationRequests;

// Simple matching service. Keeps logic minimal:
// - returns compatible donation post candidates to the UI
// - creates Match, Conversation and ConversationParticipants only after user picks one
public class MatchService : IMatchService
{
    private readonly DamiContext _context;

    public MatchService(DamiContext context)
    {
        _context = context;
    }

    public async Task<List<DonationPostMatchCandidateViewModel>> GetCandidates(long donationRequestId, CancellationToken cancellationToken)
    {
        var request = await _context.DonationRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == donationRequestId, cancellationToken);

        if (request == null)
            return new List<DonationPostMatchCandidateViewModel>();

        return await _context.DonationPosts
            .AsNoTracking()
            .Where(p => p.BloodTypeName == request.BloodTypeName)
            .Select(p => new DonationPostMatchCandidateViewModel
            {
                DonationPostId = p.Id,
                DonorUserId = p.UserId,
                BloodTypeName = p.BloodTypeName.ToString(),
                Quantity = p.Quantity
            })
            .ToListAsync(cancellationToken);
    }

    public async Task ConfirmMatch(long donationRequestId, long donationPostId, CancellationToken cancellationToken)
    {
        var exists = await _context.Matches
            .AsNoTracking()
            .AnyAsync(m => m.DonationPostId == donationPostId && m.DonationRequestId == donationRequestId, cancellationToken);

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
            new ConversationParticipant { ConversationId = conversation.Id, UserId = request.UserId },
            new ConversationParticipant { ConversationId = conversation.Id, UserId = post.UserId });

        await _context.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }
}


