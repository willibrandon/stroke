# Feature 18: Cursor Shape Configuration

## Overview

Implement cursor shape configuration classes for changing the terminal cursor appearance based on editing mode.

**Note:** The `CursorShape` enum is defined in Feature 15 (Output) to avoid circular dependencies. This feature provides the configuration classes that use that enum.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/cursor_shapes.py`

## Public API

### ICursorShapeConfig Interface

```csharp
namespace Stroke.CursorShapes;

using Stroke.Output; // For CursorShape enum

/// <summary>
/// Abstract base for cursor shape configuration.
/// </summary>
public interface ICursorShapeConfig
{
    /// <summary>
    /// Return the cursor shape to be used in the current state.
    /// </summary>
    /// <param name="application">The current application.</param>
    /// <typeparam name="TResult">The application result type.</typeparam>
    CursorShape GetCursorShape<TResult>(Application<TResult> application);
}
```

### SimpleCursorShapeConfig Class

```csharp
namespace Stroke.CursorShapes;

/// <summary>
/// Always show the given cursor shape.
/// </summary>
public sealed class SimpleCursorShapeConfig : ICursorShapeConfig
{
    /// <summary>
    /// Creates a simple cursor shape config.
    /// </summary>
    /// <param name="cursorShape">The cursor shape to always use.</param>
    public SimpleCursorShapeConfig(CursorShape cursorShape = CursorShape.NeverChange);

    /// <summary>
    /// The cursor shape.
    /// </summary>
    public CursorShape CursorShape { get; }

    public CursorShape GetCursorShape<TResult>(Application<TResult> application) => CursorShape;
}
```

### ModalCursorShapeConfig Class

```csharp
namespace Stroke.CursorShapes;

/// <summary>
/// Show cursor shape according to the current input mode.
/// - Vi navigation mode: Block
/// - Vi insert mode: Beam
/// - Vi replace mode: Underline
/// - Emacs mode: Beam
/// </summary>
public sealed class ModalCursorShapeConfig : ICursorShapeConfig
{
    public CursorShape GetCursorShape<TResult>(Application<TResult> application);
}
```

### DynamicCursorShapeConfig Class

```csharp
namespace Stroke.CursorShapes;

/// <summary>
/// Dynamic cursor shape configuration.
/// </summary>
public sealed class DynamicCursorShapeConfig : ICursorShapeConfig
{
    /// <summary>
    /// Creates a dynamic cursor shape config.
    /// </summary>
    /// <param name="getCursorShapeConfig">Callable that returns the cursor shape config.</param>
    public DynamicCursorShapeConfig(Func<ICursorShapeConfig?> getCursorShapeConfig);

    /// <summary>
    /// The callable that returns the cursor shape config.
    /// </summary>
    public Func<ICursorShapeConfig?> GetCursorShapeConfig { get; }

    public CursorShape GetCursorShape<TResult>(Application<TResult> application);
}
```

### ToCursorShapeConfig Function

```csharp
namespace Stroke.CursorShapes;

/// <summary>
/// Cursor shape configuration utilities.
/// </summary>
public static class CursorShapeConfigUtils
{
    /// <summary>
    /// Take a CursorShape or ICursorShapeConfig and turn it into an ICursorShapeConfig.
    /// </summary>
    /// <param name="value">CursorShape, ICursorShapeConfig, or null.</param>
    /// <returns>An ICursorShapeConfig instance.</returns>
    public static ICursorShapeConfig ToCursorShapeConfig(CursorShape? value);

    /// <summary>
    /// Take a CursorShape or ICursorShapeConfig and turn it into an ICursorShapeConfig.
    /// </summary>
    /// <param name="value">ICursorShapeConfig or null.</param>
    /// <returns>An ICursorShapeConfig instance.</returns>
    public static ICursorShapeConfig ToCursorShapeConfig(ICursorShapeConfig? value);
}
```

## Project Structure

```
src/Stroke/
└── CursorShapes/
    ├── ICursorShapeConfig.cs
    ├── SimpleCursorShapeConfig.cs
    ├── ModalCursorShapeConfig.cs
    ├── DynamicCursorShapeConfig.cs
    └── CursorShapeConfigUtils.cs
tests/Stroke.Tests/
└── CursorShapes/
    ├── SimpleCursorShapeConfigTests.cs
    ├── ModalCursorShapeConfigTests.cs
    ├── DynamicCursorShapeConfigTests.cs
    └── CursorShapeConfigUtilsTests.cs
```

**Note:** The `CursorShape` enum and its tests are in Feature 15 (Output) at `src/Stroke/Output/CursorShape.cs`.

## Implementation Notes

### VT100 Escape Sequences

Cursor shapes are set using DECSCUSR (DEC Set Cursor Style):
- `\x1b[0 q` - Default cursor
- `\x1b[1 q` - Blinking block
- `\x1b[2 q` - Steady block
- `\x1b[3 q` - Blinking underline
- `\x1b[4 q` - Steady underline
- `\x1b[5 q` - Blinking beam (bar)
- `\x1b[6 q` - Steady beam (bar)

### Modal Cursor Shapes

The `ModalCursorShapeConfig` changes cursor shape based on editing mode:

| Mode | Cursor Shape |
|------|--------------|
| Vi Navigation | Block |
| Vi Insert | Beam |
| Vi Insert Multiple | Beam |
| Vi Replace | Underline |
| Vi Replace Single | Underline |
| Emacs | Beam |

### Default Behavior

By default (`CursorShape.NeverChange`), cursor shape escape sequences are not sent. This prevents interference with custom cursor shape handling in applications like IPython.

### Cursor Shape Tracking

The output layer tracks whether the cursor shape was ever changed. On exit, it only resets the cursor shape if it was modified during the session.

## Dependencies

- `Stroke.Output.CursorShape` (Feature 15) - Cursor shape enum
- `Stroke.Application.Application<TResult>` (Feature 31) - Application class
- `Stroke.KeyBinding.EditingMode` (Feature 27) - Editing mode enum
- `Stroke.KeyBinding.ViState` (Feature 28) - Vi input mode

## Implementation Tasks

1. Implement `ICursorShapeConfig` interface
2. Implement `SimpleCursorShapeConfig` class
3. Implement `ModalCursorShapeConfig` class
4. Implement `DynamicCursorShapeConfig` class
5. Implement `CursorShapeConfigUtils` static class
6. Write comprehensive unit tests

## Acceptance Criteria

- [ ] All cursor shapes match Python Prompt Toolkit semantics
- [ ] Modal cursor shape changes work correctly
- [ ] Dynamic cursor shape configuration works correctly
- [ ] Unit tests achieve 80% coverage
