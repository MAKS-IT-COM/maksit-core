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

    serviceCollection.AddLogging(builder => builder.AddFileLogger(_testFolderPath, TimeSpan.FromDays(7)));

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

    serviceCollection.AddLogging(builder => builder.AddFileLogger(_testFolderPath, retentionPeriod));

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

    serviceCollection.AddLogging(builder => builder.AddFileLogger(_testFolderPath));

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

  [Fact]
  public void ShouldWriteLogsToSubfolderWhenFolderPrefixUsed() {
    // Arrange
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddSingleton<IHostEnvironment>(sp =>
        new TestHostEnvironment {
            EnvironmentName = Environments.Development,
            ApplicationName = "TestApp",
            ContentRootPath = Directory.GetCurrentDirectory()
        });

    serviceCollection.AddLogging(builder => builder.AddFileLogger(_testFolderPath, TimeSpan.FromDays(7)));

    var provider = serviceCollection.BuildServiceProvider();
    var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

    // Act - Create logger with Folder prefix
    var logger = loggerFactory.CreateLogger(LoggerPrefix.Folder.WithValue("Audit"));
    logger.LogInformation("Audit log message");

    // Assert
    var auditFolder = Path.Combine(_testFolderPath, "Audit");
    Assert.True(Directory.Exists(auditFolder), "Audit subfolder should be created");

    var logFile = Directory.GetFiles(auditFolder, "log_*.txt").FirstOrDefault();
    Assert.NotNull(logFile);
    var logContent = File.ReadAllText(logFile);
    Assert.Contains("Audit log message", logContent);
  }

  [Fact]
  public void ShouldWriteLogsToDefaultFolderWhenNoPrefixUsed() {
    // Arrange
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddSingleton<IHostEnvironment>(sp =>
        new TestHostEnvironment {
            EnvironmentName = Environments.Development,
            ApplicationName = "TestApp",
            ContentRootPath = Directory.GetCurrentDirectory()
        });

    serviceCollection.AddLogging(builder => builder.AddFileLogger(_testFolderPath, TimeSpan.FromDays(7)));

    var provider = serviceCollection.BuildServiceProvider();
    var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

    // Act - Create logger with full type name (simulating ILogger<T>)
    var logger = loggerFactory.CreateLogger("MyApp.Services.OrderService");
    logger.LogInformation("Order service log message");

    // Assert - Should NOT create subfolder for type names
    var logFile = Directory.GetFiles(_testFolderPath, "log_*.txt").FirstOrDefault();
    Assert.NotNull(logFile);
    var logContent = File.ReadAllText(logFile);
    Assert.Contains("Order service log message", logContent);
  }

  [Fact]
  public void ShouldHandleFolderPrefixWithSpaces() {
    // Arrange
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddSingleton<IHostEnvironment>(sp =>
        new TestHostEnvironment {
            EnvironmentName = Environments.Development,
            ApplicationName = "TestApp",
            ContentRootPath = Directory.GetCurrentDirectory()
        });

    serviceCollection.AddLogging(builder => builder.AddFileLogger(_testFolderPath, TimeSpan.FromDays(7)));

    var provider = serviceCollection.BuildServiceProvider();
    var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

    // Act
    var logger = loggerFactory.CreateLogger(LoggerPrefix.Folder.WithValue("My Custom Logs"));
    logger.LogInformation("Custom folder log message");

    // Assert
    var customFolder = Path.Combine(_testFolderPath, "My Custom Logs");
    Assert.True(Directory.Exists(customFolder), "Custom subfolder with spaces should be created");

    var logFile = Directory.GetFiles(customFolder, "log_*.txt").FirstOrDefault();
    Assert.NotNull(logFile);
    var logContent = File.ReadAllText(logFile);
    Assert.Contains("Custom folder log message", logContent);
  }

  [Fact]
  public void ShouldIgnoreEmptyFolderPrefix() {
    // Arrange
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddSingleton<IHostEnvironment>(sp =>
        new TestHostEnvironment {
            EnvironmentName = Environments.Development,
            ApplicationName = "TestApp",
            ContentRootPath = Directory.GetCurrentDirectory()
        });

    serviceCollection.AddLogging(builder => builder.AddFileLogger(_testFolderPath, TimeSpan.FromDays(7)));

    var provider = serviceCollection.BuildServiceProvider();
    var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

    // Act - Create logger with empty folder value
    var logger = loggerFactory.CreateLogger(LoggerPrefix.Folder.WithValue(""));
    logger.LogInformation("Empty folder prefix log message");

    // Assert - Should use default folder (not create empty subfolder)
    var logFile = Directory.GetFiles(_testFolderPath, "log_*.txt").FirstOrDefault();
    Assert.NotNull(logFile);
    var logContent = File.ReadAllText(logFile);
    Assert.Contains("Empty folder prefix log message", logContent);
  }

  [Fact]
  public void ShouldRecreateLogFolderIfDeleted() {
    // Arrange
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddSingleton<IHostEnvironment>(sp =>
        new TestHostEnvironment {
          EnvironmentName = Environments.Development,
          ApplicationName = "TestApp",
          ContentRootPath = Directory.GetCurrentDirectory()
        });

    serviceCollection.AddLogging(builder => builder.AddFileLogger(_testFolderPath, TimeSpan.FromDays(7)));

    var provider = serviceCollection.BuildServiceProvider();
    var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

    // Act - Create logger and write a log (folder is created)
    var logger = loggerFactory.CreateLogger(LoggerPrefix.Folder.WithValue("Audit"));
    logger.LogInformation("First log message");

    var auditFolder = Path.Combine(_testFolderPath, "Audit");
    Assert.True(Directory.Exists(auditFolder), "Audit subfolder should be created");

    // Delete the folder
    Directory.Delete(auditFolder, true);
    Assert.False(Directory.Exists(auditFolder), "Audit subfolder should be deleted");

    // Write another log, which should trigger folder recreation
    logger.LogInformation("Second log message after folder deletion");

    // Assert
    Assert.True(Directory.Exists(auditFolder), "Audit subfolder should be recreated");
    var logFile = Directory.GetFiles(auditFolder, "log_*.txt").FirstOrDefault();
    Assert.NotNull(logFile);
    var logContent = File.ReadAllText(logFile);
    Assert.Contains("Second log message after folder deletion", logContent);
  }
}
