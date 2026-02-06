using System.Threading;
using Stroke.Core;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Widgets.Base;
using AppContext = Stroke.Application.AppContext;

namespace Stroke.Widgets.Menus;

/// <summary>
/// Container with a menu bar and floating dropdown menus.
/// </summary>
/// <remarks>
/// <para>
/// Wraps a body container with a top menu bar. Supports up to 3 levels of nested menus.
/// Navigation via arrow keys, Enter to activate, Escape/Ctrl+C to close.
/// Mouse interaction is supported for both the menu bar and dropdown items.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>MenuContainer</c> class from <c>widgets/menus.py</c>.
/// </para>
/// <para>
/// This class is thread-safe. All mutable state (<c>_selectedMenu</c>) is protected
/// by a <see cref="Lock"/>. Individual operations are atomic; compound operations
/// require external synchronization by the caller.
/// </para>
/// </remarks>
public class MenuContainer : IMagicContainer
{
    private readonly Lock _lock = new();
    private readonly List<int> _selectedMenu = [0];
    private readonly FloatContainer _container;

    /// <summary>
    /// Gets the body container displayed below the menu bar.
    /// </summary>
    public AnyContainer Body { get; }

    /// <summary>
    /// Gets the menu items for the menu bar.
    /// </summary>
    public IReadOnlyList<MenuItem> MenuItems { get; }

    /// <summary>
    /// Gets the FormattedTextControl used for the menu bar.
    /// </summary>
    public FormattedTextControl Control { get; }

    /// <summary>
    /// Gets the Window containing the menu bar.
    /// </summary>
    public Window Window { get; }

    /// <summary>
    /// Gets the mutable list of floats (accessible for dynamic dialog insertion).
    /// </summary>
    public List<Float> Floats { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuContainer"/> class.
    /// </summary>
    /// <param name="body">The body container displayed below the menu bar.</param>
    /// <param name="menuItems">Menu items for the menu bar.</param>
    /// <param name="floats">Additional floats to display.</param>
    /// <param name="keyBindings">Additional key bindings.</param>
    public MenuContainer(
        AnyContainer body,
        IReadOnlyList<MenuItem> menuItems,
        IReadOnlyList<Float>? floats = null,
        IKeyBindingsBase? keyBindings = null)
    {
        Body = body;
        MenuItems = menuItems;

        // Key bindings for menu navigation.
        var kb = new KeyBindings();

        var inMainMenu = new Condition(() => { using (_lock.EnterScope()) return _selectedMenu.Count == 1; });
        var inSubMenu = new Condition(() => { using (_lock.EnterScope()) return _selectedMenu.Count > 1; });

        // Navigation through the main menu.
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Left)],
            filter: new FilterOrBool(inMainMenu))((e) =>
        {
            using (_lock.EnterScope())
                _selectedMenu[0] = Math.Max(0, _selectedMenu[0] - 1);
            return null;
        });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Right)],
            filter: new FilterOrBool(inMainMenu))((e) =>
        {
            using (_lock.EnterScope())
                _selectedMenu[0] = Math.Min(MenuItems.Count - 1, _selectedMenu[0] + 1);
            return null;
        });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Down)],
            filter: new FilterOrBool(inMainMenu))((e) =>
        {
            using (_lock.EnterScope())
                _selectedMenu.Add(0);
            return null;
        });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlC)],
            filter: new FilterOrBool(inMainMenu))((e) =>
        {
            e.GetApp().Layout.FocusLast();
            return null;
        });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlG)],
            filter: new FilterOrBool(inMainMenu))((e) =>
        {
            e.GetApp().Layout.FocusLast();
            return null;
        });

        // Sub menu navigation.
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Left)],
            filter: new FilterOrBool(inSubMenu))((e) =>
        {
            using (_lock.EnterScope())
            {
                if (_selectedMenu.Count > 1)
                    _selectedMenu.RemoveAt(_selectedMenu.Count - 1);
            }
            return null;
        });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlG)],
            filter: new FilterOrBool(inSubMenu))((e) =>
        {
            using (_lock.EnterScope())
            {
                if (_selectedMenu.Count > 1)
                    _selectedMenu.RemoveAt(_selectedMenu.Count - 1);
            }
            return null;
        });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlC)],
            filter: new FilterOrBool(inSubMenu))((e) =>
        {
            using (_lock.EnterScope())
            {
                if (_selectedMenu.Count > 1)
                    _selectedMenu.RemoveAt(_selectedMenu.Count - 1);
            }
            return null;
        });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Right)],
            filter: new FilterOrBool(inSubMenu))((e) =>
        {
            using (_lock.EnterScope())
            {
                var menu = GetMenu(_selectedMenu.Count - 1);
                if (menu.Children.Count > 0)
                {
                    _selectedMenu.Add(0);
                }
                else if (_selectedMenu.Count == 2
                         && _selectedMenu[0] < MenuItems.Count - 1)
                {
                    var currentIndex = _selectedMenu[0];
                    _selectedMenu.Clear();
                    _selectedMenu.Add(Math.Min(MenuItems.Count - 1, currentIndex + 1));
                    if (MenuItems[_selectedMenu[0]].Children.Count > 0)
                        _selectedMenu.Add(0);
                }
            }
            return null;
        });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Up)],
            filter: new FilterOrBool(inSubMenu))((e) =>
        {
            using (_lock.EnterScope())
            {
                var menu = GetMenu(_selectedMenu.Count - 2);
                var index = _selectedMenu[^1];

                var previousIndex = -1;
                for (var i = 0; i < menu.Children.Count; i++)
                {
                    if (i < index && !menu.Children[i].Disabled)
                        previousIndex = i;
                }

                if (previousIndex >= 0)
                    _selectedMenu[^1] = previousIndex;
                else if (_selectedMenu.Count == 2)
                    _selectedMenu.RemoveAt(_selectedMenu.Count - 1);
            }
            return null;
        });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.Down)],
            filter: new FilterOrBool(inSubMenu))((e) =>
        {
            using (_lock.EnterScope())
            {
                var menu = GetMenu(_selectedMenu.Count - 2);
                var index = _selectedMenu[^1];

                var nextIndex = -1;
                for (var i = 0; i < menu.Children.Count; i++)
                {
                    if (i > index && !menu.Children[i].Disabled)
                    {
                        nextIndex = i;
                        break;
                    }
                }

                if (nextIndex >= 0)
                    _selectedMenu[^1] = nextIndex;
            }
            return null;
        });

        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar(Keys.ControlM)])((e) =>
        {
            Action? handler;
            using (_lock.EnterScope())
            {
                var item = GetMenu(_selectedMenu.Count - 1);
                handler = item.Handler;
            }
            if (handler != null)
            {
                e.GetApp().Layout.FocusLast();
                handler();
            }
            return null;
        });

        // Controls.
        Control = new FormattedTextControl(
            GetMenuFragments,
            keyBindings: kb,
            focusable: true,
            showCursor: false);

        Window = new Window(
            height: Dimension.Exact(1),
            content: Control,
            style: "class:menu-bar");

        var submenu = CreateSubmenu(0);
        var submenu2 = CreateSubmenu(1);
        var submenu3 = CreateSubmenu(2);

        var hasFocus = new Condition(() =>
            AppContext.GetApp().Layout.CurrentWindow == Window);

        var allFloats = new List<Float>
        {
            new Float(
                xcursor: true,
                ycursor: true,
                content: new AnyContainer(new ConditionalContainer(
                    content: new AnyContainer(new Shadow(new AnyContainer(submenu)).PtContainer()),
                    filter: new FilterOrBool(hasFocus)))),
            new Float(
                attachToWindow: submenu,
                xcursor: true,
                ycursor: true,
                allowCoverCursor: true,
                content: new AnyContainer(new ConditionalContainer(
                    content: new AnyContainer(new Shadow(new AnyContainer(submenu2)).PtContainer()),
                    filter: new FilterOrBool(hasFocus
                        & new Condition(() => { using (_lock.EnterScope()) return _selectedMenu.Count >= 1; }))))),
            new Float(
                attachToWindow: submenu2,
                xcursor: true,
                ycursor: true,
                allowCoverCursor: true,
                content: new AnyContainer(new ConditionalContainer(
                    content: new AnyContainer(new Shadow(new AnyContainer(submenu3)).PtContainer()),
                    filter: new FilterOrBool(hasFocus
                        & new Condition(() => { using (_lock.EnterScope()) return _selectedMenu.Count >= 2; }))))),
        };

        if (floats != null)
        {
            foreach (var f in floats)
                allFloats.Add(f);
        }

        Floats = allFloats;

        _container = new FloatContainer(
            content: new AnyContainer(new HSplit(
            [
                Window,
                body.ToContainer(),
            ])),
            floats: allFloats,
            keyBindings: keyBindings);
    }

    private MenuItem GetMenu(int level)
    {
        if (_selectedMenu.Count == 0 || _selectedMenu[0] >= MenuItems.Count)
            return new MenuItem("debug");

        var menu = MenuItems[_selectedMenu[0]];

        for (var i = 0; i < _selectedMenu.Count - 1; i++)
        {
            if (i < level)
            {
                var index = _selectedMenu[i + 1];
                if (index < menu.Children.Count)
                    menu = menu.Children[index];
                else
                    return new MenuItem("debug");
            }
        }

        return menu;
    }

    private IReadOnlyList<StyleAndTextTuple> GetMenuFragments()
    {
        var focused = AppContext.GetApp().Layout.HasFocus(Window);

        using (_lock.EnterScope())
        {
            // Reset menu state when focus is lost.
            if (!focused)
            {
                _selectedMenu.Clear();
                _selectedMenu.Add(0);
            }

            var result = new List<StyleAndTextTuple>();

            for (var i = 0; i < MenuItems.Count; i++)
            {
                var item = MenuItems[i];
                var idx = i; // capture for closure

                NotImplementedOrNone MouseHandler(MouseEvent mouseEvent)
                {
                    var hover = mouseEvent.EventType == MouseEventType.MouseMove;
                    if (mouseEvent.EventType == MouseEventType.MouseDown
                        || (hover && focused))
                    {
                        var app = AppContext.GetApp();
                        if (!hover)
                        {
                            if (app.Layout.HasFocus(Window))
                            {
                                using (_lock.EnterScope())
                                {
                                    if (_selectedMenu.Count == 1 && _selectedMenu[0] == idx)
                                        app.Layout.FocusLast();
                                }
                            }
                            else
                            {
                                app.Layout.Focus(Window);
                            }
                        }
                        using (_lock.EnterScope())
                        {
                            _selectedMenu.Clear();
                            _selectedMenu.Add(idx);
                        }
                    }
                    return NotImplementedOrNone.None;
                }

                result.Add(new StyleAndTextTuple("class:menu-bar", " ", MouseHandler));

                if (i == _selectedMenu[0] && focused)
                {
                    result.Add(new StyleAndTextTuple("[SetMenuPosition]", "", MouseHandler));
                    result.Add(new StyleAndTextTuple("class:menu-bar.selected-item", item.Text, MouseHandler));
                }
                else
                {
                    result.Add(new StyleAndTextTuple("class:menu-bar", item.Text, MouseHandler));
                }
            }

            return result;
        }
    }

    private Window CreateSubmenu(int level)
    {
        IReadOnlyList<StyleAndTextTuple> GetTextFragments()
        {
            using (_lock.EnterScope())
            {
                var result = new List<StyleAndTextTuple>();

                if (level < _selectedMenu.Count)
                {
                    var menu = GetMenu(level);
                    if (menu.Children.Count > 0)
                    {
                        result.Add(new StyleAndTextTuple("class:menu", Border.TopLeft));
                        result.Add(new StyleAndTextTuple("class:menu",
                            new string(Border.Horizontal[0], menu.Width + 4)));
                        result.Add(new StyleAndTextTuple("class:menu", Border.TopRight));
                        result.Add(new StyleAndTextTuple("", "\n"));

                        var selectedItem = level + 1 < _selectedMenu.Count
                            ? _selectedMenu[level + 1]
                            : -1;

                        for (var i = 0; i < menu.Children.Count; i++)
                        {
                            var item = menu.Children[i];
                            var idx = i;

                            NotImplementedOrNone MouseHandler(MouseEvent mouseEvent)
                            {
                                if (item.Disabled)
                                    return NotImplementedOrNone.None;

                                var hover = mouseEvent.EventType == MouseEventType.MouseMove;
                                if (mouseEvent.EventType == MouseEventType.MouseUp || hover)
                                {
                                    var app = AppContext.GetApp();
                                    if (!hover && item.Handler != null)
                                    {
                                        app.Layout.FocusLast();
                                        item.Handler();
                                    }
                                    else
                                    {
                                        using (_lock.EnterScope())
                                        {
                                            // Update selected path.
                                            while (_selectedMenu.Count > level + 1)
                                                _selectedMenu.RemoveAt(_selectedMenu.Count - 1);
                                            if (_selectedMenu.Count == level + 1)
                                                _selectedMenu.Add(idx);
                                            else
                                                _selectedMenu[level + 1] = idx;
                                        }
                                    }
                                }
                                return NotImplementedOrNone.None;
                            }

                            string style;
                            if (i == selectedItem)
                            {
                                result.Add(new StyleAndTextTuple("[SetCursorPosition]", ""));
                                style = "class:menu-bar.selected-item";
                            }
                            else
                            {
                                style = "";
                            }

                            result.Add(new StyleAndTextTuple("class:menu", Border.Vertical));

                            if (item.Text == "-")
                            {
                                result.Add(new StyleAndTextTuple(
                                    style + "class:menu-border",
                                    new string(Border.Horizontal[0], menu.Width + 3),
                                    MouseHandler));
                            }
                            else
                            {
                                result.Add(new StyleAndTextTuple(
                                    style,
                                    ($" {item.Text}").PadRight(menu.Width + 3),
                                    MouseHandler));
                            }

                            if (item.Children.Count > 0)
                                result.Add(new StyleAndTextTuple(style, ">", MouseHandler));
                            else
                                result.Add(new StyleAndTextTuple(style, " ", MouseHandler));

                            if (i == selectedItem)
                                result.Add(new StyleAndTextTuple("[SetMenuPosition]", ""));

                            result.Add(new StyleAndTextTuple("class:menu", Border.Vertical));
                            result.Add(new StyleAndTextTuple("", "\n"));
                        }

                        result.Add(new StyleAndTextTuple("class:menu", Border.BottomLeft));
                        result.Add(new StyleAndTextTuple("class:menu",
                            new string(Border.Horizontal[0], menu.Width + 4)));
                        result.Add(new StyleAndTextTuple("class:menu", Border.BottomRight));
                    }
                }

                return result;
            }
        }

        return new Window(
            content: new FormattedTextControl(GetTextFragments),
            style: "class:menu");
    }

    /// <inheritdoc/>
    public IContainer PtContainer() => _container;
}
