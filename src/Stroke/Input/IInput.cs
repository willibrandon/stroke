namespace Stroke.Input;

/// <summary>
/// Abstraction for any terminal input source.
/// </summary>
/// <remarks>
/// <para>
/// An instance of this interface can be passed to an Application and will be used
/// for reading keyboard and mouse input. This is a faithful port of Python Prompt
/// Toolkit's <c>prompt_toolkit.input.Input</c> class.
/// </para>
/// <para>
/// Thread safety: <see cref="ReadKeys"/> and <see cref="FlushKeys"/> assume single-threaded
/// access. Thread safety at the event distribution layer (channels, queues) is the caller's
/// responsibility.
/// </para>
/// </remarks>
public interface IInput : IDisposable
{
    /// <summary>
    /// Gets whether the input stream is closed.
    /// </summary>
    /// <remarks>
    /// When true, the application should handle this as an EOF condition.
    /// </remarks>
    bool Closed { get; }

    /// <summary>
    /// Reads and parses available key presses from the input.
    /// </summary>
    /// <returns>
    /// A list of <see cref="KeyPress"/> objects parsed from the input stream.
    /// Returns an empty list if no input is available.
    /// </returns>
    /// <exception cref="ObjectDisposedException">Thrown if called after <see cref="Close"/> or <see cref="IDisposable.Dispose"/>.</exception>
    /// <remarks>
    /// <para>
    /// Blocking behavior:
    /// <list type="bullet">
    /// <item><description>When attached to event loop: Non-blocking; returns immediately with available data or empty list</description></item>
    /// <item><description>When not attached: May block until input is available (platform-dependent)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Reads raw input data and parses it into key press events.
    /// For VT100 input, this includes parsing escape sequences.
    /// Assumes single-threaded access; concurrent calls result in undefined behavior.
    /// </para>
    /// </remarks>
    IReadOnlyList<KeyPress> ReadKeys();

    /// <summary>
    /// Flushes pending partial escape sequences and returns any resulting key presses.
    /// </summary>
    /// <returns>A list of <see cref="KeyPress"/> objects from flushed partial sequences.</returns>
    /// <remarks>
    /// <para>
    /// Critical for detecting standalone Escape key presses. When Escape is pressed alone
    /// (not as part of a sequence), the parser waits briefly for additional characters.
    /// If none arrive, flush should be called to emit the Escape as a key press.
    /// </para>
    /// <para>
    /// Default implementation returns an empty list.
    /// </para>
    /// </remarks>
    IReadOnlyList<KeyPress> FlushKeys();

    /// <summary>
    /// Enters raw terminal mode.
    /// </summary>
    /// <returns>A disposable that restores the previous terminal mode when disposed.</returns>
    /// <remarks>
    /// <para>
    /// Raw mode settings:
    /// <list type="bullet">
    /// <item><description>Echo: Disabled</description></item>
    /// <item><description>Canonical mode: Disabled (characters available immediately)</description></item>
    /// <item><description>Signal generation: Disabled (Ctrl+C produces a key press)</description></item>
    /// <item><description>Flow control: Disabled (Ctrl+S/Ctrl+Q produce key presses)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// If the input is not connected to a TTY, this may return a no-op disposable.
    /// </para>
    /// </remarks>
    IDisposable RawMode();

    /// <summary>
    /// Enters cooked (canonical) terminal mode.
    /// </summary>
    /// <returns>A disposable that restores the previous terminal mode when disposed.</returns>
    /// <remarks>
    /// <para>
    /// Use case: Temporarily needing normal terminal behavior while in raw mode,
    /// for example when running a subprocess or prompting for line-buffered input.
    /// </para>
    /// <para>
    /// Cooked mode settings:
    /// <list type="bullet">
    /// <item><description>Echo: Enabled</description></item>
    /// <item><description>Canonical mode: Enabled (line buffering)</description></item>
    /// <item><description>Signal generation: Enabled</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    IDisposable CookedMode();

    /// <summary>
    /// Attaches this input to the current event loop with a callback.
    /// </summary>
    /// <param name="inputReadyCallback">A callback invoked when input is available to read.</param>
    /// <returns>A disposable that detaches the input when disposed.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="inputReadyCallback"/> is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if called after <see cref="Close"/> or <see cref="IDisposable.Dispose"/>.</exception>
    /// <remarks>
    /// <para>
    /// Multiple <see cref="Attach"/> calls are supported and form a stack (LIFO).
    /// Each call returns a unique disposable. Disposing the most recent attachment
    /// restores the previous callback.
    /// </para>
    /// <para>
    /// Callback invocation contract:
    /// <list type="bullet">
    /// <item><description>Callback is invoked on the event loop thread (platform-dependent)</description></item>
    /// <item><description>Callback MUST NOT block; it should call <see cref="ReadKeys"/> and return promptly</description></item>
    /// <item><description>Callback may be invoked spuriously (check <see cref="ReadKeys"/> result for actual data)</description></item>
    /// <item><description>Callback is NOT invoked during or after <see cref="Close"/></description></item>
    /// </list>
    /// </para>
    /// </remarks>
    IDisposable Attach(Action inputReadyCallback);

    /// <summary>
    /// Temporarily detaches this input from the event loop.
    /// </summary>
    /// <returns>A disposable that reattaches the input when disposed.</returns>
    /// <remarks>
    /// <para>
    /// Use case: Temporarily suspending input processing, for example when running
    /// an external command that needs direct terminal access.
    /// </para>
    /// <para>
    /// When not attached, returns a no-op disposable; no error is thrown.
    /// </para>
    /// </remarks>
    IDisposable Detach();

    /// <summary>
    /// Gets the native file descriptor or handle for this input.
    /// </summary>
    /// <returns>The file descriptor (POSIX) or handle (Windows) for event loop registration.</returns>
    /// <exception cref="NotSupportedException">Thrown by <see cref="DummyInput"/> which has no file descriptor.</exception>
    nint FileNo();

    /// <summary>
    /// Gets a unique identifier for storing typeahead key presses.
    /// </summary>
    /// <returns>A string that uniquely identifies this input source.</returns>
    /// <remarks>
    /// Typeahead handling stores excess key presses from one prompt for the next prompt.
    /// This hash ensures different inputs don't interfere. The hash is composed of the
    /// input type name and file descriptor/handle, ensuring uniqueness per input instance.
    /// </remarks>
    string TypeaheadHash();

    /// <summary>
    /// Reads a line directly from the underlying file descriptor/handle,
    /// bypassing .NET's Console class. Used to wait for Enter after running
    /// a system command while the application is suspended.
    /// </summary>
    /// <remarks>
    /// <para>
    /// On POSIX, .NET's <c>Console.ReadLine()</c> internally manages terminal
    /// state via termios, which conflicts with <see cref="CookedMode"/> changes.
    /// This method uses direct POSIX <c>read()</c> to avoid that conflict.
    /// </para>
    /// <para>
    /// The default implementation falls back to <c>Console.ReadLine()</c> for
    /// platforms or implementations where direct fd reading is not available.
    /// </para>
    /// </remarks>
    void ReadLineFromFd()
    {
        Console.ReadLine();
    }

    /// <summary>
    /// Closes the input.
    /// </summary>
    /// <remarks>
    /// After closing, <see cref="Closed"/> returns true and <see cref="ReadKeys"/>
    /// returns an empty list.
    /// </remarks>
    void Close();
}
