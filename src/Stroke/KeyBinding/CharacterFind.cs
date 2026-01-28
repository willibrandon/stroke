namespace Stroke.KeyBinding;

/// <summary>
/// Stores the target character and direction for Vi character find operations (f/F/t/T commands).
/// </summary>
/// <remarks>
/// <para>
/// This type is immutable and inherently thread-safe.
/// </para>
/// <para>
/// Equivalent to Python Prompt Toolkit's <c>CharacterFind</c> class from <c>prompt_toolkit.key_binding.vi_state</c>.
/// </para>
/// </remarks>
/// <param name="Character">The target character to find.</param>
/// <param name="Backwards">True for backwards search (F/T), false for forwards search (f/t).</param>
public sealed record CharacterFind(string Character, bool Backwards = false);
