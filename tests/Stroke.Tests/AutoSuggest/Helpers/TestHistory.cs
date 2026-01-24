using Stroke.History;

namespace Stroke.Tests.AutoSuggest.Helpers;

/// <summary>
/// Test implementation of IHistory for AutoSuggest tests.
/// </summary>
/// <remarks>
/// This is a real implementation (not a mock) per Constitution VIII.
/// </remarks>
internal sealed class TestHistory : IHistory
{
    private readonly List<string> _strings = [];

    /// <inheritdoc />
    public IReadOnlyList<string> GetStrings() => _strings;

    /// <summary>
    /// Appends a string to the history.
    /// </summary>
    /// <param name="value">The string to append.</param>
    public void AppendString(string value) => _strings.Add(value);
}
