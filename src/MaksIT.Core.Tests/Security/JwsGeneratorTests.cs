using System.Security.Cryptography;
using MaksIT.Core.Security;
using MaksIT.Core.Security.JWK;
using MaksIT.Core.Security.JWS;


namespace MaksIT.Core.Tests.Security;

public class JwsGeneratorTests {
  [Fact]
  public void TryEncode_ValidRsaAndJwk_ReturnsTrueAndMessage() {
    using var rsa = RSA.Create(2048);
    var jwkResult = JwkGenerator.TryGenerateFromRSA(rsa, out var jwk, out var jwkError);
    Assert.True(jwkResult);
    Assert.NotNull(jwk);
    var header = new JwsHeader();
    var result = JwsGenerator.TryEncode(rsa, jwk!, header, out var message, out var error);
    Assert.True(result);
    Assert.NotNull(message);
    Assert.Null(error);
    Assert.False(string.IsNullOrEmpty(message!.Protected));
    Assert.False(string.IsNullOrEmpty(message.Signature));
  }

  [Fact]
  public void TryEncode_WithPayload_ReturnsEncodedPayload() {
    using var rsa = RSA.Create(2048);
    var jwkResult = JwkGenerator.TryGenerateFromRSA(rsa, out var jwk, out var jwkError);
    Assert.True(jwkResult);
    Assert.NotNull(jwk);
    var header = new JwsHeader();
    var payload = "test-payload";
    var result = JwsGenerator.TryEncode(rsa, jwk!, header, payload, out var message, out var error);
    Assert.True(result);
    Assert.NotNull(message);
    Assert.Null(error);
    Assert.False(string.IsNullOrEmpty(message!.Payload));
    // Decoded payload should match
    var decoded = Base64UrlUtility.DecodeToString(message.Payload);
    Assert.Equal(payload, decoded);
  }

  [Fact]
  public void TryEncode_InvalidRsa_ReturnsFalseAndError() {
    var fakeRsa = new FakeRsaThrows();
    var jwk = new Jwk { KeyType = JwkKeyType.Rsa.Name };
    var header = new JwsHeader();
    var result = JwsGenerator.TryEncode(fakeRsa, jwk, header, out var message, out var error);
    Assert.False(result);
    Assert.Null(message);
    Assert.NotNull(error);
  }

  [Fact]
  public void TryEncode_JwkWithKeyId_SetsHeaderKid() {
    using var rsa = RSA.Create(2048);
    var jwk = new Jwk { KeyType = JwkKeyType.Rsa.Name, KeyId = "my-key-id" };
    var header = new JwsHeader();
    var result = JwsGenerator.TryEncode(rsa, jwk, header, out var message, out var error);
    Assert.True(result);
    Assert.NotNull(message);
    Assert.Null(error);
    // Decode protected header
    var protectedJson = Base64UrlUtility.DecodeToString(message!.Protected);
    Assert.Contains("my-key-id", protectedJson);
  }

  [Fact]
  public void TryEncode_JwkWithoutKeyId_SetsHeaderJwk() {
    using var rsa = RSA.Create(2048);
    var jwk = new Jwk { KeyType = JwkKeyType.Rsa.Name };
    var header = new JwsHeader();
    var result = JwsGenerator.TryEncode(rsa, jwk, header, out var message, out var error);
    Assert.True(result);
    Assert.NotNull(message);
    Assert.Null(error);
    var protectedJson = Base64UrlUtility.DecodeToString(message!.Protected);
    Assert.Contains("jwk", protectedJson);
  }

  private class FakeRsaThrows : RSA {
    public override RSAParameters ExportParameters(bool includePrivateParameters)
      => throw new Exception("ExportParameters failed");
    public override byte[] Decrypt(byte[] data, RSAEncryptionPadding padding) => throw new NotImplementedException();
    public override byte[] Encrypt(byte[] data, RSAEncryptionPadding padding) => throw new NotImplementedException();
    public override byte[] SignHash(byte[] hash, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) => throw new Exception("SignData failed");
    public override bool VerifyHash(byte[] hash, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) => throw new NotImplementedException();
    public override void ImportParameters(RSAParameters parameters) => throw new NotImplementedException();
    protected override void Dispose(bool disposing) { }
  }
}
