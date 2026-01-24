# Quickstart: Project Setup and Primitives

**Date**: 2026-01-23
**Feature**: 001-project-setup-primitives

## Overview

This guide covers the implementation sequence for setting up the Stroke solution and implementing the Point and Size primitives.

## Prerequisites

- .NET 10 SDK installed
- Git configured
- IDE with C# 13 support (VS 2025, Rider, VS Code with C# extension)

## Implementation Sequence

### Step 1: Create Solution Structure

```bash
# From repository root
mkdir -p src/Stroke/Core/Primitives
mkdir -p tests/Stroke.Tests/Core/Primitives
```

### Step 2: Create Directory.Build.props

Create `Directory.Build.props` at repository root:

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

### Step 3: Create Directory.Packages.props

Create `Directory.Packages.props` at repository root:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <!-- Test packages -->
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageVersion Include="xunit" Version="2.9.2" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="3.0.0" />
    <PackageVersion Include="coverlet.collector" Version="6.0.2" />
  </ItemGroup>
</Project>
```

### Step 4: Create Stroke.csproj

Create `src/Stroke/Stroke.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <PackageId>Stroke</PackageId>
    <Version>0.1.0</Version>
    <Description>Cross-platform terminal UI framework - .NET port of Python Prompt Toolkit</Description>
    <Authors>Stroke Contributors</Authors>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>
</Project>
```

### Step 5: Create Stroke.Tests.csproj

Create `tests/Stroke.Tests/Stroke.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="coverlet.collector" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../../src/Stroke/Stroke.csproj" />
  </ItemGroup>
</Project>
```

### Step 6: Create Solution File

```bash
dotnet new sln -n Stroke
dotnet sln add src/Stroke/Stroke.csproj
dotnet sln add tests/Stroke.Tests/Stroke.Tests.csproj
```

### Step 7: Implement Point

Create `src/Stroke/Core/Primitives/Point.cs` with the implementation from data-model.md.

### Step 8: Implement Size

Create `src/Stroke/Core/Primitives/Size.cs` with the implementation from data-model.md.

### Step 9: Write Point Tests

Create `tests/Stroke.Tests/Core/Primitives/PointTests.cs`:

```csharp
namespace Stroke.Tests.Core.Primitives;

public class PointTests
{
    [Fact]
    public void Constructor_SetsCoordinates()
    {
        var point = new Point(5, 10);
        Assert.Equal(5, point.X);
        Assert.Equal(10, point.Y);
    }

    [Fact]
    public void Zero_ReturnsOrigin()
    {
        Assert.Equal(0, Point.Zero.X);
        Assert.Equal(0, Point.Zero.Y);
    }

    [Fact]
    public void Offset_ReturnsNewPoint()
    {
        var point = new Point(10, 20);
        var result = point.Offset(5, -3);
        Assert.Equal(15, result.X);
        Assert.Equal(17, result.Y);
    }

    [Fact]
    public void AdditionOperator_AddsComponents()
    {
        var a = new Point(3, 4);
        var b = new Point(1, 2);
        var result = a + b;
        Assert.Equal(4, result.X);
        Assert.Equal(6, result.Y);
    }

    [Fact]
    public void SubtractionOperator_SubtractsComponents()
    {
        var a = new Point(5, 7);
        var b = new Point(2, 3);
        var result = a - b;
        Assert.Equal(3, result.X);
        Assert.Equal(4, result.Y);
    }

    [Fact]
    public void Equality_ValueSemantics()
    {
        var a = new Point(5, 10);
        var b = new Point(5, 10);
        Assert.Equal(a, b);
    }

    [Fact]
    public void NegativeCoordinates_Allowed()
    {
        var point = new Point(-5, -10);
        Assert.Equal(-5, point.X);
        Assert.Equal(-10, point.Y);
    }
}
```

### Step 10: Write Size Tests

Create `tests/Stroke.Tests/Core/Primitives/SizeTests.cs`:

```csharp
namespace Stroke.Tests.Core.Primitives;

public class SizeTests
{
    [Fact]
    public void Constructor_SetsDimensions()
    {
        var size = new Size(24, 80);
        Assert.Equal(24, size.Rows);
        Assert.Equal(80, size.Columns);
    }

    [Fact]
    public void Zero_ReturnsZeroSize()
    {
        Assert.Equal(0, Size.Zero.Rows);
        Assert.Equal(0, Size.Zero.Columns);
    }

    [Fact]
    public void HeightWidth_AliasRowsColumns()
    {
        var size = new Size(24, 80);
        Assert.Equal(24, size.Height);
        Assert.Equal(80, size.Width);
    }

    [Fact]
    public void IsEmpty_ZeroRows_ReturnsTrue()
    {
        var size = new Size(0, 80);
        Assert.True(size.IsEmpty);
    }

    [Fact]
    public void IsEmpty_ZeroColumns_ReturnsTrue()
    {
        var size = new Size(24, 0);
        Assert.True(size.IsEmpty);
    }

    [Fact]
    public void IsEmpty_PositiveDimensions_ReturnsFalse()
    {
        var size = new Size(24, 80);
        Assert.False(size.IsEmpty);
    }

    [Fact]
    public void IsEmpty_NegativeRows_ReturnsTrue()
    {
        var size = new Size(-1, 80);
        Assert.True(size.IsEmpty);
    }

    [Fact]
    public void IsEmpty_NegativeColumns_ReturnsTrue()
    {
        var size = new Size(24, -1);
        Assert.True(size.IsEmpty);
    }

    [Fact]
    public void Equality_ValueSemantics()
    {
        var a = new Size(24, 80);
        var b = new Size(24, 80);
        Assert.Equal(a, b);
    }
}
```

### Step 11: Build and Test

```bash
dotnet build
dotnet test
```

## Verification Checklist

- [ ] `dotnet build` succeeds with zero warnings
- [ ] `dotnet test` passes all tests
- [ ] Point has X, Y, Zero, Offset, +, - as specified
- [ ] Size has Rows, Columns, Height, Width, Zero, IsEmpty as specified
- [ ] All public types have XML documentation
- [ ] No external dependencies in Stroke.csproj (except implicit framework)
