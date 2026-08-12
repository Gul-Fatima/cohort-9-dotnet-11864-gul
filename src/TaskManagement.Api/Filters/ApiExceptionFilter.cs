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
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ApiException api)
        {
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
    }
}
