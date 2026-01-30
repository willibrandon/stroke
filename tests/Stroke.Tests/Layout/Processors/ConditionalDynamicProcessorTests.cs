using Stroke.Core;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Layout.Controls;
using Stroke.Layout.Processors;
using Xunit;

namespace Stroke.Tests.Layout.Processors;

/// <summary>
/// Tests for ConditionalProcessor and DynamicProcessor.
/// </summary>
public class ConditionalProcessorTests
{
    private static TransformationInput CreateInput(
        string text = "hello",
        IReadOnlyList<StyleAndTextTuple>? fragments = null)
    {
        var bc = new BufferControl();
        var doc = new Document(text);
        var frags = fragments ?? new List<StyleAndTextTuple> { new("", text) };
        return new TransformationInput(bc, doc, 0, i => i, frags, 80, 24);
    }

    [Fact]
    public void FilterTrue_AppliesInnerProcessor()
    {
        var inner = new PasswordProcessor();
        var processor = new ConditionalProcessor(inner, new FilterOrBool(true));
        var input = CreateInput("abc");

        var result = processor.ApplyTransformation(input);

        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        Assert.Equal("***", fullText);
    }

    [Fact]
    public void FilterFalse_PassesThrough()
    {
        var inner = new PasswordProcessor();
        var processor = new ConditionalProcessor(inner, new FilterOrBool(false));
        var input = CreateInput("abc");

        var result = processor.ApplyTransformation(input);

        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        Assert.Equal("abc", fullText);
    }

    [Fact]
    public void DynamicFilter_ReadsAtApplyTime()
    {
        var enabled = false;
        var condition = new Condition(() => enabled);
        var inner = new PasswordProcessor();
        var processor = new ConditionalProcessor(inner, new FilterOrBool(condition));
        var input = CreateInput("abc");

        // Filter false initially
        var result1 = processor.ApplyTransformation(input);
        Assert.Equal("abc", string.Join("", result1.Fragments.Select(f => f.Text)));

        // Now enable
        enabled = true;
        var result2 = processor.ApplyTransformation(input);
        Assert.Equal("***", string.Join("", result2.Fragments.Select(f => f.Text)));
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var inner = new DummyProcessor();
        var processor = new ConditionalProcessor(inner, new FilterOrBool(true));
        var str = processor.ToString();
        Assert.StartsWith("ConditionalProcessor(", str);
        Assert.Contains("processor=", str);
        Assert.Contains("filter=", str);
    }

    [Fact]
    public void Properties_ReturnConstructorValues()
    {
        var inner = new DummyProcessor();
        var processor = new ConditionalProcessor(inner, new FilterOrBool(true));
        Assert.Same(inner, processor.Processor);
        Assert.NotNull(processor.Filter);
    }

    [Fact]
    public void IsSealed()
    {
        Assert.True(typeof(ConditionalProcessor).IsSealed);
    }
}

public class DynamicProcessorTests
{
    private static TransformationInput CreateInput(
        string text = "hello",
        IReadOnlyList<StyleAndTextTuple>? fragments = null)
    {
        var bc = new BufferControl();
        var doc = new Document(text);
        var frags = fragments ?? new List<StyleAndTextTuple> { new("", text) };
        return new TransformationInput(bc, doc, 0, i => i, frags, 80, 24);
    }

    [Fact]
    public void ReturnsProcessor_AppliesIt()
    {
        var processor = new DynamicProcessor(() => new PasswordProcessor());
        var input = CreateInput("abc");

        var result = processor.ApplyTransformation(input);

        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        Assert.Equal("***", fullText);
    }

    [Fact]
    public void ReturnsNull_UsesDummyProcessor()
    {
        var processor = new DynamicProcessor(() => null);
        var input = CreateInput("abc");

        var result = processor.ApplyTransformation(input);

        var fullText = string.Join("", result.Fragments.Select(f => f.Text));
        Assert.Equal("abc", fullText);
    }

    [Fact]
    public void DynamicSwitching_PerInvocation()
    {
        IProcessor? current = null;
        var processor = new DynamicProcessor(() => current);
        var input = CreateInput("abc");

        // Null → DummyProcessor
        var result1 = processor.ApplyTransformation(input);
        Assert.Equal("abc", string.Join("", result1.Fragments.Select(f => f.Text)));

        // Switch to PasswordProcessor
        current = new PasswordProcessor();
        var result2 = processor.ApplyTransformation(input);
        Assert.Equal("***", string.Join("", result2.Fragments.Select(f => f.Text)));
    }

    [Fact]
    public void GetProcessor_PropertyReturnsCtor()
    {
        Func<IProcessor?> factory = () => null;
        var processor = new DynamicProcessor(factory);
        Assert.Same(factory, processor.GetProcessor);
    }

    [Fact]
    public void IsSealed()
    {
        Assert.True(typeof(DynamicProcessor).IsSealed);
    }
}
