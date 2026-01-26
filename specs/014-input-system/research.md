# Research: Input System

**Feature**: 014-input-system
**Date**: 2026-01-25

## Research Summary

This document resolves all technical unknowns for implementing the Input System in Stroke.

---

## R1: .NET Terminal Mode Control

### Question
How do we control terminal modes (raw/cooked) on .NET across Windows, macOS, and Linux?

### Decision
Use P/Invoke to call native terminal APIs:
- **POSIX (Linux/macOS)**: Use `libc` P/Invoke for `tcgetattr`, `tcsetattr`, `cfmakeraw`
- **Windows**: Use `kernel32.dll` P/Invoke for `GetConsoleMode`, `SetConsoleMode`

### Rationale
.NET does not provide managed APIs for terminal mode control. The `System.Console` class abstracts away low-level terminal settings. Direct P/Invoke is the standard approach used by:
- Terminal.Gui (our reference implementation)
- Spectre.Console
- System.CommandLine

### Alternatives Considered
1. **Third-party NuGet package**: Rejected - adds dependency; Constitution III requires zero external dependencies for Stroke.Input
2. **Console.ReadKey(intercept: true)**: Rejected - only intercepts individual keys, doesn't control echo/canonical modes
3. **Process.Start with stty**: Rejected - spawning subprocesses is slow and fragile

### Implementation Notes
```csharp
// POSIX termios structure (simplified)
[StructLayout(LayoutKind.Sequential)]
internal struct Termios
{
    public uint c_iflag;   // Input modes
    public uint c_oflag;   // Output modes
    public uint c_cflag;   // Control modes
    public uint c_lflag;   // Local modes
    // ... cc array and speeds
}

// Key flags to modify:
// ICANON (canonical mode - line buffering)
// ECHO (echo input)
// ISIG (generate signals from Ctrl+C, Ctrl+Z)
// IXON/IXOFF (flow control)
```

---

## R2: VT100 Escape Sequence Parsing Strategy

### Question
What parsing strategy should we use for VT100 escape sequences?

### Decision
Implement a coroutine-style state machine using iterators, matching Python Prompt Toolkit's `Vt100Parser._input_parser_generator()` pattern.

### Rationale
The VT100 parser must handle:
1. **Partial sequences**: ESC followed by more bytes that may arrive later
2. **Ambiguous prefixes**: ESC[ could be start of many sequences
3. **Timeout handling**: Standalone ESC key vs start of escape sequence
4. **Bracketed paste**: Multi-character content between markers

A state machine with explicit continuation (via flush) handles all these cases elegantly.

### Alternatives Considered
1. **Regex matching**: Rejected - can't handle partial input or state
2. **Switch statement**: Rejected - doesn't handle continuation between calls
3. **External parser library**: Rejected - adds dependency

### Implementation Notes
```csharp
public sealed class Vt100Parser
{
    private readonly Action<KeyPress> _feedKeyCallback;
    private ParserState _state = ParserState.Ground;
    private readonly StringBuilder _buffer = new();

    public void Feed(string data)
    {
        foreach (char c in data)
        {
            ProcessChar(c);
        }
    }

    public void Flush()
    {
        // Emit buffered partial sequence as individual keys
        // Critical for standalone Escape key detection
    }
}
```

---

## R3: ANSI Sequence Dictionary Structure

### Question
How should we store and look up ANSI escape sequences?

### Decision
Use a `FrozenDictionary<string, Keys>` for escape sequence → key mapping, with a prefix cache for partial match detection.

### Rationale
- **FrozenDictionary** (.NET 8+): Optimized for read-heavy workloads with no modifications
- **Prefix cache**: Dictionary indicating whether a prefix could match a longer sequence (enables waiting vs. emitting)
- Python Prompt Toolkit uses `_IS_PREFIX_OF_LONGER_MATCH_CACHE` for this purpose

### Implementation Notes
```csharp
public static class AnsiSequences
{
    // Primary lookup: sequence → key
    public static FrozenDictionary<string, Keys> Sequences { get; } =
        new Dictionary<string, Keys>
        {
            ["\x1b[A"] = Keys.Up,
            ["\x1b[B"] = Keys.Down,
            ["\x1b[C"] = Keys.Right,
            ["\x1b[D"] = Keys.Left,
            ["\x1bOP"] = Keys.F1,
            ["\x1bOQ"] = Keys.F2,
            // ... ~100 more sequences
        }.ToFrozenDictionary();

    // Reverse lookup: key → sequence (for Windows compatibility)
    public static FrozenDictionary<Keys, string> ReverseSequences { get; }

    // Prefix cache: is this prefix potentially the start of a longer sequence?
    public static FrozenSet<string> ValidPrefixes { get; }
}
```

---

## R4: Event Loop Integration Pattern

### Question
How should input sources integrate with async event loops in .NET?

### Decision
Use `IDisposable` context pattern for attach/detach, with `Action` callbacks for input-ready notification.

### Rationale
.NET async patterns differ from Python's event loop:
- Python: `loop.add_reader(fd, callback)` / `loop.remove_reader(fd)`
- .NET: No direct equivalent; use `SynchronizationContext` or callbacks

The callback pattern allows integration with any event loop implementation (custom, `System.Threading.Channels`, etc.).

### Alternatives Considered
1. **IAsyncEnumerable<KeyPress>**: Rejected - doesn't support attach/detach semantics
2. **Events (C# event keyword)**: Rejected - would change API shape from Python
3. **Observable (Rx.NET)**: Rejected - adds dependency

### Implementation Notes
```csharp
public interface IInput
{
    /// <summary>
    /// Attach this input to the current event loop with a callback.
    /// </summary>
    /// <returns>Disposable that detaches on dispose.</returns>
    IDisposable Attach(Action inputReadyCallback);

    /// <summary>
    /// Temporarily detach from event loop.
    /// </summary>
    /// <returns>Disposable that reattaches on dispose.</returns>
    IDisposable Detach();
}
```

---

## R5: Windows Console Input Modes

### Question
How do we handle Windows console input for both legacy and modern terminals?

### Decision
Detect Windows 10 VT100 support at runtime and use appropriate reader:
- **Win10+ with VT100**: Use `Vt100ConsoleInputReader` (reads raw sequences)
- **Legacy Windows**: Use `ConsoleInputReader` (reads INPUT_RECORD structures)

### Rationale
Windows 10 1809+ supports `ENABLE_VIRTUAL_TERMINAL_INPUT` flag, which makes the console emit VT100 sequences instead of INPUT_RECORD events. This allows sharing the `Vt100Parser` code.

Python Prompt Toolkit uses `_is_win_vt100_input_enabled()` to detect this.

### Implementation Notes
```csharp
public static class ConsoleApi
{
    // Console mode flags
    public const uint ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200;

    public static bool IsVt100InputEnabled()
    {
        if (!OperatingSystem.IsWindows()) return false;

        var handle = GetStdHandle(STD_INPUT_HANDLE);
        if (!GetConsoleMode(handle, out uint mode)) return false;

        return (mode & ENABLE_VIRTUAL_TERMINAL_INPUT) != 0;
    }
}
```

---

## R6: File Descriptor Handling on .NET

### Question
How do we get file descriptors from .NET streams for event loop registration?

### Decision
Use `SafeFileHandle.DangerousGetHandle()` on POSIX, and `GetStdHandle()` on Windows.

### Rationale
.NET 7+ provides `Console.OpenStandardInput().SafeFileHandle` which wraps the native handle. For event loop registration, we need the raw descriptor/handle.

### Implementation Notes
```csharp
// POSIX
public int FileNo()
{
    var handle = Console.OpenStandardInput().SafeFileHandle;
    return handle.DangerousGetHandle().ToInt32();
}

// Windows
public IntPtr Handle => ConsoleApi.GetStdHandle(ConsoleApi.STD_INPUT_HANDLE);
```

---

## R7: Bracketed Paste Mode

### Question
How should bracketed paste be handled?

### Decision
Detect `\x1b[200~` (start) and `\x1b[201~` (end) sequences. Accumulate all content between markers and emit as a single `KeyPress` with `Keys.BracketedPaste` and the pasted content as data.

### Rationale
Bracketed paste mode (enabled by application output) allows distinguishing typed input from pasted content. Applications can handle pastes differently (e.g., no command history for pasted commands).

### Implementation Notes
```csharp
// In Vt100Parser state machine
if (sequence == "\x1b[200~")
{
    _inBracketedPaste = true;
    _pasteBuffer.Clear();
}
else if (sequence == "\x1b[201~" && _inBracketedPaste)
{
    _inBracketedPaste = false;
    _feedKeyCallback(new KeyPress(Keys.BracketedPaste, _pasteBuffer.ToString()));
}
```

---

## R8: Mouse Event Detection

### Question
How should mouse events be detected and parsed?

### Decision
Recognize mouse event escape sequences by pattern matching (regex) since coordinates are variable:
- **X10**: `\x1b[M<button><x><y>`
- **SGR**: `\x1b[<btn;x;y[mM]`
- **urxvt**: `\x1b[<btn;x;yM`

Emit as `Keys.Vt100MouseEvent` with the raw sequence as data.

### Rationale
Mouse coordinates vary, so we can't use a static dictionary. Pattern matching identifies the sequence type, and the raw data is passed to the MouseEvent parser (already implemented in Feature 013).

### Implementation Notes
```csharp
// Mouse event detection regex patterns
private static readonly Regex X10MousePattern = new(@"^\x1b\[M...$");
private static readonly Regex SgrMousePattern = new(@"^\x1b\[<\d+;\d+;\d+[mM]$");

private bool IsMouse(string sequence)
{
    return X10MousePattern.IsMatch(sequence) ||
           SgrMousePattern.IsMatch(sequence);
}
```

---

## R9: Pipe Input Implementation

### Question
How should pipe input work for testing scenarios?

### Decision
Create a platform-specific pipe:
- **POSIX**: Use `pipe()` system call via P/Invoke; read end becomes stdin
- **Windows**: Use Windows event object for signaling

### Rationale
Pipe input allows programmatic feeding of test data through the full VT100 parser, enabling integration tests without real terminal interaction.

### Implementation Notes
```csharp
// POSIX pipe
[DllImport("libc")]
private static extern int pipe(int[] pipefd);

public static PosixPipeInput Create(string initialText = "")
{
    var fds = new int[2];
    pipe(fds); // fds[0] = read, fds[1] = write

    var input = new PosixPipeInput(fds[0], fds[1]);
    if (!string.IsNullOrEmpty(initialText))
        input.SendText(initialText);
    return input;
}
```

---

## R10: Conditional Compilation Strategy

### Question
How should platform-specific code be organized?

### Decision
Use `#if` conditional compilation with runtime OS checks:
- `#if WINDOWS` / `#if !WINDOWS` for compile-time exclusion
- `OperatingSystem.IsWindows()` / `OperatingSystem.IsLinux()` / `OperatingSystem.IsMacOS()` for runtime checks

### Rationale
Some APIs (P/Invoke signatures) are platform-specific at compile time. Runtime checks handle cross-compiled assemblies.

### Implementation Notes
```csharp
// InputFactory.cs
public static IInput Create(Stream? stdin = null, bool alwaysPreferTty = false)
{
    if (OperatingSystem.IsWindows())
        return new Win32Input();
    else
        return new Vt100Input(stdin ?? Console.OpenStandardInput());
}
```

---

## Summary of Decisions

| Research Topic | Decision |
|----------------|----------|
| R1: Terminal Mode Control | P/Invoke to libc (POSIX) and kernel32 (Windows) |
| R2: VT100 Parsing | State machine with explicit flush for partial sequences |
| R3: ANSI Sequences | FrozenDictionary with prefix cache |
| R4: Event Loop | IDisposable attach/detach with Action callbacks |
| R5: Windows Modes | Runtime VT100 detection, dual reader implementations |
| R6: File Descriptors | SafeFileHandle.DangerousGetHandle() / GetStdHandle() |
| R7: Bracketed Paste | Accumulate between markers, emit as single KeyPress |
| R8: Mouse Events | Regex pattern matching, emit with raw data |
| R9: Pipe Input | Platform-specific pipes (POSIX pipe(), Windows events) |
| R10: Conditional Compilation | #if directives + OperatingSystem.Is* runtime checks |
