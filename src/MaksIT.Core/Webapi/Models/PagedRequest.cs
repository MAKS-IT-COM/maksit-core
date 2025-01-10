using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using MaksIT.Core.Abstractions.Webapi;
using MaksIT.Core.Extensions;

public class PagedRequest : RequestModelBase {
  public int PageSize { get; set; } = 100;
  public int PageNumber { get; set; } = 1;
  public string? Filters { get; set; }

  public string? SortBy { get; set; }
  public bool IsAscending { get; set; } = true;

  public Expression<Func<T, bool>> BuildFilterExpression<T>() {
    return BuildFilterExpression<T>(Filters);
  }

  public virtual Expression<Func<T, bool>> BuildFilterExpression<T>(string? filters) {
    if (string.IsNullOrWhiteSpace(filters))
      return x => true; // Returns an expression that doesn't filter anything.

    // Get the type of T
    var type = typeof(T);

    // Adjust Filters to make Contains, StartsWith, EndsWith, ==, and != case-insensitive
    string adjustedFilters = filters;

    // Regex to find property names and methods
    adjustedFilters = Regex.Replace(adjustedFilters, @"(\w+)\.(Contains|StartsWith|EndsWith)\(\""(.*?)\""\)", m => {
      var propertyName = m.Groups[1].Value.ToTitle();
      var method = m.Groups[2].Value;
      var value = m.Groups[3].Value;
      var property = type.GetProperty(propertyName);
      if (property != null && property.PropertyType == typeof(string)) {
        return $"{propertyName}.ToLower().{method}(\"{value.ToLower()}\")";
      }
      return m.Value;
    });

    // Regex to find equality and inequality comparisons
    adjustedFilters = Regex.Replace(adjustedFilters, @"(\w+)\s*(==|!=)\s*\""(.*?)\""", m => {
      var propertyName = m.Groups[1].Value.ToTitle();
      var comparison = m.Groups[2].Value;
      var value = m.Groups[3].Value;
      var property = type.GetProperty(propertyName);
      if (property != null && property.PropertyType == typeof(string)) {
        return $"{propertyName}.ToLower() {comparison} \"{value.ToLower()}\"";
      }
      else
        return $"{propertyName} {comparison} \"{value}\"";
    });

    // Parse the adjusted filter string into a dynamic lambda expression
    var predicate = DynamicExpressionParser.ParseLambda<T, bool>(
        new ParsingConfig(), false, adjustedFilters);

    return predicate;
  }



  public Func<IQueryable<T>, IOrderedQueryable<T>> BuildSortExpression<T>() {
    if (string.IsNullOrWhiteSpace(SortBy))
      return q => (IOrderedQueryable<T>)q; // Cast to IOrderedQueryable

    var direction = IsAscending ? "ascending" : "descending";
    // Return a Func that takes an IQueryable and applies the sorting to it.
    return q => (IOrderedQueryable<T>)q.OrderBy($"{SortBy} {direction}");
  }
}
