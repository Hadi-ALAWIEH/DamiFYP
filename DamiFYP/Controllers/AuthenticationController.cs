using DamiFYP.Application.Features.Authentication;
using DamiFYP.Application.Helpers;
using DamiFYP.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DamiFYP.Controllers;

public class UpdateUserProfileFormData
{
    public string Name { get; set; } = string.Empty;
    public BusinessRole BusinessRole { get; set; }
    public bool IsAvailable { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public IFormFile? ProfilePicture { get; set; }
}

[Route("/api/[action]")]
[ApiController]
public class AuthenticationController : ControllerBase {

    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _env;

    public AuthenticationController(IMediator mediator, IWebHostEnvironment env)
    {
        _mediator = mediator;
        _env = env;
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
        var profile = HttpContext.GetUserProfile();
        var keycloakUserId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userEmailFromToken = User.FindFirstValue("email") ?? User.FindFirstValue(ClaimTypes.Email);

        if (profile == null || string.IsNullOrWhiteSpace(keycloakUserId))
        {
            return Unauthorized();
        }

        request.UserId = profile.UserId;
        request.KeyCloakUserId = keycloakUserId;
        request.Email = userEmailFromToken;

        var result = await _mediator.Send(request, token);
        return Ok(result);
    }

    [Authorize]
    [HttpPut]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserProfileFormData form, CancellationToken token)
    {
        var profile = HttpContext.GetUserProfile();
        var keycloakUserId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (profile == null || string.IsNullOrWhiteSpace(keycloakUserId))
            return Unauthorized();

        string? profilePictureUrl = null;
        if (form.ProfilePicture is { Length: > 0 })
        {
            var ext = Path.GetExtension(form.ProfilePicture.FileName).ToLowerInvariant();
            var fileName = $"{profile.UserId}_{DateTime.UtcNow.Ticks}{ext}";
            var webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
            var dir = Path.Combine(webRoot, "profile-pictures");
            Directory.CreateDirectory(dir);
            await using var stream = System.IO.File.Create(Path.Combine(dir, fileName));
            await form.ProfilePicture.CopyToAsync(stream, token);
            profilePictureUrl = $"/profile-pictures/{fileName}";
        }

        var command = new UpdateUserProfileCommand
        {
            UserId = profile.UserId,
            KeyCloakUserId = keycloakUserId,
            Name = string.IsNullOrWhiteSpace(form.Name) ? profile.Name : form.Name,
            BusinessRole = form.BusinessRole,
            IsAvailable = form.IsAvailable,
            Latitude = form.Latitude ?? profile.Latitude,
            Longitude = form.Longitude ?? profile.Longitude,
            ProfilePictureUrl = profilePictureUrl,
        };

        var updated = await _mediator.Send(command, token);
        return Ok(updated);
    }

    [Authorize]
    [HttpGet]
    public async Task<CheckProfileExistenceViewModel> CheckProfileExistence(CancellationToken token) => await _mediator.Send(
        new CheckProfileExistenceQuery() {} , token);

    [Authorize]
    [HttpGet]
    public async Task<UserProfile> GetUserProfile(CancellationToken token) => await _mediator.Send(
        new GetUserProfileQuery() {} , token);

}
