using Stroke.Input;
using Xunit;

namespace Stroke.Tests.Input;

/// <summary>
/// Tests for the <see cref="Keys"/> enum.
/// </summary>
public class KeysTests
{
    /// <summary>
    /// T006: Verifies that the Keys enum has exactly 151 members.
    /// </summary>
    /// <remarks>
    /// The spec originally said 143, but the actual Python Prompt Toolkit keys.py has 151 primary keys.
    /// Count: 2 (escape) + 31 (ctrl chars) + 10 (ctrl+num) + 10 (ctrl+shift+num) + 10 (nav) + 10 (ctrl+nav)
    ///        + 10 (shift+nav) + 10 (ctrl+shift+nav) + 1 (backtab) + 24 (F1-F24) + 24 (ctrl+F1-F24) + 9 (special) = 151
    /// </remarks>
    [Fact]
    public void EnumHas151Values()
    {
        var values = Enum.GetValues<Keys>();
        Assert.Equal(151, values.Length);
    }

    /// <summary>
    /// T007: Verifies that all Keys enum values have unique backing integers.
    /// </summary>
    [Fact]
    public void AllValuesHaveUniqueBackingIntegers()
    {
        var values = Enum.GetValues<Keys>();
        var backingValues = values.Select(k => (int)k).ToList();
        var distinctCount = backingValues.Distinct().Count();

        Assert.Equal(values.Length, distinctCount);
    }

    /// <summary>
    /// Verifies that the Keys enum contains all expected key categories.
    /// </summary>
    [Fact]
    public void EnumContainsAllKeyCategories()
    {
        // Escape keys (2)
        Assert.True(Enum.IsDefined(Keys.Escape));
        Assert.True(Enum.IsDefined(Keys.ShiftEscape));

        // Control characters - spot check first and last
        Assert.True(Enum.IsDefined(Keys.ControlAt));
        Assert.True(Enum.IsDefined(Keys.ControlZ));
        Assert.True(Enum.IsDefined(Keys.ControlBackslash));
        Assert.True(Enum.IsDefined(Keys.ControlUnderscore));

        // Control + Numbers
        Assert.True(Enum.IsDefined(Keys.Control0));
        Assert.True(Enum.IsDefined(Keys.Control9));

        // Control + Shift + Numbers
        Assert.True(Enum.IsDefined(Keys.ControlShift0));
        Assert.True(Enum.IsDefined(Keys.ControlShift9));

        // Navigation keys
        Assert.True(Enum.IsDefined(Keys.Left));
        Assert.True(Enum.IsDefined(Keys.Right));
        Assert.True(Enum.IsDefined(Keys.Up));
        Assert.True(Enum.IsDefined(Keys.Down));
        Assert.True(Enum.IsDefined(Keys.Home));
        Assert.True(Enum.IsDefined(Keys.End));
        Assert.True(Enum.IsDefined(Keys.PageUp));
        Assert.True(Enum.IsDefined(Keys.PageDown));

        // Control + Navigation
        Assert.True(Enum.IsDefined(Keys.ControlLeft));
        Assert.True(Enum.IsDefined(Keys.ControlPageDown));

        // Shift + Navigation
        Assert.True(Enum.IsDefined(Keys.ShiftLeft));
        Assert.True(Enum.IsDefined(Keys.ShiftPageDown));

        // Control + Shift + Navigation
        Assert.True(Enum.IsDefined(Keys.ControlShiftLeft));
        Assert.True(Enum.IsDefined(Keys.ControlShiftPageDown));

        // BackTab
        Assert.True(Enum.IsDefined(Keys.BackTab));

        // Function keys
        Assert.True(Enum.IsDefined(Keys.F1));
        Assert.True(Enum.IsDefined(Keys.F24));

        // Control + Function keys
        Assert.True(Enum.IsDefined(Keys.ControlF1));
        Assert.True(Enum.IsDefined(Keys.ControlF24));

        // Special keys
        Assert.True(Enum.IsDefined(Keys.Any));
        Assert.True(Enum.IsDefined(Keys.ScrollUp));
        Assert.True(Enum.IsDefined(Keys.ScrollDown));
        Assert.True(Enum.IsDefined(Keys.CPRResponse));
        Assert.True(Enum.IsDefined(Keys.Vt100MouseEvent));
        Assert.True(Enum.IsDefined(Keys.WindowsMouseEvent));
        Assert.True(Enum.IsDefined(Keys.BracketedPaste));
        Assert.True(Enum.IsDefined(Keys.SIGINT));
        Assert.True(Enum.IsDefined(Keys.Ignore));
    }

    /// <summary>
    /// Verifies that the enum values follow expected ordering (Escape first, special keys last).
    /// </summary>
    [Fact]
    public void EnumValuesAreOrderedCorrectly()
    {
        // First value should be Escape
        Assert.Equal(0, (int)Keys.Escape);

        // Last value should be Ignore (index 150 for 151 values)
        Assert.Equal(150, (int)Keys.Ignore);
    }
}
