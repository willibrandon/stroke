# Contracts: Project Setup and Primitives

**Date**: 2026-01-23
**Feature**: 001-project-setup-primitives

## Overview

This feature does not define API contracts (REST/GraphQL schemas) because Point and Size are internal primitive types, not external service interfaces.

The types are used as building blocks within the Stroke library and are consumed directly via NuGet package reference.

## Public API Surface

The public API is defined entirely in `data-model.md`:

- `Stroke.Core.Primitives.Point` - 2D screen coordinate
- `Stroke.Core.Primitives.Size` - Terminal dimensions

Both are exported as part of the Stroke NuGet package and consumed via standard .NET project references.
