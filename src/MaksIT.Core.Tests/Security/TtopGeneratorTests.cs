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
      var validTotpCode = TotpGenerator.Generate(Base32Secret, timestep);

      // Act
      var isValid = TotpGenerator.Validate(validTotpCode, Base32Secret);

      // Assert
      Assert.True(isValid);
    }

    [Fact]
    public void Validate_InvalidTotpCode_ReturnsFalse() {
      // Arrange
      var invalidTotpCode = "123456"; // Example invalid TOTP code

      // Act
      var isValid = TotpGenerator.Validate(invalidTotpCode, Base32Secret);

      // Assert
      Assert.False(isValid);
    }

    [Fact]
    public void Validate_TotpCodeWithTimeTolerance_ReturnsTrue() {
      // Arrange
      var timestep = TotpGenerator.GetCurrentTimeStepNumber() - 1; // One timestep in the past
      var validTotpCode = TotpGenerator.Generate(Base32Secret, timestep);

      // Act
      var isValid = TotpGenerator.Validate(validTotpCode, Base32Secret, timeTolerance: 1);

      // Assert
      Assert.True(isValid);
    }

    [Fact]
    public void Generate_WithBase32Secret_ReturnsTotpCode() {
      // Arrange
      var timestep = TotpGenerator.GetCurrentTimeStepNumber();

      // Act
      var totpCode = TotpGenerator.Generate(Base32Secret, timestep);

      // Assert
      Assert.False(string.IsNullOrEmpty(totpCode));
      Assert.Equal(6, totpCode.Length);
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
      var secret = TotpGenerator.GenerateSecret();

      // Assert
      Assert.False(string.IsNullOrEmpty(secret));
      Assert.True(secret.IsBase32String());
    }

    
  }
}
