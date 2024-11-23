using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;


namespace MaksIT.Core;

/// <summary>
/// Allows to Set and Unset environment variables
/// </summary>
public static class EnvVar {
  private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

  /// <summary>
  /// Adds a new path to the PATH environment variable.
  /// </summary>
  /// <param name="newPath">The new path to add.</param>
  /// <param name="errorMessage">The error message if the operation fails.</param>
  /// <returns>True if the operation was successful; otherwise, false.</returns>
  public static bool TryAddToPath(string newPath, [NotNullWhen(false)] out string? errorMessage) {
    try {
      var pathEnvVar = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
      char separator = IsWindows ? ';' : ':';

      if (!pathEnvVar.Split(separator).Contains(newPath)) {
        pathEnvVar = pathEnvVar.TrimEnd(separator) + separator + newPath;
        Environment.SetEnvironmentVariable("PATH", pathEnvVar);
      }

      errorMessage = null;
      return true;
    }
    catch (Exception ex) {
      errorMessage = ex.Message;
      return false;
    }
  }

  /// <summary>
  /// Sets an environment variable.
  /// </summary>
  /// <param name="envName">The name of the environment variable.</param>
  /// <param name="envValue">The value of the environment variable.</param>
  /// <param name="envTarget">The target of the environment variable (machine, user, process).</param>
  /// <param name="errorMessage">The error message if the operation fails.</param>
  /// <returns>True if the operation was successful; otherwise, false.</returns>
  public static bool TrySet(string envName, string envValue, string envTarget, [NotNullWhen(false)] out string? errorMessage) {
    try {
      EnvironmentVariableTarget target = GetEnvironmentVariableTarget(envTarget);
      if (target == EnvironmentVariableTarget.Machine && !IsWindows) {
        throw new PlatformNotSupportedException("Setting machine-level environment variables is not supported on this platform.");
      }

      Environment.SetEnvironmentVariable(envName, envValue, target);
      errorMessage = null;
      return true;
    }
    catch (Exception ex) {
      errorMessage = ex.Message;
      return false;
    }
  }

  /// <summary>
  /// Unsets an environment variable.
  /// </summary>
  /// <param name="envName">The name of the environment variable.</param>
  /// <param name="envTarget">The target of the environment variable (machine, user, process).</param>
  /// <param name="errorMessage">The error message if the operation fails.</param>
  /// <returns>True if the operation was successful; otherwise, false.</returns>
  public static bool TryUnSet(string envName, string envTarget, [NotNullWhen(false)] out string? errorMessage) {
    try {
      EnvironmentVariableTarget target = GetEnvironmentVariableTarget(envTarget);
      if (target == EnvironmentVariableTarget.Machine && !IsWindows) {
        throw new PlatformNotSupportedException("Unsetting machine-level environment variables is not supported on this platform.");
      }

      Environment.SetEnvironmentVariable(envName, null, target);
      errorMessage = null;
      return true;
    }
    catch (Exception ex) {
      errorMessage = ex.Message;
      return false;
    }
  }

  private static EnvironmentVariableTarget GetEnvironmentVariableTarget(string envTarget) {
    return envTarget.ToLower() switch {
      "user" => EnvironmentVariableTarget.User,
      "process" => EnvironmentVariableTarget.Process,
      _ => EnvironmentVariableTarget.Machine,
    };
  }
}
