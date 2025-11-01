using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace MaksIT.Core.Logging;

public static class LoggingBuilderExtensions {
  public static ILoggingBuilder AddFileLogger(this ILoggingBuilder logging, string folderPath, TimeSpan? retentionPeriod = null) {
    logging.Services.AddSingleton<ILoggerProvider>(new FileLoggerProvider(folderPath, retentionPeriod));
    return logging;
  }

  public static ILoggingBuilder AddJsonFileLogger(this ILoggingBuilder logging, string folderPath, TimeSpan? retentionPeriod = null) {
    logging.Services.AddSingleton<ILoggerProvider>(new JsonFileLoggerProvider(folderPath, retentionPeriod));
    return logging;
  }

  public static ILoggingBuilder AddSimpleConsoleLogger(this ILoggingBuilder logging) {
    logging.AddSimpleConsole(options => {
      options.IncludeScopes = true;
      options.SingleLine = false;
      options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
    });
    return logging;
  }

  public static ILoggingBuilder AddJsonConsoleLogger(this ILoggingBuilder logging) {
    logging.AddJsonConsole(options => {
      options.IncludeScopes = true;
      options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
    });
    return logging;
  }

  public static ILoggingBuilder AddConsoleLogger(this ILoggingBuilder logging, string? fileLoggerPath = null) {
    logging.ClearProviders();
    logging.AddSimpleConsoleLogger();

    if (fileLoggerPath != null)
      logging.AddFileLogger(fileLoggerPath);

    return logging;
  }

  public static ILoggingBuilder AddJsonConsoleLogger(this ILoggingBuilder logging, string? fileLoggerPath = null) {
    logging.ClearProviders();
    logging.AddJsonConsoleLogger();
    if (fileLoggerPath != null)
      logging.AddJsonFileLogger(fileLoggerPath);
    return logging;
  }
}
