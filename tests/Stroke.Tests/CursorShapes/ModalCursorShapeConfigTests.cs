using Stroke.CursorShapes;
using Xunit;
using static Stroke.CursorShapes.ModalCursorShapeConfig;

namespace Stroke.Tests.CursorShapes;

/// <summary>
/// Tests for <see cref="ModalCursorShapeConfig"/>.
/// </summary>
public sealed class ModalCursorShapeConfigTests
{
    #region Vi Navigation Mode

    [Fact]
    public void GetCursorShape_ViNavigation_ReturnsBlock()
    {
        var config = new ModalCursorShapeConfig(() => EditingMode.ViNavigation);

        Assert.Equal(CursorShape.Block, config.GetCursorShape());
    }

    [Fact]
    public void GetCursorShape_ViNavigation_CustomShape_ReturnsCustom()
    {
        var config = new ModalCursorShapeConfig(
            () => EditingMode.ViNavigation,
            navigationShape: CursorShape.BlinkingBlock);

        Assert.Equal(CursorShape.BlinkingBlock, config.GetCursorShape());
    }

    #endregion

    #region Vi Insert Mode

    [Fact]
    public void GetCursorShape_ViInsert_ReturnsBeam()
    {
        var config = new ModalCursorShapeConfig(() => EditingMode.ViInsert);

        Assert.Equal(CursorShape.Beam, config.GetCursorShape());
    }

    [Fact]
    public void GetCursorShape_ViInsert_CustomShape_ReturnsCustom()
    {
        var config = new ModalCursorShapeConfig(
            () => EditingMode.ViInsert,
            insertShape: CursorShape.BlinkingBeam);

        Assert.Equal(CursorShape.BlinkingBeam, config.GetCursorShape());
    }

    #endregion

    #region Vi Replace Mode

    [Fact]
    public void GetCursorShape_ViReplace_ReturnsUnderline()
    {
        var config = new ModalCursorShapeConfig(() => EditingMode.ViReplace);

        Assert.Equal(CursorShape.Underline, config.GetCursorShape());
    }

    [Fact]
    public void GetCursorShape_ViReplace_CustomShape_ReturnsCustom()
    {
        var config = new ModalCursorShapeConfig(
            () => EditingMode.ViReplace,
            replaceShape: CursorShape.BlinkingUnderline);

        Assert.Equal(CursorShape.BlinkingUnderline, config.GetCursorShape());
    }

    #endregion

    #region Emacs Mode

    [Fact]
    public void GetCursorShape_Emacs_ReturnsBeam()
    {
        var config = new ModalCursorShapeConfig(() => EditingMode.Emacs);

        Assert.Equal(CursorShape.Beam, config.GetCursorShape());
    }

    [Fact]
    public void GetCursorShape_Emacs_UsesInsertShape()
    {
        var config = new ModalCursorShapeConfig(
            () => EditingMode.Emacs,
            insertShape: CursorShape.Block);

        // Emacs uses the insert shape
        Assert.Equal(CursorShape.Block, config.GetCursorShape());
    }

    #endregion

    #region Dynamic Mode Changes

    [Fact]
    public void GetCursorShape_ModeChanges_ReturnsCorrectShape()
    {
        var currentMode = EditingMode.ViNavigation;
        var config = new ModalCursorShapeConfig(() => currentMode);

        Assert.Equal(CursorShape.Block, config.GetCursorShape());

        currentMode = EditingMode.ViInsert;
        Assert.Equal(CursorShape.Beam, config.GetCursorShape());

        currentMode = EditingMode.ViReplace;
        Assert.Equal(CursorShape.Underline, config.GetCursorShape());

        currentMode = EditingMode.Emacs;
        Assert.Equal(CursorShape.Beam, config.GetCursorShape());
    }

    #endregion

    #region Unknown Editing Mode

    [Fact]
    public void GetCursorShape_UnknownEditingMode_ReturnsNeverChange()
    {
        var config = new ModalCursorShapeConfig(() => (EditingMode)999);

        Assert.Equal(CursorShape.NeverChange, config.GetCursorShape());
    }

    #endregion

    #region Constructor Validation

    [Fact]
    public void Constructor_NullFunc_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ModalCursorShapeConfig(null!));
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ImplementsICursorShapeConfig()
    {
        var config = new ModalCursorShapeConfig(() => EditingMode.ViNavigation);

        Assert.IsAssignableFrom<ICursorShapeConfig>(config);
    }

    #endregion

    #region All Shapes Configurable

    [Fact]
    public void Constructor_AllShapesConfigurable()
    {
        var currentMode = EditingMode.ViNavigation;
        var config = new ModalCursorShapeConfig(
            () => currentMode,
            navigationShape: CursorShape.BlinkingBlock,
            insertShape: CursorShape.BlinkingBeam,
            replaceShape: CursorShape.BlinkingUnderline);

        currentMode = EditingMode.ViNavigation;
        Assert.Equal(CursorShape.BlinkingBlock, config.GetCursorShape());

        currentMode = EditingMode.ViInsert;
        Assert.Equal(CursorShape.BlinkingBeam, config.GetCursorShape());

        currentMode = EditingMode.ViReplace;
        Assert.Equal(CursorShape.BlinkingUnderline, config.GetCursorShape());
    }

    #endregion
}
