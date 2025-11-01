using Microsoft.Extensions.Logging;
using System.IO;

namespace MaksIT.Core.Logging;

public class FileLogger : ILogger {
  private readonly string _folderPath;
  private readonly object _lock = new object();
  private readonly TimeSpan _retentionPeriod;

  public FileLogger(string folderPath, TimeSpan retentionPeriod) {
    _folderPath = folderPath;
    _retentionPeriod = retentionPeriod;
    Directory.CreateDirectory(_folderPath); // Ensure the folder exists
  }

  public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

  public bool IsEnabled(LogLevel logLevel) {
    return logLevel != LogLevel.None;
  }

  public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
    if (!IsEnabled(logLevel))
      return;

    var message = formatter(state, exception);
    if (string.IsNullOrEmpty(message))
      return;

    var logRecord = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {message}";
    if (exception != null) {
      logRecord += Environment.NewLine + exception;
    }

    var logFileName = Path.Combine(_folderPath, $"log_{DateTime.Now:yyyy-MM-dd}.txt"); // Generate log file name by date

    lock (_lock) {
      File.AppendAllText(logFileName, logRecord + Environment.NewLine);
      CleanUpOldLogs();
    }
  }

  private void CleanUpOldLogs() {
    var logFiles = Directory.GetFiles(_folderPath, "log_*.txt");
    var expirationDate = DateTime.Now - _retentionPeriod;

    foreach (var logFile in logFiles) {
      var fileName = Path.GetFileNameWithoutExtension(logFile);
      if (DateTime.TryParseExact(fileName.Substring(4), "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var logDate)) {
        if (logDate < expirationDate) {
          File.Delete(logFile);
        }
      }
    }
  }
}
