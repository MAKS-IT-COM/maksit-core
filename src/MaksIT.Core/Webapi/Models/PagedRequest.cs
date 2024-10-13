using System.Linq.Expressions;

using MaksIT.Core.Abstractions.Webapi;

namespace MaksIT.Core.Webapi.Models;
public class PagedRequest : RequestModelBase {
  public int PageSize { get; set; } = 100;
  public int PageNumber { get; set; } = 1;

  // Global filter applicable to any collection
  public string? Filters { get; set; }

  // Specific filters for different collections
  public Dictionary<string, string>? CollectionFilters { get; set; }

  // Optional sorting
  public string? SortBy { get; set; }
  public bool IsAscending { get; set; } = true;

  // Method to build an expression for a specific collection, combining global and collection-specific filters
  public Expression<Func<T, bool>> BuildCollectionFilterExpression<T>(string collectionName) {
    var expressions = new List<Expression>();

    var parameter = Expression.Parameter(typeof(T), "x");

    // Add global filters if available
    if (!string.IsNullOrEmpty(Filters)) {
      var globalFilterExpression = BuildFilterExpression<T>(Filters, parameter);
      expressions.Add(globalFilterExpression);
    }

    // Add collection-specific filters if available
    if (CollectionFilters != null && CollectionFilters.ContainsKey(collectionName) && !string.IsNullOrEmpty(CollectionFilters[collectionName])) {
      var collectionFilterExpression = BuildFilterExpression<T>(CollectionFilters[collectionName], parameter);
      expressions.Add(collectionFilterExpression);
    }

    // Combine the expressions using AND (you can extend to OR logic if needed)
    Expression combinedExpression;
    if (expressions.Any()) {
      combinedExpression = expressions.Aggregate(Expression.AndAlso);
    }
    else {
      // Default to 'true' if no filters are provided
      combinedExpression = Expression.Constant(true);
    }

    return Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
  }

  // Method to build a LINQ expression from a single filter string (global or collection-specific)
  public Expression<Func<T, bool>> BuildFilterExpression<T>(string filter, ParameterExpression? parameter = null) {
    if (string.IsNullOrEmpty(filter))
      return x => true; // Default to 'true' if no filters are provided

    parameter ??= Expression.Parameter(typeof(T), "x");
    var expressions = new List<Expression>();

    // Parse the filters string (this logic can be extended to support more complex filtering)
    var filters = filter.Split(new[] { "AND", "OR" }, StringSplitOptions.None);

    foreach (var subFilter in filters) {
      if (subFilter.Contains('=')) {
        var parts = subFilter.Split('=');
        var propertyName = parts[0].Trim();
        var value = parts[1].Trim().Replace("'", "");

        var property = Expression.Property(parameter, propertyName);
        var constant = Expression.Constant(ConvertValue(property.Type, value));

        expressions.Add(Expression.Equal(property, constant));
      }
      else if (subFilter.Contains('>') || subFilter.Contains('<')) {
        var comparisonType = subFilter.Contains(">=") ? ">=" :
                             subFilter.Contains("<=") ? "<=" :
                             subFilter.Contains(">") ? ">" : "<";
        var parts = subFilter.Split(new[] { '>', '<', '=', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var propertyName = parts[0].Trim();
        var value = parts[1].Trim().Replace("'", "");

        var property = Expression.Property(parameter, propertyName);
        var constant = Expression.Constant(ConvertValue(property.Type, value));

        switch (comparisonType) {
          case ">":
            expressions.Add(Expression.GreaterThan(property, constant));
            break;
          case "<":
            expressions.Add(Expression.LessThan(property, constant));
            break;
          case ">=":
            expressions.Add(Expression.GreaterThanOrEqual(property, constant));
            break;
          case "<=":
            expressions.Add(Expression.LessThanOrEqual(property, constant));
            break;
        }
      }
    }

    // Combine the expressions using AND (you can extend to support OR)
    var combinedExpression = expressions.Aggregate(Expression.AndAlso);

    return Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
  }

  // Helper method to convert the value to the correct type
  private static object ConvertValue(Type type, string value) {
    if (type == typeof(int))
      return int.Parse(value);
    if (type == typeof(bool))
      return bool.Parse(value);
    if (type == typeof(DateTime))
      return DateTime.Parse(value);

    return value; // Default to string
  }
}
