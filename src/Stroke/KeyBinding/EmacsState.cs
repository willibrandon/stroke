namespace Stroke.KeyBinding;

using InputKeyPress = Stroke.Input.KeyPress;

/// <summary>
/// Mutable class to hold Emacs-specific state.
/// </summary>
/// <remarks>
/// <para>
/// Thread safety: All property access is thread-safe. Individual operations are atomic.
/// Compound operations (read-modify-write sequences) require external synchronization.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>EmacsState</c> class from <c>prompt_toolkit.key_binding.emacs_state</c>.
/// </para>
/// </remarks>
public sealed class EmacsState
{
    private readonly Lock _lock = new();
    private List<InputKeyPress>? _macro = [];
    private List<InputKeyPress>? _currentRecording;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmacsState"/> class with default values.
    /// </summary>
    public EmacsState()
    {
    }

    /// <summary>
    /// Gets the last recorded macro, or null if no macro has been recorded or if
    /// <see cref="EndMacro"/> was called while not recording.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns a copy of the internal list to ensure thread safety.
    /// </para>
    /// <para>
    /// Initially an empty list. Becomes null if <see cref="EndMacro"/> is called
    /// while not recording (matching Python Prompt Toolkit behavior).
    /// </para>
    /// </remarks>
    public IReadOnlyList<InputKeyPress>? Macro
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _macro?.ToList();
            }
        }
    }

    /// <summary>
    /// Gets the in-progress macro recording, or null if not recording.
    /// </summary>
    /// <remarks>
    /// Returns a copy of the internal list to ensure thread safety.
    /// </remarks>
    public IReadOnlyList<InputKeyPress>? CurrentRecording
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _currentRecording?.ToList();
            }
        }
    }

    /// <summary>
    /// Gets whether a macro is currently being recorded.
    /// </summary>
    public bool IsRecording
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _currentRecording != null;
            }
        }
    }

    /// <summary>
    /// Starts recording a new macro.
    /// </summary>
    /// <remarks>
    /// Sets <see cref="CurrentRecording"/> to a new empty list.
    /// If already recording, the previous recording is discarded.
    /// </remarks>
    public void StartMacro()
    {
        using (_lock.EnterScope())
        {
            _currentRecording = [];
        }
    }

    /// <summary>
    /// Ends macro recording.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sets <see cref="Macro"/> to the current recording and clears <see cref="CurrentRecording"/>.
    /// </para>
    /// <para>
    /// If not recording, <see cref="Macro"/> becomes null (matching Python Prompt Toolkit's
    /// <c>self.macro = self.current_recording</c> which sets macro to None when not recording).
    /// </para>
    /// </remarks>
    public void EndMacro()
    {
        using (_lock.EnterScope())
        {
            // Python: self.macro = self.current_recording
            // When not recording, current_recording is None, so macro becomes None
            _macro = _currentRecording != null ? [.. _currentRecording] : null;
            _currentRecording = null;
        }
    }

    /// <summary>
    /// Appends a key press to the current recording.
    /// </summary>
    /// <param name="keyPress">The key press to append.</param>
    /// <remarks>
    /// Does nothing if not currently recording.
    /// </remarks>
    public void AppendToRecording(InputKeyPress keyPress)
    {
        using (_lock.EnterScope())
        {
            _currentRecording?.Add(keyPress);
        }
    }

    /// <summary>
    /// Resets the Emacs state.
    /// </summary>
    /// <remarks>
    /// Sets <see cref="CurrentRecording"/> to null. Does NOT clear <see cref="Macro"/>.
    /// </remarks>
    public void Reset()
    {
        using (_lock.EnterScope())
        {
            _currentRecording = null;
        }
    }
}
