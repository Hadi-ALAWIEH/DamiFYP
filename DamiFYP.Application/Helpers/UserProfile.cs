using System;
using DamiFYP.Domain.Models;

namespace DamiFYP.Application.Helpers;

public sealed class UserProfile
{
    public long UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public BusinessRole BusinessRole { get; init; } = BusinessRole.None;
    public string? BloodTypeName { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public bool IsAvailable { get; init; }
    public DateTime CreatedAt { get; init; }
    public VerificationStatus VerificationStatus { get; init; } = VerificationStatus.NotStarted;
    public string? ProfilePictureUrl { get; init; }
}
