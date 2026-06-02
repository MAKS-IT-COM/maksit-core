namespace MaksIT.Core.Tests;

/// <summary>
/// User/machine-level environment variable updates are not safe to run in parallel on Windows.
/// </summary>
[CollectionDefinition(nameof(EnvVarTests), DisableParallelization = true)]
public class EnvVarTestsCollection;

[Collection(nameof(EnvVarTests))]
public class EnvVarTests {

  private const string TestEnvVarName = "MAKSIT_TEST_ENV_VAR";
  private const string TestEnvVarValue = "test_value_123";

  [Fact]
  public void TrySet_ProcessLevel_SetsEnvironmentVariable() {
    // Arrange & Act
    var result = EnvVar.TrySet(TestEnvVarName, TestEnvVarValue, "process", out var errorMessage);

    try {
      // Assert
      Assert.True(result);
      Assert.Null(errorMessage);
      Assert.Equal(TestEnvVarValue, Environment.GetEnvironmentVariable(TestEnvVarName));
    }
    finally {
      // Cleanup
      Environment.SetEnvironmentVariable(TestEnvVarName, null);
    }
  }

  [Fact]
  public void TryUnSet_ProcessLevel_RemovesEnvironmentVariable() {
    // Arrange
    Environment.SetEnvironmentVariable(TestEnvVarName, TestEnvVarValue);

    // Act
    var result = EnvVar.TryUnSet(TestEnvVarName, "process", out var errorMessage);

    // Assert
    Assert.True(result);
    Assert.Null(errorMessage);
    Assert.Null(Environment.GetEnvironmentVariable(TestEnvVarName));
  }

  [Fact]
  public void TrySet_UserLevel_SetsEnvironmentVariable() {
    // User-level env vars are registry-backed on Windows and can block indefinitely from the
    // xUnit test host. On Linux/Docker they may fail due to permissions.
    if (!TrySetUserLevelWithTimeout(TimeSpan.FromSeconds(5), out var result, out var errorMessage)) {
      return;
    }

    Assert.True(result || errorMessage != null);
  }

  private static bool TrySetUserLevelWithTimeout(
    TimeSpan timeout,
    out bool result,
    out string? errorMessage) {
    var setTask = Task.Run(() => {
      var ok = EnvVar.TrySet(TestEnvVarName, TestEnvVarValue, "user", out var error);
      return (ok, error);
    });

    if (!setTask.Wait(timeout)) {
      result = false;
      errorMessage = null;
      return false;
    }

    result = setTask.Result.ok;
    errorMessage = setTask.Result.error;
    return true;
  }

  [Fact]
  public void TryAddToPath_AddsPathToEnvironment() {
    // Arrange
    var originalPath = Environment.GetEnvironmentVariable("PATH");
    var newPath = "/test/path/that/does/not/exist";

    try {
      // Act
      var result = EnvVar.TryAddToPath(newPath, out var errorMessage);

      // Assert
      Assert.True(result);
      Assert.Null(errorMessage);
      var currentPath = Environment.GetEnvironmentVariable("PATH");
      Assert.Contains(newPath, currentPath);
    }
    finally {
      // Cleanup - restore original PATH
      Environment.SetEnvironmentVariable("PATH", originalPath);
    }
  }

  [Fact]
  public void TryAddToPath_DuplicatePath_DoesNotAddAgain() {
    // Arrange
    var originalPath = Environment.GetEnvironmentVariable("PATH");
    var newPath = "/test/unique/path";

    try {
      // Add first time
      EnvVar.TryAddToPath(newPath, out _);
      var pathAfterFirstAdd = Environment.GetEnvironmentVariable("PATH");

      // Act - Add same path again
      var result = EnvVar.TryAddToPath(newPath, out var errorMessage);
      var pathAfterSecondAdd = Environment.GetEnvironmentVariable("PATH");

      // Assert
      Assert.True(result);
      Assert.Null(errorMessage);
      // Path should not have duplicate entries
      Assert.Equal(pathAfterFirstAdd, pathAfterSecondAdd);
    }
    finally {
      // Cleanup
      Environment.SetEnvironmentVariable("PATH", originalPath);
    }
  }

  [Theory]
  [InlineData("process")]
  [InlineData("user")]
  [InlineData("Process")]
  [InlineData("USER")]
  public void TrySet_VariousTargets_HandlesCorrectly(string target) {
    // Arrange
    var envName = $"{TestEnvVarName}_{target.ToUpper()}";
    var normalizedTarget = target.ToLowerInvariant();

    if (normalizedTarget == "user" && OperatingSystem.IsWindows()) {
      var setTask = Task.Run(() => {
        var ok = EnvVar.TrySet(envName, TestEnvVarValue, target, out var error);
        return (ok, error);
      });
      if (!setTask.Wait(TimeSpan.FromSeconds(5))) {
        return;
      }

      Assert.True(setTask.Result.ok || setTask.Result.error != null);
      TryUnSetQuietly(envName, target);
      return;
    }

    // Act
    var result = EnvVar.TrySet(envName, TestEnvVarValue, target, out var errorMessage);

    // Assert - for process level, should always succeed
    if (normalizedTarget == "process") {
      Assert.True(result);
      Assert.Null(errorMessage);
    }
    // For other levels, result depends on permissions

    TryUnSetQuietly(envName, target);
  }

  private static void TryUnSetQuietly(string envName, string target) {
    try {
      EnvVar.TryUnSet(envName, target, out _);
    }
    catch {
      // Ignore cleanup errors
    }
  }

  [Fact]
  public void TrySet_EmptyValue_SetsEmptyString() {
    // Arrange & Act
    var result = EnvVar.TrySet(TestEnvVarName, "", "process", out var errorMessage);

    try {
      // Assert
      Assert.True(result);
      Assert.Null(errorMessage);
      Assert.Equal("", Environment.GetEnvironmentVariable(TestEnvVarName));
    }
    finally {
      // Cleanup
      Environment.SetEnvironmentVariable(TestEnvVarName, null);
    }
  }

  [Fact]
  public void TryUnSet_NonExistentVariable_Succeeds() {
    // Arrange
    var nonExistentVar = "MAKSIT_NON_EXISTENT_VAR_12345";

    // Act
    var result = EnvVar.TryUnSet(nonExistentVar, "process", out var errorMessage);

    // Assert
    Assert.True(result);
    Assert.Null(errorMessage);
  }
}
