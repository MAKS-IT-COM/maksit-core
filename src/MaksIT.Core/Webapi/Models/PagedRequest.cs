using System.Linq.Expressions;

using MaksIT.Core.Extensions;
using MaksIT.Core.Abstractions.Webapi;


namespace MaksIT.Core.Webapi.Models;

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
    var expressions = new List<Expression>();

    var filters = filter.Split(new[] { "AND", "OR" }, StringSplitOptions.None);

    foreach (var subFilter in filters) {
      Expression? expression = null;

      if (subFilter.Contains('=')) {
        var parts = subFilter.Split('=');
        var propertyName = parts[0].Trim();
        var value = parts[1].Trim().Replace("'", "");

        var property = Expression.Property(parameter, propertyName);
        var constant = Expression.Constant(ConvertValue(property.Type, value));

        expression = Expression.Equal(property, constant);
      }
      else if (subFilter.Contains('>') || subFilter.Contains('<')) {
        expression = BuildComparisonExpression(subFilter, parameter);
      }

      if (expression != null) {
        expressions.Add(expression);
      }
    }

    if (!expressions.Any()) {
      return null;
    }

    var combinedExpression = expressions.Aggregate(Expression.AndAlso);

    return Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
  }

  private static Expression? BuildComparisonExpression(string subFilter, ParameterExpression parameter) {
    var comparisonType = subFilter.Contains(">=") ? ">=" :
                         subFilter.Contains("<=") ? "<=" :
                         subFilter.Contains(">") ? ">" : "<";

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
