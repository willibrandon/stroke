using Stroke.FormattedText;

namespace Stroke.Shortcuts;

/// <summary>
/// Delegate for generating continuation prompt text in multiline mode.
/// </summary>
/// <param name="promptWidth">The character width of the prompt on the first line.</param>
/// <param name="lineNumber">The current line number (0-based, relative to the input start).</param>
/// <param name="wrapCount">How many times the current line has wrapped.</param>
/// <returns>Formatted text to display as the line prefix.</returns>
/// <remarks>
/// Port of the callable form of Python Prompt Toolkit's <c>PromptContinuationText</c> union type.
/// </remarks>
public delegate AnyFormattedText PromptContinuationCallable(
    int promptWidth, int lineNumber, int wrapCount);
