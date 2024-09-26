﻿using System.Text;
using System.Security.Claims;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.IdentityModel.Tokens;


namespace MaksIT.Core.Security;

public class JWTTokenClaims {
  public required string? Username { get; set; }
  public required List<string>? Roles { get; set; }
  public DateTime? IssuedAt { get; set; }
  public DateTime? ExpiresAt { get; set; }
}


public static class JwtGenerator {
  public static (string, JWTTokenClaims) GenerateToken(string secret, string issuer, string audience, double expiration, string username, List<string> roles) {
    var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

    var issuedAt = DateTime.UtcNow;
    var expiresAt = issuedAt.AddMinutes(expiration);

    var claims = new List<Claim>
    {
          new Claim(ClaimTypes.Name, username),
          new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
          new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(issuedAt).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
          new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(expiresAt).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
      };

    claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

    var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.Now.AddMinutes(Convert.ToDouble(expiration)),
        signingCredentials: credentials
    );

    var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);


    // Create the JWTTokenClaims object
    var tokenClaims = new JWTTokenClaims {
      Username = username,
      Roles = roles,
      IssuedAt = issuedAt,
      ExpiresAt = expiresAt
    };

    return (jwtToken, tokenClaims);
  }



  public static JWTTokenClaims? ValidateToken(string secret, string issuer, string audience, string token) {
    try {
      var key = Encoding.UTF8.GetBytes(secret);

      var tokenHandler = new JwtSecurityTokenHandler();
      var validationParameters = new TokenValidationParameters {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
      };

      var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

      var username = principal?.Identity?.Name;
      var roles = principal?.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();


      var issuedAtClaim = principal?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iat)?.Value;
      var expiresAtClaim = principal?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;

      DateTime? issuedAt = issuedAtClaim != null ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(issuedAtClaim)).UtcDateTime : (DateTime?)null;
      DateTime? expiresAt = expiresAtClaim != null ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiresAtClaim)).UtcDateTime : (DateTime?)null;


      return new JWTTokenClaims {
        Username = username,
        Roles = roles,
        IssuedAt = issuedAt,
        ExpiresAt = expiresAt
      };
    }
    catch {
      return null;
    }
  }

  public static string GenerateRefreshToken() {
    var randomNumber = new byte[32];
    using (var rng = RandomNumberGenerator.Create()) {
      rng.GetBytes(randomNumber);
      return Convert.ToBase64String(randomNumber);
    }
  }
}
