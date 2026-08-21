using System.Diagnostics.CodeAnalysis;
using MaksIT.Core.Comb;
using MaksIT.Core.Security;
using MaksIT.Core.Security.JWT;


namespace MaksIT.Core.Cli;

/// <summary>
/// Thin wrappers around MaksIT.Core secret and token helpers.
/// </summary>
public static class SecretOperations {
  /// <summary>
  /// Generates a Base64 secret suitable for JWT signing or a password pepper.
  /// </summary>
  public static string GenerateSecret(int keySize = 32) =>
    JwtGenerator.GenerateSecret(keySize);

  /// <summary>
  /// Generates an opaque refresh token.
  /// </summary>
  public static string GenerateRefreshToken() =>
    JwtGenerator.GenerateRefreshToken();

  /// <summary>
  /// Generates a Base64 AES-256 key.
  /// </summary>
  public static string GenerateAesKey() =>
    AESGCMUtility.GenerateKeyBase64();

  /// <summary>
  /// Generates a COMB GUID for the given layout.
  /// </summary>
  public static Guid GenerateCombGuid(CombGuidType type) =>
    CombGuidGenerator.CreateCombGuid(DateTime.UtcNow, type);

  /// <summary>
  /// Signs an access JWT.
  /// </summary>
  public static bool TryGenerateJwt(
    JWTTokenGenerateRequest request,
    [NotNullWhen(true)] out string? token,
    [NotNullWhen(false)] out string? errorMessage
  ) {
    if (!JwtGenerator.TryGenerateToken(request, out var tokenData, out errorMessage)) {
      token = null;
      return false;
    }

    token = tokenData.Value.Item1;
    return true;
  }

  /// <summary>
  /// Validates an access JWT and returns its claims.
  /// </summary>
  public static bool TryValidateJwt(
    string secret,
    string issuer,
    string audience,
    string token,
    out JWTTokenClaims? claims,
    [NotNullWhen(false)] out string? errorMessage
  ) =>
    JwtGenerator.TryValidateToken(secret, issuer, audience, token, out claims, out errorMessage);

  /// <summary>
  /// Generates a Base32 TOTP shared secret.
  /// </summary>
  public static bool TryGenerateTotpSecret(
    [NotNullWhen(true)] out string? secret,
    [NotNullWhen(false)] out string? errorMessage
  ) =>
    TotpGenerator.TryGenerateSecret(out secret, out errorMessage);

  /// <summary>
  /// Generates TOTP recovery codes.
  /// </summary>
  public static bool TryGenerateRecoveryCodes(
    int count,
    [NotNullWhen(true)] out List<string>? codes,
    [NotNullWhen(false)] out string? errorMessage
  ) =>
    TotpGenerator.TryGenerateRecoveryCodes(count, out codes, out errorMessage);

  /// <summary>
  /// Builds an <c>otpauth://</c> URI for authenticator apps.
  /// </summary>
  public static bool TryGenerateTotpAuthLink(
    string label,
    string username,
    string secret,
    string issuer,
    [NotNullWhen(true)] out string? authLink,
    [NotNullWhen(false)] out string? errorMessage
  ) =>
    TotpGenerator.TryGenerateTotpAuthLink(
      label,
      username,
      secret,
      issuer,
      algorithm: null,
      digits: null,
      period: null,
      out authLink,
      out errorMessage
    );

  /// <summary>
  /// Validates a TOTP code against a Base32 secret.
  /// </summary>
  public static bool TryValidateTotp(
    string totpCode,
    string base32Secret,
    int timeTolerance,
    out bool isValid,
    [NotNullWhen(false)] out string? errorMessage
  ) =>
    TotpGenerator.TryValidate(totpCode, base32Secret, timeTolerance, out isValid, out errorMessage);

  /// <summary>
  /// Creates a salted password hash with the given pepper.
  /// </summary>
  public static bool TryHashPassword(
    string password,
    string pepper,
    [NotNullWhen(true)] out (string Salt, string Hash)? saltedHash,
    [NotNullWhen(false)] out string? errorMessage
  ) =>
    PasswordHasher.TryCreateSaltedHash(password, pepper, out saltedHash, out errorMessage);
}
