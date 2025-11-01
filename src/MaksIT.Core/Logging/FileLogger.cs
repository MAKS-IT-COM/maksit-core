using Microsoft.Extensions.Logging;

namespace MaksIT.Core.Logging;

public class FileLogger : BaseFileLogger {
  public FileLogger(string folderPath, TimeSpan retentionPeriod) : base(folderPath, retentionPeriod) { }

  public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
    if (!IsEnabled(logLevel))
      return;

    var message = formatter(state, exception);
    if (string.IsNullOrEmpty(message))
      return;

    var logRecord = $"{DateTime.UtcNow.ToString("o")} [{logLevel}] {message}";
    if (exception != null) {
      logRecord += Environment.NewLine + exception;
    }

    var logFileName = GenerateLogFileName("txt");
    AppendToLogFile(logFileName, logRecord + Environment.NewLine);
  }
}
