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
using EmacsBindingsType = Stroke.Application.Bindings.EmacsBindings;
using Keys = Stroke.Input.Keys;

namespace Stroke.Tests.Application.Bindings.EmacsBindings;

/// <summary>
/// Tests for EmacsBindings.LoadEmacsShiftSelectionBindings() registration:
/// verifies that all 34 shift-selection bindings (10 start-selection,
/// 10 extend-selection, 4 replace/cancel, 10 cancel-movement) are registered.
/// </summary>
public sealed class LoadEmacsShiftSelectionBindingsTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public LoadEmacsShiftSelectionBindingsTests()
    {
        _input = new SimplePipeInput();
        _output = new DummyOutput();
    }

    public void Dispose()
    {
        _input.Dispose();
    }

    #region Test Environment Setup

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
        app.EditingMode = Stroke.KeyBinding.EditingMode.Emacs;
        var scope = AppContext.SetApp(app.UnsafeCast);

        return (buffer, app, scope);
    }

    private static IReadOnlyList<Binding> GetBindings(
        IKeyBindingsBase kb, params KeyOrChar[] keys)
    {
        return kb.GetBindingsForKeys(keys);
    }

    #endregion

    #region Loader Return Type

    [Fact]
    public void LoadEmacsShiftSelectionBindings_ReturnsConditionalKeyBindings()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var result = EmacsBindingsType.LoadEmacsShiftSelectionBindings();
            Assert.IsType<ConditionalKeyBindings>(result);
        }
    }

    [Fact]
    public void LoadEmacsShiftSelectionBindings_Has34Bindings()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsShiftSelectionBindings();
            Assert.Equal(34, kb.Bindings.Count);
        }
    }

    #endregion

    #region Start-Selection Bindings (10 shift keys, filter: ~has_selection)

    [Theory]
    [InlineData(Keys.ShiftLeft)]
    [InlineData(Keys.ShiftRight)]
    [InlineData(Keys.ShiftUp)]
    [InlineData(Keys.ShiftDown)]
    [InlineData(Keys.ShiftHome)]
    [InlineData(Keys.ShiftEnd)]
    [InlineData(Keys.ControlShiftLeft)]
    [InlineData(Keys.ControlShiftRight)]
    [InlineData(Keys.ControlShiftHome)]
    [InlineData(Keys.ControlShiftEnd)]
    public void RegistersStartSelectionBinding_ForShiftKey(Keys key)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsShiftSelectionBindings();
            var bindings = GetBindings(kb, new KeyOrChar(key));
            // Each shift key should have at least 2 bindings: start-selection + extend-selection
            Assert.True(bindings.Count >= 2,
                $"Expected at least 2 bindings for {key} (start + extend), found {bindings.Count}");
        }
    }

    #endregion

    #region Extend-Selection Bindings (10 shift keys, filter: shift_selection_mode)

    [Theory]
    [InlineData(Keys.ShiftLeft)]
    [InlineData(Keys.ShiftRight)]
    [InlineData(Keys.ShiftUp)]
    [InlineData(Keys.ShiftDown)]
    [InlineData(Keys.ShiftHome)]
    [InlineData(Keys.ShiftEnd)]
    [InlineData(Keys.ControlShiftLeft)]
    [InlineData(Keys.ControlShiftRight)]
    [InlineData(Keys.ControlShiftHome)]
    [InlineData(Keys.ControlShiftEnd)]
    public void RegistersExtendSelectionBinding_ForShiftKey(Keys key)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsShiftSelectionBindings();
            var bindings = GetBindings(kb, new KeyOrChar(key));
            // Verify both start and extend are registered (at least 2 bindings per shift key)
            Assert.True(bindings.Count >= 2,
                $"Expected at least 2 bindings for {key}, found {bindings.Count}");
        }
    }

    #endregion

    #region Replace/Cancel Bindings (4 bindings, filter: shift_selection_mode)

    [Fact]
    public void RegistersAny_ShiftReplaceSelection()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsShiftSelectionBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.Any));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersEnter_ShiftNewline()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsShiftSelectionBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlM));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersBackspace_ShiftDelete()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsShiftSelectionBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlH));
            Assert.NotEmpty(bindings);
        }
    }

    [Fact]
    public void RegistersCtrlY_ShiftYank()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsShiftSelectionBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlY));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region Cancel-Movement Bindings (10 keys, filter: shift_selection_mode)

    [Theory]
    [InlineData(Keys.Left)]
    [InlineData(Keys.Right)]
    [InlineData(Keys.Up)]
    [InlineData(Keys.Down)]
    [InlineData(Keys.Home)]
    [InlineData(Keys.End)]
    [InlineData(Keys.ControlLeft)]
    [InlineData(Keys.ControlRight)]
    [InlineData(Keys.ControlHome)]
    [InlineData(Keys.ControlEnd)]
    public void RegistersCancelMovementBinding(Keys key)
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsShiftSelectionBindings();
            var bindings = GetBindings(kb, new KeyOrChar(key));
            Assert.NotEmpty(bindings);
        }
    }

    #endregion

    #region Binding Group Counts

    [Fact]
    public void ShiftKeys_EachHaveAtLeastTwoSpecificBindings()
    {
        // Each shift key has a start-selection + extend-selection binding.
        // GetBindingsForKeys may also return the Any wildcard binding,
        // so we assert >= 2 (start + extend) plus possibly Any.
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsShiftSelectionBindings();

            Keys[] shiftKeys =
            [
                Keys.ShiftLeft, Keys.ShiftRight,
                Keys.ShiftUp, Keys.ShiftDown,
                Keys.ShiftHome, Keys.ShiftEnd,
                Keys.ControlShiftLeft, Keys.ControlShiftRight,
                Keys.ControlShiftHome, Keys.ControlShiftEnd,
            ];

            foreach (var key in shiftKeys)
            {
                var bindings = GetBindings(kb, new KeyOrChar(key));
                // At least 2 specific bindings (start + extend); Any wildcard adds a third
                Assert.True(bindings.Count >= 2,
                    $"Expected at least 2 bindings for {key}, found {bindings.Count}");
                // Verify at least 2 bindings have the exact key (not Any)
                var specificBindings = bindings.Count(b =>
                    b.Keys.Count == 1 && b.Keys[0].IsKey && b.Keys[0].Key == key);
                Assert.True(specificBindings >= 2,
                    $"Expected at least 2 specific (non-Any) bindings for {key}, found {specificBindings}");
            }
        }
    }

    [Fact]
    public void CancelMovementKeys_EachHaveAtLeastOneSpecificBinding()
    {
        // Each cancel-movement key has one specific binding.
        // GetBindingsForKeys may also return the Any wildcard binding.
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = EmacsBindingsType.LoadEmacsShiftSelectionBindings();

            Keys[] cancelKeys =
            [
                Keys.Left, Keys.Right,
                Keys.Up, Keys.Down,
                Keys.Home, Keys.End,
                Keys.ControlLeft, Keys.ControlRight,
                Keys.ControlHome, Keys.ControlEnd,
            ];

            foreach (var key in cancelKeys)
            {
                var bindings = GetBindings(kb, new KeyOrChar(key));
                Assert.NotEmpty(bindings);
                // Verify at least 1 binding with the exact key
                var specificBindings = bindings.Count(b =>
                    b.Keys.Count == 1 && b.Keys[0].IsKey && b.Keys[0].Key == key);
                Assert.True(specificBindings >= 1,
                    $"Expected at least 1 specific binding for {key}, found {specificBindings}");
            }
        }
    }

    #endregion
}
