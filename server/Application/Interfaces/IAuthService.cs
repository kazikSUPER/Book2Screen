// <copyright file="IAuthService.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Application.Interfaces;

using Book2Screen.Application.DTOs;

/// <summary>
/// Інтерфейс сервісу авторизації та реєстрації.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Авторизує користувача за електронною поштою та паролем.
    /// </summary>
    /// <param name="loginDto">Дані для входу.</param>
    /// <returns>Об'єкт з токеном та даними профілю або null, якщо дані невірні.</returns>
    Task<AuthResponse?> LoginAsync(LoginDto loginDto);

    /// <summary>
    /// Реєструє нового користувача в системі.
    /// </summary>
    /// <param name="registerRequest">Дані для реєстрації.</param>
    /// <returns>Об'єкт з токеном для нового користувача або null, якщо пошта чи нікнейм зайняті.</returns>
    Task<AuthResponse?> RegisterAsync(RegisterRequest registerRequest);

    /// <summary>
    /// Генерує код для відновлення паролю та надсилає його на пошту.
    /// </summary>
    /// <param name="request">Запит з Email.</param>
    /// <returns>True, якщо успішно.</returns>
    Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request);

    /// <summary>
    /// Перевіряє правильність коду відновлення.
    /// </summary>
    /// <param name="request">Запит з Email та кодом.</param>
    /// <returns>True, якщо код вірний та не протермінований.</returns>
    Task<bool> VerifyResetCodeAsync(VerifyCodeRequest request);

    /// <summary>
    /// Скидає пароль на новий.
    /// </summary>
    /// <param name="request">Запит з Email, кодом та новим паролем.</param>
    /// <returns>True, якщо успішно.</returns>
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
}
