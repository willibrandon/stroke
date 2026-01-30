using Stroke.Application;
using Stroke.Input;
using Stroke.Output;
using Xunit;

using AppContext = Stroke.Application.AppContext;

namespace Stroke.Tests.Application;

public class AppSessionTests
{
    [Fact]
    public void Constructor_WithDefaults_CreatesSession()
    {
        var session = new AppSession();
        Assert.NotNull(session);
    }

    [Fact]
    public void Constructor_WithCustomInput_SetsInput()
    {
        var input = new DummyInput();
        var session = new AppSession(input: input);

        Assert.Same(input, session.Input);
    }

    [Fact]
    public void Constructor_WithCustomOutput_SetsOutput()
    {
        var output = new DummyOutput();
        var session = new AppSession(output: output);

        Assert.Same(output, session.Output);
    }

    [Fact]
    public void Constructor_WithCustomIO_SetsBoth()
    {
        var input = new DummyInput();
        var output = new DummyOutput();
        var session = new AppSession(input, output);

        Assert.Same(input, session.Input);
        Assert.Same(output, session.Output);
    }

    [Fact]
    public void Input_LazyCreation_WhenNotProvided()
    {
        // When no input is provided, Input property creates one lazily.
        // This test verifies it doesn't throw.
        var session = new AppSession();

        // Accessing Input should not throw (may create via factory)
        var input = session.Input;
        Assert.NotNull(input);
    }

    [Fact]
    public void Output_LazyCreation_WhenNotProvided()
    {
        var session = new AppSession();

        var output = session.Output;
        Assert.NotNull(output);
    }

    [Fact]
    public void Input_ReturnsSameInstanceOnMultipleCalls()
    {
        var session = new AppSession();

        var input1 = session.Input;
        var input2 = session.Input;
        Assert.Same(input1, input2);
    }

    [Fact]
    public void Output_ReturnsSameInstanceOnMultipleCalls()
    {
        var session = new AppSession();

        var output1 = session.Output;
        var output2 = session.Output;
        Assert.Same(output1, output2);
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var session = AppContext.CreateAppSession(new DummyInput(), new DummyOutput());

        // Should not throw when disposed multiple times
        session.Dispose();
        session.Dispose();
        session.Dispose();
    }

    [Fact]
    public void Dispose_RestoresPreviousSession()
    {
        var outerSession = AppContext.GetAppSession();

        var session = AppContext.CreateAppSession(new DummyInput(), new DummyOutput());
        Assert.Same(session, AppContext.GetAppSession());

        session.Dispose();

        Assert.Same(outerSession, AppContext.GetAppSession());
    }

    [Fact]
    public void NestedCreateAppSession_RestoresCorrectly()
    {
        // Verifies that nested sessions form a proper stack (A → B → C).
        // Disposing C restores B, disposing B restores A.
        var sessionA = AppContext.GetAppSession();

        var sessionB = AppContext.CreateAppSession(new DummyInput(), new DummyOutput());
        Assert.Same(sessionB, AppContext.GetAppSession());

        var sessionC = AppContext.CreateAppSession(new DummyInput(), new DummyOutput());
        Assert.Same(sessionC, AppContext.GetAppSession());

        // Dispose C → should restore B
        sessionC.Dispose();
        Assert.Same(sessionB, AppContext.GetAppSession());

        // Dispose B → should restore A
        sessionB.Dispose();
        Assert.Same(sessionA, AppContext.GetAppSession());
    }

    [Fact]
    public void ToString_ContainsApp()
    {
        var session = new AppSession();
        var result = session.ToString();
        Assert.Contains("AppSession", result);
    }

    [Fact]
    public void ExplicitInput_ReturnsProvidedInput()
    {
        var input = new DummyInput();
        var session = new AppSession(input: input);

        Assert.Same(input, session.ExplicitInput);
    }

    [Fact]
    public void ExplicitInput_NullWhenNotProvided()
    {
        var session = new AppSession();
        Assert.Null(session.ExplicitInput);
    }

    [Fact]
    public void ExplicitOutput_ReturnsProvidedOutput()
    {
        var output = new DummyOutput();
        var session = new AppSession(output: output);

        Assert.Same(output, session.ExplicitOutput);
    }

    [Fact]
    public void ExplicitOutput_NullWhenNotProvided()
    {
        var session = new AppSession();
        Assert.Null(session.ExplicitOutput);
    }
}
