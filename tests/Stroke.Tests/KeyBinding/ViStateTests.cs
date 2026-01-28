using Stroke.Clipboard;
using Stroke.KeyBinding;
using Xunit;

namespace Stroke.Tests.KeyBinding;

/// <summary>
/// Tests for the <see cref="ViState"/> class.
/// </summary>
public class ViStateTests
{
    #region Initial Value Tests (US1.1)

    [Fact]
    public void ViState_DefaultInputMode_IsInsert()
    {
        var state = new ViState();

        Assert.Equal(InputMode.Insert, state.InputMode);
    }

    [Fact]
    public void ViState_DefaultLastCharacterFind_IsNull()
    {
        var state = new ViState();

        Assert.Null(state.LastCharacterFind);
    }

    [Fact]
    public void ViState_DefaultOperatorFunc_IsNull()
    {
        var state = new ViState();

        Assert.Null(state.OperatorFunc);
    }

    [Fact]
    public void ViState_DefaultOperatorArg_IsNull()
    {
        var state = new ViState();

        Assert.Null(state.OperatorArg);
    }

    [Fact]
    public void ViState_DefaultWaitingForDigraph_IsFalse()
    {
        var state = new ViState();

        Assert.False(state.WaitingForDigraph);
    }

    [Fact]
    public void ViState_DefaultDigraphSymbol1_IsNull()
    {
        var state = new ViState();

        Assert.Null(state.DigraphSymbol1);
    }

    [Fact]
    public void ViState_DefaultTildeOperator_IsFalse()
    {
        var state = new ViState();

        Assert.False(state.TildeOperator);
    }

    [Fact]
    public void ViState_DefaultRecordingRegister_IsNull()
    {
        var state = new ViState();

        Assert.Null(state.RecordingRegister);
    }

    [Fact]
    public void ViState_DefaultCurrentRecording_IsEmptyString()
    {
        var state = new ViState();

        Assert.Equal("", state.CurrentRecording);
    }

    [Fact]
    public void ViState_DefaultTemporaryNavigationMode_IsFalse()
    {
        var state = new ViState();

        Assert.False(state.TemporaryNavigationMode);
    }

    #endregion

    #region InputMode Transition Tests (US1.2)

    [Fact]
    public void ViState_SetNavigationMode_ClearsOperatorFunc()
    {
        var state = new ViState();
        state.OperatorFunc = (_, _) => NotImplementedOrNone.None;
        Assert.NotNull(state.OperatorFunc);

        state.InputMode = InputMode.Navigation;

        Assert.Null(state.OperatorFunc);
    }

    [Fact]
    public void ViState_SetNavigationMode_ClearsOperatorArg()
    {
        var state = new ViState();
        state.OperatorArg = 5;
        Assert.Equal(5, state.OperatorArg);

        state.InputMode = InputMode.Navigation;

        Assert.Null(state.OperatorArg);
    }

    [Fact]
    public void ViState_SetNavigationMode_ClearsWaitingForDigraph()
    {
        var state = new ViState();
        state.WaitingForDigraph = true;
        Assert.True(state.WaitingForDigraph);

        state.InputMode = InputMode.Navigation;

        Assert.False(state.WaitingForDigraph);
    }

    [Fact]
    public void ViState_SetNavigationMode_PreservesDigraphSymbol1()
    {
        var state = new ViState();
        state.DigraphSymbol1 = "a";
        Assert.Equal("a", state.DigraphSymbol1);

        state.InputMode = InputMode.Navigation;

        // DigraphSymbol1 is preserved (not cleared)
        Assert.Equal("a", state.DigraphSymbol1);
    }

    [Fact]
    public void ViState_SetInsertMode_DoesNotClearOperatorFunc()
    {
        var state = new ViState();
        state.InputMode = InputMode.Navigation;
        state.OperatorFunc = (_, _) => NotImplementedOrNone.None;

        state.InputMode = InputMode.Insert;

        // Only Navigation mode clears these, not other modes
        Assert.NotNull(state.OperatorFunc);
    }

    [Fact]
    public void ViState_SetReplaceMode_DoesNotClearOperatorFunc()
    {
        var state = new ViState();
        state.OperatorFunc = (_, _) => NotImplementedOrNone.None;

        state.InputMode = InputMode.Replace;

        Assert.NotNull(state.OperatorFunc);
    }

    #endregion

    #region Reset Tests (US1.3)

    [Fact]
    public void ViState_Reset_SetsInputModeToInsert()
    {
        var state = new ViState();
        state.InputMode = InputMode.Navigation;

        state.Reset();

        Assert.Equal(InputMode.Insert, state.InputMode);
    }

    [Fact]
    public void ViState_Reset_ClearsOperatorFunc()
    {
        var state = new ViState();
        state.OperatorFunc = (_, _) => NotImplementedOrNone.None;

        state.Reset();

        Assert.Null(state.OperatorFunc);
    }

    [Fact]
    public void ViState_Reset_ClearsOperatorArg()
    {
        var state = new ViState();
        state.OperatorArg = 10;

        state.Reset();

        Assert.Null(state.OperatorArg);
    }

    [Fact]
    public void ViState_Reset_ClearsWaitingForDigraph()
    {
        var state = new ViState();
        state.WaitingForDigraph = true;

        state.Reset();

        Assert.False(state.WaitingForDigraph);
    }

    [Fact]
    public void ViState_Reset_ClearsRecordingRegister()
    {
        var state = new ViState();
        state.RecordingRegister = "a";

        state.Reset();

        Assert.Null(state.RecordingRegister);
    }

    [Fact]
    public void ViState_Reset_ClearsCurrentRecording()
    {
        var state = new ViState();
        state.CurrentRecording = "some keys";

        state.Reset();

        Assert.Equal("", state.CurrentRecording);
    }

    [Fact]
    public void ViState_Reset_PreservesLastCharacterFind()
    {
        var state = new ViState();
        var find = new CharacterFind("x", false);
        state.LastCharacterFind = find;

        state.Reset();

        // LastCharacterFind is NOT cleared by Reset()
        Assert.Equal(find, state.LastCharacterFind);
    }

    [Fact]
    public void ViState_Reset_PreservesNamedRegisters()
    {
        var state = new ViState();
        var data = new ClipboardData("test");
        state.SetNamedRegister("a", data);

        state.Reset();

        // NamedRegisters are NOT cleared by Reset()
        Assert.NotNull(state.GetNamedRegister("a"));
    }

    [Fact]
    public void ViState_Reset_PreservesTildeOperator()
    {
        var state = new ViState();
        state.TildeOperator = true;

        state.Reset();

        // TildeOperator is NOT cleared by Reset()
        Assert.True(state.TildeOperator);
    }

    [Fact]
    public void ViState_Reset_PreservesTemporaryNavigationMode()
    {
        var state = new ViState();
        state.TemporaryNavigationMode = true;

        state.Reset();

        // TemporaryNavigationMode is NOT cleared by Reset()
        Assert.True(state.TemporaryNavigationMode);
    }

    [Fact]
    public void ViState_Reset_PreservesDigraphSymbol1()
    {
        var state = new ViState();
        state.DigraphSymbol1 = "x";

        state.Reset();

        // DigraphSymbol1 is NOT cleared by Reset()
        Assert.Equal("x", state.DigraphSymbol1);
    }

    #endregion

    #region Named Registers Tests (US5)

    [Fact]
    public void ViState_GetNamedRegisterNames_InitiallyEmpty()
    {
        var state = new ViState();

        var names = state.GetNamedRegisterNames();

        Assert.Empty(names);
    }

    [Fact]
    public void ViState_SetNamedRegister_ThenGet_ReturnsData()
    {
        var state = new ViState();
        var data = new ClipboardData("yanked text");

        state.SetNamedRegister("a", data);
        var retrieved = state.GetNamedRegister("a");

        Assert.Same(data, retrieved);
    }

    [Fact]
    public void ViState_ClearNamedRegister_WhenExists_ReturnsTrue()
    {
        var state = new ViState();
        state.SetNamedRegister("a", new ClipboardData("test"));

        var result = state.ClearNamedRegister("a");

        Assert.True(result);
        Assert.Null(state.GetNamedRegister("a"));
    }

    [Fact]
    public void ViState_ClearNamedRegister_WhenNotExists_ReturnsFalse()
    {
        var state = new ViState();

        var result = state.ClearNamedRegister("x");

        Assert.False(result);
    }

    [Fact]
    public void ViState_GetNamedRegister_WhenNotExists_ReturnsNull()
    {
        var state = new ViState();

        var result = state.GetNamedRegister("nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public void ViState_SetNamedRegister_AcceptsAnyStringKey()
    {
        var state = new ViState();

        // Various key formats should all work
        state.SetNamedRegister("a", new ClipboardData("single char"));
        state.SetNamedRegister("longkey", new ClipboardData("long key"));
        state.SetNamedRegister("", new ClipboardData("empty key"));
        state.SetNamedRegister("123", new ClipboardData("numeric key"));

        Assert.NotNull(state.GetNamedRegister("a"));
        Assert.NotNull(state.GetNamedRegister("longkey"));
        Assert.NotNull(state.GetNamedRegister(""));
        Assert.NotNull(state.GetNamedRegister("123"));
    }

    [Fact]
    public void ViState_SetNamedRegister_NullData_Allowed()
    {
        var state = new ViState();

        // Python allows None for register values
        state.SetNamedRegister("a", null!);

        Assert.Null(state.GetNamedRegister("a"));
    }

    [Fact]
    public void ViState_GetNamedRegisterNames_ReturnsAllSetRegisters()
    {
        var state = new ViState();
        state.SetNamedRegister("a", new ClipboardData("a"));
        state.SetNamedRegister("b", new ClipboardData("b"));
        state.SetNamedRegister("c", new ClipboardData("c"));

        var names = state.GetNamedRegisterNames();

        Assert.Equal(3, names.Count);
        Assert.Contains("a", names);
        Assert.Contains("b", names);
        Assert.Contains("c", names);
    }

    [Fact]
    public void ViState_GetNamedRegisterNames_ReturnsCopy()
    {
        var state = new ViState();
        state.SetNamedRegister("a", new ClipboardData("a"));

        var names1 = state.GetNamedRegisterNames();
        var names2 = state.GetNamedRegisterNames();

        // Returns a copy each time, not the same instance
        Assert.NotSame(names1, names2);
    }

    [Fact]
    public void ViState_SetNamedRegister_Overwrites_ExistingValue()
    {
        var state = new ViState();
        var data1 = new ClipboardData("first");
        var data2 = new ClipboardData("second");

        state.SetNamedRegister("a", data1);
        state.SetNamedRegister("a", data2);

        Assert.Same(data2, state.GetNamedRegister("a"));
    }

    #endregion

    #region Macro Recording Tests (US6)

    [Fact]
    public void ViState_RecordingRegister_DefaultNull()
    {
        var state = new ViState();

        Assert.Null(state.RecordingRegister);
    }

    [Fact]
    public void ViState_CurrentRecording_DefaultEmptyString()
    {
        var state = new ViState();

        Assert.Equal("", state.CurrentRecording);
    }

    [Fact]
    public void ViState_CurrentRecording_CanAccumulateData()
    {
        var state = new ViState();

        state.CurrentRecording = "abc";
        state.CurrentRecording += "def";

        Assert.Equal("abcdef", state.CurrentRecording);
    }

    #endregion

    #region Property Get/Set Tests

    [Fact]
    public void ViState_InputMode_CanBeSetAndRetrieved()
    {
        var state = new ViState();

        state.InputMode = InputMode.Replace;

        Assert.Equal(InputMode.Replace, state.InputMode);
    }

    [Fact]
    public void ViState_LastCharacterFind_CanBeSetAndRetrieved()
    {
        var state = new ViState();
        var find = new CharacterFind("x", true);

        state.LastCharacterFind = find;

        Assert.Equal(find, state.LastCharacterFind);
    }

    [Fact]
    public void ViState_OperatorArg_CanBeSetAndRetrieved()
    {
        var state = new ViState();

        state.OperatorArg = 42;

        Assert.Equal(42, state.OperatorArg);
    }

    [Fact]
    public void ViState_WaitingForDigraph_CanBeSetAndRetrieved()
    {
        var state = new ViState();

        state.WaitingForDigraph = true;

        Assert.True(state.WaitingForDigraph);
    }

    [Fact]
    public void ViState_DigraphSymbol1_CanBeSetAndRetrieved()
    {
        var state = new ViState();

        state.DigraphSymbol1 = "a";

        Assert.Equal("a", state.DigraphSymbol1);
    }

    [Fact]
    public void ViState_TildeOperator_CanBeSetAndRetrieved()
    {
        var state = new ViState();

        state.TildeOperator = true;

        Assert.True(state.TildeOperator);
    }

    [Fact]
    public void ViState_RecordingRegister_CanBeSetAndRetrieved()
    {
        var state = new ViState();

        state.RecordingRegister = "q";

        Assert.Equal("q", state.RecordingRegister);
    }

    [Fact]
    public void ViState_CurrentRecording_CanBeSetAndRetrieved()
    {
        var state = new ViState();

        state.CurrentRecording = "test recording";

        Assert.Equal("test recording", state.CurrentRecording);
    }

    [Fact]
    public void ViState_TemporaryNavigationMode_CanBeSetAndRetrieved()
    {
        var state = new ViState();

        state.TemporaryNavigationMode = true;

        Assert.True(state.TemporaryNavigationMode);
    }

    #endregion

    #region Thread Safety Tests (SC-004)

    [Fact]
    public void ViState_ConcurrentInputModeChanges_NoCorruption()
    {
        var state = new ViState();
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
                        // Cycle through all input modes
                        state.InputMode = InputMode.Insert;
                        _ = state.InputMode;
                        state.InputMode = InputMode.Navigation;
                        _ = state.InputMode;
                        state.InputMode = InputMode.Replace;
                        _ = state.InputMode;
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
        // Final state should be a valid InputMode value
        Assert.True(Enum.IsDefined(typeof(InputMode), state.InputMode));
    }

    [Fact]
    public void ViState_ConcurrentPropertyAccess_NoDeadlocks()
    {
        var state = new ViState();
        var completedCount = 0;
        const int threadCount = 10;
        const int operationsPerThread = 1000;
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        var threads = new Thread[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < operationsPerThread && !cts.Token.IsCancellationRequested; j++)
                {
                    // Mix of reads and writes across different properties
                    state.InputMode = InputMode.Insert;
                    _ = state.LastCharacterFind;
                    state.OperatorArg = threadId * 1000 + j;
                    _ = state.WaitingForDigraph;
                    state.CurrentRecording = $"thread{threadId}";
                    _ = state.GetNamedRegisterNames();
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

    [Fact]
    public void ViState_ConcurrentNamedRegisterAccess_NoCorruption()
    {
        var state = new ViState();
        var exceptions = new List<Exception>();
        const int threadCount = 10;
        const int operationsPerThread = 1000;

        var threads = new Thread[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    for (int j = 0; j < operationsPerThread; j++)
                    {
                        var key = $"reg{threadId % 5}";
                        state.SetNamedRegister(key, new ClipboardData($"value{j}"));
                        _ = state.GetNamedRegister(key);
                        _ = state.GetNamedRegisterNames();
                        state.ClearNamedRegister(key);
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

    #endregion

    #region Edge Case Tests

    [Fact]
    public void ViState_Reset_DuringRecording_StopsRecording()
    {
        var state = new ViState();
        state.RecordingRegister = "a";
        state.CurrentRecording = "some data";

        state.Reset();

        Assert.Null(state.RecordingRegister);
        Assert.Equal("", state.CurrentRecording);
    }

    [Fact]
    public void ViState_MultipleResets_AreIdempotent()
    {
        var state = new ViState();

        state.Reset();
        state.Reset();
        state.Reset();

        // Should still be in valid initial state
        Assert.Equal(InputMode.Insert, state.InputMode);
        Assert.Equal("", state.CurrentRecording);
    }

    #endregion
}
