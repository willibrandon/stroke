# Feature 60: Patch Stdout

## Overview

Implement the `patch_stdout` context manager and `StdoutProxy` class that allow print statements within an application to be displayed above the current prompt without destroying the UI.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/patch_stdout.py`

## Public API

### patch_stdout Context Manager

```csharp
namespace Stroke;

public static class StdoutPatching
{
    /// <summary>
    /// Replace System.Console output with a StdoutProxy.
    /// Writing to this proxy ensures text appears above the prompt
    /// without destroying the renderer output.
    /// </summary>
    /// <param name="raw">When true, VT100 escapes are not filtered.</param>
    /// <returns>A disposable that restores the original stdout.</returns>
    /// <remarks>
    /// If a new event loop is installed, apply this after the loop change.
    /// Both stdout and stderr are redirected to the proxy.
    /// </remarks>
    public static IDisposable PatchStdout(bool raw = false);
}
```

### StdoutProxy Class

```csharp
namespace Stroke;

/// <summary>
/// TextWriter that prints everything above the current prompt.
/// Compatible with logging handlers and other file-like consumers.
///
/// Writes are batched with a short delay to avoid excessive repaints.
/// </summary>
public sealed class StdoutProxy : TextWriter, IDisposable
{
    /// <summary>
    /// Creates a StdoutProxy.
    /// </summary>
    /// <param name="sleepBetweenWrites">Delay between batched writes (seconds).</param>
    /// <param name="raw">Whether to pass VT100 escapes through.</param>
    public StdoutProxy(float sleepBetweenWrites = 0.2f, bool raw = false);

    /// <summary>
    /// Delay between batched writes in seconds.
    /// </summary>
    public float SleepBetweenWrites { get; }

    /// <summary>
    /// Whether VT100 escapes are passed through.
    /// </summary>
    public bool Raw { get; }

    /// <summary>
    /// Whether the proxy is closed.
    /// </summary>
    public bool Closed { get; }

    /// <summary>
    /// The original stdout stream.
    /// </summary>
    public TextWriter? OriginalStdout { get; }

    /// <inheritdoc />
    public override Encoding Encoding { get; }

    /// <inheritdoc />
    public override void Write(char value);

    /// <inheritdoc />
    public override void Write(string? value);

    /// <inheritdoc />
    public override void Flush();

    /// <summary>
    /// Close the proxy and flush remaining output.
    /// </summary>
    public void Close();

    /// <inheritdoc />
    protected override void Dispose(bool disposing);
}
```

## Project Structure

```
src/Stroke/
└── StdoutPatching.cs
└── StdoutProxy.cs
tests/Stroke.Tests/
└── StdoutProxyTests.cs
```

## Implementation Notes

### StdoutProxy Architecture

```csharp
public sealed class StdoutProxy : TextWriter, IDisposable
{
    private readonly float _sleepBetweenWrites;
    private readonly bool _raw;
    private readonly object _lock = new();
    private readonly List<string> _buffer = new();
    private readonly BlockingCollection<object> _flushQueue = new();
    private readonly Thread _flushThread;
    private readonly AppSession _appSession;
    private readonly IOutput _output;
    private bool _closed;

    public StdoutProxy(float sleepBetweenWrites = 0.2f, bool raw = false)
    {
        _sleepBetweenWrites = sleepBetweenWrites;
        _raw = raw;

        // Capture current app session
        _appSession = Application.GetSession();

        // Get output before we potentially become sys.stdout
        _output = _appSession.Output;

        // Start flush thread
        _flushThread = new Thread(WriteThread)
        {
            Name = "patch-stdout-flush-thread",
            IsBackground = true
        };
        _flushThread.Start();
    }
}
```

### Write Buffering

Buffer text until newline, then queue for flushing:

```csharp
public override void Write(string? value)
{
    if (value == null) return;

    lock (_lock)
    {
        if (value.Contains('\n'))
        {
            // Split at last newline
            var lastNewline = value.LastIndexOf('\n');
            var before = value.Substring(0, lastNewline + 1);
            var after = value.Substring(lastNewline + 1);

            // Flush everything up to and including the newline
            _buffer.Add(before);
            var text = string.Concat(_buffer);
            _buffer.Clear();
            _buffer.Add(after);

            _flushQueue.Add(text);
        }
        else
        {
            // Just buffer
            _buffer.Add(value);
        }
    }
}
```

### Flush Thread

Background thread processes the queue:

```csharp
private void WriteThread()
{
    while (true)
    {
        var item = _flushQueue.Take();

        if (item is DoneSentinel)
            break;

        if (item is not string text || string.IsNullOrEmpty(text))
            continue;

        // Collect more text if available
        var builder = new StringBuilder(text);
        while (_flushQueue.TryTake(out var more))
        {
            if (more is DoneSentinel)
            {
                // Put it back and exit
                _flushQueue.Add(more);
                break;
            }
            if (more is string moreText)
                builder.Append(moreText);
        }

        var loop = GetAppLoop();
        WriteAndFlush(loop, builder.ToString());

        // Delay to batch writes
        if (loop != null)
        {
            Thread.Sleep(TimeSpan.FromSeconds(_sleepBetweenWrites));
        }
    }
}
```

### Write Through run_in_terminal

```csharp
private void WriteAndFlush(SynchronizationContext? loop, string text)
{
    void DoWrite()
    {
        // Enable autowrap (Windows may disable it after flush)
        _output.EnableAutowrap();

        if (_raw)
            _output.WriteRaw(text);
        else
            _output.Write(text);

        _output.Flush();
    }

    void DoWriteInLoop()
    {
        // Use run_in_terminal for thread-safe output
        Application.RunInTerminal(DoWrite, inExecutor: false);
    }

    if (loop == null)
    {
        DoWrite();
    }
    else
    {
        loop.Post(_ => DoWriteInLoop(), null);
    }
}
```

### Getting App Event Loop

```csharp
private SynchronizationContext? GetAppLoop()
{
    var app = _appSession.App;
    return app?.SynchronizationContext;
}
```

### patch_stdout Implementation

```csharp
public static IDisposable PatchStdout(bool raw = false)
{
    var proxy = new StdoutProxy(raw: raw);
    var originalOut = Console.Out;
    var originalError = Console.Error;

    Console.SetOut(proxy);
    Console.SetError(proxy);

    return new DisposableAction(() =>
    {
        Console.SetOut(originalOut);
        Console.SetError(originalError);
        proxy.Dispose();
    });
}
```

### Closing the Proxy

```csharp
public void Close()
{
    if (_closed) return;

    // Signal thread to stop
    _flushQueue.Add(new DoneSentinel());

    // Wait for thread to finish
    _flushThread.Join();

    _closed = true;
}

protected override void Dispose(bool disposing)
{
    if (disposing)
    {
        Close();
        _flushQueue.Dispose();
    }
    base.Dispose(disposing);
}
```

### TextWriter Compatibility

```csharp
public override Encoding Encoding => _output.Encoding;

public override void Write(char value)
{
    Write(value.ToString());
}

public override void Flush()
{
    lock (_lock)
    {
        var text = string.Concat(_buffer);
        _buffer.Clear();
        _flushQueue.Add(text);
    }
}

public bool IsAtty => _output.Stdout?.IsAtty ?? false;
```

## Dependencies

- `Stroke.Application.AppSession` (Feature 49) - App session context
- `Stroke.Application.RunInTerminal` (Feature 37) - Thread-safe terminal operations
- `Stroke.Output.IOutput` (Feature 51) - Output abstraction

## Implementation Tasks

1. Implement `StdoutProxy` constructor
2. Implement write buffering logic
3. Implement flush queue with background thread
4. Implement `WriteAndFlush` with run_in_terminal integration
5. Implement `Close` and `Dispose`
6. Implement TextWriter interface
7. Implement `PatchStdout` context manager
8. Write comprehensive unit tests

## Acceptance Criteria

- [ ] PatchStdout replaces Console.Out and Console.Error
- [ ] Text is buffered until newline
- [ ] Writes are batched to reduce repaints
- [ ] Background thread processes queue
- [ ] run_in_terminal used when app is running
- [ ] Flush forces output of buffered content
- [ ] Close stops background thread
- [ ] Original streams restored on dispose
- [ ] Thread-safe operation
- [ ] Unit tests achieve 80% coverage
