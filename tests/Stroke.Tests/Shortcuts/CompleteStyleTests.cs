using Stroke.Shortcuts;
using Xunit;

namespace Stroke.Tests.Shortcuts;

/// <summary>
/// Tests for the <see cref="CompleteStyle"/> enum.
/// </summary>
public sealed class CompleteStyleTests
{
    #region Enum Value Tests

    [Fact]
    public void Column_IsDefined()
    {
        Assert.True(Enum.IsDefined(typeof(CompleteStyle), CompleteStyle.Column));
    }

    [Fact]
    public void MultiColumn_IsDefined()
    {
        Assert.True(Enum.IsDefined(typeof(CompleteStyle), CompleteStyle.MultiColumn));
    }

    [Fact]
    public void ReadlineLike_IsDefined()
    {
        Assert.True(Enum.IsDefined(typeof(CompleteStyle), CompleteStyle.ReadlineLike));
    }

    [Fact]
    public void EnumValues_HasExactlyThreeMembers()
    {
        var values = Enum.GetValues<CompleteStyle>();
        Assert.Equal(3, values.Length);
    }

    [Fact]
    public void EnumValues_AreDistinct()
    {
        var values = Enum.GetValues<CompleteStyle>();
        Assert.Equal(values.Length, values.Distinct().Count());
    }

    #endregion

    #region Default Value Tests

    [Fact]
    public void Default_IsColumn()
    {
        // Column should be the first/default enum value (0)
        Assert.Equal(CompleteStyle.Column, default(CompleteStyle));
    }

    #endregion
}
