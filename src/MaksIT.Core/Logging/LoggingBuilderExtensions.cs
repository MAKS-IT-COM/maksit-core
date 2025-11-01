using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace MaksIT.Core.Logging;

public static class LoggingBuilderExtensions {
  public static ILoggingBuilder AddFile(this ILoggingBuilder builder, string folderPath, TimeSpan? retentionPeriod = null) {
    builder.Services.AddSingleton<ILoggerProvider>(new FileLoggerProvider(folderPath, retentionPeriod));
    return builder;
  }
  public static ILoggingBuilder AddConsole(this ILoggingBuilder logging, IHostEnvironment env) {
    logging.ClearProviders();
    if (env.IsDevelopment()) {
      logging.AddSimpleConsole(options => {
        options.IncludeScopes = true;
        options.SingleLine = false;
        options.TimestampFormat = "hh:mm:ss ";
      });
    }
    else {
      logging.AddJsonConsole(options => {
        options.IncludeScopes = true;
        options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
      });
    }
    return logging;
  }
}
