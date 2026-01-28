using Stroke.KeyBinding;
using Xunit;

namespace Stroke.Tests.KeyBinding;

/// <summary>
/// Tests for the <see cref="InputMode"/> enum.
/// </summary>
public class InputModeTests
{
    [Fact]
    public void InputMode_HasExactlyFiveValues()
    {
        // Per FR-003: InputMode must have exactly 5 values
        var values = Enum.GetValues<InputMode>();

        Assert.Equal(5, values.Length);
    }

    [Fact]
    public void InputMode_ContainsInsertValue()
    {
        Assert.True(Enum.IsDefined(typeof(InputMode), InputMode.Insert));
    }

    [Fact]
    public void InputMode_ContainsInsertMultipleValue()
    {
        Assert.True(Enum.IsDefined(typeof(InputMode), InputMode.InsertMultiple));
    }

    [Fact]
    public void InputMode_ContainsNavigationValue()
    {
        Assert.True(Enum.IsDefined(typeof(InputMode), InputMode.Navigation));
    }

    [Fact]
    public void InputMode_ContainsReplaceValue()
    {
        Assert.True(Enum.IsDefined(typeof(InputMode), InputMode.Replace));
    }

    [Fact]
    public void InputMode_ContainsReplaceSingleValue()
    {
        Assert.True(Enum.IsDefined(typeof(InputMode), InputMode.ReplaceSingle));
    }

    [Fact]
    public void InputMode_AllValuesAreDifferent()
    {
        var values = Enum.GetValues<InputMode>();
        var distinctCount = values.Cast<InputMode>().Distinct().Count();

        Assert.Equal(values.Length, distinctCount);
    }

    [Fact]
    public void InputMode_CanUseInSwitchExpression()
    {
        // Verify enum can be used in switch expressions
        static string GetModeDescription(InputMode mode) => mode switch
        {
            InputMode.Insert => "Normal text insertion",
            InputMode.InsertMultiple => "Insert mode for multiple cursors",
            InputMode.Navigation => "Vi normal mode",
            InputMode.Replace => "Overwrite mode",
            InputMode.ReplaceSingle => "Replace single character",
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };

        Assert.Equal("Normal text insertion", GetModeDescription(InputMode.Insert));
        Assert.Equal("Vi normal mode", GetModeDescription(InputMode.Navigation));
    }
}
