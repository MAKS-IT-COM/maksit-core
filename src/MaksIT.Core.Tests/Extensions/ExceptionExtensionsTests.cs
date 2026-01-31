namespace MaksIT.Core.Tests.Extensions;

using MaksIT.Core.Extensions;

public class ExceptionExtensionsTests {

  [Fact]
  public void ExtractMessages_SingleException_ReturnsSingleMessage() {
    // Arrange
    var exception = new InvalidOperationException("Test message");

    // Act
    var messages = exception.ExtractMessages();

    // Assert
    Assert.Single(messages);
    Assert.Equal("Test message", messages[0]);
  }

  [Fact]
  public void ExtractMessages_WithInnerException_ReturnsAllMessages() {
    // Arrange
    var innerException = new ArgumentException("Inner message");
    var outerException = new InvalidOperationException("Outer message", innerException);

    // Act
    var messages = outerException.ExtractMessages();

    // Assert
    Assert.Equal(2, messages.Count);
    Assert.Equal("Outer message", messages[0]);
    Assert.Equal("Inner message", messages[1]);
  }

  [Fact]
  public void ExtractMessages_WithMultipleNestedExceptions_ReturnsAllMessages() {
    // Arrange
    var innermost = new ArgumentNullException("param", "Innermost message");
    var middle = new ArgumentException("Middle message", innermost);
    var outer = new InvalidOperationException("Outer message", middle);

    // Act
    var messages = outer.ExtractMessages();

    // Assert
    Assert.Equal(3, messages.Count);
    Assert.Equal("Outer message", messages[0]);
    Assert.Equal("Middle message", messages[1]);
    Assert.Contains("Innermost message", messages[2]);
  }

  [Fact]
  public void ExtractMessages_AggregateException_ReturnsOuterMessage() {
    // Arrange
    var inner1 = new InvalidOperationException("Error 1");
    var inner2 = new ArgumentException("Error 2");
    var aggregate = new AggregateException("Multiple errors", inner1, inner2);

    // Act
    var messages = aggregate.ExtractMessages();

    // Assert
    // AggregateException's InnerException is the first inner exception
    Assert.Equal(2, messages.Count);
    Assert.Contains("Multiple errors", messages[0]);
  }

  [Fact]
  public void ExtractMessages_EmptyMessage_ReturnsEmptyString() {
    // Arrange
    var exception = new Exception("");

    // Act
    var messages = exception.ExtractMessages();

    // Assert
    Assert.Single(messages);
    Assert.Equal("", messages[0]);
  }
}
