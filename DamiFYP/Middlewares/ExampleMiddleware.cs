using System.Security.Claims;

namespace DamiFYP.Middlewares;

public class ExampleMiddleware : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ClaimsPrincipal user = context.User;
        throw new NotImplementedException();
    }
}