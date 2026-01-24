using Stroke.Core;
using Stroke.History;

namespace Stroke.Tests.AutoSuggest.Helpers;

/// <summary>
/// Test implementation of IBuffer for AutoSuggest tests.
/// </summary>
/// <remarks>
/// This is a real implementation (not a mock) per Constitution VIII.
/// </remarks>
internal sealed class TestBuffer : IBuffer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestBuffer"/> class.
    /// </summary>
    /// <param name="history">The history to use.</param>
    public TestBuffer(IHistory history)
    {
        History = history;
    }

    /// <inheritdoc />
    public Document Document { get; set; } = new();

    /// <inheritdoc />
    public IHistory History { get; }
}
