namespace DamiFYP.Application.Features.Conversations;

public class MessageViewModel
{
    public long MessageId { get; set; }

    public long ConversationId { get; set; }

    public long SenderUserId { get; set; }

    public string SenderName { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }

    public bool IsRead { get; set; }
}
