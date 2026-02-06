# API Contract: Prompt Example Class

**Feature**: 065-prompt-examples

## Example Class Contract

Every example MUST follow this contract:

```csharp
namespace Stroke.Examples.Prompts;

/// <summary>
/// [Brief description of what the example demonstrates].
/// Port of Python Prompt Toolkit's [python-file-name].py example.
/// </summary>
public static class [ClassName]
{
    public static void Run()
    {
        try
        {
            // Example implementation using Stroke public API only
        }
        catch (KeyboardInterruptException)
        {
            // Ctrl+C pressed - exit gracefully
        }
        catch (EOFException)
        {
            // Ctrl+D pressed - exit gracefully
        }
    }
}
```

### Rules

1. **Access**: `public static class`
2. **Method**: `public static void Run()` (no parameters, no return)
3. **Namespace**: `Stroke.Examples.Prompts` (flat — no sub-namespaces for subdirectories)
4. **XML docs**: `<summary>` with description + Python source reference
5. **Exception handling**: Always catch `KeyboardInterruptException` and `EOFException`
6. **No `using static`**: Import Stroke APIs via `using` declarations only
7. **Max LOC**: 200 lines including blank lines and comments

## Program.cs Routing Contract

```csharp
namespace Stroke.Examples.Prompts;

internal static class Program
{
    private static readonly Dictionary<string, Action> Examples =
        new(StringComparer.OrdinalIgnoreCase)
    {
        // Basic Prompts
        ["get-input"] = GetInput.Run,
        ["get-input-with-default"] = GetInputWithDefault.Run,
        // ... all 56 entries in category order ...

        // Subdirectory examples use path separator
        ["auto-completion/basic-completion"] = Autocompletion.Run,
        ["auto-completion/control-space-trigger"] = ControlSpaceTrigger.Run,
        // ...

        // Backward compatibility aliases
        ["autocompletion"] = Autocompletion.Run,
        ["auto-suggestion"] = AutoSuggestion.Run,
        ["fuzzy-word-completer"] = FuzzyWordCompleterExample.Run,
    };

    public static void Main(string[] args) { /* routing logic */ }
    private static void ShowUsage() { /* sorted example list */ }
}
```

### Routing Name Rules

1. **Format**: kebab-case derived from Python file name
2. **Subdirectories**: Use `/` separator (e.g., `auto-completion/basic-completion`)
3. **Case-insensitive**: `StringComparer.OrdinalIgnoreCase`
4. **Backward compatible**: Existing 4 entries preserved as-is
5. **ShowUsage()**: Lists `Examples.Keys.Order()` alphabetically

## REPL Example Contract (for looping examples)

```csharp
public static class [ReplExampleName]
{
    public static void Run()
    {
        // Setup (session, completer, etc.)
        var session = new PromptSession<string>(/* params */);

        while (true)
        {
            try
            {
                var text = session.Prompt(/* params */);
                // Process result
                Console.WriteLine($"You said: {text}");
            }
            catch (KeyboardInterruptException)
            {
                // Continue loop (or break, per Python behavior)
            }
            catch (EOFException)
            {
                // Ctrl+D exits the loop
                break;
            }
        }
    }
}
```

## Custom Completer Contract (for examples with custom completers)

```csharp
/// <summary>
/// Custom completer for [example name].
/// </summary>
public sealed class [CompleterName] : ICompleter
{
    public IEnumerable<Completion> GetCompletions(
        Document document, CompleteEvent completeEvent)
    {
        // Yield completions
    }
}
```

- Custom completers are defined in the same file as the example
- Use `public sealed class` (defined in the same file as the example)
- Implement `ICompleter` interface
