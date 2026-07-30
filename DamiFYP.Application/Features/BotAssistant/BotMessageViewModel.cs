namespace DamiFYP.Application.Features.BotAssistant;

public class BotMessageViewModel
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}
