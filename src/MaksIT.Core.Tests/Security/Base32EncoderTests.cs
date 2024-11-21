using System.Text;
using MaksIT.Core.Security;
using Xunit;

namespace MaksIT.Core.Tests.Security {
  public class Base32EncoderTests {
    [Fact]
    public void Encode_ValidInput_ReturnsExpectedBase32String() {
      // Arrange
      var input = Encoding.UTF8.GetBytes("Hello World");
      var expected = "JBSWY3DPEBLW64TMMQ======";

      // Act
      var result = Base32Encoder.TryEncode(input, out var encoded, out var errorMessage);

      // Assert
      Assert.True(result);
      Assert.Equal(expected, encoded);
      Assert.Null(errorMessage);
    }

    [Fact]
    public void Decode_ValidBase32String_ReturnsExpectedByteArray() {
      // Arrange
      var input = "JBSWY3DPEBLW64TMMQ======";
      var expected = Encoding.UTF8.GetBytes("Hello World");

      // Act
      var result = Base32Encoder.TryDecode(input, out var decoded, out var errorMessage);

      // Assert
      Assert.True(result);
      Assert.Equal(expected, decoded);
      Assert.Null(errorMessage);
    }

    [Fact]
    public void Decode_InvalidBase32String_ReturnsFalse() {
      // Act
      var result = Base32Encoder.TryDecode("InvalidBase32String", out var decoded, out var errorMessage);

      // Assert
      Assert.False(result);
      Assert.Null(decoded);
      Assert.NotNull(errorMessage);
    }

    [Fact]
    public void EncodeDecode_RoundTrip_ReturnsOriginalData() {
      // Arrange
      var originalData = Encoding.UTF8.GetBytes("RoundTripTest");

      // Act
      var encodeResult = Base32Encoder.TryEncode(originalData, out var encoded, out var encodeErrorMessage);
      var decodeResult = Base32Encoder.TryDecode(encoded, out var decoded, out var decodeErrorMessage);

      // Assert
      Assert.True(encodeResult);
      Assert.True(decodeResult);
      Assert.Equal(originalData, decoded);
      Assert.Null(encodeErrorMessage);
      Assert.Null(decodeErrorMessage);
    }
  }
}
