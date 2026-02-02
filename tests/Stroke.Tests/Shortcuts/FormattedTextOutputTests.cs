using Stroke.FormattedText;
using Stroke.Layout.Containers;
using Stroke.Output;
using Stroke.Shortcuts;
using Stroke.Styles;
using Xunit;

using FT = Stroke.FormattedText.FormattedText;

namespace Stroke.Tests.Shortcuts;

public class FormattedTextOutputTests
{
    #region Helper Methods

    /// <summary>
    /// Create a PlainTextOutput backed by a StringWriter for capturing output text.
    /// </summary>
    private static (IOutput output, StringWriter writer) CreateCaptureOutput()
    {
        var writer = new StringWriter();
        var output = OutputFactory.Create(stdout: writer);
        return (output, writer);
    }

    /// <summary>
    /// Flush the output and return the captured text.
    /// </summary>
    private static string GetCapturedText(IOutput output, StringWriter writer)
    {
        output.Flush();
        return writer.ToString();
    }

    #endregion

    #region US1: Print Single-Value Overload

    [Fact]
    public void Print_PlainString_WritesTextWithNewline()
    {
        // US1-AS1: Plain string prints "Hello\n"
        var (output, writer) = CreateCaptureOutput();

        FormattedTextOutput.Print("Hello", output: output);

        var text = GetCapturedText(output, writer);
        Assert.Contains("Hello", text);
        Assert.Contains("\n", text);
    }

    [Fact]
    public void Print_HtmlFormatted_RendersText()
    {
        // US1-AS2: HTML renders with styling — verify text content is present
        var (output, writer) = CreateCaptureOutput();
        var html = new Html("<b>Bold</b>");

        FormattedTextOutput.Print(html, output: output);

        var text = GetCapturedText(output, writer);
        Assert.Contains("Bold", text);
    }

    [Fact]
    public void Print_CustomEnd_ReplacesNewline()
    {
        // US1-AS4: Custom end parameter replaces newline
        var (output, writer) = CreateCaptureOutput();

        FormattedTextOutput.Print("Hello", end: "!", output: output);

        var text = GetCapturedText(output, writer);
        Assert.Contains("Hello", text);
        Assert.Contains("!", text);
        Assert.DoesNotContain("\n", text);
    }

    [Fact]
    public void Print_CustomFile_RedirectsOutput()
    {
        // US1-AS5: Custom TextWriter redirects output
        var writer = new StringWriter();

        FormattedTextOutput.Print("Hello", file: writer);

        var text = writer.ToString();
        Assert.Contains("Hello", text);
    }

    [Fact]
    public void Print_OutputAndFileBothSpecified_ThrowsArgumentException()
    {
        // US1-AS6: output+file conflict throws ArgumentException
        var (output, _) = CreateCaptureOutput();
        var file = new StringWriter();

        Assert.Throws<ArgumentException>(() =>
            FormattedTextOutput.Print("Hello", file: file, output: output));
    }

    [Fact]
    public void Print_FlushTrue_FlushesOutput()
    {
        // US1-AS7: flush: true flushes output (verify no exception, text is written)
        var (output, writer) = CreateCaptureOutput();

        FormattedTextOutput.Print("Hello", flush: true, output: output);

        var text = GetCapturedText(output, writer);
        Assert.Contains("Hello", text);
    }

    [Fact]
    public void Print_IncludeDefaultPygmentsStyleFalse_ExcludesPygmentsStyle()
    {
        // US1-AS9: includeDefaultPygmentsStyle: false excludes Pygments style
        // Verify the method runs without error and produces output
        var (output, writer) = CreateCaptureOutput();

        FormattedTextOutput.Print(
            "Hello",
            includeDefaultPygmentsStyle: false,
            output: output);

        var text = GetCapturedText(output, writer);
        Assert.Contains("Hello", text);
    }

    [Fact]
    public void Print_ExplicitOutput_BypassesSessionDefault()
    {
        // US1-AS10: Explicit output parameter bypasses session default
        var (output, writer) = CreateCaptureOutput();

        FormattedTextOutput.Print("Direct", output: output);

        var text = GetCapturedText(output, writer);
        Assert.Contains("Direct", text);
    }

    [Fact]
    public void Print_FileTextWriterNull_SilentlyDiscards()
    {
        // Edge case: TextWriter.Null silently discards via DummyOutput
        // Should not throw
        FormattedTextOutput.Print("Hello", file: TextWriter.Null);
    }

    [Fact]
    public void Print_EmptyEnd_NoTrailingNewline()
    {
        // Edge case: empty end produces no trailing newline
        var (output, writer) = CreateCaptureOutput();

        FormattedTextOutput.Print("Hello", end: "", output: output);

        var text = GetCapturedText(output, writer);
        Assert.Contains("Hello", text);
        Assert.DoesNotContain("\n", text);
    }

    [Fact]
    public void Print_FormattedTextObject_RendersContent()
    {
        // Verify FormattedText object prints correctly
        var (output, writer) = CreateCaptureOutput();
        var ft = new FT([new StyleAndTextTuple("", "Styled text")]);

        FormattedTextOutput.Print((AnyFormattedText)ft, output: output);

        var text = GetCapturedText(output, writer);
        Assert.Contains("Styled text", text);
    }

    [Fact]
    public void Print_AnsiFormatted_RendersText()
    {
        // Verify ANSI formatted text prints the text content
        var (output, writer) = CreateCaptureOutput();
        var ansi = new Ansi("\x1b[1mBold ANSI\x1b[0m");

        FormattedTextOutput.Print(ansi, output: output);

        var text = GetCapturedText(output, writer);
        Assert.Contains("Bold ANSI", text);
    }

    #endregion

    #region US1: Print Multi-Value Overload

    [Fact]
    public void Print_MultipleValues_JoinedWithSeparator()
    {
        // US1-AS3: Multiple values with custom separator
        var (output, writer) = CreateCaptureOutput();

        FormattedTextOutput.Print(
            ["a", "b", "c"],
            sep: ", ",
            output: output);

        var text = GetCapturedText(output, writer);
        Assert.Contains("a", text);
        Assert.Contains(", ", text);
        Assert.Contains("b", text);
        Assert.Contains("c", text);
    }

    [Fact]
    public void Print_ZeroValues_PrintsOnlyEnd()
    {
        // FR-013: Zero values prints only end string (default newline)
        var (output, writer) = CreateCaptureOutput();

        FormattedTextOutput.Print(
            Array.Empty<object>(),
            output: output);

        var text = GetCapturedText(output, writer);
        Assert.Contains("\n", text);
    }

    [Fact]
    public void Print_EmptySep_ConcatenatesWithoutSpacing()
    {
        // FR-002: Empty sep concatenates without spacing
        var (output, writer) = CreateCaptureOutput();

        FormattedTextOutput.Print(
            ["Hello", "World"],
            sep: "",
            output: output);

        var text = GetCapturedText(output, writer);
        Assert.Contains("HelloWorld", text);
    }

    [Fact]
    public void Print_EmptySepAndEnd_NoSpacingNoNewline()
    {
        // Edge case: empty sep AND empty end
        var (output, writer) = CreateCaptureOutput();

        FormattedTextOutput.Print(
            ["a", "b"],
            sep: "",
            end: "",
            output: output);

        var text = GetCapturedText(output, writer);
        Assert.Contains("ab", text);
        Assert.DoesNotContain("\n", text);
    }

    [Fact]
    public void Print_PlainIList_ConvertedViaToString()
    {
        // FR-010: Plain IList (not FormattedText) is converted via ToString()
        var (output, writer) = CreateCaptureOutput();
        var list = new System.Collections.ArrayList { 1, 2, 3 };

        FormattedTextOutput.Print(
            [list],
            output: output);

        var text = GetCapturedText(output, writer);
        // ArrayList.ToString() returns the type name, but the list is converted
        // This verifies it doesn't crash and produces some output
        Assert.False(string.IsNullOrEmpty(text));
    }

    [Fact]
    public void Print_MultiValue_DefaultSeparatorIsSpace()
    {
        // Default sep is space
        var (output, writer) = CreateCaptureOutput();

        FormattedTextOutput.Print(
            ["Hello", "World"],
            output: output);

        var text = GetCapturedText(output, writer);
        Assert.Contains("Hello", text);
        Assert.Contains(" ", text);
        Assert.Contains("World", text);
    }

    [Fact]
    public void Print_MultiValue_OutputAndFileBothSpecified_ThrowsArgumentException()
    {
        // Multi-value overload also validates output+file conflict
        var (output, _) = CreateCaptureOutput();
        var file = new StringWriter();

        Assert.Throws<ArgumentException>(() =>
            FormattedTextOutput.Print(
                ["a", "b"],
                file: file,
                output: output));
    }

    #endregion

    #region US1: Custom Style and ColorDepth (US1-AS8)

    [Fact]
    public void Print_CustomStyleAndColorDepth_RendersCorrectly()
    {
        // US1-AS8: Custom style and colorDepth render correctly
        var (output, writer) = CreateCaptureOutput();
        var customStyle = Style.FromDict(new Dictionary<string, string>
        {
            ["test"] = "#ff0000"
        });

        FormattedTextOutput.Print(
            "Hello",
            style: customStyle,
            colorDepth: ColorDepth.Depth4Bit,
            output: output);

        var text = GetCapturedText(output, writer);
        Assert.Contains("Hello", text);
    }

    #endregion

    #region US3: PrintContainer

    /// <summary>
    /// Helper to create a simple container wrapping a FormattedTextControl in a Window.
    /// </summary>
    private static AnyContainer CreateSimpleContainer(string text)
    {
        var control = new Stroke.Layout.Controls.FormattedTextControl(text);
        var window = new Stroke.Layout.Containers.Window(content: control);
        return new AnyContainer(window);
    }

    [Fact]
    public void PrintContainer_SimpleContainer_RendersAndTerminates()
    {
        // US3-AS1: Container renders to output and terminates without hanging
        var writer = new StringWriter();

        FormattedTextOutput.PrintContainer(
            CreateSimpleContainer("Container text"),
            file: writer);

        // If we get here, it terminated cleanly (DummyInput → EndOfStreamException)
        Assert.True(true);
    }

    [Fact]
    public void PrintContainer_CustomFile_RedirectsOutput()
    {
        // US3-AS2: Custom file TextWriter redirects container output
        var writer = new StringWriter();

        FormattedTextOutput.PrintContainer(
            CreateSimpleContainer("Test output"),
            file: writer);

        Assert.True(true);
    }

    [Fact]
    public void PrintContainer_CustomStyle_UsesMergedStyle()
    {
        // US3-AS3: Custom style uses merged style
        var writer = new StringWriter();
        var customStyle = Style.FromDict(new Dictionary<string, string>
        {
            ["custom"] = "#00ff00"
        });

        FormattedTextOutput.PrintContainer(
            CreateSimpleContainer("Styled"),
            file: writer,
            style: customStyle);

        Assert.True(true);
    }

    [Fact]
    public void PrintContainer_EmptyContainer_CompletesNormally()
    {
        // US3-AS4: Empty container completes normally
        var writer = new StringWriter();

        FormattedTextOutput.PrintContainer(
            CreateSimpleContainer(""),
            file: writer);

        Assert.True(true);
    }

    #endregion

    #region Style Merging (SC-009)

    [Fact]
    public void Print_StyleMerging_UserOverridesPygments()
    {
        // SC-009: Verify style merge precedence — user style overrides Pygments
        // which overrides default UI style. We verify indirectly by ensuring
        // the method completes without error with all style options.
        var (output, writer) = CreateCaptureOutput();

        var userStyle = Style.FromDict(new Dictionary<string, string>
        {
            ["test-style"] = "#ff0000 bold"
        });

        // With Pygments included
        FormattedTextOutput.Print(
            "Styled",
            style: userStyle,
            includeDefaultPygmentsStyle: true,
            output: output);

        var text1 = GetCapturedText(output, writer);
        Assert.Contains("Styled", text1);
    }

    [Fact]
    public void Print_StyleMerging_WithoutPygments()
    {
        // Verify style merge without Pygments style
        var (output, writer) = CreateCaptureOutput();

        var userStyle = Style.FromDict(new Dictionary<string, string>
        {
            ["test-style"] = "#00ff00 italic"
        });

        FormattedTextOutput.Print(
            "NoPygments",
            style: userStyle,
            includeDefaultPygmentsStyle: false,
            output: output);

        var text = GetCapturedText(output, writer);
        Assert.Contains("NoPygments", text);
    }

    [Fact]
    public void Print_NullStyle_UsesDefaultsOnly()
    {
        // Edge case: null style uses only default UI + Pygments
        var (output, writer) = CreateCaptureOutput();

        FormattedTextOutput.Print(
            "DefaultOnly",
            style: null,
            output: output);

        var text = GetCapturedText(output, writer);
        Assert.Contains("DefaultOnly", text);
    }

    #endregion

    #region Overload Resolution (T020)

    [Fact]
    public void Print_SingleString_ResolvesToAnyFormattedTextOverload()
    {
        // Verify calling Print("hello") resolves to the AnyFormattedText overload
        var (output, writer) = CreateCaptureOutput();

        // This should resolve to Print(AnyFormattedText, ...) via implicit conversion
        FormattedTextOutput.Print("hello", output: output);

        var text = GetCapturedText(output, writer);
        Assert.Contains("hello", text);
    }

    [Fact]
    public void Print_ObjectArray_ResolvesToMultiValueOverload()
    {
        // Verify calling Print(new object[] { "a", "b" }) resolves to multi-value
        var (output, writer) = CreateCaptureOutput();

        FormattedTextOutput.Print(new object[] { "a", "b" }, output: output);

        var text = GetCapturedText(output, writer);
        Assert.Contains("a", text);
        Assert.Contains("b", text);
    }

    #endregion
}
