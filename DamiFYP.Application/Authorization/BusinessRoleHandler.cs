using System.Linq;
using System.Threading.Tasks;
using DamiFYP.Application.Helpers;
using Microsoft.AspNetCore.Authorization;
using DamiFYP.Domain.Models;

namespace DamiFYP.Application.Authorization;

public sealed class BusinessRoleHandler : AuthorizationHandler<BusinessRoleRequirement>
{
    private readonly ICurrentUserProfileService _profileService;

    public BusinessRoleHandler(ICurrentUserProfileService profileService)
    {
        _profileService = profileService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, BusinessRoleRequirement requirement)
    {
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        var profile = await _profileService.GetCurrentAsync();
        if (profile == null)
        {
            return;
        }

        if (requirement.AllowedRoles.Contains(profile.BusinessRole))
        {
            context.Succeed(requirement);
        }
    }
}

