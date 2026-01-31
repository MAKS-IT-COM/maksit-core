namespace MaksIT.Core.Tests.Security;

using MaksIT.Core.Security;

public class Base64UrlUtilityTests {

  #region Encode Tests

  [Fact]
  public void Encode_String_ReturnsBase64UrlString() {
    // Arrange
    var input = "Hello, World!";

    // Act
    var result = Base64UrlUtility.Encode(input);

    // Assert
    Assert.NotNull(result);
    Assert.DoesNotContain("+", result);
    Assert.DoesNotContain("/", result);
    Assert.DoesNotContain("=", result);
  }

  [Fact]
  public void Encode_EmptyString_ReturnsEmptyString() {
    // Arrange
    var input = "";

    // Act
    var result = Base64UrlUtility.Encode(input);

    // Assert
    Assert.Equal("", result);
  }

  [Fact]
  public void Encode_ByteArray_ReturnsBase64UrlString() {
    // Arrange
    var input = new byte[] { 0x00, 0x01, 0x02, 0x03, 0xFF, 0xFE };

    // Act
    var result = Base64UrlUtility.Encode(input);

    // Assert
    Assert.NotNull(result);
    Assert.DoesNotContain("+", result);
    Assert.DoesNotContain("/", result);
    Assert.DoesNotContain("=", result);
  }

  [Fact]
  public void Encode_NullByteArray_ThrowsArgumentNullException() {
    // Arrange
    byte[] input = null!;

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => Base64UrlUtility.Encode(input));
  }

  [Theory]
  [InlineData("f", "Zg")]
  [InlineData("fo", "Zm8")]
  [InlineData("foo", "Zm9v")]
  [InlineData("foob", "Zm9vYg")]
  [InlineData("fooba", "Zm9vYmE")]
  [InlineData("foobar", "Zm9vYmFy")]
  public void Encode_RFC4648TestVectors_ReturnsExpectedResult(string input, string expected) {
    // Act
    var result = Base64UrlUtility.Encode(input);

    // Assert
    Assert.Equal(expected, result);
  }

  [Fact]
  public void Encode_StringWithSpecialChars_HandlesCorrectly() {
    // Arrange - characters that would produce + and / in standard base64
    var input = "subjects?_d";

    // Act
    var result = Base64UrlUtility.Encode(input);

    // Assert
    Assert.DoesNotContain("+", result);
    Assert.DoesNotContain("/", result);
  }

  #endregion

  #region Decode Tests

  [Fact]
  public void Decode_ValidBase64Url_ReturnsOriginalBytes() {
    // Arrange
    var original = new byte[] { 0x00, 0x01, 0x02, 0x03, 0xFF, 0xFE };
    var encoded = Base64UrlUtility.Encode(original);

    // Act
    var decoded = Base64UrlUtility.Decode(encoded);

    // Assert
    Assert.Equal(original, decoded);
  }

  [Fact]
  public void Decode_NullInput_ThrowsArgumentNullException() {
    // Arrange
    string input = null!;

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => Base64UrlUtility.Decode(input));
  }

  [Theory]
  [InlineData("Zg", "f")]
  [InlineData("Zm8", "fo")]
  [InlineData("Zm9v", "foo")]
  [InlineData("Zm9vYg", "foob")]
  [InlineData("Zm9vYmE", "fooba")]
  [InlineData("Zm9vYmFy", "foobar")]
  public void DecodeToString_RFC4648TestVectors_ReturnsExpectedResult(string input, string expected) {
    // Act
    var result = Base64UrlUtility.DecodeToString(input);

    // Assert
    Assert.Equal(expected, result);
  }

  [Fact]
  public void DecodeToString_ValidBase64Url_ReturnsOriginalString() {
    // Arrange
    var original = "Hello, World!";
    var encoded = Base64UrlUtility.Encode(original);

    // Act
    var decoded = Base64UrlUtility.DecodeToString(encoded);

    // Assert
    Assert.Equal(original, decoded);
  }

  [Fact]
  public void Decode_EmptyString_ReturnsEmptyArray() {
    // Arrange
    var input = "";

    // Act
    var result = Base64UrlUtility.Decode(input);

    // Assert
    Assert.Empty(result);
  }

  #endregion

  #region Round-trip Tests

  [Theory]
  [InlineData("Simple text")]
  [InlineData("Text with spaces and numbers 123")]
  [InlineData("Special chars: !@#$%^&*()")]
  [InlineData("Unicode: 日本語 中文 한국어")]
  [InlineData("")]
  public void RoundTrip_String_ReturnsOriginal(string original) {
    // Act
    var encoded = Base64UrlUtility.Encode(original);
    var decoded = Base64UrlUtility.DecodeToString(encoded);

    // Assert
    Assert.Equal(original, decoded);
  }

  [Fact]
  public void RoundTrip_BinaryData_ReturnsOriginal() {
    // Arrange
    var original = new byte[256];
    for (int i = 0; i < 256; i++) {
      original[i] = (byte)i;
    }

    // Act
    var encoded = Base64UrlUtility.Encode(original);
    var decoded = Base64UrlUtility.Decode(encoded);

    // Assert
    Assert.Equal(original, decoded);
  }

  #endregion
}
