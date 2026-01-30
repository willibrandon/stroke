using Stroke.FormattedText;
using Stroke.Input;
using Stroke.Output;

namespace Stroke.Application;

/// <summary>
/// Sentinel application returned by <see cref="AppContext.GetApp"/> when no real
/// application is running. All run methods throw <see cref="NotImplementedException"/>.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>DummyApplication</c> class from
/// <c>prompt_toolkit.application.dummy</c>.
/// </para>
/// </remarks>
public sealed class DummyApplication : Application<object?>
{
    /// <summary>
    /// Create a DummyApplication with DummyInput and DummyOutput.
    /// </summary>
    public DummyApplication()
        : base(new DummyInput(), new DummyOutput(), isDummy: true)
    {
    }

    /// <summary>
    /// A DummyApplication is not supposed to run.
    /// </summary>
    /// <exception cref="NotImplementedException">Always thrown.</exception>
    public new object? Run(
        Action? preRun = null,
        bool setExceptionHandler = true,
        bool handleSigint = true,
        bool inThread = false,
        InputHook? inputHook = null)
    {
        throw new NotImplementedException("A DummyApplication is not supposed to run.");
    }

    /// <summary>
    /// A DummyApplication is not supposed to run.
    /// </summary>
    /// <exception cref="NotImplementedException">Always thrown.</exception>
    public new Task<object?> RunAsync(
        Action? preRun = null,
        bool setExceptionHandler = true,
        bool handleSigint = true)
    {
        throw new NotImplementedException("A DummyApplication is not supposed to run.");
    }

    /// <summary>
    /// A DummyApplication is not supposed to run system commands.
    /// </summary>
    /// <exception cref="NotImplementedException">Always thrown.</exception>
    public new Task RunSystemCommandAsync(
        string command,
        bool waitForEnter = true,
        AnyFormattedText displayBeforeText = default,
        string waitText = "Press ENTER to continue...")
    {
        throw new NotImplementedException("A DummyApplication is not supposed to run system commands.");
    }

    /// <summary>
    /// A DummyApplication is not supposed to suspend.
    /// </summary>
    /// <exception cref="NotImplementedException">Always thrown.</exception>
    public new void SuspendToBackground(bool suspendGroup = true)
    {
        throw new NotImplementedException("A DummyApplication is not supposed to suspend.");
    }
}
