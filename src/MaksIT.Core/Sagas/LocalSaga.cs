using Microsoft.Extensions.Logging;


namespace MaksIT.Core.Sagas;
/// <summary>
/// Executable local saga with LIFO compensation on failure.
/// </summary>
public sealed class LocalSaga {
  private readonly IReadOnlyList<ILocalSagaStep> _pipeline;
  private readonly ILogger _logger;

  internal LocalSaga(
    IReadOnlyList<ILocalSagaStep> pipeline,
    ILogger logger
  ) {
    _pipeline = pipeline;
    _logger = logger;
  }

  public async Task ExecuteAsync(LocalSagaContext? context = null, CancellationToken cancellationToken = default)
  {
      var ctx = context ?? new LocalSagaContext();
      var executedStack = new Stack<ILocalSagaStep>();

      for (int i = 0; i < _pipeline.Count; i++)
      {
          cancellationToken.ThrowIfCancellationRequested();

          var step = _pipeline[i];
          try
          {
              _logger.LogInformation($"LocalSaga: executing step [{i + 1}/{_pipeline.Count}] '{step.Name}'");
              var ran = await step.ExecuteAsync(ctx, cancellationToken);
              if (ran)
                  executedStack.Push(step); // Ensure step is pushed if it ran successfully
              else
                  _logger.LogInformation($"LocalSaga: skipped step '{step.Name}'");
          }
          catch (Exception ex)
          {
              _logger.LogError(ex, $"LocalSaga: step '{step.Name}' failed");
              executedStack.Push(step); // Push the step to ensure compensation is triggered
              await CompensateAsync(executedStack, ctx, cancellationToken);
              throw;
          }
      }

      _logger.LogInformation("LocalSaga: completed successfully");
  }

  private async Task CompensateAsync(
    Stack<ILocalSagaStep> executedStack,
    LocalSagaContext ctx,
    CancellationToken ct) {
    _logger.LogInformation("LocalSaga: starting compensation");

    var compensationErrors = new List<Exception>();
    int totalSteps = executedStack.Count;

    try
    {
        while (executedStack.Count > 0)
        {
            var step = executedStack.Pop();
            try
            {
                _logger.LogInformation($"LocalSaga: compensating step '{step.Name}' ({totalSteps - executedStack.Count}/{totalSteps})");
                await step.CompensateAsync(ctx, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"LocalSaga: compensation of step '{step.Name}' failed");
                compensationErrors.Add(ex);
            }
        }
    }
    finally
    {
        _logger.LogInformation("LocalSaga: compensation finished");
    }

    if (compensationErrors.Count > 0)
        throw new AggregateException("One or more compensation steps failed.", compensationErrors);
  }
}