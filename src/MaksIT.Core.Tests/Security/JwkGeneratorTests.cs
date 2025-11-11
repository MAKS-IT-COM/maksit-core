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
        Assert.Equal(JwkKeyType.Rsa.Name, jwk.Kty);
        Assert.NotNull(jwk.N);
        Assert.NotNull(jwk.E);
        Assert.Null(jwk.D);
        Assert.Null(jwk.P);
        Assert.Null(jwk.Q);
        Assert.Null(jwk.DP);
        Assert.Null(jwk.DQ);
        Assert.Null(jwk.QI);
        Assert.False(string.IsNullOrWhiteSpace(jwk.Kid));
    }

    [Fact]
    public void GenerateRsa_PrivateKey_ShouldHavePrivateFields()
    {
        var result = JwkGenerator.TryGenerateRsa(2048, true, null, null, null, out var jwk, out var errorMessage);
        Assert.True(result, errorMessage);
        Assert.NotNull(jwk);
        Assert.Equal(JwkKeyType.Rsa.Name, jwk.Kty);
        Assert.NotNull(jwk.D);
        Assert.NotNull(jwk.P);
        Assert.NotNull(jwk.Q);
        Assert.NotNull(jwk.DP);
        Assert.NotNull(jwk.DQ);
        Assert.NotNull(jwk.QI);
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
        Assert.Equal(JwkKeyType.Ec.Name, jwk.Kty);
        Assert.Equal(curve, jwk.Crv);
        Assert.NotNull(jwk.X);
        Assert.NotNull(jwk.Y);
        Assert.Null(jwk.D_EC);
        Assert.False(string.IsNullOrWhiteSpace(jwk.Kid));
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
        Assert.Equal(JwkKeyType.Ec.Name, jwk.Kty);
        Assert.Equal(curve, jwk.Crv);
        Assert.NotNull(jwk.D_EC);
    }

    [Fact]
    public void GenerateOct_ShouldHaveRequiredFields()
    {
        var result = JwkGenerator.TryGenerateOct(256, null, null, null, out var jwk, out var errorMessage);
        Assert.True(result, errorMessage);
        Assert.NotNull(jwk);
        Assert.Equal(JwkKeyType.Oct.Name, jwk.Kty);
        Assert.NotNull(jwk.K);
        Assert.False(string.IsNullOrWhiteSpace(jwk.Kid));
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
        Assert.NotEqual(jwk1.Kid, jwk2.Kid);
    }
}
