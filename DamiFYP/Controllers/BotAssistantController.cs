using DamiFYP.Application.Authorization;
using DamiFYP.Application.Features.BotAssistant;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DamiFYP.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BotAssistantController : ControllerBase
{
    private readonly IMediator _mediator;

    public BotAssistantController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // History for the assistant chat window on load.
    [HttpGet("messages")]
    [Authorize(Policy = AuthorizationPolicies.CanUseAssistant)]
    public async Task<IActionResult> GetMessages()
    {
        return Ok(await _mediator.Send(new GetBotMessagesQuery()));
    }

    // Sends a message to the assistant and returns its reply. Synchronous
    // request/response (not SignalR) since a reply always answers the exact
    // request that produced it — no independent live-push case like human
    // conversations have.
    [HttpPost("messages")]
    [Authorize(Policy = AuthorizationPolicies.CanUseAssistant)]
    [EnableRateLimiting(AssistantRateLimiterPolicy.Endpoint)]
    public async Task<IActionResult> SendMessage([FromBody] SendBotMessageCommand command)
    {
        return Ok(await _mediator.Send(command));
    }
}
