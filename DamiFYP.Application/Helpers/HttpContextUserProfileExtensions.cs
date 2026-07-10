using Microsoft.AspNetCore.Http;

namespace DamiFYP.Application.Helpers;

public static class HttpContextUserProfileExtensions
{
    public const string ItemKey = "CurrentUserProfile";

    public static UserProfile? GetUserProfile(this HttpContext context)
    {
        return context.Items.TryGetValue(ItemKey, out var value) && value is UserProfile profile
            ? profile
            : null;
    }
}

