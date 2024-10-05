using System.Security.Cryptography;


namespace MaksIT.Core.Security;

public static class TotpGenerator {
  private const int Timestep = 30; // Time step in seconds (standard is 30 seconds)
  private const int TotpDigits = 6; // Standard TOTP length is 6 digits

  public static bool Validate(string totpCode, string base32Secret, int timeTolerance = 1) {
    // Convert the Base32 encoded secret to a byte array
    byte[] secretBytes = Base32Encoder.Decode(base32Secret);

    // Get current timestamp
    long timeStepWindow = GetCurrentTimeStepNumber();

    // Validate the TOTP code against the valid time windows (current and around it)
    for (int i = -timeTolerance; i <= timeTolerance; i++) {
      var generatedTotp = Generate(secretBytes, timeStepWindow + i);
      if (generatedTotp == totpCode) {
        return true;
      }
    }

    return false;
  }

  public static string Generate(string base32Secret, long timestep) {
    // Convert the Base32 encoded secret to a byte array
    byte[] secretBytes = Base32Encoder.Decode(base32Secret);
    return Generate(secretBytes, timestep);
  }

  private static string Generate(byte[] secretBytes, long timestep) {
    // Convert the time step to byte array (8-byte big-endian)
    byte[] timestepBytes = BitConverter.GetBytes(timestep);
    if (BitConverter.IsLittleEndian) {
      Array.Reverse(timestepBytes);
    }

    // Generate HMAC-SHA1 hash based on the secret and the time step
    using (var hmac = new HMACSHA1(secretBytes)) {
      byte[] hash = hmac.ComputeHash(timestepBytes);

      // Extract a 4-byte dynamic binary code from the hash
      int offset = hash[hash.Length - 1] & 0x0F;
      int binaryCode = (hash[offset] & 0x7F) << 24
                       | (hash[offset + 1] & 0xFF) << 16
                       | (hash[offset + 2] & 0xFF) << 8
                       | (hash[offset + 3] & 0xFF);

      // Reduce to the desired number of digits
      int totp = binaryCode % (int)Math.Pow(10, TotpDigits);
      return totp.ToString(new string('0', TotpDigits)); // Ensure leading zeroes
    }
  }

  public static long GetCurrentTimeStepNumber() {
    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    return unixTimestamp / Timestep;
  }

  public static string GenerateSecret() {
    // Example of generating a 32-character base32 secret for TOTP
    var random = new byte[20];
    using (var rng = RandomNumberGenerator.Create()) {
      rng.GetBytes(random);
    }

    return Base32Encoder.Encode(random); // You can use a Base32 encoder to generate the secret.
  }

  public static List<string> GenerateRecoveryCodes(int defaultCodeCount = 6) {
    var recoveryCodes = new List<string>();

    for (int i = 0; i < defaultCodeCount; i++) {
      var code = Guid.NewGuid().ToString("N").Substring(0, 8); // Generate an 8-character code
      var formattedCode = $"{code.Substring(0, 4)}-{code.Substring(4, 4)}"; // Format as XXXX-XXXX
      recoveryCodes.Add(formattedCode);
    }

    return recoveryCodes;
  }

  public static string GenerateTotpAuthLink(string label, string username, string twoFactoSharedKey, string issuer, string? algorithm = null, int? digits = null, int? period = null) {

    var queryParams = new List<string> {
        $"secret={Uri.EscapeDataString(twoFactoSharedKey)}",
        $"issuer={Uri.EscapeDataString(issuer)}"
    };

    if (algorithm != null) {
      queryParams.Add($"algorithm={Uri.EscapeDataString(algorithm)}");
    }

    if (digits != null) {
      queryParams.Add($"digits={digits}");
    }

    if (period != null) {
      queryParams.Add($"period={period}");
    }

    var queryString = string.Join("&", queryParams);
    var authLink = $"otpauth://totp/{Uri.EscapeDataString(label)}:{Uri.EscapeDataString(username)}?{queryString}";

    return authLink;
  }
}

