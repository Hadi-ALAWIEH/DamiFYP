using System.Text.Json.Serialization;
using DamiFYP.Application.Helpers;
using DamiFYP.Domain.Models;
using DamiFYP.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DamiFYP.Application.Features.Authentication;

public sealed class CompleteUserOnboardingCommand : IRequest<CompleteUserOnboardingViewModel>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public BusinessRole BusinessRole { get; set; } = BusinessRole.None;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsAvailable { get; set; }

    [JsonIgnore]
    public long UserId { get; set; }

    [JsonIgnore]
    public string KeyCloakUserId { get; set; } = string.Empty;
}

public sealed class CompleteUserOnboardingCommandHandler : IRequestHandler<CompleteUserOnboardingCommand, CompleteUserOnboardingViewModel>
{
    private readonly DamiContext _context;
    private readonly ICurrentUserProfileService _profileService;

    public CompleteUserOnboardingCommandHandler(DamiContext context, ICurrentUserProfileService profileService)
    {
        _context = context;
        _profileService = profileService;
    }

    public async Task<CompleteUserOnboardingViewModel> Handle(CompleteUserOnboardingCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId <= 0)
        {
            throw new InvalidOperationException("Current user is required for onboarding.");
        }

        if (request.BusinessRole == BusinessRole.None)
        {
            throw new ArgumentException("BusinessRole must be selected.", nameof(request.BusinessRole));
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required.", nameof(request.Email));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Name is required.", nameof(request.Name));
        }

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken)
                   ?? throw new InvalidOperationException("User profile not found.");

        var normalizedEmail = request.Email.Trim();
        var emailTaken = await _context.Users
            .AsNoTracking()
            .AnyAsync(x => x.Email == normalizedEmail && x.Id != request.UserId, cancellationToken);

        if (emailTaken)
        {
            throw new InvalidOperationException("Email is already in use by another account.");
        }

        user.Name = request.Name.Trim();
        user.Email = normalizedEmail;
        user.Role = request.BusinessRole;
        user.Latitude = request.Latitude;
        user.Longitude = request.Longitude;
        user.IsAvailable = request.IsAvailable;

        await _context.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.KeyCloakUserId))
        {
            await _profileService.InvalidateAsync(request.KeyCloakUserId);
            var refreshedProfile = await _profileService.GetByUserIdAsync(request.KeyCloakUserId, normalizedEmail, cancellationToken);
            if (refreshedProfile != null)
            {
                return Map(refreshedProfile);
            }
        }

        return new CompleteUserOnboardingViewModel
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            BusinessRole = user.Role,
            Latitude = user.Latitude,
            Longitude = user.Longitude,
            IsAvailable = user.IsAvailable
        };
    }

    private static CompleteUserOnboardingViewModel Map(UserProfile profile)
    {
        return new CompleteUserOnboardingViewModel
        {
            UserId = profile.UserId,
            Name = profile.Name,
            Email = profile.Email,
            BusinessRole = profile.BusinessRole,
            Latitude = profile.Latitude,
            Longitude = profile.Longitude,
            IsAvailable = profile.IsAvailable
        };
    }
}

public sealed class CompleteUserOnboardingViewModel
{
    public long UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public BusinessRole BusinessRole { get; init; } = BusinessRole.None;
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public bool IsAvailable { get; init; }
}

