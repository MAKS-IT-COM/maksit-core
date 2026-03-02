using System.Linq.Expressions;


namespace MaksIT.Core.Extensions;

/// <summary>
/// Extension methods for combining and negating expression predicates.
/// AndAlso and OrElse use parameter replacement (no Expression.Invoke), so the result
/// is safe for use with IQueryable and EF Core (translatable to SQL).
/// </summary>
public static class ExpressionExtensions {

  /// <summary>
  /// Combines two predicates with AND. Uses a single parameter and Expression.AndAlso;
  /// safe for IQueryable/EF Core (no Invoke).
  /// </summary>
  public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second) {
    ArgumentNullException.ThrowIfNull(first);
    ArgumentNullException.ThrowIfNull(second);

    var parameter = first.Parameters[0];
    var visitor = new SubstituteParameterVisitor(second.Parameters[0], parameter);
    var secondBody = visitor.Visit(second.Body);
    var combinedBody = Expression.AndAlso(first.Body, secondBody);

    return Expression.Lambda<Func<T, bool>>(combinedBody, parameter);
  }

  /// <summary>
  /// Combines two predicates with OR. Uses a single parameter and Expression.OrElse;
  /// safe for IQueryable/EF Core (no Invoke).
  /// </summary>
  public static Expression<Func<T, bool>> OrElse<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second) {
    ArgumentNullException.ThrowIfNull(first);
    ArgumentNullException.ThrowIfNull(second);

    var parameter = first.Parameters[0];
    var visitor = new SubstituteParameterVisitor(second.Parameters[0], parameter);
    var secondBody = visitor.Visit(second.Body);
    var combinedBody = Expression.OrElse(first.Body, secondBody);

    return Expression.Lambda<Func<T, bool>>(combinedBody, parameter);
  }

  public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expression) {
    ArgumentNullException.ThrowIfNull(expression);

    var parameter = expression.Parameters[0];
    var body = Expression.Not(expression.Body);

    return Expression.Lambda<Func<T, bool>>(body, parameter);
  }

  private class SubstituteParameterVisitor : ExpressionVisitor {
    private readonly ParameterExpression _oldParameter;
    private readonly ParameterExpression _newParameter;

    public SubstituteParameterVisitor(ParameterExpression oldParameter, ParameterExpression newParameter) {
      _oldParameter = oldParameter;
      _newParameter = newParameter;
    }

    protected override Expression VisitParameter(ParameterExpression node) {
      // Replace old parameter with the new one
      return node == _oldParameter ? _newParameter : base.VisitParameter(node);
    }
  }

  public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> source, int batchSize) {
    var batch = new List<T>(batchSize);
    foreach (var item in source) {
      batch.Add(item);
      if (batch.Count == batchSize) {
        yield return batch;
        batch = new List<T>(batchSize);
      }
    }
    if (batch.Any()) {
      yield return batch;
    }
  }



}
