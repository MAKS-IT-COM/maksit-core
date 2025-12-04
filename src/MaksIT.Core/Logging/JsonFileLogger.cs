using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MaksIT.Core.Logging;

public class JsonFileLogger : BaseFileLogger {
  public JsonFileLogger(string folderPath, TimeSpan retentionPeriod) : base(folderPath, retentionPeriod) { }

  public override async Task Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
    if (!IsEnabled(logLevel))
      return;

    var logEntry = new {
      Timestamp = DateTime.UtcNow.ToString("o"),
      LogLevel = logLevel.ToString(),
      Message = formatter(state, exception),
      Exception = exception?.ToString()
    };

    var logFileName = GenerateLogFileName("json");
    await AppendToLogFileAsync(logFileName, JsonSerializer.Serialize(logEntry) + Environment.NewLine);
  }
}