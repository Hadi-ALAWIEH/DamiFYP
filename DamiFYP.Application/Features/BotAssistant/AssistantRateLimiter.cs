using System.Threading.RateLimiting;

namespace DamiFYP.Application.Features.BotAssistant;

// Protects the single shared Gemini API key's free-tier budget (about
// 15 requests/minute on the free tier) from being exhausted by requests
// piling up across ALL users at once. Registered as a singleton so every
// request funnels through the same window, regardless of which user made
// it. Deliberately set below Gemini's actual limit to leave headroom.
//
// This is separate from — and in addition to — the per-user throttling
// applied to the HTTP endpoint itself (see Program.cs's AddRateLimiter /
// AssistantRateLimiterPolicy), which exists to keep usage fair between
// individual users rather than to protect the shared quota.
public sealed class AssistantRateLimiter : IDisposable
{
    private readonly FixedWindowRateLimiter _limiter = new(new FixedWindowRateLimiterOptions
    {
        PermitLimit = 12,
        Window = TimeSpan.FromMinutes(1),
        QueueLimit = 0,
        AutoReplenishment = true
    });

    public bool TryAcquire()
    {
        using var lease = _limiter.AttemptAcquire();
        return lease.IsAcquired;
    }

    public void Dispose() => _limiter.Dispose();
}

// Thrown when the shared app-wide Gemini budget is exhausted, so callers can
// tell this apart from an actual Gemini/API failure and give the user a more
// accurate "try again shortly" message instead of a generic error.
public sealed class AssistantRateLimitedException : Exception
{
    public AssistantRateLimitedException()
        : base("The assistant is handling a lot of requests right now.")
    {
    }
}
