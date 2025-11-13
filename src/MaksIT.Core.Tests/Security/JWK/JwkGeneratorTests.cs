using System.Security.Cryptography;
using MaksIT.Core.Security.JWK;


namespace MaksIT.Core.Tests.Security.JWK;

public class JwkGeneratorTests {
  [Fact]
  public void TryGenerateFromRSA_ValidRsa_ReturnsTrueAndJwk() {
    using var rsa = RSA.Create(2048);
    var result = JwkGenerator.TryGenerateFromRSA(rsa, out var jwk, out var errorMessage);
    Assert.True(result);
    Assert.NotNull(jwk);
    Assert.Null(errorMessage);
    Assert.Equal(JwkKeyType.Rsa.Name, jwk!.KeyType);
    Assert.False(string.IsNullOrEmpty(jwk.RsaExponent));
    Assert.False(string.IsNullOrEmpty(jwk.RsaModulus));
  }

  [Fact]
  public void TryGenerateFromRSA_MissingExponentOrModulus_ReturnsFalseAndError() {
    using var rsa = RSA.Create();
    // ExportParameters returns valid values, so we simulate missing exponent/modulus by mocking
    // Instead, test with a custom RSA implementation that throws
    var fakeRsa = new FakeRsaMissingParams();
    var result = JwkGenerator.TryGenerateFromRSA(fakeRsa, out var jwk, out var errorMessage);
    Assert.False(result);
    Assert.Null(jwk);
    Assert.Contains("missing exponent or modulus", errorMessage);
  }

  [Fact]
  public void TryGenerateFromRSA_ExportParametersThrows_ReturnsFalseAndError() {
    var fakeRsa = new FakeRsaThrows();
    var result = JwkGenerator.TryGenerateFromRSA(fakeRsa, out var jwk, out var errorMessage);
    Assert.False(result);
    Assert.Null(jwk);
    Assert.Contains("ExportParameters failed", errorMessage);
  }

  private class FakeRsaMissingParams : RSA {
    public override RSAParameters ExportParameters(bool includePrivateParameters)
        => new RSAParameters { Exponent = null, Modulus = null };
    // ...other abstract members throw NotImplementedException
    public override byte[] Decrypt(byte[] data, RSAEncryptionPadding padding) => throw new NotImplementedException();
    public override byte[] Encrypt(byte[] data, RSAEncryptionPadding padding) => throw new NotImplementedException();
    public override byte[] SignHash(byte[] hash, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) => throw new NotImplementedException();
    public override bool VerifyHash(byte[] hash, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) => throw new NotImplementedException();
    public override void ImportParameters(RSAParameters parameters) => throw new NotImplementedException();
    protected override void Dispose(bool disposing) { }
  }

  private class FakeRsaThrows : RSA {
    public override RSAParameters ExportParameters(bool includePrivateParameters)
        => throw new Exception("ExportParameters failed");
    // ...other abstract members throw NotImplementedException
    public override byte[] Decrypt(byte[] data, RSAEncryptionPadding padding) => throw new NotImplementedException();
    public override byte[] Encrypt(byte[] data, RSAEncryptionPadding padding) => throw new NotImplementedException();
    public override byte[] SignHash(byte[] hash, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) => throw new NotImplementedException();
    public override bool VerifyHash(byte[] hash, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) => throw new NotImplementedException();
    public override void ImportParameters(RSAParameters parameters) => throw new NotImplementedException();
    protected override void Dispose(bool disposing) { }
  }
}
