namespace Stroke.Completion;

/// <summary>
/// Describes how completion was triggered.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's <c>CompleteEvent</c> from
/// <c>prompt_toolkit.completion.base</c>.
/// </remarks>
/// <param name="TextInserted">True if text was just inserted (complete-while-typing).</param>
/// <param name="CompletionRequested">True if user explicitly requested completion (Tab key).</param>
public sealed record CompleteEvent(
    bool TextInserted = false,
    bool CompletionRequested = false);
