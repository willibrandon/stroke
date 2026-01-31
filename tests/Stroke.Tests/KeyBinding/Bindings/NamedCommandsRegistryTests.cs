using Stroke.Core;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;
using Xunit;
using Buffer = Stroke.Core.Buffer;
using Keys = Stroke.Input.Keys;

namespace Stroke.Tests.KeyBinding.Bindings;

/// <summary>
/// Tests for the NamedCommands static registry: GetByName, Register, error handling.
/// </summary>
public sealed class NamedCommandsRegistryTests
{
    private static KeyPressEvent CreateEvent(Buffer buffer)
    {
        return new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: [new KeyPress(Keys.Any)],
            previousKeySequence: [],
            isRepeat: false,
            app: null,
            currentBuffer: buffer);
    }

    [Fact]
    public void GetByName_ReturnsValidBinding_ForKnownCommand()
    {
        var binding = NamedCommands.GetByName("forward-char");
        Assert.NotNull(binding);
    }

    [Fact]
    public void GetByName_ThrowsKeyNotFoundException_ForUnknownCommand()
    {
        var ex = Assert.Throws<KeyNotFoundException>(
            () => NamedCommands.GetByName("nonexistent-command"));
        Assert.Contains("Unknown Readline command: 'nonexistent-command'", ex.Message);
    }

    [Fact]
    public void GetByName_ThrowsArgumentNullException_ForNull()
    {
        Assert.Throws<ArgumentNullException>(() => NamedCommands.GetByName(null!));
    }

    [Fact]
    public void GetByName_ThrowsKeyNotFoundException_ForEmptyString()
    {
        var ex = Assert.Throws<KeyNotFoundException>(
            () => NamedCommands.GetByName(""));
        Assert.Contains("Unknown Readline command: ''", ex.Message);
    }

    [Fact]
    public void Register_AddsCustomCommand_RetrievableViaGetByName()
    {
        NotImplementedOrNone? Handler(KeyPressEvent e) => null;

        NamedCommands.Register("test-custom-registry-cmd", Handler);
        var binding = NamedCommands.GetByName("test-custom-registry-cmd");
        Assert.NotNull(binding);
    }

    [Fact]
    public void Register_ReplacesExistingBuiltInCommand()
    {
        var originalBinding = NamedCommands.GetByName("forward-char");

        NotImplementedOrNone? CustomHandler(KeyPressEvent e) => null;
        NamedCommands.Register("forward-char", CustomHandler);

        var newBinding = NamedCommands.GetByName("forward-char");
        Assert.NotSame(originalBinding, newBinding);

        // Restore the original (other tests depend on it)
        NamedCommands.Register("forward-char", originalBinding.Handler);
    }

    [Fact]
    public void Register_ThrowsArgumentNullException_ForNullName()
    {
        NotImplementedOrNone? Handler(KeyPressEvent e) => null;
        Assert.Throws<ArgumentNullException>(() => NamedCommands.Register(null!, Handler));
    }

    [Fact]
    public void Register_ThrowsArgumentNullException_ForNullHandler()
    {
        Assert.Throws<ArgumentNullException>(() => NamedCommands.Register("test", null!));
    }

    [Fact]
    public void Register_ThrowsArgumentException_ForEmptyName()
    {
        NotImplementedOrNone? Handler(KeyPressEvent e) => null;
        Assert.Throws<ArgumentException>(() => NamedCommands.Register("", Handler));
    }

    [Fact]
    public void Register_ThrowsArgumentException_ForWhitespaceName()
    {
        NotImplementedOrNone? Handler(KeyPressEvent e) => null;
        Assert.Throws<ArgumentException>(() => NamedCommands.Register("  ", Handler));
    }

    [Fact]
    public void ConcurrentStressTest_NoExceptionsOrDataCorruption()
    {
        // Constitution XI: spawn 10+ threads performing simultaneous GetByName and Register
        // calls (1000+ operations total), verify no exceptions or data corruption.
        const int threadCount = 12;
        const int opsPerThread = 100;
        var exceptions = new List<Exception>();
        var lockObj = new object();

        var threads = new Thread[threadCount];
        for (var t = 0; t < threadCount; t++)
        {
            var threadIndex = t;
            threads[t] = new Thread(() =>
            {
                try
                {
                    for (var i = 0; i < opsPerThread; i++)
                    {
                        if (i % 2 == 0)
                        {
                            // GetByName for a known command
                            var binding = NamedCommands.GetByName("forward-char");
                            Assert.NotNull(binding);
                        }
                        else
                        {
                            // Register a thread-specific command
                            NotImplementedOrNone? H(KeyPressEvent e) => null;
                            NamedCommands.Register($"stress-test-{threadIndex}-{i}", H);
                        }
                    }
                }
                catch (Exception ex)
                {
                    lock (lockObj)
                    {
                        exceptions.Add(ex);
                    }
                }
            });
        }

        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.Empty(exceptions);

        // Verify commands registered during stress test are retrievable
        for (var t = 0; t < threadCount; t++)
        {
            for (var i = 1; i < opsPerThread; i += 2)
            {
                var binding = NamedCommands.GetByName($"stress-test-{t}-{i}");
                Assert.NotNull(binding);
            }
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // T010: Custom command registration behavioral tests
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void CustomCommand_InsertsTextWhenInvoked()
    {
        NotImplementedOrNone? Handler(KeyPressEvent e)
        {
            e.CurrentBuffer!.InsertText("CUSTOM");
            return null;
        }

        NamedCommands.Register("test-behavioral-custom", Handler);
        var buffer = new Buffer(document: new Document("", cursorPosition: 0));
        var binding = NamedCommands.GetByName("test-behavioral-custom");
        binding.Call(CreateEvent(buffer));

        Assert.Equal("CUSTOM", buffer.Document.Text);
    }

    [Fact]
    public void CustomCommand_OverridesBuiltIn_ExecutesNewBehavior()
    {
        // Save original
        var original = NamedCommands.GetByName("backward-char");

        NotImplementedOrNone? CustomHandler(KeyPressEvent e)
        {
            e.CurrentBuffer!.InsertText("OVERRIDE");
            return null;
        }

        NamedCommands.Register("backward-char", CustomHandler);
        var buffer = new Buffer(document: new Document("", cursorPosition: 0));
        var binding = NamedCommands.GetByName("backward-char");
        binding.Call(CreateEvent(buffer));

        Assert.Equal("OVERRIDE", buffer.Document.Text);

        // Restore original
        NamedCommands.Register("backward-char", original.Handler);
    }

    [Fact]
    public void Register_WithRecordInMacroFalse_BindingReflectsIt()
    {
        NotImplementedOrNone? Handler(KeyPressEvent e) => null;
        NamedCommands.Register("test-no-macro-record", Handler, recordInMacro: false);

        var binding = NamedCommands.GetByName("test-no-macro-record");
        Assert.False(binding.RecordInMacro.Invoke());
    }

    [Fact]
    public void Register_WithRecordInMacroTrue_BindingReflectsIt()
    {
        NotImplementedOrNone? Handler(KeyPressEvent e) => null;
        NamedCommands.Register("test-yes-macro-record", Handler, recordInMacro: true);

        var binding = NamedCommands.GetByName("test-yes-macro-record");
        Assert.True(binding.RecordInMacro.Invoke());
    }
}
