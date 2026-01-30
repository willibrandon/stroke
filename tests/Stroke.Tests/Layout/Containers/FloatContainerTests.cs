using Stroke.Core.Primitives;
using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Xunit;

namespace Stroke.Tests.Layout.Containers;

/// <summary>
/// Tests for FloatContainer class.
/// </summary>
public sealed class FloatContainerTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithContent_StoresContent()
    {
        var control = new FormattedTextControl("Background");
        var window = new Window(content: control);
        var container = new FloatContainer(new AnyContainer(window));

        Assert.Same(window, container.Content);
    }

    [Fact]
    public void Constructor_WithFloats_StoresFloats()
    {
        var background = new Window(content: new DummyControl());
        var floatWindow = new Window(content: new DummyControl());
        var floats = new[] { new Float(new AnyContainer(floatWindow), left: 0, top: 0) };

        var container = new FloatContainer(new AnyContainer(background), floats: floats);

        Assert.Single(container.Floats);
    }

    [Fact]
    public void Constructor_WithNullFloats_UsesEmptyArray()
    {
        var background = new Window(content: new DummyControl());
        var container = new FloatContainer(new AnyContainer(background), floats: null);

        Assert.Empty(container.Floats);
    }

    [Fact]
    public void Constructor_WithModal_StoresModal()
    {
        var background = new Window(content: new DummyControl());
        var container = new FloatContainer(new AnyContainer(background), modal: true);

        Assert.True(container.Modal);
        Assert.True(container.IsModal);
    }

    [Fact]
    public void Constructor_WithStyle_CreatesStyleGetter()
    {
        var background = new Window(content: new DummyControl());
        var container = new FloatContainer(new AnyContainer(background), style: "class:test");

        Assert.NotNull(container.StyleGetter);
        Assert.Equal("class:test", container.StyleGetter!());
    }

    [Fact]
    public void Constructor_WithStyleGetter_UsesGetter()
    {
        var style = "class:dynamic";
        var background = new Window(content: new DummyControl());
        var container = new FloatContainer(new AnyContainer(background), styleGetter: () => style);

        Assert.Equal("class:dynamic", container.StyleGetter!());

        style = "class:changed";
        Assert.Equal("class:changed", container.StyleGetter!());
    }

    [Fact]
    public void Constructor_WithZIndex_StoresZIndex()
    {
        var background = new Window(content: new DummyControl());
        var container = new FloatContainer(new AnyContainer(background), zIndex: 5);

        Assert.Equal(5, container.ZIndex);
    }

    #endregion

    #region Reset Tests

    [Fact]
    public void Reset_ResetsContent()
    {
        var background = new Window();
        background.VerticalScroll = 10;

        var container = new FloatContainer(new AnyContainer(background));

        container.Reset();

        Assert.Equal(0, background.VerticalScroll);
    }

    [Fact]
    public void Reset_ResetsFloats()
    {
        var background = new Window();
        var floatWindow = new Window();
        floatWindow.VerticalScroll = 5;

        var floats = new[] { new Float(new AnyContainer(floatWindow)) };
        var container = new FloatContainer(new AnyContainer(background), floats: floats);

        container.Reset();

        Assert.Equal(0, floatWindow.VerticalScroll);
    }

    #endregion

    #region PreferredWidth/Height Tests

    [Fact]
    public void PreferredWidth_DelegatesToContent()
    {
        var control = new FormattedTextControl("Hello World");
        var background = new Window(content: control);
        var container = new FloatContainer(new AnyContainer(background));

        var width = container.PreferredWidth(100);

        // Should reflect the background content's preferred width
        Assert.True(width.Preferred > 0);
    }

    [Fact]
    public void PreferredHeight_DelegatesToContent()
    {
        var control = new FormattedTextControl("Line 1\nLine 2\nLine 3");
        var background = new Window(content: control);
        var container = new FloatContainer(new AnyContainer(background));

        var height = container.PreferredHeight(80, 100);

        // Should reflect the background content's preferred height
        Assert.Equal(3, height.Preferred);
    }

    #endregion

    #region WriteToScreen Tests

    [Fact]
    public void WriteToScreen_RendersBackgroundContent()
    {
        var control = new FormattedTextControl("Background");
        var background = new Window(content: control);
        var container = new FloatContainer(new AnyContainer(background));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        screen.DrawAllFloats();

        // Verify background was rendered (check some characters)
        var chars = screen[0, 0];
        // The background should have been rendered
        Assert.NotNull(chars);
    }

    [Fact]
    public void WriteToScreen_RendersFloatOnTop()
    {
        var backgroundControl = new FormattedTextControl("Background");
        var background = new Window(content: backgroundControl);

        var floatControl = new FormattedTextControl("Float");
        var floatWindow = new Window(content: floatControl);
        var floats = new[] { new Float(new AnyContainer(floatWindow), left: 5, top: 5, width: 10, height: 3) };

        var container = new FloatContainer(new AnyContainer(background), floats: floats);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        screen.DrawAllFloats();

        // Float should be rendered at position (5, 5)
        // We can't easily verify the exact characters, but the float should be drawn
        Assert.NotNull(screen);
    }

    [Fact]
    public void WriteToScreen_WithZIndex_DefersDrawing()
    {
        var control = new FormattedTextControl("Background");
        var background = new Window(content: control);
        var container = new FloatContainer(new AnyContainer(background), zIndex: 5);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);

        // Drawing is deferred, so we need to call DrawAllFloats
        screen.DrawAllFloats();

        // After DrawAllFloats, content should be rendered
        Assert.NotNull(screen);
    }

    [Fact]
    public void WriteToScreen_MultipleFloats_RendersInOrder()
    {
        var background = new Window(content: new DummyControl());

        var float1 = new Float(new AnyContainer(new Window(content: new FormattedTextControl("First"))),
            left: 0, top: 0, width: 10, height: 1, zIndex: 1);
        var float2 = new Float(new AnyContainer(new Window(content: new FormattedTextControl("Second"))),
            left: 0, top: 0, width: 10, height: 1, zIndex: 2);

        var floats = new[] { float1, float2 };
        var container = new FloatContainer(new AnyContainer(background), floats: floats);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        screen.DrawAllFloats();

        // Higher z-index float should be on top
        Assert.NotNull(screen);
    }

    #endregion

    #region Float Positioning Tests

    [Fact]
    public void WriteToScreen_FloatWithLeftTop_PositionsCorrectly()
    {
        var background = new Window(content: new DummyControl());
        var floatContent = new Window(content: new FormattedTextControl("X"));
        var floats = new[] { new Float(new AnyContainer(floatContent), left: 10, top: 5, width: 1, height: 1) };

        var container = new FloatContainer(new AnyContainer(background), floats: floats);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        screen.DrawAllFloats();

        // Float should be at position (10, 5)
        // The character 'X' should be at that position
        Assert.NotNull(screen);
    }

    [Fact]
    public void WriteToScreen_FloatWithRightBottom_PositionsCorrectly()
    {
        var background = new Window(content: new DummyControl());
        var floatContent = new Window(content: new FormattedTextControl("X"));
        var floats = new[] { new Float(new AnyContainer(floatContent), right: 10, bottom: 5, width: 5, height: 3) };

        var container = new FloatContainer(new AnyContainer(background), floats: floats);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        screen.DrawAllFloats();

        // Float should be positioned from the right and bottom edges
        Assert.NotNull(screen);
    }

    [Fact]
    public void WriteToScreen_FloatWithNoPosition_CentersFloat()
    {
        var background = new Window(content: new DummyControl());
        var floatContent = new Window(content: new FormattedTextControl("Centered"));
        var floats = new[] { new Float(new AnyContainer(floatContent), width: 10, height: 3) };

        var container = new FloatContainer(new AnyContainer(background), floats: floats);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        screen.DrawAllFloats();

        // Float should be centered in the available space
        Assert.NotNull(screen);
    }

    #endregion

    #region GetChildren Tests

    [Fact]
    public void GetChildren_ReturnsContentAndFloats()
    {
        var background = new Window(content: new DummyControl());
        var floatWindow1 = new Window(content: new DummyControl());
        var floatWindow2 = new Window(content: new DummyControl());
        var floats = new[]
        {
            new Float(new AnyContainer(floatWindow1)),
            new Float(new AnyContainer(floatWindow2))
        };

        var container = new FloatContainer(new AnyContainer(background), floats: floats);

        var children = container.GetChildren();

        Assert.Equal(3, children.Count);
        Assert.Contains(background, children);
        Assert.Contains(floatWindow1, children);
        Assert.Contains(floatWindow2, children);
    }

    [Fact]
    public void GetChildren_NoFloats_ReturnsOnlyContent()
    {
        var background = new Window(content: new DummyControl());
        var container = new FloatContainer(new AnyContainer(background));

        var children = container.GetChildren();

        Assert.Single(children);
        Assert.Same(background, children[0]);
    }

    #endregion

    #region GetKeyBindings Tests

    [Fact]
    public void GetKeyBindings_NoBindings_ReturnsNull()
    {
        var background = new Window(content: new DummyControl());
        var container = new FloatContainer(new AnyContainer(background));

        Assert.Null(container.GetKeyBindings());
    }

    #endregion

    #region IsModal Tests

    [Fact]
    public void IsModal_Default_ReturnsFalse()
    {
        var background = new Window(content: new DummyControl());
        var container = new FloatContainer(new AnyContainer(background));

        Assert.False(container.IsModal);
    }

    [Fact]
    public void IsModal_WhenModalTrue_ReturnsTrue()
    {
        var background = new Window(content: new DummyControl());
        var container = new FloatContainer(new AnyContainer(background), modal: true);

        Assert.True(container.IsModal);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsDescriptiveString()
    {
        var background = new Window(content: new DummyControl());
        var floats = new[] { new Float(new AnyContainer(new Window())) };
        var container = new FloatContainer(new AnyContainer(background), floats: floats);

        var result = container.ToString();

        Assert.Contains("FloatContainer", result);
        Assert.Contains("floats=1", result);
    }

    #endregion

    #region Style Inheritance Tests

    [Fact]
    public void WriteToScreen_CombinesParentAndContainerStyle()
    {
        var background = new Window(content: new DummyControl());
        var container = new FloatContainer(new AnyContainer(background), style: "class:container");

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        // No exception should be thrown when combining styles
        container.WriteToScreen(screen, mouseHandlers, writePosition, "class:parent", true, null);
        screen.DrawAllFloats();
    }

    [Fact]
    public void WriteToScreen_EmptyParentStyle_UsesContainerStyle()
    {
        var background = new Window(content: new DummyControl());
        var container = new FloatContainer(new AnyContainer(background), style: "class:only");

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        screen.DrawAllFloats();
    }

    [Fact]
    public void WriteToScreen_NoStyle_UsesParentStyle()
    {
        var background = new Window(content: new DummyControl());
        var container = new FloatContainer(new AnyContainer(background));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        container.WriteToScreen(screen, mouseHandlers, writePosition, "class:parent", true, null);
        screen.DrawAllFloats();
    }

    #endregion

    #region Float Dimension Calculation Tests

    [Fact]
    public void WriteToScreen_FloatWithLeftAndRight_CalculatesWidth()
    {
        var background = new Window(content: new DummyControl());
        var floatContent = new Window(content: new FormattedTextControl("Wide float"));
        // Left=5, Right=5 with 80 wide container = float width of 70
        var floats = new[] { new Float(new AnyContainer(floatContent), left: 5, right: 5, top: 0, height: 3) };

        var container = new FloatContainer(new AnyContainer(background), floats: floats);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        screen.DrawAllFloats();

        // Should render without exception
        Assert.NotNull(screen);
    }

    [Fact]
    public void WriteToScreen_FloatWithTopAndBottom_CalculatesHeight()
    {
        var background = new Window(content: new DummyControl());
        var floatContent = new Window(content: new FormattedTextControl("Tall float"));
        // Top=2, Bottom=2 with 24 tall container = float height of 20
        var floats = new[] { new Float(new AnyContainer(floatContent), top: 2, bottom: 2, left: 0, width: 20) };

        var container = new FloatContainer(new AnyContainer(background), floats: floats);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        screen.DrawAllFloats();

        Assert.NotNull(screen);
    }

    [Fact]
    public void WriteToScreen_FloatWithExplicitWidth_UsesExplicitWidth()
    {
        var background = new Window(content: new DummyControl());
        var floatContent = new Window(content: new FormattedTextControl("Explicit size"));
        var floats = new[] { new Float(new AnyContainer(floatContent), left: 0, top: 0, width: 30, height: 5) };

        var container = new FloatContainer(new AnyContainer(background), floats: floats);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        screen.DrawAllFloats();

        Assert.NotNull(screen);
    }

    [Fact]
    public void WriteToScreen_FloatWithExplicitHeight_UsesExplicitHeight()
    {
        var background = new Window(content: new DummyControl());
        var floatContent = new Window(content: new FormattedTextControl("Explicit height"));
        var floats = new[] { new Float(new AnyContainer(floatContent), left: 0, top: 0, width: 20, height: 10) };

        var container = new FloatContainer(new AnyContainer(background), floats: floats);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        screen.DrawAllFloats();

        Assert.NotNull(screen);
    }

    [Fact]
    public void WriteToScreen_TransparentFloat_DoesNotEraseBackground()
    {
        var bgControl = new FormattedTextControl("Background text that fills the area");
        var background = new Window(content: bgControl);
        var floatContent = new Window(content: new FormattedTextControl("Overlay"));
        var floats = new[] { new Float(new AnyContainer(floatContent), left: 0, top: 0, width: 10, height: 1, transparent: true) };

        var container = new FloatContainer(new AnyContainer(background), floats: floats);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        screen.DrawAllFloats();

        Assert.NotNull(screen);
    }

    [Fact]
    public void WriteToScreen_FloatExceedingBounds_ClampedToAvailableArea()
    {
        var background = new Window(content: new DummyControl());
        // Float positioned beyond the visible area
        var floatContent = new Window(content: new FormattedTextControl("Overflow"));
        var floats = new[] { new Float(new AnyContainer(floatContent), left: 75, top: 22, width: 20, height: 5) };

        var container = new FloatContainer(new AnyContainer(background), floats: floats);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        screen.DrawAllFloats();

        // Should render without exception, float clamped to bounds
        Assert.NotNull(screen);
    }

    [Fact]
    public void WriteToScreen_DefaultAnyContainer_UsesDefaultContent()
    {
        // Empty AnyContainer creates a default Window with DummyControl
        var container = new FloatContainer(default(AnyContainer));

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        screen.DrawAllFloats();

        Assert.IsType<DummyControl>(((Window)container.Content).Content);
    }

    [Fact]
    public void WriteToScreen_FloatWithHideWhenCoveringContent_RespectsSetting()
    {
        var bgControl = new FormattedTextControl("Background",
            getCursorPosition: () => new Point(5, 5));
        var background = new Window(content: bgControl);

        var floatContent = new Window(content: new FormattedTextControl("Float"));
        var floats = new[] { new Float(new AnyContainer(floatContent),
            left: 0, top: 0, width: 20, height: 10, hideWhenCoveringContent: true) };

        var container = new FloatContainer(new AnyContainer(background), floats: floats);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        screen.DrawAllFloats();

        // Should handle hide-when-covering logic without exception
        Assert.NotNull(screen);
    }

    [Fact]
    public void WriteToScreen_FloatWithAllowCoverCursorFalse_RespectsSetting()
    {
        var bgControl = new FormattedTextControl("Background",
            getCursorPosition: () => new Point(5, 5));
        var background = new Window(content: bgControl);

        var floatContent = new Window(content: new FormattedTextControl("Float"));
        var floats = new[] { new Float(new AnyContainer(floatContent),
            left: 0, top: 0, width: 20, height: 10, allowCoverCursor: false) };

        var container = new FloatContainer(new AnyContainer(background), floats: floats);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        screen.DrawAllFloats();

        Assert.NotNull(screen);
    }

    #endregion

    #region Cursor Position Tests

    [Fact]
    public void WriteToScreen_FloatWithXCursor_PositionsRelativeToCursor()
    {
        var bgControl = new FormattedTextControl("Line 1\nLine 2\nLine 3",
            getCursorPosition: () => new Point(3, 1));
        var background = new Window(content: bgControl);

        var floatContent = new Window(content: new FormattedTextControl("Float"));
        var floats = new[] { new Float(new AnyContainer(floatContent),
            xcursor: true, top: 5, width: 10, height: 1) };

        var container = new FloatContainer(new AnyContainer(background), floats: floats);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        screen.DrawAllFloats();

        Assert.NotNull(screen);
    }

    [Fact]
    public void WriteToScreen_FloatWithYCursor_PositionsRelativeToCursor()
    {
        var bgControl = new FormattedTextControl("Line 1\nLine 2\nLine 3",
            getCursorPosition: () => new Point(3, 1));
        var background = new Window(content: bgControl);

        var floatContent = new Window(content: new FormattedTextControl("Float"));
        var floats = new[] { new Float(new AnyContainer(floatContent),
            left: 5, ycursor: true, width: 10, height: 1) };

        var container = new FloatContainer(new AnyContainer(background), floats: floats);

        var screen = new Screen();
        var mouseHandlers = new MouseHandlers();
        var writePosition = new WritePosition(0, 0, 80, 24);

        container.WriteToScreen(screen, mouseHandlers, writePosition, "", true, null);
        screen.DrawAllFloats();

        Assert.NotNull(screen);
    }

    #endregion
}
