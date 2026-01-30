using System.Reflection;
using Stroke.Application;
using Stroke.AutoSuggest;
using Stroke.Core;
using Stroke.Filters;
using Stroke.Input.Pipe;
using Stroke.Output;
using Stroke.Validation;
using Xunit;
using AppContext = Stroke.Application.AppContext;
using Buffer = Stroke.Core.Buffer;
using CompletionItem = Stroke.Completion.Completion;
using StrokeLayout = Stroke.Layout.Layout;

namespace Stroke.Tests.Application;

/// <summary>
/// Tests for AppFilters state property filters (User Story 1).
/// </summary>
public class AppFiltersTests
{
    /// <summary>Helper to set a private field on a buffer via reflection.</summary>
    private static void SetBufferField(Buffer buffer, string fieldName, object? value)
    {
        var field = typeof(Buffer).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);
        field!.SetValue(buffer, value);
    }

    private static (Application<object?> app, IDisposable scope) CreateApp(
        Buffer? buffer = null,
        Stroke.KeyBinding.EditingMode editingMode = Stroke.KeyBinding.EditingMode.Emacs)
    {
        using var input = new SimplePipeInput();
        var output = new DummyOutput();
        Application<object?> app;
        if (buffer is not null)
        {
            var control = new Stroke.Layout.Controls.BufferControl(buffer: buffer);
            var window = new Stroke.Layout.Containers.Window(content: control);
            var layout = new StrokeLayout(new Stroke.Layout.Containers.AnyContainer(window));
            app = new Application<object?>(layout: layout, input: input, output: output, editingMode: editingMode);
        }
        else
        {
            app = new Application<object?>(input: input, output: output, editingMode: editingMode);
        }
        var scope = AppContext.SetApp(app.UnsafeCast);
        return (app, scope);
    }

    // ═══════════════════════════════════════════════════════════════════
    // HasSelection
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void HasSelection_TrueWhenSelectionActive()
    {
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 0));
        var (_, scope) = CreateApp(buffer);
        using (scope)
        {
            buffer.StartSelection();
            Assert.True(AppFilters.HasSelection.Invoke());
        }
    }

    [Fact]
    public void HasSelection_FalseWhenNoSelection()
    {
        var buffer = new Buffer(document: new Document("hello world"));
        var (_, scope) = CreateApp(buffer);
        using (scope)
        {
            Assert.False(AppFilters.HasSelection.Invoke());
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // HasSuggestion
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void HasSuggestion_TrueWithNonEmptySuggestion()
    {
        var buffer = new Buffer();
        SetBufferField(buffer, "_suggestion", new Suggestion("hello"));
        var (_, scope) = CreateApp(buffer);
        using (scope)
        {
            Assert.True(AppFilters.HasSuggestion.Invoke());
        }
    }

    [Fact]
    public void HasSuggestion_FalseWithNullSuggestion()
    {
        var buffer = new Buffer();
        var (_, scope) = CreateApp(buffer);
        using (scope)
        {
            Assert.False(AppFilters.HasSuggestion.Invoke());
        }
    }

    [Fact]
    public void HasSuggestion_FalseWithEmptySuggestion()
    {
        var buffer = new Buffer();
        SetBufferField(buffer, "_suggestion", new Suggestion(""));
        var (_, scope) = CreateApp(buffer);
        using (scope)
        {
            Assert.False(AppFilters.HasSuggestion.Invoke());
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // HasCompletions
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void HasCompletions_TrueWithNonEmptyCompletions()
    {
        var buffer = new Buffer(document: new Document("hel", cursorPosition: 3));
        buffer.SetCompletions([
            new CompletionItem("hello"),
            new CompletionItem("help")
        ]);
        var (_, scope) = CreateApp(buffer);
        using (scope)
        {
            Assert.True(AppFilters.HasCompletions.Invoke());
        }
    }

    [Fact]
    public void HasCompletions_FalseWithNoCompleteState()
    {
        var buffer = new Buffer();
        var (_, scope) = CreateApp(buffer);
        using (scope)
        {
            Assert.False(AppFilters.HasCompletions.Invoke());
        }
    }

    [Fact]
    public void HasCompletions_FalseWithEmptyCompletionsList()
    {
        var buffer = new Buffer(document: new Document("test", cursorPosition: 4));
        buffer.SetCompletions([]);
        var (_, scope) = CreateApp(buffer);
        using (scope)
        {
            Assert.False(AppFilters.HasCompletions.Invoke());
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // CompletionIsSelected
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void CompletionIsSelected_TrueWhenCompletionSelected()
    {
        var buffer = new Buffer(document: new Document("hel", cursorPosition: 3));
        buffer.SetCompletions([
            new CompletionItem("hello"),
            new CompletionItem("help")
        ]);
        buffer.GoToCompletion(0);
        var (_, scope) = CreateApp(buffer);
        using (scope)
        {
            Assert.True(AppFilters.CompletionIsSelected.Invoke());
        }
    }

    [Fact]
    public void CompletionIsSelected_FalseWhenNoCompleteState()
    {
        var buffer = new Buffer();
        var (_, scope) = CreateApp(buffer);
        using (scope)
        {
            Assert.False(AppFilters.CompletionIsSelected.Invoke());
        }
    }

    [Fact]
    public void CompletionIsSelected_FalseWhenNoCurrentCompletion()
    {
        var buffer = new Buffer(document: new Document("hel", cursorPosition: 3));
        buffer.SetCompletions([new CompletionItem("hello")]);
        // CompleteState exists but no selection (CompleteIndex is null)
        var (_, scope) = CreateApp(buffer);
        using (scope)
        {
            Assert.False(AppFilters.CompletionIsSelected.Invoke());
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // IsReadOnly
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void IsReadOnly_TrueWhenBufferReadOnly()
    {
        var buffer = new Buffer(readOnly: () => true);
        var (_, scope) = CreateApp(buffer);
        using (scope)
        {
            Assert.True(AppFilters.IsReadOnly.Invoke());
        }
    }

    [Fact]
    public void IsReadOnly_FalseWhenBufferWritable()
    {
        var buffer = new Buffer();
        var (_, scope) = CreateApp(buffer);
        using (scope)
        {
            Assert.False(AppFilters.IsReadOnly.Invoke());
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // HasValidationError
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void HasValidationError_TrueWhenValidationErrorSet()
    {
        var validator = ValidatorBase.FromCallable(doc =>
        {
            throw new ValidationError(0, "always fails");
        });
        var buffer = new Buffer(validator: validator, document: new Document("test"));
        buffer.Validate();
        var (_, scope) = CreateApp(buffer);
        using (scope)
        {
            Assert.True(AppFilters.HasValidationError.Invoke());
        }
    }

    [Fact]
    public void HasValidationError_FalseWhenNoError()
    {
        var buffer = new Buffer();
        var (_, scope) = CreateApp(buffer);
        using (scope)
        {
            Assert.False(AppFilters.HasValidationError.Invoke());
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // HasArg
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void HasArg_TrueWhenArgSet()
    {
        var buffer = new Buffer();
        var (app, scope) = CreateApp(buffer);
        using (scope)
        {
            app.KeyProcessor.Arg = "5";
            Assert.True(AppFilters.HasArg.Invoke());
        }
    }

    [Fact]
    public void HasArg_FalseWhenArgNull()
    {
        var buffer = new Buffer();
        var (_, scope) = CreateApp(buffer);
        using (scope)
        {
            Assert.False(AppFilters.HasArg.Invoke());
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // IsDone
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void IsDone_FalseWhenNotDone()
    {
        var buffer = new Buffer();
        var (_, scope) = CreateApp(buffer);
        using (scope)
        {
            Assert.False(AppFilters.IsDone.Invoke());
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // RendererHeightIsKnown
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void RendererHeightIsKnown_ReturnsBoolWithoutException()
    {
        var buffer = new Buffer();
        var (_, scope) = CreateApp(buffer);
        using (scope)
        {
            _ = AppFilters.RendererHeightIsKnown.Invoke();
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // IsMultiline
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void IsMultiline_TrueWhenBufferIsMultiline()
    {
        var buffer = new Buffer(multiline: () => true);
        var (_, scope) = CreateApp(buffer);
        using (scope)
        {
            Assert.True(AppFilters.IsMultiline.Invoke());
        }
    }

    [Fact]
    public void IsMultiline_FalseWhenBufferIsSingleLine()
    {
        var buffer = new Buffer(multiline: () => false);
        var (_, scope) = CreateApp(buffer);
        using (scope)
        {
            Assert.False(AppFilters.IsMultiline.Invoke());
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // InPasteMode
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void InPasteMode_FalseByDefault()
    {
        var buffer = new Buffer();
        var (_, scope) = CreateApp(buffer);
        using (scope)
        {
            Assert.False(AppFilters.InPasteMode.Invoke());
        }
    }

    [Fact]
    public void InPasteMode_TrueWhenPasteModeEnabled()
    {
        using var input = new SimplePipeInput();
        var app = new Application<object?>(
            input: input, output: new DummyOutput(),
            pasteMode: new FilterOrBool(true));
        using var scope = AppContext.SetApp(app.UnsafeCast);

        Assert.True(AppFilters.InPasteMode.Invoke());
    }

    // ═══════════════════════════════════════════════════════════════════
    // DummyApplication graceful false (FR-009)
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void AllStateFilters_ReturnFalseWithDummyApplication()
    {
        Assert.False(AppFilters.HasSelection.Invoke());
        Assert.False(AppFilters.HasSuggestion.Invoke());
        Assert.False(AppFilters.HasCompletions.Invoke());
        Assert.False(AppFilters.CompletionIsSelected.Invoke());
        Assert.False(AppFilters.IsReadOnly.Invoke());
        Assert.False(AppFilters.HasValidationError.Invoke());
        Assert.False(AppFilters.HasArg.Invoke());
        Assert.False(AppFilters.IsDone.Invoke());
        Assert.False(AppFilters.InPasteMode.Invoke());
    }

    // ═══════════════════════════════════════════════════════════════════
    // Filter composition (FR-011)
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void FilterComposition_AndOperator()
    {
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));
        var (_, scope) = CreateApp(buffer);
        using (scope)
        {
            buffer.StartSelection();
            // HasSelection=true, IsReadOnly=false → AND=false
            var combined = AppFilters.HasSelection.And(AppFilters.IsReadOnly);
            Assert.False(combined.Invoke());
        }
    }

    [Fact]
    public void FilterComposition_OrOperator()
    {
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 0));
        var (_, scope) = CreateApp(buffer);
        using (scope)
        {
            buffer.StartSelection();
            // HasSelection=true, IsReadOnly=false → OR=true
            var combined = AppFilters.HasSelection.Or(AppFilters.IsReadOnly);
            Assert.True(combined.Invoke());
        }
    }

    [Fact]
    public void FilterComposition_NotOperator()
    {
        var buffer = new Buffer();
        var (_, scope) = CreateApp(buffer);
        using (scope)
        {
            // HasSelection=false → NOT=true
            var inverted = AppFilters.HasSelection.Invert();
            Assert.True(inverted.Invoke());
        }
    }
}
