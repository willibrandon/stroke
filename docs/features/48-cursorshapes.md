# Feature 48: Cursor Shapes

## Overview

Implement the cursor shape system for controlling terminal cursor appearance including CursorShape enum and various CursorShapeConfig implementations for static, modal, and dynamic cursor shape control.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/cursor_shapes.py`

## Public API

### CursorShape Enum

```csharp
namespace Stroke;

/// <summary>
/// Terminal cursor shapes.
/// </summary>
public enum CursorShape
{
    /// <summary>
    /// Never change cursor shape (default for compatibility).
    /// </summary>
    NeverChange,

    /// <summary>
    /// Solid block cursor.
    /// </summary>
    Block,

    /// <summary>
    /// Vertical bar cursor.
    /// </summary>
    Beam,

    /// <summary>
    /// Horizontal line cursor.
    /// </summary>
    Underline,

    /// <summary>
    /// Blinking block cursor.
    /// </summary>
    BlinkingBlock,

    /// <summary>
    /// Blinking vertical bar cursor.
    /// </summary>
    BlinkingBeam,

    /// <summary>
    /// Blinking horizontal line cursor.
    /// </summary>
    BlinkingUnderline
}
```

### CursorShapeConfig Abstract Class

```csharp
namespace Stroke;

/// <summary>
/// Configuration for cursor shape.
/// </summary>
public abstract class CursorShapeConfig
{
    /// <summary>
    /// Return the cursor shape to be used in the current state.
    /// </summary>
    /// <param name="application">The current application.</param>
    /// <returns>The cursor shape to use.</returns>
    public abstract CursorShape GetCursorShape(Application application);
}
```

### SimpleCursorShapeConfig Class

```csharp
namespace Stroke;

/// <summary>
/// Always show the given cursor shape.
/// </summary>
public sealed class SimpleCursorShapeConfig : CursorShapeConfig
{
    /// <summary>
    /// Creates a SimpleCursorShapeConfig.
    /// </summary>
    /// <param name="cursorShape">The fixed cursor shape to use.</param>
    public SimpleCursorShapeConfig(CursorShape cursorShape = CursorShape.NeverChange);

    /// <summary>
    /// The cursor shape.
    /// </summary>
    public CursorShape CursorShape { get; }

    public override CursorShape GetCursorShape(Application application);
}
```

### ModalCursorShapeConfig Class

```csharp
namespace Stroke;

/// <summary>
/// Show cursor shape according to the current input mode.
/// </summary>
public sealed class ModalCursorShapeConfig : CursorShapeConfig
{
    public override CursorShape GetCursorShape(Application application);
}
```

### DynamicCursorShapeConfig Class

```csharp
namespace Stroke;

/// <summary>
/// Cursor shape config that dynamically returns a config.
/// </summary>
public sealed class DynamicCursorShapeConfig : CursorShapeConfig
{
    /// <summary>
    /// Creates a DynamicCursorShapeConfig.
    /// </summary>
    /// <param name="getCursorShapeConfig">Callable returning cursor shape config.</param>
    public DynamicCursorShapeConfig(Func<AnyCursorShapeConfig?> getCursorShapeConfig);

    public override CursorShape GetCursorShape(Application application);
}
```

### AnyCursorShapeConfig Type

```csharp
namespace Stroke;

/// <summary>
/// Union type for cursor shape configuration.
/// Can be CursorShape enum, CursorShapeConfig, or null.
/// </summary>
/// <remarks>
/// In C#, this is typically handled by overloads or implicit conversions.
/// </remarks>
```

### Conversion Function

```csharp
namespace Stroke;

public static class CursorShapes
{
    /// <summary>
    /// Convert a CursorShape or CursorShapeConfig to CursorShapeConfig.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A CursorShapeConfig instance.</returns>
    public static CursorShapeConfig ToCursorShapeConfig(AnyCursorShapeConfig? value);
}
```

## Project Structure

```
src/Stroke/
├── CursorShape.cs
├── CursorShapeConfig.cs
├── SimpleCursorShapeConfig.cs
├── ModalCursorShapeConfig.cs
├── DynamicCursorShapeConfig.cs
└── CursorShapes.cs
tests/Stroke.Tests/
├── CursorShapeTests.cs
├── SimpleCursorShapeConfigTests.cs
├── ModalCursorShapeConfigTests.cs
└── DynamicCursorShapeConfigTests.cs
```

## Implementation Notes

### ModalCursorShapeConfig Logic

The modal config changes cursor based on editing mode:

```csharp
public override CursorShape GetCursorShape(Application application)
{
    if (application.EditingMode == EditingMode.Vi)
    {
        return application.ViState.InputMode switch
        {
            InputMode.Navigation => CursorShape.Block,
            InputMode.Insert => CursorShape.Beam,
            InputMode.InsertMultiple => CursorShape.Beam,
            InputMode.Replace => CursorShape.Underline,
            InputMode.ReplaceSingle => CursorShape.Underline,
            _ => CursorShape.Block
        };
    }
    else if (application.EditingMode == EditingMode.Emacs)
    {
        return CursorShape.Beam;  // Like Vi insert mode
    }

    return CursorShape.Block;  // Default
}
```

### NeverChange Default

The `NeverChange` value is the default because:
- Some applications (like IPython) monkey-patch cursor shape handling
- Sending cursor shape escape sequences could interfere with existing workarounds
- Applications must explicitly opt-in to cursor shape changes

### Cursor Shape Escape Sequences

The output layer translates cursor shapes to VT sequences:

| Shape | Escape Sequence |
|-------|----------------|
| Block | `\x1b[2 q` |
| BlinkingBlock | `\x1b[1 q` |
| Underline | `\x1b[4 q` |
| BlinkingUnderline | `\x1b[3 q` |
| Beam | `\x1b[6 q` |
| BlinkingBeam | `\x1b[5 q` |
| NeverChange | (no output) |

### DynamicCursorShapeConfig

Wraps a callable that returns any cursor shape config:

```csharp
public override CursorShape GetCursorShape(Application application)
{
    var config = ToCursorShapeConfig(_getCursorShapeConfig());
    return config.GetCursorShape(application);
}
```

### ToCursorShapeConfig Conversion

```csharp
public static CursorShapeConfig ToCursorShapeConfig(AnyCursorShapeConfig? value)
{
    if (value is null)
        return new SimpleCursorShapeConfig();

    if (value is CursorShape shape)
        return new SimpleCursorShapeConfig(shape);

    return (CursorShapeConfig)value;
}
```

## Dependencies

- `Stroke.Application` (Feature 31) - Application class
- `Stroke.KeyBinding.ViState` (Feature 38) - Vi input modes
- `Stroke.Enums.EditingMode` (Feature 07) - Editing mode enum

## Implementation Tasks

1. Implement `CursorShape` enum
2. Implement `CursorShapeConfig` abstract class
3. Implement `SimpleCursorShapeConfig` class
4. Implement `ModalCursorShapeConfig` with mode logic
5. Implement `DynamicCursorShapeConfig` class
6. Implement `ToCursorShapeConfig` conversion
7. Integrate with output layer for escape sequences
8. Write comprehensive unit tests

## Acceptance Criteria

- [ ] CursorShape enum has all values
- [ ] SimpleCursorShapeConfig returns fixed shape
- [ ] ModalCursorShapeConfig varies by Vi mode
- [ ] ModalCursorShapeConfig returns Beam for Emacs
- [ ] DynamicCursorShapeConfig delegates correctly
- [ ] ToCursorShapeConfig handles all input types
- [ ] NeverChange is the default (no escape sequences)
- [ ] Unit tests achieve 80% coverage
