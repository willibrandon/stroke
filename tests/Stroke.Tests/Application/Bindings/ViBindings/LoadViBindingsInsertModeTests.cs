using Stroke.Core;
using Stroke.Input.Pipe;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;
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
/// Tests for Vi insert mode binding registration (T057):
/// verifies insert mode completion, quoted-insert, indent/unindent,
/// replace mode, replace-single, insert-multiple, digraph, and macro bindings.
/// </summary>
public sealed class LoadViBindingsInsertModeTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public LoadViBindingsInsertModeTests()
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
        var scope = AppContext.SetApp(app);

        return (buffer, app, scope);
    }

    private static IReadOnlyList<Binding> GetBindings(
        IKeyBindingsBase kb, params KeyOrChar[] keys)
    {
        return kb.GetBindingsForKeys(keys);
    }

    private static Binding? FindNamedCommandBinding(
        IKeyBindingsBase kb, string commandName, params KeyOrChar[] keys)
    {
        var bindings = kb.GetBindingsForKeys(keys);
        var expectedHandler = NamedCommands.GetByName(commandName).Handler;
        return bindings.FirstOrDefault(b => b.Handler == expectedHandler);
    }

    #region Quoted-Insert

    [Fact]
    public void RegistersCtrlVQuotedInsert()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var match = FindNamedCommandBinding(kb, "quoted-insert",
                new KeyOrChar(Keys.ControlV));
            Assert.NotNull(match);
        }
    }

    #endregion

    #region Completion Bindings

    [Fact]
    public void RegistersCtrlNCompletion()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlN));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersCtrlPCompletion()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlP));
            Assert.NotEmpty(bindings);
        }
    }

    [Theory]
    [InlineData(Keys.ControlG)]
    [InlineData(Keys.ControlY)]
    public void RegistersAcceptCompletion(Keys key)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(key));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersCtrlECancelCompletion()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlE));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region Indent/Unindent

    [Fact]
    public void RegistersCtrlTIndent()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlT));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersCtrlDUnindent()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlD));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region Line/File Completion Stubs

    [Fact]
    public void RegistersCtrlXCtrlLLineCompletion()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar(Keys.ControlX), new KeyOrChar(Keys.ControlL));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersCtrlXCtrlFFileCompletion()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar(Keys.ControlX), new KeyOrChar(Keys.ControlF));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region Replace Mode

    [Fact]
    public void RegistersReplaceAnyHandler()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.Any));
            // Multiple Any handlers exist for different modes
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region Insert-Multiple Mode

    [Fact]
    public void RegistersInsertMultipleBackspaceHandler()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            // Backspace is registered for insert-multiple mode
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlH));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersInsertMultipleDeleteHandler()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.Delete));
            Assert.NotEmpty(bindings);
        }
    }

    [Theory]
    [InlineData(Keys.Left)]
    [InlineData(Keys.Right)]
    [InlineData(Keys.Up)]
    [InlineData(Keys.Down)]
    public void RegistersInsertMultipleArrowHandler(Keys key)
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

    #region Digraph Mode

    [Fact]
    public void RegistersCtrlKDigraphEntry()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlK));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion
}
