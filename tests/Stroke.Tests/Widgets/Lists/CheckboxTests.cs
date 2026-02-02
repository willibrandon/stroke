using Stroke.Widgets.Lists;
using Xunit;

namespace Stroke.Tests.Widgets.Lists;

public class CheckboxTests
{
    [Fact]
    public void DefaultCheckbox_UncheckedByDefault()
    {
        var cb = new Checkbox("Accept terms");
        Assert.False(cb.Checked);
    }

    [Fact]
    public void InitialChecked_True()
    {
        var cb = new Checkbox("Accept terms", @checked: true);
        Assert.True(cb.Checked);
    }

    [Fact]
    public void Checked_SetToTrue_AddsValue()
    {
        var cb = new Checkbox("Accept terms");
        cb.Checked = true;
        Assert.True(cb.Checked);
        Assert.Contains("value", cb.CurrentValues);
    }

    [Fact]
    public void Checked_SetToFalse_RemovesValue()
    {
        var cb = new Checkbox("Accept terms", @checked: true);
        cb.Checked = false;
        Assert.False(cb.Checked);
        Assert.Empty(cb.CurrentValues);
    }

    [Fact]
    public void ShowScrollbar_AlwaysFalse()
    {
        var cb = new Checkbox("Test");
        Assert.False(cb.ShowScrollbar);
        cb.ShowScrollbar = true; // Should be ignored
        Assert.False(cb.ShowScrollbar);
    }

    [Fact]
    public void PtContainer_ReturnsWindow()
    {
        var cb = new Checkbox("Test");
        Assert.Same(cb.Window, cb.PtContainer());
    }

    [Fact]
    public void SingleItemInValues()
    {
        var cb = new Checkbox("Test");
        var item = Assert.Single(cb.Values);
        Assert.Equal("value", item.Value);
    }

    [Fact]
    public void IsCheckboxList()
    {
        var cb = new Checkbox("Test");
        Assert.IsType<CheckboxList<string>>(cb, exactMatch: false);
    }

    [Fact]
    public void EmptyText_StillCreates()
    {
        var cb = new Checkbox();
        Assert.NotNull(cb);
        Assert.False(cb.Checked);
    }

    [Fact]
    public void Toggle_Checked()
    {
        var cb = new Checkbox("Test");
        Assert.False(cb.Checked);
        cb.Checked = true;
        Assert.True(cb.Checked);
        cb.Checked = false;
        Assert.False(cb.Checked);
        cb.Checked = true;
        Assert.True(cb.Checked);
    }
}
