using Stroke.Application;
using Stroke.Core;
using Stroke.Input;

using AppContext = Stroke.Application.AppContext;

namespace Stroke.KeyBinding;

/// <summary>
/// State machine that receives <see cref="KeyPress"/> instances and dispatches them
/// to the matching handlers from the given <see cref="IKeyBindingsBase"/>.
/// </summary>
/// <remarks>
/// <para>
/// The KeyProcessor is NOT thread-safe. All calls to <see cref="Feed"/>,
/// <see cref="FeedMultiple"/>, <see cref="ProcessKeys"/>, <see cref="EmptyQueue"/>,
/// <see cref="SendSigint"/>, and <see cref="Reset"/> MUST occur on the application's
/// async context (the thread running <c>Application.RunAsync</c>).
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>KeyProcessor</c> class from
/// <c>prompt_toolkit.key_binding.key_processor</c>.
/// </para>
/// </remarks>
public sealed class KeyProcessor
{
    private readonly IKeyBindingsBase _bindings;
    private readonly LinkedList<KeyPress> _inputQueue = new();
    private readonly List<KeyPress> _keyBuffer = [];
    private List<KeyPress> _previousKeySequence = [];
    private Binding? _previousHandler;

    // Flush sentinel
    private static readonly KeyPress FlushSentinel = new(new KeyOrChar('?'), "_Flush");

    // Timeout task
    private CancellationTokenSource? _flushWaitCts;

    /// <summary>
    /// Create a new KeyProcessor with the given key bindings registry.
    /// </summary>
    /// <param name="keyBindings">The key bindings registry to dispatch against.</param>
    public KeyProcessor(IKeyBindingsBase keyBindings)
    {
        ArgumentNullException.ThrowIfNull(keyBindings);
        _bindings = keyBindings;

        BeforeKeyPress = new Event<KeyPressEvent>(null!);
        AfterKeyPress = new Event<KeyPressEvent>(null!);

        Reset();
    }

    /// <summary>The pending key presses waiting to be processed.</summary>
    public IReadOnlyCollection<KeyPress> InputQueue => _inputQueue;

    /// <summary>The current key sequence being matched.</summary>
    public IReadOnlyList<KeyPress> KeyBuffer => _keyBuffer;

    /// <summary>
    /// The numeric argument accumulator (e.g., "3" in "3dd" for Vi).
    /// Null when no argument has been typed.
    /// </summary>
    public string? Arg { get; set; }

    /// <summary>Event fired before processing a key press.</summary>
    public Event<KeyPressEvent> BeforeKeyPress { get; }

    /// <summary>Event fired after processing a key press.</summary>
    public Event<KeyPressEvent> AfterKeyPress { get; }

    /// <summary>
    /// Add a key press to the input queue for future processing.
    /// </summary>
    /// <param name="keyPress">The key press to enqueue.</param>
    /// <param name="first">If true, insert before everything else.</param>
    public void Feed(KeyPress keyPress, bool first = false)
    {
        if (first)
        {
            _inputQueue.AddFirst(keyPress);
        }
        else
        {
            _inputQueue.AddLast(keyPress);
        }
    }

    /// <summary>
    /// Add multiple key presses to the input queue.
    /// </summary>
    /// <param name="keyPresses">The key presses to enqueue.</param>
    /// <param name="first">If true, insert before everything else.</param>
    public void FeedMultiple(IEnumerable<KeyPress> keyPresses, bool first = false)
    {
        ArgumentNullException.ThrowIfNull(keyPresses);

        if (first)
        {
            // Insert in reverse order at the front to maintain order
            var node = _inputQueue.First;
            foreach (var kp in keyPresses)
            {
                if (node is null)
                {
                    _inputQueue.AddLast(kp);
                    node = _inputQueue.Last;
                }
                else
                {
                    _inputQueue.AddBefore(node, kp);
                }
            }
        }
        else
        {
            foreach (var kp in keyPresses)
            {
                _inputQueue.AddLast(kp);
            }
        }
    }

    /// <summary>
    /// Process all keys in the input queue. Dispatch algorithm:
    /// <list type="number">
    /// <item>Dequeue next key from input queue, append to key buffer.</item>
    /// <item><b>Exact match:</b> Check if key buffer matches any binding exactly.
    /// If matches found, evaluate their filters.</item>
    /// <item><b>Prefix match:</b> Check if key buffer is a prefix of any binding.
    /// If prefixes exist, wait for more keys.</item>
    /// <item><b>Eager bindings:</b> If an exact match is marked eager, dispatch immediately
    /// even if prefix matches exist.</item>
    /// <item><b>No match:</b> If no exact match and no prefix match, flush the key buffer.</item>
    /// <item><b>Flush timeout:</b> If prefix matches exist but no more keys arrive within
    /// timeout, flush the escape key.</item>
    /// </list>
    /// </summary>
    public void ProcessKeys()
    {
        var app = AppContext.GetApp();
        bool isFlush = false;

        while (NotEmpty(app))
        {
            var keyPress = GetNext(app);
            isFlush = IsFlushSentinel(keyPress);
            bool isCpr = !isFlush && keyPress.Key.IsKey && keyPress.Key.Key == Keys.CPRResponse;

            if (!isFlush && !isCpr)
            {
                BeforeKeyPress.Fire();
            }

            try
            {
                ProcessSingleKey(keyPress, isFlush);
            }
            catch
            {
                // If something goes wrong in the parser, restart the processor
                Reset();
                EmptyQueue();
                throw;
            }

            if (!isFlush && !isCpr)
            {
                AfterKeyPress.Fire();
            }
        }

        // Skip timeout if the last key was flush
        if (!isFlush)
        {
            StartTimeout();
        }
    }

    /// <summary>
    /// Empty the input queue and return any unprocessed key presses.
    /// Called by the Application during shutdown to extract unprocessed keys
    /// for typeahead storage.
    /// </summary>
    /// <returns>Unprocessed key presses from the queue and key buffer.</returns>
    public IReadOnlyList<KeyPress> EmptyQueue()
    {
        var result = new List<KeyPress>();

        // Add key buffer contents
        result.AddRange(_keyBuffer);
        _keyBuffer.Clear();

        // Add queue contents, filtering out CPR responses
        foreach (var kp in _inputQueue)
        {
            if (!(kp.Key.IsKey && kp.Key.Key == Keys.CPRResponse))
            {
                result.Add(kp);
            }
        }
        _inputQueue.Clear();

        return result;
    }

    /// <summary>
    /// Send a SIGINT key event to the processor, as if the user pressed Ctrl+C.
    /// </summary>
    public void SendSigint()
    {
        Feed(new KeyPress(Keys.SIGINT), first: true);
        ProcessKeys();
    }

    /// <summary>
    /// Reset the processor state: clear key buffer, argument, and input queue.
    /// </summary>
    public void Reset()
    {
        _previousKeySequence = [];
        _previousHandler = null;
        _inputQueue.Clear();
        _keyBuffer.Clear();
        Arg = null;

        // Cancel any pending flush timeout
        _flushWaitCts?.Cancel();
        _flushWaitCts = null;
    }

    // --- Private helpers ---

    private bool NotEmpty(object app)
    {
        // When application result is set, only process CPR responses
        if (app is IApplicationDoneCheck doneCheck && doneCheck.IsDone)
        {
            foreach (var k in _inputQueue)
            {
                if (k.Key.IsKey && k.Key.Key == Keys.CPRResponse)
                    return true;
            }
            return false;
        }
        return _inputQueue.Count > 0;
    }

    private KeyPress GetNext(object app)
    {
        if (app is IApplicationDoneCheck doneCheck && doneCheck.IsDone)
        {
            // Only process CPR responses when done. Everything else is typeahead.
            var node = _inputQueue.First;
            while (node is not null)
            {
                if (node.Value.Key.IsKey && node.Value.Key.Key == Keys.CPRResponse)
                {
                    _inputQueue.Remove(node);
                    return node.Value;
                }
                node = node.Next;
            }
        }

        var first = _inputQueue.First!.Value;
        _inputQueue.RemoveFirst();
        return first;
    }

    private static bool IsFlushSentinel(KeyPress kp)
    {
        return kp.Data == "_Flush";
    }

    private void ProcessSingleKey(KeyPress keyPress, bool isFlush)
    {
        if (!isFlush)
        {
            _keyBuffer.Add(keyPress);
        }

        if (_keyBuffer.Count == 0)
            return;

        var matches = GetMatches(_keyBuffer);
        bool isPrefixOfLongerMatch = isFlush ? false : IsPrefixOfLongerMatch(_keyBuffer);

        // When eager matches were found, give priority to them
        var eagerMatches = matches.Where(m => m.Eager.Invoke()).ToList();
        if (eagerMatches.Count > 0)
        {
            matches = eagerMatches;
            isPrefixOfLongerMatch = false;
        }

        // Exact matches found, call handler
        if (!isPrefixOfLongerMatch && matches.Count > 0)
        {
            CallHandler(matches[^1], [.. _keyBuffer]);
            _keyBuffer.Clear();
        }
        // No match found
        else if (!isPrefixOfLongerMatch && matches.Count == 0)
        {
            bool found = false;

            // Loop over the input, try longest match first and shift
            for (int i = _keyBuffer.Count; i > 0; i--)
            {
                var subBuffer = _keyBuffer.Take(i).ToList();
                var subMatches = GetMatches(subBuffer);
                if (subMatches.Count > 0)
                {
                    CallHandler(subMatches[^1], [.. subBuffer]);
                    _keyBuffer.RemoveRange(0, i);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                // Remove first key and retry
                if (_keyBuffer.Count > 0)
                {
                    _keyBuffer.RemoveAt(0);
                }
            }

            // Retry with remaining buffer (don't consume another key from queue)
            if (_keyBuffer.Count > 0)
            {
                ProcessSingleKey(default, isFlush: true);
            }
        }
        // else: is prefix of longer match — wait for more keys
    }

    private List<Binding> GetMatches(List<KeyPress> keyPresses)
    {
        var keys = keyPresses.Select(k => k.Key).ToList();
        return _bindings.GetBindingsForKeys(keys)
            .Where(b => b.Filter.Invoke())
            .ToList();
    }

    private bool IsPrefixOfLongerMatch(List<KeyPress> keyPresses)
    {
        var keys = keyPresses.Select(k => k.Key).ToList();
        var startingWith = _bindings.GetBindingsStartingWithKeys(keys);

        // Collect unique filters and check if any is active
        var filters = new HashSet<Filters.IFilter>();
        foreach (var b in startingWith)
        {
            filters.Add(b.Filter);
        }

        return filters.Any(f => f.Invoke());
    }

    private void CallHandler(Binding handler, List<KeyPress> keySequence)
    {
        var app = AppContext.GetApp();
        bool wasRecordingEmacs = app.EmacsState.IsRecording;
        bool wasRecordingVi = !string.IsNullOrEmpty(app.ViState.RecordingRegister);
        bool wasTemporaryNavigationMode = app.ViState.TemporaryNavigationMode;
        string? arg = Arg;
        Arg = null;

        var @event = new KeyPressEvent(
            keyProcessorRef: new WeakReference<object>(this),
            arg: arg,
            keySequence: keySequence,
            previousKeySequence: _previousKeySequence,
            isRepeat: handler == _previousHandler,
            app: app,
            currentBuffer: app.CurrentBuffer);

        // Save to undo stack if SaveBefore returns true
        if (handler.SaveBefore(@event))
        {
            app.CurrentBuffer.SaveToUndoStack();
        }

        // Call handler
        try
        {
            handler.Call(@event);
            FixViCursorPosition(@event);
        }
        catch (Core.EditReadOnlyBufferException)
        {
            // When a key binding does an attempt to change a buffer which is
            // read-only, we can ignore that. We sound a bell and go on.
            app.Output.Bell();
        }

        if (wasTemporaryNavigationMode)
        {
            LeaveViTempNavigationMode(@event);
        }

        _previousKeySequence = keySequence;
        _previousHandler = handler;

        // Record the key sequence in our macro.
        // (Only if we're in macro mode before and after executing the key.)
        if (handler.RecordInMacro.Invoke())
        {
            if (app.EmacsState.IsRecording && wasRecordingEmacs)
            {
                // Convert KeyBinding.KeyPress → Input.KeyPress for recording
                foreach (var k in keySequence)
                {
                    var inputKey = k.Key.IsKey ? k.Key.Key : Keys.Any;
                    app.EmacsState.AppendToRecording(new Input.KeyPress(inputKey, k.Data));
                }
            }

            if (!string.IsNullOrEmpty(app.ViState.RecordingRegister) && wasRecordingVi)
            {
                foreach (var k in keySequence)
                {
                    app.ViState.CurrentRecording += k.Data;
                }
            }
        }
    }

    private static void FixViCursorPosition(KeyPressEvent @event)
    {
        // After every command, make sure that if we are in Vi navigation mode,
        // we never put the cursor after the last character of a line.
        var app = AppContext.GetApp();
        var buff = app.CurrentBuffer;

        if (AppFilters.ViNavigationMode.Invoke()
            && buff.Document.IsCursorAtTheEndOfLine
            && buff.Document.CurrentLine.Length > 0)
        {
            buff.CursorPosition -= 1;
            // Note: In Python PTK, preferred_column is restored here.
            // In C#, PreferredColumn is managed internally by the Buffer's
            // CursorPosition setter, so no explicit restore is needed.
        }
    }

    private void LeaveViTempNavigationMode(KeyPressEvent @event)
    {
        var app = AppContext.GetApp();

        if (app.EditingMode == EditingMode.Vi)
        {
            // Not waiting for a text object and no argument has been given
            if (app.ViState.OperatorFunc is null && Arg is null)
            {
                app.ViState.TemporaryNavigationMode = false;
            }
        }
    }

    private void StartTimeout()
    {
        var app = AppContext.GetApp();
        var timeout = app.TimeoutLen;

        if (timeout is null)
            return;

        // Cancel previous timeout
        _flushWaitCts?.Cancel();
        _flushWaitCts = new CancellationTokenSource();
        var cts = _flushWaitCts;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(timeout.Value), cts.Token);
                if (_keyBuffer.Count > 0)
                {
                    // Flush keys
                    Feed(FlushSentinel);
                    ProcessKeys();
                }
            }
            catch (OperationCanceledException)
            {
                // Cancelled — no action needed
            }
        }, cts.Token);
    }
}

/// <summary>
/// Internal interface for checking if an application is done (result has been set).
/// Used by KeyProcessor to determine whether to process only CPR responses.
/// </summary>
internal interface IApplicationDoneCheck
{
    /// <summary>Whether the application result has been set.</summary>
    bool IsDone { get; }
}
