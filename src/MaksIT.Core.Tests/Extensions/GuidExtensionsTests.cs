using MaksIT.Core.Extensions;


namespace MaksIT.Core.Tests.Extensions {

  public class GuidExtensionsTests {
    [Fact]
    public void ToNullable_WithEmptyGuid_ShouldReturnNull() {
      // Arrange
      var emptyGuid = Guid.Empty;

      // Act
      var result = emptyGuid.ToNullable();

      // Assert
      Assert.Null(result);
    }

    [Fact]
    public void ToNullable_WithNonEmptyGuid_ShouldReturnSameGuid() {
      // Arrange
      var nonEmptyGuid = Guid.NewGuid();

      // Act
      var result = nonEmptyGuid.ToNullable();

      // Assert
      Assert.NotNull(result);
      Assert.Equal(nonEmptyGuid, result);
    }

    [Fact]
    public void ToNullable_WithDefaultGuid_ShouldReturnNull() {
      // Arrange
      var defaultGuid = default(Guid);

      // Act
      var result = defaultGuid.ToNullable();

      // Assert
      Assert.Null(result);
    }

    [Fact]
    public void ToNullable_WithSameGuidTwice_ShouldReturnSameGuidEachTime() {
      // Arrange
      var guid = Guid.NewGuid();

      // Act
      var result1 = guid.ToNullable();
      var result2 = guid.ToNullable();

      // Assert
      Assert.Equal(result1, result2);
    }

    [Fact]
    public void ToNullable_WithMultipleNewGuids_ShouldReturnUniqueNonEmptyResults() {
      // Arrange
      var guid1 = Guid.NewGuid();
      var guid2 = Guid.NewGuid();

      // Act
      var result1 = guid1.ToNullable();
      var result2 = guid2.ToNullable();

      // Assert
      Assert.NotEqual(result1, result2);
      Assert.NotNull(result1);
      Assert.NotNull(result2);
    }
  }
}
