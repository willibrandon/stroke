using Stroke.Input;
using Xunit;

namespace Stroke.Tests.Input;

/// <summary>
/// Tests for <see cref="MouseButton"/> enum.
/// </summary>
public class MouseButtonTests
{
    [Fact]
    public void MouseButton_HasFiveValues()
    {
        var values = Enum.GetValues<MouseButton>();
        Assert.Equal(5, values.Length);
    }

    [Fact]
    public void MouseButton_HasLeft()
    {
        Assert.True(Enum.IsDefined(MouseButton.Left));
    }

    [Fact]
    public void MouseButton_HasMiddle()
    {
        Assert.True(Enum.IsDefined(MouseButton.Middle));
    }

    [Fact]
    public void MouseButton_HasRight()
    {
        Assert.True(Enum.IsDefined(MouseButton.Right));
    }

    [Fact]
    public void MouseButton_HasNone()
    {
        Assert.True(Enum.IsDefined(MouseButton.None));
    }

    [Fact]
    public void MouseButton_HasUnknown()
    {
        Assert.True(Enum.IsDefined(MouseButton.Unknown));
    }

    [Theory]
    [InlineData(MouseButton.Left, "Left")]
    [InlineData(MouseButton.Middle, "Middle")]
    [InlineData(MouseButton.Right, "Right")]
    [InlineData(MouseButton.None, "None")]
    [InlineData(MouseButton.Unknown, "Unknown")]
    public void MouseButton_ToString_ReturnsCorrectName(MouseButton button, string expectedName)
    {
        Assert.Equal(expectedName, button.ToString());
    }

    [Fact]
    public void MouseButton_ValuesMatchPythonPromptToolkit()
    {
        // Verify all 5 values from Python mouse_events.py are present
        // Per SC-001: "All 5 MouseButton values from Python mouse_events.py are represented"
        var expectedValues = new[]
        {
            MouseButton.Left,
            MouseButton.Middle,
            MouseButton.Right,
            MouseButton.None,
            MouseButton.Unknown
        };

        var actualValues = Enum.GetValues<MouseButton>();
        Assert.Equal(expectedValues.OrderBy(v => (int)v), actualValues.OrderBy(v => (int)v));
    }

    // T015: Edge case tests for MouseButton.None and MouseButton.Unknown
    [Fact]
    public void MouseButton_None_RepresentsNoButtonPressed()
    {
        // MouseButton.None is used when no button is pressed (scrolling or just moving)
        var button = MouseButton.None;
        Assert.Equal("None", button.ToString());
    }

    [Fact]
    public void MouseButton_Unknown_RepresentsUnidentifiedButton()
    {
        // MouseButton.Unknown is used when a button was pressed but we don't know which one
        var button = MouseButton.Unknown;
        Assert.Equal("Unknown", button.ToString());
    }

    [Fact]
    public void MouseButton_None_IsDistinctFromUnknown()
    {
        Assert.NotEqual(MouseButton.None, MouseButton.Unknown);
    }
}
