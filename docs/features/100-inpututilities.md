# Feature 100: Input Utilities

## Overview

Implement input utility classes including typeahead buffer management, PosixStdinReader for low-level stdin reading with incremental UTF-8 decoding, and FlushStdout for safe stdout flushing with blocking I/O handling.

## Python Prompt Toolkit Reference

**Source:**
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/input/typeahead.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/input/posix_utils.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/output/flush_stdout.py`

## Public API

### Typeahead Buffer

```csharp
namespace Stroke.Input;

/// <summary>
/// Typeahead buffer management for storing key strokes read ahead.
/// When reading input in chunks, we may read more than needed for the current
/// prompt. This module stores those extra key strokes for the next prompt.
/// </summary>
public static class Typeahead
{
    /// <summary>
    /// Store typeahead key presses for the given input.
    /// </summary>
    /// <param name="input">The input source.</param>
    /// <param name="keyPresses">Key presses to store.</param>
    public static void Store(IInput input, IReadOnlyList<KeyPress> keyPresses);

    /// <summary>
    /// Retrieve typeahead key presses for the given input and clear the buffer.
    /// </summary>
    /// <param name="input">The input source.</param>
    /// <returns>List of stored key presses.</returns>
    public static IReadOnlyList<KeyPress> Get(IInput input);

    /// <summary>
    /// Clear the typeahead buffer for the given input.
    /// </summary>
    /// <param name="input">The input source.</param>
    public static void Clear(IInput input);
}
```

### PosixStdinReader

```csharp
namespace Stroke.Input;

/// <summary>
/// Wrapper around stdin which reads (non-blocking) the next available bytes
/// and decodes them using incremental UTF-8 decoding.
/// </summary>
/// <remarks>
/// Note that you can't be sure the input stream is closed if Read() returns
/// an empty string. When using 'ignore' error handling, Read() can return
/// empty if all malformed input was replaced. Check the Closed property.
///
/// Error handling modes:
/// - "surrogateescape" (default): Allows transferring unrecognized bytes to
///   key bindings. Some terminals use 'Mxx' notation for mouse events.
/// - "ignore": Ignores malformed input (useful for noisy input streams).
/// - "replace": Replaces malformed input with replacement character.
/// - "strict": Throws on malformed input.
/// </remarks>
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
public sealed class PosixStdinReader : IDisposable
{
    /// <summary>
    /// Create a POSIX stdin reader.
    /// </summary>
    /// <param name="stdinFd">File descriptor to read from.</param>
    /// <param name="errors">Error handling mode.</param>
    /// <param name="encoding">Character encoding (default: UTF-8).</param>
    public PosixStdinReader(
        int stdinFd,
        string errors = "surrogateescape",
        string encoding = "utf-8");

    /// <summary>
    /// The file descriptor being read.
    /// </summary>
    public int StdinFd { get; }

    /// <summary>
    /// Error handling mode.
    /// </summary>
    public string Errors { get; }

    /// <summary>
    /// Whether the stream has been closed.
    /// </summary>
    public bool Closed { get; }

    /// <summary>
    /// Read input and return it as a string.
    /// </summary>
    /// <param name="count">Maximum bytes to read (default: 1024).</param>
    /// <returns>
    /// The decoded text. May return empty even if stream is not closed
    /// (e.g., when decoding failed with 'ignore' mode).
    /// </returns>
    public string Read(int count = 1024);

    /// <inheritdoc/>
    public void Dispose();
}
```

### FlushStdout

```csharp
namespace Stroke.Output;

/// <summary>
/// Helper for safely flushing stdout with proper blocking I/O handling.
/// Handles edge cases like non-blocking I/O (uvloop), encoding issues,
/// and interrupt signals.
/// </summary>
public static class FlushStdout
{
    /// <summary>
    /// Flush data to stdout, handling blocking I/O and interrupts.
    /// </summary>
    /// <param name="stdout">The text writer to flush to.</param>
    /// <param name="data">Data to write.</param>
    /// <remarks>
    /// This method:
    /// - Ensures stdout is in blocking mode during write (for async runtimes)
    /// - Encodes with 'replace' to handle unsupported characters
    /// - Ignores EINTR (window resize signals)
    /// - Ignores errno 0 (Ctrl+C during heavy output)
    /// </remarks>
    public static void Flush(TextWriter stdout, string data);
}
```

## Project Structure

```
src/Stroke/
├── Input/
│   ├── Typeahead.cs
│   └── PosixStdinReader.cs  (Unix-only)
└── Output/
    └── FlushStdout.cs
tests/Stroke.Tests/
├── Input/
│   ├── TypeaheadTests.cs
│   └── PosixStdinReaderTests.cs
└── Output/
    └── FlushStdoutTests.cs
```

## Implementation Notes

### Typeahead Implementation

```csharp
public static class Typeahead
{
    private static readonly Dictionary<string, List<KeyPress>> _buffer = new();
    private static readonly object _lock = new();

    public static void Store(IInput input, IReadOnlyList<KeyPress> keyPresses)
    {
        var key = input.TypeaheadHash();

        lock (_lock)
        {
            if (!_buffer.TryGetValue(key, out var list))
            {
                list = new List<KeyPress>();
                _buffer[key] = list;
            }
            list.AddRange(keyPresses);
        }
    }

    public static IReadOnlyList<KeyPress> Get(IInput input)
    {
        var key = input.TypeaheadHash();

        lock (_lock)
        {
            if (_buffer.TryGetValue(key, out var list))
            {
                var result = list.ToList();
                list.Clear();
                return result;
            }
            return Array.Empty<KeyPress>();
        }
    }

    public static void Clear(IInput input)
    {
        var key = input.TypeaheadHash();

        lock (_lock)
        {
            if (_buffer.TryGetValue(key, out var list))
            {
                list.Clear();
            }
        }
    }
}
```

### PosixStdinReader Implementation

```csharp
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
public sealed class PosixStdinReader : IDisposable
{
    private readonly Decoder _decoder;
    private readonly byte[] _readBuffer = new byte[1024];
    private readonly char[] _charBuffer = new char[2048];

    public PosixStdinReader(
        int stdinFd,
        string errors = "surrogateescape",
        string encoding = "utf-8")
    {
        StdinFd = stdinFd;
        Errors = errors;

        // Create decoder with specified error handling
        var enc = Encoding.GetEncoding(
            encoding,
            new EncoderReplacementFallback(),
            CreateDecoderFallback(errors));
        _decoder = enc.GetDecoder();
    }

    public int StdinFd { get; }
    public string Errors { get; }
    public bool Closed { get; private set; }

    public string Read(int count = 1024)
    {
        if (Closed)
            return string.Empty;

        // Check if data is available using poll/select
        if (!IsDataAvailable())
            return string.Empty;

        try
        {
            // Read raw bytes
            var bytesRead = PosixInterop.Read(
                StdinFd,
                _readBuffer,
                Math.Min(count, _readBuffer.Length));

            if (bytesRead == 0)
            {
                Closed = true;
                return string.Empty;
            }

            if (bytesRead < 0)
            {
                var errno = Marshal.GetLastWin32Error();
                if (errno == EINTR)
                    return string.Empty; // Window resize signal
                throw new IOException($"Read failed with errno {errno}");
            }

            // Decode incrementally (handles partial UTF-8 sequences)
            var charCount = _decoder.GetChars(
                _readBuffer, 0, bytesRead,
                _charBuffer, 0);

            return new string(_charBuffer, 0, charCount);
        }
        catch (IOException)
        {
            Closed = true;
            return string.Empty;
        }
    }

    private bool IsDataAvailable()
    {
        try
        {
            // Use poll with 0 timeout to check for available data
            var pfd = new PollFd
            {
                Fd = StdinFd,
                Events = PollEvents.POLLIN
            };
            var result = PosixInterop.Poll(ref pfd, 1, 0);
            return result > 0 && (pfd.REvents & PollEvents.POLLIN) != 0;
        }
        catch
        {
            Closed = true;
            return false;
        }
    }

    private static DecoderFallback CreateDecoderFallback(string errors)
    {
        return errors switch
        {
            "ignore" => new DecoderReplacementFallback(string.Empty),
            "replace" => new DecoderReplacementFallback("\uFFFD"),
            "strict" => new DecoderExceptionFallback(),
            "surrogateescape" => new SurrogateEscapeFallback(),
            _ => new DecoderReplacementFallback("\uFFFD")
        };
    }

    private const int EINTR = 4;

    public void Dispose()
    {
        Closed = true;
    }
}
```

### FlushStdout Implementation

```csharp
public static class FlushStdout
{
    public static void Flush(TextWriter stdout, string data)
    {
        try
        {
            using var _ = EnsureBlocking(stdout);

            // Try binary write if possible for proper encoding handling
            if (stdout is StreamWriter sw && sw.BaseStream != null)
            {
                var encoding = sw.Encoding ?? Encoding.UTF8;
                var bytes = encoding.GetBytes(data);
                sw.BaseStream.Write(bytes);
                sw.BaseStream.Flush();
            }
            else
            {
                stdout.Write(data);
                stdout.Flush();
            }
        }
        catch (IOException e)
        {
            // EINTR: Interrupted by signal (e.g., window resize)
            if (e.HResult == EINTR || GetErrno(e) == EINTR)
                return;

            // errno 0: Can happen with Ctrl+C during heavy output
            if (e.HResult == 0 || GetErrno(e) == 0)
                return;

            throw;
        }
    }

    private static IDisposable EnsureBlocking(TextWriter writer)
    {
        if (!OperatingSystem.IsWindows() &&
            writer is StreamWriter sw &&
            sw.BaseStream is FileStream fs)
        {
            try
            {
                var fd = (int)fs.SafeFileHandle.DangerousGetHandle();
                var wasBlocking = PosixInterop.GetBlocking(fd);

                if (!wasBlocking)
                {
                    PosixInterop.SetBlocking(fd, true);
                    return new BlockingRestorer(fd, wasBlocking);
                }
            }
            catch
            {
                // Ignore errors - assume blocking
            }
        }

        return NullDisposable.Instance;
    }

    private const int EINTR = 4;

    private static int GetErrno(IOException e)
    {
        // Platform-specific errno extraction
        return Marshal.GetLastWin32Error();
    }

    private sealed class BlockingRestorer : IDisposable
    {
        private readonly int _fd;
        private readonly bool _originalBlocking;

        public BlockingRestorer(int fd, bool originalBlocking)
        {
            _fd = fd;
            _originalBlocking = originalBlocking;
        }

        public void Dispose()
        {
            PosixInterop.SetBlocking(_fd, _originalBlocking);
        }
    }
}
```

### IInput Extension

```csharp
public interface IInput
{
    // ... existing members ...

    /// <summary>
    /// Returns a unique hash for this input source.
    /// Used for typeahead buffer identification.
    /// </summary>
    string TypeaheadHash();
}
```

## Dependencies

- Feature 7: Input abstraction (IInput)
- Feature 9: Key press events (KeyPress)

## Implementation Tasks

1. Implement Typeahead static class with thread-safe buffer
2. Add TypeaheadHash() to IInput interface
3. Implement PosixStdinReader with incremental decoding
4. Implement SurrogateEscapeFallback for Python compatibility
5. Implement FlushStdout with blocking I/O handling
6. Add PosixInterop methods for poll and blocking control
7. Write unit tests

## Acceptance Criteria

- [ ] Typeahead stores and retrieves key presses per input
- [ ] TypeaheadHash uniquely identifies input sources
- [ ] PosixStdinReader handles partial UTF-8 sequences
- [ ] PosixStdinReader supports surrogateescape mode
- [ ] FlushStdout handles non-blocking I/O
- [ ] FlushStdout ignores EINTR signals
- [ ] Thread-safe typeahead buffer operations
- [ ] Unit tests achieve 80% coverage
