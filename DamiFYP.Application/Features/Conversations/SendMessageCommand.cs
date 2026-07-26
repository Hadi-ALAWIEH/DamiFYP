using DamiFYP.Domain.Models;
using DamiFYP.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DamiFYP.Application.Features.Conversations;

public class SendMessageCommand : IRequest<MessageViewModel>
{
    public long ConversationId { get; set; }
    public long SenderUserId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageViewModel>
{
    private readonly DamiContext _context;

    public SendMessageCommandHandler(DamiContext context)
    {
        _context = context;
    }

    public async Task<MessageViewModel> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException("Message content cannot be empty.", nameof(request.Content));
        }

        // A message hangs off the sender's ConversationParticipant row, and that row
        // only exists if MatchService created it when the match was confirmed - so
        // looking it up here doubles as the "are you actually part of this chat" check.
        var participant = await _context.ConversationParticipants
            .Include(p => p.DamiUser)
            .FirstOrDefaultAsync(
                p => p.ConversationId == request.ConversationId && p.DamiUserId == request.SenderUserId,
                cancellationToken);

        if (participant == null)
        {
            throw new UnauthorizedAccessException("User is not a participant of this conversation.");
        }

        var message = new Message
        {
            ConversationParticipantId = participant.Id,
            Content = request.Content.Trim(),
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);

        return new MessageViewModel
        {
            MessageId = message.Id,
            ConversationId = request.ConversationId,
            SenderUserId = participant.DamiUserId,
            SenderName = participant.DamiUser.Name,
            Content = message.Content,
            SentAt = message.SentAt,
            IsRead = message.IsRead
        };
    }
}
