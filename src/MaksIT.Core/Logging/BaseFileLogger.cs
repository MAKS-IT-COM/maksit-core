using MaksIT.Core.Threading;
using Microsoft.Extensions.Logging;


namespace MaksIT.Core.Logging;

public abstract class BaseFileLogger : ILogger, IDisposable {
  private readonly LockManager _lockManager = new LockManager();
  private readonly string _folderPath;
  private readonly TimeSpan _retentionPeriod;
  private static readonly Mutex _fileMutex = new Mutex(false, "Global\\MaksITLoggerFileMutex"); // Named mutex for cross-process locking

  protected BaseFileLogger(string folderPath, TimeSpan retentionPeriod) {
    _folderPath = folderPath;
    _retentionPeriod = retentionPeriod;
    Directory.CreateDirectory(_folderPath); // Ensure the folder exists
  }

  public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

  public bool IsEnabled(LogLevel logLevel) {
    return logLevel != LogLevel.None;
  }

  public abstract Task Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter);

  // Explicit interface implementation for ILogger.Log (void)
  void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
    // Call the async Log and wait synchronously for compatibility
    Log(logLevel, eventId, state, exception, formatter).GetAwaiter().GetResult();
  }

  protected string GenerateLogFileName(string extension) {
    return Path.Combine(_folderPath, $"log_{DateTime.UtcNow:yyyy-MM-dd}.{extension}");
  }

  protected Task AppendToLogFileAsync(string logFileName, string content) {
    bool mutexAcquired = false;
    try {
        mutexAcquired = _fileMutex.WaitOne(10000);
        if (!mutexAcquired) throw new IOException("Could not acquire file mutex for logging.");
        File.AppendAllText(logFileName, content); // Synchronous write
        RemoveExpiredLogFiles(Path.GetExtension(logFileName));
        return Task.CompletedTask;
    }
    finally {
        if (mutexAcquired) _fileMutex.ReleaseMutex();
    }
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
    // Do NOT dispose the static mutex here; it should be disposed once per process, not per instance.
  }

  // Optionally, add a static method to dispose the mutex at application shutdown:
  public static void DisposeMutex() {
    _fileMutex?.Dispose();
  }
}