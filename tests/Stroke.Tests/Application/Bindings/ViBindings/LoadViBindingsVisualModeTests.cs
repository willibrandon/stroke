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
/// Tests for Vi visual mode binding registration (T042/US9):
/// verifies all visual mode bindings exist (j, k, x, J, g,J, v, V, Ctrl-V,
/// I, A, a,w, a,W).
/// </summary>
public sealed class LoadViBindingsVisualModeTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public LoadViBindingsVisualModeTests()
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

    #region Visual Mode Selection Extension

    [Theory]
    [InlineData('j')]
    [InlineData('k')]
    public void RegistersVisualModeSelectionExtend(char key)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(key));
            // These keys have bindings in both navigation and selection modes
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region Visual Mode Operations

    [Fact]
    public void RegistersVisualXCut()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            // x has bindings in both navigation and selection modes
            var bindings = GetBindings(kb, new KeyOrChar('x'));
            Assert.True(bindings.Count >= 2,
                "x should have at least 2 bindings (navigation delete + selection cut)");
        }
    }

    [Fact]
    public void RegistersVisualJJoin()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            // J has bindings in both navigation and selection modes
            var bindings = GetBindings(kb, new KeyOrChar('J'));
            Assert.True(bindings.Count >= 2,
                "J should have at least 2 bindings (navigation join + selection join)");
        }
    }

    [Fact]
    public void RegistersVisualGJJoin()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('g'), new KeyOrChar('J'));
            Assert.True(bindings.Count >= 2,
                "g,J should have at least 2 bindings (navigation + selection)");
        }
    }

    #endregion

    #region Visual Mode Toggle

    [Fact]
    public void RegistersVisualVToggle()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            // v has bindings in both navigation (enter visual) and selection (toggle)
            var bindings = GetBindings(kb, new KeyOrChar('v'));
            Assert.True(bindings.Count >= 2,
                "v should have at least 2 bindings (enter visual + toggle)");
        }
    }

    [Fact]
    public void RegistersVisualCapVToggle()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar('V'));
            Assert.True(bindings.Count >= 2,
                "V should have at least 2 bindings (enter visual-line + toggle)");
        }
    }

    [Fact]
    public void RegistersVisualCtrlVToggle()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlV));
            Assert.True(bindings.Count >= 2,
                "Ctrl-V should have at least 2 bindings (enter visual-block + toggle)");
        }
    }

    #endregion

    #region Block Selection Insert/Append

    [Fact]
    public void RegistersBlockSelectionI()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            // I has bindings in navigation mode and block selection mode
            var bindings = GetBindings(kb, new KeyOrChar('I'));
            Assert.True(bindings.Count >= 2,
                "I should have at least 2 bindings (insert mode + block selection)");
        }
    }

    [Fact]
    public void RegistersBlockSelectionA()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar('A'));
            Assert.True(bindings.Count >= 2,
                "A should have at least 2 bindings (insert mode + block selection)");
        }
    }

    #endregion

    #region Visual Mode Auto-Word

    [Fact]
    public void RegistersVisualAwBinding()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('a'), new KeyOrChar('w'));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersVisualAWBinding()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb,
                new KeyOrChar('a'), new KeyOrChar('W'));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion
}
