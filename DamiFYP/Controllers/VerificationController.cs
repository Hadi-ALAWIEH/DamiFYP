using System.Security.Claims;
using DamiFYP.Application.Features.Verification;
using DamiFYP.Application.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DamiFYP.Controllers;

[Route("/api/verification")]
[ApiController]
[Authorize]
public class VerificationController : ControllerBase
{
    private readonly IMediator _mediator;

    public VerificationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("status")]
    public async Task<VerificationStatusViewModel> GetStatus(CancellationToken token) =>
        await _mediator.Send(new GetVerificationStatusQuery(), token);

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] SubmitVerificationCommand request, CancellationToken token)
    {
        // Same pattern as AuthenticationController.CompleteOnboarding - UserId
        // and KeyCloakUserId always come from the authenticated context, never
        // from the request body, so a client can't submit verification frames
        // on behalf of a different user id.
        var profile = HttpContext.GetUserProfile();
        var keycloakUserId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (profile == null || string.IsNullOrWhiteSpace(keycloakUserId))
        {
            return Unauthorized();
        }

        request.UserId = profile.UserId;
        request.KeyCloakUserId = keycloakUserId;

        var result = await _mediator.Send(request, token);
        return Ok(result);
    }
}
