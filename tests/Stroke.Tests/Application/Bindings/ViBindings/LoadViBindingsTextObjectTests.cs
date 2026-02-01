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
/// Tests for Vi text object binding registration (T030/US4) and
/// character find handler behavior tests (T033/US5):
/// verifies all inner/around text objects and character find bindings exist.
/// </summary>
public sealed class LoadViBindingsTextObjectTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public LoadViBindingsTextObjectTests()
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

    #region Inner/Around Word Text Objects

    [Theory]
    [InlineData('w')]
    [InlineData('W')]
    public void RegistersInnerWordTextObject(char key)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('i'), new KeyOrChar(key));
            Assert.NotEmpty(bindings);
        }
    }

    [Theory]
    [InlineData('w')]
    [InlineData('W')]
    [InlineData('p')]
    public void RegistersAroundTextObject(char key)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('a'), new KeyOrChar(key));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region Quote Text Objects

    [Theory]
    [InlineData('"')]
    [InlineData('\'')]
    [InlineData('`')]
    public void RegistersInnerQuoteTextObject(char quote)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('i'), new KeyOrChar(quote));
            Assert.NotEmpty(bindings);
        }
    }

    [Theory]
    [InlineData('"')]
    [InlineData('\'')]
    [InlineData('`')]
    public void RegistersAroundQuoteTextObject(char quote)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('a'), new KeyOrChar(quote));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region Bracket Text Objects

    [Theory]
    [InlineData('(')]
    [InlineData(')')]
    [InlineData('[')]
    [InlineData(']')]
    [InlineData('{')]
    [InlineData('}')]
    [InlineData('<')]
    [InlineData('>')]
    public void RegistersInnerBracketTextObject(char bracket)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('i'), new KeyOrChar(bracket));
            Assert.NotEmpty(bindings);
        }
    }

    [Theory]
    [InlineData('(')]
    [InlineData(')')]
    [InlineData('[')]
    [InlineData(']')]
    [InlineData('{')]
    [InlineData('}')]
    [InlineData('<')]
    [InlineData('>')]
    public void RegistersAroundBracketTextObject(char bracket)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('a'), new KeyOrChar(bracket));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region Bracket Aliases

    [Theory]
    [InlineData('b')]
    [InlineData('B')]
    public void RegistersInnerBracketAlias(char alias)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('i'), new KeyOrChar(alias));
            Assert.NotEmpty(bindings);
        }
    }

    [Theory]
    [InlineData('b')]
    [InlineData('B')]
    public void RegistersAroundBracketAlias(char alias)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('a'), new KeyOrChar(alias));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region Character Find Bindings (US5)

    [Theory]
    [InlineData('f')]
    [InlineData('F')]
    [InlineData('t')]
    [InlineData('T')]
    public void RegistersCharacterFindBinding(char key)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar(key), new KeyOrChar(Keys.Any));
            Assert.NotEmpty(bindings);
        }
    }

    [Theory]
    [InlineData(';')]
    [InlineData(',')]
    public void RegistersRepeatFindBinding(char key)
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
}
