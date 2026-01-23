# Feature 111: CPR Bindings

## Overview

Implement the CPR (Cursor Position Response) key bindings - handlers for terminal cursor position query responses used for determining the actual cursor position.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/bindings/cpr.py`

## Public API

### LoadCprBindings

```csharp
namespace Stroke.KeyBinding.Bindings;

/// <summary>
/// Key bindings for handling Cursor Position Response.
/// </summary>
public static class CprBindings
{
    /// <summary>
    /// Load key bindings for handling cursor position responses.
    /// </summary>
    /// <returns>Key bindings for CPR handling.</returns>
    /// <remarks>
    /// When the terminal receives a CPR query (ESC[6n), it responds with
    /// the current cursor position in the format ESC[{row};{col}R.
    /// This binding handles that response and reports it to the renderer.
    /// </remarks>
    public static IKeyBindings LoadCprBindings();
}
```

## Project Structure

```
src/Stroke/
└── KeyBinding/
    └── Bindings/
        └── CprBindings.cs
tests/Stroke.Tests/
└── KeyBinding/
    └── Bindings/
        └── CprBindingsTests.cs
```

## Implementation Notes

### CprBindings Implementation

```csharp
namespace Stroke.KeyBinding.Bindings;

public static class CprBindings
{
    public static IKeyBindings LoadCprBindings()
    {
        var kb = new KeyBindings();

        // Handle CPR response - don't save to undo buffer
        kb.Add(Keys.CPRResponse, saveBefore: _ => false, handler: HandleCprResponse);

        return kb;
    }

    private static void HandleCprResponse(KeyPressEvent e)
    {
        // The incoming data looks like "\x1b[35;1R"
        // Parse row/col information.
        var data = e.Data;

        // Skip ESC[ prefix and R suffix
        // Format: ESC [ row ; col R
        if (data.StartsWith("\x1b[") && data.EndsWith("R"))
        {
            var coords = data[2..^1]; // Extract "35;1"
            var parts = coords.Split(';');

            if (parts.Length == 2 &&
                int.TryParse(parts[0], out var row) &&
                int.TryParse(parts[1], out var col))
            {
                // Report absolute cursor position to the renderer
                e.App.Renderer.ReportAbsoluteCursorRow(row);
            }
        }
    }
}
```

### Keys.CPRResponse

```csharp
namespace Stroke;

public static partial class Keys
{
    /// <summary>
    /// Cursor Position Response from terminal.
    /// Generated when terminal responds to CPR query (ESC[6n).
    /// </summary>
    public static readonly Key CPRResponse = new("cpr-response");
}
```

### Usage in Application

```csharp
// CPR bindings are automatically included in default key bindings
var app = new Application(
    layout: myLayout,
    keyBindings: KeyBindings.Merge(
        BasicBindings.LoadBasicBindings(),
        CprBindings.LoadCprBindings()  // Handle cursor position responses
    )
);
```

### Renderer Integration

```csharp
public sealed class Renderer
{
    private readonly Queue<TaskCompletionSource<int>> _cprResponses = new();

    /// <summary>
    /// Report the absolute cursor row from CPR response.
    /// </summary>
    /// <param name="row">The row number (1-based).</param>
    public void ReportAbsoluteCursorRow(int row)
    {
        if (_cprResponses.TryDequeue(out var tcs))
        {
            tcs.TrySetResult(row);
        }
    }

    /// <summary>
    /// Request and wait for cursor position response.
    /// </summary>
    public async Task<int> RequestCursorPositionAsync()
    {
        var tcs = new TaskCompletionSource<int>();
        _cprResponses.Enqueue(tcs);

        // Send CPR query
        _output.Write("\x1b[6n");
        _output.Flush();

        // Wait for response with timeout
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        try
        {
            return await tcs.Task.WaitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            return -1; // Unknown position
        }
    }
}
```

## Dependencies

- Feature 10: Keys
- Feature 19: Key Bindings
- Feature 23: Renderer

## Implementation Tasks

1. Define Keys.CPRResponse constant
2. Implement LoadCprBindings function
3. Parse CPR response format (ESC[row;colR)
4. Integrate with Renderer.ReportAbsoluteCursorRow
5. Write unit tests

## Acceptance Criteria

- [ ] CPR responses are correctly parsed
- [ ] Row and column are extracted from response
- [ ] Renderer receives absolute cursor position
- [ ] Invalid responses are ignored gracefully
- [ ] saveBefore returns false (no undo entry)
- [ ] Unit tests achieve 80% coverage
