using MaksIT.Core.Logging;
using MaksIT.Core.Sagas;
using Microsoft.Extensions.Logging;

namespace MaksIT.Core.Tests.Sagas;

public class LocalSagaTests
{
    [Fact]
    public async Task LocalSagaBuilder_ShouldBuildSagaWithSteps()
    {
        // Arrange
        var logger = LoggerHelper.CreateConsoleLogger();
        var builder = new LocalSagaBuilder(logger);
        var stepExecuted = false;

        builder.AddAction(
            "TestStep",
            async (ctx, ct) =>
            {
                stepExecuted = true;
                await Task.CompletedTask;
            });

        var saga = builder.Build();

        // Act
        await saga.ExecuteAsync();

        // Assert
        Assert.True(stepExecuted, "The step should have been executed.");
    }

    [Fact]
    public async Task LocalSaga_ShouldCompensateOnFailure()
    {
        // Arrange
        var logger = LoggerHelper.CreateConsoleLogger();
        var builder = new LocalSagaBuilder(logger);
        var compensationCalled = false;

        builder.AddAction(
            "FailingStep",
            async (ctx, ct) =>
            {
                throw new InvalidOperationException("Step failed");
            },
            async (ctx, ct) =>
            {
                compensationCalled = true;
                await Task.CompletedTask;
            });

        var saga = builder.Build();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => saga.ExecuteAsync());
        Assert.True(compensationCalled, "Compensation should have been called.");
    }

    [Fact]
    public async Task LocalSaga_ShouldSkipConditionalSteps()
    {
        // Arrange
        var logger = LoggerHelper.CreateConsoleLogger();
        var builder = new LocalSagaBuilder(logger);
        var stepExecuted = false;

        builder.AddActionIf(
            ctx => false,
            "SkippedStep",
            async (ctx, ct) =>
            {
                stepExecuted = true;
                await Task.CompletedTask;
            });

        var saga = builder.Build();

        // Act
        await saga.ExecuteAsync();

        // Assert
        Assert.False(stepExecuted, "The step should have been skipped.");
    }

    [Fact]
    public async Task LocalSaga_ShouldLogExecution()
    {
        // Arrange
        var logger = LoggerHelper.CreateConsoleLogger();
        var builder = new LocalSagaBuilder(logger);

        builder.AddAction(
            "LoggingStep",
            async (ctx, ct) => await Task.CompletedTask);

        var saga = builder.Build();

        // Act
        await saga.ExecuteAsync();
        // No assertion on logs, but output will be visible in test runner console
    }

    [Fact]
    public async Task LocalSaga_ShouldRestorePreviousStateOnError()
    {
        // Arrange
        var logger = LoggerHelper.CreateConsoleLogger();
        var builder = new LocalSagaBuilder(logger);
        var context = new LocalSagaContext();
        context.Set("state", "initial");

        builder.AddAction(
            "ModifyStateStep",
            async (ctx, ct) =>
            {
                ctx.Set("state", "modified");
                await Task.CompletedTask;
            },
            async (ctx, ct) =>
            {
                ctx.Set("state", "initial");
                await Task.CompletedTask;
            });

        builder.AddAction(
            "FailingStep",
            async (ctx, ct) =>
            {
                throw new InvalidOperationException("Step failed");
            });

        var saga = builder.Build();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => saga.ExecuteAsync(context));
        Assert.Equal("initial", context.Get<string>("state"));
    }

    [Fact]
    public async Task LocalSaga_ShouldHandleMultipleCompensations()
    {
        // Arrange
        var logger = LoggerHelper.CreateConsoleLogger();
        var builder = new LocalSagaBuilder(logger);
        var context = new LocalSagaContext();
        var compensationLog = new List<string>();

        builder.AddAction(
            "Step1",
            async (ctx, ct) =>
            {
                ctx.Set("step1", true);
                await Task.CompletedTask;
            },
            async (ctx, ct) =>
            {
                compensationLog.Add("Step1 compensated");
                await Task.CompletedTask;
            });

        builder.AddAction(
            "Step2",
            async (ctx, ct) =>
            {
                ctx.Set("step2", true);
                await Task.CompletedTask;
            },
            async (ctx, ct) =>
            {
                compensationLog.Add("Step2 compensated");
                await Task.CompletedTask;
            });

        builder.AddAction(
            "FailingStep",
            async (ctx, ct) =>
            {
                throw new InvalidOperationException("Step failed");
            });

        var saga = builder.Build();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => saga.ExecuteAsync(context));
        Assert.Contains("Step2 compensated", compensationLog);
        Assert.Contains("Step1 compensated", compensationLog);
    }
}
