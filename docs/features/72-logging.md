# Feature 72: Logging

## Overview

Implement logging infrastructure for the Stroke library, providing a centralized logger for debugging and diagnostics.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/log.py`

## Public API

### Logger

```csharp
namespace Stroke;

/// <summary>
/// Central logger for the Stroke library.
/// Uses Microsoft.Extensions.Logging for structured logging.
/// </summary>
public static class StrokeLogger
{
    /// <summary>
    /// The logger instance for the Stroke namespace.
    /// </summary>
    public static ILogger Logger { get; }

    /// <summary>
    /// Configure the logger factory.
    /// Must be called before first use of Logger.
    /// </summary>
    /// <param name="factory">Logger factory to use.</param>
    public static void Configure(ILoggerFactory factory);

    /// <summary>
    /// Create a logger for a specific category.
    /// </summary>
    /// <typeparam name="T">Type to use as category name.</typeparam>
    /// <returns>Logger instance.</returns>
    public static ILogger<T> CreateLogger<T>();

    /// <summary>
    /// Create a logger for a specific category name.
    /// </summary>
    /// <param name="categoryName">Category name.</param>
    /// <returns>Logger instance.</returns>
    public static ILogger CreateLogger(string categoryName);
}
```

## Project Structure

```
src/Stroke/
└── StrokeLogger.cs
tests/Stroke.Tests/
└── StrokeLoggerTests.cs
```

## Implementation Notes

### StrokeLogger Implementation

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Stroke;

/// <summary>
/// Central logger for the Stroke library.
/// </summary>
public static class StrokeLogger
{
    private static ILoggerFactory _factory = NullLoggerFactory.Instance;
    private static ILogger? _logger;

    /// <summary>
    /// The main logger for the Stroke namespace.
    /// </summary>
    public static ILogger Logger => _logger ??= CreateLogger("Stroke");

    /// <summary>
    /// Configure the logger factory.
    /// </summary>
    /// <param name="factory">Logger factory to use.</param>
    public static void Configure(ILoggerFactory factory)
    {
        _factory = factory ?? NullLoggerFactory.Instance;
        _logger = null; // Reset to pick up new factory
    }

    /// <summary>
    /// Create a typed logger.
    /// </summary>
    public static ILogger<T> CreateLogger<T>() =>
        _factory.CreateLogger<T>();

    /// <summary>
    /// Create a logger for a category name.
    /// </summary>
    public static ILogger CreateLogger(string categoryName) =>
        _factory.CreateLogger(categoryName);
}
```

### Usage Throughout Library

```csharp
// In Renderer
internal partial class Renderer
{
    private static readonly ILogger _logger =
        StrokeLogger.CreateLogger<Renderer>();

    public void Render(...)
    {
        _logger.LogDebug("Starting render, size={Size}", size);

        try
        {
            // Rendering logic...
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Render failed");
            throw;
        }
    }
}

// In VT100 parser
internal class Vt100Parser
{
    private static readonly ILogger _logger =
        StrokeLogger.CreateLogger<Vt100Parser>();

    public void Feed(string data)
    {
        _logger.LogTrace("Parsing {Length} characters", data.Length);
        // ...
    }
}

// In KeyProcessor
internal class KeyProcessor
{
    private static readonly ILogger _logger =
        StrokeLogger.CreateLogger<KeyProcessor>();

    public void ProcessKey(KeyPress keyPress)
    {
        _logger.LogDebug("Processing key: {Key}", keyPress.Key);
        // ...
    }
}
```

### Application Configuration

```csharp
// Console logging
using Microsoft.Extensions.Logging;

StrokeLogger.Configure(
    LoggerFactory.Create(builder =>
    {
        builder
            .AddFilter("Stroke", LogLevel.Debug)
            .AddConsole();
    }));

// File logging with Serilog
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File("stroke.log")
    .CreateLogger();

StrokeLogger.Configure(
    LoggerFactory.Create(builder => builder.AddSerilog()));
```

### Logging Categories

The following categories are used throughout Stroke:

| Category | Description |
|----------|-------------|
| `Stroke` | Root logger |
| `Stroke.Renderer` | Screen rendering |
| `Stroke.Input` | Input processing |
| `Stroke.KeyBinding` | Key binding matching |
| `Stroke.Application` | Application lifecycle |
| `Stroke.Layout` | Layout calculations |
| `Stroke.Completion` | Completion providers |

### Debug Output Examples

```
dbug: Stroke.Renderer[0] Starting render, size=Size { Rows = 24, Columns = 80 }
dbug: Stroke.KeyBinding[0] Processing key: c-a
dbug: Stroke.KeyBinding[0] Matched binding: beginning-of-line
trce: Stroke.Input.Vt100Parser[0] Parsing 5 characters
dbug: Stroke.Completion[0] Getting completions for "git sta"
dbug: Stroke.Completion[0] Found 3 completions
```

### Conditional Logging

For performance-critical code, use conditional logging:

```csharp
if (_logger.IsEnabled(LogLevel.Trace))
{
    // Expensive operation only if trace is enabled
    var details = ComputeExpensiveDetails();
    _logger.LogTrace("Details: {Details}", details);
}
```

## Dependencies

- `Microsoft.Extensions.Logging.Abstractions` - ILogger interface
- `Microsoft.Extensions.Logging` - LoggerFactory (optional, for configuration)

## Implementation Tasks

1. Implement `StrokeLogger` static class
2. Add `Configure` method for factory setup
3. Add `CreateLogger<T>` for typed loggers
4. Add `CreateLogger(string)` for named loggers
5. Add logging to key library components
6. Document logging categories
7. Write unit tests

## Acceptance Criteria

- [ ] StrokeLogger.Logger returns valid logger
- [ ] Configure() sets the factory
- [ ] CreateLogger<T>() creates typed logger
- [ ] CreateLogger(string) creates named logger
- [ ] Default is NullLogger (no-op)
- [ ] Works with Microsoft.Extensions.Logging providers
- [ ] Logging doesn't impact performance when disabled
- [ ] Unit tests achieve 80% coverage
