using DamiFYP.Application.Helpers;
using DamiFYP.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DamiFYP.Application.Features.BotAssistant;

// Hydrates the assistant chat window on first load. There's no live-push
// counterpart (unlike human conversations over DamiHub) because a reply is
// always a direct response to the request that produced it.
public class GetBotMessagesQuery : IRequest<List<BotMessageViewModel>>
{
}

public class GetBotMessagesQueryHandler : IRequestHandler<GetBotMessagesQuery, List<BotMessageViewModel>>
{
    private const int HistoryPageSize = 50;

    private readonly ICurrentUserProfileService _profileService;
    private readonly DamiContext _context;

    public GetBotMessagesQueryHandler(ICurrentUserProfileService profileService, DamiContext context)
    {
        _profileService = profileService;
        _context = context;
    }

    public async Task<List<BotMessageViewModel>> Handle(GetBotMessagesQuery request,
        CancellationToken cancellationToken)
    {
        var profile = await _profileService.GetCurrentAsync(cancellationToken);
        if (profile == null)
        {
            throw new UnauthorizedAccessException("No current user profile.");
        }

        var messages = await _context.BotMessages
            .Where(m => m.DamiUserId == profile.UserId)
            .OrderByDescending(m => m.SentAt)
            .Take(HistoryPageSize)
            .OrderBy(m => m.SentAt)
            .ToListAsync(cancellationToken);

        return messages.Select(m => new BotMessageViewModel
        {
            Role = m.Role.ToString(),
            Content = m.Content,
            SentAt = m.SentAt
        }).ToList();
    }
}
