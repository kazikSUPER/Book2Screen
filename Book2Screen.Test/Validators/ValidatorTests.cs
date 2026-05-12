// Book2Screen.Tests/Validators/ValidatorTests.cs
// xUnit — FluentValidation unit tests.
// Pure functions — no mocking, no DB, no HTTP.

namespace Book2Screen.Tests.Validators;

using Book2Screen.Application.Validators;
using Domain.Entities;
using Xunit;

// ═══════════════════════════════════════════════════════════════════════════════
// UserValidator
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Unit tests for <see cref="UserValidator"/>.
/// Rules: Username (NotEmpty, MaxLength 50),
///        Email (NotEmpty, EmailAddress),
///        Role (user | admin | moderator — case-sensitive).
/// </summary>
public class UserValidatorTests
{
    private readonly UserValidator _validator = new();

    private static User ValidUser() => new()
    {
        Username = "testuser",
        Email = "test@example.com",
        PasswordHash = "hashed",
        Role = "user",
    };

    // ── TC-UNIT-06 ─────────────────────────────────────────────────────────
    [Fact]
    public void UserValidator_ValidUser_PassesAllRules()
    {
        var result = _validator.Validate(ValidUser());

        Assert.True(result.IsValid,
            "Errors: " + string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
    }

    // ── TC-UNIT-07 ─────────────────────────────────────────────────────────
    [Fact]
    public void UserValidator_EmptyUsername_FailsValidation()
    {
        var user = ValidUser();
        user.Username = string.Empty;

        var result = _validator.Validate(user);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Username");
    }

    // ── TC-UNIT-08 ─────────────────────────────────────────────────────────
    [Fact]
    public void UserValidator_UsernameTooLong_FailsMaxLength()
    {
        var user = ValidUser();
        user.Username = new string('a', 51);

        var result = _validator.Validate(user);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.PropertyName == "Username" && e.ErrorMessage.Contains("50"));
    }

    // ── TC-UNIT-09 ─────────────────────────────────────────────────────────
    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain.com")]
    public void UserValidator_InvalidEmail_FailsEmailRule(string badEmail)
    {
        var user = ValidUser();
        user.Email = badEmail;

        var result = _validator.Validate(user);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    // ── TC-UNIT-10 ─────────────────────────────────────────────────────────
    [Theory]
    [InlineData("user")]
    [InlineData("admin")]
    [InlineData("moderator")]
    public void UserValidator_ValidRoles_PassRoleRule(string role)
    {
        var user = ValidUser();
        user.Role = role;

        var result = _validator.Validate(user);

        Assert.True(result.IsValid,
            $"Role '{role}' should pass — errors: " +
            string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
    }

    // ── TC-UNIT-11 ─────────────────────────────────────────────────────────
    /// <summary>
    /// UserValidator.Role rule has NO ToLower() — "USER" is rejected.
    /// </summary>
    [Theory]
    [InlineData("superadmin")]
    [InlineData("guest")]
    [InlineData("USER")]        // case-sensitive: rule array is lowercase only
    [InlineData("Admin")]
    public void UserValidator_InvalidRole_FailsRoleRule(string invalidRole)
    {
        var user = ValidUser();
        user.Role = invalidRole;

        var result = _validator.Validate(user);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Role");
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// AdaptationValidator
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Unit tests for <see cref="AdaptationValidator"/>.
/// Rules: Title (NotEmpty, MaxLength 255),
///        Type Must(t => ["movie","series"].Contains(t.ToLower())),
///        ReleaseYear GreaterThan(0) — only when HasValue.
/// </summary>
public class AdaptationValidatorTests
{
    private readonly AdaptationValidator _validator = new();

    private static Adaptation ValidAdaptation() => new()
    {
        Title = "Dune: Part Two",
        Type = "movie",
        ReleaseYear = 2024,
    };

    // ── TC-UNIT-12 ─────────────────────────────────────────────────────────
    [Fact]
    public void AdaptationValidator_ValidAdaptation_PassesAllRules()
    {
        var result = _validator.Validate(ValidAdaptation());

        Assert.True(result.IsValid,
            "Errors: " + string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
    }

    // ── TC-UNIT-13 ─────────────────────────────────────────────────────────
    [Fact]
    public void AdaptationValidator_EmptyTitle_FailsNotEmptyRule()
    {
        var a = ValidAdaptation();
        a.Title = string.Empty;

        var result = _validator.Validate(a);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
    }

    // ── TC-UNIT-14 ─────────────────────────────────────────────────────────
    [Fact]
    public void AdaptationValidator_TitleTooLong_FailsMaxLength()
    {
        var a = ValidAdaptation();
        a.Title = new string('x', 256); // 256 > 255

        var result = _validator.Validate(a);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
    }

    // ── TC-UNIT-15 ─────────────────────────────────────────────────────────
    /// <summary>
    /// AdaptationValidator.Type rule calls .ToLower() internally,
    /// so "MOVIE" and "Series" are both accepted.
    /// </summary>
    [Theory]
    [InlineData("movie")]
    [InlineData("series")]
    [InlineData("MOVIE")]
    [InlineData("Series")]
    public void AdaptationValidator_ValidTypes_PassTypeRule(string type)
    {
        var a = ValidAdaptation();
        a.Type = type;

        var result = _validator.Validate(a);

        Assert.True(result.IsValid,
            $"Type '{type}' should pass — errors: " +
            string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
    }

    // ── TC-UNIT-16 ─────────────────────────────────────────────────────────
    [Theory]
    [InlineData("anime")]
    [InlineData("documentary")]
    public void AdaptationValidator_InvalidType_FailsTypeRule(string badType)
    {
        var a = ValidAdaptation();
        a.Type = badType;

        var result = _validator.Validate(a);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Type");
    }

    // ── TC-UNIT-17 ─────────────────────────────────────────────────────────
    [Fact]
    public void AdaptationValidator_ReleaseYearZero_FailsGreaterThanRule()
    {
        var a = ValidAdaptation();
        a.ReleaseYear = 0;

        var result = _validator.Validate(a);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ReleaseYear");
    }

    // ── TC-UNIT-18 ─────────────────────────────────────────────────────────
    /// <summary>Null ReleaseYear skips the GreaterThan rule (When HasValue guard).</summary>
    [Fact]
    public void AdaptationValidator_NullReleaseYear_PassesOptionalRule()
    {
        var a = ValidAdaptation();
        a.ReleaseYear = null;

        var result = _validator.Validate(a);

        Assert.True(result.IsValid,
            "Null ReleaseYear should be allowed — errors: " +
            string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
    }
}
