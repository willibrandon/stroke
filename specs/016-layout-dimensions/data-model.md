# Data Model: Layout Dimensions

**Feature**: 016-layout-dimensions
**Date**: 2026-01-26

## Entities

### Dimension

Represents size constraints for a layout element (width or height).

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| Min | int | Minimum allowed size | 0 |
| Max | int | Maximum allowed size | 1,000,000,000 (10^9) |
| Preferred | int | Preferred size (layout engine tries to honor) | Same as Min |
| Weight | int | Proportional weight for split containers | 1 |
| MinSpecified | bool | True if min was explicitly provided | false |
| MaxSpecified | bool | True if max was explicitly provided | false |
| PreferredSpecified | bool | True if preferred was explicitly provided | false |
| WeightSpecified | bool | True if weight was explicitly provided | false |

**Invariants**:
- `Min >= 0`
- `Max >= 0`
- `Max >= Min`
- `Weight >= 0`
- `Preferred` is clamped to `[Min, Max]`

**Lifecycle**: Immutable after construction.

**Identity**: Value equality based on all properties.

**Terminology**: A dimension with min=max=preferred=0 is called a "zero dimension" or "invisible element" (synonyms). Such elements occupy no space in the layout.

### AnyDimension (Conceptual Type)

Not a concrete type in C#, but represents the set of values that can be converted to a Dimension:
- `null` → Default Dimension (no constraints)
- `int` → Exact Dimension (min = max = preferred = value)
- `Dimension` → Passthrough
- `Func<object?>` → Recursive evaluation (invoked synchronously)

**IsDimension Behavior**: Returns true for callable types without invoking them. This matches Python's duck-typing approach but cannot guarantee the callable will produce a valid dimension at runtime.

## Relationships

```text
┌─────────────────────────────────────────────────────────────┐
│                        Dimension                            │
│  ┌─────────┐ ┌─────────┐ ┌───────────┐ ┌────────┐          │
│  │   Min   │ │   Max   │ │ Preferred │ │ Weight │          │
│  │  (int)  │ │  (int)  │ │   (int)   │ │ (int)  │          │
│  └─────────┘ └─────────┘ └───────────┘ └────────┘          │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────────────┐│
│  │ MinSpecified │ │ MaxSpecified │ │ PreferredSpecified   ││
│  │   (bool)     │ │   (bool)     │ │       (bool)         ││
│  └──────────────┘ └──────────────┘ └──────────────────────┘│
│  ┌─────────────────┐                                       │
│  │ WeightSpecified │                                       │
│  │     (bool)      │                                       │
│  └─────────────────┘                                       │
└─────────────────────────────────────────────────────────────┘

Factory Methods:
  Dimension.Exact(amount) → Dimension where min=max=preferred=amount
  Dimension.Zero()        → Dimension.Exact(0)

Utility Functions:
  SumLayoutDimensions(dimensions) → Aggregated Dimension (additive)
  MaxLayoutDimensions(dimensions) → Aggregated Dimension (maximum)
  ToDimension(value)              → Dimension (with type coercion)
  IsDimension(value)              → bool (type validation)
```

## State Transitions

Dimension is immutable - no state transitions occur after construction.

## Validation Rules

| Rule | Enforcement | Error Type | Message |
|------|-------------|------------|---------|
| min >= 0 | Constructor | ArgumentOutOfRangeException | (parameter name: "min") |
| max >= 0 | Constructor | ArgumentOutOfRangeException | (parameter name: "max") |
| preferred >= 0 | Constructor | ArgumentOutOfRangeException | (parameter name: "preferred") |
| weight >= 0 | Constructor | ArgumentOutOfRangeException | (parameter name: "weight") |
| max >= min | Constructor | ArgumentException | "Invalid Dimension: max < min." |
| preferred clamping | Constructor | Auto-corrected, no error | N/A |
| null list to aggregation | SumLayoutDimensions/MaxLayoutDimensions | ArgumentNullException | (parameter name: "dimensions") |
| unsupported type | ToDimension | ArgumentException | "Not an integer or Dimension object." |

## Constants

| Name | Value | Purpose |
|------|-------|---------|
| MaxDimensionValue | 1,000,000,000 | Default for unspecified max (effectively unlimited) |
| DefaultWeight | 1 | Default weight for proportional sizing |

## Type Mappings (Python → C#)

| Python | C# |
|--------|-----|
| `Dimension` | `Dimension` (sealed class) |
| `D` | `D` (static class alias with factory methods) |
| `sum_layout_dimensions` | `DimensionUtils.SumLayoutDimensions` |
| `max_layout_dimensions` | `DimensionUtils.MaxLayoutDimensions` |
| `to_dimension` | `DimensionUtils.ToDimension` |
| `is_dimension` | `DimensionUtils.IsDimension` |
| `AnyDimension` | `object?` (no direct equivalent) |
| `LayoutDimension` | Not ported (deprecated alias in Python) |
