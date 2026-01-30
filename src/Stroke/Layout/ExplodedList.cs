using System.Collections.ObjectModel;
using Stroke.FormattedText;

namespace Stroke.Layout;

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
    public ExplodedList(IList<StyleAndTextTuple> items) : base(items)
    {
    }

    /// <summary>
    /// Whether this list has been exploded. Always true for ExplodedList.
    /// </summary>
    public bool Exploded => true;

    /// <summary>
    /// Inserts an item, auto-exploding if multi-character.
    /// </summary>
    protected override void InsertItem(int index, StyleAndTextTuple item)
    {
        if (item.Text.Length <= 1)
        {
            base.InsertItem(index, item);
        }
        else
        {
            // Explode the item and insert each character
            var offset = 0;
            foreach (var c in item.Text)
            {
                base.InsertItem(index + offset, new StyleAndTextTuple(item.Style, c.ToString(), item.MouseHandler));
                offset++;
            }
        }
    }

    /// <summary>
    /// Sets an item at the given index, auto-exploding if multi-character.
    /// If the new item is multi-character, the single item at the index
    /// is removed and the exploded fragments are inserted starting at that index.
    /// This may change the list length.
    /// </summary>
    protected override void SetItem(int index, StyleAndTextTuple item)
    {
        if (item.Text.Length <= 1)
        {
            base.SetItem(index, item);
        }
        else
        {
            // Remove the current item and insert exploded fragments
            base.RemoveItem(index);
            var offset = 0;
            foreach (var c in item.Text)
            {
                base.InsertItem(index + offset, new StyleAndTextTuple(item.Style, c.ToString(), item.MouseHandler));
                offset++;
            }
        }
    }

    /// <summary>
    /// Adds a range of items, auto-exploding each.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public void AddRange(IEnumerable<StyleAndTextTuple> items)
    {
        foreach (var item in items)
        {
            Add(item); // Add calls InsertItem which auto-explodes
        }
    }
}
