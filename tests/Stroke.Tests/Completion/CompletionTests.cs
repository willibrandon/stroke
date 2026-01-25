using Stroke.Completion;
using Stroke.FormattedText;
using Xunit;
using CompletionItem = Stroke.Completion.Completion;
using FText = Stroke.FormattedText.FormattedText;

namespace Stroke.Tests.Completion;

/// <summary>
/// Tests for <see cref="Completion"/>.
/// </summary>
public sealed class CompletionTests
{
    [Fact]
    public void Constructor_WithText_CreatesCompletion()
    {
        var completion = new CompletionItem("hello");

        Assert.Equal("hello", completion.Text);
        Assert.Equal(0, completion.StartPosition);
        Assert.Null(completion.Display);
        Assert.Null(completion.DisplayMeta);
        Assert.Equal("", completion.Style);
        Assert.Equal("", completion.SelectedStyle);
    }

    [Fact]
    public void Constructor_WithAllParameters_SetsAll()
    {
        AnyFormattedText display = "display text";
        AnyFormattedText meta = "meta info";

        var completion = new CompletionItem(
            text: "text",
            startPosition: -5,
            display: display,
            displayMeta: meta,
            style: "style1",
            selectedStyle: "style2");

        Assert.Equal("text", completion.Text);
        Assert.Equal(-5, completion.StartPosition);
        Assert.NotNull(completion.Display);
        Assert.NotNull(completion.DisplayMeta);
        Assert.Equal("style1", completion.Style);
        Assert.Equal("style2", completion.SelectedStyle);
    }

    [Fact]
    public void Constructor_StartPosition_Zero_IsValid()
    {
        var completion = new CompletionItem("text", startPosition: 0);

        Assert.Equal(0, completion.StartPosition);
    }

    [Fact]
    public void Constructor_StartPosition_Negative_IsValid()
    {
        var completion = new CompletionItem("text", startPosition: -10);

        Assert.Equal(-10, completion.StartPosition);
    }

    [Fact]
    public void Constructor_StartPosition_Positive_ThrowsArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => new CompletionItem("text", startPosition: 1));

        Assert.Equal("startPosition", ex.ParamName);
        Assert.Contains("must be <= 0", ex.Message);
    }

    [Fact]
    public void DisplayText_WhenDisplayIsSet_ReturnsDisplay()
    {
        AnyFormattedText display = "custom display";
        var completion = new CompletionItem("text", display: display);

        var displayText = completion.DisplayText;

        Assert.Equal("custom display", displayText.ToPlainText());
    }

    [Fact]
    public void DisplayText_WhenDisplayIsNull_ReturnsText()
    {
        var completion = new CompletionItem("text");

        var displayText = completion.DisplayText;

        Assert.Equal("text", displayText.ToPlainText());
    }

    [Fact]
    public void DisplayMetaText_WhenDisplayMetaIsSet_ReturnsDisplayMeta()
    {
        AnyFormattedText meta = "meta info";
        var completion = new CompletionItem("text", displayMeta: meta);

        var metaText = completion.DisplayMetaText;

        Assert.Equal("meta info", metaText.ToPlainText());
    }

    [Fact]
    public void DisplayMetaText_WhenDisplayMetaIsNull_ReturnsEmpty()
    {
        var completion = new CompletionItem("text");

        var metaText = completion.DisplayMetaText;

        Assert.True(metaText.IsEmpty);
    }

    [Fact]
    public void NewCompletionFromPosition_SubtractsFromStartPosition()
    {
        var completion = new CompletionItem("text", startPosition: -5);

        var newCompletion = completion.NewCompletionFromPosition(3);

        Assert.Equal(-8, newCompletion.StartPosition);
    }

    [Fact]
    public void NewCompletionFromPosition_PreservesOtherProperties()
    {
        AnyFormattedText display = "display";
        AnyFormattedText meta = "meta";
        var completion = new CompletionItem("text", startPosition: -5, display: display, displayMeta: meta, style: "s1", selectedStyle: "s2");

        var newCompletion = completion.NewCompletionFromPosition(3);

        Assert.Equal("text", newCompletion.Text);
        Assert.NotNull(newCompletion.Display);
        Assert.NotNull(newCompletion.DisplayMeta);
        Assert.Equal("s1", newCompletion.Style);
        Assert.Equal("s2", newCompletion.SelectedStyle);
    }

    [Fact]
    public void NewCompletionFromPosition_WithNegativePosition_IncreasesStartPosition()
    {
        var completion = new CompletionItem("text", startPosition: -5);

        var newCompletion = completion.NewCompletionFromPosition(-3);

        Assert.Equal(-2, newCompletion.StartPosition);
    }

    [Fact]
    public void NewCompletionFromPosition_ResultingPositive_ThrowsArgumentOutOfRangeException()
    {
        var completion = new CompletionItem("text", startPosition: 0);

        // Subtracting negative position (adding) should make StartPosition positive
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => completion.NewCompletionFromPosition(-1));

        Assert.Equal("StartPosition", ex.ParamName);
    }

    [Fact]
    public void Display_WithFormattedText_WorksCorrectly()
    {
        var ft = new FText([
            new StyleAndTextTuple("bold", "hello"),
            new StyleAndTextTuple("", " world")
        ]);
        AnyFormattedText display = ft;
        var completion = new CompletionItem("text", display: display);

        var displayText = completion.DisplayText;

        Assert.Equal("hello world", displayText.ToPlainText());
    }

    [Fact]
    public void Display_WithFunc_EvaluatesLazily()
    {
        var callCount = 0;
        Func<AnyFormattedText> func = () =>
        {
            callCount++;
            return "lazy display";
        };
        AnyFormattedText display = func;
        var completion = new CompletionItem("text", display: display);

        Assert.Equal(0, callCount);

        var displayText = completion.DisplayText.ToPlainText();

        Assert.Equal(1, callCount);
        Assert.Equal("lazy display", displayText);
    }

    [Fact]
    public void Equality_SameProperties_AreEqual()
    {
        var c1 = new CompletionItem("text", startPosition: -3, style: "s1");
        var c2 = new CompletionItem("text", startPosition: -3, style: "s1");

        Assert.Equal(c1, c2);
    }

    [Fact]
    public void Equality_DifferentText_AreNotEqual()
    {
        var c1 = new CompletionItem("text1");
        var c2 = new CompletionItem("text2");

        Assert.NotEqual(c1, c2);
    }

    [Fact]
    public void Record_WithExpression_CreatesNewInstance()
    {
        var original = new CompletionItem("text", startPosition: -5, style: "s1");

        var modified = original with { Style = "s2" };

        Assert.Equal("text", modified.Text);
        Assert.Equal(-5, modified.StartPosition);
        Assert.Equal("s2", modified.Style);
        Assert.Equal("s1", original.Style); // Original unchanged
    }
}
