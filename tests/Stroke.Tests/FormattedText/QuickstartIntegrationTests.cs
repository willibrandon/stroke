using Stroke.FormattedText;
using Xunit;

namespace Stroke.Tests.FormattedText;

/// <summary>
/// Integration tests verifying all quickstart.md code examples work correctly.
/// These tests ensure the documentation examples are accurate and functional.
/// </summary>
public class QuickstartIntegrationTests
{
    #region Basic Usage Examples

    [Fact]
    public void BasicUsage_PlainText_ConvertsToFormattedText()
    {
        // From quickstart.md: Plain text (no styling)
        AnyFormattedText plain = "Hello, World!";

        var ft = plain.ToFormattedText();

        Assert.Single(ft);
        Assert.Equal("", ft[0].Style);
        Assert.Equal("Hello, World!", ft[0].Text);
    }

    [Fact]
    public void BasicUsage_HtmlMarkup_ParsesCorrectly()
    {
        // From quickstart.md: HTML-like markup
        var html = new Html("<b>Bold</b> and <i>italic</i>");

        var ft = html.ToFormattedText();

        Assert.Equal(3, ft.Count);
        Assert.Equal("class:b", ft[0].Style);
        Assert.Equal("Bold", ft[0].Text);
        Assert.Equal("", ft[1].Style);
        Assert.Equal(" and ", ft[1].Text);
        Assert.Equal("class:i", ft[2].Style);
        Assert.Equal("italic", ft[2].Text);
    }

    [Fact]
    public void BasicUsage_AnsiEscapeSequences_ParsesCorrectly()
    {
        // From quickstart.md: ANSI escape sequences
        var ansi = new Ansi("\x1b[31mRed\x1b[0m text");

        var ft = ansi.ToFormattedText();

        // ANSI parser creates per-character fragments
        // Check total text content by concatenating
        var text = FormattedTextUtils.FragmentListToText(ft);
        Assert.Equal("Red text", text);

        // Check that red-styled fragments exist for "Red"
        var redFragments = ft.Where(f => f.Style.Contains("ansired")).ToList();
        var redText = string.Concat(redFragments.Select(f => f.Text));
        Assert.Equal("Red", redText);

        // Check that unstyled fragments exist for " text"
        var unstyledFragments = ft.Where(f => f.Style == "").ToList();
        var unstyledText = string.Concat(unstyledFragments.Select(f => f.Text));
        Assert.Equal(" text", unstyledText);
    }

    [Fact]
    public void BasicUsage_ConvertToCanonicalForm_Works()
    {
        // From quickstart.md: Convert to canonical form
        var html = new Html("<b>Bold</b>");
        Stroke.FormattedText.FormattedText ft = FormattedTextUtils.ToFormattedText(html);

        Assert.Single(ft);
        Assert.Equal("class:b", ft[0].Style);
    }

    #endregion

    #region HTML Styling Examples

    [Fact]
    public void HtmlStyling_TextDecoration_AllTypesWork()
    {
        // From quickstart.md: Text decoration
        var styled = new Html("<b>Bold</b> <i>Italic</i> <u>Underline</u> <s>Strike</s>");

        var ft = styled.ToFormattedText();

        Assert.Contains(ft, f => f.Style == "class:b" && f.Text == "Bold");
        Assert.Contains(ft, f => f.Style == "class:i" && f.Text == "Italic");
        Assert.Contains(ft, f => f.Style == "class:u" && f.Text == "Underline");
        Assert.Contains(ft, f => f.Style == "class:s" && f.Text == "Strike");
    }

    [Fact]
    public void HtmlStyling_Colors_FgAndBgWork()
    {
        // From quickstart.md: Colors
        var colored = new Html("<style fg=\"red\" bg=\"blue\">Colored text</style>");

        var ft = colored.ToFormattedText();

        Assert.Single(ft);
        Assert.Contains("fg:red", ft[0].Style);
        Assert.Contains("bg:blue", ft[0].Style);
        Assert.Equal("Colored text", ft[0].Text);
    }

    [Fact]
    public void HtmlStyling_CustomClasses_ProducesClassStyle()
    {
        // From quickstart.md: Custom classes (for your stylesheets)
        var custom = new Html("<username>admin</username>");

        var ft = custom.ToFormattedText();

        Assert.Single(ft);
        Assert.Equal("class:username", ft[0].Style);
        Assert.Equal("admin", ft[0].Text);
    }

    [Fact]
    public void HtmlStyling_NestedElements_CombinesClasses()
    {
        // From quickstart.md: Nested elements
        var nested = new Html("<error><b>Critical!</b></error>");

        var ft = nested.ToFormattedText();

        Assert.Single(ft);
        Assert.Equal("class:error,b", ft[0].Style);
        Assert.Equal("Critical!", ft[0].Text);
    }

    [Fact]
    public void HtmlStyling_SafeInterpolation_EscapesUserInput()
    {
        // From quickstart.md: Safe interpolation (escapes user input)
        string userInput = "<script>alert('xss')</script>";
        var safe = new Html("<b>{0}</b>").Format(userInput);

        var ft = safe.ToFormattedText();
        var text = FormattedTextUtils.FragmentListToText(ft);

        // The < and > in userInput are escaped, so they appear as literal text
        Assert.Contains("<script>", text);
        Assert.Contains("</script>", text);
        Assert.DoesNotContain("&lt;", text); // After decoding, should be literal <
    }

    #endregion

    #region ANSI Parsing Examples

    [Fact]
    public void AnsiParsing_BasicColors_Work()
    {
        // From quickstart.md: Basic colors
        var red = new Ansi("\x1b[31mRed text\x1b[0m");

        var ft = red.ToFormattedText();

        // ANSI parser creates per-character fragments
        var redFragments = ft.Where(f => f.Style.Contains("ansired")).ToList();
        var redText = string.Concat(redFragments.Select(f => f.Text));
        Assert.Equal("Red text", redText);
    }

    [Fact]
    public void AnsiParsing_Bold_Works()
    {
        // From quickstart.md: Bold
        var bold = new Ansi("\x1b[1mBold text\x1b[0m");

        var ft = bold.ToFormattedText();

        // ANSI parser creates per-character fragments
        var boldFragments = ft.Where(f => f.Style.Contains("bold")).ToList();
        var boldText = string.Concat(boldFragments.Select(f => f.Text));
        Assert.Equal("Bold text", boldText);
    }

    [Fact]
    public void AnsiParsing_Combined_BoldAndColor()
    {
        // From quickstart.md: Combined
        var combined = new Ansi("\x1b[1;32mBold green\x1b[0m");

        var ft = combined.ToFormattedText();

        // ANSI parser creates per-character fragments
        var styledFragments = ft.Where(f => f.Style.Contains("bold") && f.Style.Contains("ansigreen")).ToList();
        var styledText = string.Concat(styledFragments.Select(f => f.Text));
        Assert.Equal("Bold green", styledText);
    }

    [Fact]
    public void AnsiParsing_256Colors_Work()
    {
        // From quickstart.md: 256 colors
        var color256 = new Ansi("\x1b[38;5;196mBright red\x1b[0m");

        var ft = color256.ToFormattedText();

        // ANSI parser creates per-character fragments
        var text = FormattedTextUtils.FragmentListToText(ft);
        Assert.Equal("Bright red", text);
    }

    [Fact]
    public void AnsiParsing_TrueColor_Works()
    {
        // From quickstart.md: True color (24-bit RGB)
        var trueColor = new Ansi("\x1b[38;2;255;128;0mOrange\x1b[0m");

        var ft = trueColor.ToFormattedText();

        // ANSI parser creates per-character fragments
        var orangeFragments = ft.Where(f => f.Style.Contains("#ff8000")).ToList();
        var orangeText = string.Concat(orangeFragments.Select(f => f.Text));
        Assert.Equal("Orange", orangeText);
    }

    [Fact]
    public void AnsiParsing_SafeInterpolation_NeutralizesEscapes()
    {
        // From quickstart.md: Safe interpolation (neutralizes escape sequences)
        // Note: Use \u001b instead of \x1b to avoid C# hex escape greediness
        // (C# interprets \x1be as hex 0x1BE, not \x1b + 'e')
        string untrusted = "text\u001bwith\u001bspecial";
        var sanitized = new Ansi("\x1b[1m{0}\x1b[0m").Format(untrusted);

        var ft = sanitized.ToFormattedText();
        var text = FormattedTextUtils.FragmentListToText(ft);

        // Escape characters should be replaced with '?'
        // Verify the result is as expected
        Assert.Equal("text?with?special", text);

        // Verify no ESC characters remain (check by character code)
        Assert.DoesNotContain(text, c => c == '\x1b');
    }

    #endregion

    #region Template Interpolation Examples

    [Fact]
    public void TemplateInterpolation_PreservesFormatting()
    {
        // From quickstart.md: Templates preserve formatting of inserted values
        var template = new Template("Welcome, {}!");
        var result = template.Format(new Html("<b>Admin</b>"));

        var ft = result().ToFormattedText();

        // Result: "Welcome, " + bold "Admin" + "!"
        Assert.Equal(3, ft.Count);
        Assert.Equal("", ft[0].Style);
        Assert.Equal("Welcome, ", ft[0].Text);
        Assert.Equal("class:b", ft[1].Style);
        Assert.Equal("Admin", ft[1].Text);
        Assert.Equal("", ft[2].Style);
        Assert.Equal("!", ft[2].Text);
    }

    [Fact]
    public void TemplateInterpolation_MultiplePlaceholders()
    {
        // From quickstart.md: Multiple placeholders
        var multi = new Template("{} says: {}");
        var messageFunc = multi.Format(
            (AnyFormattedText)new Html("<i>Alice</i>"),
            (AnyFormattedText)"Hello!");

        var ft = messageFunc().ToFormattedText();

        Assert.Equal(3, ft.Count);
        Assert.Equal("class:i", ft[0].Style);
        Assert.Equal("Alice", ft[0].Text);
        Assert.Equal("", ft[1].Style);
        Assert.Equal(" says: ", ft[1].Text);
        Assert.Equal("", ft[2].Style);
        Assert.Equal("Hello!", ft[2].Text);
    }

    #endregion

    #region Utility Functions Examples

    [Fact]
    public void UtilityFunctions_ToPlainText_Works()
    {
        // From quickstart.md: Get plain text
        var fragments = new Stroke.FormattedText.FormattedText(
            new StyleAndTextTuple("class:header", "Title\n"),
            new StyleAndTextTuple("", "Line 1\n"),
            new StyleAndTextTuple("class:footer", "End"));

        string plain = FormattedTextUtils.ToPlainText(fragments);

        Assert.Equal("Title\nLine 1\nEnd", plain);
    }

    [Fact]
    public void UtilityFunctions_FragmentListLen_Works()
    {
        // From quickstart.md: Character count
        var fragments = new Stroke.FormattedText.FormattedText(
            new StyleAndTextTuple("class:header", "Title\n"),
            new StyleAndTextTuple("", "Line 1\n"),
            new StyleAndTextTuple("class:footer", "End"));

        int len = FormattedTextUtils.FragmentListLen(fragments);

        // "Title\n" (6) + "Line 1\n" (7) + "End" (3) = 16 characters
        Assert.Equal(16, len);
    }

    [Fact]
    public void UtilityFunctions_FragmentListWidth_HandlesCjk()
    {
        // From quickstart.md: Display width (handles CJK double-width)
        var cjk = new Stroke.FormattedText.FormattedText(
            new StyleAndTextTuple("", "日本語")); // 3 chars

        int width = FormattedTextUtils.FragmentListWidth(cjk);

        Assert.Equal(6, width); // each CJK char is width 2
    }

    [Fact]
    public void UtilityFunctions_SplitLines_Works()
    {
        // From quickstart.md: Split by lines
        var fragments = new Stroke.FormattedText.FormattedText(
            new StyleAndTextTuple("class:header", "Title\n"),
            new StyleAndTextTuple("", "Line 1\n"),
            new StyleAndTextTuple("class:footer", "End"));

        var lines = FormattedTextUtils.SplitLines(fragments).ToList();

        Assert.Equal(3, lines.Count); // 3 lists, one per line
    }

    [Fact]
    public void UtilityFunctions_Merge_Works()
    {
        // From quickstart.md: Merge multiple formatted texts
        var mergedFunc = FormattedTextUtils.Merge(
            new Html("<b>Hello</b>"),
            (AnyFormattedText)" ",
            new Html("<i>World</i>"));

        var ft = mergedFunc().ToFormattedText();

        Assert.Equal(3, ft.Count);
        Assert.Equal("class:b", ft[0].Style);
        Assert.Equal("Hello", ft[0].Text);
        Assert.Equal("", ft[1].Style);
        Assert.Equal(" ", ft[1].Text);
        Assert.Equal("class:i", ft[2].Style);
        Assert.Equal("World", ft[2].Text);
    }

    #endregion

    #region AnyFormattedText Examples

    [Fact]
    public void AnyFormattedText_PlainString_Works()
    {
        // From quickstart.md: All these are valid AnyFormattedText values
        AnyFormattedText text1 = "Plain string";

        var ft = text1.ToFormattedText();

        Assert.Single(ft);
        Assert.Equal("Plain string", ft[0].Text);
    }

    [Fact]
    public void AnyFormattedText_Html_Works()
    {
        AnyFormattedText text2 = new Html("<b>Bold</b>");

        var ft = text2.ToFormattedText();

        Assert.Single(ft);
        Assert.Equal("class:b", ft[0].Style);
    }

    [Fact]
    public void AnyFormattedText_Ansi_Works()
    {
        AnyFormattedText text3 = new Ansi("\x1b[31mRed\x1b[0m");

        var ft = text3.ToFormattedText();

        // ANSI parser creates per-character fragments
        var text = FormattedTextUtils.FragmentListToText(ft);
        Assert.Equal("Red", text);
    }

    [Fact]
    public void AnyFormattedText_Lazy_Works()
    {
        // From quickstart.md: Lazy evaluation
        Func<AnyFormattedText> lazyFunc = () => "Evaluated";
        AnyFormattedText text4 = lazyFunc;

        var ft = text4.ToFormattedText();

        Assert.Single(ft);
        Assert.Equal("Evaluated", ft[0].Text);
    }

    [Fact]
    public void AnyFormattedText_InCompletionApi_Works()
    {
        // From quickstart.md: Use in APIs
        // Using Completion from Stroke.Completion namespace
        var completion = new Stroke.Completion.Completion(
            text: "hello",
            display: new Html("<b>hello</b>"),  // Implicit conversion
            displayMeta: "greeting");

        Assert.Equal("hello", completion.Text);
        var displayFt = completion.Display!.Value.ToFormattedText();
        Assert.Single(displayFt);
        Assert.Equal("class:b", displayFt[0].Style);
    }

    #endregion

    #region Common Patterns Examples

    [Fact]
    public void CommonPatterns_BuildDynamicContent_Works()
    {
        // From quickstart.md: Building Dynamic Content
        Func<AnyFormattedText> BuildPrompt(string user, bool isAdmin)
        {
            var parts = new List<AnyFormattedText>
            {
                new Html($"<username>{Html.Escape(user)}</username>"),
                (AnyFormattedText)" "
            };

            if (isAdmin)
                parts.Add(new Html("<admin>[ADMIN]</admin> "));

            parts.Add((AnyFormattedText)"> ");

            return FormattedTextUtils.Merge(parts);
        }

        var promptFunc = BuildPrompt("john", true);
        var ft = promptFunc().ToFormattedText();

        Assert.Contains(ft, f => f.Style == "class:username" && f.Text == "john");
        Assert.Contains(ft, f => f.Style == "class:admin" && f.Text == "[ADMIN]");
        Assert.Contains(ft, f => f.Text == "> ");
    }

    [Fact]
    public void CommonPatterns_ProcessExternalOutput_Works()
    {
        // From quickstart.md: Processing External Command Output
        // Simulating git output with ANSI colors
        string gitOutput = "\x1b[32mM\x1b[0m file.txt";
        var formatted = new Ansi(gitOutput);
        var fragments = formatted.ToFormattedText();

        // ANSI parser creates per-character fragments
        // Check green "M"
        var greenFragments = fragments.Where(f => f.Style.Contains("ansigreen")).ToList();
        var greenText = string.Concat(greenFragments.Select(f => f.Text));
        Assert.Equal("M", greenText);

        // Check unstyled " file.txt"
        var unstyledFragments = fragments.Where(f => f.Style == "").ToList();
        var unstyledText = string.Concat(unstyledFragments.Select(f => f.Text));
        Assert.Equal(" file.txt", unstyledText);
    }

    [Fact]
    public void CommonPatterns_ConditionalStyling_Works()
    {
        // From quickstart.md: Conditional Styling
        AnyFormattedText FormatStatus(string status)
        {
            return status switch
            {
                "Success" => new Html("<success>✓ Success</success>"),
                "Warning" => new Html("<warning>⚠ Warning</warning>"),
                "Error" => new Html("<error>✗ Error</error>"),
                _ => (AnyFormattedText)status
            };
        }

        var success = FormatStatus("Success");
        var ft = success.ToFormattedText();
        Assert.Single(ft);
        Assert.Equal("class:success", ft[0].Style);
        Assert.Equal("✓ Success", ft[0].Text);
    }

    #endregion
}
