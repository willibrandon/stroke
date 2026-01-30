namespace Stroke.Application;

/// <summary>
/// Delegate for input hook integration. Called when the application is idle
/// and waiting for input, allowing custom event loop processing.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>_StdinReader</c> input hook pattern
/// from <c>prompt_toolkit.application.application</c>.
/// </para>
/// <para>
/// This hook allows integration with external event loops (e.g., GUI frameworks).
/// The hook receives a context with a file descriptor to monitor, and should call
/// <see cref="InputHookContext.InputIsReady"/> when input is available.
/// </para>
/// </remarks>
/// <param name="context">The input hook context providing file descriptor info.</param>
public delegate void InputHook(InputHookContext context);

/// <summary>
/// Context provided to <see cref="InputHook"/> callbacks.
/// </summary>
/// <remarks>
/// <para>
/// Port of the context parameter used in Python Prompt Toolkit's input hook mechanism.
/// </para>
/// </remarks>
public sealed class InputHookContext
{
    private readonly Action _inputIsReadyCallback;

    /// <summary>
    /// Creates a new InputHookContext.
    /// </summary>
    /// <param name="fileDescriptor">The file descriptor to monitor for input.</param>
    /// <param name="inputIsReadyCallback">Callback to invoke when input is available.</param>
    internal InputHookContext(int fileDescriptor, Action inputIsReadyCallback)
    {
        FileDescriptor = fileDescriptor;
        _inputIsReadyCallback = inputIsReadyCallback ?? throw new ArgumentNullException(nameof(inputIsReadyCallback));
    }

    /// <summary>
    /// The file descriptor to monitor for input.
    /// </summary>
    public int FileDescriptor { get; }

    /// <summary>
    /// Signal that input is available. The application will resume processing input
    /// after this is called.
    /// </summary>
    public void InputIsReady() => _inputIsReadyCallback();
}
