using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;
using Xunit;

namespace Stroke.Tests.KeyBinding;

/// <summary>
/// Tests for the <see cref="ConditionalKeyBindings"/> class.
/// </summary>
public sealed class ConditionalKeyBindingsTests
{
    #region Test Helpers

    private static NotImplementedOrNone? TestHandler(KeyPressEvent e) => null;

    #endregion

    #region Construction

    [Fact]
    public void Constructor_WithValidArgs_CreatesWrapper()
    {
        var kb = new KeyBindings();
        var filter = new Condition(() => true);

        var ckb = new ConditionalKeyBindings(kb, filter);

        Assert.Same(filter, ckb.Filter);
    }

    [Fact]
    public void Constructor_WithNullKeyBindings_ThrowsArgumentNullException()
    {
        var filter = new Condition(() => true);

        var ex = Assert.Throws<ArgumentNullException>(() => new ConditionalKeyBindings(null!, filter));
        Assert.Equal("keyBindings", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithNullFilter_ThrowsArgumentNullException()
    {
        var kb = new KeyBindings();

        var ex = Assert.Throws<ArgumentNullException>(() => new ConditionalKeyBindings(kb, null!));
        Assert.Equal("filter", ex.ParamName);
    }

    #endregion

    #region Filter Composition (T027 per FR-026, FR-056)

    [Fact]
    public void Bindings_WhenWrapperFilterFalse_AllBindingsInactive()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);

        // Wrapper filter returns false - all bindings should be inactive
        var ckb = new ConditionalKeyBindings(kb, new Condition(() => false));

        Assert.Single(ckb.Bindings);
        Assert.False(ckb.Bindings[0].Filter.Invoke());
    }

    [Fact]
    public void Bindings_WhenWrapperFilterTrue_BindingsActiveAsOriginal()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC], filter: new Condition(() => true))(TestHandler);

        // Wrapper filter returns true - bindings should maintain original state
        var ckb = new ConditionalKeyBindings(kb, new Condition(() => true));

        Assert.Single(ckb.Bindings);
        Assert.True(ckb.Bindings[0].Filter.Invoke());
    }

    [Fact]
    public void Bindings_FilterComposition_UsesAND()
    {
        var kb = new KeyBindings();
        var bindingFilter = new Condition(() => true);
        kb.Add<KeyHandlerCallable>([Keys.ControlC], filter: bindingFilter)(TestHandler);

        // Wrapper filter = true, binding filter = true → composed = true
        var ckb1 = new ConditionalKeyBindings(kb, new Condition(() => true));
        Assert.True(ckb1.Bindings[0].Filter.Invoke());

        // Wrapper filter = false, binding filter = true → composed = false (AND)
        var ckb2 = new ConditionalKeyBindings(kb, new Condition(() => false));
        Assert.False(ckb2.Bindings[0].Filter.Invoke());
    }

    [Fact]
    public void Bindings_DynamicFilterEvaluation()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);

        bool wrapperResult = true;
        var ckb = new ConditionalKeyBindings(kb, new Condition(() => wrapperResult));

        Assert.True(ckb.Bindings[0].Filter.Invoke());

        wrapperResult = false;
        Assert.False(ckb.Bindings[0].Filter.Invoke());
    }

    #endregion

    #region IKeyBindingsBase Implementation

    [Fact]
    public void Version_ReturnsBaseVersion()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);

        var ckb = new ConditionalKeyBindings(kb, Always.Instance);
        var v1 = ckb.Version;

        kb.Add<KeyHandlerCallable>([Keys.ControlX])(TestHandler);
        var v2 = ckb.Version;

        Assert.NotEqual(v1, v2);
    }

    [Fact]
    public void Bindings_ReflectsBaseBindings()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);

        var ckb = new ConditionalKeyBindings(kb, Always.Instance);

        Assert.Single(ckb.Bindings);
        Assert.Equal(Keys.ControlC, ckb.Bindings[0].Keys[0].Key);
    }

    [Fact]
    public void GetBindingsForKeys_ReturnsFilteredBindings()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);

        var ckb = new ConditionalKeyBindings(kb, Always.Instance);
        var result = ckb.GetBindingsForKeys([Keys.ControlC]);

        Assert.Single(result);
    }

    [Fact]
    public void GetBindingsStartingWithKeys_ReturnsFilteredBindings()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlX, Keys.ControlC])(TestHandler);

        var ckb = new ConditionalKeyBindings(kb, Always.Instance);
        var result = ckb.GetBindingsStartingWithKeys([Keys.ControlX]);

        Assert.Single(result);
    }

    #endregion

    #region Cache Invalidation

    [Fact]
    public void Bindings_WhenBaseChanges_UpdatesCache()
    {
        var kb = new KeyBindings();
        var ckb = new ConditionalKeyBindings(kb, Always.Instance);

        Assert.Empty(ckb.Bindings);

        kb.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);

        Assert.Single(ckb.Bindings);
    }

    #endregion

    #region Preserves Other Properties

    [Fact]
    public void Bindings_PreservesHandler()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);

        var ckb = new ConditionalKeyBindings(kb, Always.Instance);

        Assert.Equal((KeyHandlerCallable)TestHandler, ckb.Bindings[0].Handler);
    }

    [Fact]
    public void Bindings_PreservesKeys()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlX, Keys.ControlC])(TestHandler);

        var ckb = new ConditionalKeyBindings(kb, Always.Instance);

        Assert.Equal(2, ckb.Bindings[0].Keys.Count);
        Assert.Equal(Keys.ControlX, ckb.Bindings[0].Keys[0].Key);
        Assert.Equal(Keys.ControlC, ckb.Bindings[0].Keys[1].Key);
    }

    [Fact]
    public void Bindings_PreservesEager()
    {
        var kb = new KeyBindings();
        var eagerFilter = new Condition(() => true);
        kb.Add<KeyHandlerCallable>([Keys.ControlC], eager: eagerFilter)(TestHandler);

        var ckb = new ConditionalKeyBindings(kb, Always.Instance);

        Assert.True(ckb.Bindings[0].Eager.Invoke());
    }

    [Fact]
    public void Bindings_PreservesIsGlobal()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC], isGlobal: true)(TestHandler);

        var ckb = new ConditionalKeyBindings(kb, Always.Instance);

        Assert.True(ckb.Bindings[0].IsGlobal.Invoke());
    }

    [Fact]
    public void Bindings_PreservesRecordInMacro()
    {
        var kb = new KeyBindings();
        var recordFilter = new Condition(() => true);
        kb.Add<KeyHandlerCallable>([Keys.ControlC], recordInMacro: recordFilter)(TestHandler);

        var ckb = new ConditionalKeyBindings(kb, Always.Instance);

        Assert.True(ckb.Bindings[0].RecordInMacro.Invoke());
    }

    #endregion
}
