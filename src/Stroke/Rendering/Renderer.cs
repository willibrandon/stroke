using Stroke.Core.Primitives;
using Stroke.CursorShapes;
using Stroke.Filters;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Output;
using Stroke.Styles;

namespace Stroke.Rendering;

/// <summary>
/// Renders the application layout to the terminal output. Uses differential
/// updates to only repaint changed regions for performance.
/// </summary>
/// <remarks>
/// <para>
/// The Renderer is NOT thread-safe for rendering operations. All Render/Erase/Reset
/// calls must occur on the application's async context. CPR response tracking is
/// thread-safe.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>Renderer</c> class from
/// <c>prompt_toolkit.renderer</c>.
/// </para>
/// </remarks>
public sealed class Renderer
{
    /// <summary>Time to wait in seconds until we consider CPR to be not supported.</summary>
    public const int CprTimeout = 2;

    private readonly Lock _cprLock = new();
    private readonly IOutput _output;
    private readonly bool _fullScreen;
    private readonly IFilter _mouseSupport;
    private readonly Action? _cprNotSupportedCallback;

    // Terminal mode state
    private bool _inAlternateScreen;
    private bool _mouseSupportEnabled;
    private bool _bracketedPasteEnabled;
    private bool _cursorKeyModeReset;

    // CPR state (thread-safe via _cprLock)
    private readonly Queue<TaskCompletionSource<bool>> _waitingForCprFutures = new();
    private CprSupportState _cprSupport;

    // Style caching
    private StyleStringToAttrsCache? _attrsForStyle;
    private StyleStringHasStyleCache? _styleStringHasStyle;
    private object? _lastStyleHash;
    private object? _lastTransformationHash;
    private ColorDepth? _lastColorDepth;

    // Rendering state

    /// <summary>
    /// The current cursor position in the rendered output.
    /// Used by the Windows mouse event handler for coordinate adjustment.
    /// </summary>
    internal Point CursorPos => _cursorPos;

    private Point _cursorPos;
    private Screen? _lastScreen;
    private Size? _lastSize;
    private string? _lastStyle;
    private CursorShape? _lastCursorShape;
    private int _minAvailableHeight;

    /// <summary>The style for rendering.</summary>
    public IStyle Style { get; set; }

    /// <summary>Mouse handlers from the last render.</summary>
    public MouseHandlers MouseHandlers { get; private set; } = new();

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
        Action? cprNotSupportedCallback = null)
    {
        ArgumentNullException.ThrowIfNull(style);
        ArgumentNullException.ThrowIfNull(output);

        Style = style;
        _output = output;
        _fullScreen = fullScreen;
        _mouseSupport = mouseSupport ?? Never.Instance;
        _cprNotSupportedCallback = cprNotSupportedCallback;

        _inAlternateScreen = false;
        _mouseSupportEnabled = false;
        _bracketedPasteEnabled = false;
        _cursorKeyModeReset = false;

        _cprSupport = output.RespondsToCpr ? CprSupportState.Unknown : CprSupportState.NotSupported;

        Reset(scroll: true);
    }

    /// <summary>The last rendered screen, or null before first render.</summary>
    public Screen? LastRenderedScreen => _lastScreen;

    /// <summary>Whether the terminal height is known (from CPR response).</summary>
    public bool HeightIsKnown
    {
        get
        {
            if (_fullScreen || _minAvailableHeight > 0)
                return true;

            try
            {
                _minAvailableHeight = _output.GetRowsBelowCursorPosition();
                return true;
            }
            catch (NotImplementedException)
            {
                return false;
            }
        }
    }

    /// <summary>Number of rows above the current layout in non-fullscreen mode.</summary>
    public int RowsAboveLayout
    {
        get
        {
            if (_inAlternateScreen)
                return 0;

            if (_minAvailableHeight > 0)
            {
                int totalRows = _output.GetSize().Rows;
                int lastScreenHeight = _lastScreen?.Height ?? 0;
                return totalRows - Math.Max(_minAvailableHeight, lastScreenHeight);
            }

            throw new HeightIsUnknownException("Rows above layout is unknown.");
        }
    }

    /// <summary>Whether we are currently waiting for a CPR response.</summary>
    public bool WaitingForCpr
    {
        get
        {
            using (_cprLock.EnterScope())
            {
                return _waitingForCprFutures.Count > 0;
            }
        }
    }

    /// <summary>
    /// The style-string-to-Attrs cache. Used by Application.GetUsedStyleStrings().
    /// </summary>
    internal Dictionary<string, Attrs>? AttrsForStyle
    {
        get
        {
            if (_attrsForStyle is null) return null;
            return _attrsForStyle.GetCachedEntries();
        }
    }

    /// <summary>
    /// Render the application layout to the output. Computes differential
    /// updates by comparing with the previous screen.
    /// </summary>
    /// <param name="app">The application being rendered.</param>
    /// <param name="layout">The layout to render.</param>
    /// <param name="isDone">Render in 'done' state (cursor at end, no more updates).</param>
    public void Render(Application.Application<object?> app, Layout.Layout layout, bool isDone = false)
    {
        var output = _output;

        // Enter alternate screen
        if (_fullScreen && !_inAlternateScreen)
        {
            _inAlternateScreen = true;
            output.EnterAlternateScreen();
        }

        // Enable bracketed paste
        if (!_bracketedPasteEnabled)
        {
            output.EnableBracketedPaste();
            _bracketedPasteEnabled = true;
        }

        // Reset cursor key mode
        if (!_cursorKeyModeReset)
        {
            output.ResetCursorKeyMode();
            _cursorKeyModeReset = true;
        }

        // Enable/disable mouse support
        bool needsMouseSupport = _mouseSupport.Invoke();
        if (needsMouseSupport && !_mouseSupportEnabled)
        {
            output.EnableMouseSupport();
            _mouseSupportEnabled = true;
        }
        else if (!needsMouseSupport && _mouseSupportEnabled)
        {
            output.DisableMouseSupport();
            _mouseSupportEnabled = false;
        }

        // Create screen and write layout to it
        var size = output.GetSize();
        var screen = new Screen();
        screen.ShowCursor = false; // Hidden by default
        var mouseHandlers = new MouseHandlers();

        // Calculate height
        int height;
        if (_fullScreen)
        {
            height = size.Rows;
        }
        else if (isDone)
        {
            height = layout.Container.PreferredHeight(size.Columns, size.Rows).Preferred;
        }
        else
        {
            int lastHeight = _lastScreen?.Height ?? 0;
            height = Math.Max(
                _minAvailableHeight,
                Math.Max(
                    lastHeight,
                    layout.Container.PreferredHeight(size.Columns, size.Rows).Preferred));
        }
        height = Math.Min(height, size.Rows);

        // When the size changes, don't consider the previous screen
        if (_lastSize != size)
        {
            _lastScreen = null;
        }

        // When we render using another style or color depth, do a full repaint
        if (!Equals(Style.InvalidationHash, _lastStyleHash)
            || !Equals(app.StyleTransformation.InvalidationHash, _lastTransformationHash)
            || app.ColorDepth != _lastColorDepth)
        {
            _lastScreen = null;
            _attrsForStyle = null;
            _styleStringHasStyle = null;
        }

        _attrsForStyle ??= new StyleStringToAttrsCache(
            Style.GetAttrsForStyleStr, app.StyleTransformation);
        _styleStringHasStyle ??= new StyleStringHasStyleCache(_attrsForStyle);

        _lastStyleHash = Style.InvalidationHash;
        _lastTransformationHash = app.StyleTransformation.InvalidationHash;
        _lastColorDepth = app.ColorDepth;

        // Write layout to screen
        layout.Container.WriteToScreen(
            screen,
            mouseHandlers,
            new WritePosition(0, 0, size.Columns, height),
            parentStyle: "",
            eraseBg: false,
            zIndex: null);
        screen.DrawAllFloats();

        // When grayed, replace all styles in the new screen
        if (app.ExitStyle is { } exitStyle && !string.IsNullOrEmpty(exitStyle))
        {
            screen.AppendStyleToContent(exitStyle);
        }

        // Wrap screen diff and flush in synchronized output block
        output.BeginSynchronizedOutput();
        try
        {
            // Process diff and write to output
            var (newCursorPos, newLastStyle) = ScreenDiff.OutputScreenDiff(
                app,
                output,
                screen,
                _cursorPos,
                app.ColorDepth,
                _lastScreen,
                _lastStyle,
                isDone,
                fullScreen: _fullScreen,
                attrsForStyleString: _attrsForStyle,
                styleStringHasStyle: _styleStringHasStyle,
                size: size,
                previousWidth: _lastSize?.Columns ?? 0);

            _cursorPos = newCursorPos;
            _lastStyle = newLastStyle;
            _lastScreen = screen;
            _lastSize = size;
            MouseHandlers = mouseHandlers;

            // Handle cursor shapes
            var newCursorShape = app.Cursor.GetCursorShape();
            if (_lastCursorShape is null || _lastCursorShape != newCursorShape)
            {
                output.SetCursorShape(newCursorShape);
                _lastCursorShape = newCursorShape;
            }

            // Flush buffered output
            output.Flush();
        }
        finally
        {
            output.EndSynchronizedOutput();
        }

        // Set visible windows in layout
        layout.SetVisibleWindows(screen.VisibleWindows.OfType<Window>().ToList());

        if (isDone)
        {
            Reset();
        }
    }

    /// <summary>
    /// Erase the renderer output from the terminal.
    /// </summary>
    /// <param name="leaveAlternateScreen">Whether to leave alternate screen if active.</param>
    public void Erase(bool leaveAlternateScreen = true)
    {
        var output = _output;

        output.BeginSynchronizedOutput();
        try
        {
            output.CursorBackward(_cursorPos.X);
            output.CursorUp(_cursorPos.Y);
            output.EraseDown();
            output.ResetAttributes();
            output.EnableAutowrap();

            output.Flush();
        }
        finally
        {
            output.EndSynchronizedOutput();
        }

        Reset(leaveAlternateScreen: leaveAlternateScreen);
    }

    /// <summary>
    /// Clear the terminal screen completely.
    /// </summary>
    public void Clear()
    {
        var output = _output;

        output.BeginSynchronizedOutput();
        try
        {
            // Inline erase logic (avoid nested sync blocks with Erase())
            output.CursorBackward(_cursorPos.X);
            output.CursorUp(_cursorPos.Y);
            output.EraseDown();
            output.ResetAttributes();
            output.EnableAutowrap();

            output.EraseScreen();
            output.CursorGoto(0, 0);
            output.Flush();
        }
        finally
        {
            output.EndSynchronizedOutput();
        }

        Reset();
        RequestAbsoluteCursorPosition();
    }

    /// <summary>
    /// Reset the renderer state.
    /// </summary>
    /// <param name="scroll">Scroll to cursor on first render.</param>
    /// <param name="leaveAlternateScreen">Leave alternate screen if active.</param>
    public void Reset(bool scroll = false, bool leaveAlternateScreen = true)
    {
        _cursorPos = new Point(0, 0);
        _lastScreen = null;
        _lastSize = null;
        _lastStyle = null;
        _lastCursorShape = null;
        MouseHandlers = new MouseHandlers();
        _minAvailableHeight = 0;

        if (scroll)
        {
            _output.ScrollBufferToPrompt();
        }

        if (_inAlternateScreen && leaveAlternateScreen)
        {
            _output.QuitAlternateScreen();
            _inAlternateScreen = false;
        }

        if (_mouseSupportEnabled)
        {
            _output.DisableMouseSupport();
            _mouseSupportEnabled = false;
        }

        if (_bracketedPasteEnabled)
        {
            _output.DisableBracketedPaste();
            _bracketedPasteEnabled = false;
        }

        _output.ResetCursorShape();
        _output.ShowCursor();
        _output.Flush();
    }

    /// <summary>
    /// Resets renderer state for a terminal resize without performing any terminal I/O.
    /// The actual erase and redraw happen during the next <see cref="Render"/> call,
    /// inside a synchronized output block to prevent flicker.
    /// </summary>
    /// <remarks>
    /// This method only modifies in-memory state. It does NOT write any escape sequences
    /// to the terminal. This is critical for flicker-free resize: the erase and redraw
    /// are deferred to the next Render() call where they occur atomically inside a
    /// synchronized output block.
    /// </remarks>
    public void ResetForResize()
    {
        _cursorPos = new Point(0, 0);
        _lastScreen = null;
        _lastSize = null;
        _lastStyle = null;
        _lastCursorShape = null;
        MouseHandlers = new MouseHandlers();
        _minAvailableHeight = 0;
        _cursorKeyModeReset = false;
        _mouseSupportEnabled = false;
        _bracketedPasteEnabled = false;
    }

    /// <summary>
    /// Request an absolute cursor position report (CPR) from the terminal.
    /// </summary>
    public void RequestAbsoluteCursorPosition()
    {
        if (_fullScreen)
        {
            _minAvailableHeight = _output.GetSize().Rows;
            return;
        }

        // Try Win32-style API call
        try
        {
            _minAvailableHeight = _output.GetRowsBelowCursorPosition();
            return;
        }
        catch (NotImplementedException)
        {
            // Fall through to CPR
        }

        // Use CPR
        using (_cprLock.EnterScope())
        {
            if (_cprSupport == CprSupportState.NotSupported)
                return;

            if (_cprSupport == CprSupportState.Supported)
            {
                var tcs = new TaskCompletionSource<bool>();
                _waitingForCprFutures.Enqueue(tcs);
                _output.AskForCpr();
                return;
            }

            // Unknown support - test it
            if (_waitingForCprFutures.Count > 0)
                return;

            var testTcs = new TaskCompletionSource<bool>();
            _waitingForCprFutures.Enqueue(testTcs);
            _output.AskForCpr();
        }

        // Start timeout timer to detect non-support
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(CprTimeout));

            using (_cprLock.EnterScope())
            {
                if (_cprSupport == CprSupportState.Unknown)
                {
                    _cprSupport = CprSupportState.NotSupported;
                    _cprNotSupportedCallback?.Invoke();
                }
            }
        });
    }

    /// <summary>
    /// Report the absolute cursor row. Called when a CPR response is received.
    /// </summary>
    /// <param name="row">The absolute cursor row (1-based).</param>
    public void ReportAbsoluteCursorRow(int row)
    {
        using (_cprLock.EnterScope())
        {
            _cprSupport = CprSupportState.Supported;

            int totalRows = _output.GetSize().Rows;
            int rowsBelowCursor = totalRows - row + 1;
            _minAvailableHeight = rowsBelowCursor;

            if (_waitingForCprFutures.Count > 0)
            {
                var tcs = _waitingForCprFutures.Dequeue();
                tcs.TrySetResult(true);
            }
        }
    }

    /// <summary>
    /// Wait for all pending CPR responses to arrive.
    /// </summary>
    /// <returns>A task that completes when all CPR responses are received or timeout.</returns>
    public async Task WaitForCprResponsesAsync()
    {
        List<TaskCompletionSource<bool>> cprFutures;
        using (_cprLock.EnterScope())
        {
            if (_waitingForCprFutures.Count == 0 || _cprSupport == CprSupportState.NotSupported)
                return;

            cprFutures = [.. _waitingForCprFutures];
        }

        var allResponses = Task.WhenAll(cprFutures.Select(t => t.Task));
        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(1));

        await Task.WhenAny(allResponses, timeoutTask);

        // On timeout, cancel remaining futures
        if (!allResponses.IsCompleted)
        {
            using (_cprLock.EnterScope())
            {
                foreach (var tcs in _waitingForCprFutures)
                {
                    tcs.TrySetCanceled();
                }
                _waitingForCprFutures.Clear();
            }
        }
    }

    // --- Internal cache types ---

    private enum CprSupportState
    {
        Unknown,
        Supported,
        NotSupported
    }
}

/// <summary>
/// Exception thrown when height information is not available (CPR response not yet received).
/// </summary>
public sealed class HeightIsUnknownException : Exception
{
    /// <summary>Create a new HeightIsUnknownException.</summary>
    /// <param name="message">Error message.</param>
    public HeightIsUnknownException(string message) : base(message) { }
}

/// <summary>
/// Cache that maps style strings to <see cref="Attrs"/>.
/// </summary>
internal sealed class StyleStringToAttrsCache
{
    private readonly Func<string, Attrs?, Attrs> _getAttrsForStyleStr;
    private readonly IStyleTransformation _styleTransformation;
    private readonly Dictionary<string, Attrs> _cache = [];

    public StyleStringToAttrsCache(
        Func<string, Attrs?, Attrs> getAttrsForStyleStr,
        IStyleTransformation styleTransformation)
    {
        _getAttrsForStyleStr = getAttrsForStyleStr;
        _styleTransformation = styleTransformation;
    }

    /// <summary>Get or compute the Attrs for a style string.</summary>
    public Attrs this[string styleStr]
    {
        get
        {
            if (_cache.TryGetValue(styleStr, out var cached))
                return cached;

            var attrs = _getAttrsForStyleStr(styleStr, null);
            attrs = _styleTransformation.TransformAttrs(attrs);
            _cache[styleStr] = attrs;
            return attrs;
        }
    }

    /// <summary>Returns a copy of all cached entries.</summary>
    public Dictionary<string, Attrs> GetCachedEntries() => new(_cache);
}

/// <summary>
/// Cache that determines whether a style string renders non-default formatting.
/// </summary>
internal sealed class StyleStringHasStyleCache
{
    private readonly StyleStringToAttrsCache _attrsCache;
    private readonly Dictionary<string, bool> _cache = [];

    public StyleStringHasStyleCache(StyleStringToAttrsCache attrsCache)
    {
        _attrsCache = attrsCache;
    }

    /// <summary>Check if style string has visible styling (color, underline, etc.).</summary>
    public bool this[string styleStr]
    {
        get
        {
            if (_cache.TryGetValue(styleStr, out var cached))
                return cached;

            var attrs = _attrsCache[styleStr];
            bool hasStyle = !string.IsNullOrEmpty(attrs.Color)
                || !string.IsNullOrEmpty(attrs.BgColor)
                || (attrs.Underline ?? false)
                || (attrs.Strike ?? false)
                || (attrs.Blink ?? false)
                || (attrs.Reverse ?? false);

            _cache[styleStr] = hasStyle;
            return hasStyle;
        }
    }
}
