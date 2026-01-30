using Stroke.Application;
using Stroke.Core;
using Stroke.Input.Pipe;
using Stroke.KeyBinding;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Output;
using Xunit;
using AppContext = Stroke.Application.AppContext;
using Buffer = Stroke.Core.Buffer;
using StrokeLayout = Stroke.Layout.Layout;

namespace Stroke.Tests.Application;

/// <summary>
/// Tests for EmacsFilters static class (User Story 4).
/// </summary>
public class EmacsFiltersTests
{
    private static (Application<object?> app, IDisposable scope) CreateEmacsApp(
        Buffer? buffer = null)
    {
        var input = new SimplePipeInput();
        var output = new DummyOutput();
        var buf = buffer ?? new Buffer();
        var control = new BufferControl(buffer: buf);
        var window = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(window));
        var app = new Application<object?>(
            layout: layout, input: input, output: output,
            editingMode: EditingMode.Emacs);
        var scope = AppContext.SetApp(app.UnsafeCast);
        return (app, scope);
    }

    // ═══════════════════════════════════════════════════════════════════
    // EmacsMode
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void EmacsMode_TrueInEmacsEditingMode()
    {
        var (app, scope) = CreateEmacsApp();
        using (scope)
        {
            Assert.True(EmacsFilters.EmacsMode.Invoke());
        }
    }

    [Fact]
    public void EmacsMode_FalseInViEditingMode()
    {
        using var input = new SimplePipeInput();
        var app = new Application<object?>(
            input: input, output: new DummyOutput(),
            editingMode: EditingMode.Vi);
        using var scope = AppContext.SetApp(app.UnsafeCast);

        Assert.False(EmacsFilters.EmacsMode.Invoke());
    }

    // ═══════════════════════════════════════════════════════════════════
    // EmacsInsertMode
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void EmacsInsertMode_TrueWithNoSelectionAndWritable()
    {
        var (app, scope) = CreateEmacsApp();
        using (scope)
        {
            Assert.True(EmacsFilters.EmacsInsertMode.Invoke());
        }
    }

    [Fact]
    public void EmacsInsertMode_FalseWithSelectionActive()
    {
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));
        var (app, scope) = CreateEmacsApp(buffer);
        using (scope)
        {
            buffer.StartSelection();
            Assert.False(EmacsFilters.EmacsInsertMode.Invoke());
        }
    }

    [Fact]
    public void EmacsInsertMode_FalseWithReadOnlyBuffer()
    {
        var buffer = new Buffer(readOnly: () => true);
        var (app, scope) = CreateEmacsApp(buffer);
        using (scope)
        {
            Assert.False(EmacsFilters.EmacsInsertMode.Invoke());
        }
    }

    [Fact]
    public void EmacsInsertMode_FalseInViMode()
    {
        using var input = new SimplePipeInput();
        var app = new Application<object?>(
            input: input, output: new DummyOutput(),
            editingMode: EditingMode.Vi);
        using var scope = AppContext.SetApp(app.UnsafeCast);

        Assert.False(EmacsFilters.EmacsInsertMode.Invoke());
    }

    // ═══════════════════════════════════════════════════════════════════
    // EmacsSelectionMode
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void EmacsSelectionMode_TrueWithSelection()
    {
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));
        var (app, scope) = CreateEmacsApp(buffer);
        using (scope)
        {
            buffer.StartSelection();
            Assert.True(EmacsFilters.EmacsSelectionMode.Invoke());
        }
    }

    [Fact]
    public void EmacsSelectionMode_FalseWithoutSelection()
    {
        var (app, scope) = CreateEmacsApp();
        using (scope)
        {
            Assert.False(EmacsFilters.EmacsSelectionMode.Invoke());
        }
    }

    [Fact]
    public void EmacsSelectionMode_FalseInViMode()
    {
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));
        var control = new BufferControl(buffer: buffer);
        var window = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(window));

        using var input = new SimplePipeInput();
        var app = new Application<object?>(
            layout: layout, input: input, output: new DummyOutput(),
            editingMode: EditingMode.Vi);
        using var scope = AppContext.SetApp(app.UnsafeCast);

        buffer.StartSelection();
        Assert.False(EmacsFilters.EmacsSelectionMode.Invoke());
    }

    // ═══════════════════════════════════════════════════════════════════
    // DummyApplication behavior (SC-002)
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void DummyApplication_EmacsModeReturnsTrue()
    {
        // DummyApplication defaults to Emacs editing mode
        Assert.True(EmacsFilters.EmacsMode.Invoke());
    }

    [Fact]
    public void DummyApplication_EmacsInsertModeReturnsTrue()
    {
        // DummyApplication: Emacs mode, no selection, not read-only
        Assert.True(EmacsFilters.EmacsInsertMode.Invoke());
    }

    [Fact]
    public void DummyApplication_EmacsSelectionModeReturnsFalse()
    {
        // DummyApplication: no selection
        Assert.False(EmacsFilters.EmacsSelectionMode.Invoke());
    }
}
