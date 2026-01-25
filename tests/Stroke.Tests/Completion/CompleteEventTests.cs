using Stroke.Completion;
using Xunit;

namespace Stroke.Tests.Completion;

/// <summary>
/// Tests for <see cref="CompleteEvent"/>.
/// </summary>
public sealed class CompleteEventTests
{
    [Fact]
    public void DefaultValues_BothFalse()
    {
        var evt = new CompleteEvent();

        Assert.False(evt.TextInserted);
        Assert.False(evt.CompletionRequested);
    }

    [Fact]
    public void TextInserted_True_SetsProperty()
    {
        var evt = new CompleteEvent(TextInserted: true);

        Assert.True(evt.TextInserted);
        Assert.False(evt.CompletionRequested);
    }

    [Fact]
    public void CompletionRequested_True_SetsProperty()
    {
        var evt = new CompleteEvent(CompletionRequested: true);

        Assert.False(evt.TextInserted);
        Assert.True(evt.CompletionRequested);
    }

    [Fact]
    public void BothTrue_IsAllowed()
    {
        // Python allows both to be true (edge case, not validated)
        var evt = new CompleteEvent(TextInserted: true, CompletionRequested: true);

        Assert.True(evt.TextInserted);
        Assert.True(evt.CompletionRequested);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var evt1 = new CompleteEvent(TextInserted: true, CompletionRequested: false);
        var evt2 = new CompleteEvent(TextInserted: true, CompletionRequested: false);

        Assert.Equal(evt1, evt2);
        Assert.Equal(evt1.GetHashCode(), evt2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var evt1 = new CompleteEvent(TextInserted: true);
        var evt2 = new CompleteEvent(CompletionRequested: true);

        Assert.NotEqual(evt1, evt2);
    }

    [Fact]
    public void Record_WithExpression_CreatesNewInstance()
    {
        var original = new CompleteEvent(TextInserted: true);

        var modified = original with { CompletionRequested = true };

        Assert.True(modified.TextInserted);
        Assert.True(modified.CompletionRequested);
    }
}
