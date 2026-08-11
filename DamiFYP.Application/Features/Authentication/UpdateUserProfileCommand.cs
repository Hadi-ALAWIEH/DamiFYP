using System;
using System.Threading;
using System.Threading.Tasks;
using DamiFYP.Application.Helpers;
using DamiFYP.Domain.Models;
using DamiFYP.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DamiFYP.Application.Features.Authentication;

public sealed class UpdateUserProfileCommand : IRequest<UserProfile>
{
    public long UserId { get; set; }
    public string? KeyCloakUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public BusinessRole BusinessRole { get; set; }
    public bool IsAvailable { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

public sealed class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, UserProfile>
{
    private readonly DamiContext _context;
    private readonly ICurrentUserProfileService _profileService;

    public UpdateUserProfileCommandHandler(DamiContext context, ICurrentUserProfileService profileService)
    {
        _context = context;
        _profileService = profileService;
    }

    public async Task<UserProfile> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.DamiUsers
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new InvalidOperationException("User not found.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required.", nameof(request.Name));

        user.Name = request.Name.Trim();
        user.Role = request.BusinessRole;
        user.IsAvailable = request.IsAvailable;
        user.Latitude = request.Latitude;
        user.Longitude = request.Longitude;

        if (request.ProfilePictureUrl != null)
            user.ProfilePictureUrl = request.ProfilePictureUrl;

        await _context.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.KeyCloakUserId))
        {
            await _profileService.InvalidateAsync(request.KeyCloakUserId);
            var refreshed = await _profileService.GetByUserIdAsync(request.KeyCloakUserId, user.Email, cancellationToken);
            if (refreshed != null)
                return refreshed;
        }

        return (await _profileService.GetCurrentAsync(cancellationToken))!;
    }
}
