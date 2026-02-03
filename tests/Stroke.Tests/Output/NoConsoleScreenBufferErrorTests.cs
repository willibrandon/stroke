using Stroke.Output;
using Xunit;

namespace Stroke.Tests.Output;

/// <summary>
/// Tests for <see cref="NoConsoleScreenBufferError"/> exception.
/// </summary>
public class NoConsoleScreenBufferErrorTests
{
    [Fact]
    public void Constructor_Default_CreatesMessageBasedOnEnvironment()
    {
        var error = new NoConsoleScreenBufferError();

        // Message should not be empty
        Assert.NotEmpty(error.Message);

        // Message should contain helpful text
        Assert.True(
            error.Message.Contains("console") ||
            error.Message.Contains("cmd.exe") ||
            error.Message.Contains("winpty"));
    }

    [Fact]
    public void Constructor_WithMessage_UsesProvidedMessage()
    {
        var customMessage = "Custom error message";
        var error = new NoConsoleScreenBufferError(customMessage);

        Assert.Equal(customMessage, error.Message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_PreservesBoth()
    {
        var customMessage = "Custom error message";
        var innerException = new InvalidOperationException("Inner");
        var error = new NoConsoleScreenBufferError(customMessage, innerException);

        Assert.Equal(customMessage, error.Message);
        Assert.Same(innerException, error.InnerException);
    }

    [Fact]
    public void DefaultMessage_ContainsActionableGuidance()
    {
        // Clear TERM to test default path
        var originalTerm = Environment.GetEnvironmentVariable("TERM");
        try
        {
            Environment.SetEnvironmentVariable("TERM", null);
            var error = new NoConsoleScreenBufferError();

            // Should ask about cmd.exe
            Assert.Contains("cmd.exe", error.Message);
        }
        finally
        {
            Environment.SetEnvironmentVariable("TERM", originalTerm);
        }
    }

    [Fact]
    public void DefaultMessage_WithXtermTerm_SuggestsWinpty()
    {
        var originalTerm = Environment.GetEnvironmentVariable("TERM");
        try
        {
            Environment.SetEnvironmentVariable("TERM", "xterm-256color");
            var error = new NoConsoleScreenBufferError();

            // Should suggest winpty
            Assert.Contains("winpty", error.Message);
            Assert.Contains("xterm", error.Message);
        }
        finally
        {
            Environment.SetEnvironmentVariable("TERM", originalTerm);
        }
    }

    [Fact]
    public void DefaultMessage_XtermDetection_IsCaseInsensitive()
    {
        var originalTerm = Environment.GetEnvironmentVariable("TERM");
        try
        {
            Environment.SetEnvironmentVariable("TERM", "XTERM");
            var error = new NoConsoleScreenBufferError();

            // Should suggest winpty (case-insensitive xterm detection)
            Assert.Contains("winpty", error.Message);
        }
        finally
        {
            Environment.SetEnvironmentVariable("TERM", originalTerm);
        }
    }
}
