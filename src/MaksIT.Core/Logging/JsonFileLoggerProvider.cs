using Microsoft.Extensions.Logging;

namespace MaksIT.Core.Logging;

[ProviderAlias("JsonFileLogger")]
public class JsonFileLoggerProvider : ILoggerProvider {
  private readonly string _folderPath;
  private readonly TimeSpan _retentionPeriod;

  public JsonFileLoggerProvider(string folderPath, TimeSpan? retentionPeriod = null) {
    _folderPath = folderPath ?? throw new ArgumentNullException(nameof(folderPath));
    _retentionPeriod = retentionPeriod ?? TimeSpan.FromDays(7); // Default retention period is 7 days
  }

  public ILogger CreateLogger(string categoryName) {
    return new JsonFileLogger(_folderPath, _retentionPeriod);
  }

  public void Dispose() { }
}