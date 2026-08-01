namespace DamiFYP.Domain.Models;

public class BotMessage
{
    public long Id { get; set; }
    public long DamiUserId { get; set; }
    public DamiUser DamiUser { get; set; } = null!;

    public BotMessageRole Role { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}

public enum BotMessageRole
{
    User,
    Assistant
}
