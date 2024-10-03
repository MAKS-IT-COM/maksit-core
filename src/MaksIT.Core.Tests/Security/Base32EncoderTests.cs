using System.Text;

using MaksIT.Core.Security;


namespace MaksIT.Core.Tests.Security;

public class Base32EncoderTests {
  [Fact]
  public void Encode_ValidInput_ReturnsExpectedBase32String() {
    // Arrange
    var input = Encoding.UTF8.GetBytes("Hello World");
    var expected = "JBSWY3DPEBLW64TMMQ======";

    // Act
    var result = Base32Encoder.Encode(input);

    // Assert
    Assert.Equal(expected, result);
  }

  [Fact]
  public void Decode_ValidBase32String_ReturnsExpectedByteArray() {
    // Arrange
    var input = "JBSWY3DPEBLW64TMMQ======";
    var expected = Encoding.UTF8.GetBytes("Hello World");

    // Act
    var result = Base32Encoder.Decode(input);

    // Assert
    Assert.Equal(expected, result);
  }

  [Fact]
  public void Decode_InvalidBase32String_ThrowsFormatException() {
    // Act & Assert
    Assert.Throws<FormatException>(() => Base32Encoder.Decode("InvalidBase32String"));
  }

  [Fact]
  public void EncodeDecode_RoundTrip_ReturnsOriginalData() {
    // Arrange
    var originalData = Encoding.UTF8.GetBytes("RoundTripTest");

    // Act
    var encoded = Base32Encoder.Encode(originalData);
    var decoded = Base32Encoder.Decode(encoded);

    // Assert
    Assert.Equal(originalData, decoded);
  }
}
