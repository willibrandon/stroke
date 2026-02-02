using Stroke.FormattedText;
using Stroke.Widgets.Lists;
using Xunit;

namespace Stroke.Tests.Widgets.Lists;

public class RadioListTests
{
    private static IReadOnlyList<(string Value, AnyFormattedText Label)> ThreeItems =>
    [
        ("a", "Alpha"),
        ("b", "Beta"),
        ("c", "Charlie"),
    ];

    [Fact]
    public void DefaultStyles_AreRadioStyles()
    {
        var radio = new RadioList<string>(ThreeItems);
        Assert.Equal("class:radio-list", radio.ContainerStyle);
        Assert.Equal("class:radio", radio.DefaultStyle);
        Assert.Equal("class:radio-selected", radio.SelectedStyle);
        Assert.Equal("class:radio-checked", radio.CheckedStyle);
        Assert.Equal("class:radio-number", radio.NumberStyle);
    }

    [Fact]
    public void DefaultCharacters_AreParentheses()
    {
        var radio = new RadioList<string>(ThreeItems);
        Assert.Equal("(", radio.OpenCharacter);
        Assert.Equal("*", radio.SelectCharacter);
        Assert.Equal(")", radio.CloseCharacter);
    }

    [Fact]
    public void MultipleSelection_AlwaysFalse()
    {
        // Even when multipleSelection: true is passed, RadioList always sets it to false
        var radio = new RadioList<string>(ThreeItems, multipleSelection: true);
        Assert.False(radio.MultipleSelection);
    }

    [Fact]
    public void Default_SetsCurrentValue()
    {
        var radio = new RadioList<string>(ThreeItems, @default: "b");
        Assert.Equal("b", radio.CurrentValue);
        Assert.Equal(1, radio.SelectedIndex);
    }

    [Fact]
    public void Default_Null_UsesFirstItem()
    {
        var radio = new RadioList<string>(ThreeItems);
        Assert.Equal("a", radio.CurrentValue);
        Assert.Equal(0, radio.SelectedIndex);
    }

    [Fact]
    public void Default_NotInList_UsesFirstItem()
    {
        var radio = new RadioList<string>(ThreeItems, @default: "z");
        Assert.Equal("a", radio.CurrentValue);
    }

    [Fact]
    public void IsDialogList()
    {
        var radio = new RadioList<string>(ThreeItems);
        Assert.IsType<DialogList<string>>(radio, exactMatch: false);
    }

    [Fact]
    public void ShowNumbers_PassedToBase()
    {
        var radio = new RadioList<string>(ThreeItems, showNumbers: true);
        Assert.True(radio.ShowNumbers);
    }

    [Fact]
    public void PtContainer_ReturnsWindow()
    {
        var radio = new RadioList<string>(ThreeItems);
        Assert.Same(radio.Window, radio.PtContainer());
    }

    [Fact]
    public void IntValues_WorkCorrectly()
    {
        IReadOnlyList<(int Value, AnyFormattedText Label)> items =
        [
            (1, "One"),
            (2, "Two"),
            (3, "Three"),
        ];
        var radio = new RadioList<int>(items, @default: 2);
        Assert.Equal(2, radio.CurrentValue);
        Assert.Equal(1, radio.SelectedIndex);
    }
}
