namespace Stroke.Completion;

/// <summary>
/// Describes how completion was triggered.
/// </summary>
/// <remarks>
/// This is a stub record for Feature 07 (Buffer).
/// Full implementation will be provided in Feature 08 (Completion System).
/// </remarks>
/// <param name="TextInserted">True if text was just inserted (complete-while-typing).</param>
/// <param name="CompletionRequested">True if user explicitly requested completion (Tab key).</param>
public sealed record CompleteEvent(
    bool TextInserted = false,
    bool CompletionRequested = false);
