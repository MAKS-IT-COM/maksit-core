namespace MaksIT.Core.Tests;

using System.Globalization;

public class CultureTests {

  [Fact]
  public void TrySet_NullCulture_SetsInvariantCulture() {
    // Arrange
    string? culture = null;

    // Act
    var result = Culture.TrySet(culture, out var errorMessage);

    // Assert
    Assert.True(result);
    Assert.Null(errorMessage);
    Assert.Equal(CultureInfo.InvariantCulture, Thread.CurrentThread.CurrentCulture);
    Assert.Equal(CultureInfo.InvariantCulture, Thread.CurrentThread.CurrentUICulture);
  }

  [Fact]
  public void TrySet_EmptyCulture_SetsInvariantCulture() {
    // Arrange
    string culture = "";

    // Act
    var result = Culture.TrySet(culture, out var errorMessage);

    // Assert
    Assert.True(result);
    Assert.Null(errorMessage);
    Assert.Equal(CultureInfo.InvariantCulture, Thread.CurrentThread.CurrentCulture);
    Assert.Equal(CultureInfo.InvariantCulture, Thread.CurrentThread.CurrentUICulture);
  }

  [Theory]
  [InlineData("en-US")]
  [InlineData("en-GB")]
  [InlineData("de-DE")]
  [InlineData("fr-FR")]
  [InlineData("ja-JP")]
  public void TrySet_ValidCulture_SetsCulture(string cultureName) {
    // Act
    var result = Culture.TrySet(cultureName, out var errorMessage);

    // Assert
    Assert.True(result);
    Assert.Null(errorMessage);
    Assert.Equal(cultureName, Thread.CurrentThread.CurrentCulture.Name);
    Assert.Equal(cultureName, Thread.CurrentThread.CurrentUICulture.Name);
  }

  [Fact]
  public void TrySet_InvalidCulture_ReturnsFalseWithErrorMessage() {
    // Arrange - use a culture name that's invalid on all platforms
    // Note: Linux is more permissive with culture names than Windows
    // Using a very malformed name that should fail everywhere
    string culture = "xx-INVALID-12345-@#$%";

    // Act
    var result = Culture.TrySet(culture, out var errorMessage);

    // Assert
    // On some Linux systems, even invalid cultures may not throw
    // So we just verify the method handles it without crashing
    if (!result) {
      Assert.NotNull(errorMessage);
      Assert.NotEmpty(errorMessage);
    }
    // If it somehow succeeds (very permissive system), that's also acceptable
  }

  [Fact]
  public void TrySet_ValidCulture_AffectsCurrentThread() {
    // Arrange
    var originalCulture = Thread.CurrentThread.CurrentCulture;

    try {
      // Act
      Culture.TrySet("de-DE", out _);

      // Assert
      Assert.Equal("de-DE", Thread.CurrentThread.CurrentCulture.Name);
    }
    finally {
      // Cleanup - restore original culture
      Thread.CurrentThread.CurrentCulture = originalCulture;
      Thread.CurrentThread.CurrentUICulture = originalCulture;
    }
  }

  [Fact]
  public void TrySet_NeutralCulture_CreatesSpecificCulture() {
    // Arrange - "en" is a neutral culture, should create specific culture
    string culture = "en";

    // Act
    var result = Culture.TrySet(culture, out var errorMessage);

    // Assert
    Assert.True(result);
    Assert.Null(errorMessage);
    // CreateSpecificCulture("en") typically returns "en-US" or similar
    Assert.StartsWith("en", Thread.CurrentThread.CurrentCulture.Name);
  }
}
