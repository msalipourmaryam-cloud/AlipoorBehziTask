using AlipoorBehTask.Application;
using AlipoorBehTask.Domain;
using Microsoft.AspNetCore.Mvc;

namespace AlipoorBehTask.Api;

public sealed class ApiExceptionMiddleware(
    RequestDelegate next,
    ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
                throw;

            var (status, title, detail) = exception switch
            {
                ResourceNotFoundException => (StatusCodes.Status404NotFound, "Resource not found", exception.Message),
                DuplicateNationalIdException => (StatusCodes.Status409Conflict, "National ID already registered", exception.Message),
                DomainRuleException => (StatusCodes.Status400BadRequest, "Request violates a business rule", exception.Message),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred", "The server could not complete the request.")
            };

            if (status == StatusCodes.Status500InternalServerError)
                logger.LogError(exception, "Unhandled API exception for {Path}", context.Request.Path);
            else
                logger.LogInformation(exception, "Request rejected for {Path}", context.Request.Path);

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";
            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            await context.Response.WriteAsJsonAsync(
                problem,
                cancellationToken: context.RequestAborted);
        }
    }
}