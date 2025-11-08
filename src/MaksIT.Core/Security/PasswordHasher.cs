using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace MaksIT.Core.Security;

public static class PasswordHasher {
  private static byte[] CreateSaltBytes() {
    byte[] randomBytes = new byte[16];
    using (var generator = RandomNumberGenerator.Create()) {
      generator.GetBytes(randomBytes);
    }
    return randomBytes;
  }

  private static string CreateHash(string value, byte[] saltBytes, string pepper) {
    // Combine password and pepper
    var valueWithPepper = value + pepper;
    var valueBytes = KeyDerivation.Pbkdf2(
        password: valueWithPepper,
        salt: saltBytes,
        prf: KeyDerivationPrf.HMACSHA512,
        iterationCount: 100_000, // Increased iteration count
        numBytesRequested: 256 / 8);

    return Convert.ToBase64String(valueBytes);
  }

  public static bool TryCreateSaltedHash(
    string value,
    string pepper,
    [NotNullWhen(true)] out (string Salt, string Hash)? saltedHash,
    [NotNullWhen(false)] out string? errorMessage
  ) {
    try {
      var saltBytes = CreateSaltBytes();
      var hash = CreateHash(value, saltBytes, pepper);
      var salt = Convert.ToBase64String(saltBytes);

      saltedHash = (salt, hash);
      errorMessage = null;
      return true;
    }
    catch (Exception ex) {
      saltedHash = null;
      errorMessage = ex.Message;
      return false;
    }
  }

  public static bool TryValidateHash(
    string value,
    string salt,
    string hash,
    string pepper,
    [NotNullWhen(true)] out bool isValid,
    [NotNullWhen(false)] out string? errorMessage
  ) {
    try {
      var saltBytes = Convert.FromBase64String(salt);
      var hashToCompare = CreateHash(value, saltBytes, pepper);

      isValid = CryptographicOperations.FixedTimeEquals(
          Convert.FromBase64String(hashToCompare),
          Convert.FromBase64String(hash)
      );

      errorMessage = null;
      return true;
    }
    catch (Exception ex) {
      isValid = false;
      errorMessage = ex.Message;
      return false;
    }
  }
}
