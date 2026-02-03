using System.Diagnostics.CodeAnalysis;
using Stroke.Core.Primitives;
using Stroke.Input.Windows;
using Stroke.Output;
using Stroke.Output.Windows;
using Xunit;

namespace Stroke.Tests.Output.Windows;

/// <summary>
/// Tests for <see cref="ConEmuOutput"/>.
/// </summary>
/// <remarks>
/// <para>
/// Per Constitution VIII, tests use real Win32Output and Vt100Output instances.
/// No mocks or fakes are used.
/// </para>
/// <para>
/// Many tests require Windows platform and a console screen buffer.
/// Tests are skipped on non-Windows platforms.
/// </para>
/// <para>
/// Test runners and mintty redirect stdio through pipes, so the process may
/// not have a Win32 console attached. <see cref="EnsureConsoleAttached"/>
/// allocates one when needed.
/// </para>
/// </remarks>
[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility",
    Justification = "Tests explicitly check OperatingSystem.IsWindows() before calling ConEmuOutput")]
public class ConEmuOutputTests
{
    /// <summary>
    /// Ensures the process has an attached Win32 console.
    /// Test runners and mintty redirect stdio through pipes, leaving
    /// no console attached. Win32Output requires a real console handle.
    /// </summary>
    private static void EnsureConsoleAttached()
    {
        if (OperatingSystem.IsWindows() && ConsoleApi.GetConsoleWindow() == nint.Zero)
        {
            ConsoleApi.AllocConsole();
        }
    }

    #region Constructor Tests (T008)

    [Fact]
    public void Constructor_WithValidTextWriter_CreatesBothOutputs()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();

        // Act
        using var output = new ConEmuOutput(writer);

        // Assert
        Assert.NotNull(output.Win32Output);
        Assert.NotNull(output.Vt100Output);
    }

    [Fact]
    public void Constructor_WithNullTextWriter_ThrowsArgumentNullException()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ConEmuOutput(null!));
    }

    [Fact]
    public void Constructor_PropagatesDefaultColorDepth()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        var colorDepth = ColorDepth.Depth8Bit;

        // Act
        using var output = new ConEmuOutput(writer, colorDepth);

        // Assert
        // Both outputs should have the color depth configured
        Assert.Equal(colorDepth, output.Win32Output.DefaultColorDepth);
        Assert.Equal(colorDepth, output.GetDefaultColorDepth());
    }

    [Fact]
    public void Win32Output_Property_ReturnsNonNullValue()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        using var output = new ConEmuOutput(writer);

        // Act & Assert
        Assert.NotNull(output.Win32Output);
    }

    [Fact]
    public void Vt100Output_Property_ReturnsNonNullValue()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        using var output = new ConEmuOutput(writer);

        // Act & Assert
        Assert.NotNull(output.Vt100Output);
    }

    #endregion

    #region User Story 1 Tests: Text Output Operations (T009-T011)

    [Fact]
    public void Write_DelegatesToVt100Output()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        using var output = new ConEmuOutput(writer);
        var testData = "Hello, ConEmu!";

        // Act
        output.Write(testData);
        output.Flush();

        // Assert
        var result = writer.ToString();
        Assert.Contains(testData, result);
    }

    [Fact]
    public void WriteRaw_DelegatesToVt100Output()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        using var output = new ConEmuOutput(writer);
        var testData = "\x1b[31mRed Text\x1b[0m";

        // Act
        output.WriteRaw(testData);
        output.Flush();

        // Assert
        var result = writer.ToString();
        Assert.Contains(testData, result);
    }

    [Fact]
    public void Flush_DelegatesToVt100Output()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        using var output = new ConEmuOutput(writer);
        output.Write("test");

        // Act
        output.Flush();

        // Assert
        Assert.Contains("test", writer.ToString());
    }

    #endregion

    #region User Story 1 Tests: Console Sizing Operations (T012-T013)

    [Fact]
    public void GetSize_DelegatesToWin32Output()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        using var output = new ConEmuOutput(writer);

        // Act
        var size = output.GetSize();

        // Assert
        Assert.True(size.Columns > 0);
        Assert.True(size.Rows > 0);
    }

    [Fact]
    public void GetRowsBelowCursorPosition_DelegatesToWin32Output()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        using var output = new ConEmuOutput(writer);

        // Act
        var rows = output.GetRowsBelowCursorPosition();

        // Assert
        Assert.True(rows >= 0);
    }

    #endregion

    #region User Story 2 Tests: Mouse Support (T045-T046)

    [Fact]
    public void EnableMouseSupport_DelegatesToWin32Output()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        using var output = new ConEmuOutput(writer);

        // Act & Assert - should not throw
        output.EnableMouseSupport();
        output.DisableMouseSupport();
    }

    [Fact]
    public void DisableMouseSupport_DelegatesToWin32Output()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        using var output = new ConEmuOutput(writer);

        // Act & Assert - should not throw
        output.DisableMouseSupport();
    }

    #endregion

    #region User Story 3 Tests: Bracketed Paste (T049-T050)

    [Fact]
    public void EnableBracketedPaste_DelegatesToWin32Output()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        using var output = new ConEmuOutput(writer);

        // Act & Assert - should not throw
        output.EnableBracketedPaste();
        output.DisableBracketedPaste();
    }

    [Fact]
    public void DisableBracketedPaste_DelegatesToWin32Output()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        using var output = new ConEmuOutput(writer);

        // Act & Assert - should not throw
        output.DisableBracketedPaste();
    }

    #endregion

    #region Edge Case Tests (T054)

    [Fact]
    public void RespondsToCpr_AlwaysReturnsFalse()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        using var output = new ConEmuOutput(writer);

        // Act & Assert
        Assert.False(output.RespondsToCpr);
    }

    [Fact]
    public void Encoding_ReturnsUtf8()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        using var output = new ConEmuOutput(writer);

        // Act & Assert
        Assert.Equal("utf-8", output.Encoding);
    }

    [Fact]
    public void Stdout_ReturnsSameTextWriterPassedToConstructor()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        using var output = new ConEmuOutput(writer);

        // Act & Assert
        Assert.Same(writer, output.Stdout);
    }

    [Fact]
    public void Constructor_OnNonWindows_ThrowsPlatformNotSupportedException()
    {
        // This test runs on non-Windows platforms to verify PlatformNotSupportedException is thrown
        if (OperatingSystem.IsWindows())
        {
            return; // Skip on Windows where it would succeed
        }

        // Arrange
        using var writer = new StringWriter();

        // Act & Assert
        Assert.Throws<PlatformNotSupportedException>(() => new ConEmuOutput(writer));
    }

    [Fact]
    [Trait("Platform", "Windows")]
    public void ConEmuAnsiDetection_IsCaseSensitive()
    {
        // This test verifies the ConEmuANSI environment variable is case-sensitive
        // "ON" is valid, "on" and "On" are not
        // Note: IsConEmuAnsi also requires Windows, so skip on non-Windows
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        // Save original value
        var originalValue = Environment.GetEnvironmentVariable("ConEmuANSI");

        try
        {
            // Test uppercase "ON"
            Environment.SetEnvironmentVariable("ConEmuANSI", "ON");
            Assert.True(Stroke.Core.PlatformUtils.IsConEmuAnsi);

            // Test lowercase "on" - should NOT be detected
            Environment.SetEnvironmentVariable("ConEmuANSI", "on");
            Assert.False(Stroke.Core.PlatformUtils.IsConEmuAnsi);

            // Test mixed case "On" - should NOT be detected
            Environment.SetEnvironmentVariable("ConEmuANSI", "On");
            Assert.False(Stroke.Core.PlatformUtils.IsConEmuAnsi);
        }
        finally
        {
            // Restore original value
            Environment.SetEnvironmentVariable("ConEmuANSI", originalValue);
        }
    }

    #endregion
}
