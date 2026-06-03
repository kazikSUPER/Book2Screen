// <copyright file="TestDbHelper.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Tests.Helpers;

using Book2Screen.Domain.Entities;
using Book2Screen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Хелпер для створення тестових даних в InMemory БД.
/// </summary>
public static class TestDbHelper
{
    /// <summary>
    /// Створює InMemory контекст з унікальним іменем.
    /// </summary>
    public static ApplicationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Створює повний набір: User + Book + Adaptation + Work.
    /// Work entity вимагає BookId і AdaptationId.
    /// </summary>
    public static async Task<(Guid userId, Guid workId)> SeedUserAndWork(
        ApplicationDbContext context,
        string userEmail = "john@example.com",
        string username = "john_doe")
    {
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var adaptationId = Guid.NewGuid();
        var workId = Guid.NewGuid();

        context.Users.Add(new User
        {
            Id = userId,
            Username = username,
            Email = userEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPass123!"),
            Role = "user",
        });

        context.Books.Add(new Book
        {
            Id = bookId,
            Title = "Test Book",
        });

        context.Adaptations.Add(new Adaptation
        {
            Id = adaptationId,
            Title = "Test Adaptation",
            Type = "movie",
        });

        context.Works.Add(new Work
        {
            Id = workId,
            Title = "Test Work",
            BookId = bookId,
            AdaptationId = adaptationId,
        });

        await context.SaveChangesAsync();
        return (userId, workId);
    }

    /// <summary>
    /// Створює тестового користувача.
    /// </summary>
    public static async Task<Guid> SeedUser(
        ApplicationDbContext context,
        string email,
        string username,
        string password = "TestPass123!",
        string role = "user")
    {
        var userId = Guid.NewGuid();
        context.Users.Add(new User
        {
            Id = userId,
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
        });
        await context.SaveChangesAsync();
        return userId;
    }
}
