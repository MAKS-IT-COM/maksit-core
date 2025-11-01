using System.Diagnostics;
using MaksIT.Core.Threading;

namespace MaksIT.Core.Tests.Threading;

public class LockManagerTests {
  [Fact]
  public async Task ShouldEnsureThreadSafety() {
    // Arrange
    var lockManager = new LockManager();
    int counter = 0;

    // Act
    var tasks = Enumerable.Range(0, 10).Select(_ => lockManager.ExecuteWithLockAsync(async () => {
      int temp = counter;
      await Task.Delay(10); // Simulate work
      counter = temp + 1;
    }));

    await Task.WhenAll(tasks);

    // Assert
    Assert.Equal(10, counter);
  }

  [Fact]
  public async Task ShouldEnforceRateLimiting() {
    // Arrange
    var lockManager = new LockManager();
    var stopwatch = Stopwatch.StartNew();

    // Act
    var tasks = Enumerable.Range(0, 10).Select(_ => lockManager.ExecuteWithLockAsync(async () => {
      await Task.Delay(10); // Simulate work
    }));

    await Task.WhenAll(tasks);
    stopwatch.Stop();

    // With 1 token and 200ms replenishment:
    // first task starts immediately, remaining 9 wait ~9 * 200ms = ~1800ms + overhead.
    // Allow some jitter on CI.
    Assert.InRange(stopwatch.ElapsedMilliseconds, 1700, 6000);
  }

  [Fact]
  public async Task ShouldAllowReentrantLocks() {
    // Arrange
    var lockManager = new LockManager();
    int counter = 0;

    // Act
    await lockManager.ExecuteWithLockAsync(async () => {
      await lockManager.ExecuteWithLockAsync(() => {
        counter++;
        return Task.CompletedTask;
      });
    });

    // Assert
    Assert.Equal(1, counter);
  }

  [Fact]
  public async Task ShouldHandleExceptionsGracefully() {
    // Arrange
    var lockManager = new LockManager();
    int counter = 0;

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(async () => {
      await lockManager.ExecuteWithLockAsync(async () => {
        counter++;
        throw new InvalidOperationException("Test exception");
      });
    });

    // Ensure lock is not in an inconsistent state
    await lockManager.ExecuteWithLockAsync(() => {
      counter++;
      return Task.CompletedTask;
    });

    Assert.Equal(2, counter);
  }

  [Fact]
  public async Task ShouldSupportConcurrentAccess() {
    // Arrange
    var lockManager = new LockManager();
    int counter = 0;

    // Act
    var tasks = Enumerable.Range(0, 100).Select(_ => lockManager.ExecuteWithLockAsync(async () => {
      int temp = counter;
      await Task.Delay(1); // Simulate work
      counter = temp + 1;
    }));

    await Task.WhenAll(tasks);

    // Assert
    Assert.Equal(100, counter);
  }
}
