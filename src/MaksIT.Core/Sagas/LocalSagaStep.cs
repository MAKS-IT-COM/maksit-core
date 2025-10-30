using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MaksIT.Core.Sagas;

/// <summary>
/// Internal non-generic step interface to unify generic steps.
/// </summary>
internal interface ILocalSagaStep {
  string Name { get; }
  Task<bool> ExecuteAsync(LocalSagaContext ctx, CancellationToken ct);
  Task CompensateAsync(LocalSagaContext ctx, CancellationToken ct);
}


/// <summary>
/// Generic step with a result that can optionally be stored into the context.
/// Execution returns true if this step actually ran (useful for conditional steps).
/// </summary>
internal sealed class LocalSagaStep<T> : ILocalSagaStep {
  public string Name { get; }
  public Func<LocalSagaContext, CancellationToken, Task<T>> Execute { get; }
  public Func<LocalSagaContext, CancellationToken, Task>? Compensate { get; }
  public Func<LocalSagaContext, bool>? Predicate { get; }
  public string? OutputKey { get; }

  public LocalSagaStep(
    string name,
    Func<LocalSagaContext, CancellationToken, Task<T>> execute,
    Func<LocalSagaContext, CancellationToken, Task>? compensate,
    Func<LocalSagaContext, bool>? predicate,
    string? outputKey) {
    Name = name;
    Execute = execute;
    Compensate = compensate;
    Predicate = predicate;
    OutputKey = outputKey;
  }

  public async Task<bool> ExecuteAsync(LocalSagaContext ctx, CancellationToken ct) {
    if (Predicate != null && !Predicate(ctx))
      return false;

    var result = await Execute(ctx, ct);
    if (OutputKey != null)
      ctx.Set(OutputKey, result);
    return true;
  }

  public async Task CompensateAsync(LocalSagaContext ctx, CancellationToken ct) {
    if (Compensate != null)
      await Compensate(ctx, ct);
  }
}