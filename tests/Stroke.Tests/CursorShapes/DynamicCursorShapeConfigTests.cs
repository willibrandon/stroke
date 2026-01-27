using Stroke.CursorShapes;
using Xunit;

namespace Stroke.Tests.CursorShapes;

/// <summary>
/// Tests for <see cref="DynamicCursorShapeConfig"/>.
/// </summary>
public sealed class DynamicCursorShapeConfigTests
{
    #region Basic Delegation

    [Fact]
    public void GetCursorShape_WithSimpleConfig_ReturnsDelegatedShape()
    {
        var innerConfig = new SimpleCursorShapeConfig(CursorShape.Block);
        var dynamicConfig = new DynamicCursorShapeConfig(() => innerConfig);

        Assert.Equal(CursorShape.Block, dynamicConfig.GetCursorShape());
    }

    [Fact]
    public void GetCursorShape_WithNullConfig_ReturnsNeverChange()
    {
        var dynamicConfig = new DynamicCursorShapeConfig(() => null);

        Assert.Equal(CursorShape.NeverChange, dynamicConfig.GetCursorShape());
    }

    #endregion

    #region Dynamic Switching

    [Fact]
    public void GetCursorShape_ConfigChanges_ReturnsUpdatedShape()
    {
        ICursorShapeConfig? currentConfig = new SimpleCursorShapeConfig(CursorShape.Block);
        var dynamicConfig = new DynamicCursorShapeConfig(() => currentConfig);

        Assert.Equal(CursorShape.Block, dynamicConfig.GetCursorShape());

        currentConfig = new SimpleCursorShapeConfig(CursorShape.Beam);
        Assert.Equal(CursorShape.Beam, dynamicConfig.GetCursorShape());

        currentConfig = new SimpleCursorShapeConfig(CursorShape.Underline);
        Assert.Equal(CursorShape.Underline, dynamicConfig.GetCursorShape());
    }

    [Fact]
    public void GetCursorShape_SwitchToNull_ReturnsNeverChange()
    {
        ICursorShapeConfig? currentConfig = new SimpleCursorShapeConfig(CursorShape.Block);
        var dynamicConfig = new DynamicCursorShapeConfig(() => currentConfig);

        Assert.Equal(CursorShape.Block, dynamicConfig.GetCursorShape());

        currentConfig = null;
        Assert.Equal(CursorShape.NeverChange, dynamicConfig.GetCursorShape());
    }

    [Fact]
    public void GetCursorShape_SwitchFromNull_ReturnsNewShape()
    {
        ICursorShapeConfig? currentConfig = null;
        var dynamicConfig = new DynamicCursorShapeConfig(() => currentConfig);

        Assert.Equal(CursorShape.NeverChange, dynamicConfig.GetCursorShape());

        currentConfig = new SimpleCursorShapeConfig(CursorShape.BlinkingBlock);
        Assert.Equal(CursorShape.BlinkingBlock, dynamicConfig.GetCursorShape());
    }

    #endregion

    #region Nested Dynamic Configs

    [Fact]
    public void GetCursorShape_NestedDynamicConfig_DelegatesCorrectly()
    {
        var innerConfig = new SimpleCursorShapeConfig(CursorShape.Beam);
        var middleConfig = new DynamicCursorShapeConfig(() => innerConfig);
        var outerConfig = new DynamicCursorShapeConfig(() => middleConfig);

        Assert.Equal(CursorShape.Beam, outerConfig.GetCursorShape());
    }

    [Fact]
    public void GetCursorShape_NestedWithModalConfig_DelegatesCorrectly()
    {
        var currentMode = ModalCursorShapeConfig.EditingMode.ViNavigation;
        var modalConfig = new ModalCursorShapeConfig(() => currentMode);
        var dynamicConfig = new DynamicCursorShapeConfig(() => modalConfig);

        Assert.Equal(CursorShape.Block, dynamicConfig.GetCursorShape());

        currentMode = ModalCursorShapeConfig.EditingMode.ViInsert;
        Assert.Equal(CursorShape.Beam, dynamicConfig.GetCursorShape());
    }

    #endregion

    #region Constructor Validation

    [Fact]
    public void Constructor_NullFunc_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new DynamicCursorShapeConfig(null!));
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ImplementsICursorShapeConfig()
    {
        var config = new DynamicCursorShapeConfig(() => null);

        Assert.IsAssignableFrom<ICursorShapeConfig>(config);
    }

    #endregion

    #region All Cursor Shapes

    [Theory]
    [InlineData(CursorShape.NeverChange)]
    [InlineData(CursorShape.Block)]
    [InlineData(CursorShape.Beam)]
    [InlineData(CursorShape.Underline)]
    [InlineData(CursorShape.BlinkingBlock)]
    [InlineData(CursorShape.BlinkingBeam)]
    [InlineData(CursorShape.BlinkingUnderline)]
    public void GetCursorShape_AllShapes_ReturnsCorrectly(CursorShape shape)
    {
        var innerConfig = new SimpleCursorShapeConfig(shape);
        var dynamicConfig = new DynamicCursorShapeConfig(() => innerConfig);

        Assert.Equal(shape, dynamicConfig.GetCursorShape());
    }

    #endregion

    #region Multiple Calls

    [Fact]
    public void GetCursorShape_MultipleCalls_CallsDelegateEachTime()
    {
        var callCount = 0;
        var innerConfig = new SimpleCursorShapeConfig(CursorShape.Block);
        var dynamicConfig = new DynamicCursorShapeConfig(() =>
        {
            callCount++;
            return innerConfig;
        });

        dynamicConfig.GetCursorShape();
        dynamicConfig.GetCursorShape();
        dynamicConfig.GetCursorShape();

        Assert.Equal(3, callCount);
    }

    #endregion
}
