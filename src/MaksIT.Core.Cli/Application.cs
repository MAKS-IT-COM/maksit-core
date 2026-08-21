using System.Text;
using System.Reflection;
using MaksIT.Core.Comb;
using MaksIT.Core.Extensions;
using MaksIT.Core.Security.JWT;


namespace MaksIT.Core.Cli;

/// <summary>
/// Interactive numbered menu for generating MaksIT.Core secrets.
/// </summary>
public sealed class Application {
  /// <summary>
  /// Runs the main menu until the user exits.
  /// </summary>
  public void Run() {
    Console.OutputEncoding = Encoding.UTF8;
    var version = typeof(Application).Assembly
      .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
      .InformationalVersion?
      .Split('+')[0]
      ?? "0.0.0";

    while (true) {
      Console.WriteLine($"MaksIT.Core.Cli v{version}");
      Console.WriteLine("© Maksym Sadovnychyy (MAKS-IT) 2026");
      Console.WriteLine();
      Console.WriteLine("1. Generate secret (JWT / pepper)");
      Console.WriteLine("2. JWT");
      Console.WriteLine("3. AES-GCM key");
      Console.WriteLine("4. TOTP / 2FA");
      Console.WriteLine("5. Password hash");
      Console.WriteLine("6. COMB GUID");
      Console.WriteLine("0. Exit");
      Console.Write("Enter your choice: ");

      var choice = Console.ReadLine();
      try {
        switch (choice) {
          case "1":
            GenerateSecret();
            Pause();
            break;
          case "2":
            RunJwtMenu();
            break;
          case "3":
            WriteLabeled("AES-256 key", SecretOperations.GenerateAesKey());
            Pause();
            break;
          case "4":
            RunTotpMenu();
            break;
          case "5":
            HashPassword();
            Pause();
            break;
          case "6":
            GenerateCombGuid();
            Pause();
            break;
          case "0":
            return;
          default:
            Console.WriteLine("Invalid option.");
            break;
        }
      }
      catch (Exception ex) {
        Console.WriteLine($"Error: {ex.Message}");
        Pause();
      }

      Console.WriteLine();
    }
  }

  private static void RunJwtMenu() {
    while (true) {
      Console.WriteLine();
      Console.WriteLine("JWT");
      Console.WriteLine("1. Generate signing secret");
      Console.WriteLine("2. Generate refresh token");
      Console.WriteLine("3. Generate access token");
      Console.WriteLine("4. Validate token");
      Console.WriteLine("0. Back");
      Console.Write("Enter your choice: ");

      var choice = Console.ReadLine();
      try {
        switch (choice) {
          case "1":
            GenerateSecret();
            Pause();
            break;
          case "2":
            WriteLabeled("Refresh token", SecretOperations.GenerateRefreshToken());
            Pause();
            break;
          case "3":
            GenerateAccessToken();
            Pause();
            break;
          case "4":
            ValidateAccessToken();
            Pause();
            break;
          case "0":
            return;
          default:
            Console.WriteLine("Invalid option.");
            break;
        }
      }
      catch (Exception ex) {
        Console.WriteLine($"Error: {ex.Message}");
        Pause();
      }
    }
  }

  private static void RunTotpMenu() {
    while (true) {
      Console.WriteLine();
      Console.WriteLine("TOTP / 2FA");
      Console.WriteLine("1. Generate secret");
      Console.WriteLine("2. Generate recovery codes");
      Console.WriteLine("3. Generate otpauth link");
      Console.WriteLine("4. Validate code");
      Console.WriteLine("0. Back");
      Console.Write("Enter your choice: ");

      var choice = Console.ReadLine();
      try {
        switch (choice) {
          case "1":
            if (!SecretOperations.TryGenerateTotpSecret(out var secret, out var secretError))
              throw new InvalidOperationException(secretError);

            WriteLabeled("TOTP secret", secret);
            Pause();
            break;
          case "2":
            GenerateRecoveryCodes();
            Pause();
            break;
          case "3":
            GenerateTotpAuthLink();
            Pause();
            break;
          case "4":
            ValidateTotp();
            Pause();
            break;
          case "0":
            return;
          default:
            Console.WriteLine("Invalid option.");
            break;
        }
      }
      catch (Exception ex) {
        Console.WriteLine($"Error: {ex.Message}");
        Pause();
      }
    }
  }

  private static void GenerateSecret() {
    var bytes = ReadPositiveInt("Key size in bytes", 32);
    WriteLabeled("Secret", SecretOperations.GenerateSecret(bytes));
  }

  private static void GenerateAccessToken() {
    var request = new JWTTokenGenerateRequest {
      Secret = ReadRequired("Secret"),
      Issuer = ReadRequired("Issuer"),
      Audience = ReadRequired("Audience"),
      Expiration = ReadPositiveInt("Expiration (minutes)", 60),
      UserId = ReadOptional("User id"),
      Username = ReadOptional("Username"),
      Roles = InputParsers.ParseOptionalList(ReadOptional("Roles (comma-separated)")),
      AclEntries = InputParsers.ParseOptionalList(ReadOptional("ACL entries (comma-separated)"))
    };

    if (!SecretOperations.TryGenerateJwt(request, out var token, out var errorMessage))
      throw new InvalidOperationException(errorMessage);

    WriteLabeled("Access token", token);
  }

  private static void ValidateAccessToken() {
    var secret = ReadRequired("Secret");
    var issuer = ReadRequired("Issuer");
    var audience = ReadRequired("Audience");
    var token = ReadRequired("Token");

    if (!SecretOperations.TryValidateJwt(secret, issuer, audience, token, out var claims, out var errorMessage))
      throw new InvalidOperationException(errorMessage);

    WriteLabeled("Claims", claims.ToJson());
  }

  private static void GenerateRecoveryCodes() {
    var count = ReadPositiveInt("Number of codes", 10);
    if (!SecretOperations.TryGenerateRecoveryCodes(count, out var codes, out var errorMessage))
      throw new InvalidOperationException(errorMessage);

    Console.WriteLine("Recovery codes:");
    foreach (var code in codes)
      Console.WriteLine(code);
  }

  private static void GenerateTotpAuthLink() {
    var label = ReadRequired("Label");
    var username = ReadRequired("Username");
    var secret = ReadRequired("TOTP secret");
    var issuer = ReadRequired("Issuer");

    if (!SecretOperations.TryGenerateTotpAuthLink(label, username, secret, issuer, out var authLink, out var errorMessage))
      throw new InvalidOperationException(errorMessage);

    WriteLabeled("otpauth link", authLink);
  }

  private static void ValidateTotp() {
    var secret = ReadRequired("TOTP secret");
    var code = ReadRequired("Code");
    var tolerance = ReadPositiveInt("Time-step tolerance", 1);

    if (!SecretOperations.TryValidateTotp(code, secret, tolerance, out var isValid, out var errorMessage))
      throw new InvalidOperationException(errorMessage);

    Console.WriteLine(isValid ? "Valid." : "Invalid.");
  }

  private static void HashPassword() {
    var pepper = ReadRequired("Pepper");
    var password = ReadSecret("Password");
    if (string.IsNullOrEmpty(password))
      throw new InvalidOperationException("Password is required.");

    if (!SecretOperations.TryHashPassword(password, pepper, out var saltedHash, out var errorMessage))
      throw new InvalidOperationException(errorMessage);

    Console.WriteLine($"Salt: {saltedHash.Value.Salt}");
    Console.WriteLine($"Hash: {saltedHash.Value.Hash}");
  }

  private static void GenerateCombGuid() {
    Console.Write("COMB type (PostgreSql/SqlServer) [PostgreSql]: ");
    if (!InputParsers.TryParseCombGuidType(Console.ReadLine(), out var type, out var errorMessage))
      throw new InvalidOperationException(errorMessage);

    WriteLabeled($"COMB GUID ({type})", SecretOperations.GenerateCombGuid(type).ToString());
  }

  private static int ReadPositiveInt(string prompt, int defaultValue) {
    Console.Write($"{prompt} [{defaultValue}]: ");
    if (!InputParsers.TryParsePositiveInt(Console.ReadLine(), defaultValue, out var value, out var errorMessage))
      throw new InvalidOperationException(errorMessage);

    return value;
  }

  private static string ReadRequired(string prompt) {
    Console.Write($"{prompt}: ");
    var value = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(value))
      throw new InvalidOperationException($"{prompt} is required.");

    return value.Trim();
  }

  private static string? ReadOptional(string prompt) {
    Console.Write($"{prompt}: ");
    var value = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(value))
      return null;

    return value.Trim();
  }

  private static string ReadSecret(string prompt) {
    Console.Write($"{prompt}: ");
    var builder = new StringBuilder();
    while (true) {
      var key = Console.ReadKey(intercept: true);
      if (key.Key == ConsoleKey.Enter) {
        Console.WriteLine();
        return builder.ToString();
      }

      if (key.Key == ConsoleKey.Backspace) {
        if (builder.Length > 0)
          builder.Length--;

        continue;
      }

      if (!char.IsControl(key.KeyChar))
        builder.Append(key.KeyChar);
    }
  }

  private static void WriteLabeled(string label, string value) {
    Console.WriteLine($"{label}:");
    Console.WriteLine(value);
  }

  private static void Pause() {
    Console.WriteLine();
    Console.Write("Press Enter to continue...");
    Console.ReadLine();
  }
}
