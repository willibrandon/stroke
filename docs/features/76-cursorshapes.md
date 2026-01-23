# Feature 76: Cursor Shapes

## Overview

Implement cursor shape configuration for dynamic cursor appearance based on editing mode and application state.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/cursor_shapes.py`

## Public API

### CursorShape Enum

```csharp
namespace Stroke;

/// <summary>
/// Cursor shape options for terminal display.
/// </summary>
public enum CursorShape
{
    /// <summary>
    /// Never change the cursor shape. This is the default to allow
    /// external cursor shape management.
    /// </summary>
    NeverChange,

    /// <summary>
    /// Block cursor (solid rectangle).
    /// </summary>
    Block,

    /// <summary>
    /// Beam cursor (vertical line).
    /// </summary>
    Beam,

    /// <summary>
    /// Underline cursor (horizontal line at bottom).
    /// </summary>
    Underline,

    /// <summary>
    /// Blinking block cursor.
    /// </summary>
    BlinkingBlock,

    /// <summary>
    /// Blinking beam cursor.
    /// </summary>
    BlinkingBeam,

    /// <summary>
    /// Blinking underline cursor.
    /// </summary>
    BlinkingUnderline
}
```

### CursorShapeConfig Abstract Base

```csharp
namespace Stroke;

/// <summary>
/// Abstract base for cursor shape configuration.
/// </summary>
public abstract class CursorShapeConfig
{
    /// <summary>
    /// Get the cursor shape for the current application state.
    /// </summary>
    /// <param name="application">The current application.</param>
    /// <returns>The cursor shape to display.</returns>
    public abstract CursorShape GetCursorShape(Application application);
}
```

### SimpleCursorShapeConfig

```csharp
namespace Stroke;

/// <summary>
/// Always show the given cursor shape.
/// </summary>
public class SimpleCursorShapeConfig : CursorShapeConfig
{
    /// <summary>
    /// The cursor shape to always display.
    /// </summary>
    public CursorShape CursorShape { get; }

    /// <summary>
    /// Create a simple cursor shape config.
    /// </summary>
    /// <param name="cursorShape">The shape to use.</param>
    public SimpleCursorShapeConfig(CursorShape cursorShape = CursorShape.NeverChange);

    /// <inheritdoc/>
    public override CursorShape GetCursorShape(Application application);
}
```

### ModalCursorShapeConfig

```csharp
namespace Stroke;

/// <summary>
/// Show cursor shape according to the current input mode.
/// Returns Block for Vi navigation, Beam for insert/Emacs, Underline for replace.
/// </summary>
public class ModalCursorShapeConfig : CursorShapeConfig
{
    /// <inheritdoc/>
    public override CursorShape GetCursorShape(Application application);
}
```

### DynamicCursorShapeConfig

```csharp
namespace Stroke;

/// <summary>
/// Dynamic cursor shape based on a callback.
/// </summary>
public class DynamicCursorShapeConfig : CursorShapeConfig
{
    /// <summary>
    /// Create a dynamic cursor shape config.
    /// </summary>
    /// <param name="getCursorShapeConfig">Callback to get the config.</param>
    public DynamicCursorShapeConfig(Func<CursorShapeConfig?> getCursorShapeConfig);

    /// <inheritdoc/>
    public override CursorShape GetCursorShape(Application application);
}
```

### Conversion Function

```csharp
namespace Stroke;

public static class CursorShapeConfigExtensions
{
    /// <summary>
    /// Convert a CursorShape or CursorShapeConfig to a CursorShapeConfig.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A CursorShapeConfig instance.</returns>
    public static CursorShapeConfig ToCursorShapeConfig(this CursorShape? value);

    /// <summary>
    /// Convert a CursorShapeConfig to itself (identity).
    /// </summary>
    public static CursorShapeConfig ToCursorShapeConfig(this CursorShapeConfig? value);
}
```

## Project Structure

```
src/Stroke/
├── CursorShape.cs
├── CursorShapeConfig.cs
├── SimpleCursorShapeConfig.cs
├── ModalCursorShapeConfig.cs
└── DynamicCursorShapeConfig.cs
tests/Stroke.Tests/
└── CursorShapeTests.cs
```

## Implementation Notes

### ModalCursorShapeConfig Logic

```csharp
public override CursorShape GetCursorShape(Application application)
{
    if (application.EditingMode == EditingMode.Vi)
    {
        return application.ViState.InputMode switch
        {
            InputMode.Navigation => CursorShape.Block,
            InputMode.Insert or InputMode.InsertMultiple => CursorShape.Beam,
            InputMode.Replace or InputMode.ReplaceSingle => CursorShape.Underline,
            _ => CursorShape.Block
        };
    }
    else if (application.EditingMode == EditingMode.Emacs)
    {
        return CursorShape.Beam;
    }

    return CursorShape.Block;
}
```

### Escape Sequences for Cursor Shapes

The VT100 output should emit these sequences:

| Shape | Escape Sequence |
|-------|-----------------|
| Block | `\x1b[2 q` |
| BlinkingBlock | `\x1b[1 q` |
| Underline | `\x1b[4 q` |
| BlinkingUnderline | `\x1b[3 q` |
| Beam | `\x1b[6 q` |
| BlinkingBeam | `\x1b[5 q` |

### Integration with Output

```csharp
// In Vt100Output
public void SetCursorShape(CursorShape shape)
{
    if (shape == CursorShape.NeverChange)
        return;

    var code = shape switch
    {
        CursorShape.Block => 2,
        CursorShape.BlinkingBlock => 1,
        CursorShape.Underline => 4,
        CursorShape.BlinkingUnderline => 3,
        CursorShape.Beam => 6,
        CursorShape.BlinkingBeam => 5,
        _ => 0
    };

    WriteRaw($"\x1b[{code} q");
}
```

## Dependencies

- Feature 1: Core enums (EditingMode)
- Feature 33: Vi state (InputMode)

## Implementation Tasks

1. Implement `CursorShape` enum
2. Implement `CursorShapeConfig` abstract base
3. Implement `SimpleCursorShapeConfig`
4. Implement `ModalCursorShapeConfig` with mode detection
5. Implement `DynamicCursorShapeConfig`
6. Add conversion extension methods
7. Integrate with VT100 output
8. Write unit tests

## Acceptance Criteria

- [ ] CursorShape enum has all shape values
- [ ] SimpleCursorShapeConfig returns constant shape
- [ ] ModalCursorShapeConfig returns correct shape per mode
- [ ] DynamicCursorShapeConfig invokes callback
- [ ] NeverChange shape does not emit escape sequences
- [ ] Output correctly emits cursor shape sequences
- [ ] Unit tests achieve 80% coverage
