using DamiFYP.Application.Helpers;
using Microsoft.AspNetCore.Http;

namespace DamiFYP.Middlewares;

public sealed class CurrentUserProfileMiddleware : IMiddleware
{
    private readonly ICurrentUserProfileService _profileService;

    public CurrentUserProfileMiddleware(ICurrentUserProfileService profileService)
    {
        _profileService = profileService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var profile = await _profileService.GetCurrentAsync(context.RequestAborted);
            if (profile != null)
            {
                context.Items[HttpContextUserProfileExtensions.ItemKey] = profile;
            }
        }

        await next(context);
    }
}


