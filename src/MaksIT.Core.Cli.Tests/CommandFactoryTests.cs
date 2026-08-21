using MaksIT.Core.Cli;


namespace MaksIT.Core.Cli.Tests;

public class CommandFactoryTests {
  [Fact]
  public void Secret_HasNoParseErrors() {
    var result = CommandFactory.CreateRootCommand().Parse(["secret"]);

    Assert.Empty(result.Errors);
  }

  [Fact]
  public void JwtGenerate_MissingSecret_HasParseError() {
    var result = CommandFactory.CreateRootCommand().Parse([
      "jwt", "generate", "--issuer", "i", "--audience", "a"
    ]);

    Assert.NotEmpty(result.Errors);
  }

  [Fact]
  public void JwtGenerate_RequiredOptions_HasNoParseErrors() {
    var result = CommandFactory.CreateRootCommand().Parse([
      "jwt", "generate",
      "--secret", "s",
      "--issuer", "i",
      "--audience", "a"
    ]);

    Assert.Empty(result.Errors);
  }

  [Fact]
  public void TotpValidate_RequiredOptions_HasNoParseErrors() {
    var result = CommandFactory.CreateRootCommand().Parse([
      "totp", "validate", "--secret", "s", "--code", "123456"
    ]);

    Assert.Empty(result.Errors);
  }
}
