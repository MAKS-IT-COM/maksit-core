using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MaksIT.Core.Logging;


namespace MaksIT.Core.Tests.Logging;

public class FileLoggerTests {
  private readonly string _testFolderPath;

  public FileLoggerTests() {
    _testFolderPath = Path.Combine(Path.GetTempPath(), "FileLoggerTests");
    if (Directory.Exists(_testFolderPath)) {
      Directory.Delete(_testFolderPath, true);
    }
    Directory.CreateDirectory(_testFolderPath);
  }

  [Fact]
  public void ShouldWriteLogsToCorrectFile() {
    // Arrange
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddSingleton<IHostEnvironment>(sp =>
        new TestHostEnvironment {
            EnvironmentName = Environments.Development,
            ApplicationName = "TestApp",
            ContentRootPath = Directory.GetCurrentDirectory()
        });

    serviceCollection.AddLogging(builder => builder.AddFile(_testFolderPath, TimeSpan.FromDays(7)));

    var provider = serviceCollection.BuildServiceProvider();
    var logger = provider.GetRequiredService<ILogger<FileLoggerTests>>();

    // Act
    logger.LogInformation("Test log message");

    // Assert
    var logFile = Directory.GetFiles(_testFolderPath, "log_*.txt").FirstOrDefault();
    Assert.NotNull(logFile);
    var logContent = File.ReadAllText(logFile);
    Assert.Contains("Test log message", logContent);
  }

  [Fact]
  public void ShouldDeleteOldLogsBasedOnRetention() {
    // Arrange
    var retentionPeriod = TimeSpan.FromDays(1);
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddSingleton<IHostEnvironment>(sp =>
        new TestHostEnvironment {
            EnvironmentName = Environments.Development,
            ApplicationName = "TestApp",
            ContentRootPath = Directory.GetCurrentDirectory()
        });

    serviceCollection.AddLogging(builder => builder.AddFile(_testFolderPath, retentionPeriod));

    var provider = serviceCollection.BuildServiceProvider();
    var logger = provider.GetRequiredService<ILogger<FileLoggerTests>>();

    // Create an old log file
    var oldLogFile = Path.Combine(_testFolderPath, $"log_{DateTime.Now.AddDays(-2):yyyy-MM-dd}.txt");
    File.WriteAllText(oldLogFile, "Old log");

    // Act
    logger.LogInformation("New log message");

    // Assert
    Assert.False(File.Exists(oldLogFile), "Old log file should have been deleted.");
    var logFile = Directory.GetFiles(_testFolderPath, "log_*.txt").FirstOrDefault();
    Assert.NotNull(logFile);
    var logContent = File.ReadAllText(logFile);
    Assert.Contains("New log message", logContent);
  }

  [Fact]
  public void ShouldHandleExceptionsGracefully() {
    // Arrange
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddSingleton<IHostEnvironment>(sp =>
        new TestHostEnvironment {
            EnvironmentName = Environments.Development,
            ApplicationName = "TestApp",
            ContentRootPath = Directory.GetCurrentDirectory()
        });

    serviceCollection.AddLogging(builder => builder.AddFile(_testFolderPath));

    var provider = serviceCollection.BuildServiceProvider();
    var logger = provider.GetRequiredService<ILogger<FileLoggerTests>>();

    // Act & Assert
    try {
      logger.LogError(new InvalidOperationException("Test exception"), "An error occurred");
      var logFile = Directory.GetFiles(_testFolderPath, "log_*.txt").FirstOrDefault();
      Assert.NotNull(logFile);
      var logContent = File.ReadAllText(logFile);
      Assert.Contains("An error occurred", logContent);
      Assert.Contains("Test exception", logContent);
    } catch {
      Assert.Fail("Logger should handle exceptions gracefully.");
    }
  }
}
