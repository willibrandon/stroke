using System.Collections.Concurrent;

namespace Stroke.KeyBinding.Bindings;

/// <summary>
/// Static registry mapping standard Readline command names to executable <see cref="Binding"/> handlers.
/// </summary>
/// <remarks>
/// <para>
/// This class is thread-safe. All registry operations use <see cref="ConcurrentDictionary{TKey, TValue}"/>
/// for lock-free concurrent reads and writes.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.bindings.named_commands</c> module.
/// The Python version maintains a module-level <c>_readline_commands</c> dictionary with a
/// <c>register(name)</c> decorator and a <c>get_by_name(name)</c> lookup function.
/// </para>
/// </remarks>
public static partial class NamedCommands
{
    private static readonly ConcurrentDictionary<string, Binding> _commands = new();

    /// <summary>
    /// Static constructor that registers all 49 built-in Readline commands.
    /// </summary>
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

    /// <summary>
    /// Returns the <see cref="Binding"/> for the Readline command with the given name.
    /// </summary>
    /// <param name="name">The Readline command name (e.g., "forward-char"). Case-sensitive.</param>
    /// <returns>The <see cref="Binding"/> associated with the command name.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no command is registered with the given name.
    /// Message format: <c>Unknown Readline command: '{name}'</c>
    /// </exception>
    public static Binding GetByName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (_commands.TryGetValue(name, out var binding))
        {
            return binding;
        }

        throw new KeyNotFoundException($"Unknown Readline command: '{name}'");
    }

    /// <summary>
    /// Registers a named command handler, creating a <see cref="Binding"/> and adding it to the registry.
    /// If a command with the same name already exists, it is replaced (last-writer-wins semantics).
    /// </summary>
    /// <param name="name">The Readline command name (e.g., "my-custom-cmd").</param>
    /// <param name="handler">The handler function.</param>
    /// <param name="recordInMacro">Whether to record invocations in macro (default: true).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="handler"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is empty or whitespace.</exception>
    public static void Register(string name, KeyHandlerCallable handler, bool recordInMacro = true)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(handler);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Command name cannot be empty or whitespace.", nameof(name));
        }

        RegisterInternal(name, handler, recordInMacro);
    }

    /// <summary>
    /// Internal helper that creates a <see cref="Binding"/> from a handler and adds it to the registry.
    /// Used by both <see cref="Register"/> and the per-category registration methods.
    /// </summary>
    /// <param name="name">The Readline command name.</param>
    /// <param name="handler">The handler function.</param>
    /// <param name="recordInMacro">Whether to record invocations in macro.</param>
    private static void RegisterInternal(string name, KeyHandlerCallable handler, bool recordInMacro = true)
    {
        var binding = new Binding(
            keys: [new KeyOrChar(Input.Keys.Any)],
            handler: handler,
            recordInMacro: new Filters.FilterOrBool(recordInMacro));
        _commands[name] = binding;
    }

    // Partial method stubs — implemented in per-category files.

    /// <summary>Registers the 10 movement commands.</summary>
    static partial void RegisterMovementCommands();

    /// <summary>Registers the 6 history commands.</summary>
    static partial void RegisterHistoryCommands();

    /// <summary>Registers the 9 text modification commands.</summary>
    static partial void RegisterTextEditCommands();

    /// <summary>Registers the 10 kill and yank commands.</summary>
    static partial void RegisterKillYankCommands();

    /// <summary>Registers the 3 completion commands.</summary>
    static partial void RegisterCompletionCommands();

    /// <summary>Registers the 4 macro commands.</summary>
    static partial void RegisterMacroCommands();

    /// <summary>Registers the 7 miscellaneous commands.</summary>
    static partial void RegisterMiscCommands();
}
