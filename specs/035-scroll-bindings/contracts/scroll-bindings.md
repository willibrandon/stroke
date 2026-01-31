# Contract: ScrollBindings

**Namespace**: `Stroke.KeyBinding.Bindings`
**File**: `src/Stroke/KeyBinding/Bindings/ScrollBindings.cs`
**Python Source**: `prompt_toolkit/key_binding/bindings/scroll.py`

## Class Signature

```csharp
/// <summary>
/// Static scroll functions for navigating through long multiline buffers.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.bindings.scroll</c> module.
/// Provides 8 scroll functions: forward, backward, half-page down/up, one-line down/up,
/// page down/up.
/// </para>
/// <para>
/// This type is stateless and inherently thread-safe. All mutable state is accessed through
/// <see cref="Stroke.Layout.Containers.Window.VerticalScroll"/> and
/// <see cref="Stroke.Core.Buffer.CursorPosition"/>, which handle their own synchronization.
/// </para>
/// </remarks>
public static class ScrollBindings
```

## Public Methods

### ScrollForward

```csharp
/// <summary>
/// Scroll window down by one full window height.
/// Moves the cursor down by the number of logical lines that fill the window height,
/// accounting for variable line heights from wrapped content.
/// </summary>
/// <param name="event">The key press event.</param>
/// <returns><c>null</c> on success.</returns>
public static NotImplementedOrNone? ScrollForward(KeyPressEvent @event)
```

### ScrollBackward

```csharp
/// <summary>
/// Scroll window up by one full window height.
/// Moves the cursor up by the number of logical lines that fill the window height,
/// accounting for variable line heights from wrapped content.
/// </summary>
/// <param name="event">The key press event.</param>
/// <returns><c>null</c> on success.</returns>
public static NotImplementedOrNone? ScrollBackward(KeyPressEvent @event)
```

### ScrollHalfPageDown

```csharp
/// <summary>
/// Scroll window down by half a page. Same as <see cref="ScrollForward"/> but scrolls
/// only half the window height.
/// </summary>
/// <param name="event">The key press event.</param>
/// <returns><c>null</c> on success.</returns>
public static NotImplementedOrNone? ScrollHalfPageDown(KeyPressEvent @event)
```

### ScrollHalfPageUp

```csharp
/// <summary>
/// Scroll window up by half a page. Same as <see cref="ScrollBackward"/> but scrolls
/// only half the window height.
/// </summary>
/// <param name="event">The key press event.</param>
/// <returns><c>null</c> on success.</returns>
public static NotImplementedOrNone? ScrollHalfPageUp(KeyPressEvent @event)
```

### ScrollOneLineDown

```csharp
/// <summary>
/// Scroll the viewport down by one line. Adjusts the cursor position only when necessary
/// to keep it within the visible area (when cursor is at the top scroll offset boundary).
/// </summary>
/// <param name="event">The key press event.</param>
/// <returns><c>null</c> on success.</returns>
public static NotImplementedOrNone? ScrollOneLineDown(KeyPressEvent @event)
```

### ScrollOneLineUp

```csharp
/// <summary>
/// Scroll the viewport up by one line. Adjusts the cursor position only when necessary
/// to keep it within the visible area (when cursor would fall below the visible region).
/// </summary>
/// <param name="event">The key press event.</param>
/// <returns><c>null</c> on success.</returns>
public static NotImplementedOrNone? ScrollOneLineUp(KeyPressEvent @event)
```

### ScrollPageDown

```csharp
/// <summary>
/// Scroll page down. Sets the vertical scroll offset to the last visible line index
/// and positions the cursor at the beginning of the newly visible content
/// (first non-whitespace character).
/// </summary>
/// <param name="event">The key press event.</param>
/// <returns><c>null</c> on success.</returns>
public static NotImplementedOrNone? ScrollPageDown(KeyPressEvent @event)
```

### ScrollPageUp

```csharp
/// <summary>
/// Scroll page up. Positions the cursor at the first visible line (ensuring at least
/// one line of movement) and resets the vertical scroll offset to 0.
/// </summary>
/// <param name="event">The key press event.</param>
/// <returns><c>null</c> on success.</returns>
public static NotImplementedOrNone? ScrollPageUp(KeyPressEvent @event)
```

## Internal Methods

### ScrollForwardInternal

```csharp
/// <summary>
/// Internal implementation for scroll forward with configurable half-page option.
/// </summary>
/// <param name="event">The key press event.</param>
/// <param name="half">If true, scroll only half a page.</param>
private static void ScrollForwardInternal(KeyPressEvent @event, bool half)
```

### ScrollBackwardInternal

```csharp
/// <summary>
/// Internal implementation for scroll backward with configurable half-page option.
/// </summary>
/// <param name="event">The key press event.</param>
/// <param name="half">If true, scroll only half a page.</param>
private static void ScrollBackwardInternal(KeyPressEvent @event, bool half)
```

## Dependencies

- `Stroke.Application` (KeyPressEventExtensions.GetApp)
- `Stroke.Core` (Buffer, Document)
- `Stroke.Layout.Containers` (Window)
- `Stroke.Layout.Windows` (WindowRenderInfo)
- `Stroke.KeyBinding` (KeyPressEvent, KeyHandlerCallable, NotImplementedOrNone)
