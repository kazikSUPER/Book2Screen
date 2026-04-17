namespace Book2Screen.Middleware;

using System.Net;
using Book2Screen.Models;
using Microsoft.AspNetCore.Diagnostics;

/// <summary>
/// Глобальний обробник виключень, що забезпечує єдиний формат відповіді API при помилках.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    /// <summary>
    /// Ініціалізує новий екземпляр <see cref="GlobalExceptionHandler"/>.
    /// </summary>
    /// <param name="logger">Екземпляр логера для запису помилок (Serilog).</param>
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Перехоплює необроблені виключення та формує стандартизовану JSON-відповідь.
    /// </summary>
    /// <param name="httpContext">Контекст поточного HTTP-запиту.</param>
    /// <param name="exception">Виключення, що було викинуто.</param>
    /// <param name="cancellationToken">Токен скасування операції.</param>
    /// <returns>True, якщо помилка успішно оброблена.</returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. Логування помилки з рівнем Error та StackTrace
        _logger.LogError(
            exception,
            "An unhandled exception occurred: {Message}. Path: {Path}",
            exception.Message,
            httpContext.Request.Path);

        // 2. Визначення статус-коду залежно від типу помилки
        var statusCode = exception switch
        {
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            _ => (int)HttpStatusCode.InternalServerError
        };

        // 3. Формування моделі відповіді
        var response = new ErrorResponse
        {
            StatusCode = statusCode,
            Message = statusCode == (int)HttpStatusCode.InternalServerError
                ? "An unexpected error occurred on the server."
                : exception.Message,
            Timestamp = DateTime.UtcNow
        };

        // 4. Налаштування HTTP-відповіді
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        // 5. Відправлення JSON-відповіді клієнту
        await httpContext.Response.WriteAsJsonAsync(
            response,
            cancellationToken);

        return true;
    }
}
