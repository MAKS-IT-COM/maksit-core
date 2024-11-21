using System;
using System.Security.Cryptography;
using Xunit;
using MaksIT.Core.Security;

namespace MaksIT.Core.Tests.Security {
  public class Crc32Tests {
    [Fact]
    public void Crc32_DefaultConstructor_InitializesCorrectly() {
      // Arrange & Act
      using var crc32 = new Crc32();

      // Assert
      Assert.NotNull(crc32);
      Assert.Equal(32, crc32.HashSize);
    }

    [Fact]
    public void Crc32_ComputeHash_ReturnsExpectedHash() {
      // Arrange
      using var crc32 = new Crc32();
      var data = System.Text.Encoding.UTF8.GetBytes("Test data");

      // Act
      var hash = crc32.ComputeHash(data);

      // Assert
      Assert.NotNull(hash);
      Assert.Equal(4, hash.Length); // CRC32 hash length is 4 bytes
    }

    [Fact]
    public void Crc32_TryCompute_ValidData_ReturnsTrue() {
      // Arrange
      var data = System.Text.Encoding.UTF8.GetBytes("Test data");

      // Act
      var result = Crc32.TryCompute(data, out var checksum, out var errorMessage);

      // Assert
      Assert.True(result);
      Assert.NotEqual(0u, checksum);
      Assert.Null(errorMessage);
    }

    [Fact]
    public void Crc32_TryCompute_InvalidData_ReturnsFalse() {
      // Arrange
      byte[] data = null;

      // Act
      var result = Crc32.TryCompute(data, out var checksum, out var errorMessage);

      // Assert
      Assert.False(result);
      Assert.Equal(0u, checksum);
      Assert.NotNull(errorMessage);
    }

    [Fact]
    public void Crc32_TryCompute_WithSeed_ReturnsExpectedHash() {
      // Arrange
      var data = System.Text.Encoding.UTF8.GetBytes("Test data");
      uint seed = 0x12345678;

      // Act
      var result = Crc32.TryCompute(seed, data, out var checksum, out var errorMessage);

      // Assert
      Assert.True(result);
      Assert.NotEqual(0u, checksum);
      Assert.Null(errorMessage);
    }

    [Fact]
    public void Crc32_TryCompute_WithPolynomialAndSeed_ReturnsExpectedHash() {
      // Arrange
      var data = System.Text.Encoding.UTF8.GetBytes("Test data");
      uint polynomial = 0x04C11DB7;
      uint seed = 0x12345678;

      // Act
      var result = Crc32.TryCompute(polynomial, seed, data, out var checksum, out var errorMessage);

      // Assert
      Assert.True(result);
      Assert.NotEqual(0u, checksum);
      Assert.Null(errorMessage);
    }

    [Fact]
    public void Crc32_Initialize_ResetsHash() {
      // Arrange
      using var crc32 = new Crc32();
      var data = System.Text.Encoding.UTF8.GetBytes("Test data");
      crc32.ComputeHash(data);

      // Act
      crc32.Initialize();
      var hash = crc32.ComputeHash(data);

      // Assert
      Assert.NotNull(hash);
      Assert.Equal(4, hash.Length); // CRC32 hash length is 4 bytes
    }
  }
}
