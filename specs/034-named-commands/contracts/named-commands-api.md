# API Contracts: Named Commands

**Feature**: 034-named-commands
**Date**: 2026-01-30

## Namespace

`Stroke.KeyBinding.Bindings`

## NamedCommands Static Class

```csharp
namespace Stroke.KeyBinding.Bindings;

/// <summary>
/// Static registry mapping standard Readline command names to executable Binding handlers.
/// </summary>
/// <remarks>
/// <para>
/// This class is thread-safe. All registry operations are synchronized via ConcurrentDictionary.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.bindings.named_commands</c> module.
/// </para>
/// </remarks>
public static partial class NamedCommands
{
    /// <summary>
    /// Returns the Binding for the Readline command with the given name.
    /// </summary>
    /// <param name="name">The Readline command name (e.g., "forward-char").</param>
    /// <returns>The Binding associated with the command name.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no command is registered with the given name.
    /// Message format: "Unknown Readline command: '{name}'"
    /// </exception>
    /// <exception cref="ArgumentNullException">Thrown when name is null.</exception>
    public static Binding GetByName(string name);

    /// <summary>
    /// Registers a named command handler, creating a Binding and adding it to the registry.
    /// If a command with the same name already exists, it is replaced.
    /// </summary>
    /// <param name="name">The Readline command name (e.g., "my-custom-cmd").</param>
    /// <param name="handler">The handler function.</param>
    /// <param name="recordInMacro">Whether to record invocations in macro (default: true).</param>
    /// <exception cref="ArgumentNullException">Thrown when name or handler is null.</exception>
    /// <exception cref="ArgumentException">Thrown when name is empty or whitespace.</exception>
    public static void Register(string name, KeyHandlerCallable handler, bool recordInMacro = true);
}
```

## Built-in Command Handlers

All handlers are `private static` methods within the `NamedCommands` partial class, registered during static construction.

### Movement Commands (NamedCommands.Movement.cs)

```csharp
// Each handler signature:
private static NotImplementedOrNone? BeginningOfBuffer(KeyPressEvent @event);
private static NotImplementedOrNone? EndOfBuffer(KeyPressEvent @event);
private static NotImplementedOrNone? BeginningOfLine(KeyPressEvent @event);
private static NotImplementedOrNone? EndOfLine(KeyPressEvent @event);
private static NotImplementedOrNone? ForwardChar(KeyPressEvent @event);
private static NotImplementedOrNone? BackwardChar(KeyPressEvent @event);
private static NotImplementedOrNone? ForwardWord(KeyPressEvent @event);
private static NotImplementedOrNone? BackwardWord(KeyPressEvent @event);
private static NotImplementedOrNone? ClearScreen(KeyPressEvent @event);
private static NotImplementedOrNone? RedrawCurrentLine(KeyPressEvent @event);
```

### History Commands (NamedCommands.History.cs)

```csharp
private static NotImplementedOrNone? AcceptLine(KeyPressEvent @event);
private static NotImplementedOrNone? PreviousHistory(KeyPressEvent @event);
private static NotImplementedOrNone? NextHistory(KeyPressEvent @event);
private static NotImplementedOrNone? BeginningOfHistory(KeyPressEvent @event);
private static NotImplementedOrNone? EndOfHistory(KeyPressEvent @event);
private static NotImplementedOrNone? ReverseSearchHistory(KeyPressEvent @event);
```

### Text Modification Commands (NamedCommands.TextEdit.cs)

```csharp
private static NotImplementedOrNone? EndOfFile(KeyPressEvent @event);
private static NotImplementedOrNone? DeleteChar(KeyPressEvent @event);
private static NotImplementedOrNone? BackwardDeleteChar(KeyPressEvent @event);
private static NotImplementedOrNone? SelfInsert(KeyPressEvent @event);
private static NotImplementedOrNone? TransposeChars(KeyPressEvent @event);
private static NotImplementedOrNone? UppercaseWord(KeyPressEvent @event);
private static NotImplementedOrNone? DowncaseWord(KeyPressEvent @event);
private static NotImplementedOrNone? CapitalizeWord(KeyPressEvent @event);
private static NotImplementedOrNone? QuotedInsert(KeyPressEvent @event);
```

### Kill and Yank Commands (NamedCommands.KillYank.cs)

```csharp
private static NotImplementedOrNone? KillLine(KeyPressEvent @event);
private static NotImplementedOrNone? KillWord(KeyPressEvent @event);
private static NotImplementedOrNone? UnixWordRuboutImpl(KeyPressEvent @event, bool word);  // Internal helper, NOT registered directly
private static NotImplementedOrNone? BackwardKillWord(KeyPressEvent @event);
private static NotImplementedOrNone? DeleteHorizontalSpace(KeyPressEvent @event);
private static NotImplementedOrNone? UnixLineDiscard(KeyPressEvent @event);
private static NotImplementedOrNone? Yank(KeyPressEvent @event);
private static NotImplementedOrNone? YankNthArg(KeyPressEvent @event);
private static NotImplementedOrNone? YankLastArg(KeyPressEvent @event);
private static NotImplementedOrNone? YankPop(KeyPressEvent @event);
```

**Note**: `UnixWordRuboutImpl` is an internal helper method with an extra `word` parameter (matching Python's `WORD` parameter). It does NOT match the `KeyHandlerCallable` delegate signature directly. The registered handler for `"unix-word-rubout"` is a lambda/wrapper: `e => UnixWordRuboutImpl(e, word: true)`. The registered handler for `"backward-kill-word"` delegates via `BackwardKillWord` which calls `UnixWordRuboutImpl(@event, word: false)`.

### Completion Commands (NamedCommands.Completion.cs)

```csharp
private static NotImplementedOrNone? Complete(KeyPressEvent @event);
private static NotImplementedOrNone? MenuComplete(KeyPressEvent @event);
private static NotImplementedOrNone? MenuCompleteBackward(KeyPressEvent @event);
```

### Macro Commands (NamedCommands.Macro.cs)

```csharp
private static NotImplementedOrNone? StartKbdMacro(KeyPressEvent @event);
private static NotImplementedOrNone? EndKbdMacro(KeyPressEvent @event);
private static NotImplementedOrNone? CallLastKbdMacro(KeyPressEvent @event);  // RecordInMacro=false
private static NotImplementedOrNone? PrintLastKbdMacro(KeyPressEvent @event);
```

### Miscellaneous Commands (NamedCommands.Misc.cs)

```csharp
private static NotImplementedOrNone? Undo(KeyPressEvent @event);
private static NotImplementedOrNone? InsertComment(KeyPressEvent @event);
private static NotImplementedOrNone? ViEditingMode(KeyPressEvent @event);
private static NotImplementedOrNone? EmacsEditingMode(KeyPressEvent @event);
private static NotImplementedOrNone? PrefixMeta(KeyPressEvent @event);
private static NotImplementedOrNone? OperateAndGetNext(KeyPressEvent @event);
private static NotImplementedOrNone? EditAndExecuteCommand(KeyPressEvent @event);
```

## CompletionBindings Static Class

Port of `prompt_toolkit.key_binding.bindings.completion` (public functions only).

```csharp
namespace Stroke.KeyBinding.Bindings;

/// <summary>
/// Key binding handlers for displaying completions.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.bindings.completion</c> module.
/// </remarks>
public static class CompletionBindings
{
    /// <summary>
    /// Tab-completion: first tab completes common suffix, second tab lists all completions.
    /// </summary>
    /// <param name="event">The key press event.</param>
    public static void GenerateCompletions(KeyPressEvent @event);

    /// <summary>
    /// Readline-style tab completion. Generates completions immediately (blocking)
    /// and displays them above the prompt in columns.
    /// </summary>
    /// <param name="event">The key press event.</param>
    public static void DisplayCompletionsLikeReadline(KeyPressEvent @event);
}
```

## Helper Extension

```csharp
namespace Stroke.KeyBinding.Bindings;

/// <summary>
/// Extension methods for KeyPressEvent to provide typed Application access.
/// </summary>
internal static class KeyPressEventExtensions
{
    /// <summary>
    /// Gets the Application instance from the event, cast to the appropriate type.
    /// </summary>
    /// <exception cref="InvalidOperationException">App is null or not an Application.</exception>
    internal static Application.Application<object> GetApp(this KeyPressEvent @event);
}
```

## Registration Pattern

Each partial class file registers its commands in the static constructor:

```csharp
// In NamedCommands.cs (main file)
static NamedCommands()
{
    RegisterMovementCommands();
    RegisterHistoryCommands();
    RegisterTextEditCommands();
    RegisterKillYankCommands();
    RegisterCompletionCommands();
    RegisterMacroCommands();
    RegisterMiscCommands();
}

// In NamedCommands.Movement.cs
private static void RegisterMovementCommands()
{
    RegisterInternal("beginning-of-buffer", BeginningOfBuffer);
    RegisterInternal("end-of-buffer", EndOfBuffer);
    // ... etc.
}
```

Where `RegisterInternal` creates a `Binding` with `[Keys.Any]` and default settings, then adds to the dictionary.

## Error Behavior

| Scenario | Exception | Message |
|----------|-----------|---------|
| `GetByName(null)` | `ArgumentNullException` | "Value cannot be null. (Parameter 'name')" |
| `GetByName("")` | `KeyNotFoundException` | "Unknown Readline command: ''" |
| `GetByName("nonexistent")` | `KeyNotFoundException` | "Unknown Readline command: 'nonexistent'" |
| `Register(null, handler)` | `ArgumentNullException` | "Value cannot be null. (Parameter 'name')" |
| `Register("name", null)` | `ArgumentNullException` | "Value cannot be null. (Parameter 'handler')" |
| `Register("", handler)` | `ArgumentException` | "Command name cannot be empty or whitespace." |
