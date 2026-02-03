using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Stroke.Input.Windows;
using Stroke.Input.Windows.Win32Types;
using Xunit;

namespace Stroke.Tests.Input.Windows.Win32Types;

/// <summary>
/// Tests for P/Invoke methods in <see cref="ConsoleApi"/>.
/// These tests verify the correct behavior of Windows Console API interop.
/// </summary>
/// <remarks>
/// Most tests are Windows-only and will be skipped on other platforms.
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed class NativeMethodsTests
{
    private static bool IsWindows => OperatingSystem.IsWindows();

    #region Console Handle Helpers

    /// <summary>
    /// P/Invoke for <c>AllocConsole</c>. Allocates a new Win32 console for the calling process.
    /// </summary>
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AllocConsole();

    /// <summary>
    /// P/Invoke for <c>GetConsoleWindow</c>. Returns the window handle of the attached console,
    /// or <see cref="nint.Zero"/> if no console is attached.
    /// </summary>
    [DllImport("kernel32.dll")]
    private static extern nint GetConsoleWindow();

    /// <summary>
    /// P/Invoke for <c>CreateFileW</c>. Used to open <c>CONIN$</c> and <c>CONOUT$</c>
    /// pseudo-files for direct console access.
    /// </summary>
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern nint CreateFileW(
        string lpFileName, uint dwDesiredAccess, uint dwShareMode,
        nint lpSecurityAttributes, uint dwCreationDisposition,
        uint dwFlagsAndAttributes, nint hTemplateFile);

    private const uint GenericRead = 0x80000000;
    private const uint GenericWrite = 0x40000000;
    private const uint FileShareRead = 0x00000001;
    private const uint FileShareWrite = 0x00000002;
    private const uint OpenExisting = 3;

    /// <summary>
    /// Ensures the process has an attached Win32 console.
    /// Test runners and mintty redirect stdio through pipes, so
    /// <see cref="ConsoleApi.GetStdHandle"/> returns pipe handles that
    /// do not support console-specific APIs. This method allocates a
    /// real console when none is attached.
    /// </summary>
    private static void EnsureConsoleAttached()
    {
        if (GetConsoleWindow() == nint.Zero)
        {
            AllocConsole();
        }
    }

    /// <summary>
    /// Opens a real console input handle via <c>CONIN$</c>, bypassing stdio redirection.
    /// Caller must close the returned handle via <see cref="ConsoleApi.CloseHandle"/>.
    /// </summary>
    private static nint OpenConsoleInputHandle()
    {
        EnsureConsoleAttached();
        return CreateFileW("CONIN$", GenericRead | GenericWrite,
            FileShareRead | FileShareWrite, nint.Zero, OpenExisting, 0, nint.Zero);
    }

    /// <summary>
    /// Opens a real console output handle via <c>CONOUT$</c>, bypassing stdio redirection.
    /// Caller must close the returned handle via <see cref="ConsoleApi.CloseHandle"/>.
    /// </summary>
    private static nint OpenConsoleOutputHandle()
    {
        EnsureConsoleAttached();
        return CreateFileW("CONOUT$", GenericRead | GenericWrite,
            FileShareRead | FileShareWrite, nint.Zero, OpenExisting, 0, nint.Zero);
    }

    #endregion

    #region GetStdHandle Tests

    [Fact]
    public void GetStdHandle_OnWindows_ReturnsValidHandle()
    {
        if (!IsWindows)
        {
            return; // Skip on non-Windows
        }

        var handle = ConsoleApi.GetStdHandle(StdHandles.STD_OUTPUT_HANDLE);

        // Handle should not be invalid (0 or -1)
        Assert.NotEqual(nint.Zero, handle);
        Assert.NotEqual(ConsoleApi.INVALID_HANDLE_VALUE, handle);
    }

    [Fact]
    public void GetStdHandle_InputHandle_OnWindows_ReturnsValidHandle()
    {
        if (!IsWindows)
        {
            return; // Skip on non-Windows
        }

        var handle = ConsoleApi.GetStdHandle(StdHandles.STD_INPUT_HANDLE);

        Assert.NotEqual(nint.Zero, handle);
        Assert.NotEqual(ConsoleApi.INVALID_HANDLE_VALUE, handle);
    }

    [Fact]
    public void GetStdHandle_ErrorHandle_OnWindows_ReturnsValidHandle()
    {
        if (!IsWindows)
        {
            return; // Skip on non-Windows
        }

        var handle = ConsoleApi.GetStdHandle(StdHandles.STD_ERROR_HANDLE);

        Assert.NotEqual(nint.Zero, handle);
        Assert.NotEqual(ConsoleApi.INVALID_HANDLE_VALUE, handle);
    }

    #endregion

    #region GetConsoleMode/SetConsoleMode Tests

    [Fact]
    public void GetConsoleMode_OnWindows_Succeeds()
    {
        if (!IsWindows)
        {
            return; // Skip on non-Windows
        }

        var handle = OpenConsoleInputHandle();
        try
        {
            Assert.NotEqual(ConsoleApi.INVALID_HANDLE_VALUE, handle);
            var result = ConsoleApi.GetConsoleMode(handle, out var mode);
            Assert.True(result, "GetConsoleMode should succeed");
        }
        finally
        {
            ConsoleApi.CloseHandle(handle);
        }
    }

    [Fact]
    public void SetConsoleMode_OnWindows_Succeeds()
    {
        if (!IsWindows)
        {
            return; // Skip on non-Windows
        }

        var handle = OpenConsoleInputHandle();
        try
        {
            Assert.NotEqual(ConsoleApi.INVALID_HANDLE_VALUE, handle);

            // Get current mode
            var getResult = ConsoleApi.GetConsoleMode(handle, out var originalMode);
            Assert.True(getResult, "GetConsoleMode should succeed");

            // Set mode back to same value (safe operation)
            var setResult = ConsoleApi.SetConsoleMode(handle, originalMode);
            Assert.True(setResult, "SetConsoleMode should succeed");
        }
        finally
        {
            ConsoleApi.CloseHandle(handle);
        }
    }

    #endregion

    #region GetConsoleScreenBufferInfo Tests

    [Fact]
    public void GetConsoleScreenBufferInfo_OnWindows_Succeeds()
    {
        if (!IsWindows)
        {
            return; // Skip on non-Windows
        }

        var handle = OpenConsoleOutputHandle();
        try
        {
            Assert.NotEqual(ConsoleApi.INVALID_HANDLE_VALUE, handle);
            var result = ConsoleApi.GetConsoleScreenBufferInfo(handle, out var info);

            Assert.True(result, "GetConsoleScreenBufferInfo should succeed");

            // Verify sensible values are populated
            Assert.True(info.Size.X > 0, "Buffer width should be positive");
            Assert.True(info.Size.Y > 0, "Buffer height should be positive");
            Assert.True(info.MaximumWindowSize.X > 0, "Max window width should be positive");
            Assert.True(info.MaximumWindowSize.Y > 0, "Max window height should be positive");
        }
        finally
        {
            ConsoleApi.CloseHandle(handle);
        }
    }

    [Fact]
    public void GetConsoleScreenBufferInfo_PopulatesAllFields()
    {
        if (!IsWindows)
        {
            return; // Skip on non-Windows
        }

        var handle = OpenConsoleOutputHandle();
        try
        {
            Assert.NotEqual(ConsoleApi.INVALID_HANDLE_VALUE, handle);
            var result = ConsoleApi.GetConsoleScreenBufferInfo(handle, out var info);

            Assert.True(result, "GetConsoleScreenBufferInfo should succeed");

            // Size should be reasonable for a console
            Assert.InRange(info.Size.X, 1, 10000);
            Assert.InRange(info.Size.Y, 1, 100000);

            // Cursor should be within buffer bounds
            Assert.InRange(info.CursorPosition.X, 0, info.Size.X - 1);
            Assert.InRange(info.CursorPosition.Y, 0, info.Size.Y - 1);

            // Window should have valid dimensions
            Assert.True(info.Window.Width > 0, "Window width should be positive");
            Assert.True(info.Window.Height > 0, "Window height should be positive");
        }
        finally
        {
            ConsoleApi.CloseHandle(handle);
        }
    }

    #endregion

    #region Event Functions Tests

    [Fact]
    public void CreateEvent_OnWindows_ReturnsValidHandle()
    {
        if (!IsWindows)
        {
            return; // Skip on non-Windows
        }

        var handle = ConsoleApi.CreateEvent(nint.Zero, true, false, null);

        try
        {
            Assert.NotEqual(nint.Zero, handle);
        }
        finally
        {
            if (handle != nint.Zero)
            {
                ConsoleApi.CloseHandle(handle);
            }
        }
    }

    [Fact]
    public void SetEvent_ResetEvent_OnWindows_Succeeds()
    {
        if (!IsWindows)
        {
            return; // Skip on non-Windows
        }

        var handle = ConsoleApi.CreateEvent(nint.Zero, true, false, null);

        try
        {
            Assert.NotEqual(nint.Zero, handle);

            var setResult = ConsoleApi.SetEvent(handle);
            Assert.True(setResult, "SetEvent should succeed");

            var resetResult = ConsoleApi.ResetEvent(handle);
            Assert.True(resetResult, "ResetEvent should succeed");
        }
        finally
        {
            if (handle != nint.Zero)
            {
                ConsoleApi.CloseHandle(handle);
            }
        }
    }

    [Fact]
    public void CloseHandle_OnWindows_Succeeds()
    {
        if (!IsWindows)
        {
            return; // Skip on non-Windows
        }

        var handle = ConsoleApi.CreateEvent(nint.Zero, true, false, null);
        Assert.NotEqual(nint.Zero, handle);

        var result = ConsoleApi.CloseHandle(handle);
        Assert.True(result, "CloseHandle should succeed");
    }

    #endregion

    #region Wait Functions Tests

    [Fact]
    public void WaitForSingleObject_OnSignaledEvent_ReturnsImmediately()
    {
        if (!IsWindows)
        {
            return; // Skip on non-Windows
        }

        var handle = ConsoleApi.CreateEvent(nint.Zero, true, true, null); // Initially signaled

        try
        {
            var result = ConsoleApi.WaitForSingleObject(handle, 0);
            Assert.Equal(ConsoleApi.WAIT_OBJECT_0, result);
        }
        finally
        {
            if (handle != nint.Zero)
            {
                ConsoleApi.CloseHandle(handle);
            }
        }
    }

    [Fact]
    public void WaitForSingleObject_OnNonSignaledEvent_TimesOut()
    {
        if (!IsWindows)
        {
            return; // Skip on non-Windows
        }

        var handle = ConsoleApi.CreateEvent(nint.Zero, true, false, null); // Initially non-signaled

        try
        {
            var result = ConsoleApi.WaitForSingleObject(handle, 1); // 1ms timeout
            Assert.Equal(ConsoleApi.WAIT_TIMEOUT, result);
        }
        finally
        {
            if (handle != nint.Zero)
            {
                ConsoleApi.CloseHandle(handle);
            }
        }
    }

    [Fact]
    public void WaitForMultipleObjects_OnSignaledEvent_ReturnsCorrectIndex()
    {
        if (!IsWindows)
        {
            return; // Skip on non-Windows
        }

        var handle1 = ConsoleApi.CreateEvent(nint.Zero, true, false, null);
        var handle2 = ConsoleApi.CreateEvent(nint.Zero, true, true, null); // Signaled

        try
        {
            var handles = new[] { handle1, handle2 };
            var result = ConsoleApi.WaitForMultipleObjects(2, handles, false, 0);

            // Should return index of signaled handle (1) plus WAIT_OBJECT_0
            Assert.Equal(ConsoleApi.WAIT_OBJECT_0 + 1, result);
        }
        finally
        {
            if (handle1 != nint.Zero) ConsoleApi.CloseHandle(handle1);
            if (handle2 != nint.Zero) ConsoleApi.CloseHandle(handle2);
        }
    }

    #endregion

    #region Struct Size Consistency Tests

    [Fact]
    public void InputRecord_SizeMatchesExpected()
    {
        // This verifies our struct layout matches what Windows expects
        Assert.Equal(20, Marshal.SizeOf<InputRecord>());
    }

    [Fact]
    public void ConsoleScreenBufferInfo_SizeMatchesExpected()
    {
        Assert.Equal(22, Marshal.SizeOf<ConsoleScreenBufferInfo>());
    }

    [Fact]
    public void Coord_SizeMatchesExpected()
    {
        Assert.Equal(4, Marshal.SizeOf<Coord>());
    }

    [Fact]
    public void SmallRect_SizeMatchesExpected()
    {
        Assert.Equal(8, Marshal.SizeOf<SmallRect>());
    }

    [Fact]
    public void CharInfo_SizeMatchesExpected()
    {
        Assert.Equal(4, Marshal.SizeOf<CharInfo>());
    }

    #endregion
}
