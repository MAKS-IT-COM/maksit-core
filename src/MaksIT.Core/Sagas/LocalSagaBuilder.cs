using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MaksIT.Core.Sagas;

/// <summary>
/// Fluent builder to compose a local saga (exception-based failures).
/// </summary>
public sealed class LocalSagaBuilder {
  private readonly List<ILocalSagaStep> _pipeline = new();
  private ILogger? _logger;

  public LocalSagaBuilder WithLogger(ILogger logger) {
    _logger = logger;
    return this;
  }

  public LocalSagaBuilder AddAction(
    string name,
    Func<LocalSagaContext, CancellationToken, Task> execute,
    Func<LocalSagaContext, CancellationToken, Task>? compensate = null) {
    _pipeline.Add(new LocalSagaStep<Unit>(
      name,
      async (c, ct) => { await execute(c, ct); return Unit.Value; },
      compensate,
      predicate: null,
      outputKey: null));
    return this;
  }

  public LocalSagaBuilder AddActionIf(
    Func<LocalSagaContext, bool> predicate,
    string name,
    Func<LocalSagaContext, CancellationToken, Task> execute,
    Func<LocalSagaContext, CancellationToken, Task>? compensate = null) {
    _pipeline.Add(new LocalSagaStep<Unit>(
      $"[conditional] {name}",
      async (c, ct) => { await execute(c, ct); return Unit.Value; },
      compensate,
      predicate,
      outputKey: null));
    return this;
  }

  public LocalSagaBuilder AddStep<T>(
    string name,
    Func<LocalSagaContext, CancellationToken, Task<T>> execute,
    string? outputKey = null,
    Func<LocalSagaContext, CancellationToken, Task>? compensate = null) {
    _pipeline.Add(new LocalSagaStep<T>(name, execute, compensate, predicate: null, outputKey: outputKey));
    return this;
  }

  public LocalSagaBuilder AddStepIf<T>(
    Func<LocalSagaContext, bool> predicate,
    string name,
    Func<LocalSagaContext, CancellationToken, Task<T>> execute,
    string? outputKey = null,
    Func<LocalSagaContext, CancellationToken, Task>? compensate = null) {
    _pipeline.Add(new LocalSagaStep<T>($"[conditional] {name}", execute, compensate, predicate, outputKey));
    return this;
  }

  public LocalSaga Build() {
    if (_logger == null)
      throw new InvalidOperationException("Logger must be provided via WithLogger().");
    return new LocalSaga(_pipeline, _logger);
  }
}