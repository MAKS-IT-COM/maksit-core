using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using MaksIT.Core.Abstractions.Webapi;

public class PagedRequest : RequestModelBase {
  public int PageSize { get; set; } = 100;
  public int PageNumber { get; set; } = 1;
  public string? Filters { get; set; }

  public string? SortBy { get; set; }
  public bool IsAscending { get; set; } = true;

  public Expression<Func<T, bool>> BuildFilterExpression<T>() {
    if (string.IsNullOrWhiteSpace(Filters))
      return x => true; // Returns an expression that doesn't filter anything.

    // Parse the filter string into a dynamic lambda expression.
    var predicate = DynamicExpressionParser.ParseLambda<T, bool>(
        new ParsingConfig(), false, Filters);
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
