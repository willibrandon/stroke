using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Xunit;

using StrokeLayout = Stroke.Layout.Layout;

namespace Stroke.Tests.Layout;

/// <summary>
/// Tests for Layout.SearchTargetBufferControl property (T046).
/// </summary>
public class LayoutSearchTargetTests
{
    [Fact]
    public void SearchTargetBufferControl_NullWhenNotSearching()
    {
        // When focused on a regular BufferControl (not SearchBufferControl),
        // SearchTargetBufferControl returns null
        var bc = new BufferControl();
        var window = new Window(bc);
        var layout = new StrokeLayout(new AnyContainer(window));

        Assert.Null(layout.SearchTargetBufferControl);
    }

    [Fact]
    public void SearchTargetBufferControl_ReturnsBufferControlWhenSearching()
    {
        // When focused on a SearchBufferControl that is linked, returns the target
        var bc = new BufferControl();
        var sbc = new SearchBufferControl();
        var mainWindow = new Window(bc);
        var searchWindow = new Window(sbc);

        var container = new HSplit(children: [mainWindow, searchWindow]);
        var layout = new StrokeLayout(new AnyContainer(container));

        // Link the search control to the buffer control (internal method, accessible via InternalsVisibleTo)
        layout.AddSearchLink(sbc, bc);

        // Focus the search window
        layout.Focus(new FocusableElement(searchWindow));

        Assert.Same(bc, layout.SearchTargetBufferControl);
    }

    [Fact]
    public void SearchTargetBufferControl_NullWhenSearchControlNotLinked()
    {
        // SearchBufferControl is focused but not in SearchLinks → null
        var sbc = new SearchBufferControl();
        var window = new Window(sbc);
        var layout = new StrokeLayout(new AnyContainer(window));

        Assert.Null(layout.SearchTargetBufferControl);
    }
}
