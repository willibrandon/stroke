using Stroke.Application;
using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Output;
using Xunit;

using AppContext = Stroke.Application.AppContext;
using KBKeyPress = Stroke.KeyBinding.KeyPress;

namespace Stroke.Tests.KeyBinding;

public class KeyProcessorTests
{
    // Helper to create a KeyBindings with a single-key binding
    private static (KeyBindings Bindings, Func<int> GetCount) CreateSingleKeyBinding(char key)
    {
        int count = 0;
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([new KeyOrChar(key)])(e =>
        {
            Interlocked.Increment(ref count);
            return null;
        });
        return (kb, () => count);
    }

    // Helper to create a KeyBindings with a two-key sequence binding
    private static (KeyBindings Bindings, Func<int> GetCount) CreateTwoKeyBinding(char key1, char key2)
    {
        int count = 0;
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([new KeyOrChar(key1), new KeyOrChar(key2)])(e =>
        {
            Interlocked.Increment(ref count);
            return null;
        });
        return (kb, () => count);
    }

    [Fact]
    public void Constructor_CreatesKeyProcessor()
    {
        var kb = new KeyBindings();
        var processor = new KeyProcessor(kb);
        Assert.NotNull(processor);
    }

    [Fact]
    public void Constructor_NullBindings_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new KeyProcessor(null!));
    }

    [Fact]
    public void InputQueue_EmptyInitially()
    {
        var kb = new KeyBindings();
        var processor = new KeyProcessor(kb);
        Assert.Empty(processor.InputQueue);
    }

    [Fact]
    public void KeyBuffer_EmptyInitially()
    {
        var kb = new KeyBindings();
        var processor = new KeyProcessor(kb);
        Assert.Empty(processor.KeyBuffer);
    }

    [Fact]
    public void Arg_NullInitially()
    {
        var kb = new KeyBindings();
        var processor = new KeyProcessor(kb);
        Assert.Null(processor.Arg);
    }

    [Fact]
    public void Feed_AddsToInputQueue()
    {
        var kb = new KeyBindings();
        var processor = new KeyProcessor(kb);

        processor.Feed(new KBKeyPress('a'));
        Assert.Single(processor.InputQueue);
    }

    [Fact]
    public void Feed_First_InsertsAtFront()
    {
        var kb = new KeyBindings();
        var processor = new KeyProcessor(kb);

        processor.Feed(new KBKeyPress('a'));
        processor.Feed(new KBKeyPress('b'), first: true);

        // 'b' should be first
        var queue = processor.InputQueue.ToList();
        Assert.Equal(2, queue.Count);
        Assert.Equal('b', queue[0].Key.Char);
        Assert.Equal('a', queue[1].Key.Char);
    }

    [Fact]
    public void FeedMultiple_AddsAllToQueue()
    {
        var kb = new KeyBindings();
        var processor = new KeyProcessor(kb);

        processor.FeedMultiple([
            new KBKeyPress('a'),
            new KBKeyPress('b'),
            new KBKeyPress('c')
        ]);

        Assert.Equal(3, processor.InputQueue.Count);
    }

    [Fact]
    public void FeedMultiple_First_InsertsAtFront()
    {
        var kb = new KeyBindings();
        var processor = new KeyProcessor(kb);

        processor.Feed(new KBKeyPress('z'));
        processor.FeedMultiple([
            new KBKeyPress('a'),
            new KBKeyPress('b')
        ], first: true);

        var queue = processor.InputQueue.ToList();
        Assert.Equal(3, queue.Count);
        Assert.Equal('a', queue[0].Key.Char);
        Assert.Equal('b', queue[1].Key.Char);
        Assert.Equal('z', queue[2].Key.Char);
    }

    [Fact]
    public void FeedMultiple_NullEnumerable_Throws()
    {
        var kb = new KeyBindings();
        var processor = new KeyProcessor(kb);

        Assert.Throws<ArgumentNullException>(() => processor.FeedMultiple(null!));
    }

    [Fact]
    public void ProcessKeys_ExactMatch_DispatchesHandler()
    {
        var (kb, getCount) = CreateSingleKeyBinding('a');
        var processor = new KeyProcessor(kb);

        // Need to set up AppContext for ProcessKeys to work
        var app = new Application<object?>(output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        processor.Feed(new KBKeyPress('a'));
        processor.ProcessKeys();

        Assert.Equal(1, getCount());
        Assert.Empty(processor.InputQueue);
        Assert.Empty(processor.KeyBuffer);
    }

    [Fact]
    public void ProcessKeys_NoMatch_FlushesBuffer()
    {
        var (kb, getCount) = CreateSingleKeyBinding('a');
        var processor = new KeyProcessor(kb);

        var app = new Application<object?>(output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        // Feed a key that doesn't match
        processor.Feed(new KBKeyPress('z'));
        processor.ProcessKeys();

        // Handler should not be called
        Assert.Equal(0, getCount());
        // Queue and buffer should be empty after processing
        Assert.Empty(processor.InputQueue);
    }

    [Fact]
    public void ProcessKeys_PrefixMatch_WaitsForMoreKeys()
    {
        // Create a two-key binding: 'a' + 'b'
        var (kb, getCount) = CreateTwoKeyBinding('a', 'b');
        var processor = new KeyProcessor(kb);

        var app = new Application<object?>(output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        // Feed only the first key
        processor.Feed(new KBKeyPress('a'));
        processor.ProcessKeys();

        // Handler should NOT be called yet (prefix match, waiting for more)
        Assert.Equal(0, getCount());
        // Key buffer should contain the prefix key
        Assert.Single(processor.KeyBuffer);
    }

    [Fact]
    public void ProcessKeys_TwoKeySequence_Dispatches()
    {
        var (kb, getCount) = CreateTwoKeyBinding('a', 'b');
        var processor = new KeyProcessor(kb);

        var app = new Application<object?>(output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        // Feed both keys
        processor.Feed(new KBKeyPress('a'));
        processor.Feed(new KBKeyPress('b'));
        processor.ProcessKeys();

        Assert.Equal(1, getCount());
        Assert.Empty(processor.KeyBuffer);
    }

    [Fact]
    public void ProcessKeys_EagerBinding_DispatchesImmediately()
    {
        // Create an eager single-key binding for 'a'
        int eagerCount = 0;
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('a')],
            eager: new FilterOrBool(true))(e =>
        {
            Interlocked.Increment(ref eagerCount);
            return null;
        });

        // Also create a two-key binding 'a' + 'b'
        var (kb2, getTwoKeyCount) = CreateTwoKeyBinding('a', 'b');
        var merged = new MergedKeyBindings(kb, kb2);

        var processor = new KeyProcessor(merged);

        var app = new Application<object?>(output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        // Feed just 'a'
        processor.Feed(new KBKeyPress('a'));
        processor.ProcessKeys();

        // Eager binding should fire immediately, not wait for 'b'
        Assert.Equal(1, eagerCount);
        Assert.Empty(processor.KeyBuffer);
    }

    [Fact]
    public void EmptyQueue_ReturnsAllUnprocessedKeys()
    {
        var kb = new KeyBindings();
        var processor = new KeyProcessor(kb);

        processor.Feed(new KBKeyPress('a'));
        processor.Feed(new KBKeyPress('b'));
        processor.Feed(new KBKeyPress('c'));

        var keys = processor.EmptyQueue();

        Assert.Equal(3, keys.Count);
        Assert.Empty(processor.InputQueue);
        Assert.Empty(processor.KeyBuffer);
    }

    [Fact]
    public void EmptyQueue_ReturnsKeyBufferAndQueue()
    {
        // Create a two-key binding so we can get a key into the buffer
        var (kb, _) = CreateTwoKeyBinding('a', 'b');
        var processor = new KeyProcessor(kb);

        var app = new Application<object?>(output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        // Feed 'a' (prefix match, goes to buffer) and 'c' (stays in queue? No, all get processed)
        processor.Feed(new KBKeyPress('a'));
        processor.ProcessKeys();

        // 'a' should be in the key buffer (prefix match)
        Assert.Single(processor.KeyBuffer);

        // Now add more to the queue
        processor.Feed(new KBKeyPress('x'));

        var keys = processor.EmptyQueue();

        // Should contain both the buffer key and the queue key
        Assert.Equal(2, keys.Count);
        Assert.Empty(processor.InputQueue);
        Assert.Empty(processor.KeyBuffer);
    }

    [Fact]
    public void EmptyQueue_FiltersCprResponses()
    {
        var kb = new KeyBindings();
        var processor = new KeyProcessor(kb);

        processor.Feed(new KBKeyPress('a'));
        processor.Feed(new KBKeyPress(Keys.CPRResponse));
        processor.Feed(new KBKeyPress('b'));

        var keys = processor.EmptyQueue();

        // CPR responses should be filtered out
        Assert.Equal(2, keys.Count);
        Assert.True(keys.All(k => !(k.Key.IsKey && k.Key.Key == Keys.CPRResponse)));
    }

    [Fact]
    public void SendSigint_FeedsSigintAndProcesses()
    {
        int sigintCount = 0;
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>([new KeyOrChar(Keys.SIGINT)])(e =>
        {
            Interlocked.Increment(ref sigintCount);
            return null;
        });

        var processor = new KeyProcessor(kb);

        var app = new Application<object?>(output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        processor.SendSigint();

        Assert.Equal(1, sigintCount);
    }

    [Fact]
    public void Reset_ClearsAllState()
    {
        var kb = new KeyBindings();
        var processor = new KeyProcessor(kb);

        processor.Feed(new KBKeyPress('a'));
        processor.Feed(new KBKeyPress('b'));
        processor.Arg = "5";

        processor.Reset();

        Assert.Empty(processor.InputQueue);
        Assert.Empty(processor.KeyBuffer);
        Assert.Null(processor.Arg);
    }

    [Fact]
    public void BeforeKeyPress_EventExists()
    {
        var kb = new KeyBindings();
        var processor = new KeyProcessor(kb);

        Assert.NotNull(processor.BeforeKeyPress);
    }

    [Fact]
    public void AfterKeyPress_EventExists()
    {
        var kb = new KeyBindings();
        var processor = new KeyProcessor(kb);

        Assert.NotNull(processor.AfterKeyPress);
    }

    [Fact]
    public void BeforeKeyPress_FiresDuringProcessing()
    {
        int beforeCount = 0;
        var (kb, _) = CreateSingleKeyBinding('a');
        var processor = new KeyProcessor(kb);
        processor.BeforeKeyPress.AddHandler(_ => Interlocked.Increment(ref beforeCount));

        var app = new Application<object?>(output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        processor.Feed(new KBKeyPress('a'));
        processor.ProcessKeys();

        Assert.True(beforeCount > 0);
    }

    [Fact]
    public void AfterKeyPress_FiresDuringProcessing()
    {
        int afterCount = 0;
        var (kb, _) = CreateSingleKeyBinding('a');
        var processor = new KeyProcessor(kb);
        processor.AfterKeyPress.AddHandler(_ => Interlocked.Increment(ref afterCount));

        var app = new Application<object?>(output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        processor.Feed(new KBKeyPress('a'));
        processor.ProcessKeys();

        Assert.True(afterCount > 0);
    }

    [Fact]
    public void Arg_CanBeSetExternally()
    {
        var kb = new KeyBindings();
        var processor = new KeyProcessor(kb);

        processor.Arg = "42";
        Assert.Equal("42", processor.Arg);

        processor.Arg = null;
        Assert.Null(processor.Arg);
    }

    [Fact]
    public void ProcessKeys_MultipleExactMatches_LastBindingWins()
    {
        int firstCount = 0;
        int secondCount = 0;
        var kb = new KeyBindings();

        // Add two bindings for the same key
        kb.Add<KeyHandlerCallable>([new KeyOrChar('a')])(e =>
        {
            Interlocked.Increment(ref firstCount);
            return null;
        });
        kb.Add<KeyHandlerCallable>([new KeyOrChar('a')])(e =>
        {
            Interlocked.Increment(ref secondCount);
            return null;
        });

        var processor = new KeyProcessor(kb);

        var app = new Application<object?>(output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        processor.Feed(new KBKeyPress('a'));
        processor.ProcessKeys();

        // The last match should win (matches[^1] in CallHandler)
        Assert.Equal(0, firstCount);
        Assert.Equal(1, secondCount);
    }

    [Fact]
    public void ProcessKeys_ConditionalBinding_FilterFalse_NotDispatched()
    {
        int count = 0;
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('a')],
            filter: new FilterOrBool(false))(e =>
        {
            Interlocked.Increment(ref count);
            return null;
        });

        var processor = new KeyProcessor(kb);

        var app = new Application<object?>(output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        processor.Feed(new KBKeyPress('a'));
        processor.ProcessKeys();

        Assert.Equal(0, count);
    }

    [Fact]
    public void ProcessKeys_ConditionalBinding_FilterTrue_Dispatched()
    {
        int count = 0;
        var kb = new KeyBindings();
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('a')],
            filter: new FilterOrBool(true))(e =>
        {
            Interlocked.Increment(ref count);
            return null;
        });

        var processor = new KeyProcessor(kb);

        var app = new Application<object?>(output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        processor.Feed(new KBKeyPress('a'));
        processor.ProcessKeys();

        Assert.Equal(1, count);
    }

    [Fact]
    public void ProcessKeys_MultipleKeys_AllDispatched()
    {
        var (kb, getCount) = CreateSingleKeyBinding('a');
        var processor = new KeyProcessor(kb);

        var app = new Application<object?>(output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        processor.Feed(new KBKeyPress('a'));
        processor.Feed(new KBKeyPress('a'));
        processor.Feed(new KBKeyPress('a'));
        processor.ProcessKeys();

        Assert.Equal(3, getCount());
    }

    [Fact]
    public void Feed_AfterReset_WorksNormally()
    {
        var (kb, getCount) = CreateSingleKeyBinding('a');
        var processor = new KeyProcessor(kb);

        var app = new Application<object?>(output: new DummyOutput());
        using var scope = AppContext.SetApp(app.UnsafeCast);

        processor.Feed(new KBKeyPress('a'));
        processor.ProcessKeys();
        Assert.Equal(1, getCount());

        processor.Reset();

        processor.Feed(new KBKeyPress('a'));
        processor.ProcessKeys();
        Assert.Equal(2, getCount());
    }
}
