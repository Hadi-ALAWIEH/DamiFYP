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
    // [Authorize(Policy = AuthorizationPolicies.CanManageDonationRequests)]
    [Authorize(Roles = "Admin", Policy = AuthorizationPolicies.CanManageDonationRequests)]
    public async Task<IActionResult> GetDonationRequest(int id)
    {
        var result = await _mediator.Send(new GetDonationRequestQuery { Id = id });
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.CanManageDonationRequests)]
    public async Task<IActionResult> CreateDonationRequest([FromBody] CreateDonationRequestCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetDonationRequest), new { id = result.DonationRequest.Id }, result);
    }

    [HttpPost("{id}/confirm-match")]
    [Authorize(Policy = AuthorizationPolicies.CanManageDonationRequests)]
    public async Task<IActionResult> ConfirmMatch(int id, [FromBody] ConfirmDonationRequestMatchCommand command)
    {
        command.DonationRequestId = id;
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPut("{id}")]
    [Authorize(Policy = AuthorizationPolicies.CanManageDonationRequests)]
    public async Task<IActionResult> UpdateDonationRequest(int id, [FromBody] UpdateDonationRequestCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AuthorizationPolicies.CanManageDonationRequests)]
    public async Task<IActionResult> DeleteDonationRequest(int id)
    {
        await _mediator.Send(new DeleteDonationRequestCommand { Id = id });
        return NoContent();
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
}