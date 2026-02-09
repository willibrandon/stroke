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
/// Tests for Vi navigation binding registration (T013/US1):
/// verifies LoadViBindings() returns ConditionalKeyBindings and that
/// all navigation keys are registered.
/// </summary>
public sealed class LoadViBindingsNavigationTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public LoadViBindingsNavigationTests()
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

    #region Loader Return Type

    [Fact]
    public void LoadViBindings_ReturnsConditionalKeyBindings()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var result = ViBindingsType.LoadViBindings();
            Assert.IsType<ConditionalKeyBindings>(result);
        }
    }

    #endregion

    #region Word Motions

    [Theory]
    [InlineData('w')]
    [InlineData('W')]
    [InlineData('b')]
    [InlineData('B')]
    [InlineData('e')]
    [InlineData('E')]
    public void RegistersWordMotion(char key)
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
    [InlineData('e')]
    [InlineData('E')]
    public void RegistersGeMotion(char secondKey)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('g'), new KeyOrChar(secondKey));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region Line Motions

    [Theory]
    [InlineData('0')]
    [InlineData('^')]
    [InlineData('$')]
    [InlineData('|')]
    public void RegistersLineMotion(char key)
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
    [InlineData('m')]
    [InlineData('_')]
    public void RegistersGPrefixLineMotion(char secondKey)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('g'), new KeyOrChar(secondKey));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region Document/Screen Motions

    [Theory]
    [InlineData('G')]
    [InlineData('{')]
    [InlineData('}')]
    [InlineData('%')]
    [InlineData('H')]
    [InlineData('M')]
    [InlineData('L')]
    public void RegistersDocumentScreenMotion(char key)
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
    public void RegistersGgMotion()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('g'), new KeyOrChar('g'));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region Cursor Motions (h, l, Space, arrows)

    [Theory]
    [InlineData('h')]
    [InlineData('l')]
    [InlineData(' ')]
    public void RegistersCursorMotion(char key)
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
    [InlineData(Keys.Left)]
    [InlineData(Keys.Right)]
    [InlineData(Keys.Up)]
    [InlineData(Keys.Down)]
    public void RegistersArrowKeyBinding(Keys key)
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

    #region Vertical Navigation

    [Theory]
    [InlineData('j')]
    [InlineData('k')]
    public void RegistersVerticalNavigation(char key)
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
    [InlineData(Keys.ControlN)]
    [InlineData(Keys.ControlP)]
    public void RegistersControlVerticalNavigation(Keys key)
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

    #region Misc Navigation

    [Fact]
    public void RegistersBackspaceBinding()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlH));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersEnterBinding()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlM));
            Assert.NotEmpty(bindings);
        }
    }

    [Theory]
    [InlineData('+')]
    [InlineData('-')]
    public void RegistersPlusMinusBinding(char key)
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
    [InlineData('(')]
    [InlineData(')')]
    public void RegistersSentenceStubBinding(char key)
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

    #region Search Motions (n, N, #, *)

    [Theory]
    [InlineData('n')]
    [InlineData('N')]
    public void RegistersSearchMotion(char key)
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
    [InlineData('#')]
    [InlineData('*')]
    public void RegistersWordSearchBinding(char key)
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
