using System.Security.Cryptography;
using MaksIT.Core.Security;
using MaksIT.Core.Security.JWK;


namespace MaksIT.Core.Tests.Security.JWK;

public class JwkThumbprintUtilityTests {
  [Fact]
  public void TryGetSha256Thumbprint_ValidRsaJwk_ReturnsTrueAndThumbprint() {
    using var rsa = RSA.Create(2048);
    var genResult = JwkGenerator.TryGenerateFromRSA(rsa, out var jwk, out var genError);
    Assert.True(genResult);
    Assert.NotNull(jwk);
    var result = JwkThumbprintUtility.TryGetSha256Thumbprint(jwk!, out var thumbprint, out var error);
    Assert.True(result);
    Assert.False(string.IsNullOrEmpty(thumbprint));
    Assert.Null(error);
    // Should be base64url encoded and of expected length (SHA256 =32 bytes)
    var decoded = Base64UrlUtility.Decode(thumbprint!);
    Assert.Equal(32, decoded.Length);
  }

  [Fact]
  public void TryGetSha256Thumbprint_NullExponentOrModulus_ReturnsFalseAndError() {
    var jwk = new Jwk { RsaExponent = null, RsaModulus = null };
    var result = JwkThumbprintUtility.TryGetSha256Thumbprint(jwk, out var thumbprint, out var error);
    Assert.False(result);
    Assert.Null(thumbprint);
    Assert.Contains("exponent or modulus", error);
  }

  [Fact]
  public void TryGetKeyAuthorization_ValidJwk_ReturnsTrueAndKeyAuthorization() {
    using var rsa = RSA.Create(2048);
    var genResult = JwkGenerator.TryGenerateFromRSA(rsa, out var jwk, out var genError);
    Assert.True(genResult);
    Assert.NotNull(jwk);
    var token = "test-token";
    var result = JwkThumbprintUtility.TryGetKeyAuthorization(jwk!, token, out var keyAuth, out var error);
    Assert.True(result);
    Assert.Null(error);
    Assert.StartsWith(token + ".", keyAuth);
    var parts = keyAuth!.Split('.');
    Assert.Equal(2, parts.Length);
    Assert.False(string.IsNullOrEmpty(parts[1]));
  }

  [Fact]
  public void TryGetKeyAuthorization_NullExponentOrModulus_ReturnsFalseAndError() {
    var jwk = new Jwk { RsaExponent = null, RsaModulus = null };
    var token = "test-token";
    var result = JwkThumbprintUtility.TryGetKeyAuthorization(jwk, token, out var keyAuth, out var error);
    Assert.False(result);
    Assert.Null(keyAuth);
    Assert.Contains("Failed to compute thumbprint", error);
  }
}
