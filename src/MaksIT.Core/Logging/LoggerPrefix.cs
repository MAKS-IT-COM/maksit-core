using MaksIT.Core.Abstractions;

namespace MaksIT.Core.Logging;

public class LoggerPrefix : Enumeration {
  public static readonly LoggerPrefix Folder = new(1, "Folder:");
  public static readonly LoggerPrefix Category = new(2, "Category:");
  public static readonly LoggerPrefix Tag = new(3, "Tag:");

  private LoggerPrefix(int id, string name) : base(id, name) { }

  /// <summary>
  /// Creates a category string with this prefix and the given value.
  /// </summary>
  public string WithValue(string value) => $"{Name}{value}";

  /// <summary>
  /// Tries to extract the prefix and value from a category name.
  /// </summary>
  public static (LoggerPrefix? prefix, string? value) Parse(string categoryName) {
    foreach (var prefix in GetAll<LoggerPrefix>()) {
      if (categoryName.StartsWith(prefix.Name, StringComparison.Ordinal)) {
        var value = categoryName.Substring(prefix.Name.Length);
        return (prefix, value);
      }
    }
    return (null, null);
  }
}
