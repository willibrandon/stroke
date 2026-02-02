using Stroke.FormattedText;
using Stroke.Layout.Containers;
using Stroke.Widgets.Base;
using Stroke.Widgets.Dialogs;
using Stroke.Widgets.Lists;
using Xunit;

namespace Stroke.Tests.Widgets.Base;

/// <summary>
/// Verifies all 15 widget classes implement IMagicContainer and can be
/// embedded in HSplit and VSplit containers.
/// </summary>
public class IMagicContainerIntegrationTests
{
    private static IReadOnlyList<(string Value, AnyFormattedText Label)> SampleValues =>
    [
        ("a", "Alpha"),
        ("b", "Beta"),
    ];

    [Fact]
    public void AllWidgets_PtContainer_ReturnsNonNull()
    {
        var body = new AnyContainer(new Window());

        IMagicContainer[] widgets =
        [
            new Label("Test"),
            new Box(body),
            new ProgressBar(),
            new VerticalLine(),
            new HorizontalLine(),
            new TextArea(),
            new Button("OK"),
            new Frame(body),
            new Shadow(body),
            new DialogList<string>(SampleValues),
            new RadioList<string>(SampleValues),
            new CheckboxList<string>(SampleValues),
            new Checkbox("Test"),
            new Dialog(body),
            new Dialog(body, buttons: [new Button("OK")]),
        ];

        foreach (var widget in widgets)
        {
            Assert.NotNull(widget.PtContainer());
        }
    }

    [Fact]
    public void AllWidgets_EmbedInHSplit()
    {
        var body = new AnyContainer(new Window());

        // Each widget's PtContainer() should be embeddable in HSplit
        IContainer[] containers =
        [
            new Label("Test").PtContainer(),
            new Box(body).PtContainer(),
            new ProgressBar().PtContainer(),
            new VerticalLine().PtContainer(),
            new HorizontalLine().PtContainer(),
            new Button("OK").PtContainer(),
            new Frame(body).PtContainer(),
            new Shadow(body).PtContainer(),
            new RadioList<string>(SampleValues).PtContainer(),
            new CheckboxList<string>(SampleValues).PtContainer(),
            new Checkbox("Test").PtContainer(),
            new Dialog(body).PtContainer(),
        ];

        var hsplit = new HSplit(children: containers);
        Assert.Equal(containers.Length, hsplit.Children.Count);
    }

    [Fact]
    public void AllWidgets_EmbedInVSplit()
    {
        var body = new AnyContainer(new Window());

        IContainer[] containers =
        [
            new Label("Test").PtContainer(),
            new Box(body).PtContainer(),
            new ProgressBar().PtContainer(),
            new VerticalLine().PtContainer(),
            new HorizontalLine().PtContainer(),
            new Button("OK").PtContainer(),
            new Frame(body).PtContainer(),
            new Shadow(body).PtContainer(),
            new RadioList<string>(SampleValues).PtContainer(),
            new CheckboxList<string>(SampleValues).PtContainer(),
            new Checkbox("Test").PtContainer(),
            new Dialog(body).PtContainer(),
        ];

        var vsplit = new VSplit(children: containers);
        Assert.Equal(containers.Length, vsplit.Children.Count);
    }
}
