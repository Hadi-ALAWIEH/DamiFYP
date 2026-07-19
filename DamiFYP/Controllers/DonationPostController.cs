using DamiFYP.Application.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DamiFYP.Controllers;

[ApiController]
[Route("api/[controller]/")]
public class DonationPostController : ControllerBase
{
    private readonly IMediator _mediator;

    public DonationPostController(IMediator mediator)
    {
        _mediator = mediator;
    }


    [HttpPost("create-donation-post")]
    [Authorize(policy: AuthorizationPolicies.CanManageDonationPosts)]
    public async Task<IActionResult> CreateDonationPost([FromBody] CreateDonationPostCommand command)
    {
        await _mediator.Send(command);
        return Ok();
    }

    [HttpGet("get-candidates-{donationRequestId}")]
    public async Task<IActionResult> GetCandidateForMatch([FromRoute] long donationRequestId)
    {
        var candidates = await _mediator.Send(new GetCandidateDonationPostsQuery { DonationRequestId = donationRequestId });
        return Ok(candidates);
    }

}