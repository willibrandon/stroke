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
/// Tests for Vi operator binding registration (T022/US2):
/// verifies operator bindings exist and doubled-key operators are registered.
/// </summary>
public sealed class LoadViBindingsOperatorTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public LoadViBindingsOperatorTests()
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

    #region Standard Operators

    [Theory]
    [InlineData('d')]
    [InlineData('c')]
    [InlineData('y')]
    [InlineData('>')]
    [InlineData('<')]
    public void RegistersSingleKeyOperator(char key)
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
    public void RegistersReshapeOperator()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('g'), new KeyOrChar('q'));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region Transform Operators

    [Theory]
    [InlineData('?')]
    [InlineData('u')]
    [InlineData('U')]
    [InlineData('~')]
    public void RegistersGPrefixTransformOperator(char secondKey)
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

    [Fact]
    public void RegistersTildeOperator()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar('~'));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region Register-Aware Operators

    [Theory]
    [InlineData('d')]
    [InlineData('c')]
    [InlineData('y')]
    public void RegistersRegisterAwareOperator(char op)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('"'), new KeyOrChar(Keys.Any), new KeyOrChar(op));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region Doubled-Key Operators

    [Fact]
    public void RegistersDdBinding()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('d'), new KeyOrChar('d'));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersYyBinding()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('y'), new KeyOrChar('y'));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersCcBinding()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('c'), new KeyOrChar('c'));
            Assert.NotEmpty(bindings);
        }
    }

    [Theory]
    [InlineData('Y')]
    [InlineData('S')]
    [InlineData('C')]
    [InlineData('D')]
    public void RegistersDoubledKeyAlias(char key)
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

    #region Indent/Unindent Doubled Keys

    [Fact]
    public void RegistersDoubleGreaterThan()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('>'), new KeyOrChar('>'));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersDoubleLessThan()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('<'), new KeyOrChar('<'));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region Doubled-Key Transforms

    [Fact]
    public void RegistersGuuBinding()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('g'), new KeyOrChar('u'), new KeyOrChar('u'));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersGUUBinding()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('g'), new KeyOrChar('U'), new KeyOrChar('U'));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersGTildeTildeBinding()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('g'), new KeyOrChar('~'), new KeyOrChar('~'));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion
}
