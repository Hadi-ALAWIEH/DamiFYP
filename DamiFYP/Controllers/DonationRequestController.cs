using DamiFYP.Application.Authorization;
using DamiFYP.Application.Features.DonationRequests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DamiFYP.Controllers;

[ApiController]
[Route("api/[controller]/")]
public class DonationRequestController : ControllerBase
{
    private readonly IMediator _mediator;

    public DonationRequestController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin", Policy = AuthorizationPolicies.CanManageDonationRequests)]
    public async Task<IActionResult> GetDonationRequest(long id)
    {
        var result = await _mediator.Send(new GetDonationRequestQuery { Id = (int)id });
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.CanManageDonationRequests)]
    public async Task<IActionResult> CreateDonationRequest([FromBody] CreateDonationRequestCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetDonationRequest), new { id = result.DonationRequest.Id }, result);
    }

    [HttpPost("confirm-match")]
    [Authorize(Policy = AuthorizationPolicies.CanManageDonationRequests)]
    public async Task<IActionResult> ConfirmMatch([FromBody] ConfirmDonationRequestMatchCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AuthorizationPolicies.CanManageDonationRequests)]
    public async Task<IActionResult> UpdateDonationRequest(long id, [FromBody] UpdateDonationRequestCommand command)
    {
        command.Id = id;
        try
        {
            var result = await _mediator.Send(command);
            if (result == null) return NotFound();
            return Ok(result);
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

    [HttpDelete("{id}")]
    [Authorize(Policy = AuthorizationPolicies.CanManageDonationRequests)]
    public async Task<IActionResult> DeleteDonationRequest(long id)
    {
        try
        {
            await _mediator.Send(new DeleteDonationRequestCommand { Id = id });
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

    [HttpPost("{id}/cancel")]
    [Authorize(Policy = AuthorizationPolicies.CanManageDonationRequests)]
    public async Task<IActionResult> CancelRequest(long id)
    {
        try
        {
            await _mediator.Send(new CancelDonationRequestCommand { Id = id });
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

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.CanViewAvailableDonationRequests)]
    public async Task<IActionResult> GetAllDonationRequests()
    {
        var result = await _mediator.Send(new GetAllDonationRequestsQuery());
        return Ok(result);
    }

    [HttpGet("current-user-donation-requests")]
    [Authorize(Policy = AuthorizationPolicies.CanManageDonationRequests)]
    public async Task<IActionResult> GetCurrentUserDonationRequests()
    {
        var result = await _mediator.Send(new GetCurrentUserDonationRequestsQuery());
        return Ok(result);
    }

    [HttpPost("{id}/feedback")]
    [Authorize(Policy = AuthorizationPolicies.CanManageDonationRequests)]
    public async Task<IActionResult> SubmitFeedback(long id, [FromBody] SubmitDonorFeedbackCommand command)
    {
        command.DonationRequestId = id;
        try
        {
            await _mediator.Send(command);
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

    [HttpPost("{id}/confirm-donation")]
    [Authorize(Policy = AuthorizationPolicies.CanManageDonationRequests)]
    public async Task<IActionResult> ConfirmDonation(long id)
    {
        try
        {
            await _mediator.Send(new ConfirmDonationCommand { DonationRequestId = id });
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