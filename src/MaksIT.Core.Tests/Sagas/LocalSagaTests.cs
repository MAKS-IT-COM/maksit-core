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

    [Fact]
    public async Task LocalSagaBuilder_ShouldBuildSagaWithStepsAndReturnResult()
    {
        // Arrange
        var logger = LoggerHelper.CreateConsoleLogger();
        var builder = new LocalSagaBuilder(logger);
        var stepResult = "";

        builder.AddStep<string>(
            "TestStep",
            async (ctx, ct) =>
            {
                await Task.CompletedTask;
                return "StepResult";
            },
            outputKey: "stepResult");

        var saga = builder.Build();
        var context = new LocalSagaContext();

        // Act
        await saga.ExecuteAsync(context);
        stepResult = context.Get<string>("stepResult");

        // Assert
        Assert.Equal("StepResult", stepResult);
    }

    [Fact]
    public async Task LocalSagaBuilder_ShouldSkipConditionalStep()
    {
        // Arrange
        var logger = LoggerHelper.CreateConsoleLogger();
        var builder = new LocalSagaBuilder(logger);
        var stepExecuted = false;

        builder.AddStepIf<string>(
            ctx => false,
            "SkippedStep",
            async (ctx, ct) =>
            {
                stepExecuted = true;
                return "Skipped";
            },
            outputKey: "skippedResult");

        var saga = builder.Build();

        // Act
        await saga.ExecuteAsync();

        // Assert
        Assert.False(stepExecuted, "The step should have been skipped.");
    }

    [Fact]
    public async Task LocalSagaBuilder_ShouldCompensateStepOnFailure()
    {
        // Arrange
        var logger = LoggerHelper.CreateConsoleLogger();
        var builder = new LocalSagaBuilder(logger);
        var compensationCalled = false;

        builder.AddStep<string>(
            "FailingStep",
            async (ctx, ct) =>
            {
                throw new InvalidOperationException("Step failed");
            },
            outputKey: "failingResult",
            compensate: async (ctx, ct) =>
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
    public async Task LocalSagaBuilder_ShouldStoreStepOutputInContext()
    {
        // Arrange
        var logger = LoggerHelper.CreateConsoleLogger();
        var builder = new LocalSagaBuilder(logger);
        var context = new LocalSagaContext();

        builder.AddStep<int>(
            "OutputStep",
            async (ctx, ct) =>
            {
                await Task.CompletedTask;
                return 42;
            },
            outputKey: "result");

        var saga = builder.Build();

        // Act
        await saga.ExecuteAsync(context);
        var result = context.Get<int>("result");

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task LocalSagaBuilder_ShouldSaveBackupValueAndRestoreOnError()
    {
        // Arrange
        var logger = LoggerHelper.CreateConsoleLogger();
        var builder = new LocalSagaBuilder(logger);
        var context = new LocalSagaContext();
        context.Set("state", "initial");

        builder.AddStep<string>(
            "ModifyStateStep",
            async (ctx, ct) =>
            {
                // Save the original value to a backup key
                var originalState = ctx.Get<string>("state");
                ctx.Set("backup_state", originalState);

                // Modify the state
                ctx.Set("state", "modified");
                await Task.CompletedTask;
                return "modified";
            },
            outputKey: "modifyStateResult",
            compensate: async (ctx, ct) =>
            {
                // Restore the original value from the backup key
                var backupState = ctx.Get<string>("backup_state");
                ctx.Set("state", backupState);
                await Task.CompletedTask;
            });

        builder.AddStep<string>(
            "FailingStep",
            async (ctx, ct) =>
            {
                throw new InvalidOperationException("Step failed");
            },
            outputKey: "failingResult");

        var saga = builder.Build();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => saga.ExecuteAsync(context));
        Assert.Equal("initial", context.Get<string>("state"));
    }

    [Fact]
    public async Task LocalSagaBuilder_ShouldUseOutputKeyToStoreAndRetrieveValues()
    {
        // Arrange
        var logger = LoggerHelper.CreateConsoleLogger();
        var builder = new LocalSagaBuilder(logger);
        var context = new LocalSagaContext();

        builder.AddStep<int>(
            "Step1",
            async (ctx, ct) =>
            {
                await Task.CompletedTask;
                return 100;
            },
            outputKey: "step1Result");

        builder.AddStep<int>(
            "Step2",
            async (ctx, ct) =>
            {
                var step1Result = ctx.Get<int>("step1Result");
                await Task.CompletedTask;
                return step1Result + 50;
            },
            outputKey: "step2Result");

        var saga = builder.Build();

        // Act
        await saga.ExecuteAsync(context);

        // Assert
        var step1Result = context.Get<int>("step1Result");
        var step2Result = context.Get<int>("step2Result");

        Assert.Equal(100, step1Result);
        Assert.Equal(150, step2Result);
    }
}
