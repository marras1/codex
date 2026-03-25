using FamilyLedger.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FamilyLedger.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception on {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext ctx, Exception ex)
    {
        var (status, title) = ex switch
        {
            UnauthorizedAccessException => (401, "Unauthorized"),
            AccountNotFoundException => (404, "Not found"),
            UnauthorisedProfileAccessException => (403, "Forbidden"),
            MonthlyRecordLockedException => (409, "Record locked"),
            DomainException => (400, "Bad request"),
            InvalidOperationException => (400, "Bad request"),
            _ => (500, "Internal server error")
        };

        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/problem+json";
        await ctx.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = ex.Message,
            Extensions = { ["traceId"] = ctx.TraceIdentifier }
        });
    }
}
