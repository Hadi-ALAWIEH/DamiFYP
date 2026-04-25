namespace DamiFYP.Domain.Models;

public class ConversationParticipant
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long ConversationId { get; set; }

    public User User { get; set; }
    public Conversation Conversation { get; set; }
    public ICollection<Message> Messages { get; set; } = new List<Message>();

}