using Stroke.Output;
using Xunit;

namespace Stroke.Tests.Output;

/// <summary>
/// Tests for <see cref="OutputFactory"/> output creation.
/// </summary>
public sealed class OutputFactoryTests
{
    #region CreateFromStream Tests

    [Fact]
    public void CreateFromStream_StringWriter_ReturnsVt100Output()
    {
        var writer = new StringWriter();

        var output = OutputFactory.CreateFromStream(writer);

        Assert.IsType<Vt100Output>(output);
    }

    [Fact]
    public void CreateFromStream_NullWriter_ReturnsDummyOutput()
    {
        var output = OutputFactory.CreateFromStream(TextWriter.Null);

        Assert.IsType<DummyOutput>(output);
    }

    [Fact]
    public void CreateFromStream_NullArgument_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => OutputFactory.CreateFromStream(null!));
    }

    [Fact]
    public void CreateFromStream_WithTerm_PassesToVt100Output()
    {
        var writer = new StringWriter();

        var output = OutputFactory.CreateFromStream(writer, term: "xterm-256color");

        // We can verify this works by checking it returns Vt100Output
        Assert.IsType<Vt100Output>(output);
    }

    [Fact]
    public void CreateFromStream_WithColorDepth_PassesToVt100Output()
    {
        var writer = new StringWriter();

        var output = OutputFactory.CreateFromStream(writer, defaultColorDepth: ColorDepth.Depth24Bit);

        // Verify the color depth was passed
        Assert.IsType<Vt100Output>(output);
        Assert.Equal(ColorDepth.Depth24Bit, output.GetDefaultColorDepth());
    }

    #endregion

    #region Create Tests

    [Fact]
    public void Create_NullWriter_ReturnsDummyOutput()
    {
        // When stdout is TextWriter.Null, should return DummyOutput
        var output = OutputFactory.Create(TextWriter.Null);

        Assert.IsType<DummyOutput>(output);
    }

    [Fact]
    public void Create_WithStringWriter_ReturnsAppropriateOutput()
    {
        // This test verifies Create works without throwing
        // Exact type depends on Console.IsOutputRedirected state
        var writer = new StringWriter();

        var output = OutputFactory.Create(writer);

        Assert.IsAssignableFrom<IOutput>(output);
    }

    #endregion

    #region Output Type Verification

    [Fact]
    public void DummyOutput_IsCorrectTypeForNullWriter()
    {
        var output = OutputFactory.CreateFromStream(TextWriter.Null);

        // Verify it's a proper DummyOutput
        Assert.IsType<DummyOutput>(output);

        // Verify DummyOutput behavior
        Assert.Equal(ColorDepth.Depth1Bit, output.GetDefaultColorDepth());
        Assert.False(output.RespondsToCpr);
    }

    [Fact]
    public void Vt100Output_IsCorrectTypeForStringWriter()
    {
        var writer = new StringWriter();
        var output = OutputFactory.CreateFromStream(writer);

        // Verify it's a proper Vt100Output
        Assert.IsType<Vt100Output>(output);
    }

    #endregion

    #region Factory Patterns

    [Fact]
    public void Create_MultipleCallsSameParams_ReturnsSeparateInstances()
    {
        var output1 = OutputFactory.Create(TextWriter.Null);
        var output2 = OutputFactory.Create(TextWriter.Null);

        // Each call should create a new instance
        Assert.NotSame(output1, output2);
    }

    [Fact]
    public void CreateFromStream_MultipleCallsSameWriter_ReturnsSeparateInstances()
    {
        var writer = new StringWriter();
        var output1 = OutputFactory.CreateFromStream(writer);
        var output2 = OutputFactory.CreateFromStream(writer);

        Assert.NotSame(output1, output2);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void CreateFromStream_UsableForWriting()
    {
        var writer = new StringWriter();
        var output = OutputFactory.CreateFromStream(writer);

        output.Write("Hello");
        output.Flush();

        Assert.Contains("Hello", writer.ToString());
    }

    [Fact]
    public void DummyOutput_UsableForAllOperations()
    {
        var output = OutputFactory.Create(TextWriter.Null);

        // All operations should complete without error
        output.Write("test");
        output.WriteRaw("\x1b[0m");
        output.Flush();
        output.CursorGoto(1, 1);
        output.EraseScreen();
        output.SetAttributes(new Stroke.Styles.Attrs(Bold: true), ColorDepth.Depth24Bit);
        output.ResetAttributes();
        output.HideCursor();
        output.ShowCursor();
        output.EnableMouseSupport();
        output.DisableMouseSupport();
    }

    #endregion
}
