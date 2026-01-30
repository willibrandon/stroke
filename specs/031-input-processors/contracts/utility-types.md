# Contract: Utility Types

**Feature**: 031-input-processors
**Namespace**: `Stroke.Layout`
**Python Source**: `prompt_toolkit/layout/utils.py` (lines 1-81)

## ExplodedList

```csharp
/// <summary>
/// A list of fragments that has been "exploded" — each element is a single
/// character. Maintains the exploded invariant: when items are added or set,
/// they are automatically exploded into single-character fragments.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>_ExplodedList</c> class from
/// <c>prompt_toolkit.layout.utils</c>.
/// </para>
/// <para>
/// This type is NOT thread-safe. It is created per-line per-render-cycle
/// and is not shared across threads.
/// </para>
/// </remarks>
public class ExplodedList : Collection<StyleAndTextTuple>
{
    /// <summary>
    /// Initializes a new ExplodedList from pre-exploded fragments.
    /// </summary>
    /// <param name="items">Already-exploded fragments (each must be single-char).</param>
    public ExplodedList(IList<StyleAndTextTuple> items);

    /// <summary>
    /// Whether this list has been exploded. Always true for ExplodedList.
    /// </summary>
    public bool Exploded => true;

    /// <summary>
    /// Inserts an item, auto-exploding if multi-character.
    /// </summary>
    protected override void InsertItem(int index, StyleAndTextTuple item);

    /// <summary>
    /// Sets an item at the given index, auto-exploding if multi-character.
    /// If the new item is multi-character, the single item at <paramref name="index"/>
    /// is removed and the exploded fragments are inserted starting at that index.
    /// This may change the list length.
    /// </summary>
    protected override void SetItem(int index, StyleAndTextTuple item);

    /// <summary>
    /// Adds a range of items, auto-exploding each.
    /// </summary>
    public void AddRange(IEnumerable<StyleAndTextTuple> items);
}
```

**Python equivalent**: `_ExplodedList(List[_T])` (lines 17-57).

**Design notes**:
- Python's `_ExplodedList` inherits from `List` and overrides `append`, `extend`, `__setitem__`. The `insert` method raises `NotImplementedError`.
- C# uses `Collection<T>` base class with `InsertItem`/`SetItem` virtual override hooks.
- The `insert` override in Python raises `NotImplementedError`. In C#, `InsertItem` performs auto-explosion instead, since `Collection<T>.Add` calls `InsertItem`.
- No `RemoveItem` override needed — removing doesn't break the single-char invariant.

---

## LayoutUtils.ExplodeTextFragments (static method)

```csharp
// Added to existing LayoutUtils static class

/// <summary>
/// Turn a list of (style, text) fragments into an ExplodedList where each
/// element is exactly one character.
/// </summary>
/// <remarks>
/// <para>
/// This function is idempotent: calling it on an already-exploded list
/// returns the same list without re-processing.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>explode_text_fragments</c> function
/// from <c>prompt_toolkit.layout.utils</c>.
/// </para>
/// </remarks>
/// <param name="fragments">The fragments to explode.</param>
/// <returns>An <see cref="ExplodedList"/> with single-character fragments.</returns>
public static ExplodedList ExplodeTextFragments(
    IReadOnlyList<StyleAndTextTuple> fragments);
```

**Python equivalent**: `explode_text_fragments(fragments)` (lines 60-80).

**Behavior**:
1. If `fragments` is already an `ExplodedList`, return it directly (idempotent).
2. Otherwise, iterate each fragment and split into per-character fragments, preserving style and mouse handler.
3. Return a new `ExplodedList` wrapping the result.
