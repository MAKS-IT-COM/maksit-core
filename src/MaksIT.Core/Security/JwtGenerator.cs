using System.Text;
using System.Security.Claims;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Diagnostics.CodeAnalysis;
using Microsoft.IdentityModel.Tokens;

namespace MaksIT.Core.Security;

public class JWTTokenClaims {
  public string? UserId { get; set; }
  public string? Username { get; set; }
  public List<string>? Roles { get; set; }
  public DateTime? IssuedAt { get; set; }
  public DateTime? ExpiresAt { get; set; }
}

public class JWTTokenGenerateRequest {
  public required string Secret { get; set; }
  public required string Issuer { get; set; }
  public required string Audience { get; set; }
  public double Expiration { get; set; }
  public string? UserId { get; set; }
  public string? Username { get; set; }
  public List<string>? Roles { get; set; }

}

public static class JwtGenerator {
  public static bool TryGenerateToken(JWTTokenGenerateRequest request, [NotNullWhen(true)] out (string, JWTTokenClaims)? tokenData, [NotNullWhen(false)] out string? errorMessage) {
    try {
      var secretKey = GetSymmetricSecurityKey(request.Secret);
      var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

      var issuedAt = DateTime.UtcNow;
      var expiresAt = issuedAt.AddMinutes(request.Expiration);

      var claims = new List<Claim>
      {
          new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
          new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(issuedAt).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
          new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(expiresAt).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
      };

      if (request.UserId != null)
        claims.Add(new Claim(ClaimTypes.NameIdentifier, request.UserId));

      if (request.Username != null)
        claims.Add(new Claim(ClaimTypes.Name, request.Username));

      if (request.Roles !=null)
        claims.AddRange(request.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

      var tokenDescriptor = new JwtSecurityToken(
          issuer: request.Issuer,
          audience: request.Audience,
          claims: claims,
          expires: expiresAt,
          signingCredentials: credentials
      );

      var jwtToken = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

      var tokenClaims = new JWTTokenClaims {
        UserId = request.UserId,
        Username = request.Username,
        IssuedAt = issuedAt,
        ExpiresAt = expiresAt
      };

      if (request.Roles != null)
        tokenClaims.Roles = request.Roles;

      tokenData = (jwtToken, tokenClaims);
      errorMessage = null;
      return true;
    }
    catch (Exception ex) {
      tokenData = null;
      errorMessage = ex.Message;
      return false;
    }
  }

  public static string GenerateSecret(int keySize = 32) => Convert.ToBase64String(GetRandomBytes(keySize));

  public static bool TryValidateToken(
    string secret,
    string issuer,
    string audience,
    string token,
    out JWTTokenClaims? tokenClaims,
    [NotNullWhen(false)] out string? errorMessage
  ) {
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

      // Validate the algorithm used
      if (validatedToken is JwtSecurityToken jwtToken && jwtToken.Header.Alg != SecurityAlgorithms.HmacSha256)
        throw new SecurityTokenException("Invalid token algorithm");

      tokenClaims = ExtractClaims(principal);
      errorMessage = null;
      return true;
    }
    catch (Exception ex) {
      tokenClaims = null;
      errorMessage = ex.Message;
      return false;
    }
  }

  public static string GenerateRefreshToken() => Convert.ToBase64String(GetRandomBytes(32));

  // Private helper method to extract claims
  private static JWTTokenClaims? ExtractClaims(ClaimsPrincipal principal) {
    var userId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

    var username = principal.Identity?.Name;
    var roles = principal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

    var issuedAtClaim = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iat)?.Value;
    var expiresAtClaim = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;

    DateTime? issuedAt = issuedAtClaim != null ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(issuedAtClaim)).UtcDateTime : (DateTime?)null;
    DateTime? expiresAt = expiresAtClaim != null ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiresAtClaim)).UtcDateTime : (DateTime?)null;

    return new JWTTokenClaims {
      UserId = userId,
      Username = username,
      Roles = roles,
      IssuedAt = issuedAt,
      ExpiresAt = expiresAt
    };
  }

  // Private helper method to get a symmetric security key
  private static SymmetricSecurityKey GetSymmetricSecurityKey(string secret) => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

  // Private helper method for generating random bytes
  private static byte[] GetRandomBytes(int size) {
    var bytes = new byte[size];
    RandomNumberGenerator.Fill(bytes);
    return bytes;
  }
}
