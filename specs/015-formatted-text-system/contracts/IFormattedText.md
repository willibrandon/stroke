# Contract: IFormattedText

**Namespace**: `Stroke.FormattedText`

## Interface Definition

```csharp
/// <summary>
/// Represents any object that can be converted to formatted text.
/// </summary>
/// <remarks>
/// This interface is the C# equivalent of Python Prompt Toolkit's
/// <c>__pt_formatted_text__</c> magic method protocol.
/// </remarks>
public interface IFormattedText
{
    /// <summary>
    /// Converts this object to a list of styled text fragments.
    /// </summary>
    /// <returns>A read-only list of style and text tuples.</returns>
    IReadOnlyList<StyleAndTextTuple> ToFormattedText();
}
```

## Implementing Classes

- `FormattedText` - Returns itself (implements `IReadOnlyList<StyleAndTextTuple>`)
- `Html` - Returns cached parse result
- `Ansi` - Returns cached parse result
- `PygmentsTokens` - Returns converted token list

## Usage Example

```csharp
IFormattedText formatted = new Html("<b>Hello</b>");
IReadOnlyList<StyleAndTextTuple> fragments = formatted.ToFormattedText();
// fragments[0] == ("class:b", "Hello")
```
