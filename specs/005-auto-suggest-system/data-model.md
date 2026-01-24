# Data Model: Auto Suggest System

**Feature**: 005-auto-suggest-system
**Date**: 2026-01-23

## Entities

### Suggestion

Immutable container for suggested text to append after the cursor.

```csharp
namespace Stroke.AutoSuggest;

/// <summary>
/// Represents a suggestion returned by an auto-suggest implementation.
/// </summary>
/// <param name="Text">The suggested text to insert after the cursor. Cannot be null; empty string is valid.</param>
/// <remarks>
/// Thread-safe: Immutable record type with no mutable state.
/// </remarks>
public sealed record Suggestion(string Text)
{
    /// <summary>
    /// Returns a string representation for debugging.
    /// </summary>
    /// <returns>Format: <c>Suggestion({Text})</c> matching Python's <c>__repr__</c>.</returns>
    public override string ToString() => $"Suggestion({Text})";
}
```

**Attributes**:
| Name | Type | Nullable | Description |
|------|------|----------|-------------|
| Text | string | No (non-null) | The suggested completion text |

**Validation Rules**:
- Text cannot be null (enforced by record primary constructor)
- Empty string is valid (represents "no additional text" suggestion)
- No maximum length constraint

**Identity**: Value-based equality (record default)

**Thread Safety**: Immutable record; inherently thread-safe

---

### IAutoSuggest

Interface contract for all auto-suggestion providers.

```csharp
namespace Stroke.AutoSuggest;

/// <summary>
/// Base interface for auto suggestion implementations.
/// </summary>
/// <remarks>
/// Implementations MUST be thread-safe. Both buffer and document are passed separately
/// because auto suggestions may be retrieved asynchronously while buffer text changes.
/// Always use <paramref name="document"/>.Text for matching, not buffer.Document.Text.
/// </remarks>
public interface IAutoSuggest
{
    /// <summary>
    /// Return a suggestion for the given buffer and document.
    /// </summary>
    /// <param name="buffer">The current buffer (provides history access).</param>
    /// <param name="document">The document snapshot at suggestion request time.</param>
    /// <returns>A suggestion, or null if no suggestion is available.</returns>
    Suggestion? GetSuggestion(IBuffer buffer, Document document);

    /// <summary>
    /// Return a suggestion asynchronously.
    /// </summary>
    /// <param name="buffer">The current buffer (provides history access).</param>
    /// <param name="document">The document snapshot at suggestion request time.</param>
    /// <returns>A suggestion, or null if no suggestion is available.</returns>
    /// <remarks>
    /// Default implementations may simply return <c>ValueTask.FromResult(GetSuggestion(buffer, document))</c>.
    /// Override for truly asynchronous providers (network, AI, etc.).
    /// </remarks>
    ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document);
}
```

**Contract Notes**:
- Both buffer and document are passed because async scenarios may have buffer changes during suggestion generation
- Document is the frozen snapshot to use for matching; buffer provides history access
- Implementations MUST use `document.Text` not `buffer.Document.Text` for correctness
- Return `null` to indicate "no suggestion available" (not an empty Suggestion)

---

### IBuffer (Stub Interface)

Minimal interface stub for buffer access. Will be expanded in Feature 05.

```csharp
namespace Stroke.Core;

/// <summary>
/// Interface for text buffer with editing capabilities.
/// </summary>
/// <remarks>
/// This is a minimal stub for the Auto Suggest feature; full implementation in Feature 05.
/// </remarks>
public interface IBuffer
{
    /// <summary>
    /// Gets the current document snapshot.
    /// </summary>
    Document Document { get; }

    /// <summary>
    /// Gets the history associated with this buffer.
    /// </summary>
    IHistory History { get; }
}
```

---

### IHistory (Stub Interface)

Minimal interface stub for history access. Will be expanded in History feature.

```csharp
namespace Stroke.History;

/// <summary>
/// Interface for command history storage.
/// </summary>
/// <remarks>
/// This is a minimal stub; full implementation in History feature.
/// Returns history entries ordered oldest-to-newest; search implementations iterate in reverse.
/// </remarks>
public interface IHistory
{
    /// <summary>
    /// Gets all history strings.
    /// </summary>
    /// <returns>Read-only list of history entries, oldest first.</returns>
    IReadOnlyList<string> GetStrings();
}
```

---

## Implementation Types

### DummyAutoSuggest

Null object pattern - always returns no suggestion.

```csharp
namespace Stroke.AutoSuggest;

/// <summary>
/// AutoSuggest that doesn't return any suggestion.
/// </summary>
/// <remarks>
/// Thread-safe: Stateless implementation with no fields.
/// Used as fallback when no auto-suggest is configured or when DynamicAutoSuggest callback returns null.
/// </remarks>
public sealed class DummyAutoSuggest : IAutoSuggest
{
    /// <inheritdoc />
    public Suggestion? GetSuggestion(IBuffer buffer, Document document) => null;

    /// <inheritdoc />
    public ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document)
        => ValueTask.FromResult<Suggestion?>(null);
}
```

---

### AutoSuggestFromHistory

Searches buffer history for matching line prefixes.

```csharp
namespace Stroke.AutoSuggest;

/// <summary>
/// Give suggestions based on the lines in the history.
/// </summary>
/// <remarks>
/// Thread-safe: Stateless implementation with no mutable fields.
///
/// Algorithm:
/// 1. Extract current line (text after last '\n')
/// 2. Skip if empty or whitespace-only
/// 3. Search history entries from most recent to oldest
/// 4. Within each entry, search lines from last to first
/// 5. Return suffix of first case-sensitive prefix match
/// </remarks>
public sealed class AutoSuggestFromHistory : IAutoSuggest
{
    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown if buffer or document is null.</exception>
    public Suggestion? GetSuggestion(IBuffer buffer, Document document)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentNullException.ThrowIfNull(document);

        var history = buffer.History;

        // Consider only the last line for the suggestion (Python: rsplit("\n", 1)[-1])
        var text = document.Text;
        var lastNewlineIndex = text.LastIndexOf('\n');
        var currentLine = lastNewlineIndex >= 0 ? text[(lastNewlineIndex + 1)..] : text;

        // Only create a suggestion when this is not an empty line (Python: text.strip())
        if (string.IsNullOrWhiteSpace(currentLine))
            return null;

        // Find first matching line in history (search from most recent)
        var historyStrings = history.GetStrings();
        for (int i = historyStrings.Count - 1; i >= 0; i--)
        {
            var entry = historyStrings[i];
            // Python: string.splitlines() then reversed()
            var lines = entry.Split('\n');
            for (int j = lines.Length - 1; j >= 0; j--)
            {
                var line = lines[j];
                // Case-sensitive prefix match (Python: line.startswith(text))
                if (line.StartsWith(currentLine, StringComparison.Ordinal))
                {
                    return new Suggestion(line[currentLine.Length..]);
                }
            }
        }

        return null;
    }

    /// <inheritdoc />
    public ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document)
        => ValueTask.FromResult(GetSuggestion(buffer, document));
}
```

**Algorithm** (matches Python exactly):
1. Extract current line via `rsplit("\n", 1)[-1]` equivalent: `text.LastIndexOf('\n')` + slice
2. Skip if empty/whitespace via `text.strip()` equivalent: `string.IsNullOrWhiteSpace`
3. Iterate `reversed(list(history.get_strings()))`: indices from Count-1 down to 0
4. Within each entry, iterate `reversed(string.splitlines())`: lines from last to first
5. Check `line.startswith(text)`: case-sensitive ordinal comparison
6. Return `Suggestion(line[len(text):])`: suffix after matched prefix

---

### ConditionalAutoSuggest

Wrapper that only returns suggestions when condition is true.

```csharp
namespace Stroke.AutoSuggest;

/// <summary>
/// Auto suggest that can be turned on/off based on a condition.
/// </summary>
/// <remarks>
/// Thread-safe: Stores only readonly references; no mutable state.
///
/// Python accepts <c>filter: bool | Filter</c> with <c>to_filter()</c> conversion.
/// Stroke uses <c>Func&lt;bool&gt;</c> for simplicity until Stroke.Filters is implemented.
/// The filter is evaluated on every call (not cached).
/// </remarks>
public sealed class ConditionalAutoSuggest : IAutoSuggest
{
    private readonly IAutoSuggest _autoSuggest;
    private readonly Func<bool> _filter;

    /// <summary>
    /// Creates a conditional auto suggest.
    /// </summary>
    /// <param name="autoSuggest">The underlying auto suggest to wrap.</param>
    /// <param name="filter">The condition that must return true for suggestions. Evaluated on every call.</param>
    /// <exception cref="ArgumentNullException">Thrown if autoSuggest or filter is null.</exception>
    public ConditionalAutoSuggest(IAutoSuggest autoSuggest, Func<bool> filter)
    {
        ArgumentNullException.ThrowIfNull(autoSuggest);
        ArgumentNullException.ThrowIfNull(filter);
        _autoSuggest = autoSuggest;
        _filter = filter;
    }

    /// <inheritdoc />
    /// <remarks>If filter throws, exception propagates to caller.</remarks>
    public Suggestion? GetSuggestion(IBuffer buffer, Document document)
        => _filter() ? _autoSuggest.GetSuggestion(buffer, document) : null;

    /// <inheritdoc />
    /// <remarks>If filter throws, exception propagates to caller.</remarks>
    public ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document)
        => _filter() ? _autoSuggest.GetSuggestionAsync(buffer, document) : ValueTask.FromResult<Suggestion?>(null);
}
```

---

### DynamicAutoSuggest

Wrapper that selects auto-suggest provider at runtime.

```csharp
namespace Stroke.AutoSuggest;

/// <summary>
/// Auto suggest class that can dynamically return any AutoSuggest.
/// </summary>
/// <remarks>
/// Thread-safe: Stores only readonly reference; no mutable state.
///
/// The callback is evaluated on every call (both sync and async) - no caching.
/// If callback returns null, falls back to DummyAutoSuggest (instantiated per call).
/// If callback throws, exception propagates to caller.
/// </remarks>
public sealed class DynamicAutoSuggest : IAutoSuggest
{
    private readonly Func<IAutoSuggest?> _getAutoSuggest;

    /// <summary>
    /// Creates a dynamic auto suggest.
    /// </summary>
    /// <param name="getAutoSuggest">Function that returns the actual auto suggest to use. Called on every suggestion request.</param>
    /// <exception cref="ArgumentNullException">Thrown if getAutoSuggest is null.</exception>
    public DynamicAutoSuggest(Func<IAutoSuggest?> getAutoSuggest)
    {
        ArgumentNullException.ThrowIfNull(getAutoSuggest);
        _getAutoSuggest = getAutoSuggest;
    }

    /// <inheritdoc />
    /// <remarks>If callback throws, exception propagates to caller.</remarks>
    public Suggestion? GetSuggestion(IBuffer buffer, Document document)
    {
        var autoSuggest = _getAutoSuggest() ?? new DummyAutoSuggest();
        return autoSuggest.GetSuggestion(buffer, document);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Evaluates callback each time (matches Python behavior).
    /// If callback throws, exception propagates to caller.
    /// </remarks>
    public ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document)
    {
        var autoSuggest = _getAutoSuggest() ?? new DummyAutoSuggest();
        return autoSuggest.GetSuggestionAsync(buffer, document);
    }
}
```

---

### ThreadedAutoSuggest

Wrapper that executes suggestion generation on thread pool.

```csharp
namespace Stroke.AutoSuggest;

/// <summary>
/// Wrapper that runs auto suggestion in a background thread.
/// </summary>
/// <remarks>
/// Thread-safe: Stores only readonly reference; execution is thread-safe via Task.Run.
///
/// Python uses <c>run_in_executor_with_context</c> to offload to thread pool while preserving
/// execution context. .NET's <c>Task.Run</c> already captures execution context, providing
/// equivalent semantics.
///
/// <c>ConfigureAwait(false)</c> is used because auto-suggest results don't need to return
/// to a specific synchronization context (no UI thread affinity required at this layer).
/// </remarks>
public sealed class ThreadedAutoSuggest : IAutoSuggest
{
    private readonly IAutoSuggest _autoSuggest;

    /// <summary>
    /// Creates a threaded auto suggest.
    /// </summary>
    /// <param name="autoSuggest">The underlying auto suggest to run in background.</param>
    /// <exception cref="ArgumentNullException">Thrown if autoSuggest is null.</exception>
    public ThreadedAutoSuggest(IAutoSuggest autoSuggest)
    {
        ArgumentNullException.ThrowIfNull(autoSuggest);
        _autoSuggest = autoSuggest;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Synchronous call delegates directly to wrapped provider on current thread.
    /// No threading occurs for sync calls (matches Python behavior).
    /// </remarks>
    public Suggestion? GetSuggestion(IBuffer buffer, Document document)
        => _autoSuggest.GetSuggestion(buffer, document);

    /// <inheritdoc />
    /// <remarks>
    /// Executes wrapped provider's sync method on thread pool via Task.Run.
    /// If wrapped provider throws, exception is captured and re-thrown when awaited.
    /// </remarks>
    public async ValueTask<Suggestion?> GetSuggestionAsync(IBuffer buffer, Document document)
    {
        return await Task.Run(() => _autoSuggest.GetSuggestion(buffer, document)).ConfigureAwait(false);
    }
}
```

---

## Relationships

```
┌─────────────────┐
│   IAutoSuggest  │◄───────────────────────────────────────┐
└────────┬────────┘                                        │
         │ implements                                      │
         │                                                 │
    ┌────┴────┬──────────────┬──────────────┬─────────────┐
    ▼         ▼              ▼              ▼             ▼
┌────────┐ ┌────────────┐ ┌────────────┐ ┌──────────┐ ┌────────────┐
│ Dummy  │ │ FromHistory│ │ Conditional│ │ Dynamic  │ │ Threaded   │
│AutoSug │ │ AutoSuggest│ │ AutoSuggest│ │AutoSuggest│ │AutoSuggest │
└────────┘ └─────┬──────┘ └─────┬──────┘ └────┬─────┘ └─────┬──────┘
                 │              │             │             │
                 │              │ wraps       │ wraps       │ wraps
                 │              ▼             ▼             ▼
                 │         IAutoSuggest  IAutoSuggest  IAutoSuggest
                 │
                 │ uses
                 ▼
           ┌──────────┐
           │ IBuffer  │
           │ .History │
           └────┬─────┘
                │
                ▼
           ┌──────────┐
           │ IHistory │
           │.GetStrings│
           └──────────┘
```

## State Transitions

None - all types are stateless. Each method call is independent.

## Validation Summary

| Entity | Field | Validation |
|--------|-------|------------|
| Suggestion | Text | Not null (record enforces) |
| All Wrappers | Constructor args | ArgumentNullException.ThrowIfNull |
| AutoSuggestFromHistory | buffer, document | ArgumentNullException.ThrowIfNull |

## Thread Safety Summary

| Type | Strategy | Fields | Thread-Safe |
|------|----------|--------|-------------|
| `Suggestion` | Immutable record | `Text` (readonly) | ✓ |
| `DummyAutoSuggest` | Stateless | None | ✓ |
| `AutoSuggestFromHistory` | Stateless | None | ✓ |
| `ConditionalAutoSuggest` | Immutable wrapper | `_autoSuggest`, `_filter` (readonly) | ✓ |
| `DynamicAutoSuggest` | Immutable wrapper | `_getAutoSuggest` (readonly) | ✓ |
| `ThreadedAutoSuggest` | Immutable wrapper | `_autoSuggest` (readonly) | ✓ |

All types are inherently thread-safe due to immutability or statelessness. No synchronization primitives are required.
