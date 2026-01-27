# Research: Layout Dimensions

**Feature**: 016-layout-dimensions
**Date**: 2026-01-26

## Overview

This research documents the analysis of Python Prompt Toolkit's `dimension.py` module and the mapping to C# implementation.

## Python Source Analysis

**Source File**: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/layout/dimension.py`

### Public API Inventory

From `__all__`:
1. `Dimension` - Main class for size constraints
2. `D` - Alias for Dimension
3. `sum_layout_dimensions` - Sum aggregation function
4. `max_layout_dimensions` - Max aggregation function
5. `AnyDimension` - Type alias (Union type)
6. `to_dimension` - Conversion function
7. `is_dimension` - Validation function

### Dimension Class Analysis

```python
class Dimension:
    def __init__(self, min=None, max=None, weight=None, preferred=None):
        # Validation
        assert weight is None or weight >= 0
        assert min is None or min >= 0
        assert max is None or max >= 0
        assert preferred is None or preferred >= 0

        # Track what was specified
        self.min_specified = min is not None
        self.max_specified = max is not None
        self.preferred_specified = preferred is not None
        self.weight_specified = weight is not None

        # Apply defaults
        min = 0 if min is None else min
        max = 1000**10 if max is None else max  # ~10^30
        preferred = min if preferred is None else preferred
        weight = 1 if weight is None else weight

        # Store values
        self.min = min
        self.max = max
        self.preferred = preferred
        self.weight = weight

        # Validation: max >= min
        if max < min:
            raise ValueError("Invalid Dimension: max < min.")

        # Clamp preferred to [min, max]
        if self.preferred < self.min:
            self.preferred = self.min
        if self.preferred > self.max:
            self.preferred = self.max

    @classmethod
    def exact(cls, amount): return cls(min=amount, max=amount, preferred=amount)

    @classmethod
    def zero(cls): return cls.exact(amount=0)

    def __repr__(self): # Shows only specified parameters
```

### Key Design Decisions

| Decision | Rationale | C# Adaptation |
|----------|-----------|---------------|
| Default max = 1000^10 (10^30) | Effectively unlimited | Use `int` with `1_000_000_000` (10^9) - see rationale below |
| Assert for validation | Python idiom | Use `ArgumentException` / `ArgumentOutOfRangeException` |
| Properties are mutable | Python allows reassignment | Make properties `{ get; }` only (init in constructor) |
| `__repr__` format | Debug output | Override `ToString()` with same format |

## C# Implementation Decisions

### Type Choices

| Python | C# | Rationale |
|--------|-----|-----------|
| `int \| None` | `int?` | Nullable value type for optional parameters |
| `min`, `max`, etc. | `int` | Non-nullable after construction with defaults applied |
| `weight` | `int` | Python uses int, matches 1:1 |
| `1000**10` (10^30) | `1_000_000_000` (10^9) | Python uses arbitrarily large int; C# uses int with 10^9 as practical equivalent |

### Default Max Value

Python uses `1000**10` which is approximately 10^30 - a deliberate "something huge" value. This doesn't fit in C# numeric types without using arbitrary-precision integers.

**Analysis**:
- Terminal sizes are bounded by practical limits (typically < 1000 columns/rows)
- Even very large terminal buffers rarely exceed 10,000 dimensions
- `int.MaxValue` (2^31-1 ≈ 2.1×10^9) is more than sufficient
- Using 10^9 keeps all dimension values as `int` for simplicity

**Final Decision**: Use `int` with constant `MaxDimensionValue = 1_000_000_000` (10^9). This is effectively unlimited for terminal UIs while keeping all dimension values as `int`. This is a documented deviation from Python's 10^30 value, chosen for C# type system compatibility.

### Validation Approach

Python uses `assert` statements. C# equivalents:
- `weight >= 0`: `ArgumentOutOfRangeException`
- `min >= 0`: `ArgumentOutOfRangeException`
- `max >= min`: `ArgumentException` with clear message

### AnyDimension Type

Python's `AnyDimension = Union[None, int, Dimension, Callable[[], Any]]`

C# equivalent: Use `object?` as parameter type in `ToDimension()` with runtime type checking, since C# doesn't have union types. Alternative would be overloads.

**Decision**: Use `object?` parameter with runtime checks, matching Python's duck-typing approach.

## Algorithm Analysis

### sum_layout_dimensions

Simple summation:
```python
min = sum(d.min for d in dimensions)
max = sum(d.max for d in dimensions)
preferred = sum(d.preferred for d in dimensions)
return Dimension(min=min, max=max, preferred=preferred)
```

C# implementation will use LINQ `.Sum()`.

### max_layout_dimensions

More complex algorithm:
1. Return zero if empty list
2. Return zero if all dimensions are zero (preferred=0 and max=0)
3. Filter out zero dimensions
4. Take highest min
5. For max: `max(min(d.max), max(d.preferred))` - don't shrink below largest preferred
6. If min > max (non-overlapping ranges), set max = min
7. Take highest preferred

## Alternatives Considered

| Alternative | Rejected Because |
|-------------|------------------|
| `record struct Dimension` | Python uses class with mutable-looking properties; `sealed class` is more faithful |
| Overloaded `ToDimension` methods | More complex API; Python uses single function with type checking |
| Generic `Dimension<T>` | Over-engineering; Python uses simple int |

## Dependencies

None. This is a leaf module with no external dependencies.

## References

- Python source: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/layout/dimension.py`
- Spec: `specs/016-layout-dimensions/spec.md`
- Constitution: `.specify/memory/constitution.md`
