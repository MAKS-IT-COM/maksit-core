using MaksIT.Core.Extensions;
using MaksIT.Core.Security;
using System;
using Xunit;

namespace MaksIT.Core.Tests.Security {
  public class TotpGeneratorTests {
    private const string Base32Secret = "JBSWY3DPEHPK3PXP"; // Example Base32 secret

    [Fact]
    public void Validate_ValidTotpCode_ReturnsTrue() {
      // Arrange
      var timestep = TotpGenerator.GetCurrentTimeStepNumber();
      TotpGenerator.TryGenerate(Base32Secret, timestep, out var validTotpCode, out var generateErrorMessage);

      // Act
      var result = TotpGenerator.TryValidate(validTotpCode, Base32Secret, 0, out var isValid, out var validateErrorMessage);

      // Assert
      Assert.True(result);
      Assert.True(isValid);
      Assert.Null(validateErrorMessage);
    }

    [Fact]
    public void Validate_InvalidTotpCode_ReturnsFalse() {
      // Arrange
      var invalidTotpCode = "123456"; // Example invalid TOTP code

      // Act
      var result = TotpGenerator.TryValidate(invalidTotpCode, Base32Secret, 0, out var isValid, out var errorMessage);

      // Assert
      Assert.True(result);
      Assert.False(isValid);
      Assert.Null(errorMessage);
    }

    [Fact]
    public void Validate_TotpCodeWithTimeTolerance_ReturnsTrue() {
      // Arrange
      var timestep = TotpGenerator.GetCurrentTimeStepNumber() - 1; // One timestep in the past
      TotpGenerator.TryGenerate(Base32Secret, timestep, out var validTotpCode, out var generateErrorMessage);

      // Act
      var result = TotpGenerator.TryValidate(validTotpCode, Base32Secret, 1, out var isValid, out var validateErrorMessage);

      // Assert
      Assert.True(result);
      Assert.True(isValid);
      Assert.Null(validateErrorMessage);
    }

    [Fact]
    public void Generate_WithBase32Secret_ReturnsTotpCode() {
      // Arrange
      var timestep = TotpGenerator.GetCurrentTimeStepNumber();

      // Act
      var result = TotpGenerator.TryGenerate(Base32Secret, timestep, out var totpCode, out var errorMessage);

      // Assert
      Assert.True(result);
      Assert.False(string.IsNullOrEmpty(totpCode));
      Assert.Equal(6, totpCode.Length);
      Assert.Null(errorMessage);
    }

    [Fact]
    public void GetCurrentTimeStepNumber_ReturnsNonNegativeValue() {
      // Act
      var timestep = TotpGenerator.GetCurrentTimeStepNumber();

      // Assert
      Assert.True(timestep >= 0);
    }

    [Fact]
    public void GenerateSecret_ReturnsValidBase32String() {
      // Act
      var result = TotpGenerator.TryGenerateSecret(out var secret, out var errorMessage);

      // Assert
      Assert.True(result);
      Assert.False(string.IsNullOrEmpty(secret));
      Assert.True(secret.IsBase32String());
      Assert.Null(errorMessage);
    }
  }
}
