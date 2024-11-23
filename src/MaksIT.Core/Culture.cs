using System.Diagnostics.CodeAnalysis;
using System.Globalization;


namespace MaksIT.Core;

/// <summary>
/// The main <c>Cultures</c> class.
/// Contains all methods for performing basic Cultures management.
/// </summary>
public static class Culture {
  /// <summary>
  /// Sets the culture for the current thread.
  /// </summary>
  /// <param name="culture">The culture to set. If null or empty, the invariant culture is used.</param>
  /// <param name="errorMessage">The error message if the operation fails.</param>
  /// <returns>True if the operation was successful; otherwise, false.</returns>
  public static bool TrySet(string? culture, [NotNullWhen(false)] out string? errorMessage) {
    try {
      var threadCulture = CultureInfo.InvariantCulture;

      if (!string.IsNullOrEmpty(culture)) {
        threadCulture = CultureInfo.CreateSpecificCulture(culture);
      }

      Thread.CurrentThread.CurrentUICulture = threadCulture;
      Thread.CurrentThread.CurrentCulture = threadCulture;

      errorMessage = null;
      return true;
    }
    catch (Exception ex) {
      errorMessage = ex.Message;
      return false;
    }
  }
}

