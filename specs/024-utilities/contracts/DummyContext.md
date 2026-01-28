# Contract: DummyContext

**Namespace**: `Stroke.Core`
**File**: `src/Stroke/Core/DummyContext.cs`

## API Contract

```csharp
namespace Stroke.Core;

/// <summary>
/// A no-op disposable for optional context manager scenarios.
/// </summary>
/// <remarks>
/// <para>
/// Use this when an API expects an <see cref="IDisposable"/> but no cleanup is actually needed.
/// The <see cref="Dispose"/> method does nothing.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>DummyContext</c> class from <c>utils.py</c>.
/// In Python, this is a context manager for use with <c>with</c> statements.
/// In C#, this is an <see cref="IDisposable"/> for use with <c>using</c> statements.
/// </para>
/// <para>
/// This type is thread-safe (stateless singleton).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Use when a method requires an IDisposable but you don't need cleanup
/// IDisposable context = someCondition ? GetRealContext() : DummyContext.Instance;
/// using (context)
/// {
///     // Do work
/// }
/// </code>
/// </example>
public sealed class DummyContext : IDisposable
{
    /// <summary>
    /// Gets the singleton instance of <see cref="DummyContext"/>.
    /// </summary>
    public static DummyContext Instance { get; }

    /// <summary>
    /// Performs no operation. This method exists only to satisfy the <see cref="IDisposable"/> interface.
    /// </summary>
    public void Dispose();
}
```

## Functional Requirements Coverage

| Requirement | Implementation |
|-------------|----------------|
| FR-025 | `DummyContext` class with `Instance` singleton and no-op `Dispose()` |

## Design Notes

- **Singleton Pattern**: The class uses a private constructor and a static `Instance` property
- **Thread Safety**: Inherently thread-safe because it's stateless
- **No Finalizer**: No destructor needed since there are no resources to release

## Usage Scenarios

1. **Optional Context**: When a method accepts an optional `IDisposable` parameter
2. **Conditional Cleanup**: When cleanup is only needed under certain conditions
3. **API Compatibility**: When implementing an interface that requires `IDisposable`

