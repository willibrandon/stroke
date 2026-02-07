# Research: Progress Bar and Print Text Examples

**Feature**: 068-progressbar-printtext-examples
**Date**: 2026-02-07
**Status**: Complete — 0 unknowns remaining

## Research Summary

No NEEDS CLARIFICATION items were identified in the Technical Context. All technologies, patterns, and APIs are already established in the codebase. Research focused on confirming existing patterns and identifying the correct API surface for each example.

## Findings

### R-001: Existing Example Project Pattern

**Decision**: Follow the exact pattern from Stroke.Examples.Prompts (class-based Program.cs with static dictionary routing)

**Rationale**: Four existing example projects (Prompts, FullScreen, Dialogs, Choices) all use the same pattern: `internal static class Program` with `Dictionary<string, Action>` and `StringComparer.OrdinalIgnoreCase`. This is the canonical pattern.

**Alternatives considered**:
- Top-level statements (used by Telnet/Ssh) — rejected because those are server examples with CancellationToken; not applicable here
- Minimal API style — rejected because it deviates from the established pattern

### R-002: PrintText API Surface (FormattedTextOutput)

**Decision**: All 9 print text examples use `FormattedTextOutput.Print()` and `FormattedTextOutput.PrintContainer()` from `Stroke.Shortcuts`

**Rationale**: Confirmed by reading `FormattedTextOutput.cs`:
- `Print(AnyFormattedText text, ...)` — accepts string, Html, Ansi, FormattedText, PygmentsTokens
- `Print(object[] values, ...)` — multi-value overload with sep/end
- `PrintContainer(AnyContainer container, ...)` — for Frame/TextArea rendering
- Parameters: `style`, `colorDepth`, `flush`, `file`, `output`, `styleTransformation`, `includeDefaultPygmentsStyle`

**Key API types**:
- `AnyFormattedText` — union type accepting string/Html/Ansi/FormattedText/PygmentsTokens
- `Html` — from `Stroke.FormattedText`
- `Ansi` — from `Stroke.FormattedText`
- `FormattedText` — from `Stroke.FormattedText` (list of `StyleAndTextTuple`)
- `PygmentsTokens` — from `Stroke.FormattedText`
- `Style.FromDict()` — from `Stroke.Styles`
- `ColorDepth` — from `Stroke.Output` (Depth1Bit, Depth4Bit, Depth8Bit, Depth24Bit)
- `Frame`, `TextArea` — from `Stroke.Widgets`
- `AnyContainer` — from `Stroke.Layout.Containers`

### R-003: ProgressBar API Surface (Feature 71 — Not Yet Implemented)

**Decision**: Port examples against the expected C# API surface as defined in docs/examples-mapping.md and Python Prompt Toolkit patterns

**Rationale**: The Python API pattern is:
```python
with ProgressBar(title=..., bottom_toolbar=..., style=..., key_bindings=..., formatters=...) as pb:
    for i in pb(range(800), label=...):
        time.sleep(0.01)
```

The expected C# equivalent (from examples-mapping.md) is:
```csharp
await using var pb = new ProgressBar(title: ..., bottomToolbar: ..., style: ..., keyBindings: ..., formatters: ...);
await foreach (var i in pb.Iterate(Enumerable.Range(0, 800), label: ...))
{
    await Task.Delay(10);
}
```

**Key expected types** (Feature 71):
- `ProgressBar` — implements `IAsyncDisposable`, constructor accepts title, bottomToolbar, style, keyBindings, formatters
- `ProgressBar.Iterate<T>(IEnumerable<T>, label)` — returns `IAsyncEnumerable<T>` wrapping a `ProgressBarCounter<T>`
- `ProgressBarCounter<T>` — has mutable `Label` property, `RemoveWhenDone` parameter
- 10 Formatter implementations: `Label`, `Text`, `Bar`, `Percentage`, `SpinningWheel`, `TimeLeft`, `TimeElapsed`, `IterationsPerSecond`, `Progress`, `Rainbow`

**Risk**: These examples cannot be runtime-tested until Feature 71 is implemented. They will be written to compile against the expected API shape.

### R-004: Threading Pattern for Parallel Tasks

**Decision**: Use `Thread` with daemon-equivalent and timeout-based joins, matching the Python pattern

**Rationale**: Python uses `threading.Thread(target=..., daemon=True)` with `t.join(timeout=0.5)` for Windows Ctrl-C compatibility. The C# equivalent:
```csharp
var thread = new Thread(() => RunTask(...)) { IsBackground = true };
thread.Start();
// ...
while (thread.IsAlive)
    thread.Join(TimeSpan.FromMilliseconds(500));
```

`IsBackground = true` is the .NET equivalent of Python's `daemon = True` — background threads are automatically terminated when the main thread exits.

**Alternatives considered**:
- `Task.Run()` with async/await — rejected because Python uses explicit threads, and the faithful port principle requires matching the threading model
- `Parallel.ForEach` — rejected for same reason; Python creates individual threads

### R-005: Signal Handling (CustomKeyBindings 'x' key)

**Decision**: Use `Environment.FailFast` or raise `KeyboardInterruptException` instead of POSIX `os.kill(os.getpid(), signal.SIGINT)`

**Rationale**: Python's `signal.SIGINT` sends Ctrl-C to the process. The cross-platform C# equivalent is to raise the same exception that Ctrl-C would generate. The `KeyboardInterruptException` from Stroke matches this behavior.

**Alternatives considered**:
- P/Invoke to `kill()` on Unix / `GenerateConsoleCtrlEvent` on Windows — rejected as overly complex for an example
- `Process.GetCurrentProcess().Kill()` — rejected as too destructive (no cleanup)

### R-006: Color Depth Prompt (StyledRainbow)

**Decision**: Use `Stroke.Shortcuts.Prompt.Confirm()` to ask user about color depth, matching Python's `confirm()` call

**Rationale**: Python's `styled-rainbow.py` uses:
```python
true_color = confirm("Is your terminal a true-color terminal?")
```
The Stroke equivalent from the Shortcuts layer provides the same functionality.

### R-007: NamedColors Dictionary

**Decision**: Use `Stroke.Styles.NamedColors.Colors` dictionary, already implemented in Feature 114

**Rationale**: Confirmed that the NamedColors system exists. The example iterates over all named colors and displays them at each color depth.

### R-008: PygmentsTokens and Lexer Integration

**Decision**: Use `PygmentsTokens` from `Stroke.FormattedText` with manual token lists and optionally `TextMateLineLexer` for lexer-based output

**Rationale**: The Python example uses both manual token lists and `PythonLexer().get_tokens()`. The C# port will use manual `PygmentsTokens` lists (matching the Python manual example) and potentially `TextMateLineLexer` for the lexer-based portion.

### R-009: Solution File Format

**Decision**: Add two project entries to `examples/Stroke.Examples.sln` with new GUIDs

**Rationale**: The .sln file uses standard VS solution format. New projects need:
1. A `Project` entry with unique GUID and relative `.csproj` path
2. Build configuration mappings in `ProjectConfigurationPlatforms` for all 6 config combinations (Debug/Release × Any CPU/x64/x86)

The pattern is well-established from the 5 existing project entries.
