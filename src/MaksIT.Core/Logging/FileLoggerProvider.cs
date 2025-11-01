using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaksIT.Core.Logging;

[ProviderAlias("FileLogger")]
public class FileLoggerProvider : ILoggerProvider {
  private readonly string _folderPath;
  private readonly TimeSpan _retentionPeriod;

  public FileLoggerProvider(string folderPath, TimeSpan? retentionPeriod = null) {
    _folderPath = folderPath ?? throw new ArgumentNullException(nameof(folderPath));
    _retentionPeriod = retentionPeriod ?? TimeSpan.FromDays(7); // Default retention period is 7 days
  }

  public ILogger CreateLogger(string categoryName) {
    return new FileLogger(_folderPath, _retentionPeriod);
  }

  public void Dispose() { }
}
