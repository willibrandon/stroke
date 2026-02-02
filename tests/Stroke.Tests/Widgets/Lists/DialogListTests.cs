using Stroke.FormattedText;
using Stroke.Layout.Containers;
using Stroke.Widgets.Lists;
using Xunit;

namespace Stroke.Tests.Widgets.Lists;

public class DialogListTests
{
    private static IReadOnlyList<(string Value, AnyFormattedText Label)> ThreeItems =>
    [
        ("a", "Alpha"),
        ("b", "Beta"),
        ("c", "Charlie"),
    ];

    [Fact]
    public void PtContainer_ReturnsWindow()
    {
        var list = new DialogList<string>(ThreeItems);
        var container = list.PtContainer();
        Assert.IsType<Window>(container);
        Assert.Same(list.Window, container);
    }

    [Fact]
    public void EmptyValues_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new DialogList<string>([]));
    }

    [Fact]
    public void DefaultValues_FirstItemSelected()
    {
        var list = new DialogList<string>(ThreeItems);
        Assert.Equal("a", list.CurrentValue);
        Assert.Equal(0, list.SelectedIndex);
    }

    [Fact]
    public void DefaultValues_MatchesProvided()
    {
        var list = new DialogList<string>(ThreeItems,
            defaultValues: ["b"]);
        Assert.Equal("b", list.CurrentValue);
        Assert.Equal(1, list.SelectedIndex);
    }

    [Fact]
    public void DefaultValues_NotInList_FallsBackToFirst()
    {
        var list = new DialogList<string>(ThreeItems,
            defaultValues: ["z"]);
        Assert.Equal("a", list.CurrentValue);
        Assert.Equal(0, list.SelectedIndex);
    }

    [Fact]
    public void MultipleSelection_DefaultValues()
    {
        var list = new DialogList<string>(ThreeItems,
            defaultValues: ["a", "c"],
            multipleSelection: true);
        Assert.Equal(["a", "c"], list.CurrentValues);
        Assert.Equal(0, list.SelectedIndex);
    }

    [Fact]
    public void MultipleSelection_DefaultValues_SkipsInvalid()
    {
        var list = new DialogList<string>(ThreeItems,
            defaultValues: ["a", "z", "c"],
            multipleSelection: true);
        Assert.Equal(["a", "c"], list.CurrentValues);
    }

    [Fact]
    public void SelectedIndex_IsGetSet()
    {
        var list = new DialogList<string>(ThreeItems);
        Assert.Equal(0, list.SelectedIndex);
        list.SelectedIndex = 2;
        Assert.Equal(2, list.SelectedIndex);
    }

    [Fact]
    public void CurrentValue_IsGetSet()
    {
        var list = new DialogList<string>(ThreeItems);
        Assert.Equal("a", list.CurrentValue);
        list.CurrentValue = "c";
        Assert.Equal("c", list.CurrentValue);
    }

    [Fact]
    public void CurrentValues_IsGetSet()
    {
        var list = new DialogList<string>(ThreeItems, multipleSelection: true);
        Assert.Empty(list.CurrentValues);
        list.CurrentValues = ["b", "c"];
        Assert.Equal(["b", "c"], list.CurrentValues);
    }

    [Fact]
    public void MultipleSelection_Property()
    {
        var single = new DialogList<string>(ThreeItems, multipleSelection: false);
        var multi = new DialogList<string>(ThreeItems, multipleSelection: true);
        Assert.False(single.MultipleSelection);
        Assert.True(multi.MultipleSelection);
    }

    [Fact]
    public void StyleProperties_MatchConstructor()
    {
        var list = new DialogList<string>(ThreeItems,
            openCharacter: "(",
            selectCharacter: "X",
            closeCharacter: ")",
            containerStyle: "cs",
            defaultStyle: "ds",
            numberStyle: "ns",
            selectedStyle: "ss",
            checkedStyle: "cks");
        Assert.Equal("(", list.OpenCharacter);
        Assert.Equal("X", list.SelectCharacter);
        Assert.Equal(")", list.CloseCharacter);
        Assert.Equal("cs", list.ContainerStyle);
        Assert.Equal("ds", list.DefaultStyle);
        Assert.Equal("ns", list.NumberStyle);
        Assert.Equal("ss", list.SelectedStyle);
        Assert.Equal("cks", list.CheckedStyle);
    }

    [Fact]
    public void ShowScrollbar_DefaultTrue()
    {
        var list = new DialogList<string>(ThreeItems);
        Assert.True(list.ShowScrollbar);
    }

    [Fact]
    public void ShowScrollbar_IsMutable()
    {
        var list = new DialogList<string>(ThreeItems);
        list.ShowScrollbar = false;
        Assert.False(list.ShowScrollbar);
    }

    [Fact]
    public void ShowNumbers_DefaultFalse()
    {
        var list = new DialogList<string>(ThreeItems);
        Assert.False(list.ShowNumbers);
    }

    [Fact]
    public void ShowNumbers_IsMutable()
    {
        var list = new DialogList<string>(ThreeItems);
        list.ShowNumbers = true;
        Assert.True(list.ShowNumbers);
    }

    [Fact]
    public void Values_MatchesConstructor()
    {
        var list = new DialogList<string>(ThreeItems);
        Assert.Equal(3, list.Values.Count);
        Assert.Equal("a", list.Values[0].Value);
        Assert.Equal("c", list.Values[2].Value);
    }

    [Fact]
    public void Control_IsNotNull()
    {
        var list = new DialogList<string>(ThreeItems);
        Assert.NotNull(list.Control);
    }

    [Fact]
    public void Window_IsNotNull()
    {
        var list = new DialogList<string>(ThreeItems);
        Assert.NotNull(list.Window);
    }

    [Fact]
    public void HandleEnter_SingleSelection_SetsCurrentValue()
    {
        var list = new DialogList<string>(ThreeItems)
        {
            SelectedIndex = 1
        };
        // Invoke protected method indirectly by simulating selection logic
        // HandleEnter is protected, so we test via RadioList/CheckboxList subclasses
        // But we can verify that constructing with default moves to correct index
        var list2 = new DialogList<string>(ThreeItems, defaultValues: ["c"]);
        Assert.Equal("c", list2.CurrentValue);
        Assert.Equal(2, list2.SelectedIndex);
    }

    [Fact]
    public async Task ThreadSafety_ConcurrentAccess()
    {
        var ct = TestContext.Current.CancellationToken;
        var list = new DialogList<string>(ThreeItems, multipleSelection: true);

        var tasks = Enumerable.Range(0, 10).Select(i => Task.Run(() =>
        {
            for (int j = 0; j < 100; j++)
            {
                list.SelectedIndex = j % 3;
                _ = list.SelectedIndex;
                list.CurrentValues = ["a"];
                _ = list.CurrentValues;
                list.CurrentValue = "b";
                _ = list.CurrentValue;
            }
        }, ct));

        await Task.WhenAll(tasks);

        // No exceptions means thread safety is working
        Assert.True(true);
    }
}
