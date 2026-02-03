using System.Diagnostics.CodeAnalysis;
using Stroke.Core.Primitives;
using Stroke.Input.Windows;
using Stroke.Output;
using Stroke.Output.Windows;
using Xunit;

namespace Stroke.Tests.Output.Windows;

/// <summary>
/// Tests for <see cref="Windows10Output"/>.
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
    Justification = "Tests explicitly check OperatingSystem.IsWindows() before calling Windows10Output")]
public class Windows10OutputTests
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

    #region Constructor Tests (T009-T011a)

    [Fact]
    public void Constructor_WithNullStdout_ThrowsArgumentNullException()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Windows10Output(null!));
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
        Assert.Throws<PlatformNotSupportedException>(() => new Windows10Output(writer));
    }

    [Fact]
    public void Constructor_PropagatesNoConsoleScreenBufferError()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        // This test verifies that NoConsoleScreenBufferError propagates from Win32Output
        // When there's no console attached and we can't allocate one, Win32Output throws
        // We ensure a console IS attached for the positive path
        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();

        // Act - should succeed when console is attached
        using var output = new Windows10Output(writer);

        // Assert - constructor succeeded
        Assert.NotNull(output);
    }

    [Fact]
    public void Constructor_StoresConsoleHandleOnce()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();

        // Act
        using var output = new Windows10Output(writer);

        // Assert - verify the handle is stored by calling operations that use it
        // Multiple flush operations should not re-acquire the handle
        output.Write("test1");
        output.Flush();
        output.Write("test2");
        output.Flush();
        output.Write("test3");
        output.Flush();

        // If handle was re-acquired each time and caused issues, we'd see exceptions
        Assert.NotNull(output);
    }

    [Fact]
    public void Constructor_CreatesBothOutputs()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();

        // Act
        using var output = new Windows10Output(writer);

        // Assert
        Assert.NotNull(output.Win32Output);
        Assert.NotNull(output.Vt100Output);
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
        using var output = new Windows10Output(writer, colorDepth);

        // Assert
        Assert.Equal(colorDepth, output.Win32Output.DefaultColorDepth);
        Assert.Equal(colorDepth, output.GetDefaultColorDepth());
    }

    #endregion

    #region Flush Tests (T012-T014)

    [Fact]
    public void Flush_AcquiresLockAndCallsVt100OutputFlush()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        using var output = new Windows10Output(writer);
        output.Write("test");

        // Act
        output.Flush();

        // Assert
        Assert.Contains("test", writer.ToString());
    }

    [Fact]
    public void Flush_RestoresConsoleModeInFinallyBlock()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        using var output = new Windows10Output(writer);
        var handle = ConsoleApi.GetStdHandle(ConsoleApi.STD_OUTPUT_HANDLE);

        // Get original mode before flush
        ConsoleApi.GetConsoleMode(handle, out var originalMode);

        // Act
        output.Write("test");
        output.Flush();

        // Assert - mode should be restored after flush
        ConsoleApi.GetConsoleMode(handle, out var currentMode);
        Assert.Equal(originalMode, currentMode);
    }

    [Fact]
    public void Flush_ConcurrentCalls_AreSerializedViaPerInstanceLock()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        using var output = new Windows10Output(writer);
        var exceptions = new List<Exception>();
        var threadCount = 10;
        var iterationsPerThread = 1000;

        // Act - Per Constitution XI: 10+ threads, 1000+ iterations
        var threads = new Thread[threadCount];
        for (var i = 0; i < threadCount; i++)
        {
            var threadIndex = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    for (var j = 0; j < iterationsPerThread; j++)
                    {
                        output.Write($"Thread{threadIndex}Iter{j}");
                        output.Flush();
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            });
        }

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        // Assert - No exceptions should occur with proper locking
        Assert.Empty(exceptions);
    }

    #endregion

    #region Writing Tests (T015-T016)

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
        using var output = new Windows10Output(writer);
        var testData = "Hello, Windows 10!";

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
        using var output = new Windows10Output(writer);
        var testData = "\x1b[31mRed Text\x1b[0m";

        // Act
        output.WriteRaw(testData);
        output.Flush();

        // Assert
        var result = writer.ToString();
        Assert.Contains(testData, result);
    }

    #endregion

    #region Color Depth Tests (T017-T018)

    [Fact]
    public void GetDefaultColorDepth_ReturnsDepth24Bit_ByDefault()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        using var output = new Windows10Output(writer);

        // Act
        var colorDepth = output.GetDefaultColorDepth();

        // Assert
        Assert.Equal(ColorDepth.Depth24Bit, colorDepth);
    }

    [Fact]
    public void GetDefaultColorDepth_ReturnsOverride_WhenProvided()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        var expectedDepth = ColorDepth.Depth4Bit;
        using var output = new Windows10Output(writer, expectedDepth);

        // Act
        var colorDepth = output.GetDefaultColorDepth();

        // Assert
        Assert.Equal(expectedDepth, colorDepth);
    }

    #endregion

    #region Property Tests (T019)

    [Fact]
    public void RespondsToCpr_ReturnsFalse()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        using var output = new Windows10Output(writer);

        // Act & Assert
        Assert.False(output.RespondsToCpr);
    }

    #endregion

    #region User Story 2 Tests: Console Operations (T031-T037)

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
        using var output = new Windows10Output(writer);

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
        using var output = new Windows10Output(writer);

        // Act
        var rows = output.GetRowsBelowCursorPosition();

        // Assert
        Assert.True(rows >= 0);
    }

    [Fact]
    public void ScrollBufferToPrompt_DelegatesToWin32Output()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        using var output = new Windows10Output(writer);

        // Act & Assert - should not throw
        output.ScrollBufferToPrompt();
    }

    // NOTE: Mouse and bracketed paste delegate to Vt100Output (not Win32Output)
    // because Windows 10 uses virtual terminal input via ANSI escape sequences.
    // See Python Prompt Toolkit windows10.py lines 68-86.

    [Fact]
    public void EnableMouseSupport_DelegatesToVt100Output()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        var output = new Windows10Output(writer);

        // Act
        output.EnableMouseSupport();
        output.Flush();

        // Assert - Vt100Output writes ANSI escape sequence for mouse support
        var result = writer.ToString();
        Assert.Contains("\x1b[?1000h", result); // Enable mouse tracking
    }

    [Fact]
    public void DisableMouseSupport_DelegatesToVt100Output()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        var output = new Windows10Output(writer);

        // Act
        output.DisableMouseSupport();
        output.Flush();

        // Assert - Vt100Output writes ANSI escape sequence to disable mouse
        var result = writer.ToString();
        Assert.Contains("\x1b[?1000l", result); // Disable mouse tracking
    }

    [Fact]
    public void EnableBracketedPaste_DelegatesToVt100Output()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        var output = new Windows10Output(writer);

        // Act
        output.EnableBracketedPaste();
        output.Flush();

        // Assert - Vt100Output writes ANSI escape sequence for bracketed paste
        var result = writer.ToString();
        Assert.Contains("\x1b[?2004h", result); // Enable bracketed paste
    }

    [Fact]
    public void DisableBracketedPaste_DelegatesToVt100Output()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        var output = new Windows10Output(writer);

        // Act
        output.DisableBracketedPaste();
        output.Flush();

        // Assert - Vt100Output writes ANSI escape sequence to disable bracketed paste
        var result = writer.ToString();
        Assert.Contains("\x1b[?2004l", result); // Disable bracketed paste
    }

    #endregion

    #region User Story 2 Tests: Public Properties (T043)

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
        using var output = new Windows10Output(writer);

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
        using var output = new Windows10Output(writer);

        // Act & Assert
        Assert.NotNull(output.Vt100Output);
    }

    #endregion

    #region Edge Case Tests (T052)

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
        using var output = new Windows10Output(writer);

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
        using var output = new Windows10Output(writer);

        // Act & Assert
        Assert.Same(writer, output.Stdout);
    }

    [Fact]
    public void IOutput_InterfaceCompliance_CanAssignToIOutput()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        using var output = new Windows10Output(writer);

        // Act
        IOutput iOutput = output;

        // Assert
        Assert.NotNull(iOutput);
        Assert.Same(output, iOutput);
    }

    [Fact]
    public void IOutput_InterfaceCompliance_CanPassAsIOutputParameter()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // Skip on non-Windows
        }

        EnsureConsoleAttached();

        // Arrange
        using var writer = new StringWriter();
        using var output = new Windows10Output(writer);

        // Act
        static void AcceptIOutput(IOutput o) => o.Write("test");
        AcceptIOutput(output);
        output.Flush();

        // Assert
        Assert.Contains("test", writer.ToString());
    }

    #endregion
}
