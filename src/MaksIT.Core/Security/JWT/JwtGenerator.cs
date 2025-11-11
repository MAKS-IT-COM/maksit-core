using System.Text;
using System.Security.Claims;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Diagnostics.CodeAnalysis;
using Microsoft.IdentityModel.Tokens;


namespace MaksIT.Core.Security.JWT;

public static class JwtGenerator {
  /// <summary>
  /// Attempts to generate a JWT token using the specified request parameters.
  /// </summary>
  /// <param name="request">
  /// The <see cref="JWTTokenGenerateRequest"/> containing the secret, issuer, audience, expiration, user ID, username, and roles for the token.
  /// </param>
  /// <param name="tokenData">
  /// When this method returns <c>true</c>, contains a tuple with the generated JWT string and its associated claims; otherwise, <c>null</c>.
  /// </param>
  /// <param name="errorMessage">
  /// When this method returns <c>false</c>, contains the error message describing the failure; otherwise, <c>null</c>.
  /// </param>
  /// <returns>
  /// <c>true</c> if the token was successfully generated; otherwise, <c>false</c>.
  /// </returns>
  public static bool TryGenerateToken(JWTTokenGenerateRequest request, [NotNullWhen(true)] out (string, JWTTokenClaims)? tokenData, [NotNullWhen(false)] out string? errorMessage) {
    try {
      var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(request.Secret));
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

      if (request.AclEntries != null)
        claims.AddRange(request.AclEntries.Select(acl => new Claim(CustomClaims.AclEntry.Name, acl)));

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

  /// <summary>
  /// Attempts to validate a JWT token using the provided secret, issuer, audience, and token string.
  /// </summary>
  /// <param name="secret"></param>
  /// <param name="issuer"></param>
  /// <param name="audience"></param>
  /// <param name="token"></param>
  /// <param name="tokenClaims"></param>
  /// <param name="errorMessage"></param>
  /// <returns></returns>
  /// <exception cref="SecurityTokenException"></exception>
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
    var aclEntries = principal.Claims.Where(c => c.Type == CustomClaims.AclEntry.Name).Select(c => c.Value).ToList();

    var issuedAtClaim = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iat)?.Value;
    var expiresAtClaim = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;

    DateTime? issuedAt = issuedAtClaim != null ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(issuedAtClaim)).UtcDateTime : null;
    DateTime? expiresAt = expiresAtClaim != null ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiresAtClaim)).UtcDateTime : null;

    return new JWTTokenClaims {
      UserId = userId,
      Username = username,
      Roles = roles,
      AclEntries = aclEntries,
      IssuedAt = issuedAt,
      ExpiresAt = expiresAt
    };
  }

  // Private helper method for generating random bytes
  private static byte[] GetRandomBytes(int size) {
    var bytes = new byte[size];
    RandomNumberGenerator.Fill(bytes);
    return bytes;
  }
}
