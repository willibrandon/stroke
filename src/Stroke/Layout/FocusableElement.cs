using Stroke.Core;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;

// Alias to avoid ambiguity with System.Buffer
using Buffer = Stroke.Core.Buffer;

namespace Stroke.Layout;

/// <summary>
/// Union type representing elements that can receive focus.
/// Accepts a <see cref="Window"/>, <see cref="IUIControl"/>, <see cref="Buffer"/>,
/// buffer name (<see cref="string"/>), or <see cref="AnyContainer"/>.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's focus parameter handling from
/// <c>prompt_toolkit.layout.layout</c>. The Python version accepts any of:
/// <c>UIControl</c>, <c>Buffer</c>, <c>str</c> (buffer name), or
/// a container with <c>__pt_container__</c>.
/// </para>
/// </remarks>
public readonly struct FocusableElement : IEquatable<FocusableElement>
{
    private readonly object? _value;

    /// <summary>
    /// Creates a FocusableElement from a Window.
    /// </summary>
    public FocusableElement(Window window)
    {
        _value = window ?? throw new ArgumentNullException(nameof(window));
    }

    /// <summary>
    /// Creates a FocusableElement from a UIControl.
    /// </summary>
    public FocusableElement(IUIControl control)
    {
        _value = control ?? throw new ArgumentNullException(nameof(control));
    }

    /// <summary>
    /// Creates a FocusableElement from a Buffer.
    /// </summary>
    public FocusableElement(Buffer buffer)
    {
        _value = buffer ?? throw new ArgumentNullException(nameof(buffer));
    }

    /// <summary>
    /// Creates a FocusableElement from a buffer name.
    /// </summary>
    public FocusableElement(string bufferName)
    {
        _value = bufferName ?? throw new ArgumentNullException(nameof(bufferName));
    }

    /// <summary>
    /// Creates a FocusableElement from an AnyContainer.
    /// </summary>
    public FocusableElement(AnyContainer container)
    {
        if (!container.HasValue)
            throw new ArgumentException("AnyContainer must have a value.", nameof(container));
        _value = container;
    }

    /// <summary>Whether this instance holds a value.</summary>
    public bool HasValue => _value is not null;

    /// <summary>Implicit conversion from Window.</summary>
    public static implicit operator FocusableElement(Window window) => new(window);

    // Note: C# does not allow implicit conversions from interfaces.
    // Use the constructor new FocusableElement(IUIControl) instead.

    /// <summary>Implicit conversion from Buffer.</summary>
    public static implicit operator FocusableElement(Buffer buffer) => new(buffer);

    /// <summary>Implicit conversion from string (buffer name).</summary>
    public static implicit operator FocusableElement(string bufferName) => new(bufferName);

    /// <summary>Implicit conversion from AnyContainer.</summary>
    public static implicit operator FocusableElement(AnyContainer container) => new(container);

    /// <summary>
    /// Gets the underlying value.
    /// </summary>
    internal object Value => _value ?? throw new InvalidOperationException("FocusableElement has no value.");

    /// <summary>
    /// Gets the value as a Window, or null if not a Window.
    /// </summary>
    public Window? AsWindow => _value as Window;

    /// <summary>
    /// Gets the value as a UIControl, or null if not a UIControl.
    /// </summary>
    public IUIControl? AsUIControl => _value as IUIControl;

    /// <summary>
    /// Gets the value as a Buffer, or null if not a Buffer.
    /// </summary>
    public Buffer? AsBuffer => _value as Buffer;

    /// <summary>
    /// Gets the value as a buffer name (string), or null if not a string.
    /// </summary>
    public string? AsBufferName => _value as string;

    /// <summary>
    /// Gets the value as an AnyContainer, or null if not an AnyContainer.
    /// </summary>
    public AnyContainer? AsContainer => _value is AnyContainer c ? c : null;

    /// <inheritdoc/>
    public bool Equals(FocusableElement other) => Equals(_value, other._value);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is FocusableElement other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => _value?.GetHashCode() ?? 0;

    /// <summary>Equality operator.</summary>
    public static bool operator ==(FocusableElement left, FocusableElement right) => left.Equals(right);

    /// <summary>Inequality operator.</summary>
    public static bool operator !=(FocusableElement left, FocusableElement right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString() => _value?.ToString() ?? "FocusableElement(empty)";
}
