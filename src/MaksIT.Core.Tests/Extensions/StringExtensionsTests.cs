using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using MaksIT.Core.Extensions;
using Xunit;

namespace MaksIT.Core.Tests.Extensions {
  public class StringExtensionsTests {
    [Theory]
    [InlineData("Hello World", "H*", true)]        // Match starts with 'H'
    [InlineData("Hello World", "h*", true)]        // Case insensitive match
    [InlineData("Hello World", "*World", true)]    // Match ends with 'World'
    [InlineData("Hello World", "Hello?World", true)] // '?' should match exactly one character (space in this case)
    [InlineData("Hello World", "*W?rld", true)]    // '?' matches 'o' in 'World'
    [InlineData("Hello World", "Goodbye*", false)] // No match for 'Goodbye*'
    public void Like_ShouldReturnExpectedResults(string input, string pattern, bool expected) {
      // Act
      bool result = input.Like(pattern);

      // Assert
      Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("abcdef", 3, "abc")]
    [InlineData("abcdef", 0, "")]
    [InlineData("abcdef", 10, "abcdef")]
    public void Left_ShouldReturnLeftSubstring(string input, int count, string expected) {
      // Act
      string result = input.Left(count);

      // Assert
      Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("abcdef", 3, "def")]
    [InlineData("abcdef", 0, "")]
    [InlineData("abcdef", 10, "abcdef")]
    public void Right_ShouldReturnRightSubstring(string input, int count, string expected) {
      // Act
      string result = input.Right(count);

      // Assert
      Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("abcdef", 1, 3, "bcd")]
    [InlineData("abcdef", 0, 2, "ab")]
    [InlineData("abcdef", 4, 10, "ef")]
    [InlineData("abcdef", 6, 2, "")]
    public void Mid_ShouldReturnSubstring(string input, int index, int count, string expected) {
      // Act
      string result = input.Mid(index, count);

      // Assert
      Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123", 123)]
    [InlineData("abc", 0)]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    public void ToInteger_ShouldConvertToInteger(string input, int expected) {
      // Act
      int result = input.ToInteger();

      // Assert
      Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123", true)]
    [InlineData("-123", true)]
    [InlineData("abc", false)]
    [InlineData("123abc", false)]
    public void IsInteger_ShouldReturnIfStringIsInteger(string input, bool expected) {
      // Act
      bool result = input.IsInteger();

      // Assert
      Assert.Equal(expected, result);
    }

    [Fact]
    public void Prepend_ShouldPrependStringToStringBuilder() {
      // Arrange
      var sb = new StringBuilder("World");
      string content = "Hello ";

      // Act
      sb.Prepend(content);

      // Assert
      Assert.Equal("Hello World", sb.ToString());
    }

    [Theory]
    [InlineData("1", DayOfWeek.Monday)]
    [InlineData("Tuesday", DayOfWeek.Tuesday)]
    [InlineData("5", DayOfWeek.Friday)]
    public void ToEnum_ShouldConvertStringToEnum(string input, DayOfWeek expected) {
      // Act
      DayOfWeek result = input.ToEnum<DayOfWeek>();

      // Assert
      Assert.Equal(expected, result);
    }

    [Fact]
    public void ToEnum_InvalidValue_ShouldThrowNotSupportedException() {
      // Arrange
      string input = "NotAnEnumValue";

      // Act & Assert
      Assert.Throws<NotSupportedException>(() => input.ToEnum<DayOfWeek>());
    }

    [Theory]
    [InlineData("  ", null)]
    [InlineData("", null)]
    [InlineData("valid", "valid")]
    public void ToNull_ShouldReturnNullForWhitespaceOrEmptyString(string input, string expected) {
      // Act
      var result = input.ToNull();

      // Assert
      Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123", 123L)]
    public void ToLong_ShouldConvertToLong_WhenValidLong(string input, long? expected) {
      // Act
      var result = input.ToLong();

      // Assert
      Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("abc", null)]
    [InlineData("", null)]
    public void ToLong_ShouldReturnNull_WhenInvalidLong(string input, long? expected) {
      // Act
      var result = input.ToLong();

      // Assert
      Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("2021-08-30", new[] { "yyyy-MM-dd" }, "2021-08-30T00:00:00Z")]
    [InlineData("30/08/2021", new[] { "dd/MM/yyyy" }, "2021-08-30T00:00:00Z")]
    public void ToDate_ShouldConvertToDate(string input, string[] formats, string expected) {
      // Act
      var result = input.ToDate(formats);

      // Assert
      Assert.Equal(DateTime.Parse(expected, null, DateTimeStyles.RoundtripKind), result);
    }

    [Theory]
    [InlineData("2021-08-30T00:00:00Z", "2021-08-30T00:00:00Z")]
    [InlineData("Now", "Now")]
    public void ToDateTime_ShouldConvertToDateTime(string input, string expected) {
      // Act
      var result = input.ToDateTime();

      // Assert
      if (expected == "Now") {
        Assert.Equal(DateTime.Now.ToString("dd/MM/yyyy"), result.ToString("dd/MM/yyyy"));
      }
      else {
        Assert.Equal(DateTime.Parse(expected, null, DateTimeStyles.RoundtripKind), result);
      }
    }

    [Theory]
    [InlineData("ok", true)]
    [InlineData("yes", true)]
    [InlineData("true", true)]
    [InlineData("1", true)]
    [InlineData("no", false)]
    [InlineData("false", false)]
    [InlineData("0", false)]
    [InlineData("invalid", false)]
    public void ToBool_ShouldConvertToBool(string input, bool expected) {
      // Act
      var result = input.ToBool();

      // Assert
      Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("e02fd0e4-00fd-090A-ca30-0d00a0038ba0", "e02fd0e4-00fd-090a-ca30-0d00a0038ba0")]
    [InlineData("invalid-guid", null)]
    public void ToGuid_ShouldConvertStringToGuid(string input, string expected) {
      // Act
      if (expected == null) {
        var result = input.ToGuid();

        // Assert that it returns a valid Guid (from MD5 hash, not an exception)
        Assert.IsType<Guid>(result);
        Assert.NotEqual(Guid.Empty, result);  // Check that it does not return Guid.Empty
      }
      else {
        var result = input.ToGuid();

        // Assert
        Assert.Equal(Guid.Parse(expected), result);
      }
    }

    [Theory]
    [InlineData("<p>Hello World</p>", "Hello World")]
    [InlineData("<script>alert('test');</script><p>Hello</p>", "Hello")]
    public void HtmlToPlainText_ShouldConvertHtmlToPlainText(string input, string expected) {
      // Act
      var result = input.HtmlToPlainText();

      // Assert
      Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello world", "helloWorld")]
    [InlineData("Hello World", "helloWorld")]
    [InlineData("HELLO_WORLD", "helloWorld")]
    [InlineData("HELLO-WORLD", "helloWorld")]
    public void ToCamelCase_ShouldConvertToCamelCase(string input, string expected) {
      // Act
      var result = input.ToCamelCase();

      // Assert
      Assert.Equal(expected, result);
    }
  }
}
