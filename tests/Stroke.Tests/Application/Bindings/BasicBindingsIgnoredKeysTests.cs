using Stroke.Application.Bindings;
using Stroke.Filters;
using Stroke.KeyBinding;
using Xunit;
using Keys = Stroke.Input.Keys;

namespace Stroke.Tests.Application.Bindings;

/// <summary>
/// Exhaustive tests verifying all 90 ignored key bindings in BasicBindings (US3).
/// Each ignored key must be registered with the shared no-op handler and have
/// an Always filter (no filter restriction).
/// </summary>
public sealed class BasicBindingsIgnoredKeysTests
{
    private readonly KeyBindings _kb;
    private readonly KeyHandlerCallable _ignoreHandler;

    public BasicBindingsIgnoredKeysTests()
    {
        _kb = BasicBindings.LoadBasicBindings();
        // The first binding is always an ignored key — extract its handler as reference
        _ignoreHandler = _kb.Bindings[0].Handler;
    }

    /// <summary>
    /// Verifies that the given key has at least one binding using the shared Ignore handler.
    /// </summary>
    private void AssertIgnored(Keys key)
    {
        var bindings = _kb.GetBindingsForKeys([new KeyOrChar(key)]);
        Assert.Contains(bindings, b => b.Handler == _ignoreHandler);
    }

    /// <summary>
    /// Verifies that the shared Ignore handler returns null (no-op).
    /// </summary>
    [Fact]
    public void IgnoreHandler_ReturnsNull()
    {
        var @event = new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: [new Stroke.KeyBinding.KeyPress(Keys.Any)],
            previousKeySequence: [],
            isRepeat: false);

        // Call the handler from the first ignored binding
        var result = _ignoreHandler(@event);
        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that all ignored key bindings share the same handler reference.
    /// </summary>
    [Fact]
    public void AllIgnoredBindings_ShareSameHandler()
    {
        var allBindings = _kb.Bindings;
        for (int i = 0; i < 90; i++)
        {
            Assert.Same(_ignoreHandler.Target, allBindings[i].Handler.Target);
            Assert.Equal(_ignoreHandler.Method, allBindings[i].Handler.Method);
        }
    }

    /// <summary>
    /// Verifies that all ignored key bindings have Always filter (no restriction).
    /// </summary>
    [Fact]
    public void AllIgnoredBindings_HaveAlwaysFilter()
    {
        var allBindings = _kb.Bindings;
        for (int i = 0; i < 90; i++)
        {
            Assert.IsType<Always>(allBindings[i].Filter);
        }
    }

    /// <summary>
    /// Verifies that exactly 90 bindings use the Ignore handler.
    /// </summary>
    [Fact]
    public void IgnoredBindingCount_Is90()
    {
        var allBindings = _kb.Bindings;
        int count = allBindings.Count(b =>
            b.Handler.Method == _ignoreHandler.Method &&
            ReferenceEquals(b.Handler.Target, _ignoreHandler.Target));
        Assert.Equal(90, count);
    }

    #region 26 Control Keys (Ctrl+A through Ctrl+Z)

    [Theory]
    [InlineData(Keys.ControlA)]
    [InlineData(Keys.ControlB)]
    [InlineData(Keys.ControlC)]
    [InlineData(Keys.ControlD)]
    [InlineData(Keys.ControlE)]
    [InlineData(Keys.ControlF)]
    [InlineData(Keys.ControlG)]
    [InlineData(Keys.ControlH)]
    [InlineData(Keys.ControlI)]
    [InlineData(Keys.ControlJ)]
    [InlineData(Keys.ControlK)]
    [InlineData(Keys.ControlL)]
    [InlineData(Keys.ControlM)]
    [InlineData(Keys.ControlN)]
    [InlineData(Keys.ControlO)]
    [InlineData(Keys.ControlP)]
    [InlineData(Keys.ControlQ)]
    [InlineData(Keys.ControlR)]
    [InlineData(Keys.ControlS)]
    [InlineData(Keys.ControlT)]
    [InlineData(Keys.ControlU)]
    [InlineData(Keys.ControlV)]
    [InlineData(Keys.ControlW)]
    [InlineData(Keys.ControlX)]
    [InlineData(Keys.ControlY)]
    [InlineData(Keys.ControlZ)]
    public void ControlKeys_AreIgnored(Keys key)
    {
        AssertIgnored(key);
    }

    #endregion

    #region 24 Function Keys (F1 through F24)

    [Theory]
    [InlineData(Keys.F1)]
    [InlineData(Keys.F2)]
    [InlineData(Keys.F3)]
    [InlineData(Keys.F4)]
    [InlineData(Keys.F5)]
    [InlineData(Keys.F6)]
    [InlineData(Keys.F7)]
    [InlineData(Keys.F8)]
    [InlineData(Keys.F9)]
    [InlineData(Keys.F10)]
    [InlineData(Keys.F11)]
    [InlineData(Keys.F12)]
    [InlineData(Keys.F13)]
    [InlineData(Keys.F14)]
    [InlineData(Keys.F15)]
    [InlineData(Keys.F16)]
    [InlineData(Keys.F17)]
    [InlineData(Keys.F18)]
    [InlineData(Keys.F19)]
    [InlineData(Keys.F20)]
    [InlineData(Keys.F21)]
    [InlineData(Keys.F22)]
    [InlineData(Keys.F23)]
    [InlineData(Keys.F24)]
    public void FunctionKeys_AreIgnored(Keys key)
    {
        AssertIgnored(key);
    }

    #endregion

    #region 5 Control-Punctuation Keys

    [Theory]
    [InlineData(Keys.ControlAt)]
    [InlineData(Keys.ControlBackslash)]
    [InlineData(Keys.ControlSquareClose)]
    [InlineData(Keys.ControlCircumflex)]
    [InlineData(Keys.ControlUnderscore)]
    public void ControlPunctuationKeys_AreIgnored(Keys key)
    {
        AssertIgnored(key);
    }

    #endregion

    #region 5 Base Navigation Keys

    [Theory]
    [InlineData(Keys.ControlH)]   // Backspace
    [InlineData(Keys.Up)]
    [InlineData(Keys.Down)]
    [InlineData(Keys.Right)]
    [InlineData(Keys.Left)]
    public void BaseNavigationKeys_AreIgnored(Keys key)
    {
        AssertIgnored(key);
    }

    #endregion

    #region 4 Shift-Arrow Keys

    [Theory]
    [InlineData(Keys.ShiftUp)]
    [InlineData(Keys.ShiftDown)]
    [InlineData(Keys.ShiftRight)]
    [InlineData(Keys.ShiftLeft)]
    public void ShiftArrowKeys_AreIgnored(Keys key)
    {
        AssertIgnored(key);
    }

    #endregion

    #region 4 Home/End Keys

    [Theory]
    [InlineData(Keys.Home)]
    [InlineData(Keys.End)]
    [InlineData(Keys.ShiftHome)]
    [InlineData(Keys.ShiftEnd)]
    public void HomeEndKeys_AreIgnored(Keys key)
    {
        AssertIgnored(key);
    }

    #endregion

    #region 3 Delete Variants

    [Theory]
    [InlineData(Keys.Delete)]
    [InlineData(Keys.ShiftDelete)]
    [InlineData(Keys.ControlDelete)]
    public void DeleteKeys_AreIgnored(Keys key)
    {
        AssertIgnored(key);
    }

    #endregion

    #region 2 Page Keys

    [Theory]
    [InlineData(Keys.PageUp)]
    [InlineData(Keys.PageDown)]
    public void PageKeys_AreIgnored(Keys key)
    {
        AssertIgnored(key);
    }

    #endregion

    #region 2 Tab Keys

    [Theory]
    [InlineData(Keys.BackTab)]    // Shift+Tab
    [InlineData(Keys.ControlI)]   // Tab
    public void TabKeys_AreIgnored(Keys key)
    {
        AssertIgnored(key);
    }

    #endregion

    #region 4 Ctrl+Shift Navigation Keys

    [Theory]
    [InlineData(Keys.ControlShiftLeft)]
    [InlineData(Keys.ControlShiftRight)]
    [InlineData(Keys.ControlShiftHome)]
    [InlineData(Keys.ControlShiftEnd)]
    public void CtrlShiftNavigationKeys_AreIgnored(Keys key)
    {
        AssertIgnored(key);
    }

    #endregion

    #region 6 Ctrl Navigation Keys

    [Theory]
    [InlineData(Keys.ControlLeft)]
    [InlineData(Keys.ControlRight)]
    [InlineData(Keys.ControlUp)]
    [InlineData(Keys.ControlDown)]
    [InlineData(Keys.ControlHome)]
    [InlineData(Keys.ControlEnd)]
    public void CtrlNavigationKeys_AreIgnored(Keys key)
    {
        AssertIgnored(key);
    }

    #endregion

    #region 3 Insert Variants

    [Theory]
    [InlineData(Keys.Insert)]
    [InlineData(Keys.ShiftInsert)]
    [InlineData(Keys.ControlInsert)]
    public void InsertKeys_AreIgnored(Keys key)
    {
        AssertIgnored(key);
    }

    #endregion

    #region SIGINT and Ignore

    [Fact]
    public void SIGINT_IsIgnored()
    {
        AssertIgnored(Keys.SIGINT);
    }

    [Fact]
    public void KeysIgnore_IsIgnored()
    {
        AssertIgnored(Keys.Ignore);
    }

    #endregion
}
