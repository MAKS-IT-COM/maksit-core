using MaksIT.Core.Security;
using Xunit;

namespace MaksIT.Core.Tests.Security {
  public class PasswordHasherTests {
    [Fact]
    public void CreateSaltedHash_ValidPassword_ReturnsSaltAndHash() {
      // Arrange
      var password = "SecurePassword123!";

      // Act
      var result = PasswordHasher.CreateSaltedHash(password);

      // Assert
      Assert.False(string.IsNullOrWhiteSpace(result.Salt));
      Assert.False(string.IsNullOrWhiteSpace(result.Hash));
    }

    [Fact]
    public void CreateSaltedHash_EmptyPassword_ReturnsSaltAndHash() {
      // Arrange
      var password = "";

      // Act
      var result = PasswordHasher.CreateSaltedHash(password);

      // Assert
      Assert.False(string.IsNullOrWhiteSpace(result.Salt));
      Assert.False(string.IsNullOrWhiteSpace(result.Hash));
    }

    [Fact]
    public void CreateSaltedHash_WhitespacePassword_ReturnsSaltAndHash() {
      // Arrange
      var password = "   ";

      // Act
      var result = PasswordHasher.CreateSaltedHash(password);

      // Assert
      Assert.False(string.IsNullOrWhiteSpace(result.Salt));
      Assert.False(string.IsNullOrWhiteSpace(result.Hash));
    }

    [Fact]
    public void ValidateHash_CorrectPassword_ReturnsTrue() {
      // Arrange
      var password = "SecurePassword123!";
      var hashResult = PasswordHasher.CreateSaltedHash(password);

      // Act
      var verifyResult = PasswordHasher.ValidateHash(password, hashResult.Salt, hashResult.Hash);

      // Assert
      Assert.True(verifyResult);
    }

    [Fact]
    public void ValidateHash_IncorrectPassword_ReturnsFalse() {
      // Arrange
      var password = "SecurePassword123!";
      var wrongPassword = "WrongPassword456!";
      var hashResult = PasswordHasher.CreateSaltedHash(password);

      // Act
      var verifyResult = PasswordHasher.ValidateHash(wrongPassword, hashResult.Salt, hashResult.Hash);

      // Assert
      Assert.False(verifyResult);
    }

    [Fact]
    public void ValidateHash_EmptyStoredHash_ReturnsFalse() {
      // Arrange
      var password = "SecurePassword123!";
      var storedHash = "";
      var salt = ""; // Assuming empty salt

      // Act
      var verifyResult = PasswordHasher.ValidateHash(password, salt, storedHash);

      // Assert
      Assert.False(verifyResult);
    }

    [Fact]
    public void ValidateHash_WhitespaceStoredHash_ReturnsFalse() {
      // Arrange
      var password = "SecurePassword123!";
      var storedHash = "   ";
      var salt = "   ";

      // Act
      var verifyResult = PasswordHasher.ValidateHash(password, salt, storedHash);

      // Assert
      Assert.False(verifyResult);
    }

    [Fact]
    public void ValidateHash_InvalidStoredHash_ReturnsFalse() {
      // Arrange
      var password = "SecurePassword123!";
      var invalidStoredHash = "InvalidHashValue";
      var invalidSalt = "InvalidSaltValue";

      // Act
      var verifyResult = PasswordHasher.ValidateHash(password, invalidSalt, invalidStoredHash);

      // Assert
      Assert.False(verifyResult);
    }

    [Fact]
    public void CreateSaltedHash_SamePasswordDifferentHashes() {
      // Arrange
      var password = "SecurePassword123!";

      // Act
      var hashResult1 = PasswordHasher.CreateSaltedHash(password);
      var hashResult2 = PasswordHasher.CreateSaltedHash(password);

      // Assert
      Assert.NotEqual(hashResult1.Hash, hashResult2.Hash);
    }

    [Fact]
    public void ValidateHash_ModifiedStoredHash_ReturnsFalse() {
      // Arrange
      var password = "SecurePassword123!";
      var hashResult = PasswordHasher.CreateSaltedHash(password);

      // Modify the stored hash
      var hashChars = hashResult.Hash.ToCharArray();
      hashChars[10] = (hashChars[10] == 'A') ? 'B' : 'A'; // Change one character
      var modifiedHash = new string(hashChars);

      // Act
      var verifyResult = PasswordHasher.ValidateHash(password, hashResult.Salt, modifiedHash);

      // Assert
      Assert.False(verifyResult);
    }

    [Fact]
    public void CreateSaltedHash_DifferentPasswordsHaveDifferentHashes() {
      // Arrange
      var password1 = "PasswordOne";
      var password2 = "PasswordTwo";

      // Act
      var hashResult1 = PasswordHasher.CreateSaltedHash(password1);
      var hashResult2 = PasswordHasher.CreateSaltedHash(password2);

      // Assert
      Assert.NotEqual(hashResult1.Hash, hashResult2.Hash);
    }

    [Fact]
    public void CreateSaltedHash_ReturnsBase64StringsOfExpectedLength() {
      // Arrange
      var password = "SecurePassword123!";

      // Act
      var result = PasswordHasher.CreateSaltedHash(password);

      // Assert
      // For 16 bytes salt, Base64 length is 24 characters
      Assert.Equal(24, result.Salt.Length);
      // For 32 bytes hash, Base64 length is 44 characters
      Assert.Equal(44, result.Hash.Length);
    }
  }
}
