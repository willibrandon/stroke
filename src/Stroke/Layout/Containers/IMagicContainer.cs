namespace Stroke.Layout.Containers;

/// <summary>
/// Interface for objects that can be converted to containers.
/// </summary>
/// <remarks>
/// <para>
/// This is the C# equivalent of Python Prompt Toolkit's <c>__pt_container__()</c> protocol,
/// which allows any object to be convertible to a container for layout purposes.
/// </para>
/// <para>
/// Objects implementing this interface can be used anywhere an <see cref="IContainer"/>
/// is expected via the <see cref="AnyContainer"/> union type.
/// </para>
/// </remarks>
public interface IMagicContainer
{
    /// <summary>
    /// Returns the container representation of this object.
    /// </summary>
    /// <returns>The underlying container.</returns>
    IContainer PtContainer();
}
