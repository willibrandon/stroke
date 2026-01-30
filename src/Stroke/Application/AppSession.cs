using Stroke.Input;
using Stroke.Output;

namespace Stroke.Application;

/// <summary>
/// An interactive session connected to one terminal. Within one session,
/// multiple applications can run sequentially. The input/output device is
/// not supposed to change during one session.
/// </summary>
/// <remarks>
/// <para>
/// Always use <see cref="AppContext.CreateAppSession"/> to create an instance
/// so that it gets activated correctly.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>AppSession</c> from
/// <c>prompt_toolkit.application.current</c>.
/// </para>
/// <para>
/// This class is thread-safe. All mutable state is protected by Lock.
/// </para>
/// </remarks>
public sealed class AppSession : IDisposable
{
    private readonly Lock _lock = new();
    private IInput? _input;
    private IOutput? _output;
    private readonly IInput? _explicitInput;
    private readonly IOutput? _explicitOutput;
    private bool _disposed;

    // Internal: the app set by SetApp context manager
    private object? _app; // Application<object?> once Application is defined

    // The session that was current when this session was created via CreateAppSession.
    // Used to restore the previous session on Dispose, forming a proper stack.
    internal AppSession? PreviousSession { get; set; }

    /// <summary>
    /// Create a new AppSession with optional input/output.
    /// </summary>
    /// <param name="input">Default input for applications in this session.</param>
    /// <param name="output">Default output for applications in this session.</param>
    public AppSession(IInput? input = null, IOutput? output = null)
    {
        _explicitInput = input;
        _explicitOutput = output;
        _input = input;
        _output = output;
    }

    /// <summary>
    /// The default input for this session. Created lazily via InputFactory if not provided.
    /// </summary>
    public IInput Input
    {
        get
        {
            using (_lock.EnterScope())
            {
                if (_input is null)
                {
                    _input = InputFactory.Create();
                }
                return _input;
            }
        }
    }

    /// <summary>
    /// The default output for this session. Created lazily via OutputFactory if not provided.
    /// </summary>
    public IOutput Output
    {
        get
        {
            using (_lock.EnterScope())
            {
                if (_output is null)
                {
                    _output = OutputFactory.Create();
                }
                return _output;
            }
        }
    }

    /// <summary>
    /// The currently active application in this session, or null if none is running.
    /// Set internally by the application during RunAsync via <see cref="AppContext.SetApp"/>.
    /// </summary>
    internal object? App
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _app;
            }
        }
        set
        {
            using (_lock.EnterScope())
            {
                _app = value;
            }
        }
    }

    /// <summary>
    /// The explicitly provided input (before lazy creation), or null.
    /// Used by CreateAppSession to avoid accidentally creating I/O.
    /// </summary>
    internal IInput? ExplicitInput
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _explicitInput;
            }
        }
    }

    /// <summary>
    /// The explicitly provided output (before lazy creation), or null.
    /// Used by CreateAppSession to avoid accidentally creating I/O.
    /// </summary>
    internal IOutput? ExplicitOutput
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _explicitOutput;
            }
        }
    }

    /// <summary>
    /// Dispose of the session, restoring the previous session as current.
    /// Disposal is idempotent.
    /// </summary>
    public void Dispose()
    {
        using (_lock.EnterScope())
        {
            if (_disposed) return;
            _disposed = true;
        }

        // Restore previous session
        AppContext.RestorePreviousSession(this);
    }

    /// <inheritdoc/>
    public override string ToString() => $"AppSession(app={App})";
}
