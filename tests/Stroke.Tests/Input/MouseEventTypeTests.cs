using Stroke.Input;
using Xunit;

namespace Stroke.Tests.Input;

/// <summary>
/// Tests for <see cref="MouseEventType"/> enum.
/// </summary>
public class MouseEventTypeTests
{
    [Fact]
    public void MouseEventType_HasFiveValues()
    {
        var values = Enum.GetValues<MouseEventType>();
        Assert.Equal(5, values.Length);
    }

    [Fact]
    public void MouseEventType_HasMouseUp()
    {
        Assert.True(Enum.IsDefined(MouseEventType.MouseUp));
    }

    [Fact]
    public void MouseEventType_HasMouseDown()
    {
        Assert.True(Enum.IsDefined(MouseEventType.MouseDown));
    }

    [Fact]
    public void MouseEventType_HasScrollUp()
    {
        Assert.True(Enum.IsDefined(MouseEventType.ScrollUp));
    }

    [Fact]
    public void MouseEventType_HasScrollDown()
    {
        Assert.True(Enum.IsDefined(MouseEventType.ScrollDown));
    }

    [Fact]
    public void MouseEventType_HasMouseMove()
    {
        Assert.True(Enum.IsDefined(MouseEventType.MouseMove));
    }

    [Theory]
    [InlineData(MouseEventType.MouseUp, "MouseUp")]
    [InlineData(MouseEventType.MouseDown, "MouseDown")]
    [InlineData(MouseEventType.ScrollUp, "ScrollUp")]
    [InlineData(MouseEventType.ScrollDown, "ScrollDown")]
    [InlineData(MouseEventType.MouseMove, "MouseMove")]
    public void MouseEventType_ToString_ReturnsCorrectName(MouseEventType eventType, string expectedName)
    {
        Assert.Equal(expectedName, eventType.ToString());
    }

    [Fact]
    public void MouseEventType_ValuesMatchPythonPromptToolkit()
    {
        // Verify all 5 values from Python mouse_events.py are present
        // Per SC-001: "All 5 MouseEventType values from Python mouse_events.py are represented"
        var expectedValues = new[]
        {
            MouseEventType.MouseUp,
            MouseEventType.MouseDown,
            MouseEventType.ScrollUp,
            MouseEventType.ScrollDown,
            MouseEventType.MouseMove
        };

        var actualValues = Enum.GetValues<MouseEventType>();
        Assert.Equal(expectedValues.OrderBy(v => (int)v), actualValues.OrderBy(v => (int)v));
    }
}
