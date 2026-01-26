using Stroke.Input;
using Stroke.Input.Pipe;
using Xunit;

namespace Stroke.Tests.Input;

/// <summary>
/// Tests for raw mode context handling.
/// Note: These tests use DummyInput/PipeInput which have no-op RawMode().
/// Platform-specific raw mode behavior is tested in integration tests.
/// </summary>
public class RawModeContextTests
{
    #region T040: Enter/Exit and Dispose Pattern

    [Fact]
    public void RawMode_OnDummyInput_ReturnsDisposable()
    {
        using var input = new DummyInput();
        using var rawMode = input.RawMode();

        Assert.NotNull(rawMode);
    }

    [Fact]
    public void RawMode_Dispose_DoesNotThrow()
    {
        using var input = new DummyInput();
        var rawMode = input.RawMode();

        // Should not throw
        rawMode.Dispose();
    }

    [Fact]
    public void RawMode_DisposeMultipleTimes_DoesNotThrow()
    {
        using var input = new DummyInput();
        var rawMode = input.RawMode();

        rawMode.Dispose();
        rawMode.Dispose(); // Second dispose should be no-op
    }

    [Fact]
    public void RawMode_UsingStatement_DisposesCorrectly()
    {
        using var input = new DummyInput();

        using (var rawMode = input.RawMode())
        {
            // In raw mode
            Assert.NotNull(rawMode);
        }
        // Exited raw mode via dispose
    }

    [Fact]
    public void RawMode_NestedContexts_HandledCorrectly()
    {
        using var input = new DummyInput();

        using (var rawMode1 = input.RawMode())
        {
            using (var rawMode2 = input.RawMode())
            {
                // Both contexts active
                Assert.NotNull(rawMode1);
                Assert.NotNull(rawMode2);
            }
            // Inner context disposed
        }
        // Outer context disposed
    }

    [Fact]
    public void RawMode_OnPipeInput_ReturnsDisposable()
    {
        using var pipe = InputFactory.CreatePipe();
        using var rawMode = pipe.RawMode();

        Assert.NotNull(rawMode);
    }

    [Fact]
    public void RawMode_PipeInput_DoesNotAffectParsing()
    {
        using var pipe = InputFactory.CreatePipe();

        using (var rawMode = pipe.RawMode())
        {
            pipe.SendText("\x1b[A");
            var keys = pipe.ReadKeys();

            Assert.Single(keys);
            Assert.Equal(Keys.Up, keys[0].Key);
        }
    }

    #endregion

    #region T041: Non-TTY Handling (Graceful No-op)

    [Fact]
    public void RawMode_OnDummyInput_GracefullyHandlesNonTty()
    {
        // DummyInput represents non-TTY scenarios
        using var input = new DummyInput();

        // Should not throw on non-TTY
        using var rawMode = input.RawMode();
        Assert.NotNull(rawMode);
    }

    [Fact]
    public void RawMode_OnPipeInput_GracefullyHandlesNonTty()
    {
        // PipeInput also represents non-TTY scenarios
        using var pipe = InputFactory.CreatePipe();

        // Should not throw on non-TTY
        using var rawMode = pipe.RawMode();
        Assert.NotNull(rawMode);
    }

    [Fact]
    public void RawMode_OnClosedInput_GracefullyHandles()
    {
        using var pipe = InputFactory.CreatePipe();
        pipe.Close();

        // Depending on implementation, may throw or return no-op
        // The key is it should not cause unexpected behavior
        try
        {
            using var rawMode = pipe.RawMode();
            Assert.NotNull(rawMode);
        }
        catch (ObjectDisposedException)
        {
            // Also acceptable behavior for closed input
        }
    }

    [Fact]
    public void CookedMode_OnDummyInput_ReturnsDisposable()
    {
        using var input = new DummyInput();
        using var cookedMode = input.CookedMode();

        Assert.NotNull(cookedMode);
    }

    [Fact]
    public void CookedMode_OnPipeInput_ReturnsDisposable()
    {
        using var pipe = InputFactory.CreatePipe();
        using var cookedMode = pipe.CookedMode();

        Assert.NotNull(cookedMode);
    }

    [Fact]
    public void CookedMode_AfterRawMode_ReturnsDisposable()
    {
        using var input = new DummyInput();

        using (var rawMode = input.RawMode())
        {
            using (var cookedMode = input.CookedMode())
            {
                // In cooked mode within raw mode
                Assert.NotNull(cookedMode);
            }
            // Back to raw mode
        }
        // Back to original state
    }

    #endregion
}
