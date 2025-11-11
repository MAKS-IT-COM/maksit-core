using System.Text;
using System.Security.Cryptography;
using MaksIT.Core.Security.JWK;
using MaksIT.Core.Security.JWS;


namespace MaksIT.Core.Tests.Security;

public class JwsGeneratorTests {
  private static (RSA rsa, Jwk jwk) GenerateRsaAndJwk() {
    var rsa = RSA.Create(2048);
    var result = JwkGenerator.TryGenerateRsaFromRsa(rsa, true, null, null, null, out var jwk, out var errorMessage);
    Assert.True(result, errorMessage);
    Assert.NotNull(jwk);
    return (rsa, jwk);
  }

  [Fact]
  public void Encode_WithStringPayload_ProducesValidJws() {
    var (rsa, jwk) = GenerateRsaAndJwk();
    var header = new JwsHeader();
    var payload = "test-payload";

    var result = JwsGenerator.TryEncode(rsa, jwk, header, payload, out var jws, out var errorMessage);
    Assert.True(result, errorMessage);
    Assert.NotNull(jws);
    Assert.False(string.IsNullOrEmpty(jws.Protected));
    Assert.False(string.IsNullOrEmpty(jws.Payload));
    Assert.False(string.IsNullOrEmpty(jws.Signature));
  }

  [Fact]
  public void Encode_WithByteArrayPayload_ProducesValidJws() {
    var (rsa, jwk) = GenerateRsaAndJwk();
    var header = new JwsHeader();
    var payload = Encoding.UTF8.GetBytes("test-bytes");

    var result = JwsGenerator.TryEncode(rsa, jwk, header, payload, out var jws, out var errorMessage);
    Assert.True(result, errorMessage);
    Assert.NotNull(jws);
    Assert.False(string.IsNullOrEmpty(jws.Protected));
    Assert.False(string.IsNullOrEmpty(jws.Payload));
    Assert.False(string.IsNullOrEmpty(jws.Signature));
  }

  [Fact]
  public void Encode_WithGenericPayload_ProducesValidJws() {
    var (rsa, jwk) = GenerateRsaAndJwk();
    var header = new JwsHeader();
    var payload = new { foo = "bar", n = 42 };

    var result = JwsGenerator.TryEncode(rsa, jwk, header, payload, out var jws, out var errorMessage);
    Assert.True(result, errorMessage);
    Assert.NotNull(jws);
    Assert.False(string.IsNullOrEmpty(jws.Protected));
    Assert.False(string.IsNullOrEmpty(jws.Payload));
    Assert.False(string.IsNullOrEmpty(jws.Signature));
  }

  [Fact]
  public void Encode_PostAsGet_ProducesValidJws() {
    var (rsa, jwk) = GenerateRsaAndJwk();
    var header = new JwsHeader();

    var result = JwsGenerator.TryEncode(rsa, jwk, header, out var jws, out var errorMessage);
    Assert.True(result, errorMessage);
    Assert.NotNull(jws);
    Assert.False(string.IsNullOrEmpty(jws.Protected));
    Assert.Equal(string.Empty, jws.Payload);
    Assert.False(string.IsNullOrEmpty(jws.Signature));
  }

  [Fact]
  public void GetKeyAuthorization_ReturnsExpectedFormat() {
    var (rsa, jwk) = GenerateRsaAndJwk();
    var token = "test-token";

    var result = JwsGenerator.TryGetKeyAuthorization(jwk, token, out var keyAuth, out var errorMessage);
    Assert.True(result, errorMessage);
    Assert.NotNull(keyAuth);
    Assert.StartsWith(token + ".", keyAuth);
    Assert.True(keyAuth.Length > token.Length + 1);
  }
}
