using MaksIT.Core.Cli;
using MaksIT.Core.Comb;
using MaksIT.Core.Security.JWT;


namespace MaksIT.Core.Cli.Tests;

public class SecretOperationsTests {
  [Fact]
  public void GenerateSecret_ReturnsUniqueNonEmptyValues() {
    var secret1 = SecretOperations.GenerateSecret();
    var secret2 = SecretOperations.GenerateSecret();

    Assert.False(string.IsNullOrWhiteSpace(secret1));
    Assert.False(string.IsNullOrWhiteSpace(secret2));
    Assert.NotEqual(secret1, secret2);
  }

  [Fact]
  public void GenerateAesKey_ReturnsNonEmptyValue() =>
    Assert.False(string.IsNullOrWhiteSpace(SecretOperations.GenerateAesKey()));

  [Fact]
  public void GenerateCombGuid_PostgreSql_ReturnsNonEmptyGuid() {
    var guid = SecretOperations.GenerateCombGuid(CombGuidType.PostgreSql);

    Assert.NotEqual(Guid.Empty, guid);
  }

  [Fact]
  public void TryGenerateJwt_ThenValidate_Succeeds() {
    var secret = SecretOperations.GenerateSecret();
    var request = new JWTTokenGenerateRequest {
      Secret = secret,
      Issuer = "cli-tests",
      Audience = "cli-tests",
      Expiration = 5,
      Username = "tester"
    };

    var generated = SecretOperations.TryGenerateJwt(request, out var token, out var generateError);

    Assert.True(generated);
    Assert.False(string.IsNullOrWhiteSpace(token));
    Assert.Null(generateError);

    var validated = SecretOperations.TryValidateJwt(
      secret,
      request.Issuer,
      request.Audience,
      token!,
      out var claims,
      out var validateError
    );

    Assert.True(validated);
    Assert.Equal("tester", claims?.Username);
    Assert.Null(validateError);
  }
}
