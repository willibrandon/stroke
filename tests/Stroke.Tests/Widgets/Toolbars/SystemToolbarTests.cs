using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Processors;
using Stroke.Widgets.Toolbars;
using Xunit;

// Alias to avoid ambiguity with System.Buffer
using Buffer = Stroke.Core.Buffer;

namespace Stroke.Tests.Widgets.Toolbars;

/// <summary>
/// Tests for SystemToolbar (system shell command toolbar with Emacs/Vi key bindings).
/// </summary>
public sealed class SystemToolbarTests
{
    #region Constructor Default Tests

    [Fact]
    public void Constructor_DefaultPrompt_IsShellCommand()
    {
        var toolbar = new SystemToolbar();

        Assert.Equal("Shell command: ", toolbar.Prompt.Value);
    }

    [Fact]
    public void Constructor_DefaultEnableGlobalBindings_IsAlways()
    {
        var toolbar = new SystemToolbar();

        Assert.IsType<Always>(toolbar.EnableGlobalBindings);
    }

    [Fact]
    public void Constructor_WithCustomPrompt_SetsPrompt()
    {
        var toolbar = new SystemToolbar(prompt: "Run: ");

        Assert.Equal("Run: ", toolbar.Prompt.Value);
    }

    [Fact]
    public void Constructor_WithEnableGlobalBindingsFalse_SetsNever()
    {
        var toolbar = new SystemToolbar(enableGlobalBindings: new FilterOrBool(false));

        Assert.IsType<Never>(toolbar.EnableGlobalBindings);
    }

    [Fact]
    public void Constructor_WithEnableGlobalBindingsTrue_SetsAlways()
    {
        var toolbar = new SystemToolbar(enableGlobalBindings: new FilterOrBool(true));

        Assert.IsType<Always>(toolbar.EnableGlobalBindings);
    }

    #endregion

    #region SystemBuffer Tests

    [Fact]
    public void SystemBuffer_HasSystemBufferName()
    {
        var toolbar = new SystemToolbar();

        Assert.Equal(BufferNames.System, toolbar.SystemBuffer.Name);
    }

    [Fact]
    public void SystemBuffer_NameEqualsSystemBuffer()
    {
        var toolbar = new SystemToolbar();

        Assert.Equal("SYSTEM_BUFFER", toolbar.SystemBuffer.Name);
    }

    [Fact]
    public void SystemBuffer_IsNotNull()
    {
        var toolbar = new SystemToolbar();

        Assert.NotNull(toolbar.SystemBuffer);
    }

    #endregion

    #region BufferControl Tests

    [Fact]
    public void BufferControl_IsNotNull()
    {
        var toolbar = new SystemToolbar();

        Assert.NotNull(toolbar.BufferControl);
    }

    [Fact]
    public void BufferControl_UsesSystemBuffer()
    {
        var toolbar = new SystemToolbar();

        Assert.Same(toolbar.SystemBuffer, toolbar.BufferControl.Buffer);
    }

    [Fact]
    public void BufferControl_HasInputProcessors()
    {
        var toolbar = new SystemToolbar();

        Assert.NotNull(toolbar.BufferControl.InputProcessors);
        Assert.NotEmpty(toolbar.BufferControl.InputProcessors);
    }

    [Fact]
    public void BufferControl_FirstInputProcessor_IsBeforeInput()
    {
        var toolbar = new SystemToolbar();

        var processors = toolbar.BufferControl.InputProcessors;
        Assert.NotNull(processors);
        Assert.IsType<BeforeInput>(processors[0]);
    }

    [Fact]
    public void BufferControl_BeforeInputProcessor_HasSystemToolbarStyle()
    {
        var toolbar = new SystemToolbar();

        var processors = toolbar.BufferControl.InputProcessors;
        Assert.NotNull(processors);
        var beforeInput = Assert.IsType<BeforeInput>(processors[0]);
        Assert.Equal("class:system-toolbar", beforeInput.Style);
    }

    [Fact]
    public void BufferControl_HasKeyBindings()
    {
        var toolbar = new SystemToolbar();

        var bindings = toolbar.BufferControl.GetKeyBindings();
        Assert.NotNull(bindings);
    }

    #endregion

    #region Window Tests

    [Fact]
    public void Window_IsNotNull()
    {
        var toolbar = new SystemToolbar();

        Assert.NotNull(toolbar.Window);
    }

    [Fact]
    public void Window_HeightPreferredIsOne()
    {
        var toolbar = new SystemToolbar();

        var preferredHeight = toolbar.Window.PreferredHeight(80, 24);
        Assert.Equal(1, preferredHeight.Preferred);
    }

    [Fact]
    public void Window_ContentIsBufferControl()
    {
        var toolbar = new SystemToolbar();

        Assert.Same(toolbar.BufferControl, toolbar.Window.Content);
    }

    #endregion

    #region Container Tests

    [Fact]
    public void Container_IsConditionalContainer()
    {
        var toolbar = new SystemToolbar();

        Assert.IsType<ConditionalContainer>(toolbar.Container);
    }

    [Fact]
    public void Container_IsNotNull()
    {
        var toolbar = new SystemToolbar();

        Assert.NotNull(toolbar.Container);
    }

    [Fact]
    public void Container_HasFilter()
    {
        var toolbar = new SystemToolbar();

        Assert.NotNull(toolbar.Container.Filter);
    }

    #endregion

    #region PtContainer Tests

    [Fact]
    public void PtContainer_ReturnsContainer()
    {
        var toolbar = new SystemToolbar();

        Assert.Same(toolbar.Container, toolbar.PtContainer());
    }

    [Fact]
    public void PtContainer_ReturnsIContainer()
    {
        var toolbar = new SystemToolbar();

        Assert.IsAssignableFrom<IContainer>(toolbar.PtContainer());
    }

    #endregion

    #region IMagicContainer Tests

    [Fact]
    public void SystemToolbar_ImplementsIMagicContainer()
    {
        var toolbar = new SystemToolbar();

        Assert.IsAssignableFrom<IMagicContainer>(toolbar);
    }

    #endregion

    #region Key Bindings Structure Tests

    [Fact]
    public void KeyBindings_IsMergedKeyBindings()
    {
        var toolbar = new SystemToolbar();

        var bindings = toolbar.BufferControl.GetKeyBindings();
        Assert.IsType<MergedKeyBindings>(bindings);
    }

    [Fact]
    public void KeyBindings_HasThreeRegistries()
    {
        var toolbar = new SystemToolbar();

        var merged = Assert.IsType<MergedKeyBindings>(toolbar.BufferControl.GetKeyBindings());
        Assert.Equal(3, merged.Registries.Count);
    }

    [Fact]
    public void KeyBindings_AllRegistriesAreConditionalKeyBindings()
    {
        var toolbar = new SystemToolbar();

        var merged = Assert.IsType<MergedKeyBindings>(toolbar.BufferControl.GetKeyBindings());
        for (int i = 0; i < merged.Registries.Count; i++)
        {
            Assert.IsType<ConditionalKeyBindings>(merged.Registries[i]);
        }
    }

    #endregion

    #region Emacs Bindings Tests

    [Fact]
    public void EmacsBindings_HasFourBindings()
    {
        var toolbar = new SystemToolbar();

        var merged = Assert.IsType<MergedKeyBindings>(toolbar.BufferControl.GetKeyBindings());
        var emacsConditional = Assert.IsType<ConditionalKeyBindings>(merged.Registries[0]);
        var emacsBindings = emacsConditional.Bindings;

        Assert.Equal(4, emacsBindings.Count);
    }

    [Fact]
    public void EmacsBindings_ContainsEscapeBinding()
    {
        var toolbar = new SystemToolbar();

        var emacsBindings = GetEmacsBindings(toolbar);
        var escapeBinding = emacsBindings.FirstOrDefault(b =>
            b.Keys.Count == 1 && b.Keys[0].IsKey && b.Keys[0].Key == Keys.Escape);

        Assert.NotNull(escapeBinding);
    }

    [Fact]
    public void EmacsBindings_ContainsControlGBinding()
    {
        var toolbar = new SystemToolbar();

        var emacsBindings = GetEmacsBindings(toolbar);
        var ctrlGBinding = emacsBindings.FirstOrDefault(b =>
            b.Keys.Count == 1 && b.Keys[0].IsKey && b.Keys[0].Key == Keys.ControlG);

        Assert.NotNull(ctrlGBinding);
    }

    [Fact]
    public void EmacsBindings_ContainsControlCBinding()
    {
        var toolbar = new SystemToolbar();

        var emacsBindings = GetEmacsBindings(toolbar);
        var ctrlCBinding = emacsBindings.FirstOrDefault(b =>
            b.Keys.Count == 1 && b.Keys[0].IsKey && b.Keys[0].Key == Keys.ControlC);

        Assert.NotNull(ctrlCBinding);
    }

    [Fact]
    public void EmacsBindings_ContainsControlMBinding()
    {
        var toolbar = new SystemToolbar();

        var emacsBindings = GetEmacsBindings(toolbar);
        var ctrlMBinding = emacsBindings.FirstOrDefault(b =>
            b.Keys.Count == 1 && b.Keys[0].IsKey && b.Keys[0].Key == Keys.ControlM);

        Assert.NotNull(ctrlMBinding);
    }

    #endregion

    #region Vi Bindings Tests

    [Fact]
    public void ViBindings_HasThreeBindings()
    {
        var toolbar = new SystemToolbar();

        var merged = Assert.IsType<MergedKeyBindings>(toolbar.BufferControl.GetKeyBindings());
        var viConditional = Assert.IsType<ConditionalKeyBindings>(merged.Registries[1]);
        var viBindings = viConditional.Bindings;

        Assert.Equal(3, viBindings.Count);
    }

    [Fact]
    public void ViBindings_ContainsEscapeBinding()
    {
        var toolbar = new SystemToolbar();

        var viBindings = GetViBindings(toolbar);
        var escapeBinding = viBindings.FirstOrDefault(b =>
            b.Keys.Count == 1 && b.Keys[0].IsKey && b.Keys[0].Key == Keys.Escape);

        Assert.NotNull(escapeBinding);
    }

    [Fact]
    public void ViBindings_ContainsControlCBinding()
    {
        var toolbar = new SystemToolbar();

        var viBindings = GetViBindings(toolbar);
        var ctrlCBinding = viBindings.FirstOrDefault(b =>
            b.Keys.Count == 1 && b.Keys[0].IsKey && b.Keys[0].Key == Keys.ControlC);

        Assert.NotNull(ctrlCBinding);
    }

    [Fact]
    public void ViBindings_ContainsControlMBinding()
    {
        var toolbar = new SystemToolbar();

        var viBindings = GetViBindings(toolbar);
        var ctrlMBinding = viBindings.FirstOrDefault(b =>
            b.Keys.Count == 1 && b.Keys[0].IsKey && b.Keys[0].Key == Keys.ControlM);

        Assert.NotNull(ctrlMBinding);
    }

    #endregion

    #region Global Bindings Tests

    [Fact]
    public void GlobalBindings_HasTwoBindings()
    {
        var toolbar = new SystemToolbar();

        var merged = Assert.IsType<MergedKeyBindings>(toolbar.BufferControl.GetKeyBindings());
        var globalConditional = Assert.IsType<ConditionalKeyBindings>(merged.Registries[2]);
        var globalBindings = globalConditional.Bindings;

        Assert.Equal(2, globalBindings.Count);
    }

    [Fact]
    public void GlobalBindings_EmacsBinding_HasEscapeExclamationSequence()
    {
        var toolbar = new SystemToolbar();

        var globalBindings = GetGlobalBindings(toolbar);
        var emacsGlobal = globalBindings.FirstOrDefault(b =>
            b.Keys.Count == 2 &&
            b.Keys[0].IsKey && b.Keys[0].Key == Keys.Escape &&
            b.Keys[1].IsChar && b.Keys[1].Char == '!');

        Assert.NotNull(emacsGlobal);
    }

    [Fact]
    public void GlobalBindings_ViBinding_HasExclamationChar()
    {
        var toolbar = new SystemToolbar();

        var globalBindings = GetGlobalBindings(toolbar);
        var viGlobal = globalBindings.FirstOrDefault(b =>
            b.Keys.Count == 1 &&
            b.Keys[0].IsChar && b.Keys[0].Char == '!');

        Assert.NotNull(viGlobal);
    }

    [Fact]
    public void GlobalBindings_BothBindingsAreGlobal()
    {
        var toolbar = new SystemToolbar();

        var globalBindings = GetGlobalBindings(toolbar);
        foreach (var binding in globalBindings)
        {
            Assert.IsType<Always>(binding.IsGlobal);
        }
    }

    #endregion

    #region Prompt and EnableGlobalBindings Property Tests

    [Fact]
    public void Prompt_DefaultValue_IsShellCommandString()
    {
        var toolbar = new SystemToolbar();

        Assert.False(toolbar.Prompt.IsEmpty);
        Assert.Equal("Shell command: ", toolbar.Prompt.Value);
    }

    [Fact]
    public void Prompt_CustomValue_IsPreserved()
    {
        var toolbar = new SystemToolbar(prompt: "Command> ");

        Assert.Equal("Command> ", toolbar.Prompt.Value);
    }

    [Fact]
    public void Prompt_EmptyString_UsesDefault()
    {
        // Empty AnyFormattedText (default) should result in the default prompt
        var toolbar = new SystemToolbar(prompt: default);

        Assert.Equal("Shell command: ", toolbar.Prompt.Value);
    }

    [Fact]
    public void EnableGlobalBindings_Default_IsAlways()
    {
        var toolbar = new SystemToolbar();

        Assert.IsType<Always>(toolbar.EnableGlobalBindings);
    }

    [Fact]
    public void EnableGlobalBindings_WithCustomFilter_PreservesFilter()
    {
        var customFilter = new Condition(() => false);
        var toolbar = new SystemToolbar(enableGlobalBindings: new FilterOrBool(customFilter));

        // The filter should be the custom filter (wrapped via FilterUtils.ToFilter)
        Assert.NotNull(toolbar.EnableGlobalBindings);
    }

    [Fact]
    public void EnableGlobalBindings_WithTrue_IsAlways()
    {
        var toolbar = new SystemToolbar(enableGlobalBindings: new FilterOrBool(true));

        Assert.IsType<Always>(toolbar.EnableGlobalBindings);
    }

    [Fact]
    public void EnableGlobalBindings_WithFalse_IsNever()
    {
        var toolbar = new SystemToolbar(enableGlobalBindings: new FilterOrBool(false));

        Assert.IsType<Never>(toolbar.EnableGlobalBindings);
    }

    #endregion

    #region Merged Key Bindings Type Verification

    [Fact]
    public void MergedKeyBindings_FirstRegistry_IsEmacsConditional()
    {
        var toolbar = new SystemToolbar();

        var merged = Assert.IsType<MergedKeyBindings>(toolbar.BufferControl.GetKeyBindings());
        var first = Assert.IsType<ConditionalKeyBindings>(merged.Registries[0]);

        // The Emacs conditional should have a filter -- we cannot directly check
        // EmacsFilters.EmacsMode identity due to composition, but we can check
        // the filter is not null.
        Assert.NotNull(first.Filter);
    }

    [Fact]
    public void MergedKeyBindings_SecondRegistry_IsViConditional()
    {
        var toolbar = new SystemToolbar();

        var merged = Assert.IsType<MergedKeyBindings>(toolbar.BufferControl.GetKeyBindings());
        var second = Assert.IsType<ConditionalKeyBindings>(merged.Registries[1]);

        Assert.NotNull(second.Filter);
    }

    [Fact]
    public void MergedKeyBindings_ThirdRegistry_IsGlobalConditional()
    {
        var toolbar = new SystemToolbar();

        var merged = Assert.IsType<MergedKeyBindings>(toolbar.BufferControl.GetKeyBindings());
        var third = Assert.IsType<ConditionalKeyBindings>(merged.Registries[2]);

        Assert.NotNull(third.Filter);
    }

    #endregion

    #region Combined Binding Count Test

    [Fact]
    public void AllBindings_TotalCount_IsNine()
    {
        // 4 emacs + 3 vi + 2 global = 9 bindings total
        var toolbar = new SystemToolbar();

        var merged = Assert.IsType<MergedKeyBindings>(toolbar.BufferControl.GetKeyBindings());
        var totalBindings = merged.Bindings;

        Assert.Equal(9, totalBindings.Count);
    }

    #endregion

    #region Helpers

    private static IReadOnlyList<Binding> GetEmacsBindings(SystemToolbar toolbar)
    {
        var merged = (MergedKeyBindings)toolbar.BufferControl.GetKeyBindings()!;
        var emacsConditional = (ConditionalKeyBindings)merged.Registries[0];
        return emacsConditional.Bindings;
    }

    private static IReadOnlyList<Binding> GetViBindings(SystemToolbar toolbar)
    {
        var merged = (MergedKeyBindings)toolbar.BufferControl.GetKeyBindings()!;
        var viConditional = (ConditionalKeyBindings)merged.Registries[1];
        return viConditional.Bindings;
    }

    private static IReadOnlyList<Binding> GetGlobalBindings(SystemToolbar toolbar)
    {
        var merged = (MergedKeyBindings)toolbar.BufferControl.GetKeyBindings()!;
        var globalConditional = (ConditionalKeyBindings)merged.Registries[2];
        return globalConditional.Bindings;
    }

    #endregion
}
