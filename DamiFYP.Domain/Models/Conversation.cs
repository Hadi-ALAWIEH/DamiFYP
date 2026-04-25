namespace DamiFYP.Domain.Models;

public class Conversation
{
    public long Id { get; set; }

    public long MatchId { get; set; }
    public Match Match { get; set; } = null!;

    ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
}