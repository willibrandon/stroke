using Stroke.Core;
using Stroke.Filters;
using Stroke.KeyBinding;

namespace Stroke.Application.Bindings;

/// <summary>
/// Key binding loaders for Vi editing mode.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>prompt_toolkit.key_binding.bindings.vi</c> module.
/// Provides the <see cref="LoadViBindings"/> factory method that creates all Vi mode key bindings
/// including navigation motions, operators, text objects, mode switches, insert mode bindings,
/// visual mode handlers, macros, digraphs, and miscellaneous commands.
/// </para>
/// <para>
/// This type is stateless and inherently thread-safe. Each factory method creates a new
/// <see cref="KeyBindings"/> instance on each call.
/// </para>
/// </remarks>
public static partial class ViBindings
{
    // ============================================================
    // Condition helpers (module-level @Condition in Python source)
    // ============================================================

    /// <summary>
    /// Dynamic condition: current buffer is returnable (has a return handler).
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>is_returnable</c> condition.
    /// </remarks>
    private static readonly IFilter IsReturnable =
        new Condition(() => AppContext.GetApp().CurrentBuffer.IsReturnable);

    /// <summary>
    /// Dynamic condition: current selection type is Block.
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>in_block_selection</c> condition.
    /// </remarks>
    private static readonly IFilter InBlockSelection =
        new Condition(() =>
        {
            var buff = AppContext.GetApp().CurrentBuffer;
            return buff.SelectionState is not null
                && buff.SelectionState.Type == SelectionType.Block;
        });

    /// <summary>
    /// Dynamic condition: first digraph character has been entered.
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>digraph_symbol_1_given</c> condition.
    /// </remarks>
    private static readonly IFilter DigraphSymbol1Given =
        new Condition(() => AppContext.GetApp().ViState.DigraphSymbol1 is not null);

    /// <summary>
    /// Dynamic condition: search buffer text is empty.
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>search_buffer_is_empty</c> condition.
    /// Note: This is also available as <see cref="SearchFilters.SearchBufferIsEmpty"/>
    /// but is defined here to match the Python source structure where it appears
    /// at module level in <c>vi.py</c>.
    /// </remarks>
    private static readonly IFilter SearchBufferIsEmpty =
        new Condition(() => AppContext.GetApp().CurrentBuffer.Text == "");

    /// <summary>
    /// Dynamic condition: tilde (~) acts as an operator.
    /// When true, ~ is registered as a transform operator via <see cref="ViTransformFunctions"/>.
    /// When false (default), standalone ~ swaps case at cursor and moves right.
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>tilde_operator</c> condition.
    /// Mirrors Vim's <c>tildeop</c> option.
    /// </remarks>
    private static readonly IFilter TildeOperatorFilter =
        new Condition(() => AppContext.GetApp().ViState.TildeOperator);

    // ============================================================
    // Transform functions (vi_transform_functions in Python source)
    // ============================================================

    /// <summary>
    /// Vi transform function definitions used by <c>CreateTransformHandler</c>
    /// to register g,?, g,u, g,U, g,~, and ~ operators.
    /// </summary>
    /// <remarks>
    /// Port of Python Prompt Toolkit's <c>vi_transform_functions</c> list.
    /// Each entry is a tuple of (keys, filter, transform function).
    /// </remarks>
    private static readonly (KeyOrChar[] Keys, IFilter Filter, Func<string, string> Transform)[]
        ViTransformFunctions =
        [
            // Rot13 transformation
            (
                [new KeyOrChar('g'), new KeyOrChar('?')],
                Always.Instance,
                s => new string(s.Select(c =>
                {
                    if (c >= 'a' && c <= 'z') return (char)('a' + (c - 'a' + 13) % 26);
                    if (c >= 'A' && c <= 'Z') return (char)('A' + (c - 'A' + 13) % 26);
                    return c;
                }).ToArray())
            ),
            // To lowercase
            (
                [new KeyOrChar('g'), new KeyOrChar('u')],
                Always.Instance,
                s => s.ToLower()
            ),
            // To uppercase
            (
                [new KeyOrChar('g'), new KeyOrChar('U')],
                Always.Instance,
                s => s.ToUpper()
            ),
            // Swap case
            (
                [new KeyOrChar('g'), new KeyOrChar('~')],
                Always.Instance,
                s => new string(s.Select(c =>
                    char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c)).ToArray())
            ),
            // Tilde as operator (only when TildeOperator is true)
            (
                [new KeyOrChar('~')],
                TildeOperatorFilter,
                s => new string(s.Select(c =>
                    char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c)).ToArray())
            ),
        ];

    /// <summary>
    /// Loads all Vi mode key bindings: navigation motions, operators, text objects,
    /// mode switches, insert mode bindings, visual mode handlers, macros, digraphs,
    /// and miscellaneous commands.
    /// </summary>
    /// <returns>
    /// An <see cref="IKeyBindingsBase"/> wrapping all Vi bindings,
    /// conditional on <see cref="ViFilters.ViMode"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Port of Python Prompt Toolkit's <c>load_vi_bindings()</c>.
    /// </para>
    /// <para>
    /// The returned bindings are wrapped in <see cref="ConditionalKeyBindings"/>
    /// gated on <see cref="ViFilters.ViMode"/> so they are only active when
    /// the application is in Vi editing mode.
    /// </para>
    /// <para>
    /// Individual bindings within are further gated by sub-mode filters
    /// (ViNavigationMode, ViInsertMode, ViSelectionMode, etc.).
    /// </para>
    /// </remarks>
    public static IKeyBindingsBase LoadViBindings()
    {
        var kb = new KeyBindings();

        // Phase 3: Operators (d, c, y, >, <, gq, transforms)
        RegisterOperators(kb);

        // Phase 4: Text objects (word, line, document, bracket, char find, search)
        RegisterTextObjects(kb);

        // Phase 4: Navigation (j, k, arrows, backspace, enter, +, -)
        RegisterNavigation(kb);

        // Phase 6: Mode switching (i, a, o, v, Escape, etc.)
        RegisterModeSwitch(kb);

        // Phase 12: Visual mode (selection extend, operations, toggle)
        RegisterVisualMode(kb);

        // Phase 14: Insert mode (completion, replace, digraph)
        RegisterInsertMode(kb);

        // Phase 13: Miscellaneous (x, X, s, dd, yy, cc, paste, undo, macros, etc.)
        RegisterMisc(kb);

        return new ConditionalKeyBindings(kb, ViFilters.ViMode);
    }

    // ============================================================
    // Partial method call points for registration methods
    // implemented in separate partial class files.
    // ============================================================

    /// <summary>
    /// Registers all Vi operator bindings (d, c, y, >, &lt;, gq, transforms).
    /// </summary>
    static partial void RegisterOperators(KeyBindings kb);

    /// <summary>
    /// Registers all Vi text object bindings (word, line, document, bracket, char find, search).
    /// </summary>
    static partial void RegisterTextObjects(KeyBindings kb);

    /// <summary>
    /// Registers Vi navigation bindings (j, k, arrows, backspace, enter, +, -).
    /// </summary>
    static partial void RegisterNavigation(KeyBindings kb);

    /// <summary>
    /// Registers Vi mode switch bindings (i, a, o, v, Escape, etc.).
    /// </summary>
    static partial void RegisterModeSwitch(KeyBindings kb);

    /// <summary>
    /// Registers Vi visual mode bindings (selection extend, operations, toggle).
    /// </summary>
    static partial void RegisterVisualMode(KeyBindings kb);

    /// <summary>
    /// Registers Vi insert mode bindings (completion, replace, digraph).
    /// </summary>
    static partial void RegisterInsertMode(KeyBindings kb);

    /// <summary>
    /// Registers Vi miscellaneous bindings (x, X, s, dd, yy, cc, paste, undo, macros, etc.).
    /// </summary>
    static partial void RegisterMisc(KeyBindings kb);
}
