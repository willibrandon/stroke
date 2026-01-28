namespace Stroke.Tests.Core;

using Stroke.Core;
using Xunit;

/// <summary>
/// Tests for <see cref="DummyContext"/> class.
/// </summary>
public class DummyContextTests
{
    [Fact]
    public void Instance_ReturnsSingleton()
    {
        var instance1 = DummyContext.Instance;
        var instance2 = DummyContext.Instance;

        Assert.NotNull(instance1);
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void Dispose_CompletesWithoutError()
    {
        var context = DummyContext.Instance;

        var exception = Record.Exception(() => context.Dispose());

        Assert.Null(exception);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var context = DummyContext.Instance;

        // Calling Dispose multiple times should not throw
        var exception = Record.Exception(() =>
        {
            context.Dispose();
            context.Dispose();
            context.Dispose();
        });

        Assert.Null(exception);
    }

    [Fact]
    public void CanBeUsedInUsingStatement()
    {
        // Using statement should work without issues
        var exception = Record.Exception(() =>
        {
            using (DummyContext.Instance)
            {
                // Do some work
                _ = 1 + 1;
            }
        });

        Assert.Null(exception);
    }

    [Fact]
    public void CanBeUsedInUsingDeclaration()
    {
        // Using declaration (C# 8+) should work
        var exception = Record.Exception(() =>
        {
            using var context = DummyContext.Instance;
            // Do some work
            _ = 1 + 1;
        });

        Assert.Null(exception);
    }

    [Fact]
    public void Instance_IsNotNull()
    {
        Assert.NotNull(DummyContext.Instance);
    }

    [Fact]
    public void ImplementsIDisposable()
    {
        // Verify DummyContext implements IDisposable
        Assert.IsAssignableFrom<IDisposable>(DummyContext.Instance);
    }
}
