using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TaskManagement.Core.Exceptions;

namespace TaskManagement.Api.Filters;

/// <summary>
/// Global filter: catches ApiException thrown anywhere (services, controllers)
/// and converts it to the { title, message } JSON error the frontend understands.
/// Registered once in Program.cs — no try/catch needed in controllers.
/// </summary>
public class ApiExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ApiException api)
        {
            _logger.LogWarning(api, "ApiException {StatusCode}: {Title} - {Message}",
                api.StatusCode, api.Title, api.Message);
            context.Result = new ObjectResult(new
            {
                title = api.Title,
                message = api.Message
            })
            {
                StatusCode = api.StatusCode
            };
            context.ExceptionHandled = true;
        }
        else
        {
            _logger.LogError(context.Exception, "Unhandled exception on {Path}",
                context.HttpContext.Request.Path);
        }
    }
}
