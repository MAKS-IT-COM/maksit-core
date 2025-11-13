using System.Security.Cryptography;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using MaksIT.Core.Extensions;


namespace MaksIT.Core.Security.JWK;

public static class JwkThumbprintUtility {
  /// <summary>
  /// Returns the key authorization string for ACME challenges.
  /// </summary>
  public static bool TryGetKeyAuthorization(
    Jwk jwk,
    string token,
    [NotNullWhen(true)] out string? keyAuthorization,
    [NotNullWhen(false)] out string? errorMessage
  ) {
    keyAuthorization = null;
    errorMessage = null;
    if (!TryGetSha256Thumbprint(jwk, out var thumbprint, out var thumbprintError)) {
      errorMessage = $"Failed to compute thumbprint: {thumbprintError}";
      return false;
    }
    keyAuthorization = $"{token}.{thumbprint}";
    return true;
  }

  /// <summary>
  /// Computes the RFC7638 JWK SHA-256 thumbprint (Base64Url encoded).
  /// For thumbprint calculation, always build the JSON string manually or use OrderedJwk for correct property order.
  /// </summary>
  public static bool TryGetSha256Thumbprint(
    Jwk jwk,
    [NotNullWhen(true)] out string? thumbprint,
    [NotNullWhen(false)] out string? errorMessage
  ) {
    thumbprint = null;
    errorMessage = null;
    try {
      if (jwk.RsaExponent == null || jwk.RsaModulus == null)
        throw new ArgumentException("RSA exponent or modulus is null.");
      var thumbprintObj = new OrderedJwk {
        E = jwk.RsaExponent,
        Kty = JwkKeyType.Rsa.Name,
        N = jwk.RsaModulus
      };
      var json = thumbprintObj.ToJson();
      thumbprint = Base64UrlUtility.Encode(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
      return true;
    } catch (Exception ex) {
      errorMessage = ex.Message;
      return false;
    }
  }
}
