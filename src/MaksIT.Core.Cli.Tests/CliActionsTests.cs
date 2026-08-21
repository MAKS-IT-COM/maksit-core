using MaksIT.Core.Cli;


namespace MaksIT.Core.Cli.Tests;

public class CliActionsTests {
  [Fact]
  public void GenerateSecret_InvalidBytes_ReturnsOne() {
    var exit = CliActions.GenerateSecret(0);

    Assert.Equal(1, exit);
  }

  [Fact]
  public void GenerateCombGuid_InvalidType_ReturnsOne() {
    var exit = CliActions.GenerateCombGuid("rsa");

    Assert.Equal(1, exit);
  }

  [Fact]
  public void GenerateCombGuid_PostgreSql_ReturnsZero() {
    var exit = CliActions.GenerateCombGuid(null);

    Assert.Equal(0, exit);
  }

  [Fact]
  public void HashPassword_EmptyPassword_ReturnsOne() {
    var exit = CliActions.HashPassword("pepper", "");

    Assert.Equal(1, exit);
  }

  [Fact]
  public void GenerateAesKey_ReturnsZero() =>
    Assert.Equal(0, CliActions.GenerateAesKey());
}
