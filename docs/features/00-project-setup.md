# Feature 00: Project Setup and Primitives

## Overview

Initialize the Stroke .NET 10 solution structure and implement core primitive types that match Python Prompt Toolkit's `data_structures.py`.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/data_structures.py`

## Public API

### Point (NamedTuple)

```csharp
namespace Stroke.Core.Primitives;

/// <summary>
/// Represents a point in 2D screen coordinates.
/// </summary>
public readonly record struct Point(int X, int Y)
{
    public static Point Zero { get; } = new(0, 0);

    public Point Offset(int dx, int dy) => new(X + dx, Y + dy);

    public static Point operator +(Point a, Point b) => new(a.X + b.X, a.Y + b.Y);
    public static Point operator -(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);
}
```

### Size (NamedTuple)

```csharp
namespace Stroke.Core.Primitives;

/// <summary>
/// Represents a size with rows (height) and columns (width).
/// Matches Python's Size(rows, columns) NamedTuple.
/// </summary>
public readonly record struct Size(int Rows, int Columns)
{
    public static Size Zero { get; } = new(0, 0);

    public int Height => Rows;
    public int Width => Columns;

    public bool IsEmpty => Rows <= 0 || Columns <= 0;
}
```

## Project Structure

```
Stroke/
├── src/
│   └── Stroke/
│       ├── Stroke.csproj
│       └── Core/
│           └── Primitives/
│               ├── Point.cs
│               └── Size.cs
├── tests/
│   └── Stroke.Tests/
│       ├── Stroke.Tests.csproj
│       └── Core/
│           └── Primitives/
│               ├── PointTests.cs
│               └── SizeTests.cs
├── Stroke.sln
├── Directory.Build.props
└── Directory.Packages.props
```

## Implementation Tasks

1. Create solution file `Stroke.sln`
2. Create `src/Stroke/Stroke.csproj` targeting .NET 10
3. Create `tests/Stroke.Tests/Stroke.Tests.csproj` with xUnit
4. Create `Directory.Build.props` with common settings
5. Create `Directory.Packages.props` for central package management
6. Implement `Point` record struct in `Core/Primitives/Point.cs`
7. Implement `Size` record struct in `Core/Primitives/Size.cs`
8. Write unit tests for Point and Size

## Project Configuration

### Directory.Build.props

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>13</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>
```

### Stroke.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <PackageId>Stroke</PackageId>
    <Version>0.1.0</Version>
    <Description>Cross-platform terminal UI framework - .NET port of Python Prompt Toolkit</Description>
    <Authors>Stroke Contributors</Authors>
    <License>MIT</License>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>
</Project>
```

## Dependencies

- No external dependencies for Core layer
- xUnit for tests
- No mocks (per constitution)

## Acceptance Criteria

- [ ] Solution builds with `dotnet build`
- [ ] Tests pass with `dotnet test`
- [ ] Point and Size match Python Prompt Toolkit semantics exactly
- [ ] XML documentation on all public types
