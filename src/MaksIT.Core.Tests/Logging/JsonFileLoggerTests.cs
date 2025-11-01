using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MaksIT.Core.Logging;


namespace MaksIT.Core.Tests.Logging;

public class JsonFileLoggerTests {
  private readonly string _testFolderPath;

  public JsonFileLoggerTests() {
    _testFolderPath = Path.Combine(Path.GetTempPath(), "JsonFileLoggerTests");
    if (Directory.Exists(_testFolderPath)) {
      Directory.Delete(_testFolderPath, true);
    }
    Directory.CreateDirectory(_testFolderPath);
  }

  [Fact]
  public void ShouldWriteLogsInJsonFormat() {
    // Arrange
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddSingleton<IHostEnvironment>(sp =>
        new TestHostEnvironment {
            EnvironmentName = Environments.Development,
            ApplicationName = "TestApp",
            ContentRootPath = Directory.GetCurrentDirectory()
        });

    serviceCollection.AddLogging(builder => builder.AddJsonFileLogger(_testFolderPath, TimeSpan.FromDays(7)));

    var provider = serviceCollection.BuildServiceProvider();
    var logger = provider.GetRequiredService<ILogger<JsonFileLoggerTests>>();

    // Act
    logger.LogInformation("Test JSON log message");

    // Assert
    var logFile = Directory.GetFiles(_testFolderPath, "log_*.json").FirstOrDefault();
    Assert.NotNull(logFile);
    var logContent = File.ReadAllText(logFile);
    Assert.Contains("Test JSON log message", logContent);

    var logEntry = JsonSerializer.Deserialize<JsonElement>(logContent.TrimEnd(','));
    Assert.Equal("Information", logEntry.GetProperty("LogLevel").GetString());
    Assert.Equal("Test JSON log message", logEntry.GetProperty("Message").GetString());
  }

  [Fact]
  public void ShouldDeleteOldJsonLogsBasedOnRetention() {
    // Arrange
    var retentionPeriod = TimeSpan.FromDays(1);
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddSingleton<IHostEnvironment>(sp =>
        new TestHostEnvironment {
            EnvironmentName = Environments.Development,
            ApplicationName = "TestApp",
            ContentRootPath = Directory.GetCurrentDirectory()
        });

    serviceCollection.AddLogging(builder => builder.AddJsonFileLogger(_testFolderPath, retentionPeriod));

    var provider = serviceCollection.BuildServiceProvider();
    var logger = provider.GetRequiredService<ILogger<JsonFileLoggerTests>>();

    // Create an old log file
    var oldLogFile = Path.Combine(_testFolderPath, $"log_{DateTime.Now.AddDays(-2):yyyy-MM-dd}.json");
    File.WriteAllText(oldLogFile, "{\"Message\":\"Old log\"}");

    // Act
    logger.LogInformation("New JSON log message");

    // Assert
    Assert.False(File.Exists(oldLogFile), "Old JSON log file should have been deleted.");
    var logFile = Directory.GetFiles(_testFolderPath, "log_*.json").FirstOrDefault();
    Assert.NotNull(logFile);
    var logContent = File.ReadAllText(logFile);
    Assert.Contains("New JSON log message", logContent);
  }

  [Fact]
  public void ShouldLogExceptionsInJsonFormat() {
    // Arrange
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddSingleton<IHostEnvironment>(sp =>
        new TestHostEnvironment {
            EnvironmentName = Environments.Development,
            ApplicationName = "TestApp",
            ContentRootPath = Directory.GetCurrentDirectory()
        });

    serviceCollection.AddLogging(builder => builder.AddJsonFileLogger(_testFolderPath, TimeSpan.FromDays(7)));

    var provider = serviceCollection.BuildServiceProvider();
    var logger = provider.GetRequiredService<ILogger<JsonFileLoggerTests>>();

    // Act
    logger.LogError(new InvalidOperationException("Test exception"), "An error occurred");

    // Assert
    var logFile = Directory.GetFiles(_testFolderPath, "log_*.json").FirstOrDefault();
    Assert.NotNull(logFile);
    var logContent = File.ReadAllText(logFile);
    Assert.Contains("An error occurred", logContent);
    Assert.Contains("Test exception", logContent);

    var logEntry = JsonSerializer.Deserialize<JsonElement>(logContent.TrimEnd(','));
    Assert.Equal("Error", logEntry.GetProperty("LogLevel").GetString());
    Assert.Equal("An error occurred", logEntry.GetProperty("Message").GetString());
    Assert.Contains("Test exception", logEntry.GetProperty("Exception").GetString());
  }

  [Fact]
  public void ShouldWorkWithConsoleLogger() {
    // Arrange
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddSingleton<IHostEnvironment>(sp =>
        new TestHostEnvironment {
            EnvironmentName = Environments.Development,
            ApplicationName = "TestApp",
            ContentRootPath = Directory.GetCurrentDirectory()
        });

    serviceCollection.AddLogging(builder => {
      builder.AddJsonFileLogger(_testFolderPath, TimeSpan.FromDays(7));
      builder.AddSimpleConsoleLogger();
    });

    var provider = serviceCollection.BuildServiceProvider();
    var logger = provider.GetRequiredService<ILogger<JsonFileLoggerTests>>();

    // Act
    logger.LogInformation("Test combined logging");

    // Assert
    var logFile = Directory.GetFiles(_testFolderPath, "log_*.json").FirstOrDefault();
    Assert.NotNull(logFile);
    var logContent = File.ReadAllText(logFile);
    Assert.Contains("Test combined logging", logContent);
  }
}