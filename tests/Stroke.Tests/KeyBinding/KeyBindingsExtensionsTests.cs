using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;
using Xunit;

namespace Stroke.Tests.KeyBinding;

/// <summary>
/// Tests for the <see cref="KeyBindingsExtensions"/> class.
/// </summary>
public sealed class KeyBindingsExtensionsTests
{
    #region Test Helpers

    private static NotImplementedOrNone? TestHandler(KeyPressEvent e) => null;

    #endregion

    #region Merge Extension (FR-040)

    [Fact]
    public void Merge_TwoRegistries_ReturnsMergedKeyBindings()
    {
        var kb1 = new KeyBindings();
        var kb2 = new KeyBindings();

        var merged = kb1.Merge(kb2);

        Assert.IsType<MergedKeyBindings>(merged);
    }

    [Fact]
    public void Merge_CombinesBindings()
    {
        var kb1 = new KeyBindings();
        kb1.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);

        var kb2 = new KeyBindings();
        kb2.Add<KeyHandlerCallable>([Keys.ControlX])(TestHandler);

        var merged = kb1.Merge(kb2);

        Assert.Equal(2, merged.Bindings.Count);
    }

    [Fact]
    public void Merge_WithNullThis_ThrowsArgumentNullException()
    {
        IKeyBindingsBase kb1 = null!;
        var kb2 = new KeyBindings();

        Assert.Throws<ArgumentNullException>(() => kb1.Merge(kb2));
    }

    [Fact]
    public void Merge_WithNullOther_ThrowsArgumentNullException()
    {
        var kb1 = new KeyBindings();

        Assert.Throws<ArgumentNullException>(() => kb1.Merge(null!));
    }

    #endregion

    #region WithFilter Extension

    [Fact]
    public void WithFilter_ReturnsConditionalKeyBindings()
    {
        var kb = new KeyBindings();
        var filter = new Condition(() => true);

        var conditional = kb.WithFilter(filter);

        Assert.IsType<ConditionalKeyBindings>(conditional);
    }

    [Fact]
    public void WithFilter_AppliesFilterToBindings()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);

        var conditional = kb.WithFilter(new Condition(() => false));

        Assert.Single(conditional.Bindings);
        Assert.False(conditional.Bindings[0].Filter.Invoke());
    }

    [Fact]
    public void WithFilter_WithNullThis_ThrowsArgumentNullException()
    {
        IKeyBindingsBase kb = null!;
        var filter = new Condition(() => true);

        Assert.Throws<ArgumentNullException>(() => kb.WithFilter(filter));
    }

    [Fact]
    public void WithFilter_WithNullFilter_ThrowsArgumentNullException()
    {
        var kb = new KeyBindings();

        Assert.Throws<ArgumentNullException>(() => kb.WithFilter(null!));
    }

    #endregion

    #region GlobalOnly Extension

    [Fact]
    public void GlobalOnly_ReturnsGlobalOnlyKeyBindings()
    {
        var kb = new KeyBindings();

        var globalOnly = kb.GlobalOnly();

        Assert.IsType<GlobalOnlyKeyBindings>(globalOnly);
    }

    [Fact]
    public void GlobalOnly_FiltersToGlobalBindings()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC], isGlobal: true)(TestHandler);
        kb.Add<KeyHandlerCallable>([Keys.ControlX], isGlobal: false)(TestHandler);

        var globalOnly = kb.GlobalOnly();

        Assert.Single(globalOnly.Bindings);
        Assert.Equal(Keys.ControlC, globalOnly.Bindings[0].Keys[0].Key);
    }

    [Fact]
    public void GlobalOnly_WithNullThis_ThrowsArgumentNullException()
    {
        IKeyBindingsBase kb = null!;

        Assert.Throws<ArgumentNullException>(() => kb.GlobalOnly());
    }

    #endregion

    #region Chaining

    [Fact]
    public void Extensions_CanBeChained()
    {
        var kb1 = new KeyBindings();
        kb1.Add<KeyHandlerCallable>([Keys.ControlC], isGlobal: true)(TestHandler);

        var kb2 = new KeyBindings();
        kb2.Add<KeyHandlerCallable>([Keys.ControlX], isGlobal: true)(TestHandler);

        var result = kb1
            .Merge(kb2)
            .WithFilter(Always.Instance)
            .GlobalOnly();

        Assert.Equal(2, result.Bindings.Count);
    }

    #endregion
}
