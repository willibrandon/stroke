# Data Model: Shortcut Utilities

**Feature**: 046-shortcut-utils
**Date**: 2026-02-01

## Overview

This feature introduces **no new data types**. It consists entirely of static utility methods that compose existing infrastructure types. This document catalogs the existing types consumed by the shortcut utilities.

## Consumed Types (No New Types Created)

### Input Types (parameters accepted by the API)

| Type | Source | Used By | Purpose |
|------|--------|---------|---------|
| `AnyFormattedText` | `Stroke.FormattedText` | `Print` | Union type: string, Html, Ansi, FormattedText, etc. |
| `FormattedText` | `Stroke.FormattedText` | `Print` (internal) | `IReadOnlyList<StyleAndTextTuple>` |
| `StyleAndTextTuple` | `Stroke.FormattedText` | `Print` (internal) | Record: `(string Style, string Text, ...)` |
| `IStyle` | `Stroke.Styles` | `Print`, `PrintContainer` | Style interface for rendering |
| `IStyleTransformation` | `Stroke.Styles` | `Print` | Optional style modification pipeline |
| `ColorDepth` | `Stroke.Output` | `Print` | Enum: Depth1Bit, Depth4Bit, Depth8Bit, Depth24Bit |
| `IOutput` | `Stroke.Output` | `Print` | Terminal output abstraction |
| `TextWriter` | `System.IO` | `Print`, `PrintContainer` | .NET stream for file-based output redirection |
| `AnyContainer` | `Stroke.Layout` | `PrintContainer` | Layout container to render |

### Infrastructure Types (used internally)

| Type | Source | Used By | Purpose |
|------|--------|---------|---------|
| `RendererUtils` | `Stroke.Rendering` | `Print` | Static `PrintFormattedText` method |
| `AppContext` | `Stroke.Application` | `Print`, `Clear`, `SetTitle` | Access current session and app |
| `AppSession` | `Stroke.Application` | `Print`, `Clear`, `SetTitle` | Holds current `Output` |
| `RunInTerminal` | `Stroke.Application` | `Print` | Dispatch rendering through running app |
| `Application<TResult>` | `Stroke.Application` | `Print`, `PrintContainer` | Running app detection, container rendering |
| `StyleMerger` | `Stroke.Styles` | `Print`, `PrintContainer` | Merge default + user styles |
| `DefaultStyles` | `Stroke.Styles` | `Print`, `PrintContainer` | Default UI and Pygments styles |
| `OutputFactory` | `Stroke.Output` | `Print`, `PrintContainer` | Create IOutput from TextWriter |
| `DummyInput` | `Stroke.Input` | `PrintContainer` | Immediate Application termination |
| `Layout` | `Stroke.Layout` | `PrintContainer` | Layout composition for container rendering |
| `FormattedTextUtils` | `Stroke.FormattedText` | `Print` | `ToFormattedText` conversion |

## State Transitions

None — all methods are stateless. The `Print` method conditionally dispatches through `RunInTerminal` when an `Application` is running, but this is a delegation pattern, not a state transition owned by this feature.

## Validation Rules

| Rule | Method | Behavior |
|------|--------|----------|
| `output` and `file` are mutually exclusive | `Print` | Throw `ArgumentException` if both provided |
| `values` may be empty | `Print` | Print only the `end` string |
| `sep` may be empty | `Print` | Concatenate values without separator |
| Plain `IList` (not `FormattedText`) | `Print` | Convert to string representation |
