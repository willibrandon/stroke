# Quickstart: Synchronized Output (DEC Mode 2026)

**Feature**: 067-synchronized-output

## Overview

This feature adds DEC Mode 2026 (Synchronized Output) to Stroke's rendering pipeline to eliminate terminal resize flicker and enable atomic render updates.

## Build

```bash
dotnet build src/Stroke/Stroke.csproj
```

## Test

```bash
dotnet test tests/Stroke.Tests/Stroke.Tests.csproj --filter "FullyQualifiedName~SynchronizedOutput"
```

## Verify

### Automated Verification
```bash
# Run the full test suite to verify no regressions
dotnet test tests/Stroke.Tests/Stroke.Tests.csproj
```

### Visual Verification (TUI Driver)
1. Launch any Stroke prompt example
2. Resize the terminal window by dragging the corner
3. Observe: content re-renders without visible blank frame or flicker
4. Compare with pre-feature behavior (checkout main branch) to see the improvement

## Files Changed

| File | Change Type | Lines |
|------|------------|-------|
| `src/Stroke/Output/IOutput.cs` | Modified | +2 methods |
| `src/Stroke/Output/Vt100Output.cs` | Modified | +flag, modified Flush() |
| `src/Stroke/Output/Windows/Win32Output.cs` | Modified | +2 no-op methods |
| `src/Stroke/Output/Windows/Windows10Output.cs` | Modified | +2 delegation methods |
| `src/Stroke/Output/Windows/ConEmuOutput.cs` | Modified | +2 delegation methods |
| `src/Stroke/Output/PlainTextOutput.cs` | Modified | +2 no-op methods |
| `src/Stroke/Output/DummyOutput.cs` | Modified | +2 no-op methods |
| `src/Stroke/Rendering/Renderer.cs` | Modified | +ResetForResize(), wrap Render/Erase/Clear |
| `src/Stroke/Rendering/Renderer.Diff.cs` | Modified | Absolute cursor home on full redraw |
| `src/Stroke/Application/Application.RunAsync.cs` | Modified | OnResize() uses ResetForResize() |
| `tests/Stroke.Tests/Output/Vt100OutputSynchronizedOutputTests.cs` | New | Sync output tests |
| `tests/Stroke.Tests/Rendering/RendererSynchronizedOutputTests.cs` | New | Renderer integration tests |

## Escape Sequences

| Sequence | Name | Effect |
|----------|------|--------|
| `\x1b[?2026h` | Mode 2026 Set | Terminal begins buffering output |
| `\x1b[?2026l` | Mode 2026 Reset | Terminal commits buffered output atomically |
| `\x1b[H` | Cursor Home | Move cursor to row 1, column 1 (absolute) |
