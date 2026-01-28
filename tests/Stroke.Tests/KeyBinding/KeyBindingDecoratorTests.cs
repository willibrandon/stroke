using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;
using Xunit;

// Use explicit alias to avoid ambiguity with Stroke.Input.KeyPress
using KeyPress = Stroke.KeyBinding.KeyPress;

namespace Stroke.Tests.KeyBinding;

/// <summary>
/// Tests for the <see cref="KeyBindingDecorator"/> class.
/// </summary>
public sealed class KeyBindingDecoratorTests
{
    #region Test Helpers

    private static NotImplementedOrNone? TestHandler(KeyPressEvent e) => null;

    #endregion

    #region Create Method

    [Fact]
    public void Create_WithDefaults_ReturnsDecorator()
    {
        var decorator = KeyBindingDecorator.Create();

        Assert.NotNull(decorator);
    }

    [Fact]
    public void Create_WithDefaults_DecoratorReturnsBinding()
    {
        var decorator = KeyBindingDecorator.Create();

        var binding = decorator(TestHandler);

        Assert.NotNull(binding);
        Assert.Equal((KeyHandlerCallable)TestHandler, binding.Handler);
    }

    [Fact]
    public void Create_WithFilter_BindingHasFilter()
    {
        var filter = new Condition(() => true);
        var decorator = KeyBindingDecorator.Create(filter: filter);

        var binding = decorator(TestHandler);

        // Filter should be the same instance (not composed with default)
        Assert.Same(filter, binding.Filter);
    }

    [Fact]
    public void Create_WithFilterTrue_BindingHasFilterAlways()
    {
        var decorator = KeyBindingDecorator.Create(filter: true);

        var binding = decorator(TestHandler);

        Assert.True(binding.Filter.Invoke());
        Assert.IsType<Always>(binding.Filter);
    }

    [Fact]
    public void Create_WithFilterFalse_BindingHasFilterNever()
    {
        // FilterOrBool.HasValue allows distinguishing explicit 'false' from struct default
        var decorator = KeyBindingDecorator.Create(filter: false);

        var binding = decorator(TestHandler);

        Assert.False(binding.Filter.Invoke());
        Assert.IsType<Never>(binding.Filter);
    }

    [Fact]
    public void Create_WithEagerTrue_BindingHasEagerAlways()
    {
        var decorator = KeyBindingDecorator.Create(eager: true);

        var binding = decorator(TestHandler);

        Assert.True(binding.Eager.Invoke());
    }

    [Fact]
    public void Create_WithEagerFalse_BindingHasEagerNever()
    {
        // FilterOrBool.HasValue allows distinguishing explicit 'false' from struct default
        var decorator = KeyBindingDecorator.Create(eager: false);

        var binding = decorator(TestHandler);

        Assert.False(binding.Eager.Invoke());
    }

    [Fact]
    public void Create_WithIsGlobalTrue_BindingHasIsGlobalAlways()
    {
        var decorator = KeyBindingDecorator.Create(isGlobal: true);

        var binding = decorator(TestHandler);

        Assert.True(binding.IsGlobal.Invoke());
    }

    [Fact]
    public void Create_WithIsGlobalFalse_BindingHasIsGlobalNever()
    {
        // FilterOrBool.HasValue allows distinguishing explicit 'false' from struct default
        var decorator = KeyBindingDecorator.Create(isGlobal: false);

        var binding = decorator(TestHandler);

        Assert.False(binding.IsGlobal.Invoke());
    }

    [Fact]
    public void Create_WithSaveBefore_BindingHasSaveBefore()
    {
        bool called = false;
        Func<KeyPressEvent, bool> saveBefore = _ =>
        {
            called = true;
            return false;
        };
        var decorator = KeyBindingDecorator.Create(saveBefore: saveBefore);

        var binding = decorator(TestHandler);
        var e = CreateTestEvent();
        bool result = binding.SaveBefore(e);

        Assert.True(called);
        Assert.False(result);
    }

    [Fact]
    public void Create_WithRecordInMacroFalse_BindingHasRecordInMacroNever()
    {
        // FilterOrBool.HasValue allows distinguishing explicit 'false' from struct default
        var decorator = KeyBindingDecorator.Create(recordInMacro: false);

        var binding = decorator(TestHandler);

        Assert.False(binding.RecordInMacro.Invoke());
    }

    [Fact]
    public void Create_WithRecordInMacroNever_BindingHasRecordInMacroNever()
    {
        var decorator = KeyBindingDecorator.Create(recordInMacro: Never.Instance);

        var binding = decorator(TestHandler);

        Assert.False(binding.RecordInMacro.Invoke());
    }

    [Fact]
    public void Create_WithRecordInMacroTrue_BindingHasRecordInMacroAlways()
    {
        var decorator = KeyBindingDecorator.Create(recordInMacro: true);

        var binding = decorator(TestHandler);

        Assert.True(binding.RecordInMacro.Invoke());
    }

    #endregion

    #region Integration with KeyBindings

    [Fact]
    public void DecoratedBinding_AddedToKeyBindings_UsesProvidedKeys()
    {
        var kb = new KeyBindings();
        var decorator = KeyBindingDecorator.Create(isGlobal: true);

        var binding = decorator(TestHandler);
        kb.Add<Binding>([Keys.ControlQ])(binding);

        // The binding in the registry should have the keys from Add, not the placeholder
        var result = kb.GetBindingsForKeys([Keys.ControlQ]);
        Assert.Single(result);
        Assert.Equal(Keys.ControlQ, result[0].Keys[0].Key);
    }

    [Fact]
    public void DecoratedBinding_AddedToKeyBindings_PreservesIsGlobal()
    {
        var kb = new KeyBindings();
        var decorator = KeyBindingDecorator.Create(isGlobal: true);

        var binding = decorator(TestHandler);
        kb.Add<Binding>([Keys.ControlQ])(binding);

        var result = kb.GetBindingsForKeys([Keys.ControlQ]);
        Assert.True(result[0].IsGlobal.Invoke());
    }

    [Fact]
    public void DecoratedBinding_AddedToKeyBindings_PreservesFilter()
    {
        var kb = new KeyBindings();
        var filter = new Condition(() => true);
        var decorator = KeyBindingDecorator.Create(filter: filter);

        var binding = decorator(TestHandler);
        kb.Add<Binding>([Keys.ControlX])(binding);

        var result = kb.GetBindingsForKeys([Keys.ControlX]);
        // Filter is composed with Add's default (Always), so should be same as original
        Assert.True(result[0].Filter.Invoke());
    }

    [Fact]
    public void DecoratedBinding_AddedWithAdditionalFilter_ComposesFilters()
    {
        var kb = new KeyBindings();
        var decoratorFilter = new Condition(() => true);
        var decorator = KeyBindingDecorator.Create(filter: decoratorFilter);

        var binding = decorator(TestHandler);
        var addFilter = new Condition(() => false);
        kb.Add<Binding>([Keys.ControlC], filter: addFilter)(binding);

        var result = kb.GetBindingsForKeys([Keys.ControlC]);
        // Filter should be decoratorFilter AND addFilter = true AND false = false
        Assert.False(result[0].Filter.Invoke());
    }

    [Fact]
    public void DecoratedBinding_AddedWithAdditionalEager_ComposesEager()
    {
        var kb = new KeyBindings();
        var decorator = KeyBindingDecorator.Create(eager: false);

        var binding = decorator(TestHandler);
        kb.Add<Binding>([Keys.ControlA], eager: true)(binding);

        var result = kb.GetBindingsForKeys([Keys.ControlA]);
        // Eager should be OR: false OR true = true
        Assert.True(result[0].Eager.Invoke());
    }

    [Fact]
    public void DecoratedBinding_AddedWithAdditionalIsGlobal_ComposesIsGlobal()
    {
        var kb = new KeyBindings();
        var decorator = KeyBindingDecorator.Create(isGlobal: false);

        var binding = decorator(TestHandler);
        kb.Add<Binding>([Keys.ControlB], isGlobal: true)(binding);

        var result = kb.GetBindingsForKeys([Keys.ControlB]);
        // IsGlobal should be OR: false OR true = true
        Assert.True(result[0].IsGlobal.Invoke());
    }

    #endregion

    #region Multiple Handlers

    [Fact]
    public void Decorator_CanBeReusedForMultipleHandlers()
    {
        var decorator = KeyBindingDecorator.Create(eager: true, isGlobal: true);

        var binding1 = decorator(e => null);
        var binding2 = decorator(e => NotImplementedOrNone.NotImplemented);

        // Both bindings should have the same settings
        Assert.True(binding1.Eager.Invoke());
        Assert.True(binding1.IsGlobal.Invoke());
        Assert.True(binding2.Eager.Invoke());
        Assert.True(binding2.IsGlobal.Invoke());

        // But different handlers
        Assert.NotSame(binding1.Handler, binding2.Handler);
    }

    #endregion

    #region Helpers

    private static KeyPressEvent CreateTestEvent()
    {
        return new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: [new KeyPress(Keys.ControlC)],
            previousKeySequence: [],
            isRepeat: false);
    }

    #endregion
}
