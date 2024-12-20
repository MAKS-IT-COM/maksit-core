﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaksIT.Core.Logging;

[ProviderAlias("FileLogger")]
public class FileLoggerProvider : ILoggerProvider {
  private readonly string _filePath;

  public FileLoggerProvider(string filePath) {
    _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
  }

  public ILogger CreateLogger(string categoryName) {
    return new FileLogger(_filePath);
  }

  public void Dispose() { }
}
