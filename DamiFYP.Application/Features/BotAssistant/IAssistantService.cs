using DamiFYP.Domain.Models;

namespace DamiFYP.Application.Features.BotAssistant;

// Thin seam around whichever LLM provider we're calling (Gemini today), so
// the endpoint/command layer (step 9) never touches Google.GenAI directly.
public interface IAssistantService
{
    Task<string> GetReplyAsync(IReadOnlyList<BotMessage> history, string userMessage,
        CancellationToken cancellationToken = default);
}
