# Contract: StdoutProxy

**Namespace**: `Stroke.Application`
**Python Source**: `prompt_toolkit.patch_stdout.StdoutProxy`

## Class Definition

```csharp
/// <summary>
/// A <see cref="TextWriter"/> that intercepts console output and routes it above
/// the current active prompt. Compatible with other TextWriter consumers and can
/// be used as a drop-in replacement for <see cref="Console.Out"/> or passed to
/// logging handlers.
/// </summary>
/// <remarks>
/// <para>
/// The current application, above which output is printed, is determined by the
/// <see cref="AppSession"/> that is active during construction of this instance.
/// </para>
/// <para>
/// To avoid continuous prompt repainting for every small write, a short delay
/// of <see cref="SleepBetweenWrites"/> is added between consecutive flushes to
/// batch many smaller writes in a short timespan.
/// </para>
/// <para>
/// This class is thread-safe. Multiple threads may write concurrently.
/// </para>
/// </remarks>
public sealed class StdoutProxy : TextWriter, IDisposable
```

## Constructor

```csharp
/// <summary>
/// Creates a new <see cref="StdoutProxy"/> that routes output above the current prompt.
/// </summary>
/// <param name="sleepBetweenWrites">
/// Delay between consecutive flushes to batch rapid writes. Default: 200ms.
/// </param>
/// <param name="raw">
/// When <c>true</c>, VT100 escape sequences are passed through unmodified.
/// When <c>false</c> (default), escape sequences are escaped by the output system.
/// </param>
public StdoutProxy(TimeSpan? sleepBetweenWrites = null, bool raw = false);
```

## Properties

```csharp
/// <summary>
/// Gets the delay between consecutive flushes to batch rapid writes.
/// </summary>
public TimeSpan SleepBetweenWrites { get; }

/// <summary>
/// Gets whether raw mode is enabled (VT100 escape sequences pass through unmodified).
/// </summary>
public bool Raw { get; }

/// <summary>
/// Gets whether this proxy has been closed/disposed.
/// </summary>
public bool Closed { get; }

/// <summary>
/// Gets the original stdout stream that this proxy wraps.
/// </summary>
public TextWriter? OriginalStdout { get; }

/// <summary>
/// Gets the encoding of the underlying output.
/// </summary>
public override Encoding Encoding { get; }
```

## Methods

```csharp
/// <summary>
/// Writes text to the proxy. Text is buffered until a newline is encountered,
/// at which point complete lines are queued for output.
/// </summary>
/// <param name="value">The text to write.</param>
public override void Write(string? value);

/// <summary>
/// Writes a single character to the proxy.
/// </summary>
/// <param name="value">The character to write.</param>
public override void Write(char value);

/// <summary>
/// Flushes all buffered content to the output queue, even without a trailing newline.
/// </summary>
public override void Flush();

/// <summary>
/// Closes the proxy, flushing remaining content and terminating the background thread.
/// Idempotent — safe to call multiple times.
/// </summary>
public void Close();

/// <summary>
/// Returns the file descriptor of the underlying output stream.
/// </summary>
/// <returns>The file descriptor number.</returns>
public int Fileno();

/// <summary>
/// Returns whether the underlying output is a terminal.
/// </summary>
/// <returns><c>true</c> if the output is a terminal; otherwise <c>false</c>.</returns>
public bool IsAtty();
```

## IDisposable

```csharp
/// <summary>
/// Disposes the proxy by calling <see cref="Close"/>.
/// </summary>
protected override void Dispose(bool disposing);
```

## Python Correspondence

| Python Member | C# Member | Notes |
|---------------|-----------|-------|
| `__init__(sleep_between_writes, raw)` | Constructor | `float` → `TimeSpan?` |
| `sleep_between_writes` | `SleepBetweenWrites` | |
| `raw` | `Raw` | |
| `closed` | `Closed` | |
| `original_stdout` | `OriginalStdout` | |
| `encoding` | `Encoding` | `str` → `System.Text.Encoding` |
| `errors` | (omitted) | Python TextIO concept, not needed in .NET |
| `write(data)` | `Write(string?)` | Returns `void` (TextWriter contract) |
| `flush()` | `Flush()` | |
| `close()` | `Close()` | |
| `fileno()` | `Fileno()` | |
| `isatty()` | `IsAtty()` | |
| `__enter__` / `__exit__` | `IDisposable` | Context manager → Dispose pattern |
| `_write(data)` | (internal) | Newline-gated buffer logic |
| `_flush()` | (internal) | Buffer → queue flush |
| `_start_write_thread()` | (internal) | Background thread creation |
| `_write_thread()` | (internal) | Flush thread main loop |
| `_get_app_loop()` | (internal) | App detection |
| `_write_and_flush(loop, text)` | (internal) | Output coordination |
