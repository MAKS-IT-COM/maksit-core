namespace MaksIT.Core.Extensions;

public static class ExceptionExtensions {
  /// <summary>
  /// Extracts all messages from an exception and its inner exceptions.
  /// </summary>
  /// <param name="exception">The exception to extract messages from.</param>
  /// <returns>A list of exception messages.</returns>
  public static List<string> ExtractMessages(this Exception exception) {
    var messages = new List<string>();
    var current = exception;

    while (current != null) {
      messages.Add(current.Message);
      current = current.InnerException;
    }

    return messages;
  }
}
