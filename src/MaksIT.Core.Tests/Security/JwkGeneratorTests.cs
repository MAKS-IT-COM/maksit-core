using MaksIT.Core.Security.JWK;

namespace MaksIT.Core.Tests.Security;

public class JwkGeneratorTests
{
    [Fact]
    public void GenerateRsa_PublicKey_ShouldHaveRequiredFields()
    {
        var result = JwkGenerator.TryGenerateRsa(2048, false, null, null, null, out var jwk, out var errorMessage);
        Assert.True(result, errorMessage);
        Assert.NotNull(jwk);
        Assert.Equal(JwkKeyType.Rsa.Name, jwk.KeyType);
        Assert.NotNull(jwk.RsaModulus);
        Assert.NotNull(jwk.RsaExponent);
        Assert.Null(jwk.PrivateKey);
        Assert.Null(jwk.RsaFirstPrimeFactor);
        Assert.Null(jwk.RsaSecondPrimeFactor);
        Assert.Null(jwk.RsaFirstFactorCRTExponent);
        Assert.Null(jwk.RsaSecondFactorCRTExponent);
        Assert.Null(jwk.RsaFirstCRTCoefficient);
        Assert.False(string.IsNullOrWhiteSpace(jwk.KeyId));
    }

    [Fact]
    public void GenerateRsa_PrivateKey_ShouldHavePrivateFields()
    {
        var result = JwkGenerator.TryGenerateRsa(2048, true, null, null, null, out var jwk, out var errorMessage);
        Assert.True(result, errorMessage);
        Assert.NotNull(jwk);
        Assert.Equal(JwkKeyType.Rsa.Name, jwk.KeyType);
        Assert.NotNull(jwk.PrivateKey);
        Assert.NotNull(jwk.RsaFirstPrimeFactor);
        Assert.NotNull(jwk.RsaSecondPrimeFactor);
        Assert.NotNull(jwk.RsaFirstFactorCRTExponent);
        Assert.NotNull(jwk.RsaSecondFactorCRTExponent);
        Assert.NotNull(jwk.RsaFirstCRTCoefficient);
    }

    [Theory]
    [InlineData("P-256")]
    [InlineData("P-384")]
    [InlineData("P-521")]
    public void GenerateEc_PublicKey_ShouldHaveRequiredFields(string curve)
    {
        var curveObj = JwkCurve.GetAll<JwkCurve>().First(c => c.Name == curve);
        var result = JwkGenerator.TryGenerateEc(curveObj, false, null, null, null, out var jwk, out var errorMessage);
        Assert.True(result, errorMessage);
        Assert.NotNull(jwk);
        Assert.Equal(JwkKeyType.Ec.Name, jwk.KeyType);
        Assert.Equal(curve, jwk.EcCurve);
        Assert.NotNull(jwk.EcX);
        Assert.NotNull(jwk.EcY);
        Assert.Null(jwk.PrivateKey);
        Assert.False(string.IsNullOrWhiteSpace(jwk.KeyId));
    }

    [Theory]
    [InlineData("P-256")]
    [InlineData("P-384")]
    [InlineData("P-521")]
    public void GenerateEc_PrivateKey_ShouldHavePrivateField(string curve)
    {
        var curveObj = JwkCurve.GetAll<JwkCurve>().First(c => c.Name == curve);
        var result = JwkGenerator.TryGenerateEc(curveObj, true, null, null, null, out var jwk, out var errorMessage);
        Assert.True(result, errorMessage);
        Assert.NotNull(jwk);
        Assert.Equal(JwkKeyType.Ec.Name, jwk.KeyType);
        Assert.Equal(curve, jwk.EcCurve);
        Assert.NotNull(jwk.PrivateKey);
    }

    [Fact]
    public void GenerateOct_ShouldHaveRequiredFields()
    {
        var result = JwkGenerator.TryGenerateOct(256, null, null, null, out var jwk, out var errorMessage);
        Assert.True(result, errorMessage);
        Assert.NotNull(jwk);
        Assert.Equal(JwkKeyType.Oct.Name, jwk.KeyType);
        Assert.NotNull(jwk.SymmetricKey);
        Assert.False(string.IsNullOrWhiteSpace(jwk.KeyId));
    }

    [Fact]
    public void TryComputeThumbprint_ShouldReturnValidThumbprint()
    {
        var result = JwkGenerator.TryGenerateRsa(2048, false, null, null, null, out var jwk, out var errorMessage);
        Assert.True(result, errorMessage);
        Assert.NotNull(jwk);
        var thumbResult = JwkGenerator.TryComputeThumbprint(jwk, out var thumb, out var error);
        Assert.True(thumbResult, error);
        Assert.False(string.IsNullOrWhiteSpace(thumb));
        Assert.Null(error);
    }

    [Fact]
    public void ComputeKid_ShouldBeUniqueForDifferentKeys()
    {
        var result1 = JwkGenerator.TryGenerateRsa(2048, false, null, null, null, out var jwk1, out var errorMessage1);
        var result2 = JwkGenerator.TryGenerateRsa(2048, false, null, null, null, out var jwk2, out var errorMessage2);
        Assert.True(result1, errorMessage1);
        Assert.True(result2, errorMessage2);
        Assert.NotNull(jwk1);
        Assert.NotNull(jwk2);
        Assert.NotEqual(jwk1.KeyId, jwk2.KeyId);
    }
}
