using System.Runtime.CompilerServices;
using Stroke.Core;

namespace Stroke.Completion;

/// <summary>
/// Abstract base class for completion providers with default async implementation.
/// </summary>
/// <remarks>
/// <para>
/// This class provides a default implementation of <see cref="ICompleter.GetCompletionsAsync"/>
/// that wraps the synchronous <see cref="GetCompletions"/> method. Subclasses that have
/// native async behavior can override <see cref="GetCompletionsAsync"/>.
/// </para>
/// <para>
/// All completers derived from this class are expected to be stateless or immutable,
/// making them inherently thread-safe per Constitution XI.
/// </para>
/// </remarks>
public abstract class CompleterBase : ICompleter
{
    /// <summary>
    /// Gets completions for the given document synchronously.
    /// </summary>
    /// <param name="document">The current document (immutable).</param>
    /// <param name="completeEvent">Event describing how completion was triggered.</param>
    /// <returns>An enumerable of completion suggestions.</returns>
    public abstract IEnumerable<Completion> GetCompletions(Document document, CompleteEvent completeEvent);

    /// <summary>
    /// Gets completions for the given document asynchronously.
    /// </summary>
    /// <remarks>
    /// The default implementation wraps the synchronous <see cref="GetCompletions"/> method.
    /// Subclasses MAY override this method to provide native async behavior for
    /// operations like network requests or database queries.
    /// </remarks>
    /// <param name="document">The current document (immutable).</param>
    /// <param name="completeEvent">Event describing how completion was triggered.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>An async enumerable of completion suggestions.</returns>
    public virtual async IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document,
        CompleteEvent completeEvent,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var completion in GetCompletions(document, completeEvent))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return completion;
        }

        await Task.CompletedTask;
    }
}
