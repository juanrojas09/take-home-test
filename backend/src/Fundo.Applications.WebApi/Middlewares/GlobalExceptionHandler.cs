using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Fundo.Applications.WebApi.Middlewares;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger):IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError("Exception caught in global exception handler: {Message}", exception.Message);
        var traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;
        logger.LogError("TraceId: {TraceId}", traceId);
        
        httpContext.Response.StatusCode = 500; 
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.Headers["TraceId"] = traceId;
        
        
        return await httpContext.Response.WriteAsJsonAsync(exception.Message, cancellationToken).ContinueWith(_=>true,cancellationToken);
    }
}