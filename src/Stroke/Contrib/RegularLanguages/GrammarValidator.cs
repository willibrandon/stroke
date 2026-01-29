using Stroke.Core;
using Stroke.Validation;

namespace Stroke.Contrib.RegularLanguages;

/// <summary>
/// Validator that validates input according to variables in a grammar.
/// Each variable can have its own validator.
/// </summary>
/// <remarks>
/// <para>
/// This validator uses full matching (not prefix matching) because validation
/// requires the actual, unambiguous interpretation of the input.
/// </para>
/// <para>
/// If the input doesn't match the grammar, a "Invalid command" error is thrown
/// with cursor position at the end of the input.
/// </para>
/// <para>
/// For per-variable validators, the error position is adjusted to be relative
/// to the original input (variable start + inner error position).
/// </para>
/// <para>
/// This class is thread-safe; all operations can be called concurrently.
/// </para>
/// </remarks>
public sealed class GrammarValidator : IValidator
{
    private readonly CompiledGrammar _compiledGrammar;
    private readonly IReadOnlyDictionary<string, IValidator> _validators;

    /// <summary>
    /// Create a new GrammarValidator.
    /// </summary>
    /// <param name="compiledGrammar">The compiled grammar to use for matching.</param>
    /// <param name="validators">Dictionary mapping variable names to their validators.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="compiledGrammar"/> is null.
    /// </exception>
    public GrammarValidator(
        CompiledGrammar compiledGrammar,
        IDictionary<string, IValidator>? validators = null)
    {
        ArgumentNullException.ThrowIfNull(compiledGrammar);

        _compiledGrammar = compiledGrammar;
        _validators = validators?.ToDictionary(kv => kv.Key, kv => kv.Value)
            ?? new Dictionary<string, IValidator>();
    }

    /// <inheritdoc/>
    public void Validate(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        // Use Match (not MatchPrefix) for unambiguous interpretation
        var match = _compiledGrammar.Match(document.Text);

        if (match == null)
        {
            throw new ValidationError(document.Text.Length, "Invalid command");
        }

        foreach (var v in match.Variables())
        {
            if (!_validators.TryGetValue(v.VarName, out var validator))
            {
                continue;
            }

            // Unescape the variable value
            var unwrappedText = v.Value;
            var innerDocument = new Document(unwrappedText, unwrappedText.Length);

            try
            {
                validator.Validate(innerDocument);
            }
            catch (ValidationError e)
            {
                // Adjust cursor position to be relative to original input
                throw new ValidationError(
                    cursorPosition: v.Start + e.CursorPosition,
                    message: e.Message);
            }
        }
    }

    /// <inheritdoc/>
    public async ValueTask ValidateAsync(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        // Use Match (not MatchPrefix) for unambiguous interpretation
        var match = _compiledGrammar.Match(document.Text);

        if (match == null)
        {
            throw new ValidationError(document.Text.Length, "Invalid command");
        }

        foreach (var v in match.Variables())
        {
            if (!_validators.TryGetValue(v.VarName, out var validator))
            {
                continue;
            }

            // Unescape the variable value
            var unwrappedText = v.Value;
            var innerDocument = new Document(unwrappedText, unwrappedText.Length);

            try
            {
                await validator.ValidateAsync(innerDocument);
            }
            catch (ValidationError e)
            {
                // Adjust cursor position to be relative to original input
                throw new ValidationError(
                    cursorPosition: v.Start + e.CursorPosition,
                    message: e.Message);
            }
        }
    }
}
