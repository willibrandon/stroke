# Quickstart: Layout Containers, UI Controls, and Window Container

**Feature Branch**: `029-layout-containers-controls-window`
**Prerequisites**: Feature 028 (Screen/Char), Feature 016 (Dimension), Feature 007 (Buffer)

---

## Quick Overview

This feature provides the core layout system for Stroke terminal UI:

- **Containers**: HSplit, VSplit, FloatContainer for arranging UI elements
- **Controls**: BufferControl (editable text), FormattedTextControl (static text)
- **Window**: Wraps controls with scrolling, margins, and cursor display

---

## Getting Started

### 1. Simple Vertical Layout (HSplit)

```csharp
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Windows;

// Create three windows stacked vertically
var header = new Window(
    content: new FormattedTextControl("Header"),
    height: Dimension.Exact(1));

var mainContent = new Window(
    content: new BufferControl(buffer));

var footer = new Window(
    content: new FormattedTextControl("Footer"),
    height: Dimension.Exact(1));

// Stack them vertically
var layout = new HSplit([header, mainContent, footer]);
```

### 2. Horizontal Split (VSplit)

```csharp
// Create a sidebar and main content side by side
var sidebar = new Window(
    content: new FormattedTextControl("Sidebar"),
    width: Dimension.Exact(20));

var editor = new Window(
    content: new BufferControl(buffer));

var layout = new VSplit([sidebar, editor]);
```

### 3. Editable Buffer with Line Numbers

```csharp
var buffer = new Buffer("Hello, World!");
var window = new Window(
    content: new BufferControl(buffer),
    leftMargins: [new NumberedMargin()],
    scrollOffsets: new ScrollOffsets(top: 3, bottom: 3),
    wrapLines: true,
    cursorline: true);
```

### 4. Floating Completion Menu

```csharp
var mainContent = new Window(
    content: new BufferControl(buffer));

var completionMenu = new Window(
    content: new FormattedTextControl("Completions..."),
    dontExtendWidth: true,
    dontExtendHeight: true);

var layout = new FloatContainer(
    new AnyContainer(mainContent),
    floats: [
        new Float(
            new AnyContainer(completionMenu),
            xcursor: true,  // Position near cursor X
            ycursor: true)  // Position near cursor Y
    ]);
```

### 5. Conditional Visibility

```csharp
var isSearchMode = new Condition(() => app.InSearchMode);

var searchBar = new Window(
    content: new BufferControl(searchBuffer),
    height: Dimension.Exact(1));

var conditionalSearch = new ConditionalContainer(
    new AnyContainer(searchBar),
    filter: isSearchMode);

var layout = new HSplit([mainContent, conditionalSearch]);
```

---

## Key Patterns

### Container Hierarchy

```
IContainer
├── HSplit       - Vertical stacking (children top to bottom)
├── VSplit       - Horizontal arrangement (children left to right)
├── FloatContainer - Background with floating overlays
├── ConditionalContainer - Filter-based show/hide
├── DynamicContainer - Runtime content switching
└── Window       - UIControl wrapper with scrolling
```

### Control Hierarchy

```
IUIControl
├── DummyControl          - Empty placeholder
├── FormattedTextControl  - Static styled text
├── BufferControl         - Editable buffer
└── SearchBufferControl   - Search input specialized
```

### Dimension System

```csharp
// Fixed size
Dimension.Exact(10)

// Minimum only
new Dimension(min: 5)

// Range with weight
new Dimension(min: 10, max: 50, weight: 2)

// Preferred with bounds
new Dimension(min: 5, max: 100, preferred: 30)
```

---

## Rendering Flow

```
1. Layout.WriteToScreen() called
2. Container calculates child dimensions
   - PreferredWidth/Height called on children
   - Space divided using weighted allocation
3. Container calls WriteToScreen on children
4. Window:
   - Gets UIContent from UIControl
   - Calculates scroll position
   - Renders margins
   - Copies content with alignment
   - Applies cursorline/cursorcolumn
5. Screen updated with deferred floats
```

---

## Common Configurations

### Text Editor Layout

```csharp
var layout = new HSplit([
    // Toolbar
    new Window(
        content: new FormattedTextControl("File | Edit | View"),
        height: Dimension.Exact(1)),

    // Main editor with line numbers and scrollbar
    new Window(
        content: new BufferControl(buffer),
        leftMargins: [new NumberedMargin()],
        rightMargins: [new ScrollbarMargin()],
        wrapLines: true,
        cursorline: true),

    // Status bar
    new Window(
        content: new FormattedTextControl("Ln 1, Col 1 | UTF-8"),
        height: Dimension.Exact(1))
]);
```

### Split Pane Editor

```csharp
var layout = new VSplit([
    new Window(content: new BufferControl(leftBuffer)),
    new Window(content: new BufferControl(rightBuffer))
], padding: 1, paddingChar: '│');
```

### Dialog Box

```csharp
var dialog = new FloatContainer(
    new AnyContainer(mainLayout),
    floats: [
        new Float(
            new AnyContainer(new HSplit([
                new Window(content: new FormattedTextControl("Dialog Title")),
                new Window(content: dialogContent),
                new Window(content: new FormattedTextControl("[OK] [Cancel]"))
            ])),
            left: 10, top: 5,
            width: 50, height: 10)
    ]);
```

---

## Testing Tips

### Unit Testing Containers

```csharp
[Fact]
public void HSplit_DividesHeightProportionally()
{
    var child1 = new Window(content: new DummyControl(), height: new Dimension(weight: 1));
    var child2 = new Window(content: new DummyControl(), height: new Dimension(weight: 2));
    var hsplit = new HSplit([child1, child2]);

    var screen = new Screen(initialWidth: 80, initialHeight: 30);
    hsplit.WriteToScreen(screen, new MouseHandlers(),
        new WritePosition(0, 0, 80, 30), "", true, null);

    // child1 gets 10 lines (1/3), child2 gets 20 lines (2/3)
}
```

### Testing Window Scrolling

```csharp
[Fact]
public void Window_ScrollsToKeepCursorVisible()
{
    var buffer = new Buffer(string.Join("\n", Enumerable.Range(1, 100).Select(i => $"Line {i}")));
    buffer.CursorPosition = buffer.Document.TranslateRowColToIndex(50, 0);

    var window = new Window(
        content: new BufferControl(buffer),
        scrollOffsets: new ScrollOffsets(top: 3, bottom: 3));

    var screen = new Screen(initialWidth: 80, initialHeight: 20);
    window.WriteToScreen(screen, new MouseHandlers(),
        new WritePosition(0, 0, 80, 20), "", true, null);

    // Verify line 50 is visible within scroll offsets
    Assert.NotNull(window.RenderInfo);
    Assert.True(window.RenderInfo.DisplayedLines.Contains(50));
}
```

---

## Troubleshooting

### Container shows "window too small"

HSplit/VSplit display this when total minimum sizes exceed available space.

**Solution**: Reduce minimum sizes or use flexible dimensions:
```csharp
new Dimension(min: 0, preferred: 10)  // Can shrink to 0 if needed
```

### Floats not appearing

Floats with `xcursor` or `ycursor` require the attached window to have a cursor position.

**Solution**: Ensure the attached window's control reports a cursor position in UIContent.

### Scroll not working

Window only scrolls when content exceeds visible area.

**Solution**: Check UIContent.LineCount and ensure WrapLines is set appropriately.

---

## Next Steps

After implementing this feature:

1. **Feature 030**: Layout manager (orchestrates full layout lifecycle)
2. **Feature 031**: Completion menus (MultiColumnCompletionsMenu)
3. **Feature 032**: Input processors (HighlightSearchProcessor, etc.)
