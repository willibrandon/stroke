using Stroke.Application;
using Stroke.Application.Bindings;
using Stroke.Core;
using Stroke.Input.Pipe;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Windows;
using Stroke.Output;
using Xunit;
using AppContext = Stroke.Application.AppContext;
using Buffer = Stroke.Core.Buffer;
using Keys = Stroke.Input.Keys;

namespace Stroke.Tests.Application.Bindings;

/// <summary>
/// Tests for <see cref="OpenInEditorBindings"/> binding loaders.
/// </summary>
public sealed class OpenInEditorBindingsTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public OpenInEditorBindingsTests()
    {
        _input = new SimplePipeInput();
        _output = new DummyOutput();
    }

    public void Dispose()
    {
        _input.Dispose();
    }

    /// <summary>
    /// Creates a minimal environment with a buffer, layout, application, and AppContext scope.
    /// </summary>
    private (Buffer Buffer, Application<object> App, IDisposable Scope)
        CreateEnvironment(EditingMode editingMode = EditingMode.Emacs)
    {
        var buffer = new Buffer();
        var bc = new BufferControl(buffer: buffer);
        var window = new Window(content: bc);
        var container = new HSplit([window]);
        var layout = new Stroke.Layout.Layout(new AnyContainer(container));
        var app = new Application<object>(
            input: _input, output: _output, layout: layout, editingMode: editingMode);
        var scope = AppContext.SetApp(app.UnsafeCast);
        return (buffer, app, scope);
    }

    // ═════════════════════════════════════════════════════════════════════
    // Phase 2: User Story 1 — Emacs Open in Editor Bindings (T001-T006)
    // ═════════════════════════════════════════════════════════════════════

    #region Emacs Loader Tests (T001-T003)

    [Fact]
    public void LoadEmacs_ReturnsKeyBindingsWithOneBinding()
    {
        var kb = OpenInEditorBindings.LoadEmacsOpenInEditorBindings();

        Assert.NotNull(kb);
        Assert.Single(kb.Bindings);
    }

    [Fact]
    public void LoadEmacs_BindingHasCorrectKeySequence()
    {
        var kb = OpenInEditorBindings.LoadEmacsOpenInEditorBindings();
        var binding = kb.Bindings[0];

        Assert.Equal(2, binding.Keys.Count);
        Assert.True(binding.Keys[0].IsKey);
        Assert.Equal(Keys.ControlX, binding.Keys[0].Key);
        Assert.True(binding.Keys[1].IsKey);
        Assert.Equal(Keys.ControlE, binding.Keys[1].Key);
    }

    [Fact]
    public void LoadEmacs_BindingHandlerIsEditAndExecuteCommand()
    {
        var kb = OpenInEditorBindings.LoadEmacsOpenInEditorBindings();
        var binding = kb.Bindings[0];

        var expected = NamedCommands.GetByName("edit-and-execute-command").Handler;
        Assert.Same(expected, binding.Handler);
    }

    #endregion

    #region Emacs Filter Tests (T004-T006)

    [Fact]
    public void LoadEmacs_FilterActivatesInEmacsModeWithoutSelection()
    {
        var (_, _, scope) = CreateEnvironment(EditingMode.Emacs);
        using (scope)
        {
            var kb = OpenInEditorBindings.LoadEmacsOpenInEditorBindings();
            var binding = kb.Bindings[0];

            Assert.True(binding.Filter.Invoke());
        }
    }

    [Fact]
    public void LoadEmacs_FilterDeactivatesWhenSelectionActive()
    {
        var (buffer, _, scope) = CreateEnvironment(EditingMode.Emacs);
        using (scope)
        {
            // Activate selection on the buffer
            buffer.InsertText("hello");
            buffer.StartSelection();

            var kb = OpenInEditorBindings.LoadEmacsOpenInEditorBindings();
            var binding = kb.Bindings[0];

            Assert.False(binding.Filter.Invoke());
        }
    }

    [Fact]
    public void LoadEmacs_FilterDeactivatesInViMode()
    {
        var (_, _, scope) = CreateEnvironment(EditingMode.Vi);
        using (scope)
        {
            var kb = OpenInEditorBindings.LoadEmacsOpenInEditorBindings();
            var binding = kb.Bindings[0];

            Assert.False(binding.Filter.Invoke());
        }
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════
    // Phase 3: User Story 2 — Vi Open in Editor Bindings (T008-T012)
    // ═════════════════════════════════════════════════════════════════════

    #region Vi Loader Tests (T008-T010)

    [Fact]
    public void LoadVi_ReturnsKeyBindingsWithOneBinding()
    {
        var kb = OpenInEditorBindings.LoadViOpenInEditorBindings();

        Assert.NotNull(kb);
        Assert.Single(kb.Bindings);
    }

    [Fact]
    public void LoadVi_BindingHasCorrectKey()
    {
        var kb = OpenInEditorBindings.LoadViOpenInEditorBindings();
        var binding = kb.Bindings[0];

        Assert.Single(binding.Keys);
        Assert.True(binding.Keys[0].IsChar);
        Assert.Equal('v', binding.Keys[0].Char);
    }

    [Fact]
    public void LoadVi_BindingHandlerIsEditAndExecuteCommand()
    {
        var kb = OpenInEditorBindings.LoadViOpenInEditorBindings();
        var binding = kb.Bindings[0];

        var expected = NamedCommands.GetByName("edit-and-execute-command").Handler;
        Assert.Same(expected, binding.Handler);
    }

    #endregion

    #region Vi Filter Tests (T011-T012)

    [Fact]
    public void LoadVi_FilterActivatesInViNavigationMode()
    {
        var (_, app, scope) = CreateEnvironment(EditingMode.Vi);
        using (scope)
        {
            // Default Vi state is Insert mode; switch to Navigation
            app.ViState.InputMode = InputMode.Navigation;

            var kb = OpenInEditorBindings.LoadViOpenInEditorBindings();
            var binding = kb.Bindings[0];

            Assert.True(binding.Filter.Invoke());
        }
    }

    [Fact]
    public void LoadVi_FilterDeactivatesInViInsertMode()
    {
        var (_, app, scope) = CreateEnvironment(EditingMode.Vi);
        using (scope)
        {
            // Switch to insert mode
            app.ViState.InputMode = Stroke.KeyBinding.InputMode.Insert;

            var kb = OpenInEditorBindings.LoadViOpenInEditorBindings();
            var binding = kb.Bindings[0];

            Assert.False(binding.Filter.Invoke());
        }
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════
    // Phase 4: User Story 3 — Combined Loader (T014-T016)
    // ═════════════════════════════════════════════════════════════════════

    #region Combined Loader Tests (T014-T016)

    [Fact]
    public void LoadCombined_ReturnsMergedKeyBindings()
    {
        var result = OpenInEditorBindings.LoadOpenInEditorBindings();

        Assert.NotNull(result);
        Assert.IsType<MergedKeyBindings>(result);
    }

    [Fact]
    public void LoadCombined_ContainsTwoBindingsTotal()
    {
        var result = OpenInEditorBindings.LoadOpenInEditorBindings();

        Assert.Equal(2, result.Bindings.Count);
    }

    [Fact]
    public void LoadCombined_ContainsBothEmacsAndViBindings()
    {
        var result = OpenInEditorBindings.LoadOpenInEditorBindings();

        // One binding should be [ControlX, ControlE]
        var emacsBinding = result.Bindings.Single(b =>
            b.Keys.Count == 2
            && b.Keys[0].IsKey && b.Keys[0].Key == Keys.ControlX
            && b.Keys[1].IsKey && b.Keys[1].Key == Keys.ControlE);
        Assert.NotNull(emacsBinding);

        // One binding should be ['v']
        var viBinding = result.Bindings.Single(b =>
            b.Keys.Count == 1
            && b.Keys[0].IsChar && b.Keys[0].Char == 'v');
        Assert.NotNull(viBinding);
    }

    #endregion
}
