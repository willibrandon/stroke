using Stroke.Core;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;

// Alias to avoid ambiguity with System.Buffer
using Buffer = Stroke.Core.Buffer;

namespace Stroke.Layout;

/// <summary>
/// The layout wrapper for a Stroke Application. Manages the root container hierarchy,
/// tracks focus, maintains parent-child relationships, and provides search linking.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>Layout</c> class from
/// <c>prompt_toolkit.layout.layout</c>.
/// </para>
/// <para>
/// This class is thread-safe. Focus changes, search links, and parent-child mapping
/// updates are synchronized via Lock.
/// </para>
/// </remarks>
public sealed class Layout
{
    private readonly Lock _lock = new();
    private readonly List<Window> _stack = [];
    private readonly Dictionary<SearchBufferControl, BufferControl> _searchLinks = new();
    private Dictionary<IContainer, IContainer> _childToParent = new();
    private readonly List<Window> _visibleWindows = [];

    /// <summary>
    /// Create a new Layout with the given root container.
    /// </summary>
    /// <param name="container">The root container for the layout.</param>
    /// <param name="focusedElement">Element to focus initially. Defaults to first window.</param>
    /// <exception cref="InvalidLayoutException">Layout contains no Window objects.</exception>
    public Layout(AnyContainer container, FocusableElement? focusedElement = null)
    {
        Container = container.ToContainer();

        if (focusedElement is null)
        {
            // Focus first window
            var firstWindow = FindAllWindows().FirstOrDefault();
            if (firstWindow is null)
            {
                throw new InvalidLayoutException(
                    "Invalid layout. The layout does not contain any Window object.");
            }
            _stack.Add(firstWindow);
        }
        else
        {
            Focus(focusedElement.Value);
        }
    }

    /// <summary>The root container.</summary>
    public IContainer Container { get; }

    /// <summary>
    /// Map from SearchBufferControl to the original BufferControl it searches.
    /// Thread-safe: access is synchronized via Lock.
    /// </summary>
    public Dictionary<SearchBufferControl, BufferControl> SearchLinks
    {
        get
        {
            using (_lock.EnterScope())
            {
                return new Dictionary<SearchBufferControl, BufferControl>(_searchLinks);
            }
        }
    }

    /// <summary>
    /// List of currently visible windows. Updated by the Renderer during each render cycle.
    /// Thread-safe: access is synchronized via Lock.
    /// </summary>
    public List<Window> VisibleWindows
    {
        get
        {
            using (_lock.EnterScope())
            {
                return new List<Window>(_visibleWindows);
            }
        }
    }

    /// <summary>
    /// The currently focused Window. The getter returns the top of the focus stack.
    /// The setter pushes the window to the top of the focus stack.
    /// </summary>
    public Window CurrentWindow
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _stack[^1];
            }
        }
        set
        {
            using (_lock.EnterScope())
            {
                _stack.Add(value);
            }
        }
    }

    /// <summary>
    /// Return the BufferControl that is the target of the current search,
    /// or null if not currently searching.
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>search_target_buffer_control</c> property
    /// from <c>prompt_toolkit.layout.layout</c>.
    /// </remarks>
    public BufferControl? SearchTargetBufferControl
    {
        get
        {
            using (_lock.EnterScope())
            {
                var control = _stack[^1].Content;
                if (control is SearchBufferControl sbc &&
                    _searchLinks.TryGetValue(sbc, out var bc))
                {
                    return bc;
                }
                return null;
            }
        }
    }

    /// <summary>The UIControl of the currently focused Window.</summary>
    public IUIControl CurrentControl
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _stack[^1].Content;
            }
        }
    }

    /// <summary>
    /// The currently focused Buffer, or null if the focused control is not a BufferControl.
    /// </summary>
    public Buffer? CurrentBuffer
    {
        get
        {
            var control = CurrentControl;
            return control is BufferControl bc ? bc.Buffer : null;
        }
    }

    /// <summary>
    /// The currently focused BufferControl, or null if focus is not on a BufferControl.
    /// </summary>
    public BufferControl? CurrentBufferControl
    {
        get
        {
            var control = CurrentControl;
            return control as BufferControl;
        }
    }

    /// <summary>Whether the current focused control has a search focus link.</summary>
    public bool IsSearching
    {
        get
        {
            using (_lock.EnterScope())
            {
                var control = _stack[^1].Content;
                return control is SearchBufferControl sbc && _searchLinks.ContainsKey(sbc);
            }
        }
    }

    /// <summary>
    /// The SearchBufferControl that is linked to the current BufferControl, or null.
    /// </summary>
    public SearchBufferControl? CurrentSearchBufferControl
    {
        get
        {
            using (_lock.EnterScope())
            {
                var control = _stack[^1].Content;
                if (control is SearchBufferControl sbc && _searchLinks.ContainsKey(sbc))
                {
                    return sbc;
                }
                return null;
            }
        }
    }

    /// <summary>
    /// The previously focused UIControl.
    /// </summary>
    public IUIControl PreviousControl
    {
        get
        {
            using (_lock.EnterScope())
            {
                if (_stack.Count >= 2)
                    return _stack[^2].Content;
                return _stack[^1].Content;
            }
        }
    }

    /// <summary>
    /// Whether the currently focused control is a BufferControl.
    /// </summary>
    public bool BufferHasFocus => CurrentControl is BufferControl;

    /// <summary>
    /// Focus the given element.
    /// </summary>
    /// <param name="value">The element to focus.</param>
    /// <exception cref="InvalidOperationException">Element is not focusable or not found.</exception>
    public void Focus(FocusableElement value)
    {
        using (_lock.EnterScope())
        {
            FocusInternal(value);
        }
    }

    private void FocusInternal(FocusableElement value)
    {
        // Focus by buffer name
        if (value.AsBufferName is { } bufferName)
        {
            foreach (var control in FindAllControlsInternal())
            {
                if (control is BufferControl bc && bc.Buffer.Name == bufferName)
                {
                    FocusInternal(new FocusableElement(control));
                    return;
                }
            }
            throw new InvalidOperationException(
                $"Couldn't find Buffer in the current layout: '{bufferName}'.");
        }

        // Focus by Buffer object
        if (value.AsBuffer is { } buffer)
        {
            foreach (var control in FindAllControlsInternal())
            {
                if (control is BufferControl bc && bc.Buffer == buffer)
                {
                    FocusInternal(new FocusableElement(control));
                    return;
                }
            }
            throw new InvalidOperationException(
                $"Couldn't find Buffer in the current layout: '{buffer}'.");
        }

        // Focus UIControl
        if (value.AsUIControl is { } uiControl)
        {
            if (!FindAllControlsInternal().Contains(uiControl))
            {
                throw new InvalidOperationException(
                    "Invalid value. Control does not appear in the layout.");
            }
            if (!uiControl.IsFocusable)
            {
                throw new InvalidOperationException(
                    "Invalid value. UIControl is not focusable.");
            }

            // Find window containing this control
            foreach (var window in FindAllWindowsInternal())
            {
                if (window.Content == uiControl)
                {
                    _stack.Add(window);
                    return;
                }
            }
            throw new InvalidOperationException("Control not found in the user interface.");
        }

        // Focus Window directly
        if (value.AsWindow is { } window2)
        {
            if (!FindAllWindowsInternal().Contains(window2))
            {
                throw new InvalidOperationException(
                    $"Invalid value. Window does not appear in the layout: {window2}");
            }
            _stack.Add(window2);
            return;
        }

        // Focus AnyContainer
        if (value.AsContainer is { } anyContainer)
        {
            var container = anyContainer.ToContainer();

            if (container is Window w)
            {
                if (!FindAllWindowsInternal().Contains(w))
                {
                    throw new InvalidOperationException(
                        $"Invalid value. Window does not appear in the layout: {w}");
                }
                _stack.Add(w);
                return;
            }

            // Focus a window in this container, preferring previously focused ones
            var windows = new List<Window>();
            foreach (var c in LayoutUtils.Walk(container, skipHidden: true))
            {
                if (c is Window win && win.Content.IsFocusable)
                {
                    windows.Add(win);
                }
            }

            // Take the first one that was focused before
            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                if (windows.Contains(_stack[i]))
                {
                    _stack.Add(_stack[i]);
                    return;
                }
            }

            // None was focused before: take the very first focusable window
            if (windows.Count > 0)
            {
                _stack.Add(windows[0]);
                return;
            }

            throw new InvalidOperationException(
                $"Invalid value. Container cannot be focused: {container}");
        }

        throw new InvalidOperationException("FocusableElement has no value.");
    }

    /// <summary>Focus the previous visible/focusable Window.</summary>
    public void FocusPrevious()
    {
        using (_lock.EnterScope())
        {
            var windows = GetVisibleFocusableWindowsInternal();
            if (windows.Count > 0)
            {
                var currentWin = _stack[^1];
                var index = windows.IndexOf(currentWin);
                if (index < 0) index = 0;
                else index = ((index - 1) % windows.Count + windows.Count) % windows.Count;
                FocusInternal(new FocusableElement(windows[index]));
            }
        }
    }

    /// <summary>Focus the next visible/focusable Window.</summary>
    public void FocusNext()
    {
        using (_lock.EnterScope())
        {
            var windows = GetVisibleFocusableWindowsInternal();
            if (windows.Count > 0)
            {
                var currentWin = _stack[^1];
                var index = windows.IndexOf(currentWin);
                if (index < 0) index = 0;
                else index = (index + 1) % windows.Count;
                FocusInternal(new FocusableElement(windows[index]));
            }
        }
    }

    /// <summary>Focus the last focused Window.</summary>
    public void FocusLast()
    {
        using (_lock.EnterScope())
        {
            if (_stack.Count > 1)
            {
                _stack.RemoveAt(_stack.Count - 1);
            }
        }
    }

    /// <summary>Whether the given Window has focus.</summary>
    public bool HasFocus(Window window)
    {
        using (_lock.EnterScope())
        {
            return _stack.Count > 0 && _stack[^1] == window;
        }
    }

    /// <summary>Whether the given element has focus.</summary>
    public bool HasFocus(FocusableElement value)
    {
        using (_lock.EnterScope())
        {
            if (value.AsBufferName is { } bufferName)
            {
                var cb = _stack[^1].Content is BufferControl bc2 ? bc2.Buffer : null;
                return cb is not null && cb.Name == bufferName;
            }
            if (value.AsBuffer is { } buffer)
            {
                var cb = _stack[^1].Content is BufferControl bc2 ? bc2.Buffer : null;
                return cb == buffer;
            }
            if (value.AsUIControl is { } uiControl)
            {
                return _stack[^1].Content == uiControl;
            }
            if (value.AsWindow is { } window)
            {
                return _stack[^1] == window;
            }
            if (value.AsContainer is { } anyContainer)
            {
                var container = anyContainer.ToContainer();
                if (container is Window w)
                {
                    return _stack[^1] == w;
                }
                // Check if any element inside is focused
                foreach (var element in LayoutUtils.Walk(container))
                {
                    if (element == _stack[^1])
                        return true;
                }
                return false;
            }
            return false;
        }
    }

    /// <summary>Find all Windows in the layout by walking the container tree.</summary>
    public IEnumerable<Window> FindAllWindows()
    {
        return FindAllWindowsInternal();
    }

    private List<Window> FindAllWindowsInternal()
    {
        var result = new List<Window>();
        foreach (var item in LayoutUtils.Walk(Container))
        {
            if (item is Window w)
                result.Add(w);
        }
        return result;
    }

    /// <summary>Find all UIControls in the layout.</summary>
    public IEnumerable<IUIControl> FindAllControls()
    {
        return FindAllControlsInternal();
    }

    private List<IUIControl> FindAllControlsInternal()
    {
        var result = new List<IUIControl>();
        foreach (var w in FindAllWindowsInternal())
        {
            result.Add(w.Content);
        }
        return result;
    }

    /// <summary>Walk the entire container tree in depth-first order.</summary>
    public IEnumerable<IContainer> Walk()
    {
        return LayoutUtils.Walk(Container);
    }

    /// <summary>
    /// Walk through all containers in the current 'modal' part of the layout.
    /// </summary>
    public IEnumerable<IContainer> WalkThroughModalArea()
    {
        using (_lock.EnterScope())
        {
            IContainer root = _stack[^1];
            while (!root.IsModal && _childToParent.TryGetValue(root, out var parent))
            {
                root = parent;
            }
            return LayoutUtils.Walk(root).ToList();
        }
    }

    /// <summary>
    /// Return all focusable windows in the modal area.
    /// </summary>
    public IEnumerable<Window> GetFocusableWindows()
    {
        foreach (var w in WalkThroughModalArea())
        {
            if (w is Window win && win.Content.IsFocusable)
                yield return win;
        }
    }

    /// <summary>
    /// Return visible focusable windows, preserving modal area ordering.
    /// </summary>
    public List<Window> GetVisibleFocusableWindows()
    {
        using (_lock.EnterScope())
        {
            return GetVisibleFocusableWindowsInternal();
        }
    }

    private List<Window> GetVisibleFocusableWindowsInternal()
    {
        var focusable = new HashSet<Window>();
        foreach (var w in WalkThroughModalAreaInternal())
        {
            if (w is Window win && win.Content.IsFocusable)
                focusable.Add(win);
        }
        var result = new List<Window>();
        foreach (var w in _visibleWindows)
        {
            if (focusable.Contains(w))
                result.Add(w);
        }
        return result;
    }

    private List<IContainer> WalkThroughModalAreaInternal()
    {
        IContainer root = _stack[^1];
        while (!root.IsModal && _childToParent.TryGetValue(root, out var parent))
        {
            root = parent;
        }
        return LayoutUtils.Walk(root).ToList();
    }

    /// <summary>Get the parent container of the given child, or null if root.</summary>
    public IContainer? GetParent(IContainer child)
    {
        using (_lock.EnterScope())
        {
            return _childToParent.TryGetValue(child, out var parent) ? parent : null;
        }
    }

    /// <summary>
    /// Look in the layout for a buffer with the given name.
    /// </summary>
    public Buffer? GetBufferByName(string bufferName)
    {
        foreach (var w in LayoutUtils.Walk(Container))
        {
            if (w is Window win && win.Content is BufferControl bc && bc.Buffer.Name == bufferName)
            {
                return bc.Buffer;
            }
        }
        return null;
    }

    /// <summary>
    /// Update parent-child relationships by walking the container tree.
    /// Called after each render cycle.
    /// </summary>
    public void UpdateParentsRelations()
    {
        var parents = new Dictionary<IContainer, IContainer>();

        void WalkTree(IContainer e)
        {
            foreach (var c in e.GetChildren())
            {
                parents[c] = e;
                WalkTree(c);
            }
        }

        WalkTree(Container);

        using (_lock.EnterScope())
        {
            _childToParent = parents;
        }
    }

    /// <summary>
    /// Set the visible windows list. Called by the Renderer during rendering.
    /// </summary>
    internal void SetVisibleWindows(List<Window> windows)
    {
        using (_lock.EnterScope())
        {
            _visibleWindows.Clear();
            _visibleWindows.AddRange(windows);
        }
    }

    /// <summary>
    /// Add a search link.
    /// </summary>
    internal void AddSearchLink(SearchBufferControl searchControl, BufferControl targetControl)
    {
        using (_lock.EnterScope())
        {
            _searchLinks[searchControl] = targetControl;
        }
    }

    /// <summary>
    /// Remove a search link.
    /// </summary>
    internal void RemoveSearchLink(SearchBufferControl searchControl)
    {
        using (_lock.EnterScope())
        {
            _searchLinks.Remove(searchControl);
        }
    }

    /// <summary>
    /// Reset layout state. Resets all containers in the tree.
    /// </summary>
    public void Reset()
    {
        using (_lock.EnterScope())
        {
            _searchLinks.Clear();
        }
        Container.Reset();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        using (_lock.EnterScope())
        {
            return $"Layout({Container}, current_window={_stack[^1]})";
        }
    }
}
