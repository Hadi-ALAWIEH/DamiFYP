using DamiFYP.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DamiFYP.Application.Features.Conversations;

// Cheap authorization check used by DamiHub.SubscribeToConversation — unlike
// GetConversationMessagesQuery, this does NOT fetch history or mark anything
// as read, since subscribing is not the same thing as opening/reading a chat.
public class IsConversationParticipantQuery : IRequest<bool>
{
    public long ConversationId { get; set; }
    public long UserId { get; set; }
}

public class IsConversationParticipantQueryHandler : IRequestHandler<IsConversationParticipantQuery, bool>
{
    private readonly DamiContext _context;

    public IsConversationParticipantQueryHandler(DamiContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(IsConversationParticipantQuery request, CancellationToken cancellationToken)
    {
        return await _context.ConversationParticipants
            .AsNoTracking()
            .AnyAsync(
                p => p.ConversationId == request.ConversationId && p.DamiUserId == request.UserId,
                cancellationToken);
    }
}
