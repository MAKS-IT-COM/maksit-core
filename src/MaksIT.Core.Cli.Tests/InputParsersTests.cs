using MaksIT.Core.Cli;
using MaksIT.Core.Comb;


namespace MaksIT.Core.Cli.Tests;

public class InputParsersTests {
  [Fact]
  public void TryParsePositiveInt_Blank_ReturnsDefault() {
    var result = InputParsers.TryParsePositiveInt("  ", 32, out var value, out var errorMessage);

    Assert.True(result);
    Assert.Equal(32, value);
    Assert.Null(errorMessage);
  }

  [Fact]
  public void TryParsePositiveInt_ValidNumber_ReturnsValue() {
    var result = InputParsers.TryParsePositiveInt("64", 32, out var value, out var errorMessage);

    Assert.True(result);
    Assert.Equal(64, value);
    Assert.Null(errorMessage);
  }

  [Fact]
  public void TryParsePositiveInt_Invalid_ReturnsError() {
    var result = InputParsers.TryParsePositiveInt("abc", 32, out var value, out var errorMessage);

    Assert.False(result);
    Assert.Equal(0, value);
    Assert.NotNull(errorMessage);
  }

  [Fact]
  public void TryParseCombGuidType_Blank_ReturnsPostgreSql() {
    var result = InputParsers.TryParseCombGuidType(null, out var type, out var errorMessage);

    Assert.True(result);
    Assert.Equal(CombGuidType.PostgreSql, type);
    Assert.Null(errorMessage);
  }

  [Fact]
  public void TryParseCombGuidType_SqlServer_ParsesIgnoreCase() {
    var result = InputParsers.TryParseCombGuidType("sqlserver", out var type, out var errorMessage);

    Assert.True(result);
    Assert.Equal(CombGuidType.SqlServer, type);
    Assert.Null(errorMessage);
  }

  [Fact]
  public void ParseOptionalList_SplitsAndTrims() {
    var items = InputParsers.ParseOptionalList(" Admin, User , ");

    Assert.NotNull(items);
    Assert.Equal(["Admin", "User"], items);
  }
}
