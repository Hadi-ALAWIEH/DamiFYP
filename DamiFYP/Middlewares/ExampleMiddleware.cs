namespace DamiFYP.Middlewares;

public class ExampleMiddleware : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        throw new NotImplementedException();
    }
}