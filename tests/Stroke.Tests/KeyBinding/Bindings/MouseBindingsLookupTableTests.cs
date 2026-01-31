using Stroke.Input;
using Stroke.KeyBinding.Bindings;
using Xunit;

namespace Stroke.Tests.KeyBinding.Bindings;

/// <summary>
/// Tests for the MouseBindings lookup tables: XtermSgrMouseEvents, TypicalMouseEvents, UrxvtMouseEvents.
/// </summary>
public sealed class MouseBindingsLookupTableTests
{
    // --- XTerm SGR lookup table tests ---

    [Fact]
    public void XtermSgrMouseEvents_Has96Entries()
    {
        Assert.Equal(96, MouseBindings.XtermSgrMouseEvents.Count);
    }

    [Fact]
    public void XtermSgrMouseEvents_Code0M_IsLeftMouseDown()
    {
        var (button, eventType, modifiers) = MouseBindings.XtermSgrMouseEvents[(0, 'M')];
        Assert.Equal(MouseButton.Left, button);
        Assert.Equal(MouseEventType.MouseDown, eventType);
        Assert.Equal(MouseModifiers.None, modifiers);
    }

    [Fact]
    public void XtermSgrMouseEvents_Code2m_IsRightMouseUp()
    {
        var (button, eventType, modifiers) = MouseBindings.XtermSgrMouseEvents[(2, 'm')];
        Assert.Equal(MouseButton.Right, button);
        Assert.Equal(MouseEventType.MouseUp, eventType);
        Assert.Equal(MouseModifiers.None, modifiers);
    }

    [Fact]
    public void XtermSgrMouseEvents_Code36M_IsLeftMouseMoveShift()
    {
        var (button, eventType, modifiers) = MouseBindings.XtermSgrMouseEvents[(36, 'M')];
        Assert.Equal(MouseButton.Left, button);
        Assert.Equal(MouseEventType.MouseMove, eventType);
        Assert.Equal(MouseModifiers.Shift, modifiers);
    }

    [Fact]
    public void XtermSgrMouseEvents_Code64M_IsScrollUpNoModifier()
    {
        var (button, eventType, modifiers) = MouseBindings.XtermSgrMouseEvents[(64, 'M')];
        Assert.Equal(MouseButton.None, button);
        Assert.Equal(MouseEventType.ScrollUp, eventType);
        Assert.Equal(MouseModifiers.None, modifiers);
    }

    [Fact]
    public void XtermSgrMouseEvents_AllMouseUpEntries_UseMSuffix()
    {
        int count = 0;
        foreach (var kvp in MouseBindings.XtermSgrMouseEvents)
        {
            if (kvp.Value.EventType == MouseEventType.MouseUp)
            {
                Assert.Equal('m', kvp.Key.Suffix);
                count++;
            }
        }
        Assert.Equal(24, count);
    }

    [Fact]
    public void XtermSgrMouseEvents_AllNonUpEntries_UseCapitalMSuffix()
    {
        int count = 0;
        foreach (var kvp in MouseBindings.XtermSgrMouseEvents)
        {
            if (kvp.Value.EventType != MouseEventType.MouseUp)
            {
                Assert.Equal('M', kvp.Key.Suffix);
                count++;
            }
        }
        Assert.Equal(72, count);
    }

    [Fact]
    public void XtermSgrMouseEvents_MiddleDown_WithShiftAltControl()
    {
        var (button, eventType, modifiers) = MouseBindings.XtermSgrMouseEvents[(29, 'M')];
        Assert.Equal(MouseButton.Middle, button);
        Assert.Equal(MouseEventType.MouseDown, eventType);
        Assert.Equal(MouseModifiers.Shift | MouseModifiers.Alt | MouseModifiers.Control, modifiers);
    }

    [Fact]
    public void XtermSgrMouseEvents_RightDrag_WithControl()
    {
        var (button, eventType, modifiers) = MouseBindings.XtermSgrMouseEvents[(50, 'M')];
        Assert.Equal(MouseButton.Right, button);
        Assert.Equal(MouseEventType.MouseMove, eventType);
        Assert.Equal(MouseModifiers.Control, modifiers);
    }

    [Fact]
    public void XtermSgrMouseEvents_ScrollDown_WithAltControl()
    {
        var (button, eventType, modifiers) = MouseBindings.XtermSgrMouseEvents[(89, 'M')];
        Assert.Equal(MouseButton.None, button);
        Assert.Equal(MouseEventType.ScrollDown, eventType);
        Assert.Equal(MouseModifiers.Alt | MouseModifiers.Control, modifiers);
    }

    [Fact]
    public void XtermSgrMouseEvents_NoButtonMove_WithNoModifier()
    {
        var (button, eventType, modifiers) = MouseBindings.XtermSgrMouseEvents[(35, 'M')];
        Assert.Equal(MouseButton.None, button);
        Assert.Equal(MouseEventType.MouseMove, eventType);
        Assert.Equal(MouseModifiers.None, modifiers);
    }

    [Fact]
    public void XtermSgrMouseEvents_LeftUp_WithAlt()
    {
        var (button, eventType, modifiers) = MouseBindings.XtermSgrMouseEvents[(8, 'm')];
        Assert.Equal(MouseButton.Left, button);
        Assert.Equal(MouseEventType.MouseUp, eventType);
        Assert.Equal(MouseModifiers.Alt, modifiers);
    }

    // --- Drag entries (codes 32-63 with 'M') ---

    [Fact]
    public void XtermSgrMouseEvents_DragEntries_MapToMouseMove()
    {
        int dragCount = 0;
        foreach (var kvp in MouseBindings.XtermSgrMouseEvents)
        {
            if (kvp.Key.Code >= 32 && kvp.Key.Code <= 63 && kvp.Key.Suffix == 'M')
            {
                Assert.Equal(MouseEventType.MouseMove, kvp.Value.EventType);
                dragCount++;
            }
        }
        Assert.Equal(32, dragCount);
    }

    // --- Scroll entries (codes 64-93 with 'M') ---

    [Fact]
    public void XtermSgrMouseEvents_ScrollEntries_MapToScrollUpOrDown()
    {
        int scrollCount = 0;
        foreach (var kvp in MouseBindings.XtermSgrMouseEvents)
        {
            if (kvp.Key.Code >= 64 && kvp.Key.Code <= 93 && kvp.Key.Suffix == 'M')
            {
                Assert.True(
                    kvp.Value.EventType == MouseEventType.ScrollUp
                    || kvp.Value.EventType == MouseEventType.ScrollDown);
                scrollCount++;
            }
        }
        Assert.Equal(16, scrollCount);
    }

    // --- Modifier combinations on drag entries ---

    [Fact]
    public void XtermSgrMouseEvents_Code81M_IsScrollDownControl()
    {
        var (button, eventType, modifiers) = MouseBindings.XtermSgrMouseEvents[(81, 'M')];
        Assert.Equal(MouseButton.None, button);
        Assert.Equal(MouseEventType.ScrollDown, eventType);
        Assert.Equal(MouseModifiers.Control, modifiers);
    }

    // --- Typical (X10) lookup table tests ---

    [Fact]
    public void TypicalMouseEvents_Has10Entries()
    {
        Assert.Equal(10, MouseBindings.TypicalMouseEvents.Count);
    }

    [Fact]
    public void TypicalMouseEvents_Code32_IsLeftMouseDown()
    {
        var (button, eventType, modifiers) = MouseBindings.TypicalMouseEvents[32];
        Assert.Equal(MouseButton.Left, button);
        Assert.Equal(MouseEventType.MouseDown, eventType);
        Assert.Equal(MouseModifiers.None, modifiers);
    }

    [Fact]
    public void TypicalMouseEvents_Code33_IsMiddleMouseDown()
    {
        var (button, eventType, modifiers) = MouseBindings.TypicalMouseEvents[33];
        Assert.Equal(MouseButton.Middle, button);
        Assert.Equal(MouseEventType.MouseDown, eventType);
        Assert.Equal(MouseModifiers.None, modifiers);
    }

    [Fact]
    public void TypicalMouseEvents_Code34_IsRightMouseDown()
    {
        var (button, eventType, modifiers) = MouseBindings.TypicalMouseEvents[34];
        Assert.Equal(MouseButton.Right, button);
        Assert.Equal(MouseEventType.MouseDown, eventType);
        Assert.Equal(MouseModifiers.None, modifiers);
    }

    [Fact]
    public void TypicalMouseEvents_Code35_IsUnknownMouseUp()
    {
        var (button, eventType, modifiers) = MouseBindings.TypicalMouseEvents[35];
        Assert.Equal(MouseButton.Unknown, button);
        Assert.Equal(MouseEventType.MouseUp, eventType);
        Assert.Equal(MouseModifiers.None, modifiers);
    }

    [Fact]
    public void TypicalMouseEvents_Code64_IsLeftMouseMove()
    {
        var (button, eventType, modifiers) = MouseBindings.TypicalMouseEvents[64];
        Assert.Equal(MouseButton.Left, button);
        Assert.Equal(MouseEventType.MouseMove, eventType);
        Assert.Equal(MouseModifiers.None, modifiers);
    }

    [Fact]
    public void TypicalMouseEvents_Code67_IsNoButtonMouseMove()
    {
        var (button, eventType, modifiers) = MouseBindings.TypicalMouseEvents[67];
        Assert.Equal(MouseButton.None, button);
        Assert.Equal(MouseEventType.MouseMove, eventType);
        Assert.Equal(MouseModifiers.None, modifiers);
    }

    [Fact]
    public void TypicalMouseEvents_Code96_IsScrollUp()
    {
        var (button, eventType, modifiers) = MouseBindings.TypicalMouseEvents[96];
        Assert.Equal(MouseButton.None, button);
        Assert.Equal(MouseEventType.ScrollUp, eventType);
        Assert.Equal(MouseModifiers.None, modifiers);
    }

    [Fact]
    public void TypicalMouseEvents_Code97_IsScrollDown()
    {
        var (button, eventType, modifiers) = MouseBindings.TypicalMouseEvents[97];
        Assert.Equal(MouseButton.None, button);
        Assert.Equal(MouseEventType.ScrollDown, eventType);
        Assert.Equal(MouseModifiers.None, modifiers);
    }

    // --- URXVT lookup table tests ---

    [Fact]
    public void UrxvtMouseEvents_Has4Entries()
    {
        Assert.Equal(4, MouseBindings.UrxvtMouseEvents.Count);
    }

    [Fact]
    public void UrxvtMouseEvents_Code32_IsUnknownMouseDown()
    {
        var (button, eventType, modifiers) = MouseBindings.UrxvtMouseEvents[32];
        Assert.Equal(MouseButton.Unknown, button);
        Assert.Equal(MouseEventType.MouseDown, eventType);
        Assert.Equal(MouseModifiers.None, modifiers);
    }

    [Fact]
    public void UrxvtMouseEvents_Code35_IsUnknownMouseUp()
    {
        var (button, eventType, modifiers) = MouseBindings.UrxvtMouseEvents[35];
        Assert.Equal(MouseButton.Unknown, button);
        Assert.Equal(MouseEventType.MouseUp, eventType);
        Assert.Equal(MouseModifiers.None, modifiers);
    }

    [Fact]
    public void UrxvtMouseEvents_Code96_IsScrollUp()
    {
        var (button, eventType, modifiers) = MouseBindings.UrxvtMouseEvents[96];
        Assert.Equal(MouseButton.None, button);
        Assert.Equal(MouseEventType.ScrollUp, eventType);
        Assert.Equal(MouseModifiers.None, modifiers);
    }

    [Fact]
    public void UrxvtMouseEvents_Code97_IsScrollDown()
    {
        var (button, eventType, modifiers) = MouseBindings.UrxvtMouseEvents[97];
        Assert.Equal(MouseButton.None, button);
        Assert.Equal(MouseEventType.ScrollDown, eventType);
        Assert.Equal(MouseModifiers.None, modifiers);
    }
}
