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

  private static string CreateHash(string value, byte[] saltBytes) {
    var valueBytes = KeyDerivation.Pbkdf2(
        password: value,
        salt: saltBytes,
        prf: KeyDerivationPrf.HMACSHA512,
        iterationCount: 100_000, // Increased iteration count
        numBytesRequested: 256 / 8);

    return Convert.ToBase64String(valueBytes);
  }

  public static bool TryCreateSaltedHash(string value, out (string Salt, string Hash)? saltedHash, out string? errorMessage) {
    try {
      var saltBytes = CreateSaltBytes();
      var hash = CreateHash(value, saltBytes);
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

  public static bool TryValidateHash(string value, string salt, string hash, out bool isValid, out string? errorMessage) {
    try {
      var saltBytes = Convert.FromBase64String(salt);
      var hashToCompare = CreateHash(value, saltBytes);

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
