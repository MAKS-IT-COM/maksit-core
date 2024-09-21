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

  public static (string Salt, string Hash) CreateSaltedHash(string value) {
    var saltBytes = CreateSaltBytes();
    var hash = CreateHash(value, saltBytes);
    var salt = Convert.ToBase64String(saltBytes);

    return (salt, hash);
  }

  public static bool ValidateHash(string value, string salt, string hash) {
    var saltBytes = Convert.FromBase64String(salt);
    var hashToCompare = CreateHash(value, saltBytes);

    return CryptographicOperations.FixedTimeEquals(
        Convert.FromBase64String(hashToCompare),
        Convert.FromBase64String(hash)
    );
  }
}