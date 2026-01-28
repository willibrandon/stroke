using Stroke.Input;
using Stroke.KeyBinding;
using Xunit;

namespace Stroke.Tests.KeyBinding;

/// <summary>
/// Tests for the <see cref="KeyBindingUtils"/> class.
/// </summary>
public sealed class KeyBindingUtilsTests
{
    #region Test Helpers

    private static NotImplementedOrNone? TestHandler(KeyPressEvent e) => null;

    #endregion

    #region Merge

    [Fact]
    public void Merge_WithEnumerable_CreatesMergedKeyBindings()
    {
        var kb1 = new KeyBindings();
        var kb2 = new KeyBindings();

        var merged = KeyBindingUtils.Merge(new[] { kb1, kb2 });

        Assert.IsType<MergedKeyBindings>(merged);
    }

    [Fact]
    public void Merge_WithParams_CreatesMergedKeyBindings()
    {
        var kb1 = new KeyBindings();
        var kb2 = new KeyBindings();

        var merged = KeyBindingUtils.Merge(kb1, kb2);

        Assert.IsType<MergedKeyBindings>(merged);
    }

    [Fact]
    public void Merge_WithNullEnumerable_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            KeyBindingUtils.Merge((IEnumerable<IKeyBindingsBase>)null!));
    }

    [Fact]
    public void Merge_WithNullParams_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            KeyBindingUtils.Merge((IKeyBindingsBase[])null!));
    }

    #endregion

    #region ParseKey - Special Names

    [Theory]
    [InlineData("space", Keys.ControlAt)]
    [InlineData("SPACE", Keys.ControlAt)]
    [InlineData("Space", Keys.ControlAt)]
    public void ParseKey_Space_ReturnsControlAt(string input, Keys expected)
    {
        var result = KeyBindingUtils.ParseKey(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("tab", Keys.ControlI)]
    [InlineData("TAB", Keys.ControlI)]
    public void ParseKey_Tab_ReturnsControlI(string input, Keys expected)
    {
        var result = KeyBindingUtils.ParseKey(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("enter", Keys.ControlM)]
    [InlineData("return", Keys.ControlM)]
    [InlineData("ENTER", Keys.ControlM)]
    public void ParseKey_Enter_ReturnsControlM(string input, Keys expected)
    {
        var result = KeyBindingUtils.ParseKey(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("escape", Keys.Escape)]
    [InlineData("esc", Keys.Escape)]
    [InlineData("ESC", Keys.Escape)]
    public void ParseKey_Escape_ReturnsEscapeKey(string input, Keys expected)
    {
        var result = KeyBindingUtils.ParseKey(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("backspace", Keys.ControlH)]
    [InlineData("bs", Keys.ControlH)]
    public void ParseKey_Backspace_ReturnsControlH(string input, Keys expected)
    {
        var result = KeyBindingUtils.ParseKey(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("delete", Keys.Delete)]
    [InlineData("del", Keys.Delete)]
    public void ParseKey_Delete_ReturnsDeleteKey(string input, Keys expected)
    {
        var result = KeyBindingUtils.ParseKey(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("up", Keys.Up)]
    [InlineData("down", Keys.Down)]
    [InlineData("left", Keys.Left)]
    [InlineData("right", Keys.Right)]
    public void ParseKey_ArrowKeys_ReturnsCorrectKey(string input, Keys expected)
    {
        var result = KeyBindingUtils.ParseKey(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("home", Keys.Home)]
    [InlineData("end", Keys.End)]
    public void ParseKey_HomeEnd_ReturnsCorrectKey(string input, Keys expected)
    {
        var result = KeyBindingUtils.ParseKey(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("pageup", Keys.PageUp)]
    [InlineData("pgup", Keys.PageUp)]
    [InlineData("pagedown", Keys.PageDown)]
    [InlineData("pgdown", Keys.PageDown)]
    public void ParseKey_PageKeys_ReturnsCorrectKey(string input, Keys expected)
    {
        var result = KeyBindingUtils.ParseKey(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("insert", Keys.Insert)]
    [InlineData("ins", Keys.Insert)]
    public void ParseKey_Insert_ReturnsInsertKey(string input, Keys expected)
    {
        var result = KeyBindingUtils.ParseKey(input);
        Assert.Equal(expected, result);
    }

    #endregion

    #region ParseKey - Control Keys (c-x pattern)

    [Theory]
    [InlineData("c-a", Keys.ControlA)]
    [InlineData("c-b", Keys.ControlB)]
    [InlineData("c-c", Keys.ControlC)]
    [InlineData("c-x", Keys.ControlX)]
    [InlineData("c-z", Keys.ControlZ)]
    public void ParseKey_ControlKey_ReturnsCorrectKey(string input, Keys expected)
    {
        var result = KeyBindingUtils.ParseKey(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseKey_ControlKeyUpperCase_ReturnsCorrectKey()
    {
        var result = KeyBindingUtils.ParseKey("C-A");
        Assert.Equal(Keys.ControlA, result);
    }

    [Fact]
    public void ParseKey_InvalidControlKey_ThrowsArgumentException()
    {
        // Note: c-1 through c-9 and c-0 are valid (Control1-Control0)
        // Use an invalid letter that doesn't have a corresponding Control key
        Assert.Throws<ArgumentException>(() => KeyBindingUtils.ParseKey("c-$"));
    }

    #endregion

    #region ParseKey - Function Keys

    [Theory]
    [InlineData("f1", Keys.F1)]
    [InlineData("f2", Keys.F2)]
    [InlineData("f12", Keys.F12)]
    [InlineData("F1", Keys.F1)]
    public void ParseKey_FunctionKey_ReturnsCorrectKey(string input, Keys expected)
    {
        var result = KeyBindingUtils.ParseKey(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseKey_InvalidFunctionKey_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => KeyBindingUtils.ParseKey("f99"));
    }

    #endregion

    #region ParseKey - Direct Enum Names

    [Fact]
    public void ParseKey_DirectEnumName_ReturnsCorrectKey()
    {
        var result = KeyBindingUtils.ParseKey("ControlC");
        Assert.Equal(Keys.ControlC, result);
    }

    [Fact]
    public void ParseKey_DirectEnumNameCaseInsensitive_ReturnsCorrectKey()
    {
        var result = KeyBindingUtils.ParseKey("controlc");
        Assert.Equal(Keys.ControlC, result);
    }

    #endregion

    #region ParseKey - Invalid Input

    [Fact]
    public void ParseKey_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => KeyBindingUtils.ParseKey(null!));
    }

    [Fact]
    public void ParseKey_InvalidInput_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => KeyBindingUtils.ParseKey("invalid-key-name"));
    }

    [Fact]
    public void ParseKey_EmptyString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => KeyBindingUtils.ParseKey(""));
    }

    #endregion
}
