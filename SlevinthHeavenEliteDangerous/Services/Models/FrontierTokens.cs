using System;

namespace SlevinthHeavenEliteDangerous.Services.Models;

public class FrontierTokens
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public DateTime RefreshTokenIssuedAt { get; set; }
    public string? CommanderName { get; set; }

    public bool IsAccessTokenExpired => DateTime.UtcNow >= AccessTokenExpiresAt.AddMinutes(-5);
    public bool IsRefreshTokenExpired => DateTime.UtcNow >= RefreshTokenIssuedAt.AddDays(25);
}
