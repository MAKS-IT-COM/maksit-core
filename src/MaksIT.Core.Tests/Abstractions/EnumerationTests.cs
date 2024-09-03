using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MaksIT.Core.Abstractions.Tests {

  public class TestEnumeration : Enumeration {
    public static readonly TestEnumeration First = new TestEnumeration(1, "First");
    public static readonly TestEnumeration Second = new TestEnumeration(2, "Second");
    public static readonly TestEnumeration Third = new TestEnumeration(3, "Third");

    public TestEnumeration(int id, string name) : base(id, name) { }
  }


  public class EnumerationTests {
    [Fact]
    public void GetAll_ShouldReturnAllEnumerations() {
      // Act
      var allValues = Enumeration.GetAll<TestEnumeration>().ToList();

      // Assert
      Assert.NotNull(allValues);
      Assert.Equal(3, allValues.Count);
      Assert.Contains(TestEnumeration.First, allValues);
      Assert.Contains(TestEnumeration.Second, allValues);
      Assert.Contains(TestEnumeration.Third, allValues);
    }

    [Theory]
    [InlineData(1, "First")]
    [InlineData(2, "Second")]
    [InlineData(3, "Third")]
    public void FromValue_ShouldReturnEnumerationByValue(int id, string expectedName) {
      // Act
      var result = Enumeration.FromValue<TestEnumeration>(id);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(expectedName, result.Name);
    }

    [Theory]
    [InlineData("First", 1)]
    [InlineData("Second", 2)]
    [InlineData("Third", 3)]
    public void FromDisplayName_ShouldReturnEnumerationByName(string displayName, int expectedId) {
      // Act
      var result = Enumeration.FromDisplayName<TestEnumeration>(displayName);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(expectedId, result.Id);
    }

    [Fact]
    public void AbsoluteDifference_ShouldReturnCorrectDifference() {
      // Act
      var difference = Enumeration.AbsoluteDifference(TestEnumeration.First, TestEnumeration.Third);

      // Assert
      Assert.Equal(2, difference);
    }

    [Fact]
    public void Equals_SameReference_ShouldReturnTrue() {
      // Act
      var result = TestEnumeration.First.Equals(TestEnumeration.First);

      // Assert
      Assert.True(result);
    }

    [Fact]
    public void Equals_DifferentReferencesSameValues_ShouldReturnTrue() {
      // Arrange
      var firstCopy = Enumeration.FromValue<TestEnumeration>(1);

      // Act
      var result = TestEnumeration.First.Equals(firstCopy);

      // Assert
      Assert.True(result);
    }

    [Fact]
    public void Equals_DifferentValues_ShouldReturnFalse() {
      // Act
      var result = TestEnumeration.First.Equals(TestEnumeration.Second);

      // Assert
      Assert.False(result);
    }

    [Fact]
    public void CompareTo_ShouldReturnZeroForEqualValues() {
      // Arrange
      var firstCopy = Enumeration.FromValue<TestEnumeration>(1);

      // Act
      var result = TestEnumeration.First.CompareTo(firstCopy);

      // Assert
      Assert.Equal(0, result);
    }

    [Fact]
    public void CompareTo_ShouldReturnPositiveForGreaterValue() {
      // Act
      var result = TestEnumeration.Second.CompareTo(TestEnumeration.First);

      // Assert
      Assert.True(result > 0);
    }

    [Fact]
    public void CompareTo_ShouldReturnNegativeForLesserValue() {
      // Act
      var result = TestEnumeration.First.CompareTo(TestEnumeration.Second);

      // Assert
      Assert.True(result < 0);
    }

    [Fact]
    public void CompareTo_InvalidComparison_ShouldThrowArgumentException() {
      // Arrange
      var nonEnumerationObject = new object();

      // Act & Assert
      Assert.Throws<ArgumentException>(() => TestEnumeration.First.CompareTo(nonEnumerationObject));
    }

    [Fact]
    public void GetHashCode_ShouldReturnIdHashCode() {
      // Act
      var hashCode = TestEnumeration.First.GetHashCode();

      // Assert
      Assert.Equal(TestEnumeration.First.Id.GetHashCode(), hashCode);
    }

    [Fact]
    public void ToString_ShouldReturnName() {
      // Act
      var result = TestEnumeration.First.ToString();

      // Assert
      Assert.Equal("First", result);
    }

    [Fact]
    public void Parse_InvalidValue_ShouldThrowInvalidOperationException() {
      // Act & Assert
      Assert.Throws<InvalidOperationException>(() => Enumeration.FromValue<TestEnumeration>(999));
    }

    [Fact]
    public void Parse_InvalidDisplayName_ShouldThrowInvalidOperationException() {
      // Act & Assert
      Assert.Throws<InvalidOperationException>(() => Enumeration.FromDisplayName<TestEnumeration>("NonExistent"));
    }
  }
}
