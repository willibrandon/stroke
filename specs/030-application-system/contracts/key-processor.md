# Contract: KeyProcessor

**Namespace**: `Stroke.KeyBinding`
**Source**: `prompt_toolkit.key_binding.key_processor`

## KeyProcessor Class

```csharp
/// <summary>
/// State machine that receives <see cref="KeyPress"/> instances and dispatches them
/// to the matching handlers from the given <see cref="IKeyBindingsBase"/>.
/// </summary>
/// <remarks>
/// <para>
/// The KeyProcessor is NOT thread-safe. All calls to <see cref="Feed"/>,
/// <see cref="FeedMultiple"/>, <see cref="ProcessKeys"/>, <see cref="EmptyQueue"/>,
/// <see cref="SendSigint"/>, and <see cref="Reset"/> MUST occur on the application's
/// async context (the thread running <see cref="Application{TResult}.RunAsync"/>).
/// </para>
/// <para>
/// <b>Thread boundary:</b> Input from background threads reaches the KeyProcessor through
/// the Application's input reading mechanism: the <see cref="IInput"/> is attached with a
/// callback that reads keys and feeds them to the KeyProcessor. This callback runs in the
/// application's async context (via <c>SynchronizationContext.Post</c> or context copy).
/// Background threads that need to inject input should call <see cref="Application{TResult}.Invalidate"/>
/// (thread-safe) rather than directly feeding the KeyProcessor.
/// </para>
/// <para>
/// For SIGINT, the signal handler calls <see cref="SendSigint"/> via
/// <c>loop.call_soon_threadsafe</c> (or equivalent), ensuring it executes on the async context.
/// </para>
/// </remarks>
public sealed class KeyProcessor
{
    /// <summary>
    /// Create a new KeyProcessor with the given key bindings registry.
    /// </summary>
    /// <param name="keyBindings">The key bindings registry to dispatch against.</param>
    public KeyProcessor(IKeyBindingsBase keyBindings);

    /// <summary>The pending key presses waiting to be processed.</summary>
    public IReadOnlyCollection<KeyPress> InputQueue { get; }

    /// <summary>The current key sequence being matched.</summary>
    public IReadOnlyList<KeyPress> KeyBuffer { get; }

    /// <summary>
    /// The numeric argument accumulator (e.g., "3" in "3dd" for Vi).
    /// Null when no argument has been typed.
    /// </summary>
    public string? Arg { get; }

    /// <summary>Event fired before processing a key press.</summary>
    public Event<KeyPressEvent> BeforeKeyPress { get; }

    /// <summary>Event fired after processing a key press.</summary>
    public Event<KeyPressEvent> AfterKeyPress { get; }

    // --- Methods ---

    /// <summary>
    /// Add a key press to the input queue for future processing.
    /// </summary>
    /// <param name="keyPress">The key press to enqueue.</param>
    public void Feed(KeyPress keyPress);

    /// <summary>
    /// Add multiple key presses to the input queue.
    /// </summary>
    /// <param name="keyPresses">The key presses to enqueue.</param>
    public void FeedMultiple(IEnumerable<KeyPress> keyPresses);

    /// <summary>
    /// Process all keys in the input queue. Dispatch algorithm:
    /// <list type="number">
    /// <item>Dequeue next key from input queue, append to key buffer.</item>
    /// <item><b>Exact match:</b> Check if key buffer matches any binding exactly via
    /// <c>GetBindingsForKeys(keyBuffer)</c>. If matches found, evaluate their filters.</item>
    /// <item><b>Prefix match:</b> Check if key buffer is a prefix of any binding via
    /// <c>GetBindingsStartingWithKeys(keyBuffer)</c>. If prefixes exist, wait for more keys
    /// (the sequence is not yet complete).</item>
    /// <item><b>Eager bindings:</b> If an exact match is marked eager, dispatch immediately
    /// even if prefix matches exist.</item>
    /// <item><b>No match:</b> If no exact match and no prefix match, flush the key buffer
    /// (discard or pass through as individual keys).</item>
    /// <item><b>Flush timeout:</b> If prefix matches exist but no more keys arrive within
    /// <see cref="Application{TResult}.TtimeoutLen"/> seconds, flush the escape key.
    /// If within <see cref="Application{TResult}.TimeoutLen"/> seconds, dispatch the
    /// best available exact match (if any) or flush.</item>
    /// </list>
    /// For each dispatched binding: fires <see cref="BeforeKeyPress"/>, invokes the handler,
    /// fires <see cref="AfterKeyPress"/>, then calls <c>App.Invalidate()</c>.
    /// </summary>
    public void ProcessKeys();

    /// <summary>
    /// Empty the input queue and return any unprocessed key presses.
    /// Called by the Application during shutdown to extract unprocessed keys
    /// for typeahead storage. The returned keys are stored via
    /// <c>TypeaheadBuffer.Store(input, keys)</c> and replayed on the next
    /// <see cref="Application{TResult}.RunAsync"/> call with the same input.
    /// Also clears the key buffer (current partial sequence).
    /// </summary>
    /// <returns>Unprocessed key presses from the queue and key buffer.</returns>
    public IReadOnlyList<KeyPress> EmptyQueue();

    /// <summary>
    /// Send a SIGINT key event to the processor, as if the user pressed Ctrl+C.
    /// </summary>
    public void SendSigint();

    /// <summary>
    /// Reset the processor state: clear key buffer, argument, and input queue.
    /// </summary>
    public void Reset();
}
```

## KeyPressEvent

Note: `KeyPressEvent` already exists in `Stroke.KeyBinding`. This documents the fields that require Application access:

```csharp
/// <summary>
/// Event data passed to key binding handlers. Provides access to the application,
/// current buffer, key sequence, and argument.
/// </summary>
public sealed class KeyPressEvent
{
    // Already existing properties:
    // public KeyPress KeyPress { get; }
    // public IReadOnlyList<KeyPress> KeySequence { get; }
    // public string? Arg { get; }

    /// <summary>The current Application instance.</summary>
    public Application<object?> App { get; }

    /// <summary>The currently focused Buffer (shortcut for App.CurrentBuffer).</summary>
    public Buffer CurrentBuffer { get; }

    /// <summary>Whether this event is a repeat (for key repeat handling).</summary>
    public bool IsRepeat { get; }
}
```
