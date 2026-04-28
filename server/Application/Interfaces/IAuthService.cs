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
}
