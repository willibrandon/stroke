namespace Stroke.Core;

/// <summary>
/// Interface for clipboard operations.
/// </summary>
/// <remarks>
/// <para>
/// This interface defines the contract for clipboard implementations that store
/// and retrieve text with selection type information.
/// </para>
/// <para>
/// Implementations include <see cref="DummyClipboard"/> (no-op),
/// <see cref="InMemoryClipboard"/> (kill ring), and <see cref="DynamicClipboard"/>
/// (runtime delegation).
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>clipboard.base.Clipboard</c>
/// abstract base class.
/// </para>
/// </remarks>
public interface IClipboard
{
    /// <summary>
    /// Set data on the clipboard.
    /// </summary>
    /// <param name="data">The clipboard data to store.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    void SetData(ClipboardData data);

    /// <summary>
    /// Return the clipboard data.
    /// </summary>
    /// <returns>
    /// The current clipboard data, or an empty <see cref="ClipboardData"/> if nothing has been stored.
    /// </returns>
    ClipboardData GetData();

    /// <summary>
    /// Shortcut for setting plain text on the clipboard with <see cref="SelectionType.Characters"/> type.
    /// </summary>
    /// <param name="text">The text to store.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null.</exception>
    void SetText(string text) => SetData(new ClipboardData(text));

    /// <summary>
    /// For clipboard implementations that support kill ring functionality,
    /// rotate the kill ring to access previous clipboard entries.
    /// </summary>
    /// <remarks>
    /// The default implementation is a no-op. Override in implementations
    /// that maintain clipboard history (e.g., <see cref="InMemoryClipboard"/>).
    /// </remarks>
    void Rotate() { }
}
