# Feature Specification: Project Setup and Primitives

**Feature Branch**: `001-project-setup-primitives`
**Created**: 2026-01-23
**Status**: Draft
**Input**: User description: "Initialize the Stroke .NET 10 solution structure and implement core primitive types that match Python Prompt Toolkit's data_structures.py"

## Python Reference *(mandatory for port features)*

**Source File**: `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/data_structures.py`
**Reference Document**: `docs/api-mapping.md` Section: `prompt_toolkit.data_structures`

### Python Public API (`__all__`)

The Python module exports exactly two types:

```python
__all__ = [
    "Point",
    "Size",
]
```

### Python Type Definitions

```python
class Point(NamedTuple):
    x: int
    y: int

class Size(NamedTuple):
    rows: int
    columns: int
```

### Field Mapping (Python → C#)

| Python Type | Python Field | C# Type | C# Property | Notes |
|-------------|--------------|---------|-------------|-------|
| `Point` | `x: int` | `Point` | `X: int` | snake_case → PascalCase |
| `Point` | `y: int` | `Point` | `Y: int` | snake_case → PascalCase |
| `Size` | `rows: int` | `Size` | `Rows: int` | snake_case → PascalCase |
| `Size` | `columns: int` | `Size` | `Columns: int` | snake_case → PascalCase |

### C# Idiom Additions (Not in Python)

The following APIs are C# idiomatic additions that do not exist in Python Prompt Toolkit. They are permitted because they extend (not modify) the Python API and follow C# conventions:

| Type | Addition | Rationale |
|------|----------|-----------|
| `Point` | `static Point Zero` | Common C# pattern for value type origins; equivalent to `Point(0, 0)` |
| `Point` | `Point Offset(int dx, int dy)` | Fluent API for coordinate manipulation; common in .NET graphics APIs |
| `Point` | `operator +(Point, Point)` | C# operator overloading convention for additive types |
| `Point` | `operator -(Point, Point)` | C# operator overloading convention for additive types |
| `Size` | `static Size Zero` | Common C# pattern for value type defaults; equivalent to `Size(0, 0)` |
| `Size` | `int Height` (alias) | C# convention alias for discoverability; returns `Rows` |
| `Size` | `int Width` (alias) | C# convention alias for discoverability; returns `Columns` |
| `Size` | `bool IsEmpty` | Common C# pattern for dimension types; true when invalid dimensions |

**Justification**: These additions do not violate Constitution Principle I (Faithful Port) because:
1. All Python APIs are preserved with identical semantics
2. Additions are purely additive and do not change existing behavior
3. Additions follow established C# conventions for value types
4. The feature specification explicitly requires them (user input)

### Semantic Equivalence (Python NamedTuple → C# Record Struct)

Python `NamedTuple` provides these semantics that MUST be preserved in C#:

| Python NamedTuple Behavior | C# `readonly record struct` Equivalent | Required |
|----------------------------|----------------------------------------|----------|
| Value equality (`==`, `!=`) | Auto-generated `Equals`, `==`, `!=` | YES |
| Immutability | `readonly` modifier prevents field mutation | YES |
| Hashability (`hash()`) | Auto-generated `GetHashCode()` | YES |
| String representation (`__repr__`) | Auto-generated `ToString()` | YES |
| Positional construction | Primary constructor `Point(int X, int Y)` | YES |
| Named field access | Properties `point.X`, `point.Y` | YES |
| Tuple unpacking | Deconstruct method (auto-generated) | YES |
| Copying with modification | `with` expression support (auto-generated) | YES |

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Developer Creates New Stroke Project (Priority: P1)

A developer wants to create a new terminal UI application using Stroke. They need to add the Stroke NuGet package to their project and start using the primitive types for screen coordinate operations.

**Why this priority**: Without a working solution structure and NuGet package, no other features can be built or consumed. This is the foundation of the entire project.

**Independent Test**: Can be fully tested by creating a new .NET project, referencing Stroke, and instantiating Point and Size structs. Delivers immediate value by enabling project development.

**Acceptance Scenarios**:

1. **Given** an empty .NET 10 project, **When** a developer adds a reference to the Stroke package, **Then** they can use `Stroke.Core.Primitives.Point` and `Stroke.Core.Primitives.Size` types without errors.
2. **Given** the Stroke solution, **When** a developer runs `dotnet build`, **Then** the solution compiles without errors or warnings.
3. **Given** the Stroke solution, **When** a developer runs `dotnet test`, **Then** all unit tests pass.

---

### User Story 2 - Developer Uses Point for Screen Coordinates (Priority: P1)

A developer needs to represent and manipulate 2D screen coordinates when building terminal UI layouts or tracking cursor positions.

**Why this priority**: Point is a fundamental building block used throughout the rendering, layout, and input systems. All higher-level features depend on this primitive.

**Independent Test**: Can be tested by creating Point instances, performing arithmetic operations, and verifying coordinate values match expectations.

**Acceptance Scenarios**:

1. **Given** a Point with coordinates (5, 10), **When** the developer accesses X and Y properties, **Then** they receive values 5 and 10 respectively.
2. **Given** two Points (3, 4) and (1, 2), **When** the developer adds them using the + operator, **Then** the result is Point (4, 6).
3. **Given** two Points (5, 7) and (2, 3), **When** the developer subtracts them using the - operator, **Then** the result is Point (3, 4).
4. **Given** a Point (10, 20), **When** the developer calls Offset(5, -3), **Then** the result is Point (15, 17).
5. **Given** the Point type, **When** the developer accesses Point.Zero, **Then** they receive Point (0, 0).
6. **Given** two Points with same coordinates, **When** compared with `==`, **Then** they are equal (value semantics).
7. **Given** a Point (3, 4), **When** deconstructed via `var (x, y) = point`, **Then** x=3 and y=4.
8. **Given** a Point (3, 4), **When** copied with `point with { X = 10 }`, **Then** result is Point (10, 4).

---

### User Story 3 - Developer Uses Size for Terminal Dimensions (Priority: P1)

A developer needs to represent terminal window dimensions (rows and columns) when calculating layouts or handling resize events.

**Why this priority**: Size is a fundamental building block used throughout the rendering and layout systems. Terminal dimensions are essential for any TUI application.

**Independent Test**: Can be tested by creating Size instances and verifying dimension properties match expectations.

**Acceptance Scenarios**:

1. **Given** a Size with dimensions (24, 80), **When** the developer accesses Rows and Columns properties, **Then** they receive values 24 and 80 respectively.
2. **Given** a Size with dimensions (24, 80), **When** the developer accesses Height and Width alias properties, **Then** they receive values 24 and 80 respectively.
3. **Given** a Size with dimensions (0, 80), **When** the developer checks IsEmpty, **Then** the result is true.
4. **Given** a Size with dimensions (24, 0), **When** the developer checks IsEmpty, **Then** the result is true.
5. **Given** a Size with dimensions (24, 80), **When** the developer checks IsEmpty, **Then** the result is false.
6. **Given** the Size type, **When** the developer accesses Size.Zero, **Then** they receive Size (0, 0).
7. **Given** two Sizes with same dimensions, **When** compared with `==`, **Then** they are equal (value semantics).
8. **Given** a Size (24, 80), **When** deconstructed via `var (rows, cols) = size`, **Then** rows=24 and cols=80.

---

### Edge Cases

- **Negative Point coordinates**: Valid. Screen coordinates can be negative for off-screen positions. No special handling required.
- **Negative Size dimensions**: Valid input, but `IsEmpty` MUST return `true` for any dimension ≤ 0.
- **Integer overflow**: Standard .NET unchecked integer overflow behavior applies. No explicit overflow checking required.
- **Zero Point**: `Point.Zero` returns `Point(0, 0)`. A point at origin is valid and common.
- **Zero Size**: `Size.Zero` returns `Size(0, 0)`. `Size.Zero.IsEmpty` MUST return `true`.

## Requirements *(mandatory)*

### Naming Conventions

All Python-to-C# name translations MUST follow these rules:

| Python Convention | C# Convention | Example |
|-------------------|---------------|---------|
| `snake_case` field | `PascalCase` property | `x` → `X`, `rows` → `Rows` |
| `snake_case` method | `PascalCase` method | N/A for this module |
| `UPPER_CASE` constant | `PascalCase` property | N/A for this module |
| Class name (already PascalCase) | Class name (unchanged) | `Point` → `Point` |

### Functional Requirements

#### Solution Setup (FR-001 to FR-004)

- **FR-001**: Solution MUST target .NET 10 with C# 13 language version
- **FR-002**: Solution MUST enable nullable reference types (`<Nullable>enable</Nullable>`)
- **FR-003**: Solution MUST treat warnings as errors (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`)
- **FR-004**: Solution MUST use central package management via `Directory.Packages.props`

#### Point Type (FR-005 to FR-008)

- **FR-005**: Point MUST be a `readonly record struct` with positional parameters `(int X, int Y)`
- **FR-006**: Point MUST provide a static `Zero` property returning `new Point(0, 0)`
- **FR-007**: Point MUST provide an `Offset(int dx, int dy)` method returning `new Point(X + dx, Y + dy)`
- **FR-008**: Point MUST provide `operator +` and `operator -` for component-wise Point arithmetic

#### Size Type (FR-009 to FR-012)

- **FR-009**: Size MUST be a `readonly record struct` with positional parameters `(int Rows, int Columns)`
- **FR-010**: Size MUST provide `Height` property (returns `Rows`) and `Width` property (returns `Columns`)
- **FR-011**: Size MUST provide a static `Zero` property returning `new Size(0, 0)`
- **FR-012**: Size MUST provide an `IsEmpty` property returning `true` when `Rows <= 0 || Columns <= 0`

#### Documentation (FR-013 to FR-015)

- **FR-013**: All public types (Point, Size) MUST have XML documentation comments with `<summary>`
- **FR-014**: All public members (properties, methods, operators, constructors) MUST have XML documentation with `<summary>`, `<param>` (where applicable), and `<returns>` (where applicable)
- **FR-015**: XML documentation MUST reference Python Prompt Toolkit equivalent in remarks (e.g., "Equivalent to Python `Point` NamedTuple")

#### Testing (FR-016)

- **FR-016**: Test project MUST use xUnit with standard assertions only. Moq, FluentAssertions, and all mock/fake libraries are forbidden per Constitution Principle VIII.

### Key Entities

- **Point**: Immutable value type representing a position in 2D screen space. Properties: `X` (column), `Y` (row). Semantics: value equality, hashable, deconstructible. Python equivalent: `Point(NamedTuple)` with fields `x: int`, `y: int`.

- **Size**: Immutable value type representing dimensions. Properties: `Rows` (height), `Columns` (width), `Height` (alias), `Width` (alias), `IsEmpty`. Semantics: value equality, hashable, deconstructible. Python equivalent: `Size(NamedTuple)` with fields `rows: int`, `columns: int`.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Solution builds successfully with `dotnet build` producing zero errors and zero warnings
- **SC-002**: All unit tests pass with `dotnet test` (100% pass rate, exit code 0)
- **SC-003**: Point and Size API surface matches Python Prompt Toolkit's `data_structures.py` exactly, verified by:
  - All Python `__all__` exports have C# equivalents (Point, Size)
  - All Python fields map to C# properties per Field Mapping table
  - All C# additions are documented in "C# Idiom Additions" table
- **SC-004**: All public types and members have XML documentation comments (verified by `<GenerateDocumentationFile>true</GenerateDocumentationFile>` with zero warnings)
- **SC-005**: Test coverage for Point and Size types reaches minimum 80% **line coverage** (measured by coverlet or equivalent)

### API Fidelity Verification Checklist

To verify SC-003, confirm:

- [x] `Point` type exists in `Stroke.Core.Primitives` namespace
- [x] `Point.X` property exists and returns `int`
- [x] `Point.Y` property exists and returns `int`
- [x] `Size` type exists in `Stroke.Core.Primitives` namespace
- [x] `Size.Rows` property exists and returns `int`
- [x] `Size.Columns` property exists and returns `int`
- [x] No Python APIs from `__all__` are missing
- [x] All C# additions are in the documented additions table

## Assumptions

- .NET 10 SDK is available on the development machine
- The solution will be hosted on GitHub and published to NuGet.org
- Standard .NET project conventions apply (src/ for source, tests/ for tests)
- `readonly record struct` provides all required Python NamedTuple semantics (equality, hashing, immutability, deconstruction, with-expressions)
- Integer overflow follows standard .NET unchecked arithmetic (no explicit bounds checking)
