using Stroke.Application;
using Stroke.Output;
using Xunit;

namespace Stroke.Tests.Application;

/// <summary>
/// Tests for <see cref="ColorDepthOption"/>.
/// </summary>
public sealed class ColorDepthOptionTests
{
    #region Fixed Value Tests

    [Fact]
    public void Constructor_FixedValue_ResolvesToThatValue()
    {
        var option = new ColorDepthOption(ColorDepth.Depth24Bit);
        var output = new DummyOutput();

        var result = option.Resolve(output);

        Assert.Equal(ColorDepth.Depth24Bit, result);
    }

    [Theory]
    [InlineData(ColorDepth.Depth1Bit)]
    [InlineData(ColorDepth.Depth4Bit)]
    [InlineData(ColorDepth.Depth8Bit)]
    [InlineData(ColorDepth.Depth24Bit)]
    public void Constructor_FixedValue_IgnoresOutputDefault(ColorDepth depth)
    {
        var option = new ColorDepthOption(depth);
        var output = new DummyOutput(); // defaults to Depth1Bit

        Assert.Equal(depth, option.Resolve(output));
    }

    #endregion

    #region Factory Tests

    [Fact]
    public void Constructor_Factory_ResolvesToFactoryResult()
    {
        var option = new ColorDepthOption(() => ColorDepth.Depth8Bit);
        var output = new DummyOutput();

        Assert.Equal(ColorDepth.Depth8Bit, option.Resolve(output));
    }

    [Fact]
    public void Constructor_Factory_ReturnsNull_FallsBackToOutputDefault()
    {
        var option = new ColorDepthOption(() => null);
        var output = new DummyOutput(); // defaults to Depth1Bit

        Assert.Equal(ColorDepth.Depth1Bit, option.Resolve(output));
    }

    [Fact]
    public void Constructor_NullFactory_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ColorDepthOption((Func<ColorDepth?>)null!));
    }

    #endregion

    #region Default (Auto-detect) Tests

    [Fact]
    public void Default_ResolvesToOutputDefault()
    {
        var option = default(ColorDepthOption);
        var output = new DummyOutput(); // defaults to Depth1Bit

        Assert.Equal(ColorDepth.Depth1Bit, option.Resolve(output));
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversion_FromColorDepth_ResolvesToValue()
    {
        ColorDepthOption option = ColorDepth.Depth24Bit;
        var output = new DummyOutput();

        Assert.Equal(ColorDepth.Depth24Bit, option.Resolve(output));
    }

    [Fact]
    public void ImplicitConversion_FromNullableColorDepth_WithValue_ResolvesToValue()
    {
        ColorDepth? nullable = ColorDepth.Depth8Bit;
        ColorDepthOption option = nullable;
        var output = new DummyOutput();

        Assert.Equal(ColorDepth.Depth8Bit, option.Resolve(output));
    }

    [Fact]
    public void ImplicitConversion_FromNullableColorDepth_Null_ResolvesToOutputDefault()
    {
        ColorDepth? nullable = null;
        ColorDepthOption option = nullable;
        var output = new DummyOutput();

        Assert.Equal(ColorDepth.Depth1Bit, option.Resolve(output));
    }

    [Fact]
    public void ImplicitConversion_FromFunc_ResolvesToFactoryResult()
    {
        Func<ColorDepth?> factory = () => ColorDepth.Depth4Bit;
        ColorDepthOption option = factory;
        var output = new DummyOutput();

        Assert.Equal(ColorDepth.Depth4Bit, option.Resolve(output));
    }

    #endregion

    #region Resolve Null Output Tests

    [Fact]
    public void Resolve_NullOutput_ThrowsArgumentNullException()
    {
        var option = new ColorDepthOption(ColorDepth.Depth24Bit);

        Assert.Throws<ArgumentNullException>(() => option.Resolve(null!));
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_SameFixedValue_ReturnsTrue()
    {
        var a = new ColorDepthOption(ColorDepth.Depth24Bit);
        var b = new ColorDepthOption(ColorDepth.Depth24Bit);

        Assert.True(a.Equals(b));
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void Equals_DifferentFixedValues_ReturnsFalse()
    {
        var a = new ColorDepthOption(ColorDepth.Depth24Bit);
        var b = new ColorDepthOption(ColorDepth.Depth4Bit);

        Assert.False(a.Equals(b));
        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void Equals_SameFactoryReference_ReturnsTrue()
    {
        Func<ColorDepth?> factory = () => ColorDepth.Depth8Bit;
        var a = new ColorDepthOption(factory);
        var b = new ColorDepthOption(factory);

        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_DifferentFactories_ReturnsFalse()
    {
        var a = new ColorDepthOption(() => ColorDepth.Depth8Bit);
        var b = new ColorDepthOption(() => ColorDepth.Depth8Bit);

        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Equals_DefaultValues_ReturnsTrue()
    {
        var a = default(ColorDepthOption);
        var b = default(ColorDepthOption);

        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_BoxedObject_Works()
    {
        var a = new ColorDepthOption(ColorDepth.Depth24Bit);
        object b = new ColorDepthOption(ColorDepth.Depth24Bit);

        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_NonColorDepthOption_ReturnsFalse()
    {
        var a = new ColorDepthOption(ColorDepth.Depth24Bit);

        Assert.False(a.Equals("not a ColorDepthOption"));
    }

    [Fact]
    public void GetHashCode_SameValues_ReturnsSameHash()
    {
        var a = new ColorDepthOption(ColorDepth.Depth24Bit);
        var b = new ColorDepthOption(ColorDepth.Depth24Bit);

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_FixedValue_ContainsValue()
    {
        var option = new ColorDepthOption(ColorDepth.Depth24Bit);

        Assert.Equal("ColorDepthOption(Depth24Bit)", option.ToString());
    }

    [Fact]
    public void ToString_Factory_ReturnsFunc()
    {
        var option = new ColorDepthOption(() => ColorDepth.Depth8Bit);

        Assert.Equal("ColorDepthOption(Func)", option.ToString());
    }

    [Fact]
    public void ToString_Default_ReturnsAuto()
    {
        var option = default(ColorDepthOption);

        Assert.Equal("ColorDepthOption(auto)", option.ToString());
    }

    #endregion
}
