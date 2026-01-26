namespace Stroke.FormattedText;

/// <summary>
/// Represents any object that can be converted to formatted text.
/// </summary>
/// <remarks>
/// <para>
/// This interface is the C# equivalent of Python Prompt Toolkit's
/// <c>__pt_formatted_text__</c> magic method protocol.
/// </para>
/// <para>
/// Any class implementing this interface can be used wherever formatted text is expected.
/// The <see cref="ToFormattedText"/> method returns the canonical representation
/// as a read-only list of styled text fragments.
/// </para>
/// </remarks>
public interface IFormattedText
{
    /// <summary>
    /// Converts this object to a list of styled text fragments.
    /// </summary>
    /// <returns>A read-only list of style and text tuples.</returns>
    IReadOnlyList<StyleAndTextTuple> ToFormattedText();
}
