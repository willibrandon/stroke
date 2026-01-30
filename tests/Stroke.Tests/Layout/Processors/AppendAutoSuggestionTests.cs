using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Layout.Controls;
using Stroke.Layout.Processors;
using Xunit;

// Alias to avoid ambiguity with System.Buffer
using Buffer = Stroke.Core.Buffer;

namespace Stroke.Tests.Layout.Processors;

/// <summary>
/// Tests for AppendAutoSuggestion processor.
/// </summary>
public class AppendAutoSuggestionTests
{
    private static TransformationInput CreateInput(
        BufferControl bc,
        string text,
        int lineNumber = 0,
        IReadOnlyList<StyleAndTextTuple>? fragments = null)
    {
        var doc = new Document(text);
        var lines = text.Split('\n');
        var lineText = lineNumber < lines.Length ? lines[lineNumber] : "";
        var frags = fragments ?? new List<StyleAndTextTuple> { new("", lineText) };
        return new TransformationInput(bc, doc, lineNumber, i => i, frags, 80, 24);
    }

    [Fact]
    public void LastLine_NoSuggestion_AppendsEmpty()
    {
        // Buffer with no suggestion set (default state)
        var buffer = new Buffer();
        buffer.SetDocument(new Document("hello", cursorPosition: 5));
        var bc = new BufferControl(buffer: buffer);
        var input = CreateInput(bc, "hello", lineNumber: 0);

        var processor = new AppendAutoSuggestion();
        var result = processor.ApplyTransformation(input);

        // Should have an empty suggestion fragment appended
        var lastFragment = result.Fragments[^1];
        Assert.Equal("", lastFragment.Text);
    }

    [Fact]
    public void LastLine_CursorNotAtEnd_AppendsEmpty()
    {
        // Cursor not at end — even if there were a suggestion, it wouldn't show
        var buffer = new Buffer();
        buffer.SetDocument(new Document("hello", cursorPosition: 2));
        var bc = new BufferControl(buffer: buffer);
        var input = CreateInput(bc, "hello", lineNumber: 0);

        var processor = new AppendAutoSuggestion();
        var result = processor.ApplyTransformation(input);

        // Cursor not at end → empty suggestion
        var lastFragment = result.Fragments[^1];
        Assert.Equal("", lastFragment.Text);
    }

    [Fact]
    public void NonLastLine_PassesThrough()
    {
        var buffer = new Buffer();
        buffer.SetDocument(new Document("line0\nline1", cursorPosition: 11));
        var bc = new BufferControl(buffer: buffer);
        var input = CreateInput(bc, "line0\nline1", lineNumber: 0);

        var processor = new AppendAutoSuggestion();
        var result = processor.ApplyTransformation(input);

        // Line 0 is not the last line (doc has 2 lines), so pass-through
        Assert.Equal(input.Fragments.Count, result.Fragments.Count);
    }

    [Fact]
    public void LastLine_StyleAppliedToAppendedFragment()
    {
        var buffer = new Buffer();
        buffer.SetDocument(new Document("hi", cursorPosition: 2));
        var bc = new BufferControl(buffer: buffer);
        var input = CreateInput(bc, "hi", lineNumber: 0);

        var processor = new AppendAutoSuggestion(style: "class:my-suggest");
        var result = processor.ApplyTransformation(input);

        var lastFragment = result.Fragments[^1];
        Assert.Equal("class:my-suggest", lastFragment.Style);
    }

    [Fact]
    public void DefaultStyle()
    {
        var processor = new AppendAutoSuggestion();
        Assert.Equal("class:auto-suggestion", processor.Style);
    }

    [Fact]
    public void IsSealed()
    {
        Assert.True(typeof(AppendAutoSuggestion).IsSealed);
    }

    [Fact]
    public void ImplementsIProcessor()
    {
        var processor = new AppendAutoSuggestion();
        Assert.IsAssignableFrom<IProcessor>(processor);
    }
}
