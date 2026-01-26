# Contract: Template

**Namespace**: `Stroke.FormattedText`

## Class Definition

```csharp
/// <summary>
/// String template for formatted text interpolation.
/// </summary>
/// <remarks>
/// <para>
/// Allows creating templates with <c>{}</c> placeholders that can be filled
/// with formatted text values (strings, HTML, ANSI, etc.).
/// </para>
/// <para>
/// Unlike <see cref="Html.Format"/> and <see cref="Ansi.Format"/>, Template
/// preserves the formatting of the interpolated values rather than escaping them.
/// </para>
/// </remarks>
public sealed class Template
{
    /// <summary>
    /// Gets the template text with <c>{}</c> placeholders.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Creates a new <see cref="Template"/> with the given text.
    /// </summary>
    /// <param name="text">The template text containing <c>{}</c> placeholders.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="text"/> contains <c>{0}</c> (positional placeholders not supported).</exception>
    public Template(string text);

    /// <summary>
    /// Formats the template with the given values.
    /// </summary>
    /// <param name="values">Values to substitute for each <c>{}</c> placeholder.</param>
    /// <returns>A callable that produces the formatted result when invoked.</returns>
    /// <remarks>
    /// <para>
    /// The number of values must match the number of <c>{}</c> placeholders.
    /// </para>
    /// <para>
    /// Returns a <see cref="Func{AnyFormattedText}"/> for lazy evaluation,
    /// matching Python Prompt Toolkit's behavior.
    /// </para>
    /// </remarks>
    public Func<AnyFormattedText> Format(params AnyFormattedText[] values);

    /// <summary>
    /// Returns a string representation of this Template.
    /// </summary>
    public override string ToString();
}
```

## Usage Examples

```csharp
// Basic interpolation
var template = new Template("Hello {}!");
var result = template.Format("World");
var fragments = FormattedTextUtils.ToFormattedText(result());
// [("", "Hello "), ("", "World"), ("", "!")]

// With HTML formatting
var greeting = new Template("Welcome, {}!");
var formatted = greeting.Format(new Html("<b>Admin</b>"));
var frags = FormattedTextUtils.ToFormattedText(formatted());
// [("", "Welcome, "), ("class:b", "Admin"), ("", "!")]

// Multiple placeholders
var message = new Template("{} says {}");
var output = message.Format(
    new Html("<i>Alice</i>"),
    new Html("<b>Hello</b>"));
var list = FormattedTextUtils.ToFormattedText(output());
// [("class:i", "Alice"), ("", " says "), ("class:b", "Hello")]

// Lazy evaluation
var lazy = new Template("Time: {}").Format(DateTime.Now.ToString());
// The callable is not evaluated until invoked
Thread.Sleep(1000);
var evaluated = lazy(); // Uses original time, not current time
```

## Key Differences from Html.Format / Ansi.Format

| Feature | Template | Html.Format / Ansi.Format |
|---------|----------|---------------------------|
| Escaping | No escaping (preserves formatting) | Escapes special characters |
| Placeholder syntax | `{}` only | `{0}`, `{name}`, `%s`, etc. |
| Return type | `Func<AnyFormattedText>` (lazy) | `Html` / `Ansi` (eager) |
| Input types | Any `AnyFormattedText` | Objects converted to string |

## Validation

- Template text must not contain `{0}` (positional placeholders)
- Number of `{}` placeholders must match number of values passed to `Format()`
- An `ArgumentException` is thrown if counts don't match
