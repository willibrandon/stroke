namespace Stroke.Layout;

/// <summary>
/// Marker interface for window types used as dictionary keys in Screen.
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
