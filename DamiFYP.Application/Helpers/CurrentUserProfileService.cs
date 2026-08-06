using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DamiFYP.Domain.Models;
using DamiFYP.Persistence.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DamiFYP.Application.Helpers;

public sealed class CurrentUserProfileService : ICurrentUserProfileService
{
    private const string CacheKeyPrefix = "user-profile:";
    private readonly DamiContext _context;
    private readonly IMemoryCache _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentUserProfileService> _logger;

    public CurrentUserProfileService(DamiContext context, IMemoryCache cache, IHttpContextAccessor httpContextAccessor,
        ILogger<CurrentUserProfileService> logger)
    {
        _context = context;
        _cache = cache;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public Task<UserProfile?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null || httpContext.User.Identity?.IsAuthenticated != true)
        {
            return Task.FromResult<UserProfile?>(null);
        }

        _logger.LogInformation(
            $"The Type of authentication used for this request is {httpContext.User.Identity.AuthenticationType}");

        foreach (var claim in httpContext.User.Claims)
        {
            _logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
        }

        var existingProfile = httpContext.GetUserProfile();
        if (existingProfile != null)
        {
            return Task.FromResult<UserProfile?>(existingProfile);
        }

        var userIdClaim = httpContext.User.FindFirstValue("sub") ??
                          httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userEmail = httpContext.User.FindFirstValue("email") ?? "";
        if (userIdClaim == null)
        {
            return Task.FromResult<UserProfile?>(null);
        }

        return GetByUserIdAsyncInternal(userIdClaim, userEmail, cancellationToken);
    }


    public Task<UserProfile?> GetByUserIdAsync(string keycloakUserId, string email, CancellationToken cancellationToken = default)
    {
        return GetByUserIdAsyncInternal(keycloakUserId, email, cancellationToken);
    }

    private async Task<UserProfile?> GetByUserIdAsyncInternal(string userId, string userEmail,
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKeyPrefix + userId, out UserProfile? cached))
        {
            return cached;
        }

        var user = await _context.DamiUsers
            .AsNoTracking()
            .Include(x => x.BloodType)
            .FirstOrDefaultAsync(x => x.KeyCloakId == userId, cancellationToken);

        if (user == null)
        {
            // Create a minimal linked user so authenticated requests have a profile to work with.
            var normalizedEmail = string.IsNullOrWhiteSpace(userEmail)
                ? $"{userId}@bootstrap.local"
                : userEmail.Trim();

            user = new DamiUser()
            {
                KeyCloakId = userId,
                Email = normalizedEmail,
                Name = "Pending Profile",
                PasswordHash = "EXTERNAL_AUTH",
                Role = BusinessRole.None,
                IsAvailable = false
            };
            await _context.DamiUsers.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            // todo: here oblige the user to send the request that has to do with bootstrapping the user and invalidating the cache
        }

        var profile = Map(user);
        _cache.Set(CacheKeyPrefix + userId, profile, TimeSpan.FromMinutes(2));
        return profile;
    }

    public Task InvalidateAsync(string keycloakUserId)
    {
        _cache.Remove(CacheKeyPrefix + keycloakUserId);
        return Task.CompletedTask;
    }

    private static UserProfile Map(DamiUser damiUser)
    {
        return new UserProfile
        {
            UserId = damiUser.Id,
            Email = damiUser.Email,
            Name = damiUser.Name,
            BusinessRole = damiUser.Role,
            BloodTypeName = damiUser.BloodType?.ToString(),
            Latitude = damiUser.Latitude,
            Longitude = damiUser.Longitude,
            IsAvailable = damiUser.IsAvailable,
            CreatedAt = damiUser.CreatedAt,
            VerificationStatus = damiUser.VerificationStatus
        };
    }
}
