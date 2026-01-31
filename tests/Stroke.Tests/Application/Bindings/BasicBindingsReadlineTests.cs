using Stroke.Application.Bindings;
using Stroke.Filters;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;
using Xunit;
using Keys = Stroke.Input.Keys;

namespace Stroke.Tests.Application.Bindings;

/// <summary>
/// Tests for BasicBindings self-insert, readline movement/editing, tab completion,
/// and history navigation bindings (US1, US2), plus polish integration tests (T029, T030).
/// </summary>
public sealed class BasicBindingsReadlineTests
{
    private readonly KeyBindings _kb;

    public BasicBindingsReadlineTests()
    {
        _kb = BasicBindings.LoadBasicBindings();
    }

    /// <summary>
    /// Gets bindings for a specific key, returning only those that match the given key.
    /// </summary>
    private IReadOnlyList<Binding> GetBindings(Keys key)
    {
        return _kb.GetBindingsForKeys([new KeyOrChar(key)]);
    }

    /// <summary>
    /// Finds a binding that uses the handler from a specific named command.
    /// </summary>
    private Binding? FindNamedCommandBinding(Keys key, string commandName)
    {
        var expectedBinding = NamedCommands.GetByName(commandName);
        var bindings = GetBindings(key);
        return bindings.FirstOrDefault(b => b.Handler == expectedBinding.Handler);
    }

    #region US1: Self-Insert Binding (T003)

    [Fact]
    public void SelfInsert_BoundToKeysAny()
    {
        var binding = FindNamedCommandBinding(Keys.Any, "self-insert");
        Assert.NotNull(binding);
    }

    [Fact]
    public void SelfInsert_HasInsertModeFilter()
    {
        var binding = FindNamedCommandBinding(Keys.Any, "self-insert");
        Assert.NotNull(binding);
        // The filter should not be Always (meaning it has a real filter)
        Assert.IsNotType<Always>(binding.Filter);
    }

    [Fact]
    public void SelfInsert_HasSaveBeforeCallback()
    {
        var binding = FindNamedCommandBinding(Keys.Any, "self-insert");
        Assert.NotNull(binding);
        // SaveBefore should not be the default (always true)
        // Create a fake non-repeated event to test
        var nonRepeatedEvent = CreateEvent(isRepeat: false);
        var repeatedEvent = CreateEvent(isRepeat: true);
        Assert.True(binding.SaveBefore(nonRepeatedEvent));
        Assert.False(binding.SaveBefore(repeatedEvent));
    }

    [Fact]
    public void SelfInsert_IfNoRepeat_ReturnsFalseForRepeatedEvents()
    {
        var binding = FindNamedCommandBinding(Keys.Any, "self-insert");
        Assert.NotNull(binding);
        var repeatedEvent = CreateEvent(isRepeat: true);
        Assert.False(binding.SaveBefore(repeatedEvent));
    }

    [Fact]
    public void SelfInsert_IfNoRepeat_ReturnsTrueForNonRepeatedEvents()
    {
        var binding = FindNamedCommandBinding(Keys.Any, "self-insert");
        Assert.NotNull(binding);
        var nonRepeatedEvent = CreateEvent(isRepeat: false);
        Assert.True(binding.SaveBefore(nonRepeatedEvent));
    }

    #endregion

    #region US2: Readline Movement Bindings (T008)

    [Theory]
    [InlineData(Keys.Home, "beginning-of-line")]
    [InlineData(Keys.End, "end-of-line")]
    [InlineData(Keys.Left, "backward-char")]
    [InlineData(Keys.Right, "forward-char")]
    [InlineData(Keys.ControlUp, "previous-history")]
    [InlineData(Keys.ControlDown, "next-history")]
    [InlineData(Keys.ControlL, "clear-screen")]
    public void ReadlineMovement_BoundToCorrectNamedCommand(Keys key, string commandName)
    {
        var binding = FindNamedCommandBinding(key, commandName);
        Assert.NotNull(binding);
    }

    [Theory]
    [InlineData(Keys.Home)]
    [InlineData(Keys.End)]
    [InlineData(Keys.Left)]
    [InlineData(Keys.Right)]
    [InlineData(Keys.ControlUp)]
    [InlineData(Keys.ControlDown)]
    [InlineData(Keys.ControlL)]
    public void ReadlineMovement_HasNoFilter(Keys key)
    {
        var bindings = GetBindings(key);
        // Find the named command binding (not the ignored one)
        var namedBinding = bindings.FirstOrDefault(b =>
            b.Handler != GetIgnoreHandler());
        Assert.NotNull(namedBinding);
        // Filter should be Always (no filter restriction)
        Assert.IsType<Always>(namedBinding.Filter);
    }

    #endregion

    #region US2: Readline Editing Bindings (T009)

    [Theory]
    [InlineData(Keys.ControlK, "kill-line")]
    [InlineData(Keys.ControlU, "unix-line-discard")]
    [InlineData(Keys.ControlH, "backward-delete-char")]
    [InlineData(Keys.Delete, "delete-char")]
    [InlineData(Keys.ControlDelete, "delete-char")]
    [InlineData(Keys.ControlT, "transpose-chars")]
    [InlineData(Keys.ControlW, "unix-word-rubout")]
    public void ReadlineEditing_BoundToCorrectNamedCommand(Keys key, string commandName)
    {
        var binding = FindNamedCommandBinding(key, commandName);
        Assert.NotNull(binding);
    }

    [Theory]
    [InlineData(Keys.ControlK)]
    [InlineData(Keys.ControlU)]
    [InlineData(Keys.ControlH)]
    [InlineData(Keys.Delete)]
    [InlineData(Keys.ControlDelete)]
    [InlineData(Keys.ControlT)]
    [InlineData(Keys.ControlW)]
    public void ReadlineEditing_HasInsertModeFilter(Keys key)
    {
        var bindings = GetBindings(key);
        // Find the named command binding (not the ignored one)
        var namedBinding = bindings.FirstOrDefault(b =>
            b.Handler != GetIgnoreHandler());
        Assert.NotNull(namedBinding);
        // Filter should not be Always (has a real filter)
        Assert.IsNotType<Always>(namedBinding.Filter);
    }

    [Theory]
    [InlineData(Keys.ControlH)]
    [InlineData(Keys.Delete)]
    [InlineData(Keys.ControlDelete)]
    public void ReadlineEditing_DeletionKeys_HaveIfNoRepeatSaveBefore(Keys key)
    {
        var bindings = GetBindings(key);
        var namedBinding = bindings.FirstOrDefault(b =>
            b.Handler != GetIgnoreHandler() &&
            b.Filter is not Always);
        Assert.NotNull(namedBinding);

        var nonRepeatedEvent = CreateEvent(isRepeat: false);
        var repeatedEvent = CreateEvent(isRepeat: true);
        Assert.True(namedBinding.SaveBefore(nonRepeatedEvent));
        Assert.False(namedBinding.SaveBefore(repeatedEvent));
    }

    [Theory]
    [InlineData(Keys.ControlK)]
    [InlineData(Keys.ControlU)]
    [InlineData(Keys.ControlT)]
    [InlineData(Keys.ControlW)]
    public void ReadlineEditing_NonDeletionKeys_HaveDefaultSaveBefore(Keys key)
    {
        var bindings = GetBindings(key);
        var namedBinding = bindings.FirstOrDefault(b =>
            b.Handler != GetIgnoreHandler() &&
            b.Filter is not Always);
        Assert.NotNull(namedBinding);

        // Default saveBefore always returns true
        var repeatedEvent = CreateEvent(isRepeat: true);
        Assert.True(namedBinding.SaveBefore(repeatedEvent));
    }

    #endregion

    #region US2: Tab Completion Bindings (T010)

    [Fact]
    public void TabCompletion_CtrlI_BoundToMenuComplete()
    {
        var binding = FindNamedCommandBinding(Keys.ControlI, "menu-complete");
        Assert.NotNull(binding);
    }

    [Fact]
    public void TabCompletion_ShiftTab_BoundToMenuCompleteBackward()
    {
        var binding = FindNamedCommandBinding(Keys.BackTab, "menu-complete-backward");
        Assert.NotNull(binding);
    }

    [Fact]
    public void TabCompletion_HasInsertModeFilter()
    {
        var ctrlIBinding = FindNamedCommandBinding(Keys.ControlI, "menu-complete");
        var shiftTabBinding = FindNamedCommandBinding(Keys.BackTab, "menu-complete-backward");
        Assert.NotNull(ctrlIBinding);
        Assert.NotNull(shiftTabBinding);
        // Both should have non-Always filters (InsertMode)
        Assert.IsNotType<Always>(ctrlIBinding.Filter);
        Assert.IsNotType<Always>(shiftTabBinding.Filter);
    }

    #endregion

    #region US2: History Navigation Bindings (T010)

    [Fact]
    public void HistoryNav_PageUp_BoundToPreviousHistory()
    {
        var binding = FindNamedCommandBinding(Keys.PageUp, "previous-history");
        Assert.NotNull(binding);
    }

    [Fact]
    public void HistoryNav_PageDown_BoundToNextHistory()
    {
        var binding = FindNamedCommandBinding(Keys.PageDown, "next-history");
        Assert.NotNull(binding);
    }

    [Fact]
    public void HistoryNav_HasNotHasSelectionFilter()
    {
        var pageUpBinding = FindNamedCommandBinding(Keys.PageUp, "previous-history");
        var pageDownBinding = FindNamedCommandBinding(Keys.PageDown, "next-history");
        Assert.NotNull(pageUpBinding);
        Assert.NotNull(pageDownBinding);
        // Filters should not be Always (has ~HasSelection filter)
        Assert.IsNotType<Always>(pageUpBinding.Filter);
        Assert.IsNotType<Always>(pageDownBinding.Filter);
    }

    #endregion

    #region US7: Ctrl+D Binding (T028)

    [Fact]
    public void CtrlD_BoundToDeleteChar()
    {
        var binding = FindNamedCommandBinding(Keys.ControlD, "delete-char");
        Assert.NotNull(binding);
    }

    [Fact]
    public void CtrlD_HasCompoundFilter()
    {
        var binding = FindNamedCommandBinding(Keys.ControlD, "delete-char");
        Assert.NotNull(binding);
        // Filter should not be Always (has HasTextBeforeCursor & InsertMode)
        Assert.IsNotType<Always>(binding.Filter);
    }

    #endregion

    #region T029: Registration Order

    [Fact]
    public void RegistrationOrder_IgnoredKeysFirst()
    {
        var allBindings = _kb.Bindings;
        // First 90 bindings should be ignored key handlers (same handler reference)
        var firstIgnored = allBindings[0];
        for (int i = 0; i < 90; i++)
        {
            Assert.Equal(firstIgnored.Handler, allBindings[i].Handler);
        }
    }

    [Fact]
    public void RegistrationOrder_ReadlineMovementAfterIgnored()
    {
        var allBindings = _kb.Bindings;
        // Binding at index 90 should be Home → beginning-of-line
        var homeBinding = allBindings[90];
        Assert.Equal(new KeyOrChar(Keys.Home), homeBinding.Keys[0]);
    }

    [Fact]
    public void RegistrationOrder_QuotedInsertLast()
    {
        var allBindings = _kb.Bindings;
        // Last binding (index 117) should be quoted insert on Keys.Any with eager
        var lastBinding = allBindings[117];
        Assert.Equal(new KeyOrChar(Keys.Any), lastBinding.Keys[0]);
        Assert.IsNotType<Never>(lastBinding.Eager);
    }

    [Fact]
    public void TotalBindingCount_Is118()
    {
        Assert.Equal(118, _kb.Bindings.Count);
    }

    #endregion

    #region T030: Integration

    [Fact]
    public void LoadBasicBindings_ReturnsNonNull()
    {
        Assert.NotNull(_kb);
    }

    [Fact]
    public void LoadBasicBindings_ReturnsExactly118Bindings()
    {
        Assert.Equal(118, _kb.Bindings.Count);
    }

    [Fact]
    public void LoadBasicBindings_All16NamedCommandsResolve()
    {
        var commandNames = new[]
        {
            "beginning-of-line", "end-of-line", "backward-char", "forward-char",
            "previous-history", "next-history", "clear-screen",
            "kill-line", "unix-line-discard", "backward-delete-char",
            "delete-char", "transpose-chars", "unix-word-rubout",
            "self-insert", "menu-complete", "menu-complete-backward"
        };

        foreach (var name in commandNames)
        {
            var binding = NamedCommands.GetByName(name);
            Assert.NotNull(binding);
        }
    }

    [Fact]
    public void LoadBasicBindings_IntegratesWithMergedKeyBindings()
    {
        var merged = new MergedKeyBindings(_kb);
        Assert.Equal(118, merged.Bindings.Count);
    }

    [Fact]
    public void LoadBasicBindings_EachCallReturnsNewInstance()
    {
        var kb2 = BasicBindings.LoadBasicBindings();
        Assert.NotSame(_kb, kb2);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Gets the handler for the first ignored key binding (shared Ignore handler).
    /// </summary>
    private KeyHandlerCallable GetIgnoreHandler()
    {
        // The first binding is always an ignored key binding
        return _kb.Bindings[0].Handler;
    }

    private static KeyPressEvent CreateEvent(bool isRepeat = false, Keys key = Keys.Any)
    {
        return new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: [new Stroke.KeyBinding.KeyPress(key)],
            previousKeySequence: [],
            isRepeat: isRepeat);
    }

    #endregion
}
