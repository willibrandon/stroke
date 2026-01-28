using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;
using Xunit;

namespace Stroke.Tests.KeyBinding;

/// <summary>
/// Tests for the <see cref="DynamicKeyBindings"/> class.
/// </summary>
public sealed class DynamicKeyBindingsTests
{
    #region Test Helpers

    private static NotImplementedOrNone? TestHandler(KeyPressEvent e) => null;
    private static NotImplementedOrNone? Handler1(KeyPressEvent e) => null;
    private static NotImplementedOrNone? Handler2(KeyPressEvent e) => null;

    #endregion

    #region Construction

    [Fact]
    public void Constructor_WithValidCallable_CreatesDynamic()
    {
        var kb = new KeyBindings();
        var dkb = new DynamicKeyBindings(() => kb);

        Assert.NotNull(dkb);
        Assert.NotNull(dkb.GetKeyBindings);
    }

    [Fact]
    public void Constructor_WithNullCallable_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new DynamicKeyBindings(null!));
        Assert.Equal("getKeyBindings", ex.ParamName);
    }

    #endregion

    #region T047: Dynamic returns registry bindings

    [Fact]
    public void Bindings_ReturnsCallableResultBindings()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);

        var dkb = new DynamicKeyBindings(() => kb);

        Assert.Single(dkb.Bindings);
        Assert.Equal(Keys.ControlC, dkb.Bindings[0].Keys[0].Key);
    }

    [Fact]
    public void GetBindingsForKeys_ReturnsCallableResultBindings()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);

        var dkb = new DynamicKeyBindings(() => kb);
        var result = dkb.GetBindingsForKeys([Keys.ControlC]);

        Assert.Single(result);
    }

    #endregion

    #region T048: Dynamic switches registry

    [Fact]
    public void Bindings_SwitchesRegistry_ReturnsNewBindings()
    {
        var kbA = new KeyBindings();
        kbA.Add<KeyHandlerCallable>([Keys.ControlC])(Handler1);

        var kbB = new KeyBindings();
        kbB.Add<KeyHandlerCallable>([Keys.ControlX])(Handler2);

        IKeyBindingsBase current = kbA;
        var dkb = new DynamicKeyBindings(() => current);

        Assert.Single(dkb.Bindings);
        Assert.Equal(Keys.ControlC, dkb.Bindings[0].Keys[0].Key);

        current = kbB;

        Assert.Single(dkb.Bindings);
        Assert.Equal(Keys.ControlX, dkb.Bindings[0].Keys[0].Key);
    }

    [Fact]
    public void Version_ChangesWhenRegistrySwitches()
    {
        var kbA = new KeyBindings();
        kbA.Add<KeyHandlerCallable>([Keys.ControlA])(Handler1); // Make versions different

        var kbB = new KeyBindings();

        IKeyBindingsBase current = kbA;
        var dkb = new DynamicKeyBindings(() => current);

        var v1 = dkb.Version;

        current = kbB;

        var v2 = dkb.Version;

        // Version should change because the registry instance changed
        Assert.NotEqual(v1, v2);
    }

    #endregion

    #region T049: Dynamic null returns empty

    [Fact]
    public void Bindings_WhenCallableReturnsNull_ReturnsEmpty()
    {
        var dkb = new DynamicKeyBindings(() => null);

        Assert.Empty(dkb.Bindings);
    }

    [Fact]
    public void Bindings_WhenCallableReturnsNull_IsNotNull()
    {
        var dkb = new DynamicKeyBindings(() => null);

        Assert.NotNull(dkb.Bindings);
    }

    [Fact]
    public void GetBindingsForKeys_WhenCallableReturnsNull_ReturnsEmpty()
    {
        var dkb = new DynamicKeyBindings(() => null);

        var result = dkb.GetBindingsForKeys([Keys.ControlC]);

        Assert.Empty(result);
        Assert.NotNull(result);
    }

    [Fact]
    public void Version_WhenCallableReturnsNull_IsStable()
    {
        var dkb = new DynamicKeyBindings(() => null);

        var v1 = dkb.Version;
        var v2 = dkb.Version;

        Assert.Equal(v1, v2);
    }

    #endregion

    #region Responds to Underlying Changes

    [Fact]
    public void Bindings_UpdatesWhenUnderlyingChanges()
    {
        var kb = new KeyBindings();
        var dkb = new DynamicKeyBindings(() => kb);

        Assert.Empty(dkb.Bindings);

        kb.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);

        Assert.Single(dkb.Bindings);
    }

    [Fact]
    public void Version_ChangesWhenUnderlyingChanges()
    {
        var kb = new KeyBindings();
        var dkb = new DynamicKeyBindings(() => kb);

        var v1 = dkb.Version;

        kb.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);

        var v2 = dkb.Version;

        Assert.NotEqual(v1, v2);
    }

    #endregion

    #region GetBindingsStartingWithKeys

    [Fact]
    public void GetBindingsStartingWithKeys_ReturnsFromCallableResult()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlX, Keys.ControlC])(TestHandler);

        var dkb = new DynamicKeyBindings(() => kb);
        var result = dkb.GetBindingsStartingWithKeys([Keys.ControlX]);

        Assert.Single(result);
    }

    [Fact]
    public void GetBindingsStartingWithKeys_WhenNull_ReturnsEmpty()
    {
        var dkb = new DynamicKeyBindings(() => null);
        var result = dkb.GetBindingsStartingWithKeys([Keys.ControlX]);

        Assert.Empty(result);
        Assert.NotNull(result);
    }

    #endregion

    #region Property Preservation

    [Fact]
    public void Bindings_PreservesFilterProperty()
    {
        var kb = new KeyBindings();
        var filter = new Condition(() => true);
        kb.Add<KeyHandlerCallable>([Keys.ControlC], filter: filter)(TestHandler);

        var dkb = new DynamicKeyBindings(() => kb);

        Assert.True(dkb.Bindings[0].Filter.Invoke());
    }

    [Fact]
    public void Bindings_PreservesEagerProperty()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC], eager: true)(TestHandler);

        var dkb = new DynamicKeyBindings(() => kb);

        Assert.True(dkb.Bindings[0].Eager.Invoke());
    }

    #endregion
}
