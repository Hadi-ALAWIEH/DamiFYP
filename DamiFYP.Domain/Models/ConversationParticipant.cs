namespace DamiFYP.Domain.Models;

public class ConversationParticipant
{
    public long Id { get; set; }
    public long DamiUserId { get; set; }
    public long ConversationId { get; set; }

    public DamiUser DamiUser { get; set; }
    public Conversation Conversation { get; set; }
    public ICollection<Message> Messages { get; set; } = new List<Message>();

}