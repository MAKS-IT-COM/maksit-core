using System.Security.Cryptography;
using System.Diagnostics.CodeAnalysis;


namespace MaksIT.Core.Security.JWK;

/// <summary>
/// Provides utilities for JWK (JSON Web Key) operations, including RFC7638 thumbprint computation and key generation.
/// </summary>
public static class JwkGenerator {
  public static bool TryGenerateFromRSA(
    RSA rsa,
    [NotNullWhen(true)]
    out Jwk? jwk,
    [NotNullWhen(false)] out string? errorMessage
) {
    try {
      var publicParameters = rsa.ExportParameters(false);

      var exp = publicParameters.Exponent;
      var mod = publicParameters.Modulus;

      if (exp == null || mod == null)
        throw new ArgumentException("RSA parameters are missing exponent or modulus.");

      jwk = new Jwk {
        KeyType = JwkKeyType.Rsa.Name,
        RsaExponent = Base64UrlUtility.Encode(exp),
        RsaModulus = Base64UrlUtility.Encode(mod),
      };
      errorMessage = null;
      return true;
    }
    catch (Exception ex) {
      jwk = null;
      errorMessage = ex.Message;
      return false;
    }
  }
}
