namespace Stroke.Core;

/// <summary>
/// A no-op disposable for optional context manager scenarios.
/// </summary>
/// <remarks>
/// <para>
/// Use this when an API expects an <see cref="IDisposable"/> but no cleanup is actually needed.
/// The <see cref="Dispose"/> method does nothing.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>DummyContext</c> class from <c>utils.py</c>.
/// In Python, this is a context manager for use with <c>with</c> statements.
/// In C#, this is an <see cref="IDisposable"/> for use with <c>using</c> statements.
/// </para>
/// <para>
/// This type is thread-safe (stateless singleton).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Use when a method requires an IDisposable but you don't need cleanup
/// IDisposable context = someCondition ? GetRealContext() : DummyContext.Instance;
/// using (context)
/// {
///     // Do work
/// }
/// </code>
/// </example>
public sealed class DummyContext : IDisposable
{
    /// <summary>
    /// Gets the singleton instance of <see cref="DummyContext"/>.
    /// </summary>
    public static DummyContext Instance { get; } = new();

    /// <summary>
    /// Private constructor to enforce singleton pattern.
    /// </summary>
    private DummyContext()
    {
    }

    /// <summary>
    /// Performs no operation. This method exists only to satisfy the <see cref="IDisposable"/> interface.
    /// </summary>
    public void Dispose()
    {
        // No-op - nothing to dispose
    }
}
