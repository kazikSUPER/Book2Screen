namespace Book2Screen.Application.Interfaces;

using Book2Screen.Domain.Entities;

/// <summary>
/// Сервіс для роботи з автентифікаційними токенами.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Створює JSON Web Token (JWT) для ідентифікації користувача в системі.
    /// </summary>
    /// <param name="user">Об'єкт користувача <see cref="User"/>, для якого генерується токен.</param>
    /// <returns>Рядок, що містить зашифрований JWT токен.</returns>
    /// <response code="200">Токен успішно згенеровано.</response>
    /// <response code="401">Користувач не пройшов валідацію для отримання токена.</response>
    string CreateToken(User user);
}
