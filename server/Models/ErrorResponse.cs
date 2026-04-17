namespace Book2Screen.Models;

/// <summary>
/// Стандартна модель відповіді при виникненні помилки
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// HTTP статус-код (напр. 400, 404, 500)
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Короткий опис помилки
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Час виникнення помилки (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; }
}
