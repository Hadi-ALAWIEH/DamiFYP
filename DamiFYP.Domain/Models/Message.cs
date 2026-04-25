namespace DamiFYP.Domain.Models;

public class Message
{
    public long Id { get; set; }
    public long? ConversationParticipantId { get; set; }

    public ConversationParticipant ConversationParticipant { get; set; } = null!;

    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
}