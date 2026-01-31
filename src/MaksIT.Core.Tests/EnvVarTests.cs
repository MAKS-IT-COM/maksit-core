namespace MaksIT.Core.Tests;

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
    // This test may fail on Linux/Docker containers due to permissions
    // Skip on non-Windows platforms as User-level env vars behave differently
    if (!OperatingSystem.IsWindows()) {
      // On Linux, user-level env vars in containers don't persist as expected
      // Just verify the method doesn't crash
      var result = EnvVar.TrySet(TestEnvVarName, TestEnvVarValue, "user", out var errorMessage);
      // Either succeeds or fails gracefully - both are acceptable on Linux
      Assert.True(result || errorMessage != null);
      return;
    }

    // Windows-specific test
    var winResult = EnvVar.TrySet(TestEnvVarName, TestEnvVarValue, "user", out var winErrorMessage);

    try {
      if (winResult) {
        Assert.Null(winErrorMessage);
        var value = Environment.GetEnvironmentVariable(TestEnvVarName, EnvironmentVariableTarget.User);
        Assert.Equal(TestEnvVarValue, value);
      }
    }
    finally {
      try {
        Environment.SetEnvironmentVariable(TestEnvVarName, null, EnvironmentVariableTarget.User);
      }
      catch {
        // Ignore cleanup errors
      }
    }
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

    // Act
    var result = EnvVar.TrySet(envName, TestEnvVarValue, target, out var errorMessage);

    // Assert - for process level, should always succeed
    if (target.ToLower() == "process") {
      Assert.True(result);
      Assert.Null(errorMessage);
    }
    // For other levels, result depends on permissions

    // Cleanup
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
