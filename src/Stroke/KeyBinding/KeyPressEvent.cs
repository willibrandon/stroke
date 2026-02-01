namespace Stroke.KeyBinding;

/// <summary>
/// Event data passed to key binding handlers.
/// </summary>
/// <remarks>
/// <para>
/// Property access is thread-safe. Buffer operations require external synchronization.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>KeyPressEvent</c> class from <c>key_processor.py</c>.
/// </para>
/// </remarks>
public class KeyPressEvent
{
    private readonly WeakReference<object>? _keyProcessorRef;
    private string? _argString;
    private int? _cachedArg;
    private const int MaxArgValue = 1_000_000;

    /// <summary>
    /// Gets the key processor that created this event.
    /// </summary>
    /// <exception cref="InvalidOperationException">KeyProcessor was garbage collected.</exception>
    /// <remarks>
    /// Returns the KeyProcessor as object until the KeyProcessor class is implemented.
    /// </remarks>
    public object KeyProcessor
    {
        get
        {
            if (_keyProcessorRef is null)
            {
                throw new InvalidOperationException("KeyProcessor reference was not set.");
            }

            if (!_keyProcessorRef.TryGetTarget(out var processor))
            {
                throw new InvalidOperationException("KeyProcessor has been garbage collected.");
            }

            return processor;
        }
    }

    /// <summary>Gets the key sequence that triggered this event.</summary>
    public IReadOnlyList<KeyPress> KeySequence { get; }

    /// <summary>Gets the previous key sequence (before this event).</summary>
    public IReadOnlyList<KeyPress> PreviousKeySequence { get; }

    /// <summary>Gets whether this is a repeat of the previous handler.</summary>
    public bool IsRepeat { get; }

    /// <summary>
    /// Gets or sets the repetition argument. Defaults to 1.
    /// Special value: -1 when arg is "-" (negative prefix).
    /// Clamped to avoid exceeding 1,000,000.
    /// </summary>
    /// <remarks>
    /// The setter allows internal code (e.g., Vi operator-pending handlers) to
    /// multiply counts before delegating to text object handlers, matching
    /// Python Prompt Toolkit's <c>event._arg = str(...)</c> pattern.
    /// </remarks>
    public int Arg
    {
        get
        {
            if (_cachedArg.HasValue)
            {
                return _cachedArg.Value;
            }

            if (string.IsNullOrEmpty(_argString))
            {
                return 1;
            }

            if (_argString == "-")
            {
                return -1;
            }

            if (int.TryParse(_argString, out int parsed))
            {
                // Clamp to MaxArgValue
                _cachedArg = Math.Min(Math.Abs(parsed), MaxArgValue);
                if (parsed < 0 || _argString.StartsWith('-'))
                {
                    _cachedArg = -_cachedArg.Value;
                }
                return _cachedArg.Value;
            }

            return 1;
        }
    }

    /// <summary>Gets whether a repetition argument was explicitly provided.</summary>
    public bool ArgPresent => !string.IsNullOrEmpty(_argString);

    /// <summary>
    /// Gets the current application.
    /// </summary>
    /// <remarks>
    /// Returns object until IApplication interface is implemented.
    /// </remarks>
    public object? App { get; }

    /// <summary>
    /// Gets the current buffer (shortcut for App.CurrentBuffer).
    /// </summary>
    public Stroke.Core.Buffer? CurrentBuffer { get; }

    /// <summary>Gets the raw data of the last key in the sequence.</summary>
    public string Data
    {
        get
        {
            if (KeySequence.Count == 0)
            {
                return string.Empty;
            }
            return KeySequence[^1].Data;
        }
    }

    /// <summary>
    /// Creates a new KeyPressEvent.
    /// </summary>
    /// <param name="keyProcessorRef">Weak reference to the key processor.</param>
    /// <param name="arg">String representation of the repetition argument.</param>
    /// <param name="keySequence">The key sequence that triggered this event.</param>
    /// <param name="previousKeySequence">The previous key sequence.</param>
    /// <param name="isRepeat">Whether this is a repeat of the previous handler.</param>
    /// <param name="app">The current application (optional until implemented).</param>
    /// <param name="currentBuffer">The current buffer (optional until implemented).</param>
    public KeyPressEvent(
        WeakReference<object>? keyProcessorRef,
        string? arg,
        IReadOnlyList<KeyPress> keySequence,
        IReadOnlyList<KeyPress> previousKeySequence,
        bool isRepeat,
        object? app = null,
        Stroke.Core.Buffer? currentBuffer = null)
    {
        ArgumentNullException.ThrowIfNull(keySequence);
        ArgumentNullException.ThrowIfNull(previousKeySequence);

        _keyProcessorRef = keyProcessorRef;
        _argString = arg;
        KeySequence = keySequence;
        PreviousKeySequence = previousKeySequence;
        IsRepeat = isRepeat;
        App = app;
        CurrentBuffer = currentBuffer;
    }

    /// <summary>
    /// Appends a digit to the repetition argument.
    /// </summary>
    /// <param name="data">Digit character ('0'-'9' or '-').</param>
    /// <exception cref="ArgumentException">Invalid digit.</exception>
    public void AppendToArgCount(string data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.Length != 1)
        {
            throw new ArgumentException("Data must be a single character.", nameof(data));
        }

        char c = data[0];

        if (c == '-')
        {
            // Handle negative prefix
            if (string.IsNullOrEmpty(_argString))
            {
                _argString = "-";
                _cachedArg = null;
                return;
            }
            // If already has content, ignore additional minus
            return;
        }

        if (c < '0' || c > '9')
        {
            throw new ArgumentException($"Invalid digit: '{c}'. Must be '0'-'9' or '-'.", nameof(data));
        }

        // Append digit
        _argString = (_argString ?? "") + c;
        _cachedArg = null;

        // Check if we've exceeded max and truncate
        if (_argString.Length > 7) // "-" + 6 digits = 7 chars for up to 1M
        {
            // Parse and clamp
            _ = Arg; // Force evaluation and clamping
        }
    }

    /// <summary>
    /// Sets the repetition argument to a specific integer value.
    /// </summary>
    /// <param name="value">The new argument value.</param>
    /// <remarks>
    /// Used by Vi operator-pending handlers to multiply counts:
    /// <c>(ViState.OperatorArg ?? 1) * (event.Arg ?? 1)</c> before calling text object handlers.
    /// Port of Python Prompt Toolkit's <c>event._arg = str(...)</c> pattern.
    /// </remarks>
    internal void SetArg(int value)
    {
        _argString = value.ToString();
        _cachedArg = null;
    }

    /// <summary>Backwards compatibility alias for App.</summary>
    [Obsolete("Use App property instead.")]
    public object? Cli => App;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"KeyPressEvent(KeySequence=[{string.Join(", ", KeySequence)}], Arg={Arg}, IsRepeat={IsRepeat})";
    }
}
