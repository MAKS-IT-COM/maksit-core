using MaksIT.Core.Comb;


namespace MaksIT.Core.Cli;

/// <summary>
/// Parses interactive menu input for the Core CLI.
/// </summary>
public static class InputParsers {
  /// <summary>
  /// Parses a positive integer, using <paramref name="defaultValue"/> when input is blank.
  /// </summary>
  public static bool TryParsePositiveInt(
    string? input,
    int defaultValue,
    out int value,
    out string? errorMessage
  ) {
    if (string.IsNullOrWhiteSpace(input)) {
      value = defaultValue;
      errorMessage = null;
      return true;
    }

    if (!int.TryParse(input.Trim(), out value) || value <= 0) {
      value = 0;
      errorMessage = "Value must be a positive integer.";
      return false;
    }

    errorMessage = null;
    return true;
  }

  /// <summary>
  /// Parses a COMB GUID type, defaulting to <see cref="CombGuidType.PostgreSql"/> when input is blank.
  /// </summary>
  public static bool TryParseCombGuidType(
    string? input,
    out CombGuidType type,
    out string? errorMessage
  ) {
    if (string.IsNullOrWhiteSpace(input)) {
      type = CombGuidType.PostgreSql;
      errorMessage = null;
      return true;
    }

    if (Enum.TryParse(input.Trim(), ignoreCase: true, out type)
        && Enum.IsDefined(type)) {
      errorMessage = null;
      return true;
    }

    type = default;
    errorMessage = "Type must be PostgreSql or SqlServer.";
    return false;
  }

  /// <summary>
  /// Splits a comma-separated list; returns <c>null</c> when input is blank.
  /// </summary>
  public static List<string>? ParseOptionalList(string? input) {
    if (string.IsNullOrWhiteSpace(input))
      return null;

    var items = input
      .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
      .ToList();

    if (items.Count == 0)
      return null;

    return items;
  }
}
