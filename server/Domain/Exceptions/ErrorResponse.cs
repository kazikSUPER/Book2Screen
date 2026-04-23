namespace Book2Screen.Domain.Exceptions;

/// <summary>
/// Стандартна модель відповіді при виникненні помилки.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// HTTP статус-код (напр. 400, 404, 500).
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Машинно-зчитуваний код помилки (напр. "INTERNAL_ERROR", "NOT_FOUND").
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// Короткий опис помилки для користувача.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Час виникнення помилки (UTC).
    /// </summary>
    public DateTime Timestamp { get; set; }
}
