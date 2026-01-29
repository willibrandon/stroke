# API Contract: Char

**Namespace**: `Stroke.Layout`
**Type**: `sealed class`
**Thread Safety**: Immutable (inherently thread-safe)

## Class Definition

```csharp
namespace Stroke.Layout;

/// <summary>
/// Represents a single character in a <see cref="Screen"/>.
/// </summary>
/// <remarks>
/// <para>
/// This type is immutable and uses value equality semantics based on
/// <see cref="Character"/> and <see cref="Style"/> properties.
/// </para>
/// <para>
/// Control characters (0x00-0x1F, 0x7F) are automatically transformed to
/// caret notation (e.g., ^A) with "class:control-character" style.
/// High-byte characters (0x80-0x9F) are transformed to hex notation (e.g., "&lt;80&gt;").
/// Non-breaking space (0xA0) displays as space with "class:nbsp" style.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>Char</c> class from <c>layout/screen.py</c>.
/// </para>
/// </remarks>
public sealed class Char : IEquatable<Char>
```

## Constants

```csharp
/// <summary>
/// Style string indicating a transparent (default) character.
/// </summary>
/// <remarks>
/// Equivalent to Python Prompt Toolkit's <c>Transparent</c> constant.
/// </remarks>
public const string Transparent = "[Transparent]";
```

## Properties

```csharp
/// <summary>
/// Gets the displayed character string.
/// </summary>
/// <remarks>
/// May be multiple characters for caret notation (e.g., "^A" for Ctrl+A)
/// or hex notation (e.g., "&lt;80&gt;").
/// </remarks>
public string Character { get; }

/// <summary>
/// Gets the style string containing CSS-like class names.
/// </summary>
/// <remarks>
/// May include "class:control-character" or "class:nbsp" added automatically
/// during construction for special characters.
/// </remarks>
public string Style { get; }

/// <summary>
/// Gets the display width of this character in terminal cells.
/// </summary>
/// <remarks>
/// 0 for combining/zero-width characters, 1 for standard width, 2 for wide (CJK) characters.
/// Calculated using <see cref="UnicodeWidth.GetWidth(string)"/>.
/// </remarks>
public int Width { get; }
```

## Constructors

```csharp
/// <summary>
/// Initializes a new instance of the <see cref="Char"/> class.
/// </summary>
/// <param name="character">The character string. Default is a single space.</param>
/// <param name="style">The style string. Default is empty.</param>
/// <exception cref="ArgumentNullException">
/// <paramref name="character"/> or <paramref name="style"/> is <c>null</c>.
/// </exception>
/// <remarks>
/// Control characters are automatically transformed to display representations.
/// Use <see cref="Create"/> for cached instances to improve memory efficiency.
/// </remarks>
public Char(string character = " ", string style = "")
```

## Static Methods

```csharp
/// <summary>
/// Creates or retrieves a cached <see cref="Char"/> instance.
/// </summary>
/// <param name="character">The character string.</param>
/// <param name="style">The style string.</param>
/// <returns>A <see cref="Char"/> instance, potentially from cache.</returns>
/// <exception cref="ArgumentNullException">
/// <paramref name="character"/> or <paramref name="style"/> is <c>null</c>.
/// </exception>
/// <remarks>
/// Uses an internal cache of up to 1,000,000 entries for memory efficiency.
/// Prefer this method over direct construction for frequently used characters.
/// </remarks>
public static Char Create(string character, string style)
```

## Instance Methods

```csharp
/// <summary>
/// Determines whether the specified <see cref="Char"/> is equal to this instance.
/// </summary>
/// <param name="other">The <see cref="Char"/> to compare.</param>
/// <returns><c>true</c> if Character and Style are equal; otherwise, <c>false</c>.</returns>
public bool Equals(Char? other)

/// <inheritdoc/>
public override bool Equals(object? obj)

/// <inheritdoc/>
public override int GetHashCode()

/// <summary>
/// Returns a debug-friendly string representation.
/// </summary>
/// <returns>A string in the format <c>Char('{Character}', '{Style}')</c>.</returns>
public override string ToString()
```

## Operators

```csharp
/// <summary>
/// Determines whether two <see cref="Char"/> instances are equal.
/// </summary>
public static bool operator ==(Char? left, Char? right)

/// <summary>
/// Determines whether two <see cref="Char"/> instances are not equal.
/// </summary>
public static bool operator !=(Char? left, Char? right)
```

## Usage Examples

```csharp
// Basic character
var ch = new Char("A", "class:keyword");

// Cached character (preferred)
var cached = Char.Create(" ", Char.Transparent);

// Control character (auto-transformed)
var ctrl = new Char("\x01", "");
// ctrl.Character == "^A"
// ctrl.Style == "class:control-character "
// ctrl.Width == 2

// Control character with existing style
var ctrlStyled = new Char("\x01", "class:highlight");
// ctrlStyled.Style == "class:control-character class:highlight"

// Wide character
var cjk = new Char("中", "");
// cjk.Width == 2

// Non-breaking space
var nbsp = new Char("\xA0", "");
// nbsp.Character == " "
// nbsp.Style == "class:nbsp "

// ToString format
var ch = new Char("A", "class:keyword");
// ch.ToString() == "Char('A', 'class:keyword')"
```
