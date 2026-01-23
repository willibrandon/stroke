# Feature 83: Patch Stdout

## Overview

Implement stdout patching to ensure print statements within an application don't destroy the terminal UI. Output is rendered above the prompt without overwriting the interface.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/patch_stdout.py`

## Public API

### PatchStdout Context Manager

```csharp
namespace Stroke;

/// <summary>
/// Context manager that replaces Console.Out and Console.Error with a proxy
/// that renders output above the current application prompt.
/// </summary>
public static class StdoutPatch
{
    /// <summary>
    /// Create a scope that patches stdout and stderr.
    /// </summary>
    /// <param name="raw">When true, VT100 escape sequences are not filtered.</param>
    /// <returns>Disposable scope.</returns>
    public static IDisposable Patch(bool raw = false);
}
```

### StdoutProxy

```csharp
namespace Stroke;

/// <summary>
/// TextWriter that renders output above the current application prompt.
/// Can be used as a drop-in replacement for Console.Out or passed to
/// logging handlers.
/// </summary>
public sealed class StdoutProxy : TextWriter, IDisposable
{
    /// <summary>
    /// Time to wait between writes to bundle output (in seconds).
    /// </summary>
    public double SleepBetweenWrites { get; }

    /// <summary>
    /// Whether VT100 sequences pass through unchanged.
    /// </summary>
    public bool Raw { get; }

    /// <summary>
    /// Create a stdout proxy.
    /// </summary>
    /// <param name="sleepBetweenWrites">Delay between flushes (default: 0.2s).</param>
    /// <param name="raw">Pass through VT100 sequences unchanged.</param>
    public StdoutProxy(double sleepBetweenWrites = 0.2, bool raw = false);

    /// <summary>
    /// The original stdout before patching.
    /// </summary>
    public TextWriter? OriginalStdout { get; }

    /// <inheritdoc/>
    public override Encoding Encoding { get; }

    /// <inheritdoc/>
    public override void Write(char value);

    /// <inheritdoc/>
    public override void Write(string? value);

    /// <inheritdoc/>
    public override void Flush();

    /// <summary>
    /// Stop the proxy and flush remaining output.
    /// </summary>
    public void Close();
}
```

## Project Structure

```
src/Stroke/
├── StdoutPatch.cs
└── StdoutProxy.cs
tests/Stroke.Tests/
└── StdoutProxyTests.cs
```

## Implementation Notes

### StdoutProxy Implementation

```csharp
public sealed class StdoutProxy : TextWriter
{
    private readonly double _sleepBetweenWrites;
    private readonly bool _raw;
    private readonly AppSession _appSession;
    private readonly IOutput _output;
    private readonly BlockingCollection<string> _flushQueue;
    private readonly Thread _flushThread;
    private readonly object _lock = new();
    private readonly List<string> _buffer = new();
    private bool _closed;

    public StdoutProxy(double sleepBetweenWrites = 0.2, bool raw = false)
    {
        _sleepBetweenWrites = sleepBetweenWrites;
        _raw = raw;
        _appSession = Application.CurrentSession;
        _output = _appSession.Output;
        _flushQueue = new BlockingCollection<string>();
        _flushThread = new Thread(WriteThread) { IsBackground = true };
        _flushThread.Start();
    }

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(string? value)
    {
        if (string.IsNullOrEmpty(value)) return;

        lock (_lock)
        {
            if (value.Contains('\n'))
            {
                // Write everything up to and including newline
                var lastNewline = value.LastIndexOf('\n');
                var toWrite = string.Concat(_buffer) + value[..(lastNewline + 1)];
                _buffer.Clear();
                _buffer.Add(value[(lastNewline + 1)..]);
                _flushQueue.Add(toWrite);
            }
            else
            {
                _buffer.Add(value);
            }
        }
    }

    public override void Flush()
    {
        lock (_lock)
        {
            var text = string.Concat(_buffer);
            _buffer.Clear();
            if (!string.IsNullOrEmpty(text))
                _flushQueue.Add(text);
        }
    }

    private void WriteThread()
    {
        while (!_closed)
        {
            if (_flushQueue.TryTake(out var item, TimeSpan.FromSeconds(1)))
            {
                WriteAndFlush(item);

                // Bundle writes if app is running
                if (GetAppLoop() != null)
                    Thread.Sleep(TimeSpan.FromSeconds(_sleepBetweenWrites));
            }
        }
    }

    private SynchronizationContext? GetAppLoop()
    {
        return _appSession.App?.SynchronizationContext;
    }

    private void WriteAndFlush(string text)
    {
        var loop = GetAppLoop();

        void DoWrite()
        {
            _output.EnableAutowrap();
            if (_raw)
                _output.WriteRaw(text);
            else
                _output.Write(text);
            _output.Flush();
        }

        if (loop == null)
        {
            DoWrite();
        }
        else
        {
            // Run in application thread
            Application.RunInTerminal(DoWrite, inExecutor: false);
        }
    }

    public void Close()
    {
        if (!_closed)
        {
            _closed = true;
            _flushQueue.CompleteAdding();
            _flushThread.Join();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            Close();
        base.Dispose(disposing);
    }
}
```

### PatchStdout Usage

```csharp
public static class StdoutPatch
{
    public static IDisposable Patch(bool raw = false)
    {
        return new StdoutPatchScope(raw);
    }

    private sealed class StdoutPatchScope : IDisposable
    {
        private readonly TextWriter _originalOut;
        private readonly TextWriter _originalError;
        private readonly StdoutProxy _proxy;

        public StdoutPatchScope(bool raw)
        {
            _originalOut = Console.Out;
            _originalError = Console.Error;
            _proxy = new StdoutProxy(raw: raw);

            Console.SetOut(_proxy);
            Console.SetError(_proxy);
        }

        public void Dispose()
        {
            Console.SetOut(_originalOut);
            Console.SetError(_originalError);
            _proxy.Dispose();
        }
    }
}
```

### Application Integration

```csharp
// Usage with Application
using (StdoutPatch.Patch())
{
    await app.RunAsync();
}

// Background thread can now safely print
Task.Run(() =>
{
    Console.WriteLine("This appears above the prompt!");
});
```

### RunInTerminal Integration

The proxy uses `RunInTerminal` to safely output text while an application is running:

```csharp
// In Application
public static void RunInTerminal(
    Action action,
    bool inExecutor = true,
    bool render = true)
{
    var app = Current;
    if (app == null)
    {
        action();
        return;
    }

    // Erase the interface temporarily
    app.Renderer.Erase(leaveAlternateScreen: false);

    try
    {
        if (inExecutor)
            Task.Run(action).Wait();
        else
            action();
    }
    finally
    {
        if (render)
            app.Invalidate();
    }
}
```

## Dependencies

- Feature 54: Application (current session, RunInTerminal)
- Feature 19: Output abstraction

## Implementation Tasks

1. Implement `StdoutProxy` TextWriter
2. Implement background write thread
3. Implement write bundling with delay
4. Implement `StdoutPatch.Patch()` context manager
5. Integrate with RunInTerminal
6. Handle raw vs escaped output
7. Handle graceful shutdown
8. Write unit tests

## Acceptance Criteria

- [ ] StdoutProxy implements TextWriter
- [ ] Output appears above prompt
- [ ] Writes are bundled to reduce flicker
- [ ] Patch() replaces Console.Out and Console.Error
- [ ] Dispose() restores original streams
- [ ] Background threads can safely print
- [ ] Raw mode passes through escape sequences
- [ ] Unit tests achieve 80% coverage
