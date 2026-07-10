using Microsoft.AspNetCore.Authorization;
using DamiFYP.Domain.Models;

namespace DamiFYP.Application.Authorization;

public sealed class BusinessRoleRequirement : IAuthorizationRequirement
{
    public BusinessRoleRequirement(params BusinessRole[] allowedRoles)
    {
        AllowedRoles = allowedRoles ?? Array.Empty<BusinessRole>();
    }

    public BusinessRole[] AllowedRoles { get; }
}


