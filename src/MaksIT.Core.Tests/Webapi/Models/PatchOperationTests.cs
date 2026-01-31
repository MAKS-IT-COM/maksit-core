namespace MaksIT.Core.Tests.Webapi.Models;

using MaksIT.Core.Webapi.Models;

public class PatchOperationTests {

  [Fact]
  public void PatchOperation_HasExpectedValues() {
    // Assert - verify all enum values exist
    Assert.Equal(0, (int)PatchOperation.SetField);
    Assert.Equal(1, (int)PatchOperation.RemoveField);
    Assert.Equal(2, (int)PatchOperation.AddToCollection);
    Assert.Equal(3, (int)PatchOperation.RemoveFromCollection);
  }

  [Fact]
  public void PatchOperation_HasFourValues() {
    // Arrange
    var values = Enum.GetValues<PatchOperation>();

    // Assert
    Assert.Equal(4, values.Length);
  }

  [Theory]
  [InlineData(PatchOperation.SetField, "SetField")]
  [InlineData(PatchOperation.RemoveField, "RemoveField")]
  [InlineData(PatchOperation.AddToCollection, "AddToCollection")]
  [InlineData(PatchOperation.RemoveFromCollection, "RemoveFromCollection")]
  public void PatchOperation_ToString_ReturnsCorrectName(PatchOperation operation, string expectedName) {
    // Act
    var result = operation.ToString();

    // Assert
    Assert.Equal(expectedName, result);
  }

  [Theory]
  [InlineData("SetField", PatchOperation.SetField)]
  [InlineData("RemoveField", PatchOperation.RemoveField)]
  [InlineData("AddToCollection", PatchOperation.AddToCollection)]
  [InlineData("RemoveFromCollection", PatchOperation.RemoveFromCollection)]
  public void PatchOperation_Parse_ReturnsCorrectValue(string name, PatchOperation expected) {
    // Act
    var result = Enum.Parse<PatchOperation>(name);

    // Assert
    Assert.Equal(expected, result);
  }

  [Fact]
  public void PatchOperation_TryParse_InvalidValue_ReturnsFalse() {
    // Act
    var result = Enum.TryParse<PatchOperation>("InvalidOperation", out var value);

    // Assert
    Assert.False(result);
  }

  [Fact]
  public void PatchOperation_IsDefined_ValidValues_ReturnsTrue() {
    // Assert
    Assert.True(Enum.IsDefined(typeof(PatchOperation), 0));
    Assert.True(Enum.IsDefined(typeof(PatchOperation), 1));
    Assert.True(Enum.IsDefined(typeof(PatchOperation), 2));
    Assert.True(Enum.IsDefined(typeof(PatchOperation), 3));
  }

  [Fact]
  public void PatchOperation_IsDefined_InvalidValue_ReturnsFalse() {
    // Assert
    Assert.False(Enum.IsDefined(typeof(PatchOperation), 99));
    Assert.False(Enum.IsDefined(typeof(PatchOperation), -1));
  }
}
