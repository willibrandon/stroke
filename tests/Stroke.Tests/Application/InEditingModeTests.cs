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
/// Tests for AppFilters.InEditingMode factory method (User Story 6).
/// </summary>
public class InEditingModeTests
{
    // ═══════════════════════════════════════════════════════════════════
    // Value correctness
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void InEditingMode_Vi_TrueInViMode()
    {
        var buffer = new Buffer();
        var control = new BufferControl(buffer: buffer);
        var window = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(window));

        using var input = new SimplePipeInput();
        var app = new Application<object?>(
            layout: layout, input: input, output: new DummyOutput(),
            editingMode: EditingMode.Vi);
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var filter = AppFilters.InEditingMode(EditingMode.Vi);
        Assert.True(filter.Invoke());
    }

    [Fact]
    public void InEditingMode_Emacs_FalseInViMode()
    {
        var buffer = new Buffer();
        var control = new BufferControl(buffer: buffer);
        var window = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(window));

        using var input = new SimplePipeInput();
        var app = new Application<object?>(
            layout: layout, input: input, output: new DummyOutput(),
            editingMode: EditingMode.Vi);
        using var scope = AppContext.SetApp(app.UnsafeCast);

        var filter = AppFilters.InEditingMode(EditingMode.Emacs);
        Assert.False(filter.Invoke());
    }

    // ═══════════════════════════════════════════════════════════════════
    // Memoization (FR-012, SC-005)
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void InEditingMode_SameMode_ReturnsSameInstance()
    {
        var f1 = AppFilters.InEditingMode(EditingMode.Vi);
        var f2 = AppFilters.InEditingMode(EditingMode.Vi);
        Assert.Same(f1, f2);
    }

    [Fact]
    public void InEditingMode_DifferentModes_ReturnDifferentInstances()
    {
        var viFilter = AppFilters.InEditingMode(EditingMode.Vi);
        var emacsFilter = AppFilters.InEditingMode(EditingMode.Emacs);
        Assert.NotSame(viFilter, emacsFilter);
    }
}
