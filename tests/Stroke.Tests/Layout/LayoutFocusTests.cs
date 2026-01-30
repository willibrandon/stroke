using Stroke.Core;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Xunit;

using Buffer = Stroke.Core.Buffer;
using StrokeLayout = Stroke.Layout.Layout;

namespace Stroke.Tests.Layout;

public class LayoutFocusTests
{
    // --- Helper methods ---

    private static Window CreateFocusableWindow()
    {
        var control = new FormattedTextControl(
            Array.Empty<StyleAndTextTuple>(),
            focusable: new FilterOrBool(true));
        return new Window(content: control);
    }

    private static Window CreateNonFocusableWindow()
    {
        var control = new FormattedTextControl(
            Array.Empty<StyleAndTextTuple>());
        // FormattedTextControl defaults to focusable=false
        return new Window(content: control);
    }

    private static Window CreateBufferWindow(string bufferName = "")
    {
        var buffer = new Buffer(name: bufferName);
        var control = new BufferControl(buffer: buffer);
        return new Window(content: control);
    }

    // --- Constructor tests ---

    [Fact]
    public void Constructor_ValidatesAtLeastOneWindow()
    {
        // An empty HSplit has no windows
        var hsplit = new HSplit(children: []);

        Assert.Throws<InvalidLayoutException>(
            () => new StrokeLayout(new AnyContainer(hsplit)));
    }

    [Fact]
    public void Constructor_SingleWindow_FocusesIt()
    {
        var window = CreateFocusableWindow();
        var layout = new StrokeLayout(new AnyContainer(window));

        Assert.Same(window, layout.CurrentWindow);
    }

    [Fact]
    public void Constructor_MultipleWindows_FocusesFirst()
    {
        var w1 = CreateFocusableWindow();
        var w2 = CreateFocusableWindow();
        var hsplit = new HSplit(children: [w1, w2]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));

        Assert.Same(w1, layout.CurrentWindow);
    }

    [Fact]
    public void Constructor_WithFocusedElement_Window()
    {
        var w1 = CreateFocusableWindow();
        var w2 = CreateFocusableWindow();
        var hsplit = new HSplit(children: [w1, w2]);
        var layout = new StrokeLayout(
            new AnyContainer(hsplit),
            focusedElement: new FocusableElement(w2));

        Assert.Same(w2, layout.CurrentWindow);
    }

    [Fact]
    public void Constructor_WithFocusedElement_UIControl()
    {
        var control = new BufferControl();
        var w1 = CreateFocusableWindow();
        var w2 = new Window(content: control);
        var hsplit = new HSplit(children: [w1, w2]);
        var layout = new StrokeLayout(
            new AnyContainer(hsplit),
            focusedElement: new FocusableElement(control));

        Assert.Same(w2, layout.CurrentWindow);
    }

    [Fact]
    public void Constructor_WithFocusedElement_Buffer()
    {
        var buffer = new Buffer(name: "my-buffer");
        var control = new BufferControl(buffer: buffer);
        var w1 = CreateFocusableWindow();
        var w2 = new Window(content: control);
        var hsplit = new HSplit(children: [w1, w2]);
        var layout = new StrokeLayout(
            new AnyContainer(hsplit),
            focusedElement: new FocusableElement(buffer));

        Assert.Same(w2, layout.CurrentWindow);
    }

    [Fact]
    public void Constructor_WithFocusedElement_BufferName()
    {
        var buffer = new Buffer(name: "test-buffer");
        var control = new BufferControl(buffer: buffer);
        var w1 = CreateFocusableWindow();
        var w2 = new Window(content: control);
        var hsplit = new HSplit(children: [w1, w2]);
        var layout = new StrokeLayout(
            new AnyContainer(hsplit),
            focusedElement: new FocusableElement("test-buffer"));

        Assert.Same(w2, layout.CurrentWindow);
    }

    [Fact]
    public void Constructor_OnlyNonFocusableWindows_FocusesFirstWindow()
    {
        // Layout with non-focusable windows should still work —
        // the constructor focuses the first Window regardless of focusability
        var w1 = CreateNonFocusableWindow();
        var w2 = CreateNonFocusableWindow();
        var hsplit = new HSplit(children: [w1, w2]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));

        Assert.Same(w1, layout.CurrentWindow);
    }

    // --- CurrentWindow get/set ---

    [Fact]
    public void CurrentWindow_SetEquivalenceWithFocus()
    {
        var w1 = CreateFocusableWindow();
        var w2 = CreateFocusableWindow();
        var hsplit = new HSplit(children: [w1, w2]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));

        Assert.Same(w1, layout.CurrentWindow);

        // Setting CurrentWindow is equivalent to Focus()
        layout.CurrentWindow = w2;
        Assert.Same(w2, layout.CurrentWindow);

        layout.Focus(new FocusableElement(w1));
        Assert.Same(w1, layout.CurrentWindow);
    }

    // --- Focus ---

    [Fact]
    public void Focus_Window()
    {
        var w1 = CreateFocusableWindow();
        var w2 = CreateFocusableWindow();
        var hsplit = new HSplit(children: [w1, w2]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));

        layout.Focus(new FocusableElement(w2));
        Assert.Same(w2, layout.CurrentWindow);
    }

    [Fact]
    public void Focus_UIControl()
    {
        var control = new BufferControl();
        var w1 = CreateFocusableWindow();
        var w2 = new Window(content: control);
        var hsplit = new HSplit(children: [w1, w2]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));

        layout.Focus(new FocusableElement(control));
        Assert.Same(w2, layout.CurrentWindow);
    }

    [Fact]
    public void Focus_Buffer()
    {
        var buffer = new Buffer(name: "buf");
        var control = new BufferControl(buffer: buffer);
        var w1 = CreateFocusableWindow();
        var w2 = new Window(content: control);
        var hsplit = new HSplit(children: [w1, w2]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));

        layout.Focus(new FocusableElement(buffer));
        Assert.Same(w2, layout.CurrentWindow);
    }

    [Fact]
    public void Focus_BufferName()
    {
        var buffer = new Buffer(name: "named");
        var control = new BufferControl(buffer: buffer);
        var w1 = CreateFocusableWindow();
        var w2 = new Window(content: control);
        var hsplit = new HSplit(children: [w1, w2]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));

        layout.Focus(new FocusableElement("named"));
        Assert.Same(w2, layout.CurrentWindow);
    }

    [Fact]
    public void Focus_AnyContainer()
    {
        var w1 = CreateFocusableWindow();
        var w2 = CreateFocusableWindow();
        var innerSplit = new HSplit(children: [w2]);
        var outerSplit = new HSplit(children: [w1, innerSplit]);
        var layout = new StrokeLayout(new AnyContainer(outerSplit));

        layout.Focus(new FocusableElement(new AnyContainer(innerSplit)));
        Assert.Same(w2, layout.CurrentWindow);
    }

    [Fact]
    public void Focus_NonFocusable_Throws()
    {
        var control = new FormattedTextControl(
            Array.Empty<StyleAndTextTuple>());
        var w1 = new Window(content: control);
        var w2 = CreateFocusableWindow();
        var hsplit = new HSplit(children: [w1, w2]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));

        // Trying to focus a non-focusable UIControl should throw
        Assert.Throws<InvalidOperationException>(
            () => layout.Focus(new FocusableElement(control)));
    }

    [Fact]
    public void Focus_WindowNotInLayout_Throws()
    {
        var w1 = CreateFocusableWindow();
        var layout = new StrokeLayout(new AnyContainer(w1));

        var outsideWindow = CreateFocusableWindow();
        Assert.Throws<InvalidOperationException>(
            () => layout.Focus(new FocusableElement(outsideWindow)));
    }

    [Fact]
    public void Focus_BufferNameNotInLayout_Throws()
    {
        var w1 = CreateFocusableWindow();
        var layout = new StrokeLayout(new AnyContainer(w1));

        Assert.Throws<InvalidOperationException>(
            () => layout.Focus(new FocusableElement("nonexistent")));
    }

    [Fact]
    public void Focus_NonVisibleWindow_Succeeds()
    {
        // Focus on a window that exists in the layout tree but isn't
        // in VisibleWindows should still succeed
        var w1 = CreateFocusableWindow();
        var w2 = CreateFocusableWindow();
        var hsplit = new HSplit(children: [w1, w2]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));

        // VisibleWindows is empty before any render
        Assert.Empty(layout.VisibleWindows);

        // Focus w2 should succeed even though it's not "visible"
        layout.Focus(new FocusableElement(w2));
        Assert.Same(w2, layout.CurrentWindow);
    }

    // --- FocusPrevious / FocusNext ---

    [Fact]
    public void FocusPrevious_FocusNext_Cycle()
    {
        var w1 = CreateFocusableWindow();
        var w2 = CreateFocusableWindow();
        var w3 = CreateFocusableWindow();
        var hsplit = new HSplit(children: [w1, w2, w3]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));

        // FocusPrevious/FocusNext depend on VisibleWindows.
        // We need to populate them for the cycle to work.
        layout.SetVisibleWindows([w1, w2, w3]);
        layout.UpdateParentsRelations();

        Assert.Same(w1, layout.CurrentWindow);

        layout.FocusNext();
        Assert.Same(w2, layout.CurrentWindow);

        layout.FocusNext();
        Assert.Same(w3, layout.CurrentWindow);

        // Wrap around
        layout.FocusNext();
        Assert.Same(w1, layout.CurrentWindow);

        // Now go backwards
        layout.FocusPrevious();
        Assert.Same(w3, layout.CurrentWindow);

        layout.FocusPrevious();
        Assert.Same(w2, layout.CurrentWindow);

        layout.FocusPrevious();
        Assert.Same(w1, layout.CurrentWindow);
    }

    // --- FocusLast ---

    [Fact]
    public void FocusLast_ReturnsPreviouslyFocused()
    {
        var w1 = CreateFocusableWindow();
        var w2 = CreateFocusableWindow();
        var hsplit = new HSplit(children: [w1, w2]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));

        Assert.Same(w1, layout.CurrentWindow);

        layout.Focus(new FocusableElement(w2));
        Assert.Same(w2, layout.CurrentWindow);

        layout.FocusLast();
        Assert.Same(w1, layout.CurrentWindow);
    }

    [Fact]
    public void FocusLast_SingleWindow_RemainsOnSame()
    {
        var w1 = CreateFocusableWindow();
        var layout = new StrokeLayout(new AnyContainer(w1));

        Assert.Same(w1, layout.CurrentWindow);

        // FocusLast with only one item in stack stays on it
        layout.FocusLast();
        Assert.Same(w1, layout.CurrentWindow);
    }

    // --- HasFocus ---

    [Fact]
    public void HasFocus_Window()
    {
        var w1 = CreateFocusableWindow();
        var w2 = CreateFocusableWindow();
        var hsplit = new HSplit(children: [w1, w2]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));

        Assert.True(layout.HasFocus(w1));
        Assert.False(layout.HasFocus(w2));

        layout.Focus(new FocusableElement(w2));
        Assert.False(layout.HasFocus(w1));
        Assert.True(layout.HasFocus(w2));
    }

    [Fact]
    public void HasFocus_UIControl()
    {
        var control1 = new BufferControl();
        var control2 = new BufferControl();
        var w1 = new Window(content: control1);
        var w2 = new Window(content: control2);
        var hsplit = new HSplit(children: [w1, w2]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));

        Assert.True(layout.HasFocus(new FocusableElement(control1)));
        Assert.False(layout.HasFocus(new FocusableElement(control2)));
    }

    [Fact]
    public void HasFocus_Buffer()
    {
        var buffer1 = new Buffer(name: "b1");
        var buffer2 = new Buffer(name: "b2");
        var w1 = new Window(content: new BufferControl(buffer: buffer1));
        var w2 = new Window(content: new BufferControl(buffer: buffer2));
        var hsplit = new HSplit(children: [w1, w2]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));

        Assert.True(layout.HasFocus(new FocusableElement(buffer1)));
        Assert.False(layout.HasFocus(new FocusableElement(buffer2)));
    }

    [Fact]
    public void HasFocus_BufferName()
    {
        var buffer = new Buffer(name: "target");
        var w1 = new Window(content: new BufferControl(buffer: buffer));
        var w2 = CreateFocusableWindow();
        var hsplit = new HSplit(children: [w1, w2]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));

        Assert.True(layout.HasFocus(new FocusableElement("target")));
        Assert.False(layout.HasFocus(new FocusableElement("other")));
    }

    [Fact]
    public void HasFocus_AnyContainer()
    {
        var w1 = CreateFocusableWindow();
        var w2 = CreateFocusableWindow();
        var innerSplit = new HSplit(children: [w2]);
        var outerSplit = new HSplit(children: [w1, innerSplit]);
        var layout = new StrokeLayout(new AnyContainer(outerSplit));

        // w1 is focused, it's in outerSplit (not innerSplit)
        Assert.False(layout.HasFocus(new FocusableElement(new AnyContainer(innerSplit))));

        layout.Focus(new FocusableElement(w2));
        Assert.True(layout.HasFocus(new FocusableElement(new AnyContainer(innerSplit))));
    }

    // --- CurrentControl, CurrentBuffer, BufferHasFocus ---

    [Fact]
    public void CurrentControl_ReturnsContentOfFocusedWindow()
    {
        var control = new BufferControl();
        var w = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(w));

        Assert.Same(control, layout.CurrentControl);
    }

    [Fact]
    public void CurrentBuffer_ReturnsBufferWhenBufferControlFocused()
    {
        var buffer = new Buffer(name: "test");
        var control = new BufferControl(buffer: buffer);
        var w = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(w));

        Assert.Same(buffer, layout.CurrentBuffer);
        Assert.True(layout.BufferHasFocus);
    }

    [Fact]
    public void CurrentBuffer_ReturnsNullWhenNotBufferControl()
    {
        var w = CreateFocusableWindow();
        var layout = new StrokeLayout(new AnyContainer(w));

        Assert.Null(layout.CurrentBuffer);
        Assert.False(layout.BufferHasFocus);
    }

    [Fact]
    public void CurrentBufferControl_ReturnsBufferControlOrNull()
    {
        var control = new BufferControl();
        var w1 = new Window(content: control);
        var w2 = CreateFocusableWindow();
        var hsplit = new HSplit(children: [w1, w2]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));

        Assert.Same(control, layout.CurrentBufferControl);

        layout.Focus(new FocusableElement(w2));
        Assert.Null(layout.CurrentBufferControl);
    }

    // --- PreviousControl ---

    [Fact]
    public void PreviousControl_ReturnsPreviouslyFocusedControl()
    {
        var control1 = new BufferControl();
        var control2 = new BufferControl();
        var w1 = new Window(content: control1);
        var w2 = new Window(content: control2);
        var hsplit = new HSplit(children: [w1, w2]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));

        // Initially only w1 is in the stack
        Assert.Same(control1, layout.PreviousControl);

        layout.Focus(new FocusableElement(w2));
        Assert.Same(control1, layout.PreviousControl);
    }

    // --- FindAllWindows ---

    [Fact]
    public void FindAllWindows_WalksTree()
    {
        var w1 = CreateFocusableWindow();
        var w2 = CreateFocusableWindow();
        var w3 = CreateFocusableWindow();
        var inner = new HSplit(children: [w2, w3]);
        var outer = new HSplit(children: [w1, inner]);
        var layout = new StrokeLayout(new AnyContainer(outer));

        var windows = layout.FindAllWindows().ToList();
        Assert.Equal(3, windows.Count);
        Assert.Contains(w1, windows);
        Assert.Contains(w2, windows);
        Assert.Contains(w3, windows);
    }

    [Fact]
    public void FindAllWindows_SingleWindow()
    {
        var w = CreateFocusableWindow();
        var layout = new StrokeLayout(new AnyContainer(w));

        var windows = layout.FindAllWindows().ToList();
        Assert.Single(windows);
        Assert.Same(w, windows[0]);
    }

    // --- FindAllControls ---

    [Fact]
    public void FindAllControls_ReturnsAllUIControls()
    {
        var control1 = new BufferControl();
        var control2 = new BufferControl();
        var w1 = new Window(content: control1);
        var w2 = new Window(content: control2);
        var hsplit = new HSplit(children: [w1, w2]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));

        var controls = layout.FindAllControls().ToList();
        Assert.Equal(2, controls.Count);
        Assert.Contains(control1, controls);
        Assert.Contains(control2, controls);
    }

    // --- Walk ---

    [Fact]
    public void Walk_DepthFirstOrder()
    {
        var w1 = CreateFocusableWindow();
        var w2 = CreateFocusableWindow();
        var w3 = CreateFocusableWindow();
        var inner = new HSplit(children: [w2, w3]);
        var outer = new HSplit(children: [w1, inner]);
        var layout = new StrokeLayout(new AnyContainer(outer));

        var walked = layout.Walk().ToList();

        // Depth-first: outer -> w1 -> inner -> w2 -> w3
        Assert.Equal(5, walked.Count);
        Assert.Same(outer, walked[0]);
        Assert.Same(w1, walked[1]);
        Assert.Same(inner, walked[2]);
        Assert.Same(w2, walked[3]);
        Assert.Same(w3, walked[4]);
    }

    // --- GetParent / UpdateParentsRelations ---

    [Fact]
    public void GetParent_BeforeUpdate_ReturnsNull()
    {
        var w1 = CreateFocusableWindow();
        var w2 = CreateFocusableWindow();
        var hsplit = new HSplit(children: [w1, w2]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));

        // Before UpdateParentsRelations, parent map is empty
        Assert.Null(layout.GetParent(w1));
    }

    [Fact]
    public void GetParent_AfterUpdate_ReturnsCorrectParent()
    {
        var w1 = CreateFocusableWindow();
        var w2 = CreateFocusableWindow();
        var hsplit = new HSplit(children: [w1, w2]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));

        layout.UpdateParentsRelations();

        Assert.Same(hsplit, layout.GetParent(w1));
        Assert.Same(hsplit, layout.GetParent(w2));
    }

    [Fact]
    public void GetParent_NestedContainers()
    {
        var w1 = CreateFocusableWindow();
        var w2 = CreateFocusableWindow();
        var inner = new HSplit(children: [w2]);
        var outer = new HSplit(children: [w1, inner]);
        var layout = new StrokeLayout(new AnyContainer(outer));

        layout.UpdateParentsRelations();

        Assert.Same(outer, layout.GetParent(w1));
        Assert.Same(outer, layout.GetParent(inner));
        Assert.Same(inner, layout.GetParent(w2));
        Assert.Null(layout.GetParent(outer)); // Root has no parent
    }

    [Fact]
    public void UpdateParentsRelations_RebuildsMap()
    {
        var w1 = CreateFocusableWindow();
        var layout = new StrokeLayout(new AnyContainer(w1));

        layout.UpdateParentsRelations();

        // After update, w1 has no parent (it's the root container)
        Assert.Null(layout.GetParent(w1));
    }

    // --- SearchLinks ---

    [Fact]
    public void SearchLinks_InitiallyEmpty()
    {
        var w = CreateFocusableWindow();
        var layout = new StrokeLayout(new AnyContainer(w));

        Assert.Empty(layout.SearchLinks);
    }

    [Fact]
    public void IsSearching_FalseWhenNoSearchLink()
    {
        var w = CreateFocusableWindow();
        var layout = new StrokeLayout(new AnyContainer(w));

        Assert.False(layout.IsSearching);
    }

    [Fact]
    public void CurrentSearchBufferControl_NullWhenNoSearchLink()
    {
        var w = CreateFocusableWindow();
        var layout = new StrokeLayout(new AnyContainer(w));

        Assert.Null(layout.CurrentSearchBufferControl);
    }

    // --- VisibleWindows ---

    [Fact]
    public void VisibleWindows_EmptyBeforeRender()
    {
        var w = CreateFocusableWindow();
        var layout = new StrokeLayout(new AnyContainer(w));

        Assert.Empty(layout.VisibleWindows);
    }

    // --- GetBufferByName ---

    [Fact]
    public void GetBufferByName_FindsBuffer()
    {
        var buffer = new Buffer(name: "search-target");
        var control = new BufferControl(buffer: buffer);
        var w1 = new Window(content: control);
        var w2 = CreateFocusableWindow();
        var hsplit = new HSplit(children: [w1, w2]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));

        var found = layout.GetBufferByName("search-target");
        Assert.Same(buffer, found);
    }

    [Fact]
    public void GetBufferByName_ReturnsNullWhenNotFound()
    {
        var w = CreateFocusableWindow();
        var layout = new StrokeLayout(new AnyContainer(w));

        Assert.Null(layout.GetBufferByName("nonexistent"));
    }

    // --- Reset ---

    [Fact]
    public void Reset_ClearsSearchLinks()
    {
        var w = CreateFocusableWindow();
        var layout = new StrokeLayout(new AnyContainer(w));

        // SearchLinks should be empty, Reset should not throw
        layout.Reset();
        Assert.Empty(layout.SearchLinks);
    }

    // --- GetFocusableWindows ---

    [Fact]
    public void GetFocusableWindows_ReturnsFocusableOnly()
    {
        var w1 = CreateFocusableWindow();
        var w2 = CreateNonFocusableWindow();
        var w3 = CreateFocusableWindow();
        var hsplit = new HSplit(children: [w1, w2, w3]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));
        layout.UpdateParentsRelations();

        var focusable = layout.GetFocusableWindows().ToList();
        Assert.Equal(2, focusable.Count);
        Assert.Contains(w1, focusable);
        Assert.Contains(w3, focusable);
        Assert.DoesNotContain(w2, focusable);
    }

    // --- ToString ---

    [Fact]
    public void ToString_ContainsClassName()
    {
        var w = CreateFocusableWindow();
        var layout = new StrokeLayout(new AnyContainer(w));

        var str = layout.ToString();
        Assert.StartsWith("Layout(", str);
    }

    // --- Thread Safety ---

    [Fact]
    public async Task ConcurrentFocus_DoesNotThrow()
    {
        var ct = TestContext.Current.CancellationToken;
        var w1 = CreateFocusableWindow();
        var w2 = CreateFocusableWindow();
        var w3 = CreateFocusableWindow();
        var hsplit = new HSplit(children: [w1, w2, w3]);
        var layout = new StrokeLayout(new AnyContainer(hsplit));
        var windows = new[] { w1, w2, w3 };

        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            var window = windows[i % 3];
            tasks.Add(Task.Run(() => layout.Focus(new FocusableElement(window)), ct));
        }

        await Task.WhenAll(tasks);

        // After concurrent focus, CurrentWindow should be one of the valid windows
        var current = layout.CurrentWindow;
        Assert.True(current == w1 || current == w2 || current == w3);
    }

    [Fact]
    public async Task ConcurrentReadProperties_DoesNotThrow()
    {
        var ct = TestContext.Current.CancellationToken;
        var buffer = new Buffer(name: "concurrent");
        var control = new BufferControl(buffer: buffer);
        var w = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(w));

        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                _ = layout.CurrentWindow;
                _ = layout.CurrentControl;
                _ = layout.CurrentBuffer;
                _ = layout.BufferHasFocus;
                _ = layout.IsSearching;
                _ = layout.VisibleWindows;
                _ = layout.SearchLinks;
                _ = layout.HasFocus(w);
                _ = layout.ToString();
            }, ct));
        }

        await Task.WhenAll(tasks);
    }
}
