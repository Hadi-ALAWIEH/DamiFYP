using DamiFYP.Application.Features.Authentication;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
}