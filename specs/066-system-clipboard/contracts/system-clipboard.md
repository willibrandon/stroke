# Contract: SystemClipboard

**Feature**: 066-system-clipboard
**Date**: 2026-02-07

## Public API

### SystemClipboard Class

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// Clipboard implementation that synchronizes with the operating system's clipboard.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>clipboard.pyperclip.PyperclipClipboard</c>.
/// It delegates to a platform-specific <c>IClipboardProvider</c> for actual clipboard I/O,
/// and adds selection type semantics: when the clipboard text matches the last text written
/// by this instance, the original <see cref="ClipboardData"/> (with its <see cref="SelectionType"/>)
/// is returned. When the text was modified externally, the selection type is inferred from
/// newline presence.
/// </para>
/// <para>
/// <b>Thread Safety:</b> This class is thread-safe. All public methods are synchronized
/// using <see cref="System.Threading.Lock"/>. Individual operations are atomic;
/// compound operations (e.g., read-modify-write sequences) require external synchronization.
/// </para>
/// </remarks>
public sealed class SystemClipboard : IClipboard
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SystemClipboard"/> class
    /// using automatic platform detection.
    /// </summary>
    /// <exception cref="Stroke.Clipboard.ClipboardProviderNotAvailableException">
    /// Thrown when no clipboard mechanism is available on the current platform.
    /// </exception>
    public SystemClipboard();

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemClipboard"/> class
    /// with a specific clipboard provider.
    /// </summary>
    /// <param name="provider">The clipboard provider to use for OS clipboard I/O.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="provider"/> is null.
    /// </exception>
    public SystemClipboard(IClipboardProvider provider);

    /// <summary>
    /// Set data on the system clipboard.
    /// </summary>
    /// <param name="data">The clipboard data to store.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="data"/> is null.
    /// </exception>
    /// <remarks>
    /// Writes the text to the OS clipboard via the provider. If the write fails,
    /// the error is silently swallowed. The data is cached for selection type
    /// preservation on subsequent reads.
    /// </remarks>
    public void SetData(ClipboardData data);

    /// <summary>
    /// Return clipboard data from the system clipboard.
    /// </summary>
    /// <returns>
    /// The clipboard data with appropriate selection type:
    /// <list type="bullet">
    /// <item>If the text matches the last <see cref="SetData"/> call, returns
    /// the original <see cref="ClipboardData"/> (preserving selection type).</item>
    /// <item>If the text was modified externally and contains newlines,
    /// returns data with <see cref="SelectionType.Lines"/>.</item>
    /// <item>If the text was modified externally and contains no newlines,
    /// returns data with <see cref="SelectionType.Characters"/>.</item>
    /// <item>If the read fails, returns empty <see cref="ClipboardData"/>.</item>
    /// </list>
    /// </returns>
    public ClipboardData GetData();

    /// <summary>
    /// Set plain text on the system clipboard with <see cref="SelectionType.Characters"/> type.
    /// </summary>
    /// <param name="text">The text to store.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="text"/> is null.
    /// </exception>
    public void SetText(string text);

    /// <summary>
    /// Rotate the kill ring (no-op for system clipboard).
    /// </summary>
    /// <remarks>
    /// OS clipboards do not support kill ring functionality.
    /// This method exists to satisfy the <see cref="IClipboard"/> interface.
    /// </remarks>
    public void Rotate();
}
```

### ClipboardProviderNotAvailableException Class

```csharp
namespace Stroke.Clipboard;

/// <summary>
/// Exception thrown when no clipboard mechanism is available on the current platform.
/// </summary>
/// <remarks>
/// This exception includes platform-specific installation guidance in its message.
/// For example, on Linux it suggests installing xclip, xsel, or wl-clipboard.
/// </remarks>
public sealed class ClipboardProviderNotAvailableException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClipboardProviderNotAvailableException"/> class.
    /// </summary>
    /// <param name="message">
    /// A message describing the platform and suggested clipboard tools to install.
    /// </param>
    public ClipboardProviderNotAvailableException(string message);

    /// <summary>
    /// Initializes a new instance of the <see cref="ClipboardProviderNotAvailableException"/> class.
    /// </summary>
    /// <param name="message">
    /// A message describing the platform and suggested clipboard tools to install.
    /// </param>
    /// <param name="innerException">The inner exception.</param>
    public ClipboardProviderNotAvailableException(string message, Exception innerException);
}
```

## Behavioral Contract

### SetData

1. Validates `data` is not null (throws `ArgumentNullException`)
2. Acquires lock
3. Caches `data` in `_lastData`
4. Calls `_provider.SetText(data.Text)` in a try/catch — write failures are silently swallowed
5. Releases lock

### GetData

1. Acquires lock
2. Calls `_provider.GetText()` in a try/catch — read failures return `""`
3. If `_lastData` is not null AND `text == _lastData.Text` → returns `_lastData` (preserves SelectionType)
4. Otherwise → returns `new ClipboardData(text, text.Contains('\n') ? SelectionType.Lines : SelectionType.Characters)`
5. Releases lock

### SetText

1. Delegates to `SetData(new ClipboardData(text))`

### Rotate

1. No-op (empty method body)

## Requirement Traceability

| Requirement | Contract Element |
|-------------|-----------------|
| FR-001 | `SystemClipboard` reads/writes OS clipboard via `IClipboardProvider` |
| FR-004 | Implements `IClipboard` interface |
| FR-005 | `_lastData` cache preserves SelectionType when text matches |
| FR-006 | `GetData()` infers Lines/Characters from newline presence |
| FR-008 | try/catch in SetData (swallow) and GetData (return empty) |
| FR-010 | `Lock` with `EnterScope()` on all public methods |
| FR-015 | `Rotate()` is a no-op |
