# Data Model: Mouse Bindings

**Feature**: 036-mouse-bindings
**Date**: 2026-01-30

## Entities

### Lookup Table Value Tuple

The three lookup tables all produce the same output shape:

| Field | Type | Description |
|-------|------|-------------|
| Button | `MouseButton` | Which button (Left, Middle, Right, None, Unknown) |
| EventType | `MouseEventType` | Event category (MouseDown, MouseUp, MouseMove, ScrollUp, ScrollDown) |
| Modifiers | `MouseModifiers` | Flags enum combining None, Shift, Alt, Control |

### XTerm SGR Lookup Table

**Key**: `(int Code, char Suffix)` — the numeric event code and the trailing 'M' (press) or 'm' (release) character.

**Size**: 108 entries total:
- 3 buttons × 2 directions (up/down) × 8 modifier combos = 48 (left/middle/right up + down)
- 4 drag sources × 8 modifier combos = 32 (left/middle/right/none move)
- 2 scroll directions × 8 modifier combos = 16 (scroll up/down)
- Subtotal with suffix: 24 entries use 'm' suffix (up only), 84 entries use 'M' suffix (down + drag + scroll)

**Modifier encoding** (bit-field in the code value):
- Bit 2 (value 4): Shift
- Bit 3 (value 8): Alt
- Bit 4 (value 16): Control

**Button encoding** (bits 0-1 in the code value):
- 0: Left, 1: Middle, 2: Right, 3: None/Release

**Special ranges**:
- 0-30 with 'm': Mouse up events
- 0-30 with 'M': Mouse down events
- 32-63 with 'M': Drag/move events (bit 5 = 32 offset)
- 64-93 with 'M': Scroll events

### Typical (X10) Lookup Table

**Key**: `int` — the raw byte value from the mouse event packet (3rd byte after `ESC[M`).

**Size**: 10 entries

| Code | Button | EventType | Notes |
|------|--------|-----------|-------|
| 32 | Left | MouseDown | |
| 33 | Middle | MouseDown | |
| 34 | Right | MouseDown | |
| 35 | Unknown | MouseUp | Button not known on release |
| 64 | Left | MouseMove | Drag |
| 65 | Middle | MouseMove | Drag |
| 66 | Right | MouseMove | Drag |
| 67 | None | MouseMove | Motion without button |
| 96 | None | ScrollUp | |
| 97 | None | ScrollDown | |

All entries have `MouseModifiers.None` (treated as unknown — the Typical protocol does not encode modifiers).

### URXVT Lookup Table

**Key**: `int` — the decimal event code parsed from the URXVT sequence.

**Size**: 4 entries

| Code | Button | EventType | Notes |
|------|--------|-----------|-------|
| 32 | Unknown | MouseDown | URXVT doesn't distinguish buttons |
| 35 | Unknown | MouseUp | |
| 96 | None | ScrollUp | |
| 97 | None | ScrollDown | |

All entries have `MouseModifiers.None` (treated as unknown — URXVT protocol does not encode modifiers).

### Coordinate Transformations

| Protocol | Raw Encoding | Step 1 | Step 2 | Final |
|----------|-------------|--------|--------|-------|
| XTerm SGR | 1-based integers in sequence | — | Subtract 1 from x and y | 0-based |
| Typical | Byte values (char ordinal) | Subtract 0xDC00 if ≥ 0xDC00 (surrogate escape) | Subtract 32, then subtract 1 | 0-based |
| URXVT | 1-based integers in sequence | — | Subtract 1 from x and y | 0-based |
| Windows | 0-based integers | — | Subtract rows-above-cursor from y | Layout-relative |

After protocol-specific coordinate extraction, all VT100 paths apply a final adjustment:
- `y -= Renderer.RowsAboveLayout` (accounts for terminal content above the application layout)

## State Transitions

None — `MouseBindings` is a static, stateless class. No mutable state. Lookup tables are initialized once at static construction time.

## Relationships

```
MouseBindings (static class)
├── produces → KeyBindings (4 registered bindings)
├── reads → KeyPressEvent.Data (raw escape sequence)
├── reads → Application.Renderer.HeightIsKnown
├── reads → Application.Renderer.RowsAboveLayout
├── reads → Application.Renderer.MouseHandlers
├── writes → KeyProcessor.Feed (for scroll fallback)
└── calls → MouseHandlers.GetHandler(x, y) → Func<MouseEvent, NotImplementedOrNone>
```
