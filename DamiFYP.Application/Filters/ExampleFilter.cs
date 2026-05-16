using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace DamiFYP.Application.Filters;

public class ExampleFilter : IAsyncActionFilter
{
    private readonly ILogger<ExampleFilter> _logger;

    public ExampleFilter(ILogger<ExampleFilter> logger)
    {
        _logger = logger;
    }

    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {

        var some = context.ActionDescriptor.Parameters;
        foreach (var param in some) _logger.LogInformation($"the name of this parameter is {param.Name} and it is a {param.ParameterType}");

        var someRequest = context.HttpContext.Request;
        var someController = context.Controller;
        _logger.LogInformation($"The controller instance of this request is {someController}");
        _logger.LogInformation($"The method of this request is {someRequest.Method}");
        return next.Invoke();
    }

}