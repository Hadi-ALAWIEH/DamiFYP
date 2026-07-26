using DamiFYP.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DamiFYP.Application.Features.Conversations;

public class GetConversationMessagesQuery : IRequest<List<MessageViewModel>>
{
    public long ConversationId { get; set; }
    public long RequestingUserId { get; set; }
}

public class GetConversationMessagesQueryHandler
    : IRequestHandler<GetConversationMessagesQuery, List<MessageViewModel>>
{
    private readonly DamiContext _context;

    public GetConversationMessagesQueryHandler(DamiContext context)
    {
        _context = context;
    }

    public async Task<List<MessageViewModel>> Handle(GetConversationMessagesQuery request,
        CancellationToken cancellationToken)
    {
        // Only a participant (i.e. one half of the matched donor/seeker pair) may
        // read the conversation's history - same rule as who may post into it.
        var isParticipant = await _context.ConversationParticipants
            .AsNoTracking()
            .AnyAsync(
                p => p.ConversationId == request.ConversationId && p.DamiUserId == request.RequestingUserId,
                cancellationToken);

        if (!isParticipant)
        {
            throw new UnauthorizedAccessException("User is not a participant of this conversation.");
        }

        return await _context.Messages
            .AsNoTracking()
            .Where(message => message.ConversationParticipant.ConversationId == request.ConversationId)
            .OrderBy(message => message.SentAt)
            .Select(message => new MessageViewModel
            {
                MessageId = message.Id,
                ConversationId = request.ConversationId,
                SenderUserId = message.ConversationParticipant.DamiUserId,
                SenderName = message.ConversationParticipant.DamiUser.Name,
                Content = message.Content,
                SentAt = message.SentAt,
                IsRead = message.IsRead
            })
            .ToListAsync(cancellationToken);
    }
}
