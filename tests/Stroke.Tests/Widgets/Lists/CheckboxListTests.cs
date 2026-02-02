using Stroke.FormattedText;
using Stroke.Widgets.Lists;
using Xunit;

namespace Stroke.Tests.Widgets.Lists;

public class CheckboxListTests
{
    private static IReadOnlyList<(string Value, AnyFormattedText Label)> ThreeItems =>
    [
        ("a", "Alpha"),
        ("b", "Beta"),
        ("c", "Charlie"),
    ];

    [Fact]
    public void DefaultStyles_AreCheckboxStyles()
    {
        var cb = new CheckboxList<string>(ThreeItems);
        Assert.Equal("class:checkbox-list", cb.ContainerStyle);
        Assert.Equal("class:checkbox", cb.DefaultStyle);
        Assert.Equal("class:checkbox-selected", cb.SelectedStyle);
        Assert.Equal("class:checkbox-checked", cb.CheckedStyle);
    }

    [Fact]
    public void DefaultCharacters_AreBrackets()
    {
        var cb = new CheckboxList<string>(ThreeItems);
        Assert.Equal("[", cb.OpenCharacter);
        Assert.Equal("*", cb.SelectCharacter);
        Assert.Equal("]", cb.CloseCharacter);
    }

    [Fact]
    public void MultipleSelection_AlwaysTrue()
    {
        var cb = new CheckboxList<string>(ThreeItems);
        Assert.True(cb.MultipleSelection);
    }

    [Fact]
    public void DefaultValues_Applied()
    {
        var cb = new CheckboxList<string>(ThreeItems,
            defaultValues: ["a", "c"]);
        Assert.Equal(["a", "c"], cb.CurrentValues);
        Assert.Equal(0, cb.SelectedIndex);
    }

    [Fact]
    public void DefaultValues_Null_EmptyCurrentValues()
    {
        var cb = new CheckboxList<string>(ThreeItems);
        Assert.Empty(cb.CurrentValues);
    }

    [Fact]
    public void DefaultValues_InvalidFiltered()
    {
        var cb = new CheckboxList<string>(ThreeItems,
            defaultValues: ["a", "z", "c"]);
        Assert.Equal(["a", "c"], cb.CurrentValues);
    }

    [Fact]
    public void IsDialogList()
    {
        var cb = new CheckboxList<string>(ThreeItems);
        Assert.IsType<DialogList<string>>(cb, exactMatch: false);
    }

    [Fact]
    public void PtContainer_ReturnsWindow()
    {
        var cb = new CheckboxList<string>(ThreeItems);
        Assert.Same(cb.Window, cb.PtContainer());
    }

    [Fact]
    public void CurrentValues_Mutable()
    {
        var cb = new CheckboxList<string>(ThreeItems);
        Assert.Empty(cb.CurrentValues);
        cb.CurrentValues = ["b"];
        Assert.Equal(["b"], cb.CurrentValues);
    }

    [Fact]
    public void IntValues_WorkCorrectly()
    {
        IReadOnlyList<(int Value, AnyFormattedText Label)> items =
        [
            (10, "Ten"),
            (20, "Twenty"),
            (30, "Thirty"),
        ];
        var cb = new CheckboxList<int>(items, defaultValues: [10, 30]);
        Assert.Equal([10, 30], cb.CurrentValues);
    }
}
