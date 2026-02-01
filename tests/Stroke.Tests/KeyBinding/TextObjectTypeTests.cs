using Stroke.KeyBinding;
using Xunit;

namespace Stroke.Tests.KeyBinding;

public class TextObjectTypeTests
{
    [Fact]
    public void HasExclusiveValue()
    {
        Assert.Equal(0, (int)TextObjectType.Exclusive);
    }

    [Fact]
    public void HasInclusiveValue()
    {
        Assert.Equal(1, (int)TextObjectType.Inclusive);
    }

    [Fact]
    public void HasLinewiseValue()
    {
        Assert.Equal(2, (int)TextObjectType.Linewise);
    }

    [Fact]
    public void HasBlockValue()
    {
        Assert.Equal(3, (int)TextObjectType.Block);
    }

    [Fact]
    public void HasExactlyFourValues()
    {
        var values = Enum.GetValues<TextObjectType>();
        Assert.Equal(4, values.Length);
    }

    [Theory]
    [InlineData(TextObjectType.Exclusive)]
    [InlineData(TextObjectType.Inclusive)]
    [InlineData(TextObjectType.Linewise)]
    [InlineData(TextObjectType.Block)]
    public void AllValuesAreDefined(TextObjectType type)
    {
        Assert.True(Enum.IsDefined(type));
    }
}
