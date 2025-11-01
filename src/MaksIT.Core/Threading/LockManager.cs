using System.Collections.Concurrent;
using System.Threading.RateLimiting;

namespace MaksIT.Core.Threading;

public class LockManager : IDisposable {
  private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

  // Use AsyncLocal to track reentrancy in the same async flow
  private static readonly AsyncLocal<int> _reentrancyDepth = new AsyncLocal<int>();

  // Strict limiter: allow 1 token, replenish 1 every 200ms
  private readonly TokenBucketRateLimiter _rateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions {
    TokenLimit = 1, // Single concurrent entry
    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
    QueueLimit = 1_000,
    ReplenishmentPeriod = TimeSpan.FromMilliseconds(200), // 1 token every 200ms
    TokensPerPeriod = 1,
    AutoReplenishment = true
  });

  public async Task<T> ExecuteWithLockAsync<T>(Func<Task<T>> action) {
    var lease = await _rateLimiter.AcquireAsync(1);
    if (!lease.IsAcquired) throw new InvalidOperationException("Rate limit exceeded");

    // Determine if this is the first entry for the current async flow
    bool isFirstEntry = false;
    if (_reentrancyDepth.Value == 0) {
      isFirstEntry = true;
      _reentrancyDepth.Value = 1;
    }
    else {
      _reentrancyDepth.Value = _reentrancyDepth.Value + 1;
    }

    if (isFirstEntry) await _semaphore.WaitAsync();
    try {
      return await action();
    }
    finally {
      // Decrement reentrancy; release semaphore only when depth reaches zero
      var newDepth = _reentrancyDepth.Value - 1;
      _reentrancyDepth.Value = newDepth < 0 ? 0 : newDepth;

      if (isFirstEntry) _semaphore.Release();

      // Dispose the lease to complete the rate-limited window
      lease.Dispose();
    }
  }

  public async Task ExecuteWithLockAsync(Func<Task> action) {
    await ExecuteWithLockAsync(async () => {
      await action();
      return true;
    });
  }

  public async Task<T> ExecuteWithLockAsync<T>(Func<T> action) {
    return await ExecuteWithLockAsync(() => Task.FromResult(action()));
  }

  public async Task ExecuteWithLockAsync(Action action) {
    await ExecuteWithLockAsync(() => {
      action();
      return Task.CompletedTask;
    });
  }

  public void Dispose() {
    _semaphore.Dispose();
    _rateLimiter.Dispose();
  }
}
