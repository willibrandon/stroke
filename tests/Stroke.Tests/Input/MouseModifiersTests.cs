using Stroke.Input;
using Xunit;

namespace Stroke.Tests.Input;

/// <summary>
/// Tests for <see cref="MouseModifiers"/> [Flags] enum.
/// </summary>
public class MouseModifiersTests
{
    [Fact]
    public void MouseModifiers_HasFourValues()
    {
        var values = Enum.GetValues<MouseModifiers>();
        Assert.Equal(4, values.Length);
    }

    [Fact]
    public void MouseModifiers_None_HasValueZero()
    {
        Assert.Equal(0, (int)MouseModifiers.None);
    }

    [Fact]
    public void MouseModifiers_Shift_HasValueOne()
    {
        Assert.Equal(1, (int)MouseModifiers.Shift);
    }

    [Fact]
    public void MouseModifiers_Alt_HasValueTwo()
    {
        Assert.Equal(2, (int)MouseModifiers.Alt);
    }

    [Fact]
    public void MouseModifiers_Control_HasValueFour()
    {
        Assert.Equal(4, (int)MouseModifiers.Control);
    }

    [Fact]
    public void MouseModifiers_HasFlagsAttribute()
    {
        var type = typeof(MouseModifiers);
        var hasFlagsAttribute = type.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0;
        Assert.True(hasFlagsAttribute);
    }

    [Fact]
    public void MouseModifiers_CanCombineShiftAndControl()
    {
        var combined = MouseModifiers.Shift | MouseModifiers.Control;
        Assert.Equal(5, (int)combined); // 1 + 4 = 5
        Assert.True((combined & MouseModifiers.Shift) != 0);
        Assert.True((combined & MouseModifiers.Control) != 0);
        Assert.False((combined & MouseModifiers.Alt) != 0);
    }

    [Fact]
    public void MouseModifiers_CanCombineShiftAndAlt()
    {
        var combined = MouseModifiers.Shift | MouseModifiers.Alt;
        Assert.Equal(3, (int)combined); // 1 + 2 = 3
        Assert.True((combined & MouseModifiers.Shift) != 0);
        Assert.True((combined & MouseModifiers.Alt) != 0);
        Assert.False((combined & MouseModifiers.Control) != 0);
    }

    [Fact]
    public void MouseModifiers_CanCombineControlAndAlt()
    {
        var combined = MouseModifiers.Control | MouseModifiers.Alt;
        Assert.Equal(6, (int)combined); // 4 + 2 = 6
        Assert.True((combined & MouseModifiers.Control) != 0);
        Assert.True((combined & MouseModifiers.Alt) != 0);
        Assert.False((combined & MouseModifiers.Shift) != 0);
    }

    [Fact]
    public void MouseModifiers_CanCombineAllThree()
    {
        var combined = MouseModifiers.Shift | MouseModifiers.Alt | MouseModifiers.Control;
        Assert.Equal(7, (int)combined); // 1 + 2 + 4 = 7
        Assert.True((combined & MouseModifiers.Shift) != 0);
        Assert.True((combined & MouseModifiers.Alt) != 0);
        Assert.True((combined & MouseModifiers.Control) != 0);
    }

    [Fact]
    public void MouseModifiers_BitwiseOrWithNone_ReturnsOriginal()
    {
        Assert.Equal(MouseModifiers.Shift, MouseModifiers.Shift | MouseModifiers.None);
        Assert.Equal(MouseModifiers.Alt, MouseModifiers.Alt | MouseModifiers.None);
        Assert.Equal(MouseModifiers.Control, MouseModifiers.Control | MouseModifiers.None);
    }

    [Theory]
    [InlineData(MouseModifiers.None, "None")]
    [InlineData(MouseModifiers.Shift, "Shift")]
    [InlineData(MouseModifiers.Alt, "Alt")]
    [InlineData(MouseModifiers.Control, "Control")]
    public void MouseModifiers_ToString_ReturnsCorrectName(MouseModifiers modifiers, string expectedName)
    {
        Assert.Equal(expectedName, modifiers.ToString());
    }

    [Fact]
    public void MouseModifiers_CombinedToString_ShowsAllFlags()
    {
        var combined = MouseModifiers.Shift | MouseModifiers.Control;
        var str = combined.ToString();
        Assert.Contains("Shift", str);
        Assert.Contains("Control", str);
    }

    // T034: Verify bitwise OR works for combinations
    [Theory]
    [InlineData(MouseModifiers.Shift, MouseModifiers.Control, 5)]
    [InlineData(MouseModifiers.Shift, MouseModifiers.Alt, 3)]
    [InlineData(MouseModifiers.Alt, MouseModifiers.Control, 6)]
    public void MouseModifiers_BitwiseOr_ProducesCorrectCombination(
        MouseModifiers first,
        MouseModifiers second,
        int expectedValue)
    {
        var combined = first | second;
        Assert.Equal(expectedValue, (int)combined);
        Assert.True(combined.HasFlag(first));
        Assert.True(combined.HasFlag(second));
    }

    [Fact]
    public void MouseModifiers_HasFlag_WorksCorrectly()
    {
        var combined = MouseModifiers.Shift | MouseModifiers.Alt | MouseModifiers.Control;

        Assert.True(combined.HasFlag(MouseModifiers.Shift));
        Assert.True(combined.HasFlag(MouseModifiers.Alt));
        Assert.True(combined.HasFlag(MouseModifiers.Control));
        Assert.True(combined.HasFlag(MouseModifiers.None)); // None is always present
    }

    [Fact]
    public void MouseModifiers_None_HasNoFlags()
    {
        var none = MouseModifiers.None;

        Assert.False((none & MouseModifiers.Shift) != 0);
        Assert.False((none & MouseModifiers.Alt) != 0);
        Assert.False((none & MouseModifiers.Control) != 0);
    }
}
