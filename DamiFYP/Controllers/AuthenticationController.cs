using DamiFYP.Application.Features.Authentication;
using DamiFYP.Application.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DamiFYP.Controllers;

[Route("/api/[action]")]
[ApiController]
public class AuthenticationController : ControllerBase {

    private readonly IMediator _mediator;

    public AuthenticationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<LoginUserRequestViewModel> Login([FromBody] LoginUserRequest request, CancellationToken token)
    {
        return await _mediator.Send(request, token);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CompleteOnboarding(
        [FromBody] CompleteUserOnboardingCommand request,
        CancellationToken token)
    {
        var profile = HttpContext.GetUserProfile(); // this will get the template profile written in Items dictionary
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