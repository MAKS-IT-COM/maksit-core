using MaksIT.Core.Extensions;
using MaksIT.Core.Security.JWT;


namespace MaksIT.Core.Cli;

/// <summary>
/// Non-interactive command handlers: values on stdout, errors on stderr, exit 0/1.
/// </summary>
public static class CliActions {
  /// <summary>
  /// Writes an error to stderr and returns exit code 1.
  /// </summary>
  public static int Fail(string errorMessage) {
    Console.Error.WriteLine(errorMessage);
    return 1;
  }

  /// <summary>
  /// Generates a Base64 secret.
  /// </summary>
  public static int GenerateSecret(int bytes) {
    if (!InputParsers.TryParsePositiveInt(bytes.ToString(), 32, out var keySize, out var errorMessage))
      return Fail(errorMessage!);

    Console.WriteLine(SecretOperations.GenerateSecret(keySize));
    return 0;
  }

  /// <summary>
  /// Generates an opaque refresh token.
  /// </summary>
  public static int GenerateRefreshToken() {
    Console.WriteLine(SecretOperations.GenerateRefreshToken());
    return 0;
  }

  /// <summary>
  /// Generates a Base64 AES-256 key.
  /// </summary>
  public static int GenerateAesKey() {
    Console.WriteLine(SecretOperations.GenerateAesKey());
    return 0;
  }

  /// <summary>
  /// Generates a COMB GUID.
  /// </summary>
  public static int GenerateCombGuid(string? typeName) {
    if (!InputParsers.TryParseCombGuidType(typeName, out var type, out var errorMessage))
      return Fail(errorMessage!);

    Console.WriteLine(SecretOperations.GenerateCombGuid(type));
    return 0;
  }

  /// <summary>
  /// Signs an access JWT.
  /// </summary>
  public static int GenerateJwt(
    string secret,
    string issuer,
    string audience,
    int expiration,
    string? userId,
    string? username,
    string? roles,
    string? aclEntries
  ) {
    if (!InputParsers.TryParsePositiveInt(expiration.ToString(), 60, out var minutes, out var errorMessage))
      return Fail(errorMessage!);

    var request = new JWTTokenGenerateRequest {
      Secret = secret,
      Issuer = issuer,
      Audience = audience,
      Expiration = minutes,
      UserId = EmptyToNull(userId),
      Username = EmptyToNull(username),
      Roles = InputParsers.ParseOptionalList(roles),
      AclEntries = InputParsers.ParseOptionalList(aclEntries)
    };

    if (!SecretOperations.TryGenerateJwt(request, out var token, out var generateError))
      return Fail(generateError);

    Console.WriteLine(token);
    return 0;
  }

  /// <summary>
  /// Validates an access JWT and writes claims JSON.
  /// </summary>
  public static int ValidateJwt(string secret, string issuer, string audience, string token) {
    if (!SecretOperations.TryValidateJwt(secret, issuer, audience, token, out var claims, out var errorMessage))
      return Fail(errorMessage);

    Console.WriteLine(claims.ToJson());
    return 0;
  }

  /// <summary>
  /// Generates a Base32 TOTP secret.
  /// </summary>
  public static int GenerateTotpSecret() {
    if (!SecretOperations.TryGenerateTotpSecret(out var secret, out var errorMessage))
      return Fail(errorMessage);

    Console.WriteLine(secret);
    return 0;
  }

  /// <summary>
  /// Generates TOTP recovery codes (one per line).
  /// </summary>
  public static int GenerateRecoveryCodes(int count) {
    if (!InputParsers.TryParsePositiveInt(count.ToString(), 10, out var codeCount, out var errorMessage))
      return Fail(errorMessage!);

    if (!SecretOperations.TryGenerateRecoveryCodes(codeCount, out var codes, out var generateError))
      return Fail(generateError);

    foreach (var code in codes)
      Console.WriteLine(code);

    return 0;
  }

  /// <summary>
  /// Builds an otpauth URI.
  /// </summary>
  public static int GenerateTotpAuthLink(string label, string username, string secret, string issuer) {
    if (!SecretOperations.TryGenerateTotpAuthLink(label, username, secret, issuer, out var authLink, out var errorMessage))
      return Fail(errorMessage);

    Console.WriteLine(authLink);
    return 0;
  }

  /// <summary>
  /// Validates a TOTP code. Exit 1 when the code is invalid.
  /// </summary>
  public static int ValidateTotp(string secret, string code, int tolerance) {
    if (!InputParsers.TryParsePositiveInt(tolerance.ToString(), 1, out var timeTolerance, out var errorMessage))
      return Fail(errorMessage!);

    if (!SecretOperations.TryValidateTotp(code, secret, timeTolerance, out var isValid, out var validateError))
      return Fail(validateError);

    Console.WriteLine(isValid ? "valid" : "invalid");
    return isValid ? 0 : 1;
  }

  /// <summary>
  /// Creates a salted password hash as JSON.
  /// </summary>
  public static int HashPassword(string pepper, string password) {
    if (string.IsNullOrEmpty(password))
      return Fail("Password is required.");

    if (!SecretOperations.TryHashPassword(password, pepper, out var saltedHash, out var errorMessage))
      return Fail(errorMessage);

    Console.WriteLine(new { salt = saltedHash.Value.Salt, hash = saltedHash.Value.Hash }.ToJson());
    return 0;
  }

  private static string? EmptyToNull(string? value) =>
    string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
