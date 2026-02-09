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
/// Tests for Vi mode switch binding registration (T025/US3):
/// verifies all mode switch bindings exist (i, I, a, A, o, O, v, V, Ctrl-V,
/// R, r, Escape, Insert).
/// </summary>
public sealed class LoadViBindingsModeSwitchTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public LoadViBindingsModeSwitchTests()
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

    #region Mode Switch Bindings Exist

    [Fact]
    public void RegistersEscapeBinding()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.Escape));
            Assert.NotEmpty(bindings);
        }
    }

    [Theory]
    [InlineData('i')]
    [InlineData('I')]
    [InlineData('a')]
    [InlineData('A')]
    [InlineData('o')]
    [InlineData('O')]
    public void RegistersInsertModeEntry(char key)
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
    [InlineData('v')]
    [InlineData('V')]
    public void RegistersVisualModeEntry(char key)
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
    public void RegistersCtrlVVisualBlockEntry()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlV));
            Assert.NotEmpty(bindings);
        }
    }

    [Theory]
    [InlineData('r')]
    [InlineData('R')]
    public void RegistersReplaceMode(char key)
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
    public void RegistersInsertKeyToggle()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.Insert));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region Escape Has Multiple Handlers

    [Fact]
    public void EscapeHasBinding()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.Escape));
            Assert.True(bindings.Count >= 1,
                "Escape should have at least 1 binding (universal handler)");
        }
    }

    #endregion

    #region Insert Key Has Two Bindings

    [Fact]
    public void InsertKeyHasMultipleBindings()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.Insert));
            Assert.True(bindings.Count >= 2,
                "Insert key should have at least 2 bindings (navigation→insert and insert→navigation)");
        }
    }

    #endregion
}
