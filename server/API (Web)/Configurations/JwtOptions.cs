// <copyright file="JwtOptions.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.API__Web_.Configurations;

/// <summary>
/// Configuration options for JWT authentication.
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// Gets or sets the secret key used for signing tokens.
    /// </summary>
    public string Secret { get; set; } = null!;

    /// <summary>
    /// Gets or sets the token issuer.
    /// </summary>
    public string? Issuer { get; set; }

    /// <summary>
    /// Gets or sets the token audience.
    /// </summary>
    public string? Audience { get; set; }

    /// <summary>
    /// Gets or sets the token expiry time in minutes.
    /// </summary>
    public int ExpiryMinutes { get; set; } = 60;
}
