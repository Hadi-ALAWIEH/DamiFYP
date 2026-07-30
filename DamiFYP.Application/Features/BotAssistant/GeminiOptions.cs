namespace DamiFYP.Application.Features.BotAssistant;

// Bound from the "Gemini" configuration section. ApiKey is expected to come
// from .NET User Secrets locally (Gemini:ApiKey) and from the Gemini__ApiKey
// environment variable in deployed environments — never committed to
// appsettings.json.
public class GeminiOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-3.6-flash";
}
