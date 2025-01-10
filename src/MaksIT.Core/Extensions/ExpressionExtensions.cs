using System.Linq.Expressions;


namespace MaksIT.Core.Extensions;

public static class ExpressionExtensions {

  public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second) {
    ArgumentNullException.ThrowIfNull(first);
    ArgumentNullException.ThrowIfNull(second);

    var parameter = first.Parameters[0];
    var visitor = new SubstituteParameterVisitor(second.Parameters[0], parameter);
    var secondBody = visitor.Visit(second.Body);
    var combinedBody = Expression.AndAlso(first.Body, secondBody);

    return Expression.Lambda<Func<T, bool>>(combinedBody, parameter);
  }

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
    if (expression == null) throw new ArgumentNullException(nameof(expression));

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
