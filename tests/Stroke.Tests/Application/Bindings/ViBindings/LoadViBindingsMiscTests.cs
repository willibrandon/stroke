using Stroke.Core;
using Stroke.Input.Pipe;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Output;
using Xunit;
using AppContext = Stroke.Application.AppContext;
using Buffer = Stroke.Core.Buffer;
using Keys = Stroke.Input.Keys;
using ViBindingsType = Stroke.Application.Bindings.ViBindings;

namespace Stroke.Tests.Application.Bindings.ViBindings;

/// <summary>
/// Tests for Vi miscellaneous binding registration:
/// T034/US6 (paste, undo, redo, registers),
/// T040/US8 (macros),
/// T048/US10 (misc commands: x, X, s, >>, <<, ~, guu, gUU, g~~, numeric args, Ctrl-A, Ctrl-X).
/// </summary>
public sealed class LoadViBindingsMiscTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public LoadViBindingsMiscTests()
    {
        _input = new SimplePipeInput();
        _output = new DummyOutput();
    }

    public void Dispose()
    {
        _input.Dispose();
    }

    private (Buffer Buffer, Stroke.Application.Application<object> App, IDisposable Scope)
        CreateEnvironment(
            string text = "",
            int cursorPosition = 0,
            bool multiline = false)
    {
        var buffer = new Buffer(
            document: new Document(text, cursorPosition: cursorPosition),
            multiline: multiline ? () => true : () => false);
        var bufferControl = new BufferControl(buffer: buffer);
        var window = new Window(content: bufferControl);
        var layout = new Stroke.Layout.Layout(new AnyContainer(window));
        var app = new Stroke.Application.Application<object>(
            input: _input, output: _output, layout: layout);
        var scope = AppContext.SetApp(app.UnsafeCast);

        return (buffer, app, scope);
    }

    private static IReadOnlyList<Binding> GetBindings(
        IKeyBindingsBase kb, params KeyOrChar[] keys)
    {
        return kb.GetBindingsForKeys(keys);
    }

    #region US6: Paste Bindings

    [Theory]
    [InlineData('p')]
    [InlineData('P')]
    public void RegistersPasteBinding(char key)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(key));
            Assert.NotEmpty(bindings);
        }
    }

    [Theory]
    [InlineData('p')]
    [InlineData('P')]
    public void RegistersRegisterPasteBinding(char pasteKey)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('"'), new KeyOrChar(Keys.Any),
                new KeyOrChar(pasteKey));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region US6: Undo/Redo Bindings

    [Fact]
    public void RegistersUndoBinding()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar('u'));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersRedoBinding()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlR));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region US8: Macro Bindings

    [Fact]
    public void RegistersMacroRecordStartBinding()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('q'), new KeyOrChar(Keys.Any));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersMacroRecordStopBinding()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar('q'));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersMacroPlaybackBinding()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('@'), new KeyOrChar(Keys.Any));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region US10: Single-Character Operations

    [Theory]
    [InlineData('x')]
    [InlineData('X')]
    [InlineData('s')]
    public void RegistersSingleCharOperation(char key)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(key));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region US10: Join Bindings

    [Fact]
    public void RegistersJoinBinding()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar('J'));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersGJoinBinding()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('g'), new KeyOrChar('J'));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region US10: Numeric Args

    [Theory]
    [InlineData('1')]
    [InlineData('5')]
    [InlineData('9')]
    public void RegistersNumericArgBinding(char digit)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(digit));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersZeroWithHasArgBinding()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            // 0 has bindings (both as motion and as numeric arg)
            var bindings = GetBindings(kb, new KeyOrChar('0'));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region US10: Increment/Decrement

    [Fact]
    public void RegistersCtrlAIncrement()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlA));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersCtrlXDecrement()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlX));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region US10: Scroll Z Commands

    [Theory]
    [InlineData('t')]
    [InlineData('+')]
    [InlineData('b')]
    [InlineData('-')]
    [InlineData('z')]
    public void RegistersZScrollBinding(char secondKey)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('z'), new KeyOrChar(secondKey));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersZEnterScrollBinding()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('z'), new KeyOrChar(Keys.ControlM));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region US10: Ctrl-O Temporary Navigation

    [Fact]
    public void RegistersCtrlOTempNavigation()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlO));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region US10: Unknown Text Object Catch-All

    [Fact]
    public void RegistersCatchAllBinding()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.Any));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion
}
