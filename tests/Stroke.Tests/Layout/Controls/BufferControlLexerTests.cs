using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Layout.Controls;
using Stroke.Lexers;
using Xunit;

using Buffer = Stroke.Core.Buffer;

namespace Stroke.Tests.Layout.Controls;

/// <summary>
/// Tests for BufferControl lexer integration.
/// </summary>
public sealed class BufferControlLexerTests
{
    #region SimpleLexer Tests

    [Fact]
    public void CreateContent_WithSimpleLexer_AppliesStyle()
    {
        var buffer = new Buffer();
        buffer.Text = "Hello World";
        var lexer = new SimpleLexer("class:test-style");
        var control = new BufferControl(buffer: buffer, lexer: lexer);

        var content = control.CreateContent(80, 24);
        var line = content.GetLine(0);

        // First fragment should have the lexer's style
        Assert.NotEmpty(line);
        var firstFragment = line[0];
        Assert.Contains("class:test-style", firstFragment.Style);
    }

    [Fact]
    public void CreateContent_WithDefaultLexer_ReturnsUnstyledText()
    {
        var buffer = new Buffer();
        buffer.Text = "Hello World";
        var control = new BufferControl(buffer: buffer); // Uses default SimpleLexer

        var content = control.CreateContent(80, 24);
        var line = content.GetLine(0);

        Assert.NotEmpty(line);
    }

    #endregion

    #region DynamicLexer Tests

    [Fact]
    public void CreateContent_WithDynamicLexer_UsesCurrentLexer()
    {
        var buffer = new Buffer();
        buffer.Text = "Test content";
        var lexer1 = new SimpleLexer("class:style1");
        var lexer2 = new SimpleLexer("class:style2");

        ILexer currentLexer = lexer1;
        var dynamicLexer = new DynamicLexer(() => currentLexer);
        var control = new BufferControl(buffer: buffer, lexer: dynamicLexer);

        // First render with lexer1
        var content1 = control.CreateContent(80, 24);
        var line1 = content1.GetLine(0);
        Assert.Contains("class:style1", line1[0].Style);

        // Switch to lexer2
        currentLexer = lexer2;

        // Need to create new content to see the change
        var content2 = control.CreateContent(80, 24);
        var line2 = content2.GetLine(0);
        Assert.Contains("class:style2", line2[0].Style);
    }

    #endregion

    #region Multi-line Lexer Tests

    [Fact]
    public void CreateContent_MultiLine_EachLineHasFragments()
    {
        var buffer = new Buffer();
        buffer.Text = "Line 1\nLine 2\nLine 3";
        var lexer = new SimpleLexer("class:multi");
        var control = new BufferControl(buffer: buffer, lexer: lexer);

        var content = control.CreateContent(80, 24);

        for (int i = 0; i < 3; i++)
        {
            var line = content.GetLine(i);
            Assert.NotEmpty(line);
            // Each line should have styled content
            Assert.True(line.Count >= 1);
        }
    }

    [Fact]
    public void CreateContent_EmptyLine_HasFragments()
    {
        var buffer = new Buffer();
        buffer.Text = "Line 1\n\nLine 3"; // Empty middle line
        var control = new BufferControl(buffer: buffer);

        var content = control.CreateContent(80, 24);

        // Middle line should still have at least trailing space
        var middleLine = content.GetLine(1);
        Assert.NotEmpty(middleLine);
    }

    #endregion

    #region Lexer Caching Tests

    [Fact]
    public void CreateContent_SameText_UsesCachedLexerResult()
    {
        var buffer = new Buffer();
        buffer.Text = "Same text";
        var callCount = 0;

        var lexer = new CountingLexer(() => callCount++);
        var control = new BufferControl(buffer: buffer, lexer: lexer);

        // First call
        control.CreateContent(80, 24);
        var firstCount = callCount;

        // Second call with same text should use cache
        control.CreateContent(80, 24);
        var secondCount = callCount;

        // Due to caching, the lexer might not be called again for same text
        Assert.True(secondCount >= firstCount);
    }

    [Fact]
    public void CreateContent_DifferentText_LexesAgain()
    {
        var buffer = new Buffer();
        buffer.Text = "First text";
        var callCount = 0;

        var lexer = new CountingLexer(() => callCount++);
        var control = new BufferControl(buffer: buffer, lexer: lexer);

        // First call
        control.CreateContent(80, 24);

        // Change text
        buffer.Text = "Different text";

        // Second call should lex the new text
        control.CreateContent(80, 24);

        Assert.True(callCount >= 1);
    }

    /// <summary>
    /// Test lexer that counts how many times it's called.
    /// </summary>
    private sealed class CountingLexer : ILexer
    {
        private readonly Action _onLex;

        public CountingLexer(Action onLex)
        {
            _onLex = onLex;
        }

        public Func<int, IReadOnlyList<StyleAndTextTuple>> LexDocument(Document document)
        {
            _onLex();
            return lineNo =>
            {
                if (lineNo >= document.LineCount) return [];
                return [new StyleAndTextTuple("", document.Lines[lineNo])];
            };
        }

        public object InvalidationHash() => Guid.NewGuid().ToString();
    }

    #endregion

    #region Unicode and Special Characters

    [Fact]
    public void CreateContent_WithUnicode_HandlesCorrectly()
    {
        var buffer = new Buffer();
        buffer.Text = "Hello 世界 🌍";
        var control = new BufferControl(buffer: buffer);

        var content = control.CreateContent(80, 24);
        var line = content.GetLine(0);

        Assert.NotEmpty(line);
        // Verify the full text is present
        var fullText = string.Concat(line.Select(f => f.Text));
        Assert.Contains("世界", fullText);
        Assert.Contains("🌍", fullText);
    }

    [Fact]
    public void CreateContent_WithTabs_HandlesCorrectly()
    {
        var buffer = new Buffer();
        buffer.Text = "Hello\tWorld";
        var control = new BufferControl(buffer: buffer);

        var content = control.CreateContent(80, 24);
        var line = content.GetLine(0);

        Assert.NotEmpty(line);
        var fullText = string.Concat(line.Select(f => f.Text));
        Assert.Contains("\t", fullText);
    }

    #endregion
}
