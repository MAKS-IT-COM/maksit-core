using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;


namespace MaksIT.Core.Extensions;

public static class ExpressionExtensions {

  public static Expression<Func<T, bool>> CombineWith<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second) {
    var parameter = first.Parameters[0];
    var visitor = new SubstituteParameterVisitor(second.Parameters[0], parameter);
    var secondBody = visitor.Visit(second.Body);
    var combinedBody = Expression.AndAlso(first.Body, secondBody);

    return Expression.Lambda<Func<T, bool>>(combinedBody, parameter);
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
}
