using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SmartPortfolio.API.Infrastructure;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Wystąpił błąd: {Message}", exception.Message);

        var (statusCode, title) = exception switch
        {
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Brak autoryzacji"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Zasób nie został znaleziony"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Błąd walidacji danych"),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Operacja niedozwolona"),
            _ => (StatusCodes.Status500InternalServerError, "Błąd serwera")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

}

