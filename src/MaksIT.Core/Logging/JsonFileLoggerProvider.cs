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
    var folderPath = ResolveFolderPath(categoryName);
    return new JsonFileLogger(folderPath, _retentionPeriod);
  }

  private string ResolveFolderPath(string categoryName) {
    var (prefix, value) = LoggerPrefix.Parse(categoryName);

    var newFolderPath = _folderPath;

    if (prefix == LoggerPrefix.Folder && !string.IsNullOrWhiteSpace(value)) {
      newFolderPath = Path.Combine(newFolderPath, SanitizeForPath(value));
    }

    return newFolderPath;
  }

  private static string SanitizeForPath(string input) {
    var invalid = Path.GetInvalidPathChars();
    return string.Concat(input.Where(c => !invalid.Contains(c)));
  }

  public void Dispose() { }
}