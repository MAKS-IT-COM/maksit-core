using MaksIT.Core.Security;
using Xunit;

namespace MaksIT.Core.Tests.Security {
  public class PasswordHasherTests {
    [Fact]
    public void CreateSaltedHash_ValidPassword_ReturnsSaltAndHash() {
      // Arrange
      var password = "SecurePassword123!";

      // Act
      var result = PasswordHasher.TryCreateSaltedHash(password, out var saltedHash, out var errorMessage);

      // Assert
      Assert.True(result);
      Assert.NotNull(saltedHash);
      Assert.False(string.IsNullOrWhiteSpace(saltedHash?.Salt));
      Assert.False(string.IsNullOrWhiteSpace(saltedHash?.Hash));
      Assert.Null(errorMessage);
    }

    [Fact]
    public void CreateSaltedHash_EmptyPassword_ReturnsSaltAndHash() {
      // Arrange
      var password = "";

      // Act
      var result = PasswordHasher.TryCreateSaltedHash(password, out var saltedHash, out var errorMessage);

      // Assert
      Assert.True(result);
      Assert.NotNull(saltedHash);
      Assert.False(string.IsNullOrWhiteSpace(saltedHash?.Salt));
      Assert.False(string.IsNullOrWhiteSpace(saltedHash?.Hash));
      Assert.Null(errorMessage);
    }

    [Fact]
    public void CreateSaltedHash_WhitespacePassword_ReturnsSaltAndHash() {
      // Arrange
      var password = "   ";

      // Act
      var result = PasswordHasher.TryCreateSaltedHash(password, out var saltedHash, out var errorMessage);

      // Assert
      Assert.True(result);
      Assert.NotNull(saltedHash);
      Assert.False(string.IsNullOrWhiteSpace(saltedHash?.Salt));
      Assert.False(string.IsNullOrWhiteSpace(saltedHash?.Hash));
      Assert.Null(errorMessage);
    }

    [Fact]
    public void ValidateHash_CorrectPassword_ReturnsTrue() {
      // Arrange
      var password = "SecurePassword123!";
      PasswordHasher.TryCreateSaltedHash(password, out var saltedHash, out var createErrorMessage);

      // Act
      var result = PasswordHasher.TryValidateHash(password, saltedHash?.Salt, saltedHash?.Hash, out var isValid, out var validateErrorMessage);

      // Assert
      Assert.True(result);
      Assert.True(isValid);
      Assert.Null(validateErrorMessage);
    }

    [Fact]
    public void ValidateHash_IncorrectPassword_ReturnsFalse() {
      // Arrange
      var password = "SecurePassword123!";
      var wrongPassword = "WrongPassword456!";
      PasswordHasher.TryCreateSaltedHash(password, out var saltedHash, out var createErrorMessage);

      // Act
      var result = PasswordHasher.TryValidateHash(wrongPassword, saltedHash?.Salt, saltedHash?.Hash, out var isValid, out var validateErrorMessage);

      // Assert
      Assert.True(result);
      Assert.False(isValid);
      Assert.Null(validateErrorMessage);
    }

    [Fact]
    public void ValidateHash_EmptyStoredHash_ReturnsFalse() {
      // Arrange
      var password = "SecurePassword123!";
      var storedHash = "";
      var salt = ""; // Assuming empty salt

      // Act
      var result = PasswordHasher.TryValidateHash(password, salt, storedHash, out var isValid, out var errorMessage);

      // Assert
      Assert.True(result);
      Assert.False(isValid);
      Assert.Null(errorMessage);
    }

    [Fact]
    public void ValidateHash_WhitespaceStoredHash_ReturnsFalse() {
      // Arrange
      var password = "SecurePassword123!";
      var storedHash = "   ";
      var salt = "   ";

      // Act
      var result = PasswordHasher.TryValidateHash(password, salt, storedHash, out var isValid, out var errorMessage);

      // Assert
      Assert.True(result);
      Assert.False(isValid);
      Assert.Null(errorMessage);
    }

    [Fact]
    public void ValidateHash_InvalidStoredHash_ReturnsFalse() {
      // Arrange
      var password = "SecurePassword123!";
      var invalidStoredHash = "InvalidHashValue";
      var invalidSalt = "InvalidSaltValue";

      // Act
      var result = PasswordHasher.TryValidateHash(password, invalidSalt, invalidStoredHash, out var isValid, out var errorMessage);

      // Assert
      Assert.True(result);
      Assert.False(isValid);
      Assert.Null(errorMessage);
    }

    [Fact]
    public void CreateSaltedHash_SamePasswordDifferentHashes() {
      // Arrange
      var password = "SecurePassword123!";

      // Act
      PasswordHasher.TryCreateSaltedHash(password, out var hashResult1, out var errorMessage1);
      PasswordHasher.TryCreateSaltedHash(password, out var hashResult2, out var errorMessage2);

      // Assert
      Assert.NotEqual(hashResult1?.Hash, hashResult2?.Hash);
    }

    [Fact]
    public void ValidateHash_ModifiedStoredHash_ReturnsFalse() {
      // Arrange
      var password = "SecurePassword123!";
      PasswordHasher.TryCreateSaltedHash(password, out var hashResult, out var createErrorMessage);

      // Modify the stored hash
      var hashChars = hashResult?.Hash.ToCharArray();
      if (hashChars != null) {
        hashChars[10] = (hashChars[10] == 'A') ? 'B' : 'A'; // Change one character
      }
      var modifiedHash = new string(hashChars);

      // Act
      var result = PasswordHasher.TryValidateHash(password, hashResult?.Salt, modifiedHash, out var isValid, out var validateErrorMessage);

      // Assert
      Assert.True(result);
      Assert.False(isValid);
      Assert.Null(validateErrorMessage);
    }

    [Fact]
    public void CreateSaltedHash_DifferentPasswordsHaveDifferentHashes() {
      // Arrange
      var password1 = "PasswordOne";
      var password2 = "PasswordTwo";

      // Act
      PasswordHasher.TryCreateSaltedHash(password1, out var hashResult1, out var errorMessage1);
      PasswordHasher.TryCreateSaltedHash(password2, out var hashResult2, out var errorMessage2);

      // Assert
      Assert.NotEqual(hashResult1?.Hash, hashResult2?.Hash);
    }

    [Fact]
    public void CreateSaltedHash_ReturnsBase64StringsOfExpectedLength() {
      // Arrange
      var password = "SecurePassword123!";

      // Act
      var result = PasswordHasher.TryCreateSaltedHash(password, out var saltedHash, out var errorMessage);

      // Assert
      // For 16 bytes salt, Base64 length is 24 characters
      Assert.Equal(24, saltedHash?.Salt.Length);
      // For 32 bytes hash, Base64 length is 44 characters
      Assert.Equal(44, saltedHash?.Hash.Length);
    }
  }
}
