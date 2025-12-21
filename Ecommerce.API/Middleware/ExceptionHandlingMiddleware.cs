using System.Net;
using System.Text.Json;
using Ecommerce.API.Exceptions;
using Ecommerce.API.Models;

namespace Ecommerce.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            await HandleException(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (BadHttpRequestException ex)
        {
            await HandleException(
                context,
                HttpStatusCode.BadRequest,
                ex.Message
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                StatusCode = 401,
                Message = ex.Message,
                Details = (string?)null
            });
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            await HandleException(
                context,
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred"
            );
        }
    }

    private static async Task HandleException(
        HttpContext context,
        HttpStatusCode statusCode,
        string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ErrorResponse
        {
            StatusCode = context.Response.StatusCode,
            Message = message
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response)
        );
    }
}
