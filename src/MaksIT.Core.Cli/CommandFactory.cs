using System.CommandLine;


namespace MaksIT.Core.Cli;

/// <summary>
/// Builds the agent-facing command tree. No arguments runs the interactive menu.
/// </summary>
public static class CommandFactory {
  /// <summary>
  /// Creates the root command with secret, jwt, aes, totp, password, and guid subcommands.
  /// </summary>
  public static RootCommand CreateRootCommand() {
    var root = new RootCommand("Generate MaksIT.Core secrets. No arguments opens the interactive menu.") {
      CreateSecretCommand(),
      CreateJwtCommand(),
      CreateAesCommand(),
      CreateTotpCommand(),
      CreatePasswordCommand(),
      CreateGuidCommand()
    };

    root.SetAction(_ => {
      new Application().Run();
      return 0;
    });

    return root;
  }

  private static Command CreateSecretCommand() {
    var bytesOption = BytesOption();
    var command = new Command("secret", "Generate a Base64 secret (JWT signing key or password pepper)") {
      bytesOption
    };

    command.SetAction(parseResult =>
      CliActions.GenerateSecret(parseResult.GetValue(bytesOption)));

    return command;
  }

  private static Command CreateJwtCommand() {
    var jwt = new Command("jwt", "JWT signing secrets, tokens, and validation");
    jwt.Subcommands.Add(CreateJwtSecretCommand());
    jwt.Subcommands.Add(CreateJwtRefreshCommand());
    jwt.Subcommands.Add(CreateJwtGenerateCommand());
    jwt.Subcommands.Add(CreateJwtValidateCommand());
    return jwt;
  }

  private static Command CreateJwtSecretCommand() {
    var bytesOption = BytesOption();
    var command = new Command("secret", "Generate a JWT signing secret") {
      bytesOption
    };

    command.SetAction(parseResult =>
      CliActions.GenerateSecret(parseResult.GetValue(bytesOption)));

    return command;
  }

  private static Command CreateJwtRefreshCommand() {
    var command = new Command("refresh", "Generate an opaque refresh token");
    command.SetAction(_ => CliActions.GenerateRefreshToken());
    return command;
  }

  private static Command CreateJwtGenerateCommand() {
    var secretOption = RequiredString("--secret", "Signing secret");
    var issuerOption = RequiredString("--issuer", "Token issuer");
    var audienceOption = RequiredString("--audience", "Token audience");
    var expirationOption = new Option<int>("--expiration") {
      Description = "Lifetime in minutes",
      DefaultValueFactory = _ => 60
    };
    var userIdOption = new Option<string?>("--user-id") {
      Description = "Optional user id claim"
    };
    var usernameOption = new Option<string?>("--username") {
      Description = "Optional username claim"
    };
    var rolesOption = new Option<string?>("--roles") {
      Description = "Optional comma-separated roles"
    };
    var aclOption = new Option<string?>("--acl") {
      Description = "Optional comma-separated ACL entries"
    };

    var command = new Command("generate", "Sign an access JWT") {
      secretOption,
      issuerOption,
      audienceOption,
      expirationOption,
      userIdOption,
      usernameOption,
      rolesOption,
      aclOption
    };

    command.SetAction(parseResult =>
      CliActions.GenerateJwt(
        parseResult.GetValue(secretOption)!,
        parseResult.GetValue(issuerOption)!,
        parseResult.GetValue(audienceOption)!,
        parseResult.GetValue(expirationOption),
        parseResult.GetValue(userIdOption),
        parseResult.GetValue(usernameOption),
        parseResult.GetValue(rolesOption),
        parseResult.GetValue(aclOption)
      ));

    return command;
  }

  private static Command CreateJwtValidateCommand() {
    var secretOption = RequiredString("--secret", "Signing secret");
    var issuerOption = RequiredString("--issuer", "Token issuer");
    var audienceOption = RequiredString("--audience", "Token audience");
    var tokenOption = RequiredString("--token", "JWT to validate");

    var command = new Command("validate", "Validate an access JWT and print claims JSON") {
      secretOption,
      issuerOption,
      audienceOption,
      tokenOption
    };

    command.SetAction(parseResult =>
      CliActions.ValidateJwt(
        parseResult.GetValue(secretOption)!,
        parseResult.GetValue(issuerOption)!,
        parseResult.GetValue(audienceOption)!,
        parseResult.GetValue(tokenOption)!
      ));

    return command;
  }

  private static Command CreateAesCommand() {
    var aes = new Command("aes", "AES-GCM keys");
    var key = new Command("key", "Generate a Base64 AES-256 key");
    key.SetAction(_ => CliActions.GenerateAesKey());
    aes.Subcommands.Add(key);
    return aes;
  }

  private static Command CreateTotpCommand() {
    var totp = new Command("totp", "TOTP / 2FA secrets, recovery codes, and validation");
    totp.Subcommands.Add(CreateTotpSecretCommand());
    totp.Subcommands.Add(CreateTotpRecoveryCommand());
    totp.Subcommands.Add(CreateTotpLinkCommand());
    totp.Subcommands.Add(CreateTotpValidateCommand());
    return totp;
  }

  private static Command CreateTotpSecretCommand() {
    var command = new Command("secret", "Generate a Base32 TOTP shared secret");
    command.SetAction(_ => CliActions.GenerateTotpSecret());
    return command;
  }

  private static Command CreateTotpRecoveryCommand() {
    var countOption = new Option<int>("--count") {
      Description = "Number of recovery codes",
      DefaultValueFactory = _ => 10
    };

    var command = new Command("recovery", "Generate TOTP recovery codes (one per line)") {
      countOption
    };

    command.SetAction(parseResult =>
      CliActions.GenerateRecoveryCodes(parseResult.GetValue(countOption)));

    return command;
  }

  private static Command CreateTotpLinkCommand() {
    var labelOption = RequiredString("--label", "Authenticator label");
    var usernameOption = RequiredString("--username", "Account username");
    var secretOption = RequiredString("--secret", "Base32 TOTP secret");
    var issuerOption = RequiredString("--issuer", "Issuer name");

    var command = new Command("link", "Build an otpauth:// URI") {
      labelOption,
      usernameOption,
      secretOption,
      issuerOption
    };

    command.SetAction(parseResult =>
      CliActions.GenerateTotpAuthLink(
        parseResult.GetValue(labelOption)!,
        parseResult.GetValue(usernameOption)!,
        parseResult.GetValue(secretOption)!,
        parseResult.GetValue(issuerOption)!
      ));

    return command;
  }

  private static Command CreateTotpValidateCommand() {
    var secretOption = RequiredString("--secret", "Base32 TOTP secret");
    var codeOption = RequiredString("--code", "Six-digit TOTP code");
    var toleranceOption = new Option<int>("--tolerance") {
      Description = "Time-step windows to accept on each side",
      DefaultValueFactory = _ => 1
    };

    var command = new Command("validate", "Validate a TOTP code (prints valid/invalid)") {
      secretOption,
      codeOption,
      toleranceOption
    };

    command.SetAction(parseResult =>
      CliActions.ValidateTotp(
        parseResult.GetValue(secretOption)!,
        parseResult.GetValue(codeOption)!,
        parseResult.GetValue(toleranceOption)
      ));

    return command;
  }

  private static Command CreatePasswordCommand() {
    var password = new Command("password", "Password hashing");
    var pepperOption = RequiredString("--pepper", "Application pepper");
    var passwordOption = RequiredString("--password", "Password to hash");

    var hash = new Command("hash", "Create a salted hash (JSON with salt and hash)") {
      pepperOption,
      passwordOption
    };

    hash.SetAction(parseResult =>
      CliActions.HashPassword(
        parseResult.GetValue(pepperOption)!,
        parseResult.GetValue(passwordOption)!
      ));

    password.Subcommands.Add(hash);
    return password;
  }

  private static Command CreateGuidCommand() {
    var guid = new Command("guid", "COMB GUID generation");
    var typeOption = new Option<string>("--type") {
      Description = "PostgreSql or SqlServer",
      DefaultValueFactory = _ => "PostgreSql"
    };

    var comb = new Command("comb", "Generate a COMB GUID") {
      typeOption
    };

    comb.SetAction(parseResult =>
      CliActions.GenerateCombGuid(parseResult.GetValue(typeOption)));

    guid.Subcommands.Add(comb);
    return guid;
  }

  private static Option<int> BytesOption() =>
    new("--bytes") {
      Description = "Random key size in bytes",
      DefaultValueFactory = _ => 32
    };

  private static Option<string> RequiredString(string name, string description) =>
    new(name) {
      Description = description,
      Required = true
    };
}
