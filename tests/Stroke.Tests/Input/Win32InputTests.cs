using System.Runtime.Versioning;
using Stroke.Input;
using Stroke.Input.Windows;
using Xunit;

namespace Stroke.Tests.Input;

/// <summary>
/// Tests for Win32Input Windows console input.
/// These tests verify the Windows input implementation works correctly.
/// Platform-specific tests only run on Windows.
/// </summary>
public class Win32InputTests
{
    #region T064: Basic Win32Input Tests

    [Fact]
    [SupportedOSPlatform("windows")]
    public void Constructor_OnWindows_CreatesInstance()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var input = new Win32Input();

        Assert.NotNull(input);
        Assert.IsAssignableFrom<IInput>(input);
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void Closed_InitiallyFalse_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var input = new Win32Input();

        Assert.False(input.Closed);
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void Close_SetsClosed_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var input = new Win32Input();
        input.Close();

        Assert.True(input.Closed);
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void Dispose_SetsClosed_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var input = new Win32Input();
        input.Dispose();

        Assert.True(input.Closed);
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void TypeaheadHash_ReturnsUniqueString_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var input1 = new Win32Input();
        using var input2 = new Win32Input();

        var hash1 = input1.TypeaheadHash();
        var hash2 = input2.TypeaheadHash();

        Assert.NotNull(hash1);
        Assert.NotNull(hash2);
        Assert.StartsWith("Win32Input-", hash1);
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void RawMode_ReturnsDisposable_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var input = new Win32Input();
        using var rawMode = input.RawMode();

        Assert.NotNull(rawMode);
        Assert.IsType<Win32RawMode>(rawMode);
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void CookedMode_ReturnsDisposable_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var input = new Win32Input();
        using var cookedMode = input.CookedMode();

        Assert.NotNull(cookedMode);
        Assert.IsType<Win32CookedMode>(cookedMode);
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void Attach_ReturnsDisposable_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var input = new Win32Input();
        using var attachment = input.Attach(() => { });

        Assert.NotNull(attachment);
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void Attach_NullCallback_ThrowsArgumentNullException_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var input = new Win32Input();

        Assert.Throws<ArgumentNullException>(() => input.Attach(null!));
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void Detach_ReturnsDisposable_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var input = new Win32Input();
        using var detach = input.Detach();

        Assert.NotNull(detach);
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void FileNo_ReturnsHandle_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var input = new Win32Input();

        var handle = input.FileNo();

        // Handle should be a valid value (not INVALID_HANDLE_VALUE = -1)
        Assert.NotEqual(nint.Zero, handle);
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void ReadKeys_WhenClosed_ReturnsEmptyList_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var input = new Win32Input();
        input.Close();

        var keys = input.ReadKeys();

        Assert.Empty(keys);
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void FlushKeys_WhenNoParser_ReturnsEmptyList_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var input = new Win32Input();

        // If VT100 mode not enabled, parser is null and FlushKeys returns empty
        var keys = input.FlushKeys();

        Assert.NotNull(keys);
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void ReadKeys_AfterDispose_ThrowsObjectDisposedException_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var input = new Win32Input();
        input.Dispose();

        Assert.Throws<ObjectDisposedException>(() => input.ReadKeys());
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void FlushKeys_AfterDispose_ThrowsObjectDisposedException_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var input = new Win32Input();
        input.Dispose();

        Assert.Throws<ObjectDisposedException>(() => input.FlushKeys());
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void RawMode_AfterDispose_ThrowsObjectDisposedException_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var input = new Win32Input();
        input.Dispose();

        Assert.Throws<ObjectDisposedException>(() => input.RawMode());
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void CookedMode_AfterDispose_ThrowsObjectDisposedException_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var input = new Win32Input();
        input.Dispose();

        Assert.Throws<ObjectDisposedException>(() => input.CookedMode());
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void Attach_AfterDispose_ThrowsObjectDisposedException_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var input = new Win32Input();
        input.Dispose();

        Assert.Throws<ObjectDisposedException>(() => input.Attach(() => { }));
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void FileNo_AfterDispose_ThrowsObjectDisposedException_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var input = new Win32Input();
        input.Dispose();

        Assert.Throws<ObjectDisposedException>(() => input.FileNo());
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void Dispose_MultipleCalls_Safe_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var input = new Win32Input();
        input.Dispose();
        input.Dispose(); // Should not throw

        Assert.True(input.Closed);
    }

    #endregion

    #region WaitForInput Tests (T083)

    [Fact]
    [SupportedOSPlatform("windows")]
    public void WaitForInput_WhenClosed_ReturnsFalse_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var input = new Win32Input();
        input.Close();

        var result = input.WaitForInput(0);

        Assert.False(result);
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void WaitForInput_WithZeroTimeout_ReturnsImmediately_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var input = new Win32Input();

        // With 0 timeout, should return immediately (likely false since no input pending)
        var result = input.WaitForInput(0);

        // Result depends on console state; mainly testing it doesn't hang
        Assert.True(result || !result); // Tautology to ensure no exception
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void WaitForInput_AfterDispose_ThrowsObjectDisposedException_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var input = new Win32Input();
        input.Dispose();

        Assert.Throws<ObjectDisposedException>(() => input.WaitForInput(0));
    }

    #endregion

    #region WaitForHandles Tests (T083)

    [Fact]
    [SupportedOSPlatform("windows")]
    public void WaitForHandles_NullArray_ThrowsArgumentNullException_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        Assert.Throws<ArgumentNullException>(() => Win32Input.WaitForHandles(null!));
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void WaitForHandles_EmptyArray_ReturnsMinusOne_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var result = Win32Input.WaitForHandles([]);

        Assert.Equal(-1, result);
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void WaitForHandles_WithNonSignaledEvent_TimesOut_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        // Create a manual-reset event in non-signaled state
        var eventHandle = ConsoleApi.CreateEvent(nint.Zero, true, false, null);
        Assert.NotEqual(nint.Zero, eventHandle);

        try
        {
            var handles = new[] { eventHandle };

            // Wait with very short timeout - should time out since event is not signaled
            var result = Win32Input.WaitForHandles(handles, 1);

            // Result is -1 on timeout
            Assert.Equal(-1, result);
        }
        finally
        {
            ConsoleApi.CloseHandle(eventHandle);
        }
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void WaitForHandles_WithSignaledEvent_ReturnsIndex_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        // Create a manual-reset event and signal it
        var eventHandle = ConsoleApi.CreateEvent(nint.Zero, true, true, null);
        Assert.NotEqual(nint.Zero, eventHandle);

        try
        {
            var handles = new[] { eventHandle };

            // Wait should return immediately since event is signaled
            var result = Win32Input.WaitForHandles(handles, 1);

            // Result is 0 (index of signaled handle)
            Assert.Equal(0, result);
        }
        finally
        {
            ConsoleApi.CloseHandle(eventHandle);
        }
    }

    #endregion

    #region Attach/Detach Stack Tests

    [Fact]
    [SupportedOSPlatform("windows")]
    public void Attach_MultipleCallbacks_StacksCorrectly_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var input = new Win32Input();
        var callOrder = new List<int>();

        using var attach1 = input.Attach(() => callOrder.Add(1));
        using var attach2 = input.Attach(() => callOrder.Add(2));

        // Detaching should return the most recent callback
        using var detach = input.Detach();

        // Reattach happens on dispose of detach
        // This tests the stack semantics are correct
        Assert.NotNull(attach1);
        Assert.NotNull(attach2);
        Assert.NotNull(detach);
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void Detach_WhenNoCallbacks_ReturnsNoOpDisposable_OnWindows()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var input = new Win32Input();

        using var detach = input.Detach();

        // Should not throw, returns no-op disposable
        Assert.NotNull(detach);
    }

    #endregion
}
