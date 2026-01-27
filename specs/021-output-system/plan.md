# Implementation Plan: Output System

**Branch**: `021-output-system` | **Date**: 2026-01-27 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/021-output-system/spec.md`

## Summary

Implement the terminal output abstraction layer for Stroke, providing VT100/ANSI escape sequence support, color depth management, cursor control, and platform-specific backends. This is a faithful port of Python Prompt Toolkit's `prompt_toolkit.output` module, enabling cross-platform terminal rendering with support for color depths from 1-bit to 24-bit true color.

## Technical Context

**Language/Version**: C# 13 / .NET 10+
**Primary Dependencies**: None (Stroke.Output depends only on Stroke.Core and Stroke.Styles per Constitution III)
**Storage**: N/A (in-memory output buffers only)
**Testing**: xUnit (no mocks per Constitution VIII)
**Target Platform**: Linux, macOS, Windows 10+ (POSIX VT100 primary, Windows Console fallback)
**Project Type**: Single library (Stroke.Output namespace within Stroke project)
**Performance Goals**: Escape code caching, minimal string allocations, buffered writes
**Constraints**: Thread-safe mutable implementations per Constitution XI
**Scale/Scope**: 6 public classes, 2 enums, 1 interface, 3+ internal cache classes

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | All APIs mapped from Python Prompt Toolkit `output` module |
| II. Immutability by Default | ✅ PASS | ColorDepth/CursorShape are enums (immutable); caches are internal |
| III. Layered Architecture | ✅ PASS | Stroke.Output depends only on Stroke.Core (Size) and Stroke.Styles (Attrs) |
| IV. Cross-Platform Terminal | ✅ PASS | VT100 primary, PlainTextOutput for redirected streams, DummyOutput for testing |
| V. Complete Editing Mode Parity | N/A | Not applicable to output layer |
| VI. Performance-Conscious Design | ✅ PASS | EscapeCodeCache, _16ColorCache, _256ColorCache for caching |
| VII. Full Scope Commitment | ✅ PASS | All 25 functional requirements addressed |
| VIII. Real-World Testing | ✅ PASS | No mocks, tests against real IOutput implementations |
| IX. Adherence to Planning Docs | ✅ PASS | Following api-mapping.md for `prompt_toolkit.output` namespace |
| X. Source Code File Size Limits | ✅ PASS | Vt100Output split across multiple files if needed |
| XI. Thread Safety by Default | ✅ PASS | Vt100Output uses Lock for buffer and state |

## Project Structure

### Documentation (this feature)

```text
specs/021-output-system/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── IOutput.md       # Interface contract
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/Stroke/
├── Output/
│   ├── ColorDepth.cs              # ColorDepth enum
│   ├── IOutput.cs                 # IOutput interface
│   ├── DummyOutput.cs             # No-op output for testing
│   ├── PlainTextOutput.cs         # Non-escape output for redirected streams
│   ├── Vt100Output.cs             # VT100/ANSI output implementation
│   ├── Vt100Output.Colors.cs      # Color handling partial class
│   ├── Vt100Output.Cursor.cs      # Cursor control partial class
│   ├── OutputFactory.cs           # CreateOutput factory method
│   ├── FlushStdout.cs             # Stdout flush helper
│   └── Internal/
│       ├── SixteenColorCache.cs   # 16-color RGB mapping cache
│       ├── TwoFiftySixColorCache.cs # 256-color RGB mapping cache
│       └── EscapeCodeCache.cs     # Attrs to escape sequence cache
├── CursorShapes/
│   ├── CursorShape.cs             # CursorShape enum
│   ├── ICursorShapeConfig.cs      # Config interface
│   ├── SimpleCursorShapeConfig.cs # Static cursor shape
│   ├── ModalCursorShapeConfig.cs  # Mode-dependent cursor shape
│   └── DynamicCursorShapeConfig.cs # Dynamic cursor shape wrapper

tests/Stroke.Tests/
├── Output/
│   ├── ColorDepthTests.cs
│   ├── DummyOutputTests.cs
│   ├── PlainTextOutputTests.cs
│   ├── Vt100OutputTests.cs
│   ├── Vt100OutputColorTests.cs
│   ├── Vt100OutputCursorTests.cs
│   ├── OutputFactoryTests.cs
│   └── Internal/
│       ├── SixteenColorCacheTests.cs
│       ├── TwoFiftySixColorCacheTests.cs
│       └── EscapeCodeCacheTests.cs
└── CursorShapes/
    ├── CursorShapeTests.cs
    ├── SimpleCursorShapeConfigTests.cs
    └── ModalCursorShapeConfigTests.cs
```

**Structure Decision**: Single project structure with namespace-based organization. Output classes go in `Stroke.Output` namespace, cursor shapes in `Stroke.CursorShapes`. Internal caches are in `Stroke.Output.Internal` to hide implementation details.

## Complexity Tracking

> No constitution violations. All implementations follow established patterns.

| Component | Complexity | Justification |
|-----------|------------|---------------|
| Vt100Output | Split into partial classes | File size limit (Principle X) - cursor, colors, and core in separate files |
| Color Caches | Internal classes | Implementation detail, not part of public API |
| Thread Safety | Lock per mutable class | Constitution XI requirement |

## Dependencies

### Internal Dependencies (already implemented in Stroke)

| Dependency | Namespace | Used For |
|------------|-----------|----------|
| `Size` | `Stroke.Core.Primitives` | Terminal size (rows, columns) |
| `Attrs` | `Stroke.Styles` | Style attributes for SetAttributes |
| `AnsiColorNames` | `Stroke.Styles` | ANSI color name validation |
| `AnsiColorsToRgb` | `Stroke.Styles` | RGB values for ANSI colors |

### External Dependencies

None. Stroke.Output has zero external dependencies per Constitution III.

## API Contract Summary

### ColorDepth Enum

```csharp
namespace Stroke.Output;

public enum ColorDepth
{
    Depth1Bit,    // Monochrome
    Depth4Bit,    // 16 ANSI colors
    Depth8Bit,    // 256 colors (Default)
    Depth24Bit    // True color
}

public static class ColorDepthExtensions
{
    public static ColorDepth? FromEnvironment(); // STROKE_COLOR_DEPTH, NO_COLOR
    public static ColorDepth Default => ColorDepth.Depth8Bit;
}
```

### CursorShape Enum

```csharp
namespace Stroke.CursorShapes;

public enum CursorShape
{
    NeverChange,       // Default - don't send cursor shape sequences
    Block,             // Solid block cursor
    Beam,              // Vertical bar cursor
    Underline,         // Underline cursor
    BlinkingBlock,     // Blinking block
    BlinkingBeam,      // Blinking beam
    BlinkingUnderline  // Blinking underline
}
```

### IOutput Interface

```csharp
namespace Stroke.Output;

public interface IOutput
{
    // Writing
    void Write(string data);           // Escape VT100 sequences
    void WriteRaw(string data);        // Pass through verbatim
    void Flush();

    // Screen control
    void EraseScreen();
    void EraseEndOfLine();
    void EraseDown();

    // Alternate screen
    void EnterAlternateScreen();
    void QuitAlternateScreen();

    // Cursor movement
    void CursorGoto(int row, int column);
    void CursorUp(int amount);
    void CursorDown(int amount);
    void CursorForward(int amount);
    void CursorBackward(int amount);

    // Cursor visibility
    void HideCursor();
    void ShowCursor();
    void SetCursorShape(CursorShape shape);
    void ResetCursorShape();

    // Attributes
    void ResetAttributes();
    void SetAttributes(Attrs attrs, ColorDepth colorDepth);

    // Line wrapping
    void DisableAutowrap();
    void EnableAutowrap();

    // Mouse
    void EnableMouseSupport();
    void DisableMouseSupport();

    // Bracketed paste
    void EnableBracketedPaste();
    void DisableBracketedPaste();

    // Title
    void SetTitle(string title);
    void ClearTitle();

    // Bell
    void Bell();

    // Cursor position report
    void AskForCpr();
    bool RespondsToCpr { get; }

    // Terminal info
    Size GetSize();
    int Fileno();
    string Encoding { get; }
    ColorDepth GetDefaultColorDepth();

    // Windows-specific (optional)
    void ScrollBufferToPrompt();
    int GetRowsBelowCursorPosition();
    void ResetCursorKeyMode();
}
```

## Implementation Phases

### Phase 1: Core Types & Enums (Story 2)

1. `ColorDepth` enum with `FromEnvironment()` static method
2. `CursorShape` enum in `Stroke.CursorShapes` namespace

### Phase 2: Interface & Base Classes (Story 7)

1. `IOutput` interface with all 33 methods
2. `DummyOutput` class - no-op implementation for testing

### Phase 3: Plain Text Output (Story 6)

1. `PlainTextOutput` class - writes text without escape sequences
2. Cursor movement emulated with spaces/newlines

### Phase 4: Color Caches (Story 2)

1. `SixteenColorCache` - RGB to 16-color ANSI mapping
2. `TwoFiftySixColorCache` - RGB to 256-color mapping
3. `EscapeCodeCache` - Attrs to escape sequence caching

### Phase 5: VT100 Output Core (Story 1)

1. `Vt100Output` base class with buffer and stdout handling
2. `Vt100Output.Colors.cs` - color and attribute handling
3. `Vt100Output.Cursor.cs` - cursor control and visibility

### Phase 6: Terminal Features (Story 4)

1. Mouse support enable/disable
2. Alternate screen buffer
3. Bracketed paste mode
4. Title setting

### Phase 7: Cursor Shape Config (Story 3)

1. `ICursorShapeConfig` interface
2. `SimpleCursorShapeConfig` - static shape
3. `ModalCursorShapeConfig` - vi mode dependent
4. `DynamicCursorShapeConfig` - dynamic wrapper

### Phase 8: Factory Method (Story 5)

1. `OutputFactory.Create()` - platform detection and output creation
2. TTY detection, stderr fallback

### Phase 9: Tests

1. Unit tests for all public APIs
2. Escape sequence verification tests
3. Color mapping tests (exact match with Python Prompt Toolkit)
4. Thread safety concurrent tests

## VT100 Escape Sequences Reference

| Operation | Escape Sequence |
|-----------|-----------------|
| Cursor Goto | `\x1b[{row};{col}H` |
| Cursor Up | `\x1b[{n}A` (or `\x1b[A` for n=1) |
| Cursor Down | `\x1b[{n}B` (or `\x1b[B` for n=1) |
| Cursor Forward | `\x1b[{n}C` (or `\x1b[C` for n=1) |
| Cursor Backward | `\x1b[{n}D` (or `\b` for n=1) |
| Erase Screen | `\x1b[2J` |
| Erase End of Line | `\x1b[K` |
| Erase Down | `\x1b[J` |
| Reset Attributes | `\x1b[0m` |
| Hide Cursor | `\x1b[?25l` |
| Show Cursor | `\x1b[?12l\x1b[?25h` |
| Alternate Screen Enter | `\x1b[?1049h\x1b[H` |
| Alternate Screen Exit | `\x1b[?1049l` |
| Mouse Enable | `\x1b[?1000h`, `\x1b[?1003h`, `\x1b[?1015h`, `\x1b[?1006h` |
| Mouse Disable | `\x1b[?1000l`, `\x1b[?1015l`, `\x1b[?1006l`, `\x1b[?1003l` |
| Bracketed Paste Enable | `\x1b[?2004h` |
| Bracketed Paste Disable | `\x1b[?2004l` |
| Set Title | `\x1b]2;{title}\x07` |
| Bell | `\a` |
| Cursor Position Request | `\x1b[6n` |
| Disable Autowrap | `\x1b[?7l` |
| Enable Autowrap | `\x1b[?7h` |
| Cursor Key Mode Reset | `\x1b[?1l` |

### Cursor Shapes (DECSCUSR)

| Shape | Escape Sequence |
|-------|-----------------|
| Block | `\x1b[2 q` |
| Blinking Block | `\x1b[1 q` |
| Underline | `\x1b[4 q` |
| Blinking Underline | `\x1b[3 q` |
| Beam | `\x1b[6 q` |
| Blinking Beam | `\x1b[5 q` |
| Reset | `\x1b[0 q` |

### Color Escape Sequences

| Color Type | Escape Sequence |
|------------|-----------------|
| 16-color FG | `\x1b[{30-37,90-97}m` |
| 16-color BG | `\x1b[{40-47,100-107}m` |
| 256-color FG | `\x1b[38;5;{0-255}m` |
| 256-color BG | `\x1b[48;5;{0-255}m` |
| True color FG | `\x1b[38;2;{r};{g};{b}m` |
| True color BG | `\x1b[48;2;{r};{g};{b}m` |

## Risk Mitigation

| Risk | Mitigation |
|------|------------|
| Platform differences in terminal detection | Use .NET Console.IsOutputRedirected, fall back to PlainTextOutput |
| Thread contention on output buffer | Use System.Threading.Lock with EnterScope() pattern |
| Memory pressure from escape code caching | Use weak references or bounded cache size |
| Color mapping accuracy | Test against Python Prompt Toolkit reference values |
