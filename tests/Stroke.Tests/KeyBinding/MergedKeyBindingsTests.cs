using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;
using Xunit;

namespace Stroke.Tests.KeyBinding;

/// <summary>
/// Tests for the <see cref="MergedKeyBindings"/> class.
/// </summary>
public sealed class MergedKeyBindingsTests
{
    #region Test Helpers

    private static NotImplementedOrNone? Handler1(KeyPressEvent e) => null;
    private static NotImplementedOrNone? Handler2(KeyPressEvent e) => null;
    private static NotImplementedOrNone? Handler3(KeyPressEvent e) => null;

    #endregion

    #region Construction

    [Fact]
    public void Constructor_WithValidRegistries_CreatesMerged()
    {
        var kb1 = new KeyBindings();
        var kb2 = new KeyBindings();

        var merged = new MergedKeyBindings(kb1, kb2);

        Assert.Equal(2, merged.Registries.Count);
    }

    [Fact]
    public void Constructor_WithEnumerable_CreatesMerged()
    {
        var registries = new List<IKeyBindingsBase>
        {
            new KeyBindings(),
            new KeyBindings()
        };

        var merged = new MergedKeyBindings(registries);

        Assert.Equal(2, merged.Registries.Count);
    }

    [Fact]
    public void Constructor_WithNullRegistries_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new MergedKeyBindings((IEnumerable<IKeyBindingsBase>)null!));
        Assert.Equal("registries", ex.ParamName);
    }

    #endregion

    #region T035: Merge non-overlapping keys

    [Fact]
    public void Bindings_MergesNonOverlappingKeys()
    {
        var kb1 = new KeyBindings();
        kb1.Add<KeyHandlerCallable>([Keys.ControlC])(Handler1);

        var kb2 = new KeyBindings();
        kb2.Add<KeyHandlerCallable>([Keys.ControlX])(Handler2);

        var merged = new MergedKeyBindings(kb1, kb2);

        Assert.Equal(2, merged.Bindings.Count);
    }

    [Fact]
    public void GetBindingsForKeys_FindsFromEitherRegistry()
    {
        var kb1 = new KeyBindings();
        kb1.Add<KeyHandlerCallable>([Keys.ControlC])(Handler1);

        var kb2 = new KeyBindings();
        kb2.Add<KeyHandlerCallable>([Keys.ControlX])(Handler2);

        var merged = new MergedKeyBindings(kb1, kb2);

        var result1 = merged.GetBindingsForKeys([Keys.ControlC]);
        var result2 = merged.GetBindingsForKeys([Keys.ControlX]);

        Assert.Single(result1);
        Assert.Single(result2);
    }

    #endregion

    #region T036: Merge overlapping keys

    [Fact]
    public void Bindings_MergesOverlappingKeys_BothIncluded()
    {
        var kb1 = new KeyBindings();
        kb1.Add<KeyHandlerCallable>([Keys.ControlC])(Handler1);

        var kb2 = new KeyBindings();
        kb2.Add<KeyHandlerCallable>([Keys.ControlC])(Handler2);

        var merged = new MergedKeyBindings(kb1, kb2);

        Assert.Equal(2, merged.Bindings.Count);
    }

    [Fact]
    public void GetBindingsForKeys_OverlappingKeys_ReturnsBothInOrder()
    {
        var kb1 = new KeyBindings();
        kb1.Add<KeyHandlerCallable>([Keys.ControlC])(Handler1);

        var kb2 = new KeyBindings();
        kb2.Add<KeyHandlerCallable>([Keys.ControlC])(Handler2);

        var merged = new MergedKeyBindings(kb1, kb2);
        var result = merged.GetBindingsForKeys([Keys.ControlC]);

        Assert.Equal(2, result.Count);
        // Order preserved: kb1's binding first, then kb2's
        Assert.Equal((KeyHandlerCallable)Handler1, result[0].Handler);
        Assert.Equal((KeyHandlerCallable)Handler2, result[1].Handler);
    }

    #endregion

    #region T037: Merged version tracking

    [Fact]
    public void Version_ChangesWhenOriginalChanges()
    {
        var kb1 = new KeyBindings();
        var kb2 = new KeyBindings();
        var merged = new MergedKeyBindings(kb1, kb2);

        var v1 = merged.Version;

        kb1.Add<KeyHandlerCallable>([Keys.ControlC])(Handler1);

        var v2 = merged.Version;

        Assert.NotEqual(v1, v2);
    }

    [Fact]
    public void Bindings_UpdatesWhenOriginalChanges()
    {
        var kb1 = new KeyBindings();
        var kb2 = new KeyBindings();
        var merged = new MergedKeyBindings(kb1, kb2);

        Assert.Empty(merged.Bindings);

        kb1.Add<KeyHandlerCallable>([Keys.ControlC])(Handler1);

        Assert.Single(merged.Bindings);

        kb2.Add<KeyHandlerCallable>([Keys.ControlX])(Handler2);

        Assert.Equal(2, merged.Bindings.Count);
    }

    #endregion

    #region GetBindingsStartingWithKeys

    [Fact]
    public void GetBindingsStartingWithKeys_ReturnsFromAllRegistries()
    {
        var kb1 = new KeyBindings();
        kb1.Add<KeyHandlerCallable>([Keys.ControlX, Keys.ControlC])(Handler1);

        var kb2 = new KeyBindings();
        kb2.Add<KeyHandlerCallable>([Keys.ControlX, Keys.ControlS])(Handler2);

        var merged = new MergedKeyBindings(kb1, kb2);
        var result = merged.GetBindingsStartingWithKeys([Keys.ControlX]);

        Assert.Equal(2, result.Count);
    }

    #endregion

    #region Preserves Properties

    [Fact]
    public void Bindings_PreservesFilterProperty()
    {
        var kb1 = new KeyBindings();
        var filter = new Condition(() => true);
        kb1.Add<KeyHandlerCallable>([Keys.ControlC], filter: filter)(Handler1);

        var merged = new MergedKeyBindings(kb1);

        Assert.True(merged.Bindings[0].Filter.Invoke());
    }

    [Fact]
    public void Bindings_PreservesEagerProperty()
    {
        var kb1 = new KeyBindings();
        kb1.Add<KeyHandlerCallable>([Keys.ControlC], eager: true)(Handler1);

        var merged = new MergedKeyBindings(kb1);

        Assert.True(merged.Bindings[0].Eager.Invoke());
    }

    [Fact]
    public void Bindings_PreservesIsGlobalProperty()
    {
        var kb1 = new KeyBindings();
        kb1.Add<KeyHandlerCallable>([Keys.ControlC], isGlobal: true)(Handler1);

        var merged = new MergedKeyBindings(kb1);

        Assert.True(merged.Bindings[0].IsGlobal.Invoke());
    }

    #endregion

    #region Empty Registries

    [Fact]
    public void Bindings_EmptyRegistries_ReturnsEmpty()
    {
        var merged = new MergedKeyBindings();

        Assert.Empty(merged.Bindings);
    }

    [Fact]
    public void Bindings_AllEmptyRegistries_ReturnsEmpty()
    {
        var kb1 = new KeyBindings();
        var kb2 = new KeyBindings();

        var merged = new MergedKeyBindings(kb1, kb2);

        Assert.Empty(merged.Bindings);
    }

    #endregion
}
