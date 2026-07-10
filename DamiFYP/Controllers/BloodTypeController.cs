using DamiFYP.Application.Authorization;
using DamiFYP.Application.Features.BloodType;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DamiFYP.Controllers;

[ApiController]
[Route("api/[controller]/")]
public class BloodTypeController : ControllerBase
{
    private readonly IMediator _mediator;

    public BloodTypeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("GetAllBloodTypes")]
    [Authorize(Policy = AuthorizationPolicies.CanManageBloodTypes)]
    public async Task<IActionResult> GetBloodTypes()
    {
        return Ok(await _mediator.Send(new GetAllBloodTypesQuery()));
    }

}