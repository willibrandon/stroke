# Feature Specification: Layout Dimensions

**Feature Branch**: `016-layout-dimensions`
**Created**: 2026-01-26
**Status**: Draft
**Input**: User description: "Implement the dimension system used to specify minimum, maximum, preferred, and weighted sizes for layout containers and controls."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Define Control Size Constraints (Priority: P1)

A UI developer building a terminal application needs to specify size constraints for controls (windows, containers, widgets). They want to set minimum and maximum bounds while indicating a preferred size that the layout engine should try to honor.

**Why this priority**: This is the foundational capability. Without the ability to create and configure dimensions, no other layout sizing behavior is possible.

**Independent Test**: Can be fully tested by creating Dimension objects with various parameter combinations and verifying the resulting constraints are correctly stored and accessible.

**Acceptance Scenarios**:

1. **Given** no size constraints specified, **When** a dimension is created with default values, **Then** it uses minimum of 0, maximum of 1,000,000,000 (10^9), preferred equal to minimum, and weight of 1.
2. **Given** explicit min, max, preferred, and weight values, **When** a dimension is created with these values, **Then** all properties reflect the specified values exactly.
3. **Given** a preferred value outside the min/max range, **When** a dimension is created, **Then** the preferred value is automatically clamped to fit within the min/max bounds.
4. **Given** max less than min, **When** a dimension is created, **Then** the system rejects the invalid configuration with an appropriate error.

---

### User Story 2 - Proportional Sizing with Weights (Priority: P1)

A UI developer wants to create split layouts (horizontal or vertical) where child elements share available space proportionally. For example, one panel should always be twice as wide as another.

**Why this priority**: Weight-based proportional sizing is essential for flexible layouts and is equally fundamental to the dimension system as min/max/preferred constraints.

**Independent Test**: Can be fully tested by creating dimensions with different weights and verifying the weight values are correctly tracked.

**Acceptance Scenarios**:

1. **Given** two dimensions, one with weight 1 and another with weight 2, **When** used in a split container, **Then** the second element's allocated size should be twice that of the first (subject to min/max constraints).
2. **Given** a dimension with no explicit weight specified, **When** accessed, **Then** the weight defaults to 1.
3. **Given** a weight of 0 specified, **When** the dimension is created, **Then** the weight is accepted (element receives no proportional space beyond its minimum).

---

### User Story 3 - Fixed-Size Elements (Priority: P2)

A UI developer needs to create elements with an exact, fixed size that should not grow or shrink regardless of available terminal space. Examples include toolbars, status bars, or fixed-width sidebars.

**Why this priority**: Fixed-size elements are common in terminal UIs but are a convenience built on top of the core min/max/preferred system.

**Independent Test**: Can be fully tested by creating an exact dimension and verifying min, max, and preferred are all set to the same value.

**Acceptance Scenarios**:

1. **Given** a desired fixed size of 10, **When** an exact dimension is created, **Then** min, max, and preferred are all set to 10.
2. **Given** a need for an invisible (zero-size) element, **When** a zero dimension is created, **Then** min, max, and preferred are all 0.

---

### User Story 4 - Combining Dimensions (Priority: P2)

A layout engine needs to calculate the total size requirements when placing multiple elements side by side (summing dimensions) or stacking them (taking maximum dimensions). This allows containers to report their aggregate size constraints to parent containers.

**Why this priority**: Dimension aggregation is essential for hierarchical layout calculations but requires the base Dimension class to be implemented first.

**Independent Test**: Can be fully tested by creating multiple dimensions and verifying that sum and max operations produce correctly aggregated results.

**Acceptance Scenarios**:

1. **Given** dimensions with min=5, max=10 and min=3, max=8, **When** summed, **Then** the result has min=8, max=18.
2. **Given** dimensions with preferred=7 and preferred=5, **When** summed, **Then** the result has preferred=12.
3. **Given** dimensions with min=5 and min=8, **When** taking maximum, **Then** the result has min=8 (highest minimum).
4. **Given** an empty list of dimensions, **When** taking maximum, **Then** the result is a zero dimension.
5. **Given** all dimensions are zero (invisible), **When** taking maximum, **Then** the result is a zero dimension.

---

### User Story 5 - Dynamic Dimensions (Priority: P3)

A UI developer needs dimensions that can change at runtime based on application state. For example, a panel that adjusts its preferred size based on content or user preferences.

**Why this priority**: Dynamic sizing is an advanced capability that builds on static dimensions and is needed less frequently.

**Independent Test**: Can be fully tested by creating a callable that returns a dimension and verifying it is invoked and its result used correctly.

**Acceptance Scenarios**:

1. **Given** a function that returns a Dimension, **When** converted to a dimension, **Then** the function is called and its result is used.
2. **Given** a function that returns an integer, **When** converted to a dimension, **Then** an exact dimension with that integer value is created.
3. **Given** a function that returns null, **When** converted to a dimension, **Then** a default dimension (no constraints) is created.
4. **Given** a nested callable (function returning a function), **When** converted to a dimension, **Then** callables are invoked recursively until a concrete value is obtained.

---

### Edge Cases

- What happens when weight is negative? System throws ArgumentOutOfRangeException.
- What happens when min, max, or preferred is negative? System throws ArgumentOutOfRangeException.
- What happens when max < min? System throws ArgumentException with message "Invalid Dimension: max < min."
- What happens when min = max (degenerate range)? Valid; represents a fixed-size element equivalent to Dimension.Exact(min).
- What happens when preferred = min = max (all equal)? Valid; this is the normal state for exact dimensions.
- What happens when summing dimensions would overflow? Aggregation uses standard integer arithmetic; overflow handling follows .NET platform behavior (wraps without exception in unchecked context).
- What happens when max dimensions have non-overlapping ranges (e.g., 1-5 and 8-9)? The max operation adjusts max upward to equal min (ensures min <= max invariant).
- What happens with a single-element dimension list in max operation? Returns that dimension's constraints (after zero-filtering).
- What happens with all-identical dimensions in max operation? Returns those same constraints.
- What happens when converting an invalid type to a dimension? System throws ArgumentException with message "Not an integer or Dimension object."
- What happens when aggregation methods receive null list? System throws ArgumentNullException.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a Dimension class that encapsulates min, max, preferred, and weight sizing constraints.
- **FR-002**: System MUST default unspecified min to 0, max to 1,000,000,000 (MaxDimensionValue constant), preferred to min (after min default applied), and weight to 1 (DefaultWeight constant).
- **FR-003**: System MUST track which parameters were explicitly specified versus defaulted (MinSpecified, MaxSpecified, PreferredSpecified, WeightSpecified properties).
- **FR-004**: System MUST validate that max >= min and reject invalid configurations.
- **FR-005**: System MUST clamp preferred to the [min, max] range automatically. Clamping occurs after defaults are applied: if preferred < min, set preferred = min; if preferred > max, set preferred = max.
- **FR-006**: System MUST validate that min, max, preferred, and weight are non-negative.
- **FR-007**: System MUST provide a factory method to create exact (fixed-size) dimensions where min = max = preferred.
- **FR-008**: System MUST provide a factory method Zero() to create zero dimensions (min=max=preferred=0, representing invisible elements that occupy no space).
- **FR-009**: System MUST provide a sum operation that aggregates min, max, and preferred values from multiple dimensions. For an empty list, returns Dimension(min: 0, max: 0, preferred: 0). Weight is not summed (not meaningful for aggregation).
- **FR-010**: System MUST provide a max operation using this algorithm: (1) filter out zero dimensions (preferred=0 AND max=0); (2) take highest min from filtered list; (3) take lowest max from filtered list, but ensure max >= highest preferred; (4) if calculated min > max, set max = min (handle non-overlapping ranges); (5) take highest preferred from filtered list.
- **FR-011**: System MUST handle empty dimension lists in max operation by returning a zero dimension.
- **FR-012**: System MUST handle all-zero dimension lists in max operation by returning a zero dimension.
- **FR-013**: System MUST filter out zero-size dimensions (preferred=0 and max=0) when calculating max, so invisible elements don't affect sizing.
- **FR-014**: System MUST provide a conversion function (ToDimension) that accepts: null (returns default Dimension), int (returns Dimension.Exact(value)), Dimension (passthrough), or Func<object?> (invoke and recursively convert result). For unsupported types, throws ArgumentException with message "Not an integer or Dimension object."
- **FR-015**: System MUST recursively invoke callables during conversion until a concrete value is obtained.
- **FR-016**: System MUST provide a validation function to test whether a value could be converted to a dimension.
- **FR-017**: System MUST provide a static class "D" as a convenient alias with factory methods: Create(min?, max?, weight?, preferred?) returning Dimension, Exact(amount) returning Dimension.Exact(amount), and Zero() returning Dimension.Zero().
- **FR-018**: System MUST provide ToString() returning format "Dimension(param=value, ...)" showing only explicitly specified parameters. Examples: new Dimension() → "Dimension()", new Dimension(min: 5) → "Dimension(min=5)", new Dimension(min: 5, max: 10, preferred: 7, weight: 2) → "Dimension(min=5, max=10, preferred=7, weight=2)".

### Key Entities

- **Dimension**: Represents size constraints with min, max, preferred, and weight properties. Tracks which values were explicitly specified. Immutable after construction.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All dimension configurations that match Python Prompt Toolkit's behavior produce identical results in Stroke.
- **SC-002**: Dimension creation with valid parameters completes without errors.
- **SC-003**: Invalid configurations (negative values, max < min) are rejected with clear error messages.
- **SC-004**: Sum and max aggregation operations produce mathematically correct results across all test cases.
- **SC-005**: Dynamic dimension resolution (callable support) correctly handles nested callables and all valid return types.
- **SC-006**: Unit tests achieve at least 80% code coverage.

## Assumptions

- The maximum default value (10^9 = 1,000,000,000) is sufficient for all practical terminal sizes (assumed max: 10,000 columns × 10,000 rows). This is a documented deviation from Python Prompt Toolkit which uses 10^30; the C# value fits within int range while remaining effectively unlimited for terminal layouts.
- Weight of 0 is valid and means the element receives no proportional space allocation beyond its minimum.
- Callable dimensions (Func<object?>) are evaluated synchronously; no async/Task support is required. Callables are invoked immediately during ToDimension conversion.
- Integer overflow behavior during aggregation follows standard .NET semantics (wraps in unchecked context). No special handling required as terminal sizes are bounded well below int.MaxValue.
- IsDimension returns true for callable types without invoking them; it cannot guarantee the callable will produce a valid dimension at runtime.
