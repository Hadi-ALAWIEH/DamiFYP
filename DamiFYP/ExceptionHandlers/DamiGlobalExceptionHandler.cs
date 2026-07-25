using System.Security.Authentication;
using DamiFYP.Infrastructure.BloodAvailability;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DamiFYP.ExceptionHandlers.cs;

public class DamiGlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<DamiGlobalExceptionHandler> _logger;

    public DamiGlobalExceptionHandler(ILogger<DamiGlobalExceptionHandler> logger)
    {
        this._logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError($"The error {exception.Message} was thrown at {DateTime.Now}");
        var responseStatusCode = exception switch
        {
            InvalidCredentialException => StatusCodes.Status404NotFound,
            BloodAvailabilityServiceException => StatusCodes.Status502BadGateway,
            _ => StatusCodes.Status400BadRequest
        };

        var details = new ProblemDetails()
        {
            Title = "There was an error processing the response",
            Type = $"/errors/{exception.GetType().Name}/",
            Detail = exception.Message,
            Status = responseStatusCode,
            Instance = httpContext.Request.Path.Value
        };

        httpContext.Response.ContentType = "application/problem+json";
        httpContext.Response.StatusCode = responseStatusCode;
        await httpContext.Response.WriteAsJsonAsync(
            details,
            cancellationToken
        );
        return true;
    }
}