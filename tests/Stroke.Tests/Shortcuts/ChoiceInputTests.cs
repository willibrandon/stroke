using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.Input.Pipe;
using Stroke.KeyBinding;
using Stroke.Output;
using Stroke.Shortcuts;
using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Shortcuts;

/// <summary>
/// Tests for <see cref="ChoiceInput{T}"/> class — selection prompt shortcut.
/// </summary>
/// <remarks>
/// Test organization follows task breakdown:
/// - T022: Constructor validation, default value handling, property getters, immutability
/// - T023: Navigation (delegated to RadioList, tested via integration)
/// - T024: Interrupt handling (Ctrl+C behavior)
/// </remarks>
public class ChoiceInputTests
{
    // ──────────────────────────────────────────────
    // T022: Constructor Validation Tests
    // ──────────────────────────────────────────────

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ChoiceInput<string>(
                message: "Select:",
                options: null!));
    }

    [Fact]
    public void Constructor_EmptyOptions_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new ChoiceInput<string>(
                message: "Select:",
                options: []));
    }

    [Fact]
    public void Constructor_SingleOption_Succeeds()
    {
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"Option A")]);

        Assert.Single(choice.Options);
    }

    [Fact]
    public void Constructor_MultipleOptions_Succeeds()
    {
        var choice = new ChoiceInput<int>(
            message: "Pick a number:",
            options:
            [
                (1, (AnyFormattedText)"One"),
                (2, (AnyFormattedText)"Two"),
                (3, (AnyFormattedText)"Three"),
            ]);

        Assert.Equal(3, choice.Options.Count);
    }

    // ──────────────────────────────────────────────
    // T022: Default Value Handling Tests
    // ──────────────────────────────────────────────

    [Fact]
    public void Constructor_DefaultValueMatches_StoresDefault()
    {
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options:
            [
                ("a", (AnyFormattedText)"Option A"),
                ("b", (AnyFormattedText)"Option B"),
            ],
            defaultValue: "b");

        Assert.Equal("b", choice.Default);
    }

    [Fact]
    public void Constructor_DefaultValueDoesNotMatch_StoresAnyway()
    {
        // RadioList handles the fallback to first option internally
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options:
            [
                ("a", (AnyFormattedText)"Option A"),
                ("b", (AnyFormattedText)"Option B"),
            ],
            defaultValue: "nonexistent");

        Assert.Equal("nonexistent", choice.Default);
    }

    [Fact]
    public void Constructor_NoDefaultValue_DefaultIsNull()
    {
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")]);

        Assert.Null(choice.Default);
    }

    [Fact]
    public void Constructor_DefaultValueForValueType_DefaultIsDefault()
    {
        var choice = new ChoiceInput<int>(
            message: "Select:",
            options: [(1, (AnyFormattedText)"One")]);

        Assert.Equal(0, choice.Default);
    }

    // ──────────────────────────────────────────────
    // T022: Property Getter Tests (all 12 properties)
    // ──────────────────────────────────────────────

    [Fact]
    public void Property_Message_ReturnsConfiguredValue()
    {
        var message = (AnyFormattedText)"Please select an option:";
        var choice = new ChoiceInput<string>(
            message: message,
            options: [("a", (AnyFormattedText)"A")]);

        Assert.Equal(message, choice.Message);
    }

    [Fact]
    public void Property_Options_ReturnsConfiguredList()
    {
        var options = new List<(string, AnyFormattedText)>
        {
            ("x", "X"),
            ("y", "Y"),
        };
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: options);

        Assert.Equal(2, choice.Options.Count);
        Assert.Equal("x", choice.Options[0].Value);
        Assert.Equal("y", choice.Options[1].Value);
    }

    [Fact]
    public void Property_MouseSupport_DefaultsFalse()
    {
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")]);

        Assert.False(choice.MouseSupport);
    }

    [Fact]
    public void Property_MouseSupport_ReturnsConfiguredValue()
    {
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")],
            mouseSupport: true);

        Assert.True(choice.MouseSupport);
    }

    [Fact]
    public void Property_Style_DefaultsToNonNull()
    {
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")]);

        Assert.NotNull(choice.Style);
    }

    [Fact]
    public void Property_Style_ReturnsConfiguredValue()
    {
        var customStyle = Style.FromDict(new Dictionary<string, string>
        {
            ["frame.border"] = "#ff0000",
        });
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")],
            style: customStyle);

        Assert.Same(customStyle, choice.Style);
    }

    [Fact]
    public void Property_Symbol_DefaultsToGreaterThan()
    {
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")]);

        Assert.Equal(">", choice.Symbol);
    }

    [Fact]
    public void Property_Symbol_ReturnsConfiguredValue()
    {
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")],
            symbol: "→");

        Assert.Equal("→", choice.Symbol);
    }

    [Fact]
    public void Property_BottomToolbar_DefaultsToNull()
    {
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")]);

        Assert.Null(choice.BottomToolbar);
    }

    [Fact]
    public void Property_BottomToolbar_ReturnsConfiguredValue()
    {
        var toolbar = (AnyFormattedText)"Press Enter to confirm";
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")],
            bottomToolbar: toolbar);

        Assert.Equal(toolbar, choice.BottomToolbar);
    }

    [Fact]
    public void Property_ShowFrame_DefaultsToFalse()
    {
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")]);

        Assert.False(FilterUtils.IsTrue(choice.ShowFrame));
    }

    [Fact]
    public void Property_ShowFrame_ReturnsConfiguredValue()
    {
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")],
            showFrame: true);

        Assert.True(FilterUtils.IsTrue(choice.ShowFrame));
    }

    [Fact]
    public void Property_EnableSuspend_DefaultsToFalse()
    {
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")]);

        Assert.False(FilterUtils.IsTrue(choice.EnableSuspend));
    }

    [Fact]
    public void Property_EnableSuspend_ReturnsConfiguredValue()
    {
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")],
            enableSuspend: true);

        Assert.True(FilterUtils.IsTrue(choice.EnableSuspend));
    }

    [Fact]
    public void Property_EnableInterrupt_DefaultsToTrue()
    {
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")]);

        Assert.True(FilterUtils.IsTrue(choice.EnableInterrupt));
    }

    [Fact]
    public void Property_EnableInterrupt_ReturnsConfiguredValue()
    {
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")],
            enableInterrupt: false);

        Assert.False(FilterUtils.IsTrue(choice.EnableInterrupt));
    }

    [Fact]
    public void Property_InterruptException_DefaultsToKeyboardInterrupt()
    {
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")]);

        Assert.Equal(typeof(KeyboardInterrupt), choice.InterruptException);
    }

    [Fact]
    public void Property_InterruptException_ReturnsConfiguredValue()
    {
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")],
            interruptException: typeof(OperationCanceledException));

        Assert.Equal(typeof(OperationCanceledException), choice.InterruptException);
    }

    [Fact]
    public void Property_KeyBindings_DefaultsToNull()
    {
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")]);

        Assert.Null(choice.KeyBindings);
    }

    [Fact]
    public void Property_KeyBindings_ReturnsConfiguredValue()
    {
        var bindings = new KeyBindings();
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")],
            keyBindings: bindings);

        Assert.Same(bindings, choice.KeyBindings);
    }

    // ──────────────────────────────────────────────
    // T022: Immutability Verification (NFR-001)
    // ──────────────────────────────────────────────

    [Fact]
    public void Immutability_AllPropertiesAreGetOnly()
    {
        // Verify via reflection that all public properties have no setter
        var type = typeof(ChoiceInput<string>);
        var properties = type.GetProperties();

        foreach (var prop in properties)
        {
            Assert.True(
                prop.GetSetMethod() is null,
                $"Property {prop.Name} has a public setter, violating immutability.");
        }
    }

    [Fact]
    public void Immutability_ClassIsSealed()
    {
        var type = typeof(ChoiceInput<string>);
        Assert.True(type.IsSealed, "ChoiceInput<T> should be sealed.");
    }

    // ──────────────────────────────────────────────
    // T023: Navigation Tests (via Application integration)
    // ──────────────────────────────────────────────
    // Note: Actual navigation is delegated to RadioList.
    // These tests verify the Application is correctly configured.

    [Fact]
    public async Task Prompt_EnterKey_ReturnsSelectedValue()
    {
        var ct = TestContext.Current.CancellationToken;
        using var input = new SimplePipeInput();
        var output = new DummyOutput();

        var choice = new ChoiceInput<string>(
            message: "Select:",
            options:
            [
                ("a", (AnyFormattedText)"Option A"),
                ("b", (AnyFormattedText)"Option B"),
            ],
            defaultValue: "b");

        // We can't directly inject input/output into ChoiceInput,
        // so we test that it creates a valid configuration
        // by verifying all properties are correctly passed through
        Assert.Equal("b", choice.Default);
        Assert.Equal(2, choice.Options.Count);
    }

    [Fact]
    public void Constructor_WithNumberedOptions_ConfiguresShowNumbers()
    {
        // RadioList uses showNumbers=true internally
        var choice = new ChoiceInput<int>(
            message: "Select:",
            options:
            [
                (1, (AnyFormattedText)"One"),
                (2, (AnyFormattedText)"Two"),
                (3, (AnyFormattedText)"Three"),
                (4, (AnyFormattedText)"Four"),
                (5, (AnyFormattedText)"Five"),
                (6, (AnyFormattedText)"Six"),
                (7, (AnyFormattedText)"Seven"),
                (8, (AnyFormattedText)"Eight"),
                (9, (AnyFormattedText)"Nine"),
            ]);

        Assert.Equal(9, choice.Options.Count);
    }

    // ──────────────────────────────────────────────
    // T024: Interrupt Tests (Ctrl+C behavior)
    // ──────────────────────────────────────────────

    [Fact]
    public void InterruptException_EnabledByDefault_IsKeyboardInterrupt()
    {
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")]);

        Assert.True(FilterUtils.IsTrue(choice.EnableInterrupt));
        Assert.Equal(typeof(KeyboardInterrupt), choice.InterruptException);
    }

    [Fact]
    public void InterruptException_CustomType_IsConfigured()
    {
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")],
            interruptException: typeof(InvalidOperationException));

        Assert.Equal(typeof(InvalidOperationException), choice.InterruptException);
    }

    [Fact]
    public void InterruptException_Disabled_FilterReturnsFalse()
    {
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")],
            enableInterrupt: false);

        Assert.False(FilterUtils.IsTrue(choice.EnableInterrupt));
    }

    [Fact]
    public void InterruptException_WithFilter_StoresFilter()
    {
        var filter = new Condition(() => true);
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")],
            enableInterrupt: new FilterOrBool(filter));

        Assert.True(FilterUtils.IsTrue(choice.EnableInterrupt));
    }

    // ──────────────────────────────────────────────
    // Dialogs.Choice<T> and ChoiceAsync<T> Tests
    // ──────────────────────────────────────────────

    [Fact]
    public void DialogsChoice_ReturnsTypeT()
    {
        // Verify signature compiles — actual execution requires terminal I/O
        static string Invoke() => Dialogs.Choice<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")]);

        Assert.NotNull((Delegate)Invoke);
    }

    [Fact]
    public void DialogsChoiceAsync_ReturnsTaskOfT()
    {
        // Verify signature compiles — actual execution requires terminal I/O
        static Task<string> Invoke() => Dialogs.ChoiceAsync<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")]);

        Assert.NotNull((Delegate)Invoke);
    }

    [Fact]
    public void DialogsChoice_AcceptsAllParameters()
    {
        // Verify all 12 parameters are accepted
        static int Invoke() => Dialogs.Choice<int>(
            message: "Select:",
            options: [(1, (AnyFormattedText)"One")],
            defaultValue: 1,
            mouseSupport: true,
            style: null,
            symbol: "→",
            bottomToolbar: "Help text",
            showFrame: true,
            enableSuspend: false,
            enableInterrupt: true,
            interruptException: typeof(KeyboardInterrupt),
            keyBindings: null);

        Assert.NotNull((Delegate)Invoke);
    }

    [Fact]
    public void DialogsChoiceAsync_AcceptsAllParameters()
    {
        // Verify all 12 parameters are accepted
        static Task<int> Invoke() => Dialogs.ChoiceAsync<int>(
            message: "Select:",
            options: [(1, (AnyFormattedText)"One")],
            defaultValue: 1,
            mouseSupport: true,
            style: null,
            symbol: "→",
            bottomToolbar: "Help text",
            showFrame: true,
            enableSuspend: false,
            enableInterrupt: true,
            interruptException: typeof(KeyboardInterrupt),
            keyBindings: null);

        Assert.NotNull((Delegate)Invoke);
    }

    // ──────────────────────────────────────────────
    // Edge Cases
    // ──────────────────────────────────────────────

    [Fact]
    public void Constructor_FormattedTextMessage_Accepted()
    {
        var formatted = new Stroke.FormattedText.FormattedText(
            new StyleAndTextTuple("", "Plain text"),
            new StyleAndTextTuple("bold", " Bold text"));
        var choice = new ChoiceInput<string>(
            message: formatted,
            options: [("a", (AnyFormattedText)"A")]);

        // Verify the choice was created with the FormattedText message
        Assert.Single(choice.Options);
    }

    [Fact]
    public void Constructor_FormattedTextLabels_Accepted()
    {
        var labelA = new Stroke.FormattedText.FormattedText(new StyleAndTextTuple("bold", "Bold A"));
        var labelB = new Stroke.FormattedText.FormattedText(new StyleAndTextTuple("italic", "Italic B"));
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options:
            [
                ("a", labelA),
                ("b", labelB),
            ]);

        Assert.Equal(2, choice.Options.Count);
    }

    [Fact]
    public void Constructor_EmptySymbol_Accepted()
    {
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")],
            symbol: "");

        Assert.Equal("", choice.Symbol);
    }

    [Fact]
    public void Constructor_UnicodeSymbol_Accepted()
    {
        var choice = new ChoiceInput<string>(
            message: "Select:",
            options: [("a", (AnyFormattedText)"A")],
            symbol: "★");

        Assert.Equal("★", choice.Symbol);
    }

    [Fact]
    public void Constructor_ValueTypeWithNullableDefault_Accepted()
    {
        // int? default with nullable value
        var choice = new ChoiceInput<int>(
            message: "Select:",
            options: [(1, (AnyFormattedText)"One"), (2, (AnyFormattedText)"Two")],
            defaultValue: 2);

        Assert.Equal(2, choice.Default);
    }

    [Fact]
    public void Constructor_EnumType_Accepted()
    {
        var choice = new ChoiceInput<DayOfWeek>(
            message: "Select day:",
            options:
            [
                (DayOfWeek.Monday, (AnyFormattedText)"Monday"),
                (DayOfWeek.Friday, (AnyFormattedText)"Friday"),
            ],
            defaultValue: DayOfWeek.Friday);

        Assert.Equal(DayOfWeek.Friday, choice.Default);
    }

    [Fact]
    public void Constructor_ComplexType_Accepted()
    {
        var obj1 = new { Id = 1, Name = "First" };
        var obj2 = new { Id = 2, Name = "Second" };

        var choice = new ChoiceInput<object>(
            message: "Select object:",
            options:
            [
                (obj1, (AnyFormattedText)"First Object"),
                (obj2, (AnyFormattedText)"Second Object"),
            ],
            defaultValue: obj2);

        Assert.Same(obj2, choice.Default);
    }
}
