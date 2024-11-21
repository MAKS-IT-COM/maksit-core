using MaksIT.Core.Security;

namespace MaksIT.Core.Tests.Security {
  public class AESGCMUtilityTests {
    [Fact]
    public void EncryptData_ValidData_ReturnsEncryptedData() {
      // Arrange
      var data = System.Text.Encoding.UTF8.GetBytes("Sensitive data");
      var key = AESGCMUtility.GenerateKeyBase64();

      // Act
      var result = AESGCMUtility.TryEncryptData(data, key, out var encryptedData, out var errorMessage);

      // Assert
      Assert.True(result);
      Assert.NotNull(encryptedData);
      Assert.Null(errorMessage);
    }

    [Fact]
    public void EncryptData_InvalidKey_ReturnsError() {
      // Arrange
      var data = System.Text.Encoding.UTF8.GetBytes("Sensitive data");
      var invalidKey = "InvalidBase64Key";

      // Act
      var result = AESGCMUtility.TryEncryptData(data, invalidKey, out var encryptedData, out var errorMessage);

      // Assert
      Assert.False(result);
      Assert.Null(encryptedData);
      Assert.NotNull(errorMessage);
    }

    [Fact]
    public void DecryptData_ValidData_ReturnsDecryptedData() {
      // Arrange
      var data = System.Text.Encoding.UTF8.GetBytes("Sensitive data");
      var key = AESGCMUtility.GenerateKeyBase64();
      AESGCMUtility.TryEncryptData(data, key, out var encryptedData, out var encryptErrorMessage);

      // Act
      var result = AESGCMUtility.TryDecryptData(encryptedData, key, out var decryptedData, out var errorMessage);

      // Assert
      Assert.True(result);
      Assert.NotNull(decryptedData);
      Assert.Equal(data, decryptedData);
      Assert.Null(errorMessage);
    }

    [Fact]
    public void DecryptData_InvalidKey_ReturnsError() {
      // Arrange
      var data = System.Text.Encoding.UTF8.GetBytes("Sensitive data");
      var key = AESGCMUtility.GenerateKeyBase64();
      AESGCMUtility.TryEncryptData(data, key, out var encryptedData, out var encryptErrorMessage);
      var invalidKey = AESGCMUtility.GenerateKeyBase64(); // Different key

      // Act
      var result = AESGCMUtility.TryDecryptData(encryptedData, invalidKey, out var decryptedData, out var errorMessage);

      // Assert
      Assert.False(result);
      Assert.Null(decryptedData);
      Assert.NotNull(errorMessage);
    }

    [Fact]
    public void DecryptData_ModifiedData_ReturnsError() {
      // Arrange
      var data = System.Text.Encoding.UTF8.GetBytes("Sensitive data");
      var key = AESGCMUtility.GenerateKeyBase64();
      AESGCMUtility.TryEncryptData(data, key, out var encryptedData, out var encryptErrorMessage);

      // Modify the encrypted data
      encryptedData[0] ^= 0xFF;

      // Act
      var result = AESGCMUtility.TryDecryptData(encryptedData, key, out var decryptedData, out var errorMessage);

      // Assert
      Assert.False(result);
      Assert.Null(decryptedData);
      Assert.NotNull(errorMessage);
    }

    [Fact]
    public void GenerateKeyBase64_ReturnsValidBase64String() {
      // Act
      var key = AESGCMUtility.GenerateKeyBase64();

      // Assert
      Assert.False(string.IsNullOrWhiteSpace(key));
      Assert.Equal(44, key.Length); // 32 bytes in Base64 is 44 characters
    }
  }
}
