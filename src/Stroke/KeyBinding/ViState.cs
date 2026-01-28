namespace Stroke.KeyBinding;

using Stroke.Clipboard;

/// <summary>
/// Mutable class to hold Vi navigation state.
/// </summary>
/// <remarks>
/// <para>
/// Thread safety: All property access is thread-safe. Individual operations are atomic.
/// Compound operations (read-modify-write sequences) require external synchronization.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>ViState</c> class from <c>prompt_toolkit.key_binding.vi_state</c>.
/// </para>
/// </remarks>
public sealed class ViState
{
    private readonly Lock _lock = new();
    private readonly Dictionary<string, ClipboardData?> _namedRegisters = new();

    private InputMode _inputMode = InputMode.Insert;
    private CharacterFind? _lastCharacterFind;
    private OperatorFuncDelegate? _operatorFunc;
    private int? _operatorArg;
    private bool _waitingForDigraph;
    private string? _digraphSymbol1;
    private bool _tildeOperator;
    private string? _recordingRegister;
    private string _currentRecording = "";
    private bool _temporaryNavigationMode;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViState"/> class with default values.
    /// </summary>
    public ViState()
    {
    }

    /// <summary>
    /// Gets or sets the current Vi input mode.
    /// </summary>
    /// <remarks>
    /// When set to <see cref="InputMode.Navigation"/>, automatically clears:
    /// <see cref="WaitingForDigraph"/>, <see cref="OperatorFunc"/>, and <see cref="OperatorArg"/>.
    /// </remarks>
    public InputMode InputMode
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _inputMode;
            }
        }
        set
        {
            using (_lock.EnterScope())
            {
                if (value == InputMode.Navigation)
                {
                    // Clear state on Navigation mode transition per FR-006
                    _waitingForDigraph = false;
                    _operatorFunc = null;
                    _operatorArg = null;
                }
                _inputMode = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the last character find operation for repeat (;) and reverse (,) commands.
    /// </summary>
    public CharacterFind? LastCharacterFind
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _lastCharacterFind;
            }
        }
        set
        {
            using (_lock.EnterScope())
            {
                _lastCharacterFind = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the pending operator function callback.
    /// </summary>
    /// <remarks>
    /// Set when waiting for a text object after an operator command (e.g., after 'd' in 'dw').
    /// </remarks>
    public OperatorFuncDelegate? OperatorFunc
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _operatorFunc;
            }
        }
        set
        {
            using (_lock.EnterScope())
            {
                _operatorFunc = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the count argument for the pending operator.
    /// </summary>
    public int? OperatorArg
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _operatorArg;
            }
        }
        set
        {
            using (_lock.EnterScope())
            {
                _operatorArg = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the editor is waiting for the second digraph character.
    /// </summary>
    public bool WaitingForDigraph
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _waitingForDigraph;
            }
        }
        set
        {
            using (_lock.EnterScope())
            {
                _waitingForDigraph = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the first digraph symbol when <see cref="WaitingForDigraph"/> is true.
    /// </summary>
    public string? DigraphSymbol1
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _digraphSymbol1;
            }
        }
        set
        {
            using (_lock.EnterScope())
            {
                _digraphSymbol1 = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets whether tilde (~) acts as an operator.
    /// </summary>
    public bool TildeOperator
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _tildeOperator;
            }
        }
        set
        {
            using (_lock.EnterScope())
            {
                _tildeOperator = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the register being recorded to, or null if not recording.
    /// </summary>
    public string? RecordingRegister
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _recordingRegister;
            }
        }
        set
        {
            using (_lock.EnterScope())
            {
                _recordingRegister = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the accumulated macro content during recording.
    /// </summary>
    public string CurrentRecording
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _currentRecording;
            }
        }
        set
        {
            using (_lock.EnterScope())
            {
                _currentRecording = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets whether temporary navigation mode is active (Ctrl+O in insert mode).
    /// </summary>
    public bool TemporaryNavigationMode
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _temporaryNavigationMode;
            }
        }
        set
        {
            using (_lock.EnterScope())
            {
                _temporaryNavigationMode = value;
            }
        }
    }

    /// <summary>
    /// Gets the value of a named register.
    /// </summary>
    /// <param name="name">The register name (typically a single character a-z).</param>
    /// <returns>The clipboard data stored in the register, or null if not set.</returns>
    public ClipboardData? GetNamedRegister(string name)
    {
        using (_lock.EnterScope())
        {
            return _namedRegisters.TryGetValue(name, out var data) ? data : null;
        }
    }

    /// <summary>
    /// Sets the value of a named register.
    /// </summary>
    /// <param name="name">The register name (typically a single character a-z).</param>
    /// <param name="data">The clipboard data to store.</param>
    public void SetNamedRegister(string name, ClipboardData? data)
    {
        using (_lock.EnterScope())
        {
            _namedRegisters[name] = data;
        }
    }

    /// <summary>
    /// Clears a named register.
    /// </summary>
    /// <param name="name">The register name to clear.</param>
    /// <returns>True if the register was present and removed; otherwise, false.</returns>
    public bool ClearNamedRegister(string name)
    {
        using (_lock.EnterScope())
        {
            return _namedRegisters.Remove(name);
        }
    }

    /// <summary>
    /// Gets all named register names currently set.
    /// </summary>
    /// <returns>A collection of register names.</returns>
    /// <remarks>
    /// Returns a copy of the register names to ensure thread safety.
    /// </remarks>
    public IReadOnlyCollection<string> GetNamedRegisterNames()
    {
        using (_lock.EnterScope())
        {
            return _namedRegisters.Keys.ToList();
        }
    }

    /// <summary>
    /// Resets the Vi state to initial values.
    /// </summary>
    /// <remarks>
    /// Sets <see cref="InputMode"/> to <see cref="InputMode.Insert"/> and clears
    /// <see cref="WaitingForDigraph"/>, <see cref="OperatorFunc"/>, <see cref="OperatorArg"/>,
    /// <see cref="RecordingRegister"/>, and <see cref="CurrentRecording"/>.
    /// Does NOT clear <see cref="LastCharacterFind"/>, named registers, <see cref="TildeOperator"/>,
    /// <see cref="TemporaryNavigationMode"/>, or <see cref="DigraphSymbol1"/>.
    /// </remarks>
    public void Reset()
    {
        using (_lock.EnterScope())
        {
            // Reset to Insert mode (this is direct assignment, not via property setter,
            // to avoid the Navigation mode side effects)
            _inputMode = InputMode.Insert;
            _waitingForDigraph = false;
            _operatorFunc = null;
            _operatorArg = null;
            _recordingRegister = null;
            _currentRecording = "";

            // The following are NOT cleared by Reset():
            // - _lastCharacterFind
            // - _namedRegisters
            // - _tildeOperator
            // - _temporaryNavigationMode
            // - _digraphSymbol1
        }
    }
}
