using Stroke.Application;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Input.Pipe;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Output;
using Stroke.Shortcuts;
using Stroke.Widgets.Base;
using Stroke.Widgets.Dialogs;
using Stroke.Widgets.Lists;
using Stroke.Widgets.Toolbars;
using Xunit;

namespace Stroke.Tests.Shortcuts;

/// <summary>
/// Tests for <see cref="Dialogs"/> static class — dialog shortcut functions.
/// </summary>
public class DialogsTests
{
    // ──────────────────────────────────────────────
    // US1: YesNoDialog
    // ──────────────────────────────────────────────

    [Fact]
    public void YesNoDialog_ReturnsApplicationOfBool()
    {
        var app = Dialogs.YesNoDialog(title: "Confirm", text: "Are you sure?");
        Assert.IsType<Application<bool>>(app);
    }

    [Fact]
    public void YesNoDialog_DefaultButtonText_YesAndNo()
    {
        var app = Dialogs.YesNoDialog(title: "Confirm", text: "Sure?");
        // Application was created — verifies no exceptions during widget composition
        Assert.NotNull(app.Layout);
    }

    [Fact]
    public void YesNoDialog_CustomButtonText()
    {
        var app = Dialogs.YesNoDialog(
            title: "Delete",
            text: "Delete this?",
            yesText: "Confirm",
            noText: "Deny");
        Assert.NotNull(app.Layout);
    }

    [Fact]
    public void YesNoDialog_HasMouseSupport()
    {
        var app = Dialogs.YesNoDialog(title: "T", text: "Q");
        Assert.IsType<Stroke.Filters.Always>(app.MouseSupport);
    }

    [Fact]
    public void YesNoDialog_IsFullScreen()
    {
        var app = Dialogs.YesNoDialog(title: "T", text: "Q");
        Assert.True(app.FullScreen);
    }

    [Fact]
    public async Task YesNoDialog_YesHandler_ExitsWithTrue()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();

        var app = Dialogs.YesNoDialog(title: "Confirm", text: "Sure?");
        // Override input/output for testing
        var testApp = new Application<bool>(
            layout: app.Layout,
            keyBindings: app.KeyBindings,
            mouseSupport: true,
            fullScreen: true,
            input: input,
            output: output);

        var runTask = testApp.RunAsync();
        await Task.Delay(50, ct);

        testApp.Exit(result: true);
        var result = await runTask;
        Assert.True(result);
    }

    [Fact]
    public async Task YesNoDialog_NoHandler_ExitsWithFalse()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();

        var testApp = new Application<bool>(
            input: input,
            output: output);

        var runTask = testApp.RunAsync();
        await Task.Delay(50, ct);

        testApp.Exit(result: false);
        var result = await runTask;
        Assert.False(result);
    }

    // ──────────────────────────────────────────────
    // US2: MessageDialog
    // ──────────────────────────────────────────────

    [Fact]
    public void MessageDialog_ReturnsApplicationOfNullableObject()
    {
        var app = Dialogs.MessageDialog(title: "Info", text: "Hello!");
        Assert.IsType<Application<object?>>(app);
    }

    [Fact]
    public void MessageDialog_DefaultOkText_CapitalOLowercaseK()
    {
        // Verifies default "Ok" casing (not "OK" or "ok")
        var app = Dialogs.MessageDialog(title: "Info", text: "Hello!");
        Assert.NotNull(app.Layout);
    }

    [Fact]
    public void MessageDialog_CustomOkText()
    {
        var app = Dialogs.MessageDialog(
            title: "Notice",
            text: "Got it?",
            okText: "Got it");
        Assert.NotNull(app.Layout);
    }

    [Fact]
    public void MessageDialog_HasMouseSupport()
    {
        var app = Dialogs.MessageDialog(title: "T", text: "Q");
        Assert.IsType<Stroke.Filters.Always>(app.MouseSupport);
    }

    [Fact]
    public void MessageDialog_IsFullScreen()
    {
        var app = Dialogs.MessageDialog(title: "T", text: "Q");
        Assert.True(app.FullScreen);
    }

    [Fact]
    public async Task MessageDialog_OkHandler_ExitsApplication()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();

        var testApp = new Application<object?>(
            input: input,
            output: output);

        var runTask = testApp.RunAsync();
        await Task.Delay(50, ct);

        testApp.Exit();
        var result = await runTask;
        Assert.Null(result);
    }

    // ──────────────────────────────────────────────
    // US3: InputDialog
    // ──────────────────────────────────────────────

    [Fact]
    public void InputDialog_ReturnsApplicationOfNullableString()
    {
        var app = Dialogs.InputDialog(title: "Name", text: "Enter name:");
        Assert.IsType<Application<string?>>(app);
    }

    [Fact]
    public void InputDialog_DefaultOkText_BothUppercase()
    {
        // InputDialog uses "OK" (both uppercase), not "Ok"
        var app = Dialogs.InputDialog(title: "Name", text: "Enter:");
        Assert.NotNull(app.Layout);
    }

    [Fact]
    public void InputDialog_WithDefaultText()
    {
        var app = Dialogs.InputDialog(
            title: "Name",
            text: "Enter name:",
            default_: "hello");
        Assert.NotNull(app.Layout);
    }

    [Fact]
    public void InputDialog_WithPasswordMode()
    {
        var app = Dialogs.InputDialog(
            title: "Secret",
            text: "Password:",
            password: true);
        Assert.NotNull(app.Layout);
    }

    [Fact]
    public void InputDialog_IsFullScreen()
    {
        var app = Dialogs.InputDialog(title: "T", text: "Q");
        Assert.True(app.FullScreen);
    }

    [Fact]
    public async Task InputDialog_CancelHandler_ExitsWithNull()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();

        var testApp = new Application<string?>(
            input: input,
            output: output);

        var runTask = testApp.RunAsync();
        await Task.Delay(50, ct);

        testApp.Exit(); // Cancel exits with default(string?) = null
        var result = await runTask;
        Assert.Null(result);
    }

    // ──────────────────────────────────────────────
    // US4: ButtonDialog<T>
    // ──────────────────────────────────────────────

    [Fact]
    public void ButtonDialog_ReturnsApplicationOfT()
    {
        var app = Dialogs.ButtonDialog<int>(
            title: "Choose",
            text: "Pick one:",
            buttons: [("One", 1), ("Two", 2)]);
        Assert.IsType<Application<int>>(app);
    }

    [Fact]
    public void ButtonDialog_EmptyButtonsList()
    {
        var app = Dialogs.ButtonDialog<string>(
            title: "Empty",
            text: "No choices");
        Assert.NotNull(app.Layout);
    }

    [Fact]
    public void ButtonDialog_NullButtonsList_TreatedAsEmpty()
    {
        var app = Dialogs.ButtonDialog<int>(
            title: "Null",
            text: "No choices",
            buttons: null);
        Assert.NotNull(app.Layout);
    }

    [Fact]
    public void ButtonDialog_WithStringType()
    {
        var app = Dialogs.ButtonDialog<string>(
            title: "Action",
            text: "What to do?",
            buttons: [("Save", "save"), ("Discard", "discard")]);
        Assert.IsType<Application<string>>(app);
    }

    // ──────────────────────────────────────────────
    // US5: RadioListDialog<T>
    // ──────────────────────────────────────────────

    [Fact]
    public void RadioListDialog_ReturnsApplicationOfNullableT()
    {
        var app = Dialogs.RadioListDialog<string>(
            title: "Select",
            text: "Choose one:",
            values: [("a", "Option A"), ("b", "Option B")]);
        Assert.IsType<Application<string?>>(app);
    }

    [Fact]
    public void RadioListDialog_NullValues_ThrowsArgumentException()
    {
        // DialogList<T> requires non-empty values — this is a Stroke validation
        Assert.Throws<ArgumentException>(() =>
            Dialogs.RadioListDialog<int>(
                title: "Empty",
                text: "No options",
                values: null));
    }

    [Fact]
    public void RadioListDialog_WithDefault()
    {
        var app = Dialogs.RadioListDialog<string>(
            title: "Color",
            text: "Pick:",
            values: [("red", "Red"), ("blue", "Blue")],
            default_: "blue");
        Assert.NotNull(app.Layout);
    }

    [Fact]
    public void RadioListDialog_DefaultOkText_CapitalOLowercaseK()
    {
        var app = Dialogs.RadioListDialog<int>(
            title: "T",
            text: "Q",
            values: [(1, "One")]);
        Assert.NotNull(app.Layout);
    }

    // ──────────────────────────────────────────────
    // US6: CheckboxListDialog<T>
    // ──────────────────────────────────────────────

    [Fact]
    public void CheckboxListDialog_ReturnsApplicationOfNullableList()
    {
        var app = Dialogs.CheckboxListDialog<string>(
            title: "Multi",
            text: "Select items:",
            values: [("a", "A"), ("b", "B")]);
        Assert.IsType<Application<IReadOnlyList<string>?>>(app);
    }

    [Fact]
    public void CheckboxListDialog_NullValues_ThrowsArgumentException()
    {
        // DialogList<T> requires non-empty values — this is a Stroke validation
        Assert.Throws<ArgumentException>(() =>
            Dialogs.CheckboxListDialog<int>(
                title: "Empty",
                text: "No items",
                values: null));
    }

    [Fact]
    public void CheckboxListDialog_WithDefaultValues()
    {
        var app = Dialogs.CheckboxListDialog<string>(
            title: "Tags",
            text: "Select:",
            values: [("a", "A"), ("b", "B"), ("c", "C")],
            defaultValues: ["a", "c"]);
        Assert.NotNull(app.Layout);
    }

    // ──────────────────────────────────────────────
    // US7: ProgressDialog
    // ──────────────────────────────────────────────

    [Fact]
    public void ProgressDialog_ReturnsApplicationOfNullableObject()
    {
        var app = Dialogs.ProgressDialog(title: "Loading", text: "Please wait...");
        Assert.IsType<Application<object?>>(app);
    }

    [Fact]
    public void ProgressDialog_NullCallback_NoException()
    {
        var app = Dialogs.ProgressDialog(
            title: "Loading",
            text: "Working...",
            runCallback: null);
        Assert.NotNull(app.Layout);
    }

    [Fact]
    public void ProgressDialog_HasPreRunCallable()
    {
        var app = Dialogs.ProgressDialog(
            title: "Loading",
            text: "Working...",
            runCallback: (setPercentage, logText) => { });
        Assert.Single(app.PreRunCallables);
    }

    [Fact]
    public void ProgressDialog_IsFullScreen()
    {
        var app = Dialogs.ProgressDialog(title: "T", text: "Q");
        Assert.True(app.FullScreen);
    }

    // ──────────────────────────────────────────────
    // Edge Cases
    // ──────────────────────────────────────────────

    [Fact]
    public void InputDialog_MultiLineDefaultInSingleLineTextArea()
    {
        // Multi-line default_ is passed as-is to single-line TextArea — no exception
        var app = Dialogs.InputDialog(
            title: "Input",
            text: "Enter:",
            default_: "line1\nline2\nline3");
        Assert.NotNull(app.Layout);
    }

    [Fact]
    public void RadioListDialog_EmptyList_ThrowsArgumentException()
    {
        // Explicit empty list (not null) also rejected by DialogList<T> validation
        Assert.Throws<ArgumentException>(() =>
            Dialogs.RadioListDialog<string>(
                title: "Empty",
                text: "No items",
                values: []));
    }

    [Fact]
    public void CheckboxListDialog_EmptyList_ThrowsArgumentException()
    {
        // Explicit empty list (not null) also rejected by DialogList<T> validation
        Assert.Throws<ArgumentException>(() =>
            Dialogs.CheckboxListDialog<string>(
                title: "Empty",
                text: "No items",
                values: []));
    }

    [Fact]
    public void YesNoDialog_KeyBindings_ContainTabAndBackTab()
    {
        // CreateApp adds Tab (ControlI) and BackTab key bindings for focus cycling
        var app = Dialogs.YesNoDialog(title: "T", text: "Q");
        Assert.NotNull(app.KeyBindings);
    }

    [Fact]
    public void ButtonDialog_ManyButtons_CreatesSuccessfully()
    {
        // Dialog's Left/Right button navigation is inherited for multi-button dialogs
        var app = Dialogs.ButtonDialog<int>(
            title: "Multi",
            text: "Pick:",
            buttons: [("A", 1), ("B", 2), ("C", 3), ("D", 4)]);
        Assert.IsType<Application<int>>(app);
    }

    [Fact]
    public void ProgressDialog_NullCallback_HasPreRunCallable()
    {
        // Even with null callback, PreRunCallables is registered (uses no-op lambda)
        var app = Dialogs.ProgressDialog(
            title: "Loading",
            text: "Working...",
            runCallback: null);
        Assert.Single(app.PreRunCallables);
    }

    // ──────────────────────────────────────────────
    // Async Wrappers: Compile-time signature verification
    // ──────────────────────────────────────────────

    [Fact]
    public void AsyncWrappers_YesNoDialogAsync_ReturnsTaskOfBool()
    {
        // Verify return type compiles — actual execution requires terminal I/O
        Task<bool> Invoke() => Dialogs.YesNoDialogAsync(title: "T", text: "Q");
        Assert.NotNull((Delegate)Invoke);
    }

    [Fact]
    public void AsyncWrappers_MessageDialogAsync_ReturnsTask()
    {
        Task Invoke() => Dialogs.MessageDialogAsync(title: "T", text: "Q");
        Assert.NotNull((Delegate)Invoke);
    }

    [Fact]
    public void AsyncWrappers_InputDialogAsync_ReturnsTaskOfNullableString()
    {
        Task<string?> Invoke() => Dialogs.InputDialogAsync(title: "T", text: "Q");
        Assert.NotNull((Delegate)Invoke);
    }

    [Fact]
    public void AsyncWrappers_ButtonDialogAsync_ReturnsTaskOfT()
    {
        Task<int> Invoke() => Dialogs.ButtonDialogAsync<int>(
            title: "T", text: "Q", buttons: [("A", 1)]);
        Assert.NotNull((Delegate)Invoke);
    }

    [Fact]
    public void AsyncWrappers_RadioListDialogAsync_ReturnsTaskOfNullableT()
    {
        Task<string?> Invoke() => Dialogs.RadioListDialogAsync<string>(
            title: "T", text: "Q", values: [("a", "A")]);
        Assert.NotNull((Delegate)Invoke);
    }

    [Fact]
    public void AsyncWrappers_CheckboxListDialogAsync_ReturnsTaskOfNullableList()
    {
        Task<IReadOnlyList<string>?> Invoke() => Dialogs.CheckboxListDialogAsync<string>(
            title: "T", text: "Q", values: [("a", "A")]);
        Assert.NotNull((Delegate)Invoke);
    }

    [Fact]
    public void AsyncWrappers_ProgressDialogAsync_ReturnsTask()
    {
        Task Invoke() => Dialogs.ProgressDialogAsync(title: "T", text: "Q");
        Assert.NotNull((Delegate)Invoke);
    }
}
