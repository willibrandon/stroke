using Stroke.FormattedText;

namespace Stroke.Layout.Processors;

/// <summary>
/// Result of a processor transformation. Contains the transformed fragments
/// and bidirectional position mapping functions.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>Transformation</c> class from
/// <c>prompt_toolkit.layout.processors</c>.
/// </para>
/// <para>
/// This type is immutable. When position mapping functions are not provided,
/// identity functions (i => i) are used as defaults.
/// </para>
/// </remarks>
public sealed class Transformation
{
    private static readonly Func<int, int> Identity = i => i;

    /// <summary>
    /// Initializes a new instance of the <see cref="Transformation"/> class.
    /// </summary>
    /// <param name="fragments">The transformed fragments.</param>
    /// <param name="sourceToDisplay">Source-to-display position mapping. Defaults to identity.</param>
    /// <param name="displayToSource">Display-to-source position mapping. Defaults to identity.</param>
    public Transformation(
        IReadOnlyList<StyleAndTextTuple> fragments,
        Func<int, int>? sourceToDisplay = null,
        Func<int, int>? displayToSource = null)
    {
        Fragments = fragments;
        SourceToDisplay = sourceToDisplay ?? Identity;
        DisplayToSource = displayToSource ?? Identity;
    }

    /// <summary>The transformed fragments.</summary>
    public IReadOnlyList<StyleAndTextTuple> Fragments { get; }

    /// <summary>Source-to-display position mapping. Defaults to identity.</summary>
    public Func<int, int> SourceToDisplay { get; }

    /// <summary>Display-to-source position mapping. Defaults to identity.</summary>
    public Func<int, int> DisplayToSource { get; }
}
