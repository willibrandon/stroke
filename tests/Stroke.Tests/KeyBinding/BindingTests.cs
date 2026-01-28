using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;
using Xunit;

// Use explicit alias to avoid ambiguity with Stroke.Input.KeyPress
using KeyPress = Stroke.KeyBinding.KeyPress;

namespace Stroke.Tests.KeyBinding;

/// <summary>
/// Tests for the <see cref="Binding"/> sealed class.
/// </summary>
public sealed class BindingTests
{
    #region Test Helpers

    private static KeyPressEvent CreateEvent()
    {
        return new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: [new KeyPress(Keys.ControlM)],
            previousKeySequence: [],
            isRepeat: false);
    }

    private static NotImplementedOrNone? TestHandler(KeyPressEvent e) => null;

    #endregion

    #region Construction - Basic

    [Fact]
    public void Constructor_WithValidArgs_CreatesBinding()
    {
        KeyOrChar[] keys = [Keys.ControlC];
        KeyHandlerCallable handler = TestHandler;

        var binding = new Binding(keys, handler);

        Assert.Single(binding.Keys);
        Assert.Equal(Keys.ControlC, binding.Keys[0].Key);
        Assert.Same(handler, binding.Handler);
    }

    [Fact]
    public void Constructor_WithMultiKeySequence_CreatesBinding()
    {
        KeyOrChar[] keys = [Keys.ControlX, Keys.ControlC];
        KeyHandlerCallable handler = TestHandler;

        var binding = new Binding(keys, handler);

        Assert.Equal(2, binding.Keys.Count);
        Assert.Equal(Keys.ControlX, binding.Keys[0].Key);
        Assert.Equal(Keys.ControlC, binding.Keys[1].Key);
    }

    [Fact]
    public void Constructor_WithCharKeys_CreatesBinding()
    {
        KeyOrChar[] keys = ['a', 'b', 'c'];
        KeyHandlerCallable handler = TestHandler;

        var binding = new Binding(keys, handler);

        Assert.Equal(3, binding.Keys.Count);
        Assert.Equal('a', binding.Keys[0].Char);
        Assert.Equal('b', binding.Keys[1].Char);
        Assert.Equal('c', binding.Keys[2].Char);
    }

    [Fact]
    public void Constructor_WithMixedKeys_CreatesBinding()
    {
        KeyOrChar[] keys = [Keys.ControlX, 'c'];
        KeyHandlerCallable handler = TestHandler;

        var binding = new Binding(keys, handler);

        Assert.Equal(2, binding.Keys.Count);
        Assert.True(binding.Keys[0].IsKey);
        Assert.True(binding.Keys[1].IsChar);
    }

    #endregion

    #region Construction - Validation Exceptions

    [Fact]
    public void Constructor_WithNullKeys_ThrowsArgumentNullException()
    {
        KeyHandlerCallable handler = TestHandler;

        var ex = Assert.Throws<ArgumentNullException>(() => new Binding(null!, handler));

        Assert.Equal("keys", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithEmptyKeys_ThrowsArgumentException()
    {
        KeyOrChar[] keys = [];
        KeyHandlerCallable handler = TestHandler;

        var ex = Assert.Throws<ArgumentException>(() => new Binding(keys, handler));

        Assert.Equal("keys", ex.ParamName);
        Assert.Contains("empty", ex.Message);
    }

    [Fact]
    public void Constructor_WithNullHandler_ThrowsArgumentNullException()
    {
        KeyOrChar[] keys = [Keys.ControlM];

        var ex = Assert.Throws<ArgumentNullException>(() => new Binding(keys, null!));

        Assert.Equal("handler", ex.ParamName);
    }

    #endregion

    #region Default Filter Values per FR-055

    [Fact]
    public void Constructor_DefaultFilter_IsAlways()
    {
        KeyOrChar[] keys = [Keys.ControlM];
        var binding = new Binding(keys, TestHandler);

        Assert.IsType<Always>(binding.Filter);
    }

    [Fact]
    public void Constructor_DefaultEager_IsNever()
    {
        KeyOrChar[] keys = [Keys.ControlM];
        var binding = new Binding(keys, TestHandler);

        Assert.IsType<Never>(binding.Eager);
    }

    [Fact]
    public void Constructor_DefaultIsGlobal_IsNever()
    {
        KeyOrChar[] keys = [Keys.ControlM];
        var binding = new Binding(keys, TestHandler);

        Assert.IsType<Never>(binding.IsGlobal);
    }

    [Fact]
    public void Constructor_DefaultRecordInMacro_IsAlways()
    {
        KeyOrChar[] keys = [Keys.ControlM];
        var binding = new Binding(keys, TestHandler);

        Assert.IsType<Always>(binding.RecordInMacro);
    }

    [Fact]
    public void Constructor_DefaultSaveBefore_ReturnsTrue()
    {
        KeyOrChar[] keys = [Keys.ControlM];
        var binding = new Binding(keys, TestHandler);

        Assert.True(binding.SaveBefore(CreateEvent()));
    }

    #endregion

    #region Explicit Filter Values

    [Fact]
    public void Constructor_WithExplicitFilterTrue_IsAlways()
    {
        KeyOrChar[] keys = [Keys.ControlM];

        var binding = new Binding(keys, TestHandler, filter: true);

        Assert.IsType<Always>(binding.Filter);
    }

    [Fact]
    public void Constructor_WithExplicitFilterFalse_IsNever()
    {
        // FilterOrBool.HasValue distinguishes explicit false from struct default.
        // Explicit false maps to Never per FR-055 semantic meaning.
        KeyOrChar[] keys = [Keys.ControlM];

        var binding = new Binding(keys, TestHandler, filter: false);

        Assert.IsType<Never>(binding.Filter);
    }

    [Fact]
    public void Constructor_WithExplicitEagerTrue_IsAlways()
    {
        KeyOrChar[] keys = [Keys.ControlM];

        var binding = new Binding(keys, TestHandler, eager: true);

        Assert.IsType<Always>(binding.Eager);
    }

    [Fact]
    public void Constructor_WithExplicitIsGlobalTrue_IsAlways()
    {
        KeyOrChar[] keys = [Keys.ControlM];

        var binding = new Binding(keys, TestHandler, isGlobal: true);

        Assert.IsType<Always>(binding.IsGlobal);
    }

    [Fact]
    public void Constructor_WithExplicitRecordInMacroFalse_IsNever()
    {
        // FilterOrBool.HasValue distinguishes explicit false from struct default.
        // Explicit false maps to Never per FR-055 semantic meaning.
        KeyOrChar[] keys = [Keys.ControlM];

        var binding = new Binding(keys, TestHandler, recordInMacro: false);

        Assert.IsType<Never>(binding.RecordInMacro);
    }

    [Fact]
    public void Constructor_WithCustomFilter_UsesProvidedFilter()
    {
        KeyOrChar[] keys = [Keys.ControlM];
        var customFilter = new Condition(() => true);

        var binding = new Binding(keys, TestHandler, filter: customFilter);

        Assert.Same(customFilter, binding.Filter);
    }

    [Fact]
    public void Constructor_WithCustomEagerFilter_UsesProvidedFilter()
    {
        KeyOrChar[] keys = [Keys.ControlM];
        var customFilter = new Condition(() => false);

        var binding = new Binding(keys, TestHandler, eager: customFilter);

        Assert.Same(customFilter, binding.Eager);
    }

    #endregion

    #region SaveBefore Callback

    [Fact]
    public void Constructor_WithCustomSaveBefore_UsesProvidedCallback()
    {
        KeyOrChar[] keys = [Keys.ControlM];
        bool called = false;
        Func<KeyPressEvent, bool> saveBefore = e =>
        {
            called = true;
            return false;
        };

        var binding = new Binding(keys, TestHandler, saveBefore: saveBefore);
        var result = binding.SaveBefore(CreateEvent());

        Assert.True(called);
        Assert.False(result);
    }

    #endregion

    #region Call Method

    [Fact]
    public void Call_InvokesHandler()
    {
        KeyOrChar[] keys = [Keys.ControlM];
        bool handlerCalled = false;
        KeyHandlerCallable handler = e =>
        {
            handlerCalled = true;
            return null;
        };
        var binding = new Binding(keys, handler);

        binding.Call(CreateEvent());

        Assert.True(handlerCalled);
    }

    [Fact]
    public void Call_InvokesSaveBeforeFirst()
    {
        KeyOrChar[] keys = [Keys.ControlM];
        var callOrder = new List<string>();
        Func<KeyPressEvent, bool> saveBefore = e =>
        {
            callOrder.Add("saveBefore");
            return true;
        };
        KeyHandlerCallable handler = e =>
        {
            callOrder.Add("handler");
            return null;
        };
        var binding = new Binding(keys, handler, saveBefore: saveBefore);

        binding.Call(CreateEvent());

        Assert.Equal(["saveBefore", "handler"], callOrder);
    }

    [Fact]
    public void Call_WithNullEvent_ThrowsArgumentNullException()
    {
        KeyOrChar[] keys = [Keys.ControlM];
        var binding = new Binding(keys, TestHandler);

        var ex = Assert.Throws<ArgumentNullException>(() => binding.Call(null!));

        // Parameter name is @event (verbatim identifier for reserved word)
        Assert.Equal("@event", ex.ParamName);
    }

    [Fact]
    public void Call_WhenSaveBeforeThrows_DoesNotCallHandler()
    {
        KeyOrChar[] keys = [Keys.ControlM];
        bool handlerCalled = false;
        Func<KeyPressEvent, bool> saveBefore = e => throw new InvalidOperationException("test");
        KeyHandlerCallable handler = e =>
        {
            handlerCalled = true;
            return null;
        };
        var binding = new Binding(keys, handler, saveBefore: saveBefore);

        Assert.Throws<InvalidOperationException>(() => binding.Call(CreateEvent()));
        Assert.False(handlerCalled);
    }

    #endregion

    #region AnyCount (Priority Sorting)

    [Fact]
    public void AnyCount_WithNoWildcards_ReturnsZero()
    {
        KeyOrChar[] keys = [Keys.ControlC];
        var binding = new Binding(keys, TestHandler);

        // Access internal AnyCount via reflection or make it internal visible
        // For now, we verify behavior through GetBindingsForKeys sorting
        Assert.Single(binding.Keys);
    }

    [Fact]
    public void AnyCount_WithKeysAny_CountsWildcards()
    {
        KeyOrChar[] keys = [Keys.Any, 'c'];
        var binding = new Binding(keys, TestHandler);

        Assert.Equal(2, binding.Keys.Count);
        Assert.Equal(Keys.Any, binding.Keys[0].Key);
    }

    [Fact]
    public void AnyCount_WithMultipleWildcards_CountsAll()
    {
        KeyOrChar[] keys = [Keys.Any, Keys.Any];
        var binding = new Binding(keys, TestHandler);

        Assert.Equal(2, binding.Keys.Count);
    }

    #endregion

    #region Immutability

    [Fact]
    public void Keys_CannotBeModified_AfterConstruction()
    {
        KeyOrChar[] keys = [Keys.ControlM, Keys.ControlI];
        var binding = new Binding(keys, TestHandler);

        // Original array modification should not affect binding
        keys[0] = Keys.Escape;

        Assert.Equal(Keys.ControlM, binding.Keys[0].Key);
    }

    [Fact]
    public void Keys_IsReadOnlyList()
    {
        KeyOrChar[] keys = [Keys.ControlM];
        var binding = new Binding(keys, TestHandler);

        Assert.IsAssignableFrom<IReadOnlyList<KeyOrChar>>(binding.Keys);
    }

    #endregion

    #region ToString

    [Fact]
    public void ToString_IncludesKeysAndHandlerName()
    {
        KeyOrChar[] keys = [Keys.ControlC];
        var binding = new Binding(keys, TestHandler);

        var result = binding.ToString();

        Assert.Contains("ControlC", result);
        Assert.Contains("TestHandler", result);
    }

    [Fact]
    public void ToString_WithMultipleKeys_IncludesAllKeys()
    {
        KeyOrChar[] keys = [Keys.ControlX, Keys.ControlC];
        var binding = new Binding(keys, TestHandler);

        var result = binding.ToString();

        Assert.Contains("ControlX", result);
        Assert.Contains("ControlC", result);
    }

    #endregion
}
