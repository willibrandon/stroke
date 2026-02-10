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
/// Integration tests for Vi bindings (T062):
/// verifies total binding count, ConditionalKeyBindings wrapper,
/// and that all partial registration methods contribute bindings.
/// </summary>
public sealed class ViBindingsIntegrationTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public ViBindingsIntegrationTests()
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

    [Fact]
    public void LoadViBindings_HasSubstantialBindingCount()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            // Vi bindings should have a large number of registrations
            // (operators, text objects, navigation, mode switch, insert, visual, misc)
            Assert.True(kb.Bindings.Count > 100,
                $"Expected > 100 Vi bindings, got {kb.Bindings.Count}");
        }
    }

    [Fact]
    public void LoadViBindings_ContainsOperatorBindings()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            // d has navigation mode (set pending) and selection mode (execute)
            var dBindings = kb.GetBindingsForKeys([new KeyOrChar('d')]);
            Assert.True(dBindings.Count >= 2,
                "d should have at least 2 bindings (navigation + selection)");
        }
    }

    [Fact]
    public void LoadViBindings_ContainsTextObjectBindings()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            // w has operator-pending, navigation, and selection modes
            var wBindings = kb.GetBindingsForKeys([new KeyOrChar('w')]);
            Assert.True(wBindings.Count >= 3,
                "w should have at least 3 bindings (operator-pending + navigation + selection)");
        }
    }

    [Fact]
    public void LoadViBindings_ContainsModeSwitchBindings()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var escBindings = kb.GetBindingsForKeys([new KeyOrChar(Keys.Escape)]);
            Assert.NotEmpty(escBindings);
        }
    }

    [Fact]
    public void LoadViBindings_ContainsInsertModeBindings()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            // Ctrl-K (digraph entry) is an insert-mode-only binding
            var ctrlKBindings = kb.GetBindingsForKeys([new KeyOrChar(Keys.ControlK)]);
            Assert.NotEmpty(ctrlKBindings);
        }
    }

    [Fact]
    public void LoadViBindings_ContainsVisualModeBindings()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            // v has navigation-mode (enter visual) and selection-mode (toggle) bindings
            var vBindings = kb.GetBindingsForKeys([new KeyOrChar('v')]);
            Assert.True(vBindings.Count >= 2,
                "v should have at least 2 bindings from mode switch + visual toggle");
        }
    }

    [Fact]
    public void LoadViBindings_ContainsMiscBindings()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            // dd is a misc binding
            var ddBindings = kb.GetBindingsForKeys(
                [new KeyOrChar('d'), new KeyOrChar('d')]);
            Assert.NotEmpty(ddBindings);
        }
    }

    [Fact]
    public void LoadViBindings_MultipleCallsProduceIndependentInstances()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb1 = ViBindingsType.LoadViBindings();
            var kb2 = ViBindingsType.LoadViBindings();
            Assert.NotSame(kb1, kb2);
        }
    }

    [Fact]
    public void LoadViBindings_AllNumericDigitsRegistered()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            for (int n = 0; n <= 9; n++)
            {
                var digit = (char)('0' + n);
                var bindings = kb.GetBindingsForKeys([new KeyOrChar(digit)]);
                Assert.NotEmpty(bindings);
            }
        }
    }

    [Fact]
    public void LoadViBindings_AllBracketTextObjectsRegistered()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            foreach (var bracket in new[] { '(', ')', '[', ']', '{', '}', '<', '>' })
            {
                var iBindings = kb.GetBindingsForKeys(
                    [new KeyOrChar('i'), new KeyOrChar(bracket)]);
                Assert.NotEmpty(iBindings);

                var aBindings = kb.GetBindingsForKeys(
                    [new KeyOrChar('a'), new KeyOrChar(bracket)]);
                Assert.NotEmpty(aBindings);
            }
        }
    }
}
