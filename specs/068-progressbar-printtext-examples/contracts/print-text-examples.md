# Contracts: Print Text Examples

**Feature**: 068-progressbar-printtext-examples
**Date**: 2026-02-07
**Project**: Stroke.Examples.PrintText

## Project Configuration

```xml
<!-- Stroke.Examples.PrintText.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <OutputType>Exe</OutputType>
    <LangVersion>13</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\Stroke\Stroke.csproj" />
  </ItemGroup>
</Project>
```

## Program.cs — Routing Contract

```csharp
namespace Stroke.Examples.PrintText;

internal static class Program
{
    private static readonly Dictionary<string, Action> Examples =
        new(StringComparer.OrdinalIgnoreCase)
    {
        ["ansi-colors"] = AnsiColors.Run,
        ["ansi"] = Ansi.Run,
        ["html"] = HtmlExample.Run,
        ["named-colors"] = NamedColors.Run,
        ["print-formatted-text"] = PrintFormattedText.Run,
        ["print-frame"] = PrintFrame.Run,
        ["true-color-demo"] = TrueColorDemo.Run,
        ["pygments-tokens"] = PygmentsTokens.Run,
        ["logo-ansi-art"] = LogoAnsiArt.Run,
    };

    public static void Main(string[] args) { /* same pattern as Prompts */ }
    private static void ShowUsage() { /* list examples */ }
}
```

**Note**: The `Html` example class is named `HtmlExample` to avoid collision with the `Html` type from `Stroke.FormattedText`.

## Example Class Contracts

### AnsiColors.cs (FR-006)
**Python source**: `print-text/ansi-colors.py` (102 lines)

```csharp
namespace Stroke.Examples.PrintText;

/// <summary>
/// Display all 16 ANSI foreground and 16 ANSI background colors.
/// Port of Python Prompt Toolkit's ansi-colors.py example.
/// </summary>
public static class AnsiColors
{
    public static void Run()
    {
        // Build FormattedText with 16 foreground colors:
        //   ansiblack, ansired, ansigreen, ansiyellow, ansiblue, ansimagenta,
        //   ansicyan, ansigray, ansibrightblack, ansibrightred, ansibrightgreen,
        //   ansibrightyellow, ansibrightblue, ansibrightmagenta, ansibrightcyan, ansiwhite
        // Then 16 background colors with "bg:" prefix
        // Print via FormattedTextOutput.Print()
    }
}
```

### Ansi.cs (FR-007)
**Python source**: `print-text/ansi.py` (52 lines)

```csharp
namespace Stroke.Examples.PrintText;

/// <summary>
/// Demonstrate ANSI escape sequences for bold, italic, underline, strikethrough,
/// and 256-color output.
/// Port of Python Prompt Toolkit's ansi.py example.
/// </summary>
public static class Ansi
{
    public static void Run()
    {
        // Print ANSI-escaped text via FormattedTextOutput.Print(new Ansi(...)):
        //   \x1b[1m bold, \x1b[3m italic, \x1b[4m underline, \x1b[9m strike
        //   \x1b[91m red, \x1b[94m blue
        //   \x1b[38;5;214m orange (256-color), \x1b[38;5;90m purple
        //   \x1b[97;101m white-on-red background
    }
}
```

### HtmlExample.cs (FR-008)
**Python source**: `print-text/html.py` (55 lines)

```csharp
namespace Stroke.Examples.PrintText;

/// <summary>
/// Demonstrate HTML-like formatting with &lt;b&gt;, &lt;i&gt;, &lt;ansired&gt;,
/// &lt;style&gt; tags, and string interpolation.
/// Port of Python Prompt Toolkit's html.py example.
/// </summary>
public static class HtmlExample
{
    public static void Run()
    {
        // Print via FormattedTextOutput.Print(new Html(...)):
        //   <b> bold, <blink> blink, <i> italic, <reverse>, <u> underline, <s> strike
        //   <ansired>, <ansiblue> color tags
        //   <orange>, <purple> named color tags
        //   <style fg="ansiwhite" bg="ansired"> inline style
        //   Interpolation with Html.Format() for escaping
    }
}
```

### NamedColors.cs (FR-009)
**Python source**: `print-text/named-colors.py` (31 lines)

```csharp
namespace Stroke.Examples.PrintText;

/// <summary>
/// Display all named colors at 4-bit, 8-bit, and 24-bit color depths.
/// Port of Python Prompt Toolkit's named-colors.py example.
/// </summary>
public static class NamedColors
{
    public static void Run()
    {
        // Iterate NamedColors.Colors dictionary
        // For each color, build FormattedText tuple with that color as background
        // Print at ColorDepth.Depth4Bit, Depth8Bit, Depth24Bit
    }
}
```

### PrintFormattedText.cs (FR-010)
**Python source**: `print-text/print-formatted-text.py` (48 lines)

```csharp
namespace Stroke.Examples.PrintText;

/// <summary>
/// Demonstrate four distinct formatting methods: FormattedText tuples,
/// HTML with style classes, HTML with inline styles, and ANSI escape sequences.
/// Port of Python Prompt Toolkit's print-formatted-text.py example.
/// </summary>
public static class PrintFormattedText
{
    public static void Run()
    {
        // 1. FormattedText list: [("class:hello", "Hello "), ("class:world", "World"), ("", "\n")]
        //    with Style.FromDict({"hello": "#ff0066", "world": "#44ff44 italic"})
        // 2. Html: <hello>hello</hello> <world>world</world>
        // 3. Html inline: <style fg="#ff0066">hello</style> <style fg="#44ff44"><i>world</i></style>
        // 4. Ansi: \x1b[31mhello \x1b[32mworld
    }
}
```

### PrintFrame.cs (FR-011)
**Python source**: `print-text/print-frame.py` (16 lines)

```csharp
namespace Stroke.Examples.PrintText;

/// <summary>
/// Render a bordered Frame containing a TextArea using PrintContainer.
/// Port of Python Prompt Toolkit's print-frame.py example.
/// </summary>
public static class PrintFrame
{
    public static void Run()
    {
        // FormattedTextOutput.PrintContainer(
        //     new AnyContainer(new Frame(
        //         new AnyContainer(new TextArea(text: "...")),
        //         title: "Stage: parse"
        //     ))
        // )
    }
}
```

### TrueColorDemo.cs (FR-012)
**Python source**: `print-text/true-color-demo.py` (37 lines)

```csharp
namespace Stroke.Examples.PrintText;

/// <summary>
/// Display 7 RGB color gradients each rendered at 3 color depths.
/// Port of Python Prompt Toolkit's true-color-demo.py example.
/// </summary>
public static class TrueColorDemo
{
    public static void Run()
    {
        // 7 gradient templates:
        //   "bg:#{0:x2}0000"    (red)
        //   "bg:#00{0:x2}00"    (green)
        //   "bg:#0000{0:x2}"    (blue)
        //   "bg:#{0:x2}{0:x2}00"  (yellow)
        //   "bg:#{0:x2}00{0:x2}"  (magenta)
        //   "bg:#00{0:x2}{0:x2}"  (cyan)
        //   "bg:#{0:x2}{0:x2}{0:x2}" (gray)
        //
        // For each template, i in 0..255 step 4:
        //   Build FormattedText fragments with (template.format(i), " ")
        //   Print at Depth4Bit, Depth8Bit, Depth24Bit
    }
}
```

### PygmentsTokens.cs (FR-013)
**Python source**: `print-text/pygments-tokens.py` (46 lines)

```csharp
namespace Stroke.Examples.PrintText;

/// <summary>
/// Display syntax-highlighted text using Pygments token types.
/// Port of Python Prompt Toolkit's pygments-tokens.py example.
/// </summary>
public static class PygmentsTokens
{
    public static void Run()
    {
        // 1. Manual PygmentsTokens list:
        //    [(Token.Keyword, "print"), (Token, "("), (Token.Literal.String, "\"hello\""), ...]
        //    with custom Style.FromDict for Token.Keyword, Token.Literal.String
        //
        // 2. Optionally: lexer-based output using TextMateLineLexer
    }
}
```

### LogoAnsiArt.cs (FR-014)
**Python source**: `print-text/prompt-toolkit-logo-ansi-art.py` (42 lines)

```csharp
namespace Stroke.Examples.PrintText;

/// <summary>
/// Render an ANSI art logo using 24-bit true color RGB background blocks.
/// Port of Python Prompt Toolkit's prompt-toolkit-logo-ansi-art.py example.
/// </summary>
public static class LogoAnsiArt
{
    public static void Run()
    {
        // Multi-line string with ANSI escape sequences:
        //   \x1b[48;2;R;G;Bm (24-bit background)
        //   \x1b[38;2;R;G;Bm (24-bit foreground)
        // Print via FormattedTextOutput.Print(new Ansi(logoString))
    }
}
```

## CLI Routing Names

| CLI Name | Class | Python Source |
|----------|-------|--------------|
| `ansi-colors` | `AnsiColors` | `ansi-colors.py` |
| `ansi` | `Ansi` | `ansi.py` |
| `html` | `HtmlExample` | `html.py` |
| `named-colors` | `NamedColors` | `named-colors.py` |
| `print-formatted-text` | `PrintFormattedText` | `print-formatted-text.py` |
| `print-frame` | `PrintFrame` | `print-frame.py` |
| `true-color-demo` | `TrueColorDemo` | `true-color-demo.py` |
| `pygments-tokens` | `PygmentsTokens` | `pygments-tokens.py` |
| `logo-ansi-art` | `LogoAnsiArt` | `prompt-toolkit-logo-ansi-art.py` |
