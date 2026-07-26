using DamiFYP.Application.Authorization;
using DamiFYP.Application.Features.Conversations;
using DamiFYP.Application.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DamiFYP.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConversationController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConversationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("get-all-conversations")]
    [Authorize(Policy = AuthorizationPolicies.CanAccessConversations)]
    public async Task<IActionResult> GetConversations()
    {
        var profile = HttpContext.GetUserProfile();
        if (profile == null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new GetAllConversationsRequest { UserId = profile.UserId });
        return Ok(result);
    }

    // Message history for a single conversation, used to hydrate the chat UI on
    // first load; the live stream while the chat is open comes from DamiHub over
    // SignalR instead. GetConversationMessagesQuery throws UnauthorizedAccessException
    // (mapped to 403 by DamiGlobalExceptionHandler) if the caller isn't a participant.
    [HttpGet("{conversationId:long}/messages")]
    [Authorize(Policy = AuthorizationPolicies.CanAccessConversations)]
    public async Task<IActionResult> GetMessages(long conversationId)
    {
        var profile = HttpContext.GetUserProfile();
        if (profile == null)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new GetConversationMessagesQuery
        {
            ConversationId = conversationId,
            RequestingUserId = profile.UserId
        });
        return Ok(result);
    }
}