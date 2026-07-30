namespace DamiFYP.Application.Features.BotAssistant;

// Name of the ASP.NET Core rate limiting policy applied to the assistant's
// send-message endpoint. Keeps usage fair between individual users; the
// separate AssistantRateLimiter protects the shared Gemini quota app-wide.
public static class AssistantRateLimiterPolicy
{
    public const string Endpoint = "Assistant";
}
