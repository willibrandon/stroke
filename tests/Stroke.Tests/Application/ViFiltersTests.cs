using Stroke.Application;
using Stroke.Core;
using Stroke.Filters;
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
/// Tests for ViFilters static class (User Story 3).
/// </summary>
public class ViFiltersTests
{
    private static (Application<object?> app, IDisposable scope) CreateViApp(
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
            editingMode: EditingMode.Vi);
        var scope = AppContext.SetApp(app.UnsafeCast);
        return (app, scope);
    }

    // ═══════════════════════════════════════════════════════════════════
    // ViMode
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void ViMode_TrueInViEditingMode()
    {
        var (app, scope) = CreateViApp();
        using (scope)
        {
            Assert.True(ViFilters.ViMode.Invoke());
        }
    }

    [Fact]
    public void ViMode_FalseInEmacsEditingMode()
    {
        using var input = new SimplePipeInput();
        var app = new Application<object?>(
            input: input, output: new DummyOutput(),
            editingMode: EditingMode.Emacs);
        using var scope = AppContext.SetApp(app.UnsafeCast);

        Assert.False(ViFilters.ViMode.Invoke());
    }

    // ═══════════════════════════════════════════════════════════════════
    // ViNavigationMode
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void ViNavigationMode_TrueWithNavigationInputMode()
    {
        var (app, scope) = CreateViApp();
        using (scope)
        {
            app.ViState.InputMode = InputMode.Navigation;
            Assert.True(ViFilters.ViNavigationMode.Invoke());
        }
    }

    [Fact]
    public void ViNavigationMode_FalseWithPendingOperator()
    {
        var (app, scope) = CreateViApp();
        using (scope)
        {
            app.ViState.InputMode = InputMode.Navigation;
            app.ViState.OperatorFunc = (_, _) => NotImplementedOrNone.None;
            Assert.False(ViFilters.ViNavigationMode.Invoke());
        }
    }

    [Fact]
    public void ViNavigationMode_FalseWithDigraphWait()
    {
        var (app, scope) = CreateViApp();
        using (scope)
        {
            app.ViState.InputMode = InputMode.Navigation;
            app.ViState.WaitingForDigraph = true;
            Assert.False(ViFilters.ViNavigationMode.Invoke());
        }
    }

    [Fact]
    public void ViNavigationMode_FalseWithSelectionActive()
    {
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));
        var (app, scope) = CreateViApp(buffer);
        using (scope)
        {
            app.ViState.InputMode = InputMode.Navigation;
            buffer.StartSelection();
            Assert.False(ViFilters.ViNavigationMode.Invoke());
        }
    }

    [Fact]
    public void ViNavigationMode_TrueWithReadOnlyBuffer()
    {
        var buffer = new Buffer(readOnly: () => true);
        var (app, scope) = CreateViApp(buffer);
        using (scope)
        {
            // Not in Navigation input mode, but buffer is read-only → navigation behavior
            app.ViState.InputMode = InputMode.Insert;
            Assert.True(ViFilters.ViNavigationMode.Invoke());
        }
    }

    [Fact]
    public void ViNavigationMode_TrueWithTemporaryNavigationMode()
    {
        var (app, scope) = CreateViApp();
        using (scope)
        {
            app.ViState.InputMode = InputMode.Insert;
            app.ViState.TemporaryNavigationMode = true;
            Assert.True(ViFilters.ViNavigationMode.Invoke());
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // ViInsertMode
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void ViInsertMode_TrueWithInsertInputMode()
    {
        var (app, scope) = CreateViApp();
        using (scope)
        {
            // Default InputMode is Insert after Reset
            app.ViState.InputMode = InputMode.Insert;
            Assert.True(ViFilters.ViInsertMode.Invoke());
        }
    }

    [Fact]
    public void ViInsertMode_FalseWithTemporaryNavigationMode()
    {
        var (app, scope) = CreateViApp();
        using (scope)
        {
            app.ViState.InputMode = InputMode.Insert;
            app.ViState.TemporaryNavigationMode = true;
            Assert.False(ViFilters.ViInsertMode.Invoke());
        }
    }

    [Fact]
    public void ViInsertMode_FalseWithReadOnlyBuffer()
    {
        var buffer = new Buffer(readOnly: () => true);
        var (app, scope) = CreateViApp(buffer);
        using (scope)
        {
            app.ViState.InputMode = InputMode.Insert;
            Assert.False(ViFilters.ViInsertMode.Invoke());
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // ViInsertMultipleMode
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void ViInsertMultipleMode_TrueWithInsertMultipleInputMode()
    {
        var (app, scope) = CreateViApp();
        using (scope)
        {
            app.ViState.InputMode = InputMode.InsertMultiple;
            Assert.True(ViFilters.ViInsertMultipleMode.Invoke());
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // ViReplaceMode
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void ViReplaceMode_TrueWithReplaceInputMode()
    {
        var (app, scope) = CreateViApp();
        using (scope)
        {
            app.ViState.InputMode = InputMode.Replace;
            Assert.True(ViFilters.ViReplaceMode.Invoke());
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // ViReplaceSingleMode
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void ViReplaceSingleMode_TrueWithReplaceSingleInputMode()
    {
        var (app, scope) = CreateViApp();
        using (scope)
        {
            app.ViState.InputMode = InputMode.ReplaceSingle;
            Assert.True(ViFilters.ViReplaceSingleMode.Invoke());
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // ViSelectionMode
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void ViSelectionMode_TrueWithSelection()
    {
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));
        var (app, scope) = CreateViApp(buffer);
        using (scope)
        {
            buffer.StartSelection();
            Assert.True(ViFilters.ViSelectionMode.Invoke());
        }
    }

    [Fact]
    public void ViSelectionMode_FalseWithoutSelection()
    {
        var (app, scope) = CreateViApp();
        using (scope)
        {
            Assert.False(ViFilters.ViSelectionMode.Invoke());
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // ViWaitingForTextObjectMode
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void ViWaitingForTextObjectMode_TrueWithPendingOperator()
    {
        var (app, scope) = CreateViApp();
        using (scope)
        {
            app.ViState.OperatorFunc = (_, _) => NotImplementedOrNone.None;
            Assert.True(ViFilters.ViWaitingForTextObjectMode.Invoke());
        }
    }

    [Fact]
    public void ViWaitingForTextObjectMode_FalseWithoutPendingOperator()
    {
        var (app, scope) = CreateViApp();
        using (scope)
        {
            Assert.False(ViFilters.ViWaitingForTextObjectMode.Invoke());
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // ViDigraphMode
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void ViDigraphMode_TrueWithDigraphWait()
    {
        var (app, scope) = CreateViApp();
        using (scope)
        {
            app.ViState.WaitingForDigraph = true;
            Assert.True(ViFilters.ViDigraphMode.Invoke());
        }
    }

    [Fact]
    public void ViDigraphMode_FalseWithoutDigraphWait()
    {
        var (app, scope) = CreateViApp();
        using (scope)
        {
            Assert.False(ViFilters.ViDigraphMode.Invoke());
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // ViRecordingMacro
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void ViRecordingMacro_TrueWhenRecording()
    {
        var (app, scope) = CreateViApp();
        using (scope)
        {
            app.ViState.RecordingRegister = "q";
            Assert.True(ViFilters.ViRecordingMacro.Invoke());
        }
    }

    [Fact]
    public void ViRecordingMacro_FalseWhenNotRecording()
    {
        var (app, scope) = CreateViApp();
        using (scope)
        {
            Assert.False(ViFilters.ViRecordingMacro.Invoke());
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // ViSearchDirectionReversed
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void ViSearchDirectionReversed_TrueWhenReversed()
    {
        using var input = new SimplePipeInput();
        var app = new Application<object?>(
            input: input, output: new DummyOutput(),
            editingMode: EditingMode.Vi,
            reverseViSearchDirection: new FilterOrBool(true));
        using var scope = AppContext.SetApp(app.UnsafeCast);

        Assert.True(ViFilters.ViSearchDirectionReversed.Invoke());
    }

    [Fact]
    public void ViSearchDirectionReversed_FalseByDefault()
    {
        var (app, scope) = CreateViApp();
        using (scope)
        {
            Assert.False(ViFilters.ViSearchDirectionReversed.Invoke());
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // DummyApplication — all Vi filters return false
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void AllViFilters_ReturnFalseWithDummyApplication()
    {
        // DummyApplication defaults to Emacs mode
        Assert.False(ViFilters.ViMode.Invoke());
        Assert.False(ViFilters.ViNavigationMode.Invoke());
        Assert.False(ViFilters.ViInsertMode.Invoke());
        Assert.False(ViFilters.ViInsertMultipleMode.Invoke());
        Assert.False(ViFilters.ViReplaceMode.Invoke());
        Assert.False(ViFilters.ViReplaceSingleMode.Invoke());
        Assert.False(ViFilters.ViSelectionMode.Invoke());
        Assert.False(ViFilters.ViWaitingForTextObjectMode.Invoke());
        Assert.False(ViFilters.ViDigraphMode.Invoke());
        Assert.False(ViFilters.ViRecordingMacro.Invoke());
        Assert.False(ViFilters.ViSearchDirectionReversed.Invoke());
    }
}
