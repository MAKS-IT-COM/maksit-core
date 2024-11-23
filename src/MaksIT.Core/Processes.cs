using MaksIT.Core.Extensions;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;


namespace MaksIT.Core;

/// <summary>
/// Main <c>CustomProcess</c> class.
/// Provide helper methods to Start and Kill processes.
/// <list type="bullet">
/// <item>
/// <term>Start</term>
/// <description>Starts new process</description>
/// </item>
/// <item>
/// <term>Kill</term>
/// <description>Kills processes by name</description>
/// </item>
/// </list>
/// </summary>
public static class Processes {
  /// <summary>
  /// Tries to start a new process.
  /// </summary>
  /// <param name="fileName">The name of the file to start.</param>
  /// <param name="arguments">The arguments to pass to the process.</param>
  /// <param name="timeout">The timeout in seconds to wait for the process to exit.</param>
  /// <param name="silent">If true, the process will be started without creating a window.</param>
  /// <param name="errorMessage">The error message if the operation fails.</param>
  /// <returns>True if the process started successfully; otherwise, false.</returns>
  public static bool TryStart(string fileName, string arguments, int timeout, bool silent, [NotNullWhen(false)] out string? errorMessage) {
    try {
      var processInfo = new ProcessStartInfo(fileName) {
        Arguments = arguments,
        UseShellExecute = !silent,
        CreateNoWindow = silent
      };

      using (var proc = new System.Diagnostics.Process { StartInfo = processInfo }) {
        proc.Start();
        if (timeout > 0) {
          proc.WaitForExit(timeout * 1000);
        }
        else {
          proc.WaitForExit();
        }
      }
      errorMessage = null;
      return true;
    }
    catch (Exception ex) {
      // Log the exception or handle it as needed
      errorMessage = ex.Message;
      return false;
    }
  }

  /// <summary>
  /// Tries to kill processes by name.
  /// </summary>
  /// <param name="process">Process name. Accepts wildcards '*' or '?'</param>
  /// <param name="errorMessage">The error message if the operation fails.</param>
  /// <returns>True if at least one process was killed successfully; otherwise, false.</returns>
  public static bool TryKill(string process, [NotNullWhen(false)] out string? errorMessage) {
    bool success = false;
    errorMessage = null;
    foreach (var proc in System.Diagnostics.Process.GetProcesses()) {
      try {
        if (proc.ProcessName.Like(process)) {
          proc.Kill();
          success = true;
        }
      }
      catch (Exception ex) {
        // Log the exception or handle it as needed
        errorMessage = ex.Message;
      }
    }
    return success;
  }
}
