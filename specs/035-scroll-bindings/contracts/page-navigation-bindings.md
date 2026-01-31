# Contract: PageNavigationBindings

**Namespace**: `Stroke.KeyBinding.Bindings`
**File**: `src/Stroke/KeyBinding/Bindings/PageNavigationBindings.cs`
**Python Source**: `prompt_toolkit/key_binding/bindings/page_navigation.py`

## Class Signature

```csharp
/// <summary>
/// Key binding loaders for page navigation in Vi and Emacs editing modes.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.bindings.page_navigation</c> module.
/// Provides 3 binding loaders: Emacs, Vi, and combined (merged with buffer-has-focus guard).
/// </para>
/// <para>
/// This type is stateless and inherently thread-safe. Each loader creates and returns a new
/// <see cref="IKeyBindingsBase"/> instance.
/// </para>
/// </remarks>
public static class PageNavigationBindings
```

## Public Methods

### LoadPageNavigationBindings

```csharp
/// <summary>
/// Load both the Vi and Emacs bindings for page navigation, guarded by a
/// buffer-has-focus condition.
/// </summary>
/// <returns>
/// A <see cref="ConditionalKeyBindings"/> wrapping the merged Emacs and Vi bindings,
/// filtered by <see cref="AppFilters.BufferHasFocus"/>.
/// </returns>
public static IKeyBindingsBase LoadPageNavigationBindings()
```

**Implementation structure**:
```csharp
return new ConditionalKeyBindings(
    new MergedKeyBindings(
        LoadEmacsPageNavigationBindings(),
        LoadViPageNavigationBindings()
    ),
    AppFilters.BufferHasFocus
);
```

### LoadEmacsPageNavigationBindings

```csharp
/// <summary>
/// Key bindings for scrolling up and down through pages in Emacs mode.
/// These are separate bindings because GNU readline doesn't have them.
/// </summary>
/// <returns>
/// A <see cref="ConditionalKeyBindings"/> with Emacs page navigation keys,
/// filtered by <see cref="EmacsFilters.EmacsMode"/>.
/// </returns>
public static IKeyBindingsBase LoadEmacsPageNavigationBindings()
```

**Key mappings**:

| Key | Handler |
|-----|---------|
| `Ctrl-V` | `ScrollBindings.ScrollPageDown` |
| `PageDown` | `ScrollBindings.ScrollPageDown` |
| `Escape, V` | `ScrollBindings.ScrollPageUp` |
| `PageUp` | `ScrollBindings.ScrollPageUp` |

### LoadViPageNavigationBindings

```csharp
/// <summary>
/// Key bindings for scrolling up and down through pages in Vi mode.
/// These are separate bindings because GNU readline doesn't have them.
/// </summary>
/// <returns>
/// A <see cref="ConditionalKeyBindings"/> with Vi page navigation keys,
/// filtered by <see cref="ViFilters.ViMode"/>.
/// </returns>
public static IKeyBindingsBase LoadViPageNavigationBindings()
```

**Key mappings**:

| Key | Handler |
|-----|---------|
| `Ctrl-F` | `ScrollBindings.ScrollForward` |
| `Ctrl-B` | `ScrollBindings.ScrollBackward` |
| `Ctrl-D` | `ScrollBindings.ScrollHalfPageDown` |
| `Ctrl-U` | `ScrollBindings.ScrollHalfPageUp` |
| `Ctrl-E` | `ScrollBindings.ScrollOneLineDown` |
| `Ctrl-Y` | `ScrollBindings.ScrollOneLineUp` |
| `PageDown` | `ScrollBindings.ScrollPageDown` |
| `PageUp` | `ScrollBindings.ScrollPageUp` |

## Dependencies

- `Stroke.Application` (AppFilters, EmacsFilters, ViFilters)
- `Stroke.Filters` (IFilter)
- `Stroke.Input` (Keys)
- `Stroke.KeyBinding` (KeyBindings, ConditionalKeyBindings, MergedKeyBindings, IKeyBindingsBase, KeyHandlerCallable, KeyOrChar)
- `Stroke.KeyBinding.Bindings` (ScrollBindings)
