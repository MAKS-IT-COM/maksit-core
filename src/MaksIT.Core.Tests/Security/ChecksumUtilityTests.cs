using System;
using System.IO;
using Xunit;
using MaksIT.Core.Security;

namespace MaksIT.Core.Tests.Security {
  public class ChecksumUtilityTests {
    [Fact]
    public void CalculateCRC32Checksum_ValidData_ReturnsChecksum() {
      // Arrange
      var data = System.Text.Encoding.UTF8.GetBytes("Test data");

      // Act
      var result = ChecksumUtility.TryCalculateCRC32Checksum(data, out var checksum, out var errorMessage);

      // Assert
      Assert.True(result);
      Assert.NotNull(checksum);
      Assert.Null(errorMessage);
    }

    [Fact]
    public void CalculateCRC32ChecksumFromFile_ValidFile_ReturnsChecksum() {
      // Arrange
      var filePath = Path.GetTempFileName();
      File.WriteAllText(filePath, "Test data");

      // Act
      var result = ChecksumUtility.TryCalculateCRC32ChecksumFromFile(filePath, out var checksum, out var errorMessage);

      // Assert
      Assert.True(result);
      Assert.NotNull(checksum);
      Assert.Null(errorMessage);

      // Cleanup
      File.Delete(filePath);
    }

    [Fact]
    public void CalculateCRC32ChecksumFromFile_FileNotFound_ReturnsError() {
      // Arrange
      var filePath = "nonexistentfile.txt";

      // Act
      var result = ChecksumUtility.TryCalculateCRC32ChecksumFromFile(filePath, out var checksum, out var errorMessage);

      // Assert
      Assert.False(result);
      Assert.Null(checksum);
      Assert.NotNull(errorMessage);
    }

    [Fact]
    public void CalculateCRC32ChecksumFromFileInChunks_ValidFile_ReturnsChecksum() {
      // Arrange
      var filePath = Path.GetTempFileName();
      File.WriteAllText(filePath, "Test data");

      // Act
      var result = ChecksumUtility.TryCalculateCRC32ChecksumFromFileInChunks(filePath, out var checksum, out var errorMessage);

      // Assert
      Assert.True(result);
      Assert.NotNull(checksum);
      Assert.Null(errorMessage);

      // Cleanup
      File.Delete(filePath);
    }

    [Fact]
    public void CalculateCRC32ChecksumFromFileInChunks_FileNotFound_ReturnsError() {
      // Arrange
      var filePath = "nonexistentfile.txt";

      // Act
      var result = ChecksumUtility.TryCalculateCRC32ChecksumFromFileInChunks(filePath, out var checksum, out var errorMessage);

      // Assert
      Assert.False(result);
      Assert.Null(checksum);
      Assert.NotNull(errorMessage);
    }

    [Fact]
    public void VerifyCRC32Checksum_ValidData_ReturnsTrue() {
      // Arrange
      var data = System.Text.Encoding.UTF8.GetBytes("Test data");
      ChecksumUtility.TryCalculateCRC32Checksum(data, out var checksum, out var errorMessage);

      // Act
      var result = ChecksumUtility.VerifyCRC32Checksum(data, checksum);

      // Assert
      Assert.True(result);
    }

    [Fact]
    public void VerifyCRC32Checksum_InvalidChecksum_ReturnsFalse() {
      // Arrange
      var data = System.Text.Encoding.UTF8.GetBytes("Test data");
      var invalidChecksum = "00000000";

      // Act
      var result = ChecksumUtility.VerifyCRC32Checksum(data, invalidChecksum);

      // Assert
      Assert.False(result);
    }

    [Fact]
    public void VerifyCRC32ChecksumFromFile_ValidFile_ReturnsTrue() {
      // Arrange
      var filePath = Path.GetTempFileName();
      File.WriteAllText(filePath, "Test data");
      ChecksumUtility.TryCalculateCRC32ChecksumFromFile(filePath, out var checksum, out var errorMessage);

      // Act
      var result = ChecksumUtility.VerifyCRC32ChecksumFromFile(filePath, checksum);

      // Assert
      Assert.True(result);

      // Cleanup
      File.Delete(filePath);
    }

    [Fact]
    public void VerifyCRC32ChecksumFromFile_InvalidChecksum_ReturnsFalse() {
      // Arrange
      var filePath = Path.GetTempFileName();
      File.WriteAllText(filePath, "Test data");
      var invalidChecksum = "00000000";

      // Act
      var result = ChecksumUtility.VerifyCRC32ChecksumFromFile(filePath, invalidChecksum);

      // Assert
      Assert.False(result);

      // Cleanup
      File.Delete(filePath);
    }

    [Fact]
    public void VerifyCRC32ChecksumFromFileInChunks_ValidFile_ReturnsTrue() {
      // Arrange
      var filePath = Path.GetTempFileName();
      File.WriteAllText(filePath, "Test data");
      ChecksumUtility.TryCalculateCRC32ChecksumFromFileInChunks(filePath, out var checksum, out var errorMessage);

      // Act
      var result = ChecksumUtility.VerifyCRC32ChecksumFromFileInChunks(filePath, checksum);

      // Assert
      Assert.True(result);

      // Cleanup
      File.Delete(filePath);
    }

    [Fact]
    public void VerifyCRC32ChecksumFromFileInChunks_InvalidChecksum_ReturnsFalse() {
      // Arrange
      var filePath = Path.GetTempFileName();
      File.WriteAllText(filePath, "Test data");
      var invalidChecksum = "00000000";

      // Act
      var result = ChecksumUtility.VerifyCRC32ChecksumFromFileInChunks(filePath, invalidChecksum);

      // Assert
      Assert.False(result);

      // Cleanup
      File.Delete(filePath);
    }
  }
}
