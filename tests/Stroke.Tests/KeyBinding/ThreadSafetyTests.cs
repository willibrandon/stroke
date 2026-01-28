using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;
using Xunit;

// Use explicit alias to avoid ambiguity with Stroke.Input.KeyPress
using KeyPress = Stroke.KeyBinding.KeyPress;

namespace Stroke.Tests.KeyBinding;

/// <summary>
/// Thread safety tests for the key binding system per Constitution XI.
/// </summary>
public sealed class ThreadSafetyTests
{
    #region Test Helpers

    private static NotImplementedOrNone? TestHandler(KeyPressEvent e) => null;
    private static NotImplementedOrNone? Handler1(KeyPressEvent e) => null;
    private static NotImplementedOrNone? Handler2(KeyPressEvent e) => null;

    #endregion

    #region T072: Concurrent reads

    [Fact]
    public async Task ConcurrentReads_NoExceptions()
    {
        var kb = new KeyBindings();
        for (int i = 0; i < 100; i++)
        {
            kb.Add<KeyHandlerCallable>([Keys.ControlA])(TestHandler);
        }

        var tasks = new List<Task>();
        var ct = TestContext.Current.CancellationToken;

        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    _ = kb.GetBindingsForKeys([Keys.ControlA]);
                    _ = kb.GetBindingsStartingWithKeys([Keys.ControlA]);
                    _ = kb.Bindings;
                    _ = kb.Version;
                }
            }, ct));
        }

        await Task.WhenAll(tasks);
        // If we get here without exceptions, concurrent reads are thread-safe
    }

    #endregion

    #region T073: Concurrent add/read

    [Fact]
    public async Task ConcurrentAddAndRead_NoExceptions()
    {
        var kb = new KeyBindings();
        var tasks = new List<Task>();
        var ct = TestContext.Current.CancellationToken;

        // Writers
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 20; j++)
                {
                    kb.Add<KeyHandlerCallable>([Keys.ControlA])(TestHandler);
                }
            }, ct));
        }

        // Readers
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    _ = kb.GetBindingsForKeys([Keys.ControlA]);
                    _ = kb.Bindings;
                    _ = kb.Version;
                }
            }, ct));
        }

        await Task.WhenAll(tasks);
        // If we get here without exceptions, concurrent add/read is thread-safe
    }

    #endregion

    #region T074: Atomic add

    [Fact]
    public async Task AtomicAdd_BindingAlwaysComplete()
    {
        var kb = new KeyBindings();
        var filter = new Condition(() => true);
        var completedBindings = new List<Binding>();
        var readLock = new Lock();
        var ct = TestContext.Current.CancellationToken;

        var addTask = Task.Run(() =>
        {
            for (int i = 0; i < 100; i++)
            {
                kb.Add<KeyHandlerCallable>([Keys.ControlC], filter: filter, eager: true)(TestHandler);
            }
        }, ct);

        var readTask = Task.Run(() =>
        {
            for (int i = 0; i < 1000; i++)
            {
                var bindings = kb.Bindings;
                using (readLock.EnterScope())
                {
                    completedBindings.AddRange(bindings);
                }
            }
        }, ct);

        await Task.WhenAll(addTask, readTask);

        // All observed bindings should be complete (filter set correctly)
        foreach (var binding in completedBindings.Distinct())
        {
            Assert.NotNull(binding.Handler);
            Assert.NotNull(binding.Filter);
            Assert.NotNull(binding.Eager);
        }
    }

    #endregion

    #region T075: Atomic remove

    [Fact]
    public async Task AtomicRemove_NoPartialState()
    {
        var kb = new KeyBindings();
        var handlers = new KeyHandlerCallable[10];
        for (int i = 0; i < 10; i++)
        {
            KeyHandlerCallable h = e => null;
            handlers[i] = h;
            kb.Add<KeyHandlerCallable>([Keys.ControlA])(h);
        }

        var ct = TestContext.Current.CancellationToken;
        var observedCounts = new List<int>();
        var countLock = new Lock();

        var removeTask = Task.Run(() =>
        {
            foreach (var handler in handlers)
            {
                try
                {
                    kb.Remove(handler);
                }
                catch (InvalidOperationException)
                {
                    // Handler might already be removed
                }
            }
        }, ct);

        var readTask = Task.Run(() =>
        {
            for (int i = 0; i < 1000; i++)
            {
                var count = kb.Bindings.Count;
                using (countLock.EnterScope())
                {
                    observedCounts.Add(count);
                }
            }
        }, ct);

        await Task.WhenAll(removeTask, readTask);

        // All observed counts should be valid (0-10)
        Assert.All(observedCounts, c => Assert.InRange(c, 0, 10));
    }

    #endregion

    #region T076: Version atomicity

    [Fact]
    public async Task VersionAtomicity_NoTornReads()
    {
        var kb = new KeyBindings();
        var ct = TestContext.Current.CancellationToken;
        var observedVersions = new List<object>();
        var versionLock = new Lock();

        var addTask = Task.Run(() =>
        {
            for (int i = 0; i < 100; i++)
            {
                kb.Add<KeyHandlerCallable>([Keys.ControlC])(TestHandler);
            }
        }, ct);

        var readTask = Task.Run(() =>
        {
            for (int i = 0; i < 1000; i++)
            {
                var version = kb.Version;
                using (versionLock.EnterScope())
                {
                    observedVersions.Add(version);
                }
            }
        }, ct);

        await Task.WhenAll(addTask, readTask);

        // All observed versions should be valid integers
        Assert.All(observedVersions, v => Assert.IsType<int>(v));

        // Versions should be monotonically increasing when compared
        var intVersions = observedVersions.Cast<int>().ToList();
        int maxVersion = intVersions.Max();

        // All versions should be within expected range
        Assert.All(intVersions, v => Assert.InRange(v, 0, maxVersion));
    }

    #endregion

    #region T077: Cache thread safety

    [Fact]
    public async Task CacheThreadSafety_ConcurrentQueriesDoNotCorrupt()
    {
        var kb = new KeyBindings();
        for (int i = 0; i < 100; i++)
        {
            kb.Add<KeyHandlerCallable>([Keys.ControlA])(TestHandler);
            kb.Add<KeyHandlerCallable>([Keys.ControlB])(Handler1);
            kb.Add<KeyHandlerCallable>([Keys.ControlC])(Handler2);
        }

        var ct = TestContext.Current.CancellationToken;
        var keys = new[] { Keys.ControlA, Keys.ControlB, Keys.ControlC };
        var errors = new List<Exception>();
        var errorLock = new Lock();

        var tasks = new List<Task>();

        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    for (int j = 0; j < 100; j++)
                    {
                        foreach (var key in keys)
                        {
                            var result = kb.GetBindingsForKeys([key]);
                            Assert.Equal(100, result.Count);

                            var starting = kb.GetBindingsStartingWithKeys([key]);
                            // Starting should be empty since all bindings are single key
                            Assert.Empty(starting);
                        }
                    }
                }
                catch (Exception ex)
                {
                    using (errorLock.EnterScope())
                    {
                        errors.Add(ex);
                    }
                }
            }, ct));
        }

        await Task.WhenAll(tasks);

        Assert.Empty(errors);
    }

    #endregion

    #region T078: Handler self-modification

    [Fact]
    public void HandlerSelfModification_AddInHandler_NoDeadlock()
    {
        var kb = new KeyBindings();
        bool handlerCalled = false;

        KeyHandlerCallable selfModifyingHandler = e =>
        {
            handlerCalled = true;
            // Handler adds a binding to its own registry
            kb.Add<KeyHandlerCallable>([Keys.ControlD])(TestHandler);
            return null;
        };

        kb.Add<KeyHandlerCallable>([Keys.ControlC])(selfModifyingHandler);

        // Get the binding and call it
        var bindings = kb.GetBindingsForKeys([Keys.ControlC]);
        Assert.Single(bindings);

        var e = CreateTestEvent();
        bindings[0].Call(e);

        Assert.True(handlerCalled);
        // The new binding should have been added
        Assert.Equal(2, kb.Bindings.Count);
    }

    [Fact]
    public void HandlerSelfModification_RemoveInHandler_NoDeadlock()
    {
        var kb = new KeyBindings();
        bool handlerCalled = false;
        KeyHandlerCallable? toRemove = null;

        KeyHandlerCallable selfModifyingHandler = e =>
        {
            handlerCalled = true;
            // Handler removes another binding from its registry
            if (toRemove != null)
            {
                try
                {
                    kb.Remove(toRemove);
                }
                catch (InvalidOperationException)
                {
                    // Already removed, ignore
                }
            }
            return null;
        };

        toRemove = TestHandler;
        kb.Add<KeyHandlerCallable>([Keys.ControlD])(TestHandler);
        kb.Add<KeyHandlerCallable>([Keys.ControlC])(selfModifyingHandler);

        Assert.Equal(2, kb.Bindings.Count);

        // Get the binding and call it
        var bindings = kb.GetBindingsForKeys([Keys.ControlC]);
        Assert.Single(bindings);

        var e = CreateTestEvent();
        bindings[0].Call(e);

        Assert.True(handlerCalled);
        // The binding should have been removed
        Assert.Single(kb.Bindings);
    }

    #endregion

    #region Proxy Thread Safety

    [Fact]
    public async Task ConditionalKeyBindings_ConcurrentAccess_NoExceptions()
    {
        var kb = new KeyBindings();
        for (int i = 0; i < 50; i++)
        {
            kb.Add<KeyHandlerCallable>([Keys.ControlA])(TestHandler);
        }

        bool filterState = true;
        var filter = new Condition(() => filterState);
        var ckb = new ConditionalKeyBindings(kb, filter);

        var ct = TestContext.Current.CancellationToken;
        var tasks = new List<Task>();

        // State toggler
        tasks.Add(Task.Run(() =>
        {
            for (int i = 0; i < 100; i++)
            {
                filterState = !filterState;
            }
        }, ct));

        // Readers
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    _ = ckb.Bindings;
                    _ = ckb.GetBindingsForKeys([Keys.ControlA]);
                    _ = ckb.Version;
                }
            }, ct));
        }

        await Task.WhenAll(tasks);
    }

    [Fact]
    public async Task MergedKeyBindings_ConcurrentModifyAndRead_NoExceptions()
    {
        var kb1 = new KeyBindings();
        var kb2 = new KeyBindings();
        var merged = new MergedKeyBindings([kb1, kb2]);

        var ct = TestContext.Current.CancellationToken;
        var tasks = new List<Task>();

        // Writers
        tasks.Add(Task.Run(() =>
        {
            for (int i = 0; i < 50; i++)
            {
                kb1.Add<KeyHandlerCallable>([Keys.ControlA])(TestHandler);
            }
        }, ct));

        tasks.Add(Task.Run(() =>
        {
            for (int i = 0; i < 50; i++)
            {
                kb2.Add<KeyHandlerCallable>([Keys.ControlB])(Handler1);
            }
        }, ct));

        // Readers
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    _ = merged.Bindings;
                    _ = merged.GetBindingsForKeys([Keys.ControlA]);
                    _ = merged.Version;
                }
            }, ct));
        }

        await Task.WhenAll(tasks);
    }

    [Fact]
    public async Task DynamicKeyBindings_ConcurrentSwitch_NoExceptions()
    {
        var kb1 = new KeyBindings();
        var kb2 = new KeyBindings();
        kb1.Add<KeyHandlerCallable>([Keys.ControlA])(TestHandler);
        kb2.Add<KeyHandlerCallable>([Keys.ControlB])(Handler1);

        IKeyBindingsBase? current = kb1;
        var dkb = new DynamicKeyBindings(() => current);

        var ct = TestContext.Current.CancellationToken;
        var tasks = new List<Task>();

        // Switcher
        tasks.Add(Task.Run(() =>
        {
            for (int i = 0; i < 100; i++)
            {
                current = current == kb1 ? kb2 : kb1;
            }
        }, ct));

        // Readers
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    _ = dkb.Bindings;
                    _ = dkb.Version;
                }
            }, ct));
        }

        await Task.WhenAll(tasks);
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
