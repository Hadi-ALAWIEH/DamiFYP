using DamiFYP.Application.Authorization;
using DamiFYP.Application.Features.BloodAvailability;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DamiFYP.Controllers;

[ApiController]
[Route("api/[controller]/")]
public class BloodAvailabilityController : ControllerBase
{
    private readonly IMediator _mediator;

    public BloodAvailabilityController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("Metadata")]
    [Authorize(Policy = AuthorizationPolicies.CanViewBloodAvailabilityPredictions)]
    public async Task<IActionResult> GetMetadata()
    {
        return Ok(await _mediator.Send(new GetBloodAvailabilityMetadataQuery()));
    }

    [HttpPost("Predict")]
    [Authorize(Policy = AuthorizationPolicies.CanViewBloodAvailabilityPredictions)]
    public async Task<IActionResult> Predict([FromBody] PredictBloodAvailabilityQuery query)
    {
        return Ok(await _mediator.Send(query));
    }

    [HttpPost("PredictBatch")]
    [Authorize(Policy = AuthorizationPolicies.CanViewBloodAvailabilityPredictions)]
    public async Task<IActionResult> PredictBatch([FromBody] PredictBloodAvailabilityBatchQuery query)
    {
        return Ok(await _mediator.Send(query));
    }
}
