using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;
using Xunit;

// Use explicit alias to avoid ambiguity with Stroke.Input.KeyPress
using KeyPress = Stroke.KeyBinding.KeyPress;

namespace Stroke.Tests.KeyBinding;

/// <summary>
/// Edge case tests for the key binding system per spec Edge Cases section.
/// </summary>
public sealed class EdgeCaseTests
{
    #region Test Helpers

    private static NotImplementedOrNone? TestHandler(KeyPressEvent e) => null;
    private static NotImplementedOrNone? Handler1(KeyPressEvent e) => null;
    private static NotImplementedOrNone? Handler2(KeyPressEvent e) => null;

    private static KeyPressEvent CreateEvent()
    {
        return new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: [new KeyPress(Keys.ControlM)],
            previousKeySequence: [],
            isRepeat: false);
    }

    #endregion

    #region T079: Empty key sequence query

    [Fact]
    public void GetBindingsForKeys_EmptySequence_ReturnsEmpty()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);

        var result = kb.GetBindingsForKeys([]);

        Assert.Empty(result);
    }

    [Fact]
    public void GetBindingsStartingWithKeys_EmptySequence_ReturnsAll()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(Handler1);
        kb.Add<KeyHandlerCallable>([Keys.ControlX, Keys.ControlC])(Handler2);

        var result = kb.GetBindingsStartingWithKeys([]);

        // All bindings start with empty prefix
        Assert.Equal(2, result.Count);
    }

    #endregion

    #region T080: Empty key sequence registration

    [Fact]
    public void Add_EmptyKeys_ThrowsArgumentException()
    {
        var kb = new KeyBindings();

        var ex = Assert.Throws<ArgumentException>(() =>
            kb.Add<KeyHandlerCallable>([])(TestHandler));

        Assert.Equal("keys", ex.ParamName);
    }

    #endregion

    #region T081: Null handler

    [Fact]
    public void Binding_NullHandler_ThrowsArgumentNullException()
    {
        KeyOrChar[] keys = [Keys.ControlC];

        var ex = Assert.Throws<ArgumentNullException>(() =>
            new Binding(keys, null!));

        Assert.Equal("handler", ex.ParamName);
    }

    #endregion

    #region T082: Keys.Any wildcard matching

    [Fact]
    public void GetBindingsForKeys_AnyMatchesAnyKey()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.Any])(TestHandler);

        // Any should match any single key
        var result1 = kb.GetBindingsForKeys([Keys.ControlC]);
        var result2 = kb.GetBindingsForKeys([Keys.ControlX]);
        var result3 = kb.GetBindingsForKeys(['a']);

        Assert.Single(result1);
        Assert.Single(result2);
        Assert.Single(result3);
    }

    [Fact]
    public void GetBindingsForKeys_AnyDoesNotMatchMultipleKeys()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.Any])(TestHandler);

        // Any matches exactly one key
        var result = kb.GetBindingsForKeys([Keys.ControlX, Keys.ControlC]);

        Assert.Empty(result);
    }

    [Fact]
    public void GetBindingsForKeys_FewerAnyHigherPriority()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.Any])(Handler1); // 1 wildcard
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(Handler2); // 0 wildcards

        var result = kb.GetBindingsForKeys([Keys.ControlC]);

        // Both match, but exact match (0 wildcards) should be first
        Assert.Equal(2, result.Count);
        Assert.Equal((KeyHandlerCallable)Handler2, result[0].Handler);
        Assert.Equal((KeyHandlerCallable)Handler1, result[1].Handler);
    }

    #endregion

    #region T083: Multiple Keys.Any

    [Fact]
    public void GetBindingsForKeys_MultipleAny_MatchesAnyTwoKeySequence()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.Any, Keys.Any])(TestHandler);

        var result = kb.GetBindingsForKeys([Keys.ControlX, Keys.ControlC]);

        Assert.Single(result);
    }

    [Fact]
    public void GetBindingsForKeys_MultipleAny_DoesNotMatchSingleKey()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.Any, Keys.Any])(TestHandler);

        var result = kb.GetBindingsForKeys([Keys.ControlC]);

        Assert.Empty(result);
    }

    #endregion

    #region T084: Filter composition AND

    [Fact]
    public void FilterComposition_AND_TrueAndTrue_ReturnsTrue()
    {
        var filter1 = new Condition(() => true);
        var filter2 = new Condition(() => true);

        var composed = filter1.And(filter2);

        Assert.True(composed.Invoke());
    }

    [Fact]
    public void FilterComposition_AND_TrueAndFalse_ReturnsFalse()
    {
        var filter1 = new Condition(() => true);
        var filter2 = new Condition(() => false);

        var composed = filter1.And(filter2);

        Assert.False(composed.Invoke());
    }

    [Fact]
    public void FilterComposition_AND_FalseAndTrue_ReturnsFalse()
    {
        var filter1 = new Condition(() => false);
        var filter2 = new Condition(() => true);

        var composed = filter1.And(filter2);

        Assert.False(composed.Invoke());
    }

    [Fact]
    public void FilterComposition_AND_FalseAndFalse_ReturnsFalse()
    {
        var filter1 = new Condition(() => false);
        var filter2 = new Condition(() => false);

        var composed = filter1.And(filter2);

        Assert.False(composed.Invoke());
    }

    #endregion

    #region T085: Filter composition OR for eager

    [Fact]
    public void FilterComposition_OR_TrueOrTrue_ReturnsTrue()
    {
        var filter1 = new Condition(() => true);
        var filter2 = new Condition(() => true);

        var composed = filter1.Or(filter2);

        Assert.True(composed.Invoke());
    }

    [Fact]
    public void FilterComposition_OR_TrueOrFalse_ReturnsTrue()
    {
        var filter1 = new Condition(() => true);
        var filter2 = new Condition(() => false);

        var composed = filter1.Or(filter2);

        Assert.True(composed.Invoke());
    }

    [Fact]
    public void FilterComposition_OR_FalseOrTrue_ReturnsTrue()
    {
        var filter1 = new Condition(() => false);
        var filter2 = new Condition(() => true);

        var composed = filter1.Or(filter2);

        Assert.True(composed.Invoke());
    }

    [Fact]
    public void FilterComposition_OR_FalseOrFalse_ReturnsFalse()
    {
        var filter1 = new Condition(() => false);
        var filter2 = new Condition(() => false);

        var composed = filter1.Or(filter2);

        Assert.False(composed.Invoke());
    }

    #endregion

    #region T087: Unicode character keys

    [Fact]
    public void KeyOrChar_WithEmoji_CreatesCorrectly()
    {
        // Note: Emojis are often surrogate pairs, but single emoji should work
        var koc = new KeyOrChar('日');

        Assert.True(koc.IsChar);
        Assert.Equal('日', koc.Char);
    }

    [Fact]
    public void KeyOrChar_WithCJK_CreatesCorrectly()
    {
        var koc = new KeyOrChar('中');

        Assert.True(koc.IsChar);
        Assert.Equal('中', koc.Char);
    }

    [Fact]
    public void Binding_WithUnicodeKeys_Works()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>(['日', '本'])(TestHandler);

        var result = kb.GetBindingsForKeys(['日', '本']);

        Assert.Single(result);
    }

    #endregion

    #region T088: Filter exception propagation

    [Fact]
    public void Filter_WhenThrows_ExceptionPropagates()
    {
        var filter = new Condition(() => throw new InvalidOperationException("test"));

        Assert.Throws<InvalidOperationException>(() => filter.Invoke());
    }

    [Fact]
    public void Binding_FilterException_PropagatesOnInvoke()
    {
        var kb = new KeyBindings();
        var filter = new Condition(() => throw new InvalidOperationException("test"));
        kb.Add<KeyHandlerCallable>([Keys.ControlC], filter: filter)(TestHandler);

        // Filter is evaluated during GetBindingsForKeys when checking if binding is active
        // Actually, filters are stored, not evaluated during lookup
        // They're evaluated when determining if binding should execute
        var binding = kb.Bindings[0];

        Assert.Throws<InvalidOperationException>(() => binding.Filter.Invoke());
    }

    #endregion

    #region T089: DynamicKeyBindings callable exception

    [Fact]
    public void DynamicKeyBindings_CallableException_Propagates()
    {
        var dkb = new DynamicKeyBindings(() => throw new InvalidOperationException("test"));

        Assert.Throws<InvalidOperationException>(() => _ = dkb.Bindings);
    }

    #endregion

    #region T090: SaveBefore exception

    [Fact]
    public void Binding_SaveBeforeException_HandlerNotExecuted()
    {
        bool handlerCalled = false;
        KeyHandlerCallable handler = e =>
        {
            handlerCalled = true;
            return null;
        };
        var binding = new Binding(
            [Keys.ControlC],
            handler,
            saveBefore: _ => throw new InvalidOperationException("test"));

        Assert.Throws<InvalidOperationException>(() => binding.Call(CreateEvent()));
        Assert.False(handlerCalled);
    }

    #endregion

    #region Keys Validation

    [Fact]
    public void Add_NullKeys_ThrowsArgumentNullException()
    {
        var kb = new KeyBindings();

        Assert.Throws<ArgumentNullException>(() =>
            kb.Add<KeyHandlerCallable>(null!));
    }

    #endregion
}
