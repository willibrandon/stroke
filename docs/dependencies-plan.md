# Stroke Dependencies Plan: Syntax Highlighting and Unicode Width

## Executive Summary

Python Prompt Toolkit explicitly acknowledges two critical dependencies:
- **Pygments**: Syntax highlighter for code coloring
- **wcwidth**: Determine columns needed for wide characters

This document defines the complete strategy for integrating equivalent functionality into Stroke, ensuring 100% API fidelity while leveraging the .NET ecosystem.

---

## Table of Contents

1. [Unicode Width Handling](#1-unicode-width-handling)
2. [Syntax Highlighting (Pygments Equivalent)](#2-syntax-highlighting-pygments-equivalent)
3. [Recommended Dependencies](#3-recommended-dependencies)
4. [Implementation Architecture](#4-implementation-architecture)
5. [API Mappings](#5-api-mappings)
6. [Test Coverage](#6-test-coverage)
7. [Migration Path](#7-migration-path)

---

## 1. Unicode Width Handling

### 1.1 Purpose

Terminal applications must correctly calculate the display width of Unicode characters to:
- Align text in columns
- Wrap text at correct positions
- Position cursors accurately
- Handle CJK (Chinese, Japanese, Korean) double-width characters
- Process combining characters (zero-width diacritics)
- Support emoji sequences

### 1.2 Python wcwidth Library Analysis

The Python wcwidth library provides:

| Function | Purpose | Return Values |
|----------|---------|---------------|
| `wcwidth(char)` | Single character width | 0 (zero-width), 1 (narrow), 2 (wide), -1 (control) |
| `wcswidth(string, n)` | String width | Sum of widths or -1 if control chars found |
| `width(text)` | Enhanced width with escape sequence parsing | Maximum cursor column |
| `ljust/rjust/center` | Width-aware text justification | Padded string |
| `wrap(text, width)` | Width-aware text wrapping | List of wrapped lines |
| `clip(text, start, end)` | Column-based substring | Clipped text |
| `iter_graphemes(text)` | Grapheme cluster iteration | Iterator of grapheme strings |
| `iter_sequences(text)` | Escape sequence segmentation | Iterator of (segment, is_sequence) |

**Key Characteristics:**
- Supports 21 Unicode versions (4.1.0 through 17.0.0)
- Binary search through Unicode interval tables (~550 KB of data)
- LRU caching for performance (maxsize=2000)
- Handles ZWJ (Zero-Width Joiner) emoji sequences
- Handles VS16 (Variation Selector-16) width changes
- Full UAX #29 grapheme clustering

### 1.3 Existing .NET Solution

**Primary Recommendation: `Wcwidth` NuGet Package**

```xml
<PackageReference Include="Wcwidth" Version="4.0.1" />
```

| Attribute | Value |
|-----------|-------|
| **Package** | [Wcwidth](https://www.nuget.org/packages/Wcwidth) |
| **Version** | 4.0.1 |
| **Repository** | [github.com/spectreconsole/wcwidth](https://github.com/spectreconsole/wcwidth) |
| **License** | MIT ✅ |
| **Origin** | Port of Python wcwidth (Markus Kuhn's C implementation) |
| **Used By** | Spectre.Console, Terminal.Gui v2 |
| **Downloads** | 32,800+ |

**API:**
```csharp
using Wcwidth;

// Single character width
int width = UnicodeCalculator.GetWidth('コ'); // Returns 2 (wide CJK)
int width = UnicodeCalculator.GetWidth('A');  // Returns 1 (narrow ASCII)
int width = UnicodeCalculator.GetWidth('\0'); // Returns 0 (null)
int width = UnicodeCalculator.GetWidth('\x1b'); // Returns -1 (control)

// The Stroke wrapper should convert -1 to 0 for safe usage
```

**Alternative: `Wcwidth.Sources`**
```xml
<PackageReference Include="Wcwidth.Sources" Version="2.0.0" PrivateAssets="all" />
```
- Source-only package (better for NativeAOT/trimming)
- Same API surface
- Preferred for library distribution

### 1.4 Implementation Strategy for Stroke

#### Core Width Utilities

```csharp
namespace Stroke.Core;

/// <summary>
/// Unicode character width calculations for terminal display.
/// Wraps the Wcwidth library for POSIX-compatible character width determination.
/// </summary>
public static class UnicodeWidth
{
    /// <summary>
    /// Gets the display width of a character in terminal cells.
    /// </summary>
    /// <returns>0 (zero-width), 1 (narrow), or 2 (wide). Never returns -1.</returns>
    public static int GetWidth(char c)
    {
        var width = Wcwidth.UnicodeCalculator.GetWidth(c);
        return Math.Max(0, width); // Convert -1 (control) to 0
    }

    /// <summary>
    /// Gets the display width of a Rune in terminal cells.
    /// </summary>
    public static int GetWidth(Rune rune)
    {
        var width = Wcwidth.UnicodeCalculator.GetWidth(rune);
        return Math.Max(0, width);
    }

    /// <summary>
    /// Gets the display width of a string in terminal cells.
    /// </summary>
    public static int GetWidth(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        int total = 0;
        foreach (var c in text)
        {
            total += GetWidth(c);
        }
        return total;
    }

    /// <summary>
    /// Gets the display width of a string, handling ANSI escape sequences.
    /// </summary>
    public static int GetWidthWithSequences(string text)
    {
        // Strip ANSI sequences before measuring
        var stripped = StripAnsiSequences(text);
        return GetWidth(stripped);
    }
}
```

#### Character-Level Caching (Performance Pattern)

Following Spectre.Console's approach:

```csharp
namespace Stroke.Core;

/// <summary>
/// Cached character width calculator for high-frequency lookups.
/// </summary>
internal static class CharWidthCache
{
    private static readonly sbyte[] _cache = new sbyte[char.MaxValue + 1];
    private const sbyte Sentinel = sbyte.MinValue;

    static CharWidthCache()
    {
        Array.Fill(_cache, Sentinel);
    }

    public static int GetWidth(char c)
    {
        var cached = _cache[c];
        if (cached == Sentinel)
        {
            var width = Wcwidth.UnicodeCalculator.GetWidth(c);
            _cache[c] = (sbyte)Math.Max(0, width);
        }
        return _cache[c];
    }
}
```

#### String-Level Caching (Python Prompt Toolkit Pattern)

```csharp
namespace Stroke.Core;

/// <summary>
/// LRU cache for string width calculations.
/// Matches Python Prompt Toolkit's _CharSizesCache behavior.
/// </summary>
internal sealed class StringWidthCache
{
    private const int LongStringMinLength = 64;
    private const int MaxLongStrings = 16;

    private readonly Dictionary<string, int> _cache = new();
    private readonly Queue<string> _longStrings = new();

    public int GetWidth(string text)
    {
        if (_cache.TryGetValue(text, out var cached))
            return cached;

        int width = text.Length == 1
            ? CharWidthCache.GetWidth(text[0])
            : text.Sum(c => CharWidthCache.GetWidth(c));

        _cache[text] = width;

        // Evict old long strings to prevent unbounded growth
        if (text.Length > LongStringMinLength)
        {
            _longStrings.Enqueue(text);
            if (_longStrings.Count > MaxLongStrings)
            {
                var toRemove = _longStrings.Dequeue();
                _cache.Remove(toRemove);
            }
        }

        return width;
    }
}
```

### 1.5 Edge Cases Handled

| Character Category | Width | Examples |
|-------------------|-------|----------|
| Null (U+0000) | 0 | NUL |
| C0 Control (U+0001–U+001F) | 0* | SOH, STX, ETX, ... |
| C1 Control (U+007F–U+00A0) | 0* | DEL, PAD, HOP, ... |
| ASCII Printable (U+0020–U+007E) | 1 | A-Z, 0-9, symbols |
| Combining Marks | 0 | Accents, diacritics |
| Zero-Width Characters | 0 | ZWSP (U+200B), ZWJ (U+200D) |
| Soft Hyphen (U+00AD) | 1 | Special case |
| East Asian Wide (W) | 2 | CJK ideographs |
| East Asian Fullwidth (F) | 2 | Fullwidth ASCII |
| East Asian Ambiguous (A) | 1 or 2 | Context-dependent |
| Emoji with VS-16 | 2 | Emoji presentation selector |

*Note: wcwidth returns -1 for control characters; Stroke converts to 0 for safe arithmetic.

---

## 2. Syntax Highlighting (Pygments Equivalent)

### 2.1 Purpose

Syntax highlighting enables:
- Colorized source code display in REPLs
- Language-aware editing with token-based coloring
- Theme support for customizable appearance
- Integration with prompt sessions for code input

### 2.2 Python Pygments Library Analysis

Pygments is a comprehensive syntax highlighting library:

| Component | Count | Purpose |
|-----------|-------|---------|
| **Lexers** | 597 | Tokenize source code for 260+ languages |
| **Formatters** | 14 | Output tokens as HTML, ANSI, LaTeX, etc. |
| **Styles** | 49 | Color themes (Monokai, Dracula, etc.) |
| **Token Types** | 80 | Semantic categories (Keyword, String, etc.) |
| **Filters** | 8 | Token stream transformations |

**Architecture:**
```
Source Code → Lexer → Token Stream → [Filters] → Formatter → Colored Output
                                          ↓
                                        Style
```

**Key Classes:**
- `Lexer` - Base class for all tokenizers
- `RegexLexer` - State-machine lexer with regex rules
- `Token` - Hierarchical token type system (Token.Keyword.Type, etc.)
- `Formatter` - Output generation (TerminalFormatter for ANSI)
- `Style` - Color/attribute definitions for token types

**Python Prompt Toolkit Integration:**
- `PygmentsLexer` wrapper adapts Pygments lexers to prompt_toolkit interface
- `SyntaxSync` enables incremental highlighting (avoid re-lexing entire document)
- Token-to-style mapping: `Token.Keyword.Type` → `class:pygments.keyword.type`
- Style bridge: `style_from_pygments_cls()` converts Pygments styles

### 2.3 Existing .NET Solutions

#### Primary Recommendation: TextMateSharp

```xml
<PackageReference Include="TextMateSharp" Version="1.0.70" />
<PackageReference Include="TextMateSharp.Grammars" Version="1.0.69" />
```

| Attribute | Value |
|-----------|-------|
| **Package** | [TextMateSharp](https://www.nuget.org/packages/TextMateSharp/) |
| **Version** | 1.0.70 |
| **Repository** | [github.com/danipen/TextMateSharp](https://github.com/danipen/TextMateSharp) |
| **License** | MIT ✅ |
| **Foundation** | Port of microsoft/vscode-textmate |
| **Languages** | 40+ via TextMate grammars |
| **Regex Engine** | Oniguruma |

**Strengths:**
- Leverages VS Code's grammar ecosystem (well-maintained, battle-tested)
- Broad language coverage comparable to Pygments core languages
- Extensible grammar system for adding new languages
- Modern, actively maintained

**Limitations:**
- Cross-grammar injections not yet supported
- Terminal ANSI formatters not built-in (requires custom implementation)

#### Secondary: ColorCode-Universal

```xml
<PackageReference Include="ColorCode.Core" Version="2.0.15" />
```

| Attribute | Value |
|-----------|-------|
| **Package** | [ColorCode.Core](https://www.nuget.org/packages/ColorCode.Core/) |
| **Languages** | 12 (C#, VB.NET, Java, JavaScript, SQL, XML, C++, PHP, PowerShell, TypeScript, F#, ASPX) |
| **License** | Open Source (Apache-compatible) |
| **Origin** | MSDN/CodePlex heritage |

**Use Case:** Supplement TextMateSharp for Microsoft-stack languages.

#### NOT Recommended

| Library | Reason |
|---------|--------|
| Smdn.LibHighlightSharp | **GPLv3 license** - Incompatible with MIT |
| Highlight (native) | **GPLv3 license** - Incompatible with MIT |
| ColorfulCode | Pre-release, not production-ready |
| Ookii.FormatC | Unmaintained, limited languages |

### 2.4 Implementation Strategy for Stroke

#### Lexer Interface (Stroke.Lexers.Core)

```csharp
namespace Stroke.Lexers.Core;

/// <summary>
/// Interface for syntax highlighting lexers.
/// Mirrors Python Prompt Toolkit's Lexer interface with line-based evaluation.
/// </summary>
public interface ILexer
{
    /// <summary>
    /// Create a callable that returns styled fragments for a given line.
    /// </summary>
    Func<int, IReadOnlyList<(string Style, string Text)>> LexDocument(Document document);
}

/// <summary>
/// Simple lexer that applies a single style to all text.
/// </summary>
public sealed class SimpleLexer : ILexer
{
    private readonly string _style;

    public SimpleLexer(string style = "")
    {
        _style = style;
    }

    public Func<int, IReadOnlyList<(string Style, string Text)>> LexDocument(Document document)
    {
        var lines = document.Lines;
        return lineNo =>
        {
            if (lineNo < 0 || lineNo >= lines.Count)
                return Array.Empty<(string, string)>();
            return new[] { (_style, lines[lineNo]) };
        };
    }
}
```

#### TextMate-Based Pygments Lexer

```csharp
namespace Stroke.Lexers;

/// <summary>
/// Lexer that wraps TextMateSharp for Pygments-like syntax highlighting.
/// </summary>
public sealed class PygmentsLexer : ILexer
{
    private readonly IGrammar _grammar;
    private readonly ITheme _theme;
    private readonly ISyntaxSync _syncStrategy;

    public PygmentsLexer(string scopeName, ISyntaxSync? syncStrategy = null)
    {
        var registry = new Registry(new RegistryOptions(ThemeName.DarkPlus));
        _grammar = registry.LoadGrammar(scopeName);
        _theme = registry.GetDefaultTheme();
        _syncStrategy = syncStrategy ?? new SyncFromStart();
    }

    /// <summary>
    /// Create a PygmentsLexer from a filename, auto-detecting the language.
    /// </summary>
    public static ILexer FromFilename(string filename)
    {
        var extension = Path.GetExtension(filename);
        var scopeName = MapExtensionToScope(extension);
        return scopeName != null
            ? new PygmentsLexer(scopeName)
            : new SimpleLexer();
    }

    public Func<int, IReadOnlyList<(string Style, string Text)>> LexDocument(Document document)
    {
        var cache = new Dictionary<int, IReadOnlyList<(string Style, string Text)>>();
        var lines = document.Lines;

        return lineNo =>
        {
            if (cache.TryGetValue(lineNo, out var cached))
                return cached;

            var (startLine, _) = _syncStrategy.GetSyncStartPosition(document, lineNo);
            var stateStack = TokenizeFromLine(lines, startLine, lineNo);

            var result = TokenizeLine(lines[lineNo], stateStack);
            cache[lineNo] = result;
            return result;
        };
    }

    private IReadOnlyList<(string Style, string Text)> TokenizeLine(
        string line, IStateStack stateStack)
    {
        var result = new List<(string Style, string Text)>();
        var tokenizeResult = _grammar.TokenizeLine(line, stateStack);

        foreach (var token in tokenizeResult.Tokens)
        {
            var text = line[token.StartIndex..token.EndIndex];
            var style = ConvertScopesToStyle(token.Scopes);
            result.Add((style, text));
        }

        return result;
    }

    private static string ConvertScopesToStyle(IReadOnlyList<string> scopes)
    {
        // Convert TextMate scopes to Pygments-style class names
        // "source.python" → "class:pygments.source.python"
        // "keyword.control.python" → "class:pygments.keyword.control"
        var parts = new List<string> { "class:pygments" };
        foreach (var scope in scopes)
        {
            parts.AddRange(scope.Split('.'));
        }
        return string.Join(",", parts.Distinct());
    }
}
```

#### Syntax Synchronization

```csharp
namespace Stroke.Lexers;

/// <summary>
/// Strategy for finding where to start lexing for incremental highlighting.
/// </summary>
public interface ISyntaxSync
{
    /// <summary>
    /// Find the position to start lexing from.
    /// </summary>
    (int Row, int Column) GetSyncStartPosition(Document document, int lineNo);
}

/// <summary>
/// Always start lexing from the beginning of the document.
/// Most accurate but slowest for large documents.
/// </summary>
public sealed class SyncFromStart : ISyntaxSync
{
    public (int Row, int Column) GetSyncStartPosition(Document document, int lineNo)
        => (0, 0);
}

/// <summary>
/// Start lexing from the last line matching a regex pattern.
/// Balances accuracy with performance for large documents.
/// </summary>
public sealed class RegexSync : ISyntaxSync
{
    private const int FromStartIfNoSyncPosFound = 100;
    private readonly Regex _pattern;

    public RegexSync(string pattern)
    {
        _pattern = new Regex(pattern, RegexOptions.Compiled | RegexOptions.Multiline);
    }

    public (int Row, int Column) GetSyncStartPosition(Document document, int lineNo)
    {
        var lines = document.Lines;

        // Search backwards for sync pattern
        for (int i = lineNo - 1; i >= 0; i--)
        {
            if (_pattern.IsMatch(lines[i]))
                return (i, 0);
        }

        // No pattern found - start from beginning if document is small
        return lineNo < FromStartIfNoSyncPosFound
            ? (0, 0)
            : (lineNo, 0);
    }

    /// <summary>
    /// Create a RegexSync for common language patterns.
    /// </summary>
    public static RegexSync ForLanguage(string language) => language switch
    {
        "Python" or "Python3" => new RegexSync(@"^\s*(class|def)\s+"),
        "HTML" => new RegexSync(@"<[/a-zA-Z]"),
        "JavaScript" => new RegexSync(@"\bfunction\b"),
        "C#" or "CSharp" => new RegexSync(@"^\s*(class|struct|interface|enum|namespace)\s+"),
        _ => new RegexSync(@"^") // Match every line by default
    };
}
```

#### Token Type System

```csharp
namespace Stroke.Lexers;

/// <summary>
/// Hierarchical token types matching Pygments token hierarchy.
/// </summary>
public static class TokenTypes
{
    // Base token
    public const string Token = "class:pygments";

    // Top-level categories
    public const string Text = "class:pygments.text";
    public const string Whitespace = "class:pygments.text.whitespace";
    public const string Error = "class:pygments.error";
    public const string Escape = "class:pygments.escape";
    public const string Other = "class:pygments.other";

    // Keywords
    public const string Keyword = "class:pygments.keyword";
    public const string KeywordConstant = "class:pygments.keyword.constant";
    public const string KeywordDeclaration = "class:pygments.keyword.declaration";
    public const string KeywordNamespace = "class:pygments.keyword.namespace";
    public const string KeywordPseudo = "class:pygments.keyword.pseudo";
    public const string KeywordReserved = "class:pygments.keyword.reserved";
    public const string KeywordType = "class:pygments.keyword.type";

    // Names
    public const string Name = "class:pygments.name";
    public const string NameAttribute = "class:pygments.name.attribute";
    public const string NameBuiltin = "class:pygments.name.builtin";
    public const string NameClass = "class:pygments.name.class";
    public const string NameConstant = "class:pygments.name.constant";
    public const string NameDecorator = "class:pygments.name.decorator";
    public const string NameException = "class:pygments.name.exception";
    public const string NameFunction = "class:pygments.name.function";
    public const string NameLabel = "class:pygments.name.label";
    public const string NameNamespace = "class:pygments.name.namespace";
    public const string NameTag = "class:pygments.name.tag";
    public const string NameVariable = "class:pygments.name.variable";

    // Literals
    public const string Literal = "class:pygments.literal";
    public const string LiteralDate = "class:pygments.literal.date";

    // Strings
    public const string String = "class:pygments.literal.string";
    public const string StringBacktick = "class:pygments.literal.string.backtick";
    public const string StringChar = "class:pygments.literal.string.char";
    public const string StringDoc = "class:pygments.literal.string.doc";
    public const string StringDouble = "class:pygments.literal.string.double";
    public const string StringEscape = "class:pygments.literal.string.escape";
    public const string StringHeredoc = "class:pygments.literal.string.heredoc";
    public const string StringInterpol = "class:pygments.literal.string.interpol";
    public const string StringOther = "class:pygments.literal.string.other";
    public const string StringRegex = "class:pygments.literal.string.regex";
    public const string StringSingle = "class:pygments.literal.string.single";
    public const string StringSymbol = "class:pygments.literal.string.symbol";

    // Numbers
    public const string Number = "class:pygments.literal.number";
    public const string NumberBin = "class:pygments.literal.number.bin";
    public const string NumberFloat = "class:pygments.literal.number.float";
    public const string NumberHex = "class:pygments.literal.number.hex";
    public const string NumberInteger = "class:pygments.literal.number.integer";
    public const string NumberOct = "class:pygments.literal.number.oct";

    // Operators
    public const string Operator = "class:pygments.operator";
    public const string OperatorWord = "class:pygments.operator.word";

    // Punctuation
    public const string Punctuation = "class:pygments.punctuation";

    // Comments
    public const string Comment = "class:pygments.comment";
    public const string CommentHashbang = "class:pygments.comment.hashbang";
    public const string CommentMultiline = "class:pygments.comment.multiline";
    public const string CommentPreproc = "class:pygments.comment.preproc";
    public const string CommentSingle = "class:pygments.comment.single";
    public const string CommentSpecial = "class:pygments.comment.special";

    // Generic (for diffs, etc.)
    public const string Generic = "class:pygments.generic";
    public const string GenericDeleted = "class:pygments.generic.deleted";
    public const string GenericEmph = "class:pygments.generic.emph";
    public const string GenericError = "class:pygments.generic.error";
    public const string GenericHeading = "class:pygments.generic.heading";
    public const string GenericInserted = "class:pygments.generic.inserted";
    public const string GenericOutput = "class:pygments.generic.output";
    public const string GenericPrompt = "class:pygments.generic.prompt";
    public const string GenericStrong = "class:pygments.generic.strong";
    public const string GenericSubheading = "class:pygments.generic.subheading";
    public const string GenericTraceback = "class:pygments.generic.traceback";
}
```

#### ANSI Terminal Formatter

```csharp
namespace Stroke.Rendering;

/// <summary>
/// Formats tokens as ANSI escape sequences for terminal output.
/// </summary>
public sealed class AnsiTerminalFormatter
{
    private readonly IReadOnlyDictionary<string, Style> _styleMap;
    private readonly ColorDepth _colorDepth;

    public AnsiTerminalFormatter(
        IReadOnlyDictionary<string, Style> styleMap,
        ColorDepth colorDepth = ColorDepth.TrueColor)
    {
        _styleMap = styleMap;
        _colorDepth = colorDepth;
    }

    public string Format(IEnumerable<(string Style, string Text)> tokens)
    {
        var sb = new StringBuilder();

        foreach (var (style, text) in tokens)
        {
            if (_styleMap.TryGetValue(style, out var styleInfo))
            {
                sb.Append(GenerateAnsiCodes(styleInfo));
                sb.Append(text);
                sb.Append("\x1b[0m"); // Reset
            }
            else
            {
                sb.Append(text);
            }
        }

        return sb.ToString();
    }

    private string GenerateAnsiCodes(Style style)
    {
        var codes = new List<string>();

        if (style.Bold) codes.Add("1");
        if (style.Italic) codes.Add("3");
        if (style.Underline) codes.Add("4");

        if (style.Foreground != null)
        {
            var (r, g, b) = style.Foreground.Value;
            codes.Add($"38;2;{r};{g};{b}");
        }

        if (style.Background != null)
        {
            var (r, g, b) = style.Background.Value;
            codes.Add($"48;2;{r};{g};{b}");
        }

        return codes.Count > 0
            ? $"\x1b[{string.Join(";", codes)}m"
            : "";
    }
}

public enum ColorDepth
{
    Monochrome = 1,
    Basic = 4,      // 16 colors
    EightBit = 8,   // 256 colors
    TrueColor = 24  // 16.7 million colors
}
```

---

## 3. Recommended Dependencies

### 3.1 Required NuGet Packages

```xml
<ItemGroup>
  <!-- Unicode Width Calculation -->
  <PackageReference Include="Wcwidth" Version="4.0.1" />

  <!-- Syntax Highlighting (TextMate grammars) -->
  <PackageReference Include="TextMateSharp" Version="1.0.70" />
  <PackageReference Include="TextMateSharp.Grammars" Version="1.0.69" />
</ItemGroup>
```

### 3.2 Optional Packages

```xml
<ItemGroup>
  <!-- For NativeAOT/trimming scenarios -->
  <PackageReference Include="Wcwidth.Sources" Version="2.0.0" PrivateAssets="all" />

  <!-- For enhanced C#/.NET syntax highlighting -->
  <PackageReference Include="ColorCode.Core" Version="2.0.15" />
</ItemGroup>
```

### 3.3 License Compliance Matrix

| Package | License | MIT Compatible | Status |
|---------|---------|----------------|--------|
| Wcwidth | MIT | ✅ | Approved |
| Wcwidth.Sources | MIT | ✅ | Approved |
| TextMateSharp | MIT | ✅ | Approved |
| TextMateSharp.Grammars | MIT | ✅ | Approved |
| ColorCode.Core | Apache 2.0 | ✅ | Approved |
| ColorCode.HTML | Apache 2.0 | ✅ | Approved |
| Smdn.LibHighlightSharp | GPLv3 | ❌ | **PROHIBITED** |
| Highlight (native) | GPLv3 | ❌ | **PROHIBITED** |

---

## 4. Implementation Architecture

### 4.1 Layer Dependencies

```
┌─────────────────────────────────────────────────────────────┐
│                    Stroke.Shortcuts                          │
│  (High-level API: PromptAsync with syntax highlighting)     │
├─────────────────────────────────────────────────────────────┤
│                    Stroke.Application                        │
│  (PromptSession, lexer/style integration)                   │
├─────────────────────────────────────────────────────────────┤
│    Stroke.Lexers          │    Stroke.Styles                │
│  (ILexer, PygmentsLexer,  │  (Style, Theme,                 │
│   SimpleLexer, RegexLexer)│   PygmentsStyleBridge)          │
├─────────────────────────────────────────────────────────────┤
│                    Stroke.Rendering                          │
│  (AnsiTerminalFormatter, Screen with width calculations)    │
├─────────────────────────────────────────────────────────────┤
│                    Stroke.Core                               │
│  (Document, UnicodeWidth, CharWidthCache)                   │
├─────────────────────────────────────────────────────────────┤
│                 External Dependencies                        │
│  (Wcwidth, TextMateSharp, TextMateSharp.Grammars)           │
└─────────────────────────────────────────────────────────────┘
```

### 4.2 Namespace Structure

```
Stroke.Core
├── UnicodeWidth.cs              # Character width calculations
├── CharWidthCache.cs            # BMP character caching
└── StringWidthCache.cs          # String-level LRU cache

Stroke.Lexers
├── Core/
│   ├── ILexer.cs                # Base lexer interface
│   ├── SimpleLexer.cs           # Single-style lexer
│   └── DynamicLexer.cs          # Runtime-switchable lexer
├── PygmentsLexer.cs             # TextMate-based syntax highlighting
├── RegexLexer.cs                # Regex pattern-based lexer
├── SyntaxSync/
│   ├── ISyntaxSync.cs           # Sync strategy interface
│   ├── SyncFromStart.cs         # Always start from beginning
│   └── RegexSync.cs             # Pattern-based sync
└── TokenTypes.cs                # Pygments-compatible token constants

Stroke.Styles
├── Core/
│   ├── Style.cs                 # Style definition
│   └── Theme.cs                 # Collection of styles
├── PygmentsStyleBridge.cs       # Convert Pygments themes
└── BuiltIn/
    ├── MonokaiStyle.cs          # Monokai theme
    ├── DraculaStyle.cs          # Dracula theme
    └── DefaultStyle.cs          # Default theme
```

---

## 5. API Mappings

### 5.1 Unicode Width APIs

| Python (wcwidth) | C# (Stroke) | Notes |
|------------------|-------------|-------|
| `wcwidth(char)` | `UnicodeWidth.GetWidth(char)` | Returns 0-2; converts -1 to 0 |
| `wcswidth(string, n)` | `UnicodeWidth.GetWidth(string)` | No length limit needed |
| `wcswidth(string, n)` returning -1 | N/A | Stroke never returns -1 |
| `width(text, control_codes='parse')` | `UnicodeWidth.GetWidthWithSequences(string)` | Strips ANSI before measuring |
| `ljust(text, width)` | `UnicodeWidth.JustifyLeft(string, int)` | Width-aware padding |
| `rjust(text, width)` | `UnicodeWidth.JustifyRight(string, int)` | Width-aware padding |
| `center(text, width)` | `UnicodeWidth.Center(string, int)` | Width-aware padding |
| `wrap(text, width)` | `UnicodeWidth.Wrap(string, int)` | Width-aware wrapping |
| `clip(text, start, end)` | `UnicodeWidth.Clip(string, int, int)` | Column-based substring |
| `iter_graphemes(text)` | `StringInfo.GetTextElementEnumerator(string)` | .NET built-in (UAX29) |
| `strip_sequences(text)` | `AnsiSequences.Strip(string)` | Remove ANSI escapes |

### 5.2 Lexer APIs

| Python (prompt_toolkit) | C# (Stroke) | Notes |
|-------------------------|-------------|-------|
| `Lexer` | `ILexer` | Interface |
| `Lexer.lex_document(doc)` | `ILexer.LexDocument(Document)` | Returns line accessor |
| `SimpleLexer(style)` | `SimpleLexer(string)` | Uniform style |
| `DynamicLexer(get_lexer)` | `DynamicLexer(Func<ILexer>)` | Runtime switching |
| `PygmentsLexer(lexer_cls)` | `PygmentsLexer(string)` | TextMate scope name |
| `PygmentsLexer.from_filename(fn)` | `PygmentsLexer.FromFilename(string)` | Auto-detect language |
| `SyntaxSync` | `ISyntaxSync` | Interface |
| `SyncFromStart()` | `SyncFromStart()` | Lex from beginning |
| `RegexSync(pattern)` | `RegexSync(string)` | Pattern-based start |

### 5.3 Style APIs

| Python (Pygments) | C# (Stroke) | Notes |
|-------------------|-------------|-------|
| `Token` hierarchy | `TokenTypes` constants | Static strings |
| `Token.Keyword.Type` | `TokenTypes.KeywordType` | `"class:pygments.keyword.type"` |
| `Style` | `PygmentsStyle` | Color/attribute definition |
| `get_style_by_name(name)` | `PygmentsStyles.GetByName(string)` | Built-in themes |
| `style_from_pygments_cls(cls)` | `PygmentsStyleBridge.FromTheme(theme)` | Convert TextMate theme |

---

## 6. Test Coverage

### 6.1 Unicode Width Tests

```csharp
public class UnicodeWidthTests
{
    [Theory]
    [InlineData('A', 1)]       // ASCII
    [InlineData('コ', 2)]      // CJK Katakana (wide)
    [InlineData('中', 2)]      // CJK ideograph (wide)
    [InlineData('\0', 0)]      // Null
    [InlineData('\x1b', 0)]    // Escape (control → 0)
    [InlineData('\u0301', 0)]  // Combining acute accent
    [InlineData('\u200B', 0)]  // Zero-width space
    [InlineData('\u00AD', 1)]  // Soft hyphen (special case)
    public void GetWidth_SingleCharacter_ReturnsCorrectWidth(char c, int expected)
    {
        Assert.Equal(expected, UnicodeWidth.GetWidth(c));
    }

    [Theory]
    [InlineData("Hello", 5)]
    [InlineData("コンニチハ", 10)]  // 5 wide chars × 2
    [InlineData("Hello世界", 9)]    // 5 + 2×2
    [InlineData("café", 4)]         // 4 chars, no combining
    [InlineData("café\u0301", 4)]   // With combining accent
    public void GetWidth_String_ReturnsCorrectWidth(string text, int expected)
    {
        Assert.Equal(expected, UnicodeWidth.GetWidth(text));
    }

    [Fact]
    public void GetWidth_WithAnsiSequences_IgnoresSequences()
    {
        var text = "\x1b[31mRed\x1b[0m";
        Assert.Equal(3, UnicodeWidth.GetWidthWithSequences(text));
    }
}
```

### 6.2 Lexer Tests

```csharp
public class PygmentsLexerTests
{
    [Fact]
    public void LexDocument_Python_TokenizesCorrectly()
    {
        var lexer = new PygmentsLexer("source.python");
        var doc = new Document("def hello():\n    pass");

        var getLine = lexer.LexDocument(doc);
        var line0 = getLine(0);

        Assert.Contains(line0, t => t.Style.Contains("keyword") && t.Text == "def");
        Assert.Contains(line0, t => t.Style.Contains("function") && t.Text == "hello");
    }

    [Fact]
    public void FromFilename_RecognizesExtensions()
    {
        var lexer = PygmentsLexer.FromFilename("test.py");
        Assert.IsType<PygmentsLexer>(lexer);
    }

    [Fact]
    public void FromFilename_UnknownExtension_ReturnsSimpleLexer()
    {
        var lexer = PygmentsLexer.FromFilename("unknown.xyz");
        Assert.IsType<SimpleLexer>(lexer);
    }
}
```

### 6.3 Test Matrix

| Test Category | Test Count | Priority |
|---------------|------------|----------|
| UnicodeWidth single char | 20+ | P0 |
| UnicodeWidth strings | 15+ | P0 |
| UnicodeWidth edge cases | 10+ | P0 |
| CharWidthCache caching | 5 | P1 |
| StringWidthCache eviction | 5 | P1 |
| SimpleLexer | 5 | P0 |
| PygmentsLexer Python | 10 | P0 |
| PygmentsLexer C# | 10 | P1 |
| PygmentsLexer JavaScript | 10 | P1 |
| RegexSync patterns | 8 | P1 |
| AnsiTerminalFormatter | 10 | P0 |
| Style conversion | 10 | P1 |

---

## 7. Migration Path

### 7.1 Phase 1: Core Width Utilities (Week 1)

1. Add Wcwidth NuGet dependency
2. Implement `UnicodeWidth` static class
3. Implement `CharWidthCache` for BMP characters
4. Implement `StringWidthCache` with LRU eviction
5. Write comprehensive unit tests
6. Integrate with `Char` class in Stroke.Core

### 7.2 Phase 2: Basic Lexer Infrastructure (Week 2)

1. Define `ILexer` interface
2. Implement `SimpleLexer`
3. Implement `DynamicLexer`
4. Implement `RegexLexer` with state machine
5. Define `ISyntaxSync` interface
6. Implement `SyncFromStart` and `RegexSync`

### 7.3 Phase 3: TextMate Integration (Week 3)

1. Add TextMateSharp NuGet dependencies
2. Implement `PygmentsLexer` wrapping TextMate
3. Implement scope-to-style conversion
4. Implement `FromFilename` factory method
5. Create token type constants matching Pygments

### 7.4 Phase 4: Style System (Week 4)

1. Define `PygmentsStyle` class
2. Implement `PygmentsStyleBridge` for TextMate themes
3. Port key Pygments themes (Monokai, Dracula, Default)
4. Implement `AnsiTerminalFormatter`
5. Integrate with rendering pipeline

### 7.5 Phase 5: PromptSession Integration (Week 5)

1. Add lexer parameter to `PromptSession`
2. Add style parameter to `PromptSession`
3. Implement syntax-highlighted input display
4. Implement syntax-highlighted multiline editing
5. End-to-end testing with real REPL scenarios

---

## Appendix A: Language Support Matrix

| Language | TextMate Scope | Python Pygments | Priority |
|----------|----------------|-----------------|----------|
| Python | `source.python` | `PythonLexer` | P0 |
| C# | `source.cs` | `CSharpLexer` | P0 |
| JavaScript | `source.js` | `JavaScriptLexer` | P0 |
| TypeScript | `source.ts` | `TypeScriptLexer` | P0 |
| JSON | `source.json` | `JsonLexer` | P0 |
| HTML | `text.html.basic` | `HtmlLexer` | P1 |
| CSS | `source.css` | `CssLexer` | P1 |
| SQL | `source.sql` | `SqlLexer` | P1 |
| Markdown | `text.html.markdown` | `MarkdownLexer` | P1 |
| YAML | `source.yaml` | `YamlLexer` | P1 |
| XML | `text.xml` | `XmlLexer` | P1 |
| Bash | `source.shell` | `BashLexer` | P2 |
| PowerShell | `source.powershell` | `PowerShellLexer` | P2 |
| Go | `source.go` | `GoLexer` | P2 |
| Rust | `source.rust` | `RustLexer` | P2 |
| Java | `source.java` | `JavaLexer` | P2 |

---

## Appendix B: Performance Benchmarks (Target)

| Operation | Target Latency | Notes |
|-----------|----------------|-------|
| `GetWidth(char)` | < 50 ns | Cached lookup |
| `GetWidth(string)` 100 chars | < 5 µs | Sum of cached lookups |
| `LexDocument` 1000 lines | < 50 ms | Initial full lex |
| `LexDocument` single line | < 1 ms | Cached/incremental |
| ANSI format 1000 tokens | < 10 ms | String building |

---

## Appendix C: Compliance Checklist

- [ ] **Principle I**: All Python APIs mapped to C# equivalents
- [ ] **Principle II**: UnicodeWidth uses immutable caching patterns
- [ ] **Principle III**: Dependencies follow layered architecture
- [ ] **Principle IV**: Cross-platform terminal support verified
- [ ] **Principle VI**: Performance benchmarks met
- [ ] **Principle VII**: No scope reduction from Python functionality
- [ ] **Principle VIII**: Test coverage ≥ 80%
- [ ] **Principle IX**: Implementation matches this planning document

---

**Document Version**: 1.0.0
**Created**: 2026-01-23
**Author**: Generated from comprehensive analysis of Python Prompt Toolkit, Pygments, wcwidth, and .NET ecosystem
**Status**: Ready for Implementation
