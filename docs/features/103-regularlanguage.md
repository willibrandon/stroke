# Feature 103: Regular Language Grammar

## Overview

Implement the Regular Language Grammar system - a tool for expressing command-line input grammar as regular expressions with named groups. Provides completion, validation, and syntax highlighting based on the grammar.

## Python Prompt Toolkit Reference

**Source:**
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/contrib/regular_languages/__init__.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/contrib/regular_languages/compiler.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/contrib/regular_languages/completion.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/contrib/regular_languages/lexer.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/contrib/regular_languages/validation.py`
- `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/contrib/regular_languages/regex_parser.py`

## Public API

### Compile Function

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Compile a regular expression grammar into a CompiledGrammar.
/// </summary>
public static class Grammar
{
    /// <summary>
    /// Compile a regular expression grammar string.
    /// </summary>
    /// <param name="expression">Regular expression with named groups.</param>
    /// <param name="escapeFuncs">Functions to escape variable values.</param>
    /// <param name="unescapeFuncs">Functions to unescape variable values.</param>
    /// <returns>A compiled grammar.</returns>
    /// <example>
    /// var grammar = Grammar.Compile(@"
    ///     (?P&lt;command&gt;add|remove|list) \s+
    ///     (?P&lt;filename&gt;[^\s]+)
    /// ");
    /// </example>
    public static CompiledGrammar Compile(
        string expression,
        IDictionary<string, Func<string, string>>? escapeFuncs = null,
        IDictionary<string, Func<string, string>>? unescapeFuncs = null);
}
```

### CompiledGrammar

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// A compiled grammar that can match input strings and extract variables.
/// </summary>
public sealed class CompiledGrammar
{
    /// <summary>
    /// Match the complete string with the grammar.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>Match instance or null if no match.</returns>
    public Match? Match(string input);

    /// <summary>
    /// Match a prefix of the string with the grammar.
    /// Used for autocompletion on incomplete input.
    /// </summary>
    /// <param name="input">The input string (typically text before cursor).</param>
    /// <returns>Match instance or null if no match.</returns>
    public Match? MatchPrefix(string input);

    /// <summary>
    /// Escape a value for a variable according to registered escape functions.
    /// </summary>
    /// <param name="varname">Variable name.</param>
    /// <param name="value">Value to escape.</param>
    /// <returns>Escaped value.</returns>
    public string Escape(string varname, string value);

    /// <summary>
    /// Unescape a value for a variable according to registered unescape functions.
    /// </summary>
    /// <param name="varname">Variable name.</param>
    /// <param name="value">Value to unescape.</param>
    /// <returns>Unescaped value.</returns>
    public string Unescape(string varname, string value);
}
```

### Match and Variables

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Result of matching input against a compiled grammar.
/// </summary>
public sealed class Match
{
    /// <summary>
    /// The original input string.
    /// </summary>
    public string Input { get; }

    /// <summary>
    /// Get the matched variables.
    /// </summary>
    public Variables Variables();

    /// <summary>
    /// Get trailing input that doesn't match the grammar.
    /// </summary>
    public MatchVariable? TrailingInput();

    /// <summary>
    /// Get variables whose match ends at the end of the input string.
    /// Used for autocompletion.
    /// </summary>
    public IEnumerable<MatchVariable> EndNodes();
}

/// <summary>
/// Collection of matched variables.
/// </summary>
public sealed class Variables : IEnumerable<MatchVariable>
{
    /// <summary>
    /// Get the value of a variable by name.
    /// </summary>
    /// <param name="key">Variable name.</param>
    /// <returns>Value or null if not found.</returns>
    public string? Get(string key);

    /// <summary>
    /// Get the value of a variable by name with a default.
    /// </summary>
    /// <param name="key">Variable name.</param>
    /// <param name="defaultValue">Default value if not found.</param>
    /// <returns>Value or default.</returns>
    public string Get(string key, string defaultValue);

    /// <summary>
    /// Get all values for a variable (for repeated matches).
    /// </summary>
    /// <param name="key">Variable name.</param>
    /// <returns>List of values.</returns>
    public IReadOnlyList<string> GetAll(string key);

    /// <summary>
    /// Indexer for getting variable values.
    /// </summary>
    public string? this[string key] { get; }

    /// <inheritdoc/>
    public IEnumerator<MatchVariable> GetEnumerator();
}

/// <summary>
/// A single matched variable.
/// </summary>
public sealed class MatchVariable
{
    /// <summary>
    /// Name of the variable.
    /// </summary>
    public string VarName { get; }

    /// <summary>
    /// Matched value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Start position in the input string.
    /// </summary>
    public int Start { get; }

    /// <summary>
    /// End position in the input string.
    /// </summary>
    public int Stop { get; }

    /// <summary>
    /// Slice as tuple (Start, Stop).
    /// </summary>
    public (int Start, int Stop) Slice { get; }
}
```

### GrammarCompleter

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Completer for autocompletion according to grammar variables.
/// Each variable can have its own completer.
/// </summary>
public sealed class GrammarCompleter : ICompleter
{
    /// <summary>
    /// Create a grammar-based completer.
    /// </summary>
    /// <param name="compiledGrammar">The compiled grammar.</param>
    /// <param name="completers">Map of variable names to completers.</param>
    public GrammarCompleter(
        CompiledGrammar compiledGrammar,
        IDictionary<string, ICompleter> completers);

    /// <summary>
    /// The compiled grammar.
    /// </summary>
    public CompiledGrammar CompiledGrammar { get; }

    /// <summary>
    /// Variable completers.
    /// </summary>
    public IReadOnlyDictionary<string, ICompleter> Completers { get; }

    /// <inheritdoc/>
    public IEnumerable<Completion> GetCompletions(
        Document document,
        CompleteEvent completeEvent);
}
```

### GrammarLexer

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Lexer for syntax highlighting according to grammar variables.
/// Each variable can have its own lexer for recursive highlighting.
/// </summary>
public sealed class GrammarLexer : ILexer
{
    /// <summary>
    /// Create a grammar-based lexer.
    /// </summary>
    /// <param name="compiledGrammar">The compiled grammar.</param>
    /// <param name="defaultStyle">Default style for unmatched text.</param>
    /// <param name="lexers">Map of variable names to lexers.</param>
    public GrammarLexer(
        CompiledGrammar compiledGrammar,
        string defaultStyle = "",
        IDictionary<string, ILexer>? lexers = null);

    /// <summary>
    /// The compiled grammar.
    /// </summary>
    public CompiledGrammar CompiledGrammar { get; }

    /// <summary>
    /// Default style.
    /// </summary>
    public string DefaultStyle { get; }

    /// <summary>
    /// Variable lexers.
    /// </summary>
    public IReadOnlyDictionary<string, ILexer> Lexers { get; }

    /// <inheritdoc/>
    public Func<int, StyleAndTextTuples> LexDocument(Document document);
}
```

### GrammarValidator

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Validator for validation according to grammar variables.
/// Each variable can have its own validator.
/// </summary>
public sealed class GrammarValidator : IValidator
{
    /// <summary>
    /// Create a grammar-based validator.
    /// </summary>
    /// <param name="compiledGrammar">The compiled grammar.</param>
    /// <param name="validators">Map of variable names to validators.</param>
    public GrammarValidator(
        CompiledGrammar compiledGrammar,
        IDictionary<string, IValidator> validators);

    /// <summary>
    /// The compiled grammar.
    /// </summary>
    public CompiledGrammar CompiledGrammar { get; }

    /// <summary>
    /// Variable validators.
    /// </summary>
    public IReadOnlyDictionary<string, IValidator> Validators { get; }

    /// <inheritdoc/>
    public void Validate(Document document);
}
```

### Parse Tree Nodes

```csharp
namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Base class for grammar parse tree nodes.
/// </summary>
public abstract class Node
{
    /// <summary>
    /// Concatenate two nodes into a sequence.
    /// </summary>
    public static NodeSequence operator +(Node left, Node right);

    /// <summary>
    /// Create an OR of two nodes.
    /// </summary>
    public static AnyNode operator |(Node left, Node right);
}

/// <summary>
/// Union (OR) of multiple nodes.
/// </summary>
public sealed class AnyNode : Node
{
    public AnyNode(IReadOnlyList<Node> children);
    public IReadOnlyList<Node> Children { get; }
}

/// <summary>
/// Sequence of nodes (concatenation).
/// </summary>
public sealed class NodeSequence : Node
{
    public NodeSequence(IReadOnlyList<Node> children);
    public IReadOnlyList<Node> Children { get; }
}

/// <summary>
/// A regular expression literal.
/// </summary>
public sealed class Regex : Node
{
    public Regex(string regex);
    public string Pattern { get; }
}

/// <summary>
/// A lookahead assertion.
/// </summary>
public sealed class Lookahead : Node
{
    public Lookahead(Node childNode, bool negative = false);
    public Node ChildNode { get; }
    public bool Negative { get; }
}

/// <summary>
/// A named variable wrapping a child node.
/// </summary>
public sealed class Variable : Node
{
    public Variable(Node childNode, string varName = "");
    public Node ChildNode { get; }
    public string VarName { get; }
}

/// <summary>
/// Repetition of a node.
/// </summary>
public sealed class Repeat : Node
{
    public Repeat(
        Node childNode,
        int minRepeat = 0,
        int? maxRepeat = null,
        bool greedy = true);
    public Node ChildNode { get; }
    public int MinRepeat { get; }
    public int? MaxRepeat { get; }
    public bool Greedy { get; }
}
```

## Project Structure

```
src/Stroke/
└── Contrib/
    └── RegularLanguages/
        ├── Grammar.cs
        ├── CompiledGrammar.cs
        ├── Match.cs
        ├── Variables.cs
        ├── MatchVariable.cs
        ├── GrammarCompleter.cs
        ├── GrammarLexer.cs
        ├── GrammarValidator.cs
        ├── Nodes/
        │   ├── Node.cs
        │   ├── AnyNode.cs
        │   ├── NodeSequence.cs
        │   ├── Regex.cs
        │   ├── Lookahead.cs
        │   ├── Variable.cs
        │   └── Repeat.cs
        └── RegexParser.cs
tests/Stroke.Tests/
└── Contrib/
    └── RegularLanguages/
        ├── GrammarTests.cs
        ├── GrammarCompleterTests.cs
        ├── GrammarLexerTests.cs
        └── GrammarValidatorTests.cs
```

## Implementation Notes

### Usage Example

```csharp
// Define a simple shell grammar
var grammar = Grammar.Compile(@"
    \s*
    (
        pwd |
        ls |
        (cd \s+ "" (?P<directory>[^""]*) "") |
        (cat \s+ "" (?P<filename>[^""]*) "")
    )
    \s*
");

// Create completer with variable-specific completers
var completer = new GrammarCompleter(grammar, new Dictionary<string, ICompleter>
{
    ["directory"] = new PathCompleter(onlyDirectories: true),
    ["filename"] = new PathCompleter()
});

// Create validator
var validator = new GrammarValidator(grammar, new Dictionary<string, IValidator>
{
    ["directory"] = new DirectoryExistsValidator(),
    ["filename"] = new FileExistsValidator()
});

// Create lexer
var lexer = new GrammarLexer(grammar, lexers: new Dictionary<string, ILexer>
{
    ["directory"] = new SimpleLexer(style: "class:path"),
    ["filename"] = new SimpleLexer(style: "class:path")
});

// Use in prompt session
var session = new PromptSession(
    completer: completer,
    validator: validator,
    lexer: lexer
);
```

### Escape/Unescape Example

```csharp
// Grammar with escaping for quoted strings
var grammar = Grammar.Compile(
    @"cat \s+ ""(?P<filename>[^""\\]|\\.)*""",
    escapeFuncs: new Dictionary<string, Func<string, string>>
    {
        ["filename"] = s => s.Replace("\"", "\\\"")
    },
    unescapeFuncs: new Dictionary<string, Func<string, string>>
    {
        ["filename"] = s => s.Replace("\\\"", "\"")
    }
);

// When completing, values are escaped before insertion
// When parsing, values are unescaped for validation
```

## Dependencies

- Feature 1: Document model
- Feature 11: Completion (ICompleter)
- Feature 34: Lexers (ILexer)
- Feature 28: Validation (IValidator)

## Implementation Tasks

1. Implement regex tokenizer (tokenize_regex)
2. Implement regex parser (parse_regex)
3. Implement parse tree nodes (Node, AnyNode, etc.)
4. Implement CompiledGrammar with prefix matching
5. Implement Match and Variables classes
6. Implement GrammarCompleter
7. Implement GrammarLexer
8. Implement GrammarValidator
9. Write comprehensive unit tests

## Acceptance Criteria

- [ ] Grammar compiles regular expressions with named groups
- [ ] Match correctly extracts variable values
- [ ] MatchPrefix handles incomplete input
- [ ] GrammarCompleter provides completions for each variable
- [ ] GrammarLexer highlights according to variable lexers
- [ ] GrammarValidator validates each variable independently
- [ ] Escape/unescape functions work correctly
- [ ] Trailing input detection works
- [ ] EndNodes identifies completion points
- [ ] Unit tests achieve 80% coverage
