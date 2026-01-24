# Research: Project Setup and Primitives

**Date**: 2026-01-23
**Feature**: 001-project-setup-primitives

## Overview

This document consolidates research findings for the project setup and primitives feature. Since the technical context is well-defined by the Constitution and feature specification, this phase focuses on confirming best practices.

## Decision 1: Record Struct vs Class for Primitives

**Decision**: Use `readonly record struct` for both Point and Size

**Rationale**:
- Python's `NamedTuple` provides value semantics - C# record structs are the closest equivalent
- Zero heap allocation for stack-allocated value types
- Built-in equality, hashing, and ToString implementations
- Immutability enforced by `readonly` modifier
- Matches Constitution Principle II (Immutability by Default)

**Alternatives Considered**:
- `class` - Rejected: heap allocation, reference semantics differ from Python NamedTuple
- `struct` (non-record) - Rejected: requires manual Equals/GetHashCode implementation
- `record class` - Rejected: heap allocation unnecessary for small value types

## Decision 2: Namespace Organization

**Decision**: `Stroke.Core.Primitives` namespace for Point and Size

**Rationale**:
- `docs/api-mapping.md` maps `prompt_toolkit.data_structures` → `Stroke.Core`
- Sub-namespace `Primitives` provides organizational clarity for primitive types
- Matches Constitution Principle III (Layered Architecture) - Core is the bottom layer
- File path mirrors namespace: `src/Stroke/Core/Primitives/`

**Alternatives Considered**:
- `Stroke.DataStructures` - Rejected: doesn't follow api-mapping.md directive
- `Stroke.Core` (no sub-namespace) - Rejected: loses organizational clarity as Core grows

## Decision 3: Central Package Management

**Decision**: Use Directory.Build.props + Directory.Packages.props

**Rationale**:
- Modern .NET best practice for multi-project solutions
- Single source of truth for framework version, language version, compiler settings
- Simplifies dependency version management as projects grow
- Required by FR-004 in feature specification

**Alternatives Considered**:
- Per-project configuration only - Rejected: leads to version drift, harder maintenance
- global.json only - Rejected: doesn't handle package versions

## Decision 4: Test Framework Configuration

**Decision**: xUnit with standard assertions only

**Rationale**:
- Constitution Principle VIII explicitly forbids Moq and FluentAssertions
- xUnit is the de facto standard for .NET testing
- Built-in Assert class provides sufficient assertion methods
- No additional test infrastructure needed for primitive types

**Alternatives Considered**:
- NUnit - Rejected: xUnit is preferred in Constitution (mentioned explicitly)
- MSTest - Rejected: less common in modern .NET ecosystem

## Decision 5: API Extensions Beyond Python

**Decision**: Add `Offset`, `+`, `-` operators to Point; add `Height`, `Width`, `IsEmpty` to Size

**Rationale**:
- The feature specification explicitly requires these additions
- They follow C# conventions for value types (operator overloading)
- `Height`/`Width` aliases improve discoverability (common C# naming)
- `IsEmpty` is a common predicate for dimension types
- These are enhancements permitted as they don't change Python API semantics

**Alternatives Considered**:
- Strict 1:1 port only - Rejected: specification explicitly includes these APIs
- Extension methods - Rejected: operators must be on the type itself

## Summary

All technical decisions are straightforward based on:
1. Constitution principles (immutability, layered architecture, testing constraints)
2. Feature specification requirements
3. API mapping document directives
4. Standard .NET best practices

No blockers or open questions remain. Ready for Phase 1 design artifacts.
