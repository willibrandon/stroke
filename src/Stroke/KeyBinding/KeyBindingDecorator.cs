using Stroke.Filters;
using Stroke.Input;

namespace Stroke.KeyBinding;

/// <summary>
/// Factory for creating Binding objects with pre-configured settings.
/// Equivalent to Python's @key_binding decorator.
/// </summary>
/// <remarks>
/// <para>
/// Use this class to pre-configure binding settings that can then be
/// applied to multiple handlers. The resulting Binding can be added to
/// a KeyBindings registry which will override the placeholder keys.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>key_binding</c> function from <c>key_bindings.py</c>.
/// </para>
/// </remarks>
public static class KeyBindingDecorator
{
    // Placeholder key used in bindings created by this decorator.
    // This is replaced when the binding is added to a KeyBindings registry.
    private static readonly KeyOrChar[] PlaceholderKeys = [Keys.Any];

    /// <summary>
    /// Creates a decorator function that turns a handler into a Binding.
    /// </summary>
    /// <param name="filter">Activation filter (default: true).</param>
    /// <param name="eager">Eager matching filter (default: false).</param>
    /// <param name="isGlobal">Global binding filter (default: false).</param>
    /// <param name="saveBefore">Save-before callback (default: always returns true).</param>
    /// <param name="recordInMacro">Macro recording filter (default: true).</param>
    /// <returns>
    /// A function that takes a handler and returns a Binding with placeholder keys.
    /// The binding should then be added to a KeyBindings registry via
    /// <see cref="KeyBindings.Add{T}"/> which will provide the actual keys.
    /// </returns>
    /// <example>
    /// <code>
    /// var decorateAsGlobal = KeyBindingDecorator.Create(isGlobal: true);
    /// var binding = decorateAsGlobal(HandleQuit);
    /// kb.Add(new[] { (KeyOrChar)Keys.ControlQ })(binding);
    /// </code>
    /// </example>
    public static Func<KeyHandlerCallable, Binding> Create(
        FilterOrBool filter = default,
        FilterOrBool eager = default,
        FilterOrBool isGlobal = default,
        Func<KeyPressEvent, bool>? saveBefore = null,
        FilterOrBool recordInMacro = default)
    {
        return handler => new Binding(
            PlaceholderKeys,
            handler,
            filter,
            eager,
            isGlobal,
            saveBefore,
            recordInMacro);
    }
}
