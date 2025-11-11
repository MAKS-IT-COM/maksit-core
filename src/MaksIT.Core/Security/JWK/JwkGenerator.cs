using System.Text;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using MaksIT.Core.Extensions;

namespace MaksIT.Core.Security.JWK;

/// <summary>
/// Provides utilities for JWK (JSON Web Key) operations, including RFC 7638 thumbprint computation and key generation.
/// </summary>
public static class JwkGenerator {
  public static bool TryGenerateRsa(int keySize, bool includePrivate, JwkAlgorithm? alg, string? use, string[]? keyOps, [NotNullWhen(true)] out Jwk? jwk, [NotNullWhen(false)] out string? errorMessage) {
    try {
      jwk = GenerateRsa(keySize, includePrivate, alg, use, keyOps);
      errorMessage = null;
      return true;
    } catch (Exception ex) {
      jwk = null;
      errorMessage = ex.Message;
      return false;
    }
  }

  public static bool TryGenerateEc(JwkCurve? curve, bool includePrivate, JwkAlgorithm? alg, string? use, string[]? keyOps, [NotNullWhen(true)] out Jwk? jwk, [NotNullWhen(false)] out string? errorMessage) {
    try {
      jwk = GenerateEc(curve, includePrivate, alg, use, keyOps);
      errorMessage = null;
      return true;
    } catch (Exception ex) {
      jwk = null;
      errorMessage = ex.Message;
      return false;
    }
  }

  public static bool TryGenerateOct(int keySizeBits, JwkAlgorithm? alg, string? use, string[]? keyOps, [NotNullWhen(true)] out Jwk? jwk, [NotNullWhen(false)] out string? errorMessage) {
    try {
      jwk = GenerateOct(keySizeBits, alg, use, keyOps);
      errorMessage = null;
      return true;
    } catch (Exception ex) {
      jwk = null;
      errorMessage = ex.Message;
      return false;
    }
  }

  public static bool TryGenerateRsaFromRsa(RSA rsa, bool includePrivate, JwkAlgorithm? alg, string? use, string[]? keyOps, [NotNullWhen(true)] out Jwk? jwk, [NotNullWhen(false)] out string? errorMessage) {
    try {
      jwk = GenerateRsaFromRsa(rsa, includePrivate, alg, use, keyOps);
      errorMessage = null;
      return true;
    } catch (Exception ex) {
      jwk = null;
      errorMessage = ex.Message;
      return false;
    }
  }

  public static bool TryComputeThumbprint(
      Jwk jwk,
      [NotNullWhen(true)] out string? thumbprint,
      [NotNullWhen(false)] out string? errorMessage) {
    thumbprint = null;
    errorMessage = null;

    if (jwk == null) {
      errorMessage = "JWK cannot be null.";
      return false;
    }
    if (string.IsNullOrEmpty(jwk.E) || string.IsNullOrEmpty(jwk.N)) {
      errorMessage = "JWK must have Exponent and Modulus set.";
      return false;
    }

    try {
      // RFC 7638: Lexicographic order: e, kty, n
      var orderedJwk = new OrderedJwk {
        E = jwk.E!,
        Kty = "RSA",
        N = jwk.N!
      };

      var json = orderedJwk.ToJson();
      var hash = SHA256.HashData(Encoding.UTF8.GetBytes(json));
      thumbprint = Base64UrlEncode(hash);
      return true;
    }
    catch (Exception ex) {
      errorMessage = ex.Message;
      return false;
    }
  }

  private static Jwk GenerateRsa(int keySize = 2048, bool includePrivate = false, JwkAlgorithm? alg = null, string? use = null, string[]? keyOps = null) {
    using var rsa = RSA.Create(keySize);
    var parameters = rsa.ExportParameters(includePrivate);
    var jwk = new Jwk {
      Kty = JwkKeyType.Rsa.Name,
      N = Base64UrlEncode(parameters.Modulus!),
      E = Base64UrlEncode(parameters.Exponent!),
      Alg = (alg ?? (keySize >= 4096 ? JwkAlgorithm.Rs512 : JwkAlgorithm.Rs256)).Name,
      Use = use,
      KeyOps = keyOps,
    };
    if (includePrivate) {
      jwk.D = Base64UrlEncode(parameters.D!);
      jwk.P = Base64UrlEncode(parameters.P!);
      jwk.Q = Base64UrlEncode(parameters.Q!);
      jwk.DP = Base64UrlEncode(parameters.DP!);
      jwk.DQ = Base64UrlEncode(parameters.DQ!);
      jwk.QI = Base64UrlEncode(parameters.InverseQ!);
    }
    jwk.Kid = ComputeKid(jwk);
    return jwk;
  }

  private static Jwk GenerateEc(JwkCurve? curve = null, bool includePrivate = false, JwkAlgorithm? alg = null, string? use = null, string[]? keyOps = null) {
    curve ??= JwkCurve.P256;
    ECCurve ecCurve = curve.Name switch {
      "P-256" => ECCurve.NamedCurves.nistP256,
      "P-384" => ECCurve.NamedCurves.nistP384,
      "P-521" => ECCurve.NamedCurves.nistP521,
      _ => throw new ArgumentException($"Unsupported curve: {curve.Name}")
    };
    using var ec = ECDsa.Create(ecCurve);
    var parameters = ec.ExportParameters(includePrivate);
    var jwk = new Jwk {
      Kty = JwkKeyType.Ec.Name,
      Crv = curve.Name,
      X = Base64UrlEncode(parameters.Q.X!),
      Y = Base64UrlEncode(parameters.Q.Y!),
      Alg = (alg ?? (curve == JwkCurve.P384 ? JwkAlgorithm.Es384 : curve == JwkCurve.P521 ? JwkAlgorithm.Es512 : JwkAlgorithm.Es256)).Name,
      Use = use,
      KeyOps = keyOps,
    };
    if (includePrivate && parameters.D != null) {
      jwk.D_EC = Base64UrlEncode(parameters.D);
    }
    jwk.Kid = ComputeKid(jwk);
    return jwk;
  }

  private static Jwk GenerateOct(int keySizeBits = 256, JwkAlgorithm? alg = null, string? use = null, string[]? keyOps = null) {
    var key = RandomNumberGenerator.GetBytes(keySizeBits / 8);
    var jwk = new Jwk {
      Kty = JwkKeyType.Oct.Name,
      K = Base64UrlEncode(key),
      Alg = (alg ?? (keySizeBits == 256 ? JwkAlgorithm.A256Gcm : keySizeBits == 128 ? JwkAlgorithm.A128Gcm : JwkAlgorithm.A512Gcm)).Name,
      Use = use,
      KeyOps = keyOps,
    };
    jwk.Kid = ComputeKid(jwk);
    return jwk;
  }

  private static Jwk GenerateRsaFromRsa(RSA rsa, bool includePrivate = false, JwkAlgorithm? alg = null, string? use = null, string[]? keyOps = null) {
    if (rsa == null) throw new ArgumentNullException(nameof(rsa));
    var parameters = rsa.ExportParameters(includePrivate);
    var jwk = new Jwk {
      Kty = JwkKeyType.Rsa.Name,
      N = Base64UrlUtility.Encode(parameters.Modulus!),
      E = Base64UrlUtility.Encode(parameters.Exponent!),
      Alg = (alg ?? JwkAlgorithm.Rs256).Name,
      Use = use,
      KeyOps = keyOps,
    };
    if (includePrivate) {
      jwk.D = Base64UrlUtility.Encode(parameters.D!);
      jwk.P = Base64UrlUtility.Encode(parameters.P!);
      jwk.Q = Base64UrlUtility.Encode(parameters.Q!);
      jwk.DP = Base64UrlUtility.Encode(parameters.DP!);
      jwk.DQ = Base64UrlUtility.Encode(parameters.DQ!);
      jwk.QI = Base64UrlUtility.Encode(parameters.InverseQ!);
    }
    jwk.Kid = ComputeKid(jwk);
    return jwk;
  }

  private static string Base64UrlEncode(byte[] data) {
    return Base64UrlUtility.Encode(data);
  }

  private static string ComputeKid(Jwk jwk) {
    // Use thumbprint as kid if possible
    if (jwk.Kty == "RSA" && !string.IsNullOrEmpty(jwk.N) && !string.IsNullOrEmpty(jwk.E)) {
      TryComputeThumbprint(jwk, out var thumb, out _);
      return thumb ?? Guid.NewGuid().ToString("N");
    }
    // For EC and oct, use a hash of the key material
    using var sha = SHA256.Create();
    string keyMaterial = jwk.Kty switch {
      "EC" => jwk.X + jwk.Y + jwk.Crv,
      "oct" => jwk.K,
      _ => null
    } ?? Guid.NewGuid().ToString();
    var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(keyMaterial));
    return Base64UrlEncode(hash);
  }

  // Helper class for correct property order and names
  private class OrderedJwk {
    [JsonPropertyName("e")]
    public string E { get; set; } = default!;

    [JsonPropertyName("kty")]
    public string Kty { get; set; } = default!;

    [JsonPropertyName("n")]
    public string N { get; set; } = default!;
  }
}
