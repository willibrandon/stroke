using Stroke.Core;

namespace Stroke.Clipboard;

/// <summary>
/// Clipboard implementation that delegates to a dynamically selected clipboard.
/// </summary>
/// <remarks>
/// <para>
/// Use this clipboard when you need to switch between different clipboard
/// implementations at runtime. The delegate function is called on every operation
/// to determine which clipboard to use.
/// </para>
/// <para>
/// When the delegate returns <c>null</c>, operations fall back to
/// <see cref="DummyClipboard"/> behavior (no-op for writes, empty data for reads).
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>clipboard.base.DynamicClipboard</c>.
/// </para>
/// <para>
/// <b>Thread Safety:</b> This class delegates thread safety to the underlying clipboard
/// implementation. If the underlying clipboard is thread-safe (e.g., <see cref="InMemoryClipboard"/>),
/// then this wrapper is also thread-safe. The delegate function itself should be thread-safe
/// if accessed from multiple threads.
/// </para>
/// </remarks>
public sealed class DynamicClipboard : IClipboard
{
    private readonly Func<IClipboard?> _getClipboard;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicClipboard"/> class.
    /// </summary>
    /// <param name="getClipboard">
    /// A function that returns the clipboard to delegate to.
    /// When this function returns <c>null</c>, operations fall back to <see cref="DummyClipboard"/> behavior.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="getClipboard"/> is null.</exception>
    public DynamicClipboard(Func<IClipboard?> getClipboard)
    {
        ArgumentNullException.ThrowIfNull(getClipboard);
        _getClipboard = getClipboard;
    }

    /// <summary>
    /// Gets the current clipboard, falling back to DummyClipboard if null.
    /// </summary>
    private IClipboard GetClipboardOrDummy() => _getClipboard() ?? new DummyClipboard();

    /// <summary>
    /// Set data on the current clipboard.
    /// </summary>
    /// <param name="data">The clipboard data to store.</param>
    public void SetData(ClipboardData data) => GetClipboardOrDummy().SetData(data);

    /// <summary>
    /// Return the clipboard data from the current clipboard.
    /// </summary>
    /// <returns>The current clipboard data.</returns>
    public ClipboardData GetData() => GetClipboardOrDummy().GetData();

    /// <summary>
    /// Set plain text on the current clipboard.
    /// </summary>
    /// <param name="text">The text to store.</param>
    public void SetText(string text) => GetClipboardOrDummy().SetText(text);

    /// <summary>
    /// Rotate the kill ring on the current clipboard.
    /// </summary>
    public void Rotate() => GetClipboardOrDummy().Rotate();
}
