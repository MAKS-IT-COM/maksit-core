using System;
using System.IO;
using MaksIT.Core.Threading;
using Microsoft.Extensions.Logging;

namespace MaksIT.Core.Logging;

public abstract class BaseFileLogger : ILogger, IDisposable {
  private readonly LockManager _lockManager = new LockManager();
  private readonly string _folderPath;
  private readonly TimeSpan _retentionPeriod;

  protected BaseFileLogger(string folderPath, TimeSpan retentionPeriod) {
    _folderPath = folderPath;
    _retentionPeriod = retentionPeriod;
    Directory.CreateDirectory(_folderPath); // Ensure the folder exists
  }

  public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

  public bool IsEnabled(LogLevel logLevel) {
    return logLevel != LogLevel.None;
  }

  public abstract void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter);

  protected string GenerateLogFileName(string extension) {
    return Path.Combine(_folderPath, $"log_{DateTime.UtcNow:yyyy-MM-dd}.{extension}");
  }

  protected void AppendToLogFile(string logFileName, string content) {
    _lockManager.ExecuteWithLockAsync(async () => {
      await File.AppendAllTextAsync(logFileName, content);
      RemoveExpiredLogFiles(Path.GetExtension(logFileName));
    }).Wait();
  }

  private void RemoveExpiredLogFiles(string extension) {
    var filePattern = $"log_*.{extension.TrimStart('.')}";

    var logFiles = Directory.GetFiles(_folderPath, filePattern);
    var expirationDate = DateTime.UtcNow - _retentionPeriod;

    foreach (var logFile in logFiles) {
      var fileName = Path.GetFileNameWithoutExtension(logFile);
      if (DateTime.TryParseExact(fileName.Substring(4), "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var logDate)) {
        if (logDate < expirationDate) {
          File.Delete(logFile);
        }
      }
    }
  }

  public void Dispose() {
    _lockManager.Dispose();
  }
}