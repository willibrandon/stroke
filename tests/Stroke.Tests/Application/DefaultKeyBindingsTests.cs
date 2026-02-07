using Stroke.Application;
using Stroke.Input;
using Stroke.KeyBinding;
using Xunit;

namespace Stroke.Tests.Application;

/// <summary>
/// Tests for <see cref="DefaultKeyBindings"/>.
/// </summary>
public sealed class DefaultKeyBindingsTests
{
    [Fact]
    public void Load_ReturnsNonNullBindings()
    {
        var bindings = DefaultKeyBindings.Load();

        Assert.NotNull(bindings);
    }

    [Fact]
    public void Load_ReturnsMergedKeyBindings()
    {
        var bindings = DefaultKeyBindings.Load();

        Assert.IsType<MergedKeyBindings>(bindings);
    }

    [Fact]
    public void LoadPageNavigation_ReturnsNonNullBindings()
    {
        var bindings = DefaultKeyBindings.LoadPageNavigation();

        Assert.NotNull(bindings);
    }

    [Fact]
    public void Load_IncludesBindingsForCommonKeys()
    {
        var bindings = DefaultKeyBindings.Load();

        // Ctrl+M (Enter) should have bindings in every configuration
        var enterBindings = bindings.GetBindingsForKeys([new KeyOrChar(Keys.ControlM)]);
        Assert.True(enterBindings.Count > 0, "Should have bindings for Enter/Ctrl+M key");
    }

    [Fact]
    public void Load_CalledTwice_ReturnsDifferentInstances()
    {
        var bindings1 = DefaultKeyBindings.Load();
        var bindings2 = DefaultKeyBindings.Load();

        Assert.NotSame(bindings1, bindings2);
    }
}
