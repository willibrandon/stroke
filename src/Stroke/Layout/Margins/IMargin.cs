using Stroke.FormattedText;
using Stroke.Layout.Controls;
using Stroke.Layout.Windows;

namespace Stroke.Layout.Margins;

/// <summary>
/// Interface for Window margins (line numbers, scrollbars, etc.).
/// </summary>
/// <remarks>
/// <para>
/// Margins appear on the left or right side of a Window, providing
/// supplementary information about the content (line numbers, scroll position, etc.).
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>Margin</c> abstract class from <c>layout/margins.py</c>.
/// </para>
/// </remarks>
public interface IMargin
{
    /// <summary>
    /// Return the width this margin requires.
    /// </summary>
    /// <param name="getUIContent">Function to retrieve the UI content for width calculation.</param>
    /// <returns>The required width in columns.</returns>
    /// <remarks>
    /// The margin width may depend on the content (e.g., line numbers width depends
    /// on the number of lines).
    /// </remarks>
    int GetWidth(Func<UIContent> getUIContent);

    /// <summary>
    /// Create the margin content for rendering.
    /// </summary>
    /// <param name="windowRenderInfo">Render information from the Window.</param>
    /// <param name="width">The available width for the margin.</param>
    /// <param name="height">The available height for the margin.</param>
    /// <returns>Styled text fragments to display in the margin.</returns>
    /// <remarks>
    /// <para>
    /// The returned fragments form a single continuous string. Use newlines to
    /// separate rows. The content should exactly fill the given height.
    /// </para>
    /// </remarks>
    IReadOnlyList<StyleAndTextTuple> CreateMargin(
        WindowRenderInfo windowRenderInfo,
        int width,
        int height);
}
