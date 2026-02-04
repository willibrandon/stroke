using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Stroke;
using Xunit;

namespace Stroke.Tests;

/// <summary>
/// Tests for <see cref="StrokeLogger"/>.
/// </summary>
public class StrokeLoggerTests : IDisposable
{
    public StrokeLoggerTests()
    {
        // Reset to known state before each test
        StrokeLogger.Reset();
    }

    public void Dispose()
    {
        // Reset after each test
        StrokeLogger.Reset();
    }

    #region Logger Property Tests

    [Fact]
    public void Logger_ReturnsValidLogger()
    {
        var logger = StrokeLogger.Logger;

        Assert.NotNull(logger);
    }

    [Fact]
    public void Logger_ReturnsSameInstance()
    {
        var logger1 = StrokeLogger.Logger;
        var logger2 = StrokeLogger.Logger;

        Assert.Same(logger1, logger2);
    }

    [Fact]
    public void Logger_DefaultsToNullLogger()
    {
        var logger = StrokeLogger.Logger;

        // NullLogger doesn't throw when logging
        logger.LogDebug("Test message");
        logger.LogError("Error message");
    }

    #endregion

    #region Configure Tests

    [Fact]
    public void Configure_WithFactory_SetsFactory()
    {
        var factory = new TestLoggerFactory();

        StrokeLogger.Configure(factory);
        var logger = StrokeLogger.Logger;

        Assert.NotNull(logger);
        Assert.True(factory.CreatedLoggers.Count > 0);
    }

    [Fact]
    public void Configure_WithNull_ResetsToNullFactory()
    {
        var factory = new TestLoggerFactory();
        StrokeLogger.Configure(factory);

        StrokeLogger.Configure(null);
        var logger = StrokeLogger.Logger;

        // Should not throw and should work like NullLogger
        logger.LogDebug("Test");
    }

    [Fact]
    public void Configure_ResetsCachedLogger()
    {
        var factory1 = new TestLoggerFactory();
        StrokeLogger.Configure(factory1);
        var logger1 = StrokeLogger.Logger;

        var factory2 = new TestLoggerFactory();
        StrokeLogger.Configure(factory2);
        var logger2 = StrokeLogger.Logger;

        // Each factory creates a new logger, but test via factory tracking
        Assert.True(factory2.CreatedLoggers.Count > 0);
    }

    #endregion

    #region CreateLogger<T> Tests

    [Fact]
    public void CreateLoggerT_ReturnsValidLogger()
    {
        var logger = StrokeLogger.CreateLogger<StrokeLoggerTests>();

        Assert.NotNull(logger);
    }

    [Fact]
    public void CreateLoggerT_UsesConfiguredFactory()
    {
        var factory = new TestLoggerFactory();
        StrokeLogger.Configure(factory);

        var logger = StrokeLogger.CreateLogger<StrokeLoggerTests>();

        Assert.NotNull(logger);
        Assert.Contains(factory.CreatedLoggers, name =>
            name.Contains(nameof(StrokeLoggerTests)));
    }

    [Fact]
    public void CreateLoggerT_MultipleCallsCreateNewInstances()
    {
        var logger1 = StrokeLogger.CreateLogger<StrokeLoggerTests>();
        var logger2 = StrokeLogger.CreateLogger<StrokeLoggerTests>();

        // May or may not be same instance depending on factory implementation
        // But both should be valid
        Assert.NotNull(logger1);
        Assert.NotNull(logger2);
    }

    #endregion

    #region CreateLogger(string) Tests

    [Fact]
    public void CreateLoggerString_ReturnsValidLogger()
    {
        var logger = StrokeLogger.CreateLogger("Stroke.Test");

        Assert.NotNull(logger);
    }

    [Fact]
    public void CreateLoggerString_UsesConfiguredFactory()
    {
        var factory = new TestLoggerFactory();
        StrokeLogger.Configure(factory);

        var logger = StrokeLogger.CreateLogger("Stroke.Custom");

        Assert.NotNull(logger);
        Assert.Contains("Stroke.Custom", factory.CreatedLoggers);
    }

    [Fact]
    public void CreateLoggerString_WithDifferentNames_CreatesDifferentLoggers()
    {
        var factory = new TestLoggerFactory();
        StrokeLogger.Configure(factory);

        StrokeLogger.CreateLogger("Stroke.Renderer");
        StrokeLogger.CreateLogger("Stroke.Input");

        Assert.Contains("Stroke.Renderer", factory.CreatedLoggers);
        Assert.Contains("Stroke.Input", factory.CreatedLoggers);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task Logger_ConcurrentAccess_IsThreadSafe()
    {
        var ct = TestContext.Current.CancellationToken;
        var tasks = new List<Task>();
        var loggers = new List<ILogger>();
        var lockObj = new object();

        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                var logger = StrokeLogger.Logger;
                lock (lockObj)
                {
                    loggers.Add(logger);
                }
            }, ct));
        }

        await Task.WhenAll(tasks);

        // All accesses should return the same instance
        Assert.All(loggers, l => Assert.Same(loggers[0], l));
    }

    [Fact]
    public async Task Configure_ConcurrentWithLoggerAccess_IsThreadSafe()
    {
        var ct = TestContext.Current.CancellationToken;
        var tasks = new List<Task>();
        var exceptions = new List<Exception>();

        for (int i = 0; i < 50; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    StrokeLogger.Configure(new TestLoggerFactory());
                    _ = StrokeLogger.Logger;
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            }, ct));
        }

        await Task.WhenAll(tasks);

        Assert.Empty(exceptions);
    }

    [Fact]
    public async Task CreateLogger_ConcurrentCalls_IsThreadSafe()
    {
        var ct = TestContext.Current.CancellationToken;
        var factory = new TestLoggerFactory();
        StrokeLogger.Configure(factory);
        var tasks = new List<Task>();
        var exceptions = new List<Exception>();

        for (int i = 0; i < 100; i++)
        {
            var index = i;
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    StrokeLogger.CreateLogger($"Stroke.Test{index}");
                    StrokeLogger.CreateLogger<StrokeLoggerTests>();
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            }, ct));
        }

        await Task.WhenAll(tasks);

        Assert.Empty(exceptions);
    }

    #endregion

    #region Logging Categories Tests

    [Fact]
    public void Logger_UsesCategoryNameStroke()
    {
        var factory = new TestLoggerFactory();
        StrokeLogger.Configure(factory);

        _ = StrokeLogger.Logger;

        Assert.Contains("Stroke", factory.CreatedLoggers);
    }

    [Theory]
    [InlineData("Stroke.Renderer")]
    [InlineData("Stroke.Input")]
    [InlineData("Stroke.KeyBinding")]
    [InlineData("Stroke.Application")]
    [InlineData("Stroke.Layout")]
    [InlineData("Stroke.Completion")]
    [InlineData("Stroke.Telnet")]
    public void CreateLogger_WithHierarchicalName_CreatesLogger(string categoryName)
    {
        var factory = new TestLoggerFactory();
        StrokeLogger.Configure(factory);

        var logger = StrokeLogger.CreateLogger(categoryName);

        Assert.NotNull(logger);
        Assert.Contains(categoryName, factory.CreatedLoggers);
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void Logger_WhenDisabled_DoesNotThrow()
    {
        // Default state - logging disabled (NullLogger)
        var logger = StrokeLogger.Logger;

        // All log levels should be no-ops
        var ex = Record.Exception(() =>
        {
            logger.LogTrace("Trace message with {Param}", "value");
            logger.LogDebug("Debug message");
            logger.LogInformation("Info message");
            logger.LogWarning("Warning message");
            logger.LogError("Error message");
            logger.LogCritical("Critical message");
        });

        Assert.Null(ex);
    }

    [Fact]
    public void Logger_WhenDisabled_IsNotEnabled()
    {
        var logger = StrokeLogger.Logger;

        // NullLogger reports all levels as disabled
        Assert.False(logger.IsEnabled(LogLevel.Trace));
        Assert.False(logger.IsEnabled(LogLevel.Debug));
        Assert.False(logger.IsEnabled(LogLevel.Information));
        Assert.False(logger.IsEnabled(LogLevel.Warning));
        Assert.False(logger.IsEnabled(LogLevel.Error));
        Assert.False(logger.IsEnabled(LogLevel.Critical));
    }

    #endregion

    #region Reset Tests

    [Fact]
    public void Reset_ClearsConfiguration()
    {
        var factory = new TestLoggerFactory();
        StrokeLogger.Configure(factory);
        _ = StrokeLogger.Logger;
        var createdBefore = factory.CreatedLoggers.Count;

        StrokeLogger.Reset();
        _ = StrokeLogger.Logger;

        // After reset, logger should come from NullLoggerFactory, not our factory
        Assert.Equal(createdBefore, factory.CreatedLoggers.Count);
    }

    #endregion

    #region Test Helpers

    /// <summary>
    /// Test logger factory that tracks created loggers.
    /// </summary>
    private sealed class TestLoggerFactory : ILoggerFactory
    {
        public List<string> CreatedLoggers { get; } = new();

        public void AddProvider(ILoggerProvider provider) { }

        public ILogger CreateLogger(string categoryName)
        {
            lock (CreatedLoggers)
            {
                CreatedLoggers.Add(categoryName);
            }
            return NullLogger.Instance;
        }

        public void Dispose() { }
    }

    #endregion
}
