using DamiFYP.Application.Helpers;
using DamiFYP.Domain.Models;
using DamiFYP.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DamiFYP.Application.Features.BotAssistant;

public class SendBotMessageCommand : IRequest<BotMessageViewModel>
{
    public string Message { get; set; } = string.Empty;
}

public class SendBotMessageCommandHandler : IRequestHandler<SendBotMessageCommand, BotMessageViewModel>
{
    private const int MaxMessageLength = 2000;
    private const int HistoryWindowSize = 20;

    private readonly ICurrentUserProfileService _profileService;
    private readonly IAssistantService _assistantService;
    private readonly DamiContext _context;
    private readonly ILogger<SendBotMessageCommandHandler> _logger;

    public SendBotMessageCommandHandler(ICurrentUserProfileService profileService, IAssistantService assistantService,
        DamiContext context, ILogger<SendBotMessageCommandHandler> logger)
    {
        _profileService = profileService;
        _assistantService = assistantService;
        _context = context;
        _logger = logger;
    }

    public async Task<BotMessageViewModel> Handle(SendBotMessageCommand request, CancellationToken cancellationToken)
    {
        var message = request.Message.Trim();
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentException("Message cannot be empty.");
        }

        if (message.Length > MaxMessageLength)
        {
            throw new ArgumentException($"Message cannot exceed {MaxMessageLength} characters.");
        }

        var profile = await _profileService.GetCurrentAsync(cancellationToken);
        if (profile == null)
        {
            throw new UnauthorizedAccessException("No current user profile.");
        }

        // Recent window only — keeps prompt size (and Gemini token usage) bounded
        // as a user's bot history grows over time.
        var history = await _context.BotMessages
            .Where(m => m.DamiUserId == profile.UserId)
            .OrderByDescending(m => m.SentAt)
            .Take(HistoryWindowSize)
            .OrderBy(m => m.SentAt)
            .ToListAsync(cancellationToken);

        string replyText;
        try
        {
            replyText = await _assistantService.GetReplyAsync(history, message, cancellationToken);
        }
        catch (AssistantRateLimitedException ex)
        {
            _logger.LogWarning(ex, "Assistant shared quota exhausted for user {UserId}", profile.UserId);
            replyText = "The assistant is handling a lot of requests right now — please try again in a minute.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Assistant call failed for user {UserId}", profile.UserId);
            replyText = "Sorry, I'm unable to respond right now. Please try again in a moment.";
        }

        var userMessage = new BotMessage
        {
            DamiUserId = profile.UserId,
            Role = BotMessageRole.User,
            Content = message,
            SentAt = DateTime.UtcNow
        };
        var assistantMessage = new BotMessage
        {
            DamiUserId = profile.UserId,
            Role = BotMessageRole.Assistant,
            Content = replyText,
            SentAt = DateTime.UtcNow
        };

        _context.BotMessages.Add(userMessage);
        _context.BotMessages.Add(assistantMessage);
        await _context.SaveChangesAsync(cancellationToken);

        return new BotMessageViewModel
        {
            Role = assistantMessage.Role.ToString(),
            Content = assistantMessage.Content,
            SentAt = assistantMessage.SentAt
        };
    }
}
