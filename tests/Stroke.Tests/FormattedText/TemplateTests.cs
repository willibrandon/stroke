using Stroke.FormattedText;
using Xunit;

namespace Stroke.Tests.FormattedText;

/// <summary>
/// Tests for the <see cref="Template"/> class.
/// </summary>
public class TemplateTests
{
    #region T073: Basic interpolation tests

    [Fact]
    public void Format_WithSinglePlaceholder_SubstitutesValue()
    {
        var template = new Template("Hello {}!");
        var result = template.Format("World");

        var formattedText = FormattedTextUtils.ToFormattedText(result());
        var text = FormattedTextUtils.FragmentListToText(formattedText);

        Assert.Equal("Hello World!", text);
    }

    [Fact]
    public void Format_WithPlainString_CreatesUnstyledFragment()
    {
        var template = new Template("{}");
        var result = template.Format("plain");

        var formattedText = FormattedTextUtils.ToFormattedText(result());
        var fragment = formattedText.First(f => f.Text == "plain");

        Assert.Equal("", fragment.Style);
    }

    #endregion

    #region T074: Formatting preservation tests (HTML in template)

    [Fact]
    public void Format_WithHtmlValue_PreservesHtmlFormatting()
    {
        var template = new Template("Value: {}");
        var html = new Html("<b>bold</b>");
        var result = template.Format(html);

        var formattedText = FormattedTextUtils.ToFormattedText(result());
        var boldFragment = formattedText.First(f => f.Text == "bold");

        Assert.Contains("class:b", boldFragment.Style);
    }

    [Fact]
    public void Format_WithAnsiValue_PreservesAnsiFormatting()
    {
        var template = new Template("Value: {}");
        var ansi = new Ansi("\x1b[1mformatted\x1b[0m");
        var result = template.Format(ansi);

        var formattedText = FormattedTextUtils.ToFormattedText(result());

        // Should have fragments with bold style
        Assert.Contains(formattedText, f => f.Style.Contains("bold"));
    }

    [Fact]
    public void Format_WithFormattedTextValue_PreservesStyles()
    {
        var template = new Template("Prefix {} Suffix");
        var styledText = new Stroke.FormattedText.FormattedText([new StyleAndTextTuple("class:custom", "styled")]);
        var result = template.Format(styledText);

        var formattedText = FormattedTextUtils.ToFormattedText(result());
        var styledFragment = formattedText.First(f => f.Text == "styled");

        Assert.Equal("class:custom", styledFragment.Style);
    }

    #endregion

    #region T075: Multiple placeholder tests

    [Fact]
    public void Format_WithMultiplePlaceholders_SubstitutesInOrder()
    {
        var template = new Template("{} + {} = {}");
        var result = template.Format("1", "2", "3");

        var formattedText = FormattedTextUtils.ToFormattedText(result());
        var text = FormattedTextUtils.FragmentListToText(formattedText);

        Assert.Equal("1 + 2 = 3", text);
    }

    [Fact]
    public void Format_WithAdjacentPlaceholders_SubstitutesCorrectly()
    {
        var template = new Template("{}{}{}");
        var result = template.Format("a", "b", "c");

        var formattedText = FormattedTextUtils.ToFormattedText(result());
        var text = FormattedTextUtils.FragmentListToText(formattedText);

        Assert.Equal("abc", text);
    }

    [Fact]
    public void Format_WithPlaceholderAtStart_SubstitutesCorrectly()
    {
        var template = new Template("{} world");
        var result = template.Format("hello");

        var formattedText = FormattedTextUtils.ToFormattedText(result());
        var text = FormattedTextUtils.FragmentListToText(formattedText);

        Assert.Equal("hello world", text);
    }

    [Fact]
    public void Format_WithPlaceholderAtEnd_SubstitutesCorrectly()
    {
        var template = new Template("hello {}");
        var result = template.Format("world");

        var formattedText = FormattedTextUtils.ToFormattedText(result());
        var text = FormattedTextUtils.FragmentListToText(formattedText);

        Assert.Equal("hello world", text);
    }

    #endregion

    #region T076: Lazy evaluation tests

    [Fact]
    public void Format_ReturnsCallable_NotImmediate()
    {
        var template = new Template("{}");
        var result = template.Format("value");

        // Result should be a callable, not FormattedText
        Assert.IsType<Func<AnyFormattedText>>(result);
    }

    [Fact]
    public void Format_CallableEvaluatesLazily_WhenInvoked()
    {
        int counter = 0;
        var template = new Template("Count: {}");
        Func<AnyFormattedText> lazyValue = () =>
        {
            counter++;
            return counter.ToString();
        };
        var result = template.Format(lazyValue);

        // Counter should still be 0 before invocation
        Assert.Equal(0, counter);

        // Invoke the callable
        _ = result();

        // Now counter should be incremented
        Assert.Equal(1, counter);
    }

    #endregion

    #region T077: Positional syntax error tests ({0} throws)

    [Fact]
    public void Constructor_WithPositionalPlaceholder_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => new Template("Hello {0}"));
    }

    [Fact]
    public void Constructor_WithMultiplePositionalPlaceholders_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => new Template("{0} + {1}"));
    }

    [Fact]
    public void Constructor_WithHighPositionalPlaceholder_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => new Template("Value: {99}"));
    }

    #endregion

    #region T078: Escaped braces tests ({{ }})

    [Fact]
    public void Format_WithEscapedOpenBrace_ProducesLiteralBrace()
    {
        var template = new Template("{{");
        var result = template.Format();

        var formattedText = FormattedTextUtils.ToFormattedText(result());
        var text = FormattedTextUtils.FragmentListToText(formattedText);

        Assert.Equal("{", text);
    }

    [Fact]
    public void Format_WithEscapedCloseBrace_ProducesLiteralBrace()
    {
        var template = new Template("}}");
        var result = template.Format();

        var formattedText = FormattedTextUtils.ToFormattedText(result());
        var text = FormattedTextUtils.FragmentListToText(formattedText);

        Assert.Equal("}", text);
    }

    [Fact]
    public void Format_WithEscapedBracesAndPlaceholder_HandlesCorrectly()
    {
        var template = new Template("{{{}}}");
        var result = template.Format("value");

        var formattedText = FormattedTextUtils.ToFormattedText(result());
        var text = FormattedTextUtils.FragmentListToText(formattedText);

        Assert.Equal("{value}", text);
    }

    [Fact]
    public void Format_WithJsonLikePattern_HandlesCorrectly()
    {
        var template = new Template("{{ \"key\": \"{}\" }}");
        var result = template.Format("value");

        var formattedText = FormattedTextUtils.ToFormattedText(result());
        var text = FormattedTextUtils.FragmentListToText(formattedText);

        Assert.Equal("{ \"key\": \"value\" }", text);
    }

    #endregion

    #region T079: Placeholder/value count mismatch tests

    [Fact]
    public void Format_WithTooFewValues_ThrowsArgumentException()
    {
        var template = new Template("{} {} {}");
        var result = template.Format("one", "two");

        Assert.Throws<ArgumentException>(() => result());
    }

    [Fact]
    public void Format_WithTooManyValues_ThrowsArgumentException()
    {
        var template = new Template("{} {}");
        var result = template.Format("one", "two", "three");

        Assert.Throws<ArgumentException>(() => result());
    }

    [Fact]
    public void Format_WithNoPlaceholdersAndValues_ThrowsArgumentException()
    {
        var template = new Template("no placeholders");
        var result = template.Format("value");

        Assert.Throws<ArgumentException>(() => result());
    }

    [Fact]
    public void Format_WithPlaceholdersAndNoValues_ThrowsArgumentException()
    {
        var template = new Template("placeholder: {}");
        var result = template.Format();

        Assert.Throws<ArgumentException>(() => result());
    }

    [Fact]
    public void Format_WithMatchingCount_Succeeds()
    {
        var template = new Template("{} {} {}");
        var result = template.Format("a", "b", "c");

        // Should not throw
        var formattedText = FormattedTextUtils.ToFormattedText(result());
        Assert.NotNull(formattedText);
    }

    #endregion

    #region Property and method tests

    [Fact]
    public void Text_ReturnsOriginalTemplate()
    {
        var templateText = "Hello {}!";
        var template = new Template(templateText);

        Assert.Equal(templateText, template.Text);
    }

    [Fact]
    public void Constructor_WithNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Template(null!));
    }

    [Fact]
    public void ToString_ReturnsDescriptiveString()
    {
        var template = new Template("Hello {}!");

        Assert.Equal("Template(Hello {}!)", template.ToString());
    }

    #endregion
}
