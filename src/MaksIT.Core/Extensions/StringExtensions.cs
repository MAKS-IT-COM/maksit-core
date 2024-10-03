using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace MaksIT.Core.Extensions {
  public static partial class StringExtensions {
    /// <summary>
    /// SQL Like implementation using wildcard patterns.
    /// </summary>
    public static bool Like(this string? text, string? wildcardedText) {
      if (text is null || wildcardedText is null) return false;

      return Regex.IsMatch(text, wildcardedText.WildcardToRegular(), RegexOptions.IgnoreCase | RegexOptions.Multiline);
    }

    /// <summary>
    /// Converts a wildcarded string to a regular expression.
    /// </summary>
    private static string WildcardToRegular(this string value) =>
        $"^{Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*")}$";

    /// <summary>
    /// Returns the left substring of the specified length.
    /// </summary>
    public static string Left(this string s, int count) =>
        s.Substring(0, Math.Min(count, s.Length));

    /// <summary>
    /// Returns the right substring of the specified length.
    /// </summary>
    public static string Right(this string s, int count) =>
        s.Substring(Math.Max(0, s.Length - count));

    /// <summary>
    /// Returns a substring starting from the specified index with the specified length.
    /// </summary>
    public static string Mid(this string s, int index, int count) =>
        s.Substring(index, Math.Min(count, s.Length - index));

    /// <summary>
    /// Converts the string to an integer, returning zero if conversion fails.
    /// </summary>
    public static int ToInteger(this string s) =>
        int.TryParse(s, out var integerValue) ? integerValue : 0;

    /// <summary>
    /// Determines whether the string represents an integer.
    /// </summary>
    public static bool IsInteger(this string s) => Regex.IsMatch(s, @"^-?\d+$");

    public static StringBuilder Prepend(this StringBuilder sb, string content) => sb.Insert(0, content);

    public static T ToEnum<T>(this string input) where T : struct {
      if (string.IsNullOrWhiteSpace(input))
        throw new ArgumentException("Input cannot be null or empty.", nameof(input));

      if (Enum.TryParse(input, true, out T result))
        return result;

      var enumType = typeof(T);

      foreach (T enumItem in Enum.GetValues(enumType)) {
        var att = enumType.GetMember(enumItem.ToString() ?? string.Empty)[0]
            .GetCustomAttributes(typeof(DisplayAttribute), false)
            .SingleOrDefault() as DisplayAttribute;

        var displayName = att?.GetName();

        if (input.Equals(displayName, StringComparison.InvariantCultureIgnoreCase))
          return enumItem;
      }

      throw new NotSupportedException($"Cannot parse the value '{input}' for {enumType}");
    }

    public static T? ToNullableEnum<T>(this string input) where T : struct =>
        !string.IsNullOrWhiteSpace(input) ? input.ToEnum<T>() : null;

    public static string? ToNull(this string s) => string.IsNullOrWhiteSpace(s) ? null : s;

    public static string? NullIfEmptyString(this string s) => s.ToNull();

    public static long? ToLong(this string s) =>
        long.TryParse(s, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var result) ? result : (long?)null;

    public static long? ToNullableLong(this string s) => string.IsNullOrWhiteSpace(s) ? (long?)null : s.ToLong();

    public static int? ToInt(this string s) =>
        int.TryParse(s, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var result) ? result : (int?)null;

    public static int? ToNullableInt(this string s) => string.IsNullOrWhiteSpace(s) ? (int?)null : s.ToInt();

    public static uint? ToUint(this string s) =>
        uint.TryParse(s, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var result) ? result : (uint?)null;

    public static uint? ToNullableUint(this string s) => string.IsNullOrWhiteSpace(s) ? (uint?)null : s.ToUint();

    public static decimal? ToDecimal(this string s) =>
        decimal.TryParse(s, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var result) ? result : (decimal?)null;

    public static decimal? ToNullableDecimal(this string s) => string.IsNullOrWhiteSpace(s) ? (decimal?)null : s.ToDecimal();

    public static double? ToDouble(this string s) =>
        double.TryParse(s, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var result) ? result : (double?)null;

    public static double? ToNullableDouble(this string s) => string.IsNullOrWhiteSpace(s) ? (double?)null : s.ToDouble();

    #region DateTime
    public static DateTime ToDate(this string s, string[] formats) =>
        DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var datetime)
            ? DateTime.SpecifyKind(datetime, DateTimeKind.Utc)
            : throw new FormatException($"The date [{s}] is not in the required format: [{formats[0]}]");

    public static DateTime ToDate(this string s) => s.ToDate(new[] { "dd/MM/yyyy" });

    public static DateTime? ToNullableDate(this string s) => string.IsNullOrEmpty(s) ? (DateTime?)null : s.ToDate();

    public static DateTime? ToNullableDate(this string s, string[] formats) => string.IsNullOrEmpty(s) ? (DateTime?)null : s.ToDate(formats);

    public static DateTime ToDateTime(this string s, string[] formats) {
      if (s.Equals("Now", StringComparison.OrdinalIgnoreCase)) return DateTime.Now;
      if (s.Equals("UtcNow", StringComparison.OrdinalIgnoreCase)) return DateTime.UtcNow;
      if (s.Equals("Today", StringComparison.OrdinalIgnoreCase)) return DateTime.Today;

      return DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
          ? DateTime.SpecifyKind(result, DateTimeKind.Utc)
          : throw new FormatException($"Unable to parse exact date from value [{s}] with formats [{string.Join(", ", formats)}]");
    }

    public static DateTime ToDateTime(this string s) => s.ToDateTime(new[] { "dd/MM/yyyy", "dd/MM/yyyy HH:mm:ss", "dd/MM/yyyy HH:mm", "yyyy-MM-dd'T'HH:mm:ss'Z'" });

    public static DateTime? ToNullableDateTime(this string s) => string.IsNullOrWhiteSpace(s) ? (DateTime?)null : s.ToDateTime();

    public static DateTime? ToNullableDateTime(this string s, string[] formats) => string.IsNullOrWhiteSpace(s) ? (DateTime?)null : s.ToDateTime(formats);
    #endregion

    public static bool ToBool(this string s) =>
        new[] { "ok", "yes", "y", "true", "1" }.Contains(s, StringComparer.InvariantCultureIgnoreCase);

    public static bool? ToNullableBool(this string s) => string.IsNullOrWhiteSpace(s) ? (bool?)null : s.ToBool();

    public static Guid ToGuid(this string text) =>
        Guid.TryParse(text, out var value) ? value : new Guid(MD5.Create().ComputeHash(Encoding.Default.GetBytes(text.ToUpper())));

    public static Guid? ToNullableGuid(this string s) => string.IsNullOrWhiteSpace(s) ? (Guid?)null : s.ToGuid();

    public static string[] StringSplit(this string s, char c) =>
        s.Split(c).Select(x => x.Trim()).ToArray();

    public static string ToTitle(this string s) => string.IsNullOrWhiteSpace(s) ? s : char.ToUpper(s[0]) + s[1..];

    [GeneratedRegex(@"(http|ftp|https):\\/\\/([\\w_-]+(?:(?:\\.[\\w_-]+)+))([\\w.,@?^=%&:\\/~+#-]*[\\w@?^=%&\\/~+#-])", RegexOptions.Compiled)]
    private static partial Regex UrlsRegex();

    public static IEnumerable<Uri> ExtractUrls(this string s) =>
        UrlsRegex().Matches(s).Cast<Match>()
        .Select(match => match.Value)
        .Where(url => Uri.TryCreate(url, UriKind.Absolute, out _))
        .Select(url => new Uri(url))
        .Distinct();

    public static string Format(this string s, params object[] args) => string.Format(s, args);

    public static string Excerpt(this string s, int length = 60) =>
        string.IsNullOrWhiteSpace(s) ? s : s.Length <= length ? s : $"{s.Substring(0, length - 3)}...";

    public static T? ToObject<T>(this string s) => ToObjectCore<T>(s, null);

    public static T? ToObject<T>(this string s, List<JsonConverter> converters) => ToObjectCore<T>(s, converters);

    private static T? ToObjectCore<T>(string s, List<JsonConverter>? converters) {
      var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
      converters?.ForEach(x => options.Converters.Add(x));
      return JsonSerializer.Deserialize<T>(s, options);
    }

    public static bool IsValidEmail(this string? s) {
      if (s is null) return false;

      const string pattern = @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$";
      var regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(2));

      return regex.IsMatch(s);
    }

    public static string HtmlToPlainText(this string htmlCode) {
      if (string.IsNullOrEmpty(htmlCode))
        return htmlCode;

      var sb = new StringBuilder(htmlCode);

      // Remove new lines, tabs, and multiple spaces
      sb.Replace("\n", " ").Replace("\t", " ");
      sb = new StringBuilder(Regex.Replace(sb.ToString(), "\\s+", " "));

      // Remove <head> and <script> sections
      sb = new StringBuilder(Regex.Replace(sb.ToString(), "<head.*?</head>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline));
      sb = new StringBuilder(Regex.Replace(sb.ToString(), "<script.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline));

      // Replace HTML entities
      string[] oldWords = { "&nbsp;", "&amp;", "&quot;", "&lt;", "&gt;", "&reg;", "&copy;", "&bull;", "&trade;", "&#39;" };
      string[] newWords = { " ", "&", "\"", "<", ">", "®", "©", "•", "™", "'" };
      for (int i = 0; i < oldWords.Length; i++) sb.Replace(oldWords[i], newWords[i]);

      // Handle line breaks
      sb.Replace("<br>", "\n").Replace("<br ", "\n<br ").Replace("<p ", "\n<p ");

      // Remove all HTML tags
      var plainText = Regex.Replace(sb.ToString(), "<[^>]*>", "").Trim();

      return plainText;
    }

    public static string ToCamelCase(this string input) {
      if (string.IsNullOrEmpty(input)) return input;

      var words = input.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
      for (var i = 0; i < words.Length; i++) {
        words[i] = i == 0 ? words[i].ToLower() : char.ToUpper(words[i][0]) + words[i][1..].ToLower();
      }

      return string.Join("", words);
    }

    public static DataTable CSVToDataTable(this string filePath) {
      if (string.IsNullOrEmpty(filePath))
        throw new ArgumentNullException(nameof(filePath));

      using var sr = new StreamReader(filePath);

      var headerLine = sr.ReadLine();
      if (headerLine == null)
        throw new InvalidOperationException("File is empty");

      var headers = headerLine.Split(',');
      var dt = new DataTable();
      foreach (var header in headers) {
        dt.Columns.Add(header);
      }

      while (!sr.EndOfStream) {
        var line = sr.ReadLine();
        if (line == null)
          continue;

        var rows = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
        var dr = dt.NewRow();

        for (var i = 0; i < headers.Length; i++) {
          dr[i] = i < rows.Length ? rows[i] : string.Empty;
        }
        dt.Rows.Add(dr);
      }

      return dt;
    }

    public static bool IsBase32String(this string input) {
      // Base32 characters are A-Z and 2-7
      foreach (char c in input) {
        if (!((c >= 'A' && c <= 'Z') || (c >= '2' && c <= '7'))) {
          return false;
        }
      }
      return true;
    }
  }
}
