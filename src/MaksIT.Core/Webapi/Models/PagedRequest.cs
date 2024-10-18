using System.Linq.Expressions;
using MaksIT.Core.Extensions;
using MaksIT.Core.Abstractions.Webapi;

namespace MaksIT.Core.Webapi.Models {
  public class PagedRequest : RequestModelBase {
    public int PageSize { get; set; } = 100;
    public int PageNumber { get; set; } = 1;

    public string? Filters { get; set; }
    public Dictionary<string, string>? CollectionFilters { get; set; }

    public string? SortBy { get; set; }
    public bool IsAscending { get; set; } = true;

    public Expression<Func<T, bool>>? BuildCollectionFilterExpression<T>(string collectionName) {
      Expression<Func<T, bool>>? globalFilterExpression = null;
      Expression<Func<T, bool>>? collectionFilterExpression = null;

      if (!string.IsNullOrEmpty(Filters)) {
        globalFilterExpression = BuildFilterExpression<T>(Filters);
      }

      if (CollectionFilters != null && CollectionFilters.TryGetValue(collectionName, out var collectionFilter) && !string.IsNullOrEmpty(collectionFilter)) {
        collectionFilterExpression = BuildFilterExpression<T>(collectionFilter);
      }

      if (globalFilterExpression == null && collectionFilterExpression == null) {
        return null;
      }

      if (globalFilterExpression != null && collectionFilterExpression != null) {
        return globalFilterExpression.CombineWith(collectionFilterExpression);
      }

      return globalFilterExpression ?? collectionFilterExpression;
    }

    public Expression<Func<T, bool>>? BuildFilterExpression<T>(string filter) {
      if (string.IsNullOrEmpty(filter)) {
        return null;
      }

      var parameter = Expression.Parameter(typeof(T), "x");
      Expression? combinedExpression = null;

      // Split filters based on && and || operators
      var tokens = filter.Split(new[] { "&&", "||" }, StringSplitOptions.None);
      var operators = new List<string>();

      // Extract operators (&&, ||) from the original filter string
      int lastIndex = 0;
      for (int i = 0; i < filter.Length; i++) {
        if (filter.Substring(i).StartsWith("&&")) {
          operators.Add("&&");
          lastIndex = i + 2;
        }
        else if (filter.Substring(i).StartsWith("||")) {
          operators.Add("||");
          lastIndex = i + 2;
        }
      }

      for (int i = 0; i < tokens.Length; i++) {
        string processedFilter = tokens[i].Trim();
        bool isNegated = false;

        // Check for '!' at the beginning for negation
        if (processedFilter.StartsWith("!")) {
          isNegated = true;
          processedFilter = processedFilter.Substring(1).Trim(); // Remove '!' and trim
        }

        Expression? expression = null;

        if (processedFilter.Contains("!=")) {
          var parts = processedFilter.Split(new[] { "!=" }, StringSplitOptions.None);
          expression = BuildEqualityExpression(parameter, parts[0], parts[1], isNegated: true);
        }
        else if (processedFilter.Contains('=')) {
          var parts = processedFilter.Split('=');
          expression = BuildEqualityExpression(parameter, parts[0], parts[1], isNegated: false);
        }
        else if (processedFilter.Contains('>') || processedFilter.Contains('<')) {
          // Handle comparison (>, <, >=, <=)
          expression = BuildComparisonExpression(processedFilter, parameter);
        }

        // Apply negation if '!' was found at the beginning
        if (isNegated && expression != null) {
          expression = Expression.Not(expression);
        }

        // Only combine expressions if the new expression is not null
        if (expression != null) {
          if (combinedExpression == null) {
            combinedExpression = expression;
          }
          else if (i - 1 < operators.Count) // Ensure we don't exceed the operators list size
          {
            var operatorType = operators[i - 1];
            combinedExpression = operatorType == "&&"
                ? Expression.AndAlso(combinedExpression, expression)
                : Expression.OrElse(combinedExpression, expression);
          }
        }
      }

      return combinedExpression != null
          ? Expression.Lambda<Func<T, bool>>(combinedExpression, parameter)
          : null;
    }

    private static Expression BuildEqualityExpression(ParameterExpression parameter, string propertyName, string value, bool isNegated) {
      var property = Expression.Property(parameter, propertyName.Trim());
      var constant = Expression.Constant(ConvertValue(property.Type, value.Trim().Replace("'", "")));

      return isNegated
          ? Expression.NotEqual(property, constant)
          : Expression.Equal(property, constant);
    }

    private static Expression? BuildComparisonExpression(string subFilter, ParameterExpression parameter) {
      var comparisonType = subFilter.Contains(">=")
        ? ">="
        : subFilter.Contains("<=")
          ? "<="
          : subFilter.Contains(">")
            ? ">"
            : "<";

      var parts = subFilter.Split(new[] { '>', '<', '=', ' ' }, StringSplitOptions.RemoveEmptyEntries);
      var propertyName = parts[0].Trim();
      var value = parts[1].Trim().Replace("'", "");

      var property = Expression.Property(parameter, propertyName);
      var constant = Expression.Constant(ConvertValue(property.Type, value));

      return comparisonType switch {
        ">" => Expression.GreaterThan(property, constant),
        "<" => Expression.LessThan(property, constant),
        ">=" => Expression.GreaterThanOrEqual(property, constant),
        "<=" => Expression.LessThanOrEqual(property, constant),
        _ => null
      };
    }

    private static object ConvertValue(Type type, string value) {
      return type switch {
        var t when t == typeof(int) => int.Parse(value),
        var t when t == typeof(bool) => bool.Parse(value),
        var t when t == typeof(DateTime) => DateTime.Parse(value),
        _ => value
      };
    }
  }
}
