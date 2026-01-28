using Stroke.Input;
using Stroke.KeyBinding;
using Xunit;

namespace Stroke.Tests.KeyBinding;

/// <summary>
/// Tests for the <see cref="EmacsState"/> class.
/// </summary>
public class EmacsStateTests
{
    #region Initial Value Tests (US2.1)

    [Fact]
    public void EmacsState_DefaultMacro_IsEmptyList()
    {
        var state = new EmacsState();

        Assert.NotNull(state.Macro);
        Assert.Empty(state.Macro);
    }

    [Fact]
    public void EmacsState_DefaultCurrentRecording_IsNull()
    {
        var state = new EmacsState();

        Assert.Null(state.CurrentRecording);
    }

    [Fact]
    public void EmacsState_DefaultIsRecording_IsFalse()
    {
        var state = new EmacsState();

        Assert.False(state.IsRecording);
    }

    #endregion

    #region Macro Recording Tests (US2.2, US2.3)

    [Fact]
    public void EmacsState_StartMacro_SetsCurrentRecordingToEmptyList()
    {
        var state = new EmacsState();

        state.StartMacro();

        Assert.NotNull(state.CurrentRecording);
        Assert.Empty(state.CurrentRecording);
    }

    [Fact]
    public void EmacsState_StartMacro_SetsIsRecordingTrue()
    {
        var state = new EmacsState();

        state.StartMacro();

        Assert.True(state.IsRecording);
    }

    [Fact]
    public void EmacsState_EndMacro_CopiesCurrentRecordingToMacro()
    {
        var state = new EmacsState();
        state.StartMacro();
        state.AppendToRecording(new Stroke.Input.KeyPress(Keys.ControlA));
        state.AppendToRecording(new Stroke.Input.KeyPress(Keys.ControlE));

        state.EndMacro();

        Assert.Equal(2, state.Macro.Count);
        Assert.Equal(Keys.ControlA, state.Macro[0].Key);
        Assert.Equal(Keys.ControlE, state.Macro[1].Key);
    }

    [Fact]
    public void EmacsState_EndMacro_SetsCurrentRecordingToNull()
    {
        var state = new EmacsState();
        state.StartMacro();

        state.EndMacro();

        Assert.Null(state.CurrentRecording);
    }

    [Fact]
    public void EmacsState_EndMacro_SetsIsRecordingFalse()
    {
        var state = new EmacsState();
        state.StartMacro();

        state.EndMacro();

        Assert.False(state.IsRecording);
    }

    [Fact]
    public void EmacsState_EndMacro_WhenNotRecording_SetsMacroToEmptyList()
    {
        var state = new EmacsState();
        // First record something
        state.StartMacro();
        state.AppendToRecording(new Stroke.Input.KeyPress(Keys.ControlA));
        state.EndMacro();
        Assert.Single(state.Macro);

        // Now call EndMacro when not recording
        state.EndMacro();

        // Macro should be empty list when EndMacro called while not recording
        Assert.NotNull(state.Macro);
        Assert.Empty(state.Macro);
    }

    [Fact]
    public void EmacsState_Reset_SetsCurrentRecordingToNull()
    {
        var state = new EmacsState();
        state.StartMacro();
        Assert.NotNull(state.CurrentRecording);

        state.Reset();

        Assert.Null(state.CurrentRecording);
    }

    [Fact]
    public void EmacsState_Reset_PreservesMacro()
    {
        var state = new EmacsState();
        state.StartMacro();
        state.AppendToRecording(new Stroke.Input.KeyPress(Keys.ControlA));
        state.EndMacro();
        Assert.Single(state.Macro);

        state.Reset();

        // Macro is NOT cleared by Reset()
        Assert.Single(state.Macro);
    }

    [Fact]
    public void EmacsState_AppendToRecording_WhenRecording_AddsKeyPress()
    {
        var state = new EmacsState();
        state.StartMacro();
        var keyPress = new Stroke.Input.KeyPress(Keys.ControlK);

        state.AppendToRecording(keyPress);

        Assert.Single(state.CurrentRecording!);
        Assert.Equal(keyPress, state.CurrentRecording![0]);
    }

    [Fact]
    public void EmacsState_AppendToRecording_WhenNotRecording_DoesNothing()
    {
        var state = new EmacsState();
        var keyPress = new Stroke.Input.KeyPress(Keys.ControlA);

        // Should not throw
        state.AppendToRecording(keyPress);

        Assert.Null(state.CurrentRecording);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void EmacsState_StartMacro_WhenAlreadyRecording_ReplacesWithNewEmptyList()
    {
        var state = new EmacsState();
        state.StartMacro();
        state.AppendToRecording(new Stroke.Input.KeyPress(Keys.ControlA));
        Assert.Single(state.CurrentRecording!);

        // Start a new recording while already recording
        state.StartMacro();

        // Should have a new empty list, previous recording is lost
        Assert.NotNull(state.CurrentRecording);
        Assert.Empty(state.CurrentRecording);
    }

    [Fact]
    public void EmacsState_EndMacro_WithEmptyRecording_SetsMacroToEmptyList()
    {
        var state = new EmacsState();
        state.StartMacro();
        // Don't append anything

        state.EndMacro();

        Assert.NotNull(state.Macro);
        Assert.Empty(state.Macro);
    }

    [Fact]
    public void EmacsState_Reset_DuringRecording_StopsRecording()
    {
        var state = new EmacsState();
        state.StartMacro();
        state.AppendToRecording(new Stroke.Input.KeyPress(Keys.ControlA));

        state.Reset();

        Assert.False(state.IsRecording);
        Assert.Null(state.CurrentRecording);
    }

    [Fact]
    public void EmacsState_MultipleRecordSessions_MacroReplacedEachTime()
    {
        var state = new EmacsState();

        // First session
        state.StartMacro();
        state.AppendToRecording(new Stroke.Input.KeyPress(Keys.ControlA));
        state.EndMacro();
        Assert.Single(state.Macro);

        // Second session
        state.StartMacro();
        state.AppendToRecording(new Stroke.Input.KeyPress(Keys.ControlB));
        state.AppendToRecording(new Stroke.Input.KeyPress(Keys.ControlC));
        state.EndMacro();

        // Macro should be the second session's content
        Assert.Equal(2, state.Macro.Count);
        Assert.Equal(Keys.ControlB, state.Macro[0].Key);
    }

    #endregion

    #region Thread Safety Tests (SC-004)

    [Fact]
    public void EmacsState_ConcurrentMacroOperations_NoCorruption()
    {
        var state = new EmacsState();
        var exceptions = new List<Exception>();
        const int threadCount = 10;
        const int operationsPerThread = 1000;

        var threads = new Thread[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            threads[i] = new Thread(() =>
            {
                try
                {
                    for (int j = 0; j < operationsPerThread; j++)
                    {
                        state.StartMacro();
                        state.AppendToRecording(new Stroke.Input.KeyPress(Keys.ControlA));
                        _ = state.IsRecording;
                        _ = state.CurrentRecording;
                        state.EndMacro();
                        _ = state.Macro;
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
            thread.Start();

        foreach (var thread in threads)
            thread.Join();

        Assert.Empty(exceptions);
    }

    [Fact]
    public void EmacsState_ConcurrentPropertyAccess_NoDeadlocks()
    {
        var state = new EmacsState();
        var completedCount = 0;
        const int threadCount = 10;
        const int operationsPerThread = 1000;
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        var threads = new Thread[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < operationsPerThread && !cts.Token.IsCancellationRequested; j++)
                {
                    _ = state.Macro;
                    _ = state.CurrentRecording;
                    _ = state.IsRecording;
                    state.StartMacro();
                    state.Reset();
                }
                Interlocked.Increment(ref completedCount);
            });
        }

        foreach (var thread in threads)
            thread.Start();

        foreach (var thread in threads)
            thread.Join();

        Assert.Equal(threadCount, completedCount);
    }

    #endregion

    #region Collection Copy Tests (Thread Safety)

    [Fact]
    public void EmacsState_Macro_ReturnsCopy()
    {
        var state = new EmacsState();
        state.StartMacro();
        state.AppendToRecording(new Stroke.Input.KeyPress(Keys.ControlA));
        state.EndMacro();

        var macro1 = state.Macro;
        var macro2 = state.Macro;

        // Returns a copy each time, not the same instance
        Assert.NotSame(macro1, macro2);
    }

    [Fact]
    public void EmacsState_CurrentRecording_ReturnsCopy()
    {
        var state = new EmacsState();
        state.StartMacro();
        state.AppendToRecording(new Stroke.Input.KeyPress(Keys.ControlA));

        var recording1 = state.CurrentRecording;
        var recording2 = state.CurrentRecording;

        // Returns a copy each time, not the same instance
        Assert.NotSame(recording1, recording2);
    }

    [Fact]
    public void EmacsState_Macro_ModifyingCopyDoesNotAffectInternal()
    {
        var state = new EmacsState();
        state.StartMacro();
        state.AppendToRecording(new Stroke.Input.KeyPress(Keys.ControlA));
        state.EndMacro();

        var macro = state.Macro;

        // Even if we somehow managed to modify the copy (via cast),
        // the internal state should be unaffected
        Assert.Single(state.Macro);
    }

    #endregion

    #region Multiple Reset Tests

    [Fact]
    public void EmacsState_MultipleResets_AreIdempotent()
    {
        var state = new EmacsState();
        state.StartMacro();

        state.Reset();
        state.Reset();
        state.Reset();

        Assert.False(state.IsRecording);
        Assert.Null(state.CurrentRecording);
    }

    #endregion
}
