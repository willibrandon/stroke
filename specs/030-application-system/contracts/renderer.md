# Contract: Renderer

**Namespace**: `Stroke.Rendering`
**Source**: `prompt_toolkit.renderer`

## Renderer Class

```csharp
/// <summary>
/// Renders the application layout to the terminal output. Uses differential
/// updates to only repaint changed regions for performance.
/// </summary>
/// <remarks>
/// The Renderer is NOT thread-safe for rendering operations. All Render/Erase/Reset
/// calls must occur on the application's async context. CPR response tracking is
/// thread-safe.
/// </remarks>
public sealed class Renderer
{
    /// <summary>
    /// Create a new Renderer.
    /// </summary>
    /// <param name="style">The merged style for rendering.</param>
    /// <param name="output">The output device.</param>
    /// <param name="fullScreen">Whether to use alternate screen buffer.</param>
    /// <param name="mouseSupport">Filter for mouse support.</param>
    /// <param name="cprNotSupportedCallback">Called when CPR response times out.</param>
    public Renderer(
        IStyle style,
        IOutput output,
        bool fullScreen = false,
        IFilter? mouseSupport = null,
        Action? cprNotSupportedCallback = null);

    /// <summary>The last rendered screen, or null before first render.</summary>
    public Screen? LastRenderedScreen { get; }

    /// <summary>Whether the terminal height is known (from CPR response).</summary>
    public bool HeightIsKnown { get; }

    /// <summary>Number of rows above the current layout in non-fullscreen mode.</summary>
    public int RowsAboveLayout { get; }

    /// <summary>Whether we are currently waiting for a CPR response.</summary>
    public bool WaitingForCpr { get; }

    /// <summary>
    /// The style-string-to-Attrs cache. Used by Application.GetUsedStyleStrings().
    /// </summary>
    internal Dictionary<string, Attrs>? AttrsForStyle { get; }

    // --- Methods ---

    /// <summary>
    /// Render the application layout to the output. Computes differential
    /// updates by comparing with the previous screen.
    /// <para><b>Side effects:</b></para>
    /// <list type="bullet">
    /// <item>Updates <see cref="LastRenderedScreen"/> with the current screen.</item>
    /// <item>Updates internal <c>AttrsForStyle</c> cache with style string → Attrs mappings.</item>
    /// <item>Updates <see cref="RowsAboveLayout"/> based on cursor position.</item>
    /// <item>Updates internal cursor position tracking.</item>
    /// <item>Updates internal <c>_lastRenderedSize</c> and <c>_lastStyle</c>.</item>
    /// <item>Writes escape sequences to the output device.</item>
    /// <item>If mouse support filter evaluates to true, enables mouse tracking via output.</item>
    /// </list>
    /// Note: <see cref="Application{TResult}.RenderCounter"/> is incremented by the Application
    /// before calling this method, not by the Renderer itself.
    /// </summary>
    /// <param name="app">The application being rendered.</param>
    /// <param name="layout">The layout to render.</param>
    /// <param name="isDone">Render in 'done' state (cursor at end, no more updates).</param>
    public void Render(Application<object?> app, Layout layout, bool isDone = false);

    /// <summary>
    /// Erase the renderer output from the terminal.
    /// </summary>
    /// <param name="leaveAlternateScreen">Whether to leave alternate screen if active.</param>
    public void Erase(bool leaveAlternateScreen = true);

    /// <summary>
    /// Clear the terminal screen completely.
    /// </summary>
    public void Clear();

    /// <summary>
    /// Reset the renderer state. Clears cached screen, style cache, and cursor position.
    /// </summary>
    public void Reset();

    /// <summary>
    /// Request an absolute cursor position report (CPR) from the terminal.
    /// Sends the DSR (Device Status Report) escape sequence to the output.
    /// The terminal responds with a CPR containing the cursor row and column.
    /// Sets <see cref="WaitingForCpr"/> to true until the response arrives.
    /// If the terminal does not respond within a timeout, the
    /// <c>cprNotSupportedCallback</c> (from constructor) is invoked and
    /// <see cref="HeightIsKnown"/> remains false.
    /// This method is called at the start of each render cycle in non-fullscreen mode
    /// to determine the terminal height and the number of rows above the layout.
    /// Thread-safe: CPR response tracking is synchronized via Lock.
    /// </summary>
    public void RequestAbsoluteCursorPosition();

    /// <summary>
    /// Wait for all pending CPR responses to arrive. Called during application
    /// shutdown (in the finally block of RunAsync) to ensure no pending responses
    /// are lost. Only waits if <see cref="IOutput.RespondsToCpr"/> is true.
    /// Times out after a reasonable period (implementation-defined) to avoid
    /// hanging on terminals that don't support CPR.
    /// </summary>
    /// <returns>A task that completes when all CPR responses are received or timeout.</returns>
    public Task WaitForCprResponsesAsync();
}
```

## Standalone Functions

```csharp
/// <summary>
/// Utility methods for formatted text output.
/// </summary>
public static class RendererUtils
{
    /// <summary>
    /// Print formatted text directly to an output device.
    /// </summary>
    /// <param name="output">The output device.</param>
    /// <param name="formattedText">The formatted text to print.</param>
    /// <param name="style">The style to use for rendering.</param>
    /// <param name="colorDepth">The color depth for rendering.</param>
    /// <param name="styleTransformation">Optional style transformation.</param>
    public static void PrintFormattedText(
        IOutput output,
        AnyFormattedText formattedText,
        IStyle? style = null,
        ColorDepth? colorDepth = null,
        IStyleTransformation? styleTransformation = null);
}
```

## Internal: Screen Diff Algorithm

```csharp
/// <summary>
/// Internal: Compute and output the difference between two screens.
/// This is performance-critical code.
/// </summary>
internal static class ScreenDiff
{
    /// <summary>
    /// Render the diff between the previous and current screen.
    /// </summary>
    /// <param name="app">The application context.</param>
    /// <param name="output">The output device.</param>
    /// <param name="screen">The current screen.</param>
    /// <param name="currentPos">Current cursor position.</param>
    /// <param name="colorDepth">Active color depth.</param>
    /// <param name="previousScreen">Previous screen for diffing (null for first render).</param>
    /// <param name="lastStyle">Last drawn style string.</param>
    /// <param name="isDone">Whether rendering in 'done' state.</param>
    /// <param name="fullScreen">Whether in full-screen mode.</param>
    /// <param name="attrsForStyleString">Style-to-attrs cache.</param>
    /// <param name="size">Current terminal size.</param>
    /// <param name="previousWidth">Previous terminal width.</param>
    /// <returns>Tuple of:
    /// <list type="bullet">
    /// <item><c>CursorPos</c>: The cursor position after all diff output is written (column, row).</item>
    /// <item><c>LastStyle</c>: The last style string that was emitted to the output, or null if no styles were applied. Used to avoid redundant style escape sequences on the next call.</item>
    /// </list></returns>
    internal static (Point CursorPos, string? LastStyle) OutputScreenDiff(
        Application<object?> app,
        IOutput output,
        Screen screen,
        Point currentPos,
        ColorDepth colorDepth,
        Screen? previousScreen,
        string? lastStyle,
        bool isDone,
        bool fullScreen,
        Dictionary<string, Attrs> attrsForStyleString,
        Size size,
        int previousWidth);
}
```
