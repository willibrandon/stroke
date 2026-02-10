using Stroke.Core;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;

namespace Stroke.Application;

/// <summary>
/// Internal key bindings aggregator for an Application. Merges key bindings from
/// the focused control hierarchy, global-only bindings, application bindings,
/// page navigation bindings, and default bindings.
/// </summary>
/// <remarks>
/// <para>
/// This class caches merged bindings keyed by (current_window, controls_set) to avoid
/// recomputation on every key press. Not exposed publicly.
/// </para>
/// <para>
/// <b>Visibility rationale:</b> Internal because this is an implementation detail of
/// Application. Users configure key bindings via Application.KeyBindings and per-control
/// bindings. The merge algorithm is not part of the public API contract, allowing it to
/// evolve without breaking changes.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>_CombinedRegistry</c> class from
/// <c>prompt_toolkit.application.application</c>.
/// </para>
/// </remarks>
internal sealed class CombinedRegistry : IKeyBindingsBase
{
    private readonly IApplication _app;
    private readonly SimpleCache<CacheKey, IKeyBindingsBase> _cache = new();

    internal CombinedRegistry(IApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        _app = app;
    }

    /// <summary>Not implemented — this object is not wrapped in another KeyBindings.</summary>
    public object Version => throw new NotImplementedException(
        "CombinedRegistry is not wrapped in another KeyBindings.");

    /// <summary>Not implemented — this object is not wrapped in another KeyBindings.</summary>
    public IReadOnlyList<Binding> Bindings => throw new NotImplementedException(
        "CombinedRegistry is not wrapped in another KeyBindings.");

    /// <summary>Get bindings matching the exact key sequence.</summary>
    public IReadOnlyList<Binding> GetBindingsForKeys(IReadOnlyList<KeyOrChar> keys)
    {
        return GetKeyBindings().GetBindingsForKeys(keys);
    }

    /// <summary>Get bindings that start with the given key sequence prefix.</summary>
    public IReadOnlyList<Binding> GetBindingsStartingWithKeys(IReadOnlyList<KeyOrChar> keys)
    {
        return GetKeyBindings().GetBindingsStartingWithKeys(keys);
    }

    private IKeyBindingsBase GetKeyBindings()
    {
        var currentWindow = _app.Layout.CurrentWindow;
        var otherControls = _app.Layout.FindAllControls().ToList();
        var key = new CacheKey(currentWindow, otherControls);

        return _cache.Get(key, () => CreateKeyBindings(currentWindow, otherControls));
    }

    /// <summary>
    /// Create a merged KeyBindings object. Priority order (after reversal):
    /// 1. Focused control's key bindings (highest priority)
    /// 2. Parent container bindings (up to first modal)
    /// 3. Global-only bindings (from containers NOT in focused hierarchy)
    /// 4. Application key bindings
    /// 5. Page navigation bindings (conditional)
    /// 6. Default bindings (lowest priority)
    /// </summary>
    private IKeyBindingsBase CreateKeyBindings(
        Window currentWindow, List<IUIControl> otherControls)
    {
        var keyBindings = new List<IKeyBindingsBase>();
        var collectedContainers = new HashSet<IContainer>();

        // Collect key bindings from currently focused control and all parent
        // containers. Don't include key bindings of container parent controls.
        IContainer container = currentWindow;
        while (true)
        {
            collectedContainers.Add(container);
            var kb = container.GetKeyBindings();
            if (kb is not null)
            {
                keyBindings.Add(kb);
            }

            if (container.IsModal)
                break;

            var parent = _app.Layout.GetParent(container);
            if (parent is null)
                break;

            container = parent;
        }

        // Include global bindings (starting at the top-modal container)
        foreach (var c in LayoutUtils.Walk(container))
        {
            if (!collectedContainers.Contains(c))
            {
                var kb = c.GetKeyBindings();
                if (kb is not null)
                {
                    keyBindings.Add(new GlobalOnlyKeyBindings(kb));
                }
            }
        }

        // Add App key bindings
        if (_app.KeyBindings is not null)
        {
            keyBindings.Add(_app.KeyBindings);
        }

        // Add page navigation bindings
        keyBindings.Add(
            new ConditionalKeyBindings(
                _app.PageNavigationBindings,
                _app.EnablePageNavigationBindings));

        // Add default bindings
        keyBindings.Add(_app.DefaultBindings);

        // Reverse this list. The current control's key bindings should come
        // last. They need priority.
        keyBindings.Reverse();

        return new MergedKeyBindings(keyBindings);
    }

    /// <summary>
    /// Cache key that combines the current window and the set of controls.
    /// </summary>
    private sealed class CacheKey : IEquatable<CacheKey>
    {
        private readonly Window _window;
        private readonly HashSet<IUIControl> _controls;
        private readonly int _hashCode;

        public CacheKey(Window window, List<IUIControl> controls)
        {
            _window = window;
            _controls = [.. controls];

            // Pre-compute hash
            var hash = new HashCode();
            hash.Add(_window);
            foreach (var c in _controls.OrderBy(c => c.GetHashCode()))
            {
                hash.Add(c);
            }
            _hashCode = hash.ToHashCode();
        }

        public bool Equals(CacheKey? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return _window == other._window && _controls.SetEquals(other._controls);
        }

        public override bool Equals(object? obj) => Equals(obj as CacheKey);
        public override int GetHashCode() => _hashCode;
    }
}
