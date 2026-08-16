using DamiFYP.Application.Authorization;
using DamiFYP.Application.Features.DonationPosts;
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

    [HttpGet("get-current-user-donation-posts")]
    [Authorize(Policy = AuthorizationPolicies.CanManageDonationPosts)]
    public async Task<IActionResult> GetCurrentUserDonationPosts()
    {
        var result = await _mediator.Send(new GetCurrentUserDonationPostsQuery());
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AuthorizationPolicies.CanManageDonationPosts)]
    public async Task<IActionResult> DeleteDonationPost(long id)
    {
        try
        {
            await _mediator.Send(new DeleteDonationPostCommand { Id = id });
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}