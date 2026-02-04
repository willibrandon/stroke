using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Stroke;

/// <summary>
/// Central logger for the Stroke library.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>prompt_toolkit.log</c> module.
/// It provides a centralized logging facility that can be configured with any
/// Microsoft.Extensions.Logging provider.
/// </para>
/// <para>
/// By default, logging is disabled (uses <see cref="NullLoggerFactory"/>), ensuring
/// zero performance impact. To enable logging, call <see cref="Configure"/> with
/// a configured <see cref="ILoggerFactory"/>.
/// </para>
/// <para>
/// Thread safety: This class is thread-safe. Configuration should ideally be done
/// once at application startup before any loggers are created.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Configure with console logging
/// StrokeLogger.Configure(
///     LoggerFactory.Create(builder =>
///     {
///         builder
///             .AddFilter("Stroke", LogLevel.Debug)
///             .AddConsole();
///     }));
///
/// // Use the root logger
/// StrokeLogger.Logger.LogDebug("Application starting");
///
/// // Create a component-specific logger
/// var logger = StrokeLogger.CreateLogger&lt;MyComponent&gt;();
/// logger.LogInformation("Component initialized");
/// </code>
/// </example>
public static class StrokeLogger
{
    private static readonly Lock _lock = new();
    private static ILoggerFactory _factory = NullLoggerFactory.Instance;
    private static ILogger? _logger;

    /// <summary>
    /// The main logger for the Stroke namespace.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the root logger for the Stroke library, equivalent to Python Prompt
    /// Toolkit's <c>prompt_toolkit.log.logger</c>.
    /// </para>
    /// <para>
    /// The logger is lazily created on first access and uses the "Stroke" category name.
    /// </para>
    /// </remarks>
    public static ILogger Logger
    {
        get
        {
            var logger = _logger;
            if (logger != null)
            {
                return logger;
            }

            using (_lock.EnterScope())
            {
                _logger ??= CreateLogger("Stroke");
                return _logger;
            }
        }
    }

    /// <summary>
    /// Configure the logger factory.
    /// </summary>
    /// <param name="factory">
    /// The <see cref="ILoggerFactory"/> to use for creating loggers. Pass
    /// <c>null</c> to disable logging (resets to <see cref="NullLoggerFactory"/>).
    /// </param>
    /// <remarks>
    /// <para>
    /// This method should be called once at application startup, before any
    /// Stroke loggers are created. Subsequent calls will reset the factory
    /// and clear any cached loggers.
    /// </para>
    /// <para>
    /// If not called, logging is disabled by default (no-op).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Enable console logging
    /// StrokeLogger.Configure(
    ///     LoggerFactory.Create(builder =>
    ///     {
    ///         builder.AddConsole();
    ///         builder.SetMinimumLevel(LogLevel.Debug);
    ///     }));
    ///
    /// // Disable logging
    /// StrokeLogger.Configure(null);
    /// </code>
    /// </example>
    public static void Configure(ILoggerFactory? factory)
    {
        using (_lock.EnterScope())
        {
            _factory = factory ?? NullLoggerFactory.Instance;
            _logger = null; // Reset to pick up new factory
        }
    }

    /// <summary>
    /// Create a typed logger for a specific component.
    /// </summary>
    /// <typeparam name="T">The type to use as the logger category name.</typeparam>
    /// <returns>A new <see cref="ILogger{T}"/> instance.</returns>
    /// <remarks>
    /// <para>
    /// Creates a logger with the category name derived from <typeparamref name="T"/>.
    /// This is the recommended pattern for component-specific logging.
    /// </para>
    /// <para>
    /// The returned logger follows the configured factory's settings for
    /// filtering and output.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class MyRenderer
    /// {
    ///     private static readonly ILogger&lt;MyRenderer&gt; _logger =
    ///         StrokeLogger.CreateLogger&lt;MyRenderer&gt;();
    ///
    ///     public void Render()
    ///     {
    ///         _logger.LogDebug("Starting render");
    ///     }
    /// }
    /// </code>
    /// </example>
    public static ILogger<T> CreateLogger<T>()
    {
        using (_lock.EnterScope())
        {
            return _factory.CreateLogger<T>();
        }
    }

    /// <summary>
    /// Create a logger for a specific category name.
    /// </summary>
    /// <param name="categoryName">The category name for the logger.</param>
    /// <returns>A new <see cref="ILogger"/> instance.</returns>
    /// <remarks>
    /// <para>
    /// Creates a logger with the specified category name. Use hierarchical
    /// category names (e.g., "Stroke.Renderer", "Stroke.Input") for structured
    /// log filtering.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var logger = StrokeLogger.CreateLogger("Stroke.Telnet");
    /// logger.LogInformation("Telnet server starting on port {Port}", port);
    /// </code>
    /// </example>
    public static ILogger CreateLogger(string categoryName)
    {
        using (_lock.EnterScope())
        {
            return _factory.CreateLogger(categoryName);
        }
    }

    /// <summary>
    /// Resets the logger to the default (disabled) state.
    /// </summary>
    /// <remarks>
    /// Primarily intended for testing. Resets the factory to
    /// <see cref="NullLoggerFactory"/> and clears cached loggers.
    /// </remarks>
    internal static void Reset()
    {
        using (_lock.EnterScope())
        {
            _factory = NullLoggerFactory.Instance;
            _logger = null;
        }
    }
}
