# Quickstart: Scroll Bindings

**Feature**: 035-scroll-bindings
**Date**: 2026-01-30

## Build & Test

```bash
# Build the project
dotnet build src/Stroke/Stroke.csproj

# Run all tests
dotnet test tests/Stroke.Tests/Stroke.Tests.csproj

# Run only scroll-related tests
dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~ScrollBindings"
dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~PageNavigationBindings"
```

## New Files

### Source Files

| File | Purpose |
|------|---------|
| `src/Stroke/KeyBinding/Bindings/ScrollBindings.cs` | 8 static scroll functions |
| `src/Stroke/KeyBinding/Bindings/PageNavigationBindings.cs` | 3 binding loader methods |

### Test Files

| File | Purpose |
|------|---------|
| `tests/Stroke.Tests/KeyBinding/Bindings/ScrollBindingsTests.cs` | Scroll function unit tests |
| `tests/Stroke.Tests/KeyBinding/Bindings/PageNavigationBindingsTests.cs` | Binding loader tests |

## Key Dependencies

- `Window.VerticalScroll` (mutable int) — from Feature 029
- `WindowRenderInfo` (render context) — from Feature 029
- `Buffer.CursorPosition` (mutable int) — from Feature 007
- `Document` navigation methods — from Feature 002
- `KeyBindings`, `ConditionalKeyBindings`, `MergedKeyBindings` — from Feature 022
- `AppFilters.BufferHasFocus`, `ViFilters.ViMode`, `EmacsFilters.EmacsMode` — from Feature 032

## Python Reference

- `python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/scroll.py` (191 lines)
- `python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/page_navigation.py` (86 lines)
