# API Contract: IWindow

**Namespace**: `Stroke.Layout`
**Type**: `interface`
**Thread Safety**: N/A (marker interface)

## Interface Definition

```csharp
namespace Stroke.Layout;

/// <summary>
/// Marker interface for window types used as dictionary keys in <see cref="Screen"/>.
/// </summary>
/// <remarks>
/// <para>
/// This interface serves as a forward reference for the Window class that will be
/// implemented in a future feature. It allows Screen to have type-safe dictionary
/// keys for cursor positions, menu positions, and write positions.
/// </para>
/// <para>
/// Implementations must provide proper equality semantics (implementing
/// <see cref="IEquatable{T}"/> or overriding <see cref="object.Equals(object)"/>
/// and <see cref="object.GetHashCode"/>) to work correctly as dictionary keys.
/// </para>
/// <para>
/// In Python Prompt Toolkit, Window is used directly as a dictionary key without
/// an interface. This interface is a C# adaptation to enable forward references
/// within the same namespace while maintaining type safety.
/// </para>
/// </remarks>
public interface IWindow
{
    // Marker interface - no members required.
    // Implementations should have proper equality semantics for dictionary key usage.
}
```

## Design Rationale

### Why a Marker Interface?

1. **Forward Reference**: Window class doesn't exist yet but Screen needs a key type now
2. **Type Safety**: Using `object` would lose compile-time type checking
3. **Documentation**: Interface documents the contract (equality semantics required)
4. **Extensibility**: Future Window class simply implements IWindow

### Why Not Generic Type Parameter?

Python Prompt Toolkit's Screen is not generic:
```python
self.cursor_positions: dict[Window, Point] = {}
```

Making Screen generic (`Screen<TWindow>`) would:
- Complicate the API unnecessarily
- Deviate from Python API structure
- Make interoperability harder

### Equality Requirements

Implementations MUST:
- Override `Equals(object)` with value or reference semantics as appropriate
- Override `GetHashCode()` consistently with Equals
- Optionally implement `IEquatable<T>` for performance

```csharp
// Example implementation (future Window class)
public sealed class Window : IWindow, IEquatable<Window>
{
    private readonly Guid _id = Guid.NewGuid();

    public bool Equals(Window? other) => other is not null && _id == other._id;
    public override bool Equals(object? obj) => obj is Window w && Equals(w);
    public override int GetHashCode() => _id.GetHashCode();
}
```

## Usage in Screen

```csharp
// Screen internally uses IWindow as key type
private readonly Dictionary<IWindow, Point> _cursorPositions = new();
private readonly Dictionary<IWindow, Point> _menuPositions = new();
private readonly Dictionary<IWindow, WritePosition> _visibleWindowsToWritePositions = new();

// Public methods accept IWindow
public void SetCursorPosition(IWindow window, Point position)
{
    using (_lock.EnterScope())
    {
        _cursorPositions[window] = position;
    }
}
```

## Testing Note

For unit tests, create a simple test implementation:

```csharp
internal sealed class TestWindow : IWindow, IEquatable<TestWindow>
{
    public string Name { get; }

    public TestWindow(string name) => Name = name;

    public bool Equals(TestWindow? other) => other?.Name == Name;
    public override bool Equals(object? obj) => obj is TestWindow w && Equals(w);
    public override int GetHashCode() => Name.GetHashCode();
}
```
