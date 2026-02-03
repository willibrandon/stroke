using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;
using Xunit;

// Use explicit alias to avoid ambiguity with Stroke.Input.KeyPress
using KeyPress = Stroke.KeyBinding.KeyPress;

namespace Stroke.Tests.KeyBinding;

/// <summary>
/// Tests for the <see cref="KeyBindings"/> class.
/// </summary>
public sealed class KeyBindingsTests
{
    #region Test Helpers

    private static NotImplementedOrNone? TestHandler(KeyPressEvent e) => null;
    private static NotImplementedOrNone? Handler1(KeyPressEvent e) => null;
    private static NotImplementedOrNone? Handler2(KeyPressEvent e) => null;

    #endregion

    #region Phase 3: User Story 1 - Register Single Key Bindings

    // T014: Test add single key binding
    [Fact]
    public void Add_SingleKeyBinding_IncreasesBindingsCount()
    {
        var kb = new KeyBindings();
        KeyOrChar[] keys = [Keys.ControlC];

        kb.Add<KeyHandlerCallable>(keys)(TestHandler);

        Assert.Single(kb.Bindings);
    }

    [Fact]
    public void Add_SingleKeyBinding_ContainsCorrectKey()
    {
        var kb = new KeyBindings();
        KeyOrChar[] keys = [Keys.ControlC];

        kb.Add<KeyHandlerCallable>(keys)(TestHandler);

        var binding = kb.Bindings[0];
        Assert.Single(binding.Keys);
        Assert.Equal(Keys.ControlC, binding.Keys[0].Key);
    }

    [Fact]
    public void Add_SingleKeyBinding_ContainsCorrectHandler()
    {
        var kb = new KeyBindings();
        KeyOrChar[] keys = [Keys.ControlC];

        kb.Add<KeyHandlerCallable>(keys)(TestHandler);

        var binding = kb.Bindings[0];
        Assert.Equal((KeyHandlerCallable)TestHandler, binding.Handler);
    }

    // T015: Test query exact match
    [Fact]
    public void GetBindingsForKeys_ExactMatch_ReturnsBinding()
    {
        var kb = new KeyBindings();
        KeyOrChar[] keys = [Keys.ControlX];
        kb.Add<KeyHandlerCallable>(keys)(TestHandler);

        var matches = kb.GetBindingsForKeys([Keys.ControlX]);

        Assert.Single(matches);
        Assert.Equal((KeyHandlerCallable)TestHandler, matches[0].Handler);
    }

    [Fact]
    public void GetBindingsForKeys_NoMatch_ReturnsEmpty()
    {
        var kb = new KeyBindings();
        KeyOrChar[] keys = [Keys.ControlX];
        kb.Add<KeyHandlerCallable>(keys)(TestHandler);

        var matches = kb.GetBindingsForKeys([Keys.ControlC]);

        Assert.Empty(matches);
    }

    [Fact]
    public void GetBindingsForKeys_PartialMatch_ReturnsEmpty()
    {
        var kb = new KeyBindings();
        KeyOrChar[] keys = [Keys.ControlX, Keys.ControlC];
        kb.Add<KeyHandlerCallable>(keys)(TestHandler);

        var matches = kb.GetBindingsForKeys([Keys.ControlX]);

        Assert.Empty(matches);
    }

    // T016: Test multi-key sequence
    [Fact]
    public void Add_MultiKeySequence_AddsBinding()
    {
        var kb = new KeyBindings();
        KeyOrChar[] keys = [Keys.ControlX, Keys.ControlC];

        kb.Add<KeyHandlerCallable>(keys)(TestHandler);

        Assert.Single(kb.Bindings);
        Assert.Equal(2, kb.Bindings[0].Keys.Count);
    }

    [Fact]
    public void GetBindingsForKeys_MultiKeySequence_ReturnsBinding()
    {
        var kb = new KeyBindings();
        KeyOrChar[] keys = [Keys.ControlX, Keys.ControlC];
        kb.Add<KeyHandlerCallable>(keys)(TestHandler);

        var matches = kb.GetBindingsForKeys([Keys.ControlX, Keys.ControlC]);

        Assert.Single(matches);
    }

    [Fact]
    public void GetBindingsForKeys_MultiKeySequenceWrongOrder_ReturnsEmpty()
    {
        var kb = new KeyBindings();
        KeyOrChar[] keys = [Keys.ControlX, Keys.ControlC];
        kb.Add<KeyHandlerCallable>(keys)(TestHandler);

        var matches = kb.GetBindingsForKeys([Keys.ControlC, Keys.ControlX]);

        Assert.Empty(matches);
    }

    // T017: Test registration order (FIFO)
    [Fact]
    public void GetBindingsForKeys_MultipleBindingsSameKey_ReturnsFIFOOrder()
    {
        var kb = new KeyBindings();
        KeyOrChar[] keys = [Keys.ControlC];
        kb.Add<KeyHandlerCallable>(keys)(Handler1);
        kb.Add<KeyHandlerCallable>(keys)(Handler2);

        var matches = kb.GetBindingsForKeys([Keys.ControlC]);

        Assert.Equal(2, matches.Count);
        Assert.Equal((KeyHandlerCallable)Handler1, matches[0].Handler);
        Assert.Equal((KeyHandlerCallable)Handler2, matches[1].Handler);
    }

    [Fact]
    public void Bindings_MultipleBindings_ReturnsFIFOOrder()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlA])(Handler1);
        kb.Add<KeyHandlerCallable>([Keys.ControlB])(Handler2);

        Assert.Equal(2, kb.Bindings.Count);
        Assert.Equal((KeyHandlerCallable)Handler1, kb.Bindings[0].Handler);
        Assert.Equal((KeyHandlerCallable)Handler2, kb.Bindings[1].Handler);
    }

    #endregion

    #region Version Tracking

    [Fact]
    public void Version_AfterAdd_Increments()
    {
        var kb = new KeyBindings();
        var initialVersion = kb.Version;

        kb.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);

        Assert.NotEqual(initialVersion, kb.Version);
    }

    [Fact]
    public void Version_MultipleAdds_IncrementsEachTime()
    {
        var kb = new KeyBindings();
        var v1 = kb.Version;

        kb.Add<KeyHandlerCallable>([Keys.ControlA])(TestHandler);
        var v2 = kb.Version;

        kb.Add<KeyHandlerCallable>([Keys.ControlB])(TestHandler);
        var v3 = kb.Version;

        Assert.NotEqual(v1, v2);
        Assert.NotEqual(v2, v3);
    }

    #endregion

    #region Bindings Property

    [Fact]
    public void Bindings_Empty_ReturnsEmptyList()
    {
        var kb = new KeyBindings();

        Assert.Empty(kb.Bindings);
    }

    [Fact]
    public void Bindings_ReturnsSnapshot()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);

        var snapshot1 = kb.Bindings;
        var snapshot2 = kb.Bindings;

        // Each call returns a new snapshot
        Assert.NotSame(snapshot1, snapshot2);
        // But content is the same
        Assert.Equal(snapshot1.Count, snapshot2.Count);
    }

    #endregion

    #region Remove Methods

    [Fact]
    public void Remove_ByHandler_RemovesBinding()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);

        kb.Remove(TestHandler);

        Assert.Empty(kb.Bindings);
    }

    [Fact]
    public void Remove_ByHandler_ThrowsIfNotFound()
    {
        var kb = new KeyBindings();

        Assert.Throws<InvalidOperationException>(() => kb.Remove(TestHandler));
    }

    [Fact]
    public void Remove_ByKeys_RemovesBinding()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);

        kb.Remove(Keys.ControlC);

        Assert.Empty(kb.Bindings);
    }

    [Fact]
    public void Remove_ByKeys_ThrowsIfNotFound()
    {
        var kb = new KeyBindings();

        Assert.Throws<InvalidOperationException>(() => kb.Remove(Keys.ControlC));
    }

    [Fact]
    public void Remove_IncrementsVersion()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);
        var versionBeforeRemove = kb.Version;

        kb.Remove(TestHandler);

        Assert.NotEqual(versionBeforeRemove, kb.Version);
    }

    #endregion

    #region GetBindingsStartingWithKeys

    [Fact]
    public void GetBindingsStartingWithKeys_ReturnsLongerSequences()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlX, Keys.ControlC])(TestHandler);

        var matches = kb.GetBindingsStartingWithKeys([Keys.ControlX]);

        Assert.Single(matches);
    }

    [Fact]
    public void GetBindingsStartingWithKeys_ExcludesExactMatches()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlX])(TestHandler);

        var matches = kb.GetBindingsStartingWithKeys([Keys.ControlX]);

        Assert.Empty(matches);
    }

    [Fact]
    public void GetBindingsStartingWithKeys_EmptyPrefix_ReturnsAll()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlA])(Handler1);
        kb.Add<KeyHandlerCallable>([Keys.ControlB])(Handler2);

        var matches = kb.GetBindingsStartingWithKeys([]);

        Assert.Equal(2, matches.Count);
    }

    #endregion

    #region Keys.Any Wildcard Priority

    [Fact]
    public void GetBindingsForKeys_WildcardLowerPriority()
    {
        var kb = new KeyBindings();
        // Specific binding (no wildcards)
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(Handler1);
        // Wildcard binding
        kb.Add<KeyHandlerCallable>([Keys.Any])(Handler2);

        var matches = kb.GetBindingsForKeys([Keys.ControlC]);

        // Both match. Sorted descending by AnyCount: [wildcard, specific].
        // KeyProcessor uses matches[^1] (last) to get the most specific (fewest wildcards).
        Assert.Equal(2, matches.Count);
        Assert.Equal((KeyHandlerCallable)Handler2, matches[0].Handler); // wildcard (1 Any)
        Assert.Equal((KeyHandlerCallable)Handler1, matches[1].Handler); // specific (0 Any)
    }

    [Fact]
    public void GetBindingsForKeys_WildcardMatchesAnyKey()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.Any])(TestHandler);

        var matches = kb.GetBindingsForKeys([Keys.ControlX]);

        Assert.Single(matches);
    }

    [Fact]
    public void GetBindingsForKeys_MultipleWildcardsLowerPriority()
    {
        var kb = new KeyBindings();
        // Two wildcards
        kb.Add<KeyHandlerCallable>([Keys.Any, Keys.Any])(Handler1);
        // One wildcard
        kb.Add<KeyHandlerCallable>([Keys.ControlX, Keys.Any])(Handler2);

        var matches = kb.GetBindingsForKeys([Keys.ControlX, Keys.ControlC]);

        // Sorted descending by AnyCount: [2 wildcards, 1 wildcard].
        // KeyProcessor uses matches[^1] (last) to get the most specific (fewest wildcards).
        Assert.Equal(2, matches.Count);
        Assert.Equal((KeyHandlerCallable)Handler1, matches[0].Handler); // 2 wildcards
        Assert.Equal((KeyHandlerCallable)Handler2, matches[1].Handler); // 1 wildcard
    }

    #endregion

    #region Cache Behavior

    [Fact]
    public void GetBindingsForKeys_ReturnsEquivalentResults()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);

        // Note: ImmutableArray<KeyOrChar> uses reference equality by default,
        // so the cache may not hit on separate array instances.
        // This test verifies the results are equivalent.
        var result1 = kb.GetBindingsForKeys([Keys.ControlC]);
        var result2 = kb.GetBindingsForKeys([Keys.ControlC]);

        // Results should be equivalent
        Assert.Equal(result1.Count, result2.Count);
        Assert.Equal((KeyHandlerCallable)TestHandler, result1[0].Handler);
        Assert.Equal((KeyHandlerCallable)TestHandler, result2[0].Handler);
    }

    [Fact]
    public void GetBindingsForKeys_CacheInvalidatedOnAdd()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(Handler1);

        var result1 = kb.GetBindingsForKeys([Keys.ControlC]);

        // Add another binding (invalidates cache)
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(Handler2);

        var result2 = kb.GetBindingsForKeys([Keys.ControlC]);

        // Different result after cache invalidation
        Assert.NotSame(result1, result2);
        Assert.Equal(2, result2.Count);
    }

    [Fact]
    public void GetBindingsForKeys_CacheInvalidatedOnRemove()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(Handler1);
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(Handler2);

        var result1 = kb.GetBindingsForKeys([Keys.ControlC]);

        // Remove a binding (invalidates cache)
        kb.Remove(Handler1);

        var result2 = kb.GetBindingsForKeys([Keys.ControlC]);

        // Different result after cache invalidation
        Assert.NotSame(result1, result2);
        Assert.Single(result2);
    }

    #endregion

    #region Validation

    [Fact]
    public void Add_EmptyKeys_ThrowsArgumentException()
    {
        var kb = new KeyBindings();

        Assert.Throws<ArgumentException>(() => kb.Add<KeyHandlerCallable>([]));
    }

    [Fact]
    public void Add_NullKeys_ThrowsArgumentNullException()
    {
        var kb = new KeyBindings();

        Assert.Throws<ArgumentNullException>(() => kb.Add<KeyHandlerCallable>(null!));
    }

    [Fact]
    public void GetBindingsForKeys_NullKeys_ThrowsArgumentNullException()
    {
        var kb = new KeyBindings();

        Assert.Throws<ArgumentNullException>(() => kb.GetBindingsForKeys(null!));
    }

    [Fact]
    public void GetBindingsForKeys_EmptyKeys_ReturnsEmpty()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);

        var result = kb.GetBindingsForKeys([]);

        Assert.Empty(result);
    }

    #endregion

    #region Decorator Pattern

    [Fact]
    public void Add_ReturnsDecoratorThatReturnsInput()
    {
        var kb = new KeyBindings();

        var decorator = kb.Add<KeyHandlerCallable>([Keys.ControlC]);
        var result = decorator(TestHandler);

        Assert.Equal((KeyHandlerCallable)TestHandler, result);
    }

    [Fact]
    public void Add_DecoratorWithBinding_ReturnsInputBinding()
    {
        var kb = new KeyBindings();
        var originalBinding = new Binding([Keys.ControlA], TestHandler);

        var decorator = kb.Add<Binding>([Keys.ControlC]);
        var result = decorator(originalBinding);

        Assert.Same(originalBinding, result);
    }

    [Fact]
    public void Add_DecoratorWithBinding_ComposesFilters()
    {
        var kb = new KeyBindings();
        var customFilter = new Condition(() => true);
        var originalEager = new Condition(() => true);
        var originalBinding = new Binding([Keys.ControlA], TestHandler, filter: customFilter, eager: originalEager);

        // Add with additional filter
        var decoratorEager = new Condition(() => false);
        kb.Add<Binding>([Keys.ControlC], eager: decoratorEager)(originalBinding);

        // The new binding should have composed filters
        var binding = kb.Bindings[0];
        // Eager should be OR composition of originalEager and decoratorEager
        // Since neither is Always/Never, a new OrList filter should be created
        Assert.NotSame(originalEager, binding.Eager);
        Assert.NotSame(decoratorEager, binding.Eager);
    }

    #endregion

    #region Backwards Compatibility Aliases

    [Fact]
    public void AddBinding_IsAliasForAdd()
    {
        var kb = new KeyBindings();

        kb.AddBinding<KeyHandlerCallable>([Keys.ControlC])(TestHandler);

        Assert.Single(kb.Bindings);
    }

    [Fact]
    public void RemoveBinding_ByHandler_IsAliasForRemove()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);

        kb.RemoveBinding(TestHandler);

        Assert.Empty(kb.Bindings);
    }

    [Fact]
    public void RemoveBinding_ByKeys_IsAliasForRemove()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);

        kb.RemoveBinding(Keys.ControlC);

        Assert.Empty(kb.Bindings);
    }

    #endregion

    #region Phase 4: User Story 2 - Conditional Key Bindings with Filters

    // T023: Test Always filter active
    [Fact]
    public void Add_WithAlwaysFilter_BindingIsActive()
    {
        var kb = new KeyBindings();

        kb.Add<KeyHandlerCallable>([Keys.ControlC], filter: Always.Instance)(TestHandler);

        Assert.Single(kb.Bindings);
        Assert.IsType<Always>(kb.Bindings[0].Filter);
        Assert.True(kb.Bindings[0].Filter.Invoke());
    }

    // T024: Test Never filter optimized away per FR-025
    [Fact]
    public void Add_WithNeverFilter_BindingNotStored()
    {
        var kb = new KeyBindings();

        kb.Add<KeyHandlerCallable>([Keys.ControlC], filter: Never.Instance)(TestHandler);

        Assert.Empty(kb.Bindings);
    }

    // T025: Test conditional filter active
    [Fact]
    public void Add_WithConditionTrue_BindingIsActive()
    {
        var kb = new KeyBindings();
        var filter = new Condition(() => true);

        kb.Add<KeyHandlerCallable>([Keys.ControlC], filter: filter)(TestHandler);

        Assert.Single(kb.Bindings);
        Assert.True(kb.Bindings[0].Filter.Invoke());
    }

    // T026: Test conditional filter inactive
    [Fact]
    public void Add_WithConditionFalse_BindingIsInactive()
    {
        var kb = new KeyBindings();
        var filter = new Condition(() => false);

        kb.Add<KeyHandlerCallable>([Keys.ControlC], filter: filter)(TestHandler);

        Assert.Single(kb.Bindings);
        Assert.False(kb.Bindings[0].Filter.Invoke());
    }

    #endregion

    #region Thread Safety (Basic Verification)

    [Fact]
    public async Task ConcurrentAdds_NoExceptions()
    {
        var kb = new KeyBindings();
        var tasks = new List<Task>();
        var ct = TestContext.Current.CancellationToken;

        for (int i = 0; i < 100; i++)
        {
            var key = (Keys)(i % 30); // Cycle through first 30 keys
            tasks.Add(Task.Run(() =>
            {
                kb.Add<KeyHandlerCallable>([key])(TestHandler);
            }, ct));
        }

        await Task.WhenAll(tasks);

        Assert.Equal(100, kb.Bindings.Count);
    }

    [Fact]
    public async Task ConcurrentReads_NoExceptions()
    {
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);

        var tasks = new List<Task>();
        var ct = TestContext.Current.CancellationToken;

        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                var bindings = kb.Bindings;
                var version = kb.Version;
                var matches = kb.GetBindingsForKeys([Keys.ControlC]);
            }, ct));
        }

        await Task.WhenAll(tasks);
    }

    #endregion
}
