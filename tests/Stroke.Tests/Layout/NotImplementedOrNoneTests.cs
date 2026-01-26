using Stroke.Layout;
using Xunit;

namespace Stroke.Tests.Layout;

/// <summary>
/// Tests for <see cref="NotImplementedOrNone"/> abstract class.
/// </summary>
public class NotImplementedOrNoneTests
{
    [Fact]
    public void NotImplementedOrNone_NotImplemented_IsSingleton()
    {
        var first = NotImplementedOrNone.NotImplemented;
        var second = NotImplementedOrNone.NotImplemented;

        Assert.Same(first, second);
    }

    [Fact]
    public void NotImplementedOrNone_None_IsSingleton()
    {
        var first = NotImplementedOrNone.None;
        var second = NotImplementedOrNone.None;

        Assert.Same(first, second);
    }

    // T038: NotImplemented != None (reference inequality)
    [Fact]
    public void NotImplementedOrNone_NotImplemented_IsNotSameAsNone()
    {
        Assert.NotSame(NotImplementedOrNone.NotImplemented, NotImplementedOrNone.None);
    }

    [Fact]
    public void NotImplementedOrNone_NotImplemented_DoesNotEqualNone()
    {
        Assert.NotEqual(NotImplementedOrNone.NotImplemented, NotImplementedOrNone.None);
    }

    // T039: Detecting NotImplemented using reference equality
    [Fact]
    public void NotImplementedOrNone_NotImplemented_CanBeDetectedWithIs()
    {
        NotImplementedOrNone result = NotImplementedOrNone.NotImplemented;

        // Using ReferenceEquals for explicit reference check
        Assert.True(ReferenceEquals(result, NotImplementedOrNone.NotImplemented));
    }

    [Fact]
    public void NotImplementedOrNone_NotImplemented_CanBeDetectedWithObjectEquals()
    {
        NotImplementedOrNone result = NotImplementedOrNone.NotImplemented;

        Assert.True(result == NotImplementedOrNone.NotImplemented);
    }

    // T040: Detecting None using reference equality
    [Fact]
    public void NotImplementedOrNone_None_CanBeDetectedWithIs()
    {
        NotImplementedOrNone result = NotImplementedOrNone.None;

        Assert.True(ReferenceEquals(result, NotImplementedOrNone.None));
    }

    [Fact]
    public void NotImplementedOrNone_None_CanBeDetectedWithObjectEquals()
    {
        NotImplementedOrNone result = NotImplementedOrNone.None;

        Assert.True(result == NotImplementedOrNone.None);
    }

    [Fact]
    public void NotImplementedOrNone_NotImplemented_IsNotNull()
    {
        Assert.NotNull(NotImplementedOrNone.NotImplemented);
    }

    [Fact]
    public void NotImplementedOrNone_None_IsNotNull()
    {
        Assert.NotNull(NotImplementedOrNone.None);
    }

    [Fact]
    public void NotImplementedOrNone_IsAbstractClass()
    {
        var type = typeof(NotImplementedOrNone);
        Assert.True(type.IsAbstract);
        Assert.True(type.IsClass);
    }

    [Fact]
    public void NotImplementedOrNone_CannotBeSubclassed_Externally()
    {
        // The only non-abstract derived types should be the private nested classes
        // This test verifies the design by checking that constructors are not accessible
        var constructors = typeof(NotImplementedOrNone).GetConstructors(
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        // All constructors should be private
        Assert.All(constructors, c => Assert.True(c.IsPrivate, "Constructor should be private"));
    }

    [Fact]
    public void NotImplementedOrNone_HandlerPattern_WorksCorrectly()
    {
        // Simulate the handler pattern from quickstart.md
        NotImplementedOrNone SimulateHandler(bool handled)
        {
            if (handled)
                return NotImplementedOrNone.None;
            return NotImplementedOrNone.NotImplemented;
        }

        var handledResult = SimulateHandler(handled: true);
        var unhandledResult = SimulateHandler(handled: false);

        Assert.Same(NotImplementedOrNone.None, handledResult);
        Assert.Same(NotImplementedOrNone.NotImplemented, unhandledResult);
    }

    [Fact]
    public void NotImplementedOrNone_CanBeUsedInConditional()
    {
        NotImplementedOrNone result = NotImplementedOrNone.NotImplemented;

        bool shouldBubble = ReferenceEquals(result, NotImplementedOrNone.NotImplemented);

        Assert.True(shouldBubble);
    }

    [Fact]
    public void NotImplementedOrNone_CanDistinguishResults_InEventBubbling()
    {
        // Simulate event bubbling logic
        NotImplementedOrNone result1 = NotImplementedOrNone.NotImplemented;
        NotImplementedOrNone result2 = NotImplementedOrNone.None;

        bool event1ShouldBubble = ReferenceEquals(result1, NotImplementedOrNone.NotImplemented);
        bool event2ShouldBubble = ReferenceEquals(result2, NotImplementedOrNone.NotImplemented);

        Assert.True(event1ShouldBubble);
        Assert.False(event2ShouldBubble);
    }
}
