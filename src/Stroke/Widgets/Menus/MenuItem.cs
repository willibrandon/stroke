using Stroke.Core;
using Stroke.Input;
using Stroke.KeyBinding;

namespace Stroke.Widgets.Menus;

/// <summary>
/// A menu item for use in <see cref="MenuContainer"/>.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>MenuItem</c> class from <c>widgets/menus.py</c>.
/// </para>
/// </remarks>
public class MenuItem
{
    /// <summary>
    /// Gets the display text for this menu item.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the handler to invoke when this item is activated, or null.
    /// </summary>
    public Action? Handler { get; }

    /// <summary>
    /// Gets the child menu items (sub-menu).
    /// </summary>
    public IReadOnlyList<MenuItem> Children { get; }

    /// <summary>
    /// Gets the keyboard shortcut hint (for display only).
    /// </summary>
    public IReadOnlyList<KeyOrChar>? Shortcut { get; }

    /// <summary>
    /// Gets whether this item is disabled.
    /// </summary>
    public bool Disabled { get; }

    /// <summary>
    /// Gets or sets the currently selected child item index.
    /// </summary>
    public int SelectedItem { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuItem"/> class.
    /// </summary>
    /// <param name="text">Display text for the menu item.</param>
    /// <param name="handler">Handler to invoke when activated.</param>
    /// <param name="children">Child menu items (sub-menu).</param>
    /// <param name="shortcut">Keyboard shortcut hint.</param>
    /// <param name="disabled">Whether the item is disabled.</param>
    public MenuItem(
        string text = "",
        Action? handler = null,
        IReadOnlyList<MenuItem>? children = null,
        IReadOnlyList<KeyOrChar>? shortcut = null,
        bool disabled = false)
    {
        Text = text;
        Handler = handler;
        Children = children ?? [];
        Shortcut = shortcut;
        Disabled = disabled;
        SelectedItem = 0;
    }

    /// <summary>
    /// Gets the width needed to display children (max child text width).
    /// </summary>
    public int Width
    {
        get
        {
            if (Children.Count == 0)
                return 0;

            var maxWidth = 0;
            foreach (var child in Children)
            {
                var w = UnicodeWidth.GetWidth(child.Text);
                if (w > maxWidth)
                    maxWidth = w;
            }
            return maxWidth;
        }
    }
}
