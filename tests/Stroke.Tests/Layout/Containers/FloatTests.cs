using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Xunit;

namespace Stroke.Tests.Layout.Containers;

/// <summary>
/// Tests for Float class.
/// </summary>
public sealed class FloatTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithContent_StoresContent()
    {
        var control = new DummyControl();
        var window = new Window(content: control);
        var floatElement = new Float(new AnyContainer(window));

        Assert.True(floatElement.Content.HasValue);
        Assert.Same(window, floatElement.Content.ToContainer());
    }

    [Fact]
    public void Constructor_WithPosition_StoresPosition()
    {
        var window = new Window();
        var floatElement = new Float(
            new AnyContainer(window),
            top: 5,
            left: 10,
            right: 15,
            bottom: 20);

        Assert.Equal(5, floatElement.Top);
        Assert.Equal(10, floatElement.Left);
        Assert.Equal(15, floatElement.Right);
        Assert.Equal(20, floatElement.Bottom);
    }

    [Fact]
    public void Constructor_WithExplicitWidth_CreatesWidthGetter()
    {
        var window = new Window();
        var floatElement = new Float(new AnyContainer(window), width: 50);

        Assert.NotNull(floatElement.WidthGetter);
        Assert.Equal(50, floatElement.GetWidth());
    }

    [Fact]
    public void Constructor_WithExplicitHeight_CreatesHeightGetter()
    {
        var window = new Window();
        var floatElement = new Float(new AnyContainer(window), height: 30);

        Assert.NotNull(floatElement.HeightGetter);
        Assert.Equal(30, floatElement.GetHeight());
    }

    [Fact]
    public void Constructor_WithWidthGetter_UsesGetter()
    {
        var width = 25;
        var window = new Window();
        var floatElement = new Float(new AnyContainer(window), widthGetter: () => width);

        Assert.Equal(25, floatElement.GetWidth());

        width = 35;
        Assert.Equal(35, floatElement.GetWidth());
    }

    [Fact]
    public void Constructor_WithHeightGetter_UsesGetter()
    {
        var height = 15;
        var window = new Window();
        var floatElement = new Float(new AnyContainer(window), heightGetter: () => height);

        Assert.Equal(15, floatElement.GetHeight());

        height = 25;
        Assert.Equal(25, floatElement.GetHeight());
    }

    [Fact]
    public void Constructor_WithXCursor_StoresXCursor()
    {
        var window = new Window();
        var floatElement = new Float(new AnyContainer(window), xcursor: true);

        Assert.True(floatElement.XCursor);
    }

    [Fact]
    public void Constructor_WithYCursor_StoresYCursor()
    {
        var window = new Window();
        var floatElement = new Float(new AnyContainer(window), ycursor: true);

        Assert.True(floatElement.YCursor);
    }

    [Fact]
    public void Constructor_WithAttachToWindow_StoresWindow()
    {
        var attachWindow = new Window();
        var contentWindow = new Window();
        var floatElement = new Float(new AnyContainer(contentWindow), attachToWindow: attachWindow);

        Assert.Same(attachWindow, floatElement.AttachToWindow);
    }

    [Fact]
    public void Constructor_WithHideWhenCoveringContent_StoresFlag()
    {
        var window = new Window();
        var floatElement = new Float(new AnyContainer(window), hideWhenCoveringContent: true);

        Assert.True(floatElement.HideWhenCoveringContent);
    }

    [Fact]
    public void Constructor_WithAllowCoverCursor_StoresFlag()
    {
        var window = new Window();
        var floatElement = new Float(new AnyContainer(window), allowCoverCursor: true);

        Assert.True(floatElement.AllowCoverCursor);
    }

    [Fact]
    public void Constructor_WithTransparent_StoresFlag()
    {
        var window = new Window();
        var floatElement = new Float(new AnyContainer(window), transparent: true);

        Assert.True(floatElement.Transparent);
    }

    #endregion

    #region Z-Index Tests

    [Fact]
    public void Constructor_DefaultZIndex_IsOne()
    {
        var window = new Window();
        var floatElement = new Float(new AnyContainer(window));

        Assert.Equal(1, floatElement.ZIndex);
    }

    [Fact]
    public void Constructor_WithZIndex_StoresZIndex()
    {
        var window = new Window();
        var floatElement = new Float(new AnyContainer(window), zIndex: 5);

        Assert.Equal(5, floatElement.ZIndex);
    }

    [Fact]
    public void Constructor_WithZIndexLessThanOne_ClampsToOne()
    {
        var window = new Window();
        var floatElement = new Float(new AnyContainer(window), zIndex: 0);

        Assert.Equal(1, floatElement.ZIndex);
    }

    [Fact]
    public void Constructor_WithNegativeZIndex_ClampsToOne()
    {
        var window = new Window();
        var floatElement = new Float(new AnyContainer(window), zIndex: -5);

        Assert.Equal(1, floatElement.ZIndex);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void Constructor_WithXCursorAndLeft_ThrowsArgumentException()
    {
        var window = new Window();

        Assert.Throws<ArgumentException>(() =>
            new Float(new AnyContainer(window), xcursor: true, left: 10));
    }

    [Fact]
    public void Constructor_WithYCursorAndTop_ThrowsArgumentException()
    {
        var window = new Window();

        Assert.Throws<ArgumentException>(() =>
            new Float(new AnyContainer(window), ycursor: true, top: 5));
    }

    [Fact]
    public void Constructor_WithXCursorAndRight_IsAllowed()
    {
        var window = new Window();

        // Should not throw - right is allowed with xcursor
        var floatElement = new Float(new AnyContainer(window), xcursor: true, right: 10);

        Assert.True(floatElement.XCursor);
        Assert.Equal(10, floatElement.Right);
    }

    [Fact]
    public void Constructor_WithYCursorAndBottom_IsAllowed()
    {
        var window = new Window();

        // Should not throw - bottom is allowed with ycursor
        var floatElement = new Float(new AnyContainer(window), ycursor: true, bottom: 5);

        Assert.True(floatElement.YCursor);
        Assert.Equal(5, floatElement.Bottom);
    }

    #endregion

    #region GetWidth/GetHeight Tests

    [Fact]
    public void GetWidth_NoWidthGetter_ReturnsNull()
    {
        var window = new Window();
        var floatElement = new Float(new AnyContainer(window));

        Assert.Null(floatElement.GetWidth());
    }

    [Fact]
    public void GetHeight_NoHeightGetter_ReturnsNull()
    {
        var window = new Window();
        var floatElement = new Float(new AnyContainer(window));

        Assert.Null(floatElement.GetHeight());
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsDescriptiveString()
    {
        var window = new Window();
        var floatElement = new Float(new AnyContainer(window), top: 5, left: 10, zIndex: 3);

        var result = floatElement.ToString();

        Assert.Contains("Float", result);
        Assert.Contains("top=5", result);
        Assert.Contains("left=10", result);
        Assert.Contains("zIndex=3", result);
    }

    #endregion

    #region Default Value Tests

    [Fact]
    public void Constructor_Defaults_AllPositionsNull()
    {
        var window = new Window();
        var floatElement = new Float(new AnyContainer(window));

        Assert.Null(floatElement.Top);
        Assert.Null(floatElement.Right);
        Assert.Null(floatElement.Bottom);
        Assert.Null(floatElement.Left);
    }

    [Fact]
    public void Constructor_Defaults_CursorRelativeFalse()
    {
        var window = new Window();
        var floatElement = new Float(new AnyContainer(window));

        Assert.False(floatElement.XCursor);
        Assert.False(floatElement.YCursor);
    }

    [Fact]
    public void Constructor_Defaults_FlagsFalse()
    {
        var window = new Window();
        var floatElement = new Float(new AnyContainer(window));

        Assert.False(floatElement.HideWhenCoveringContent);
        Assert.False(floatElement.AllowCoverCursor);
        Assert.False(floatElement.Transparent);
    }

    [Fact]
    public void Constructor_Defaults_AttachToWindowNull()
    {
        var window = new Window();
        var floatElement = new Float(new AnyContainer(window));

        Assert.Null(floatElement.AttachToWindow);
    }

    #endregion
}
