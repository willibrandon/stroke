# API Contract: Screen

**Namespace**: `Stroke.Layout`
**Type**: `sealed class`
**Thread Safety**: Thread-safe (Lock synchronization)

## Class Definition

```csharp
namespace Stroke.Layout;

/// <summary>
/// Two-dimensional buffer of <see cref="Char"/> instances.
/// </summary>
/// <remarks>
/// <para>
/// Provides sparse storage for terminal screen content with defaultdict-like access patterns.
/// Accessing unset positions returns a configurable default character.
/// </para>
/// <para>
/// Supports cursor and menu position tracking per window, zero-width escape sequence
/// storage, and deferred float drawing with z-index ordering.
/// </para>
/// <para>
/// This type is thread-safe. All mutable operations are synchronized using
/// <see cref="System.Threading.Lock"/>.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>Screen</c> class from <c>layout/screen.py</c>.
/// </para>
/// </remarks>
public sealed class Screen
```

## Constructors

```csharp
/// <summary>
/// Initializes a new instance of the <see cref="Screen"/> class.
/// </summary>
/// <param name="defaultChar">
/// The default character returned for unset positions.
/// If null, uses a space with <see cref="Char.Transparent"/> style.
/// </param>
/// <param name="initialWidth">Initial screen width. Default is 0.</param>
/// <param name="initialHeight">Initial screen height. Default is 0.</param>
public Screen(Char? defaultChar = null, int initialWidth = 0, int initialHeight = 0)
```

## Properties

```csharp
/// <summary>
/// Gets the default character used for empty cells.
/// </summary>
public Char DefaultChar { get; }

/// <summary>
/// Gets or sets the current screen width.
/// </summary>
/// <remarks>
/// This value may increase when writing to positions beyond the current width.
/// </remarks>
public int Width { get; set; }

/// <summary>
/// Gets or sets the current screen height.
/// </summary>
/// <remarks>
/// This value may increase when writing to positions beyond the current height.
/// </remarks>
public int Height { get; set; }

/// <summary>
/// Gets or sets whether the cursor should be visible.
/// </summary>
public bool ShowCursor { get; set; }

/// <summary>
/// Gets the list of visible windows.
/// </summary>
/// <remarks>
/// Returns a snapshot of window references from <see cref="VisibleWindowsToWritePositions"/>.
/// </remarks>
public IReadOnlyList<IWindow> VisibleWindows { get; }

/// <summary>
/// Gets the dictionary mapping windows to their write positions.
/// </summary>
/// <remarks>
/// Windows add themselves to this dictionary when drawn.
/// </remarks>
public IDictionary<IWindow, WritePosition> VisibleWindowsToWritePositions { get; }
```

## Indexers

```csharp
/// <summary>
/// Gets or sets the character at the specified position.
/// </summary>
/// <param name="row">The row (y coordinate).</param>
/// <param name="col">The column (x coordinate).</param>
/// <returns>
/// The character at the position, or <see cref="DefaultChar"/> if not set.
/// </returns>
/// <remarks>
/// Setting a value creates the row dictionary if needed. Getting a value
/// for an unset position does not create entries (sparse storage).
/// </remarks>
public Char this[int row, int col] { get; set; }
```

## Methods - Cursor and Menu Position

```csharp
/// <summary>
/// Sets the cursor position for a given window.
/// </summary>
/// <param name="window">The window to set the cursor for.</param>
/// <param name="position">The cursor position.</param>
/// <exception cref="ArgumentNullException"><paramref name="window"/> is <c>null</c>.</exception>
public void SetCursorPosition(IWindow window, Point position)

/// <summary>
/// Gets the cursor position for a given window.
/// </summary>
/// <param name="window">The window to get the cursor for.</param>
/// <returns>
/// The cursor position, or <see cref="Point.Zero"/> if not set.
/// </returns>
/// <exception cref="ArgumentNullException"><paramref name="window"/> is <c>null</c>.</exception>
public Point GetCursorPosition(IWindow window)

/// <summary>
/// Sets the menu position for a given window.
/// </summary>
/// <param name="window">The window to set the menu position for.</param>
/// <param name="position">The menu anchor position.</param>
/// <exception cref="ArgumentNullException"><paramref name="window"/> is <c>null</c>.</exception>
public void SetMenuPosition(IWindow window, Point position)

/// <summary>
/// Gets the menu position for a given window.
/// </summary>
/// <param name="window">The window to get the menu position for.</param>
/// <returns>
/// The menu position if set; otherwise the cursor position if set;
/// otherwise <see cref="Point.Zero"/>.
/// </returns>
/// <exception cref="ArgumentNullException"><paramref name="window"/> is <c>null</c>.</exception>
public Point GetMenuPosition(IWindow window)
```

## Methods - Zero-Width Escapes

```csharp
/// <summary>
/// Gets the zero-width escape sequences at the specified position.
/// </summary>
/// <param name="row">The row (y coordinate).</param>
/// <param name="col">The column (x coordinate).</param>
/// <returns>
/// The concatenated escape sequences, or empty string if none.
/// </returns>
public string GetZeroWidthEscapes(int row, int col)

/// <summary>
/// Adds a zero-width escape sequence at the specified position.
/// </summary>
/// <param name="row">The row (y coordinate).</param>
/// <param name="col">The column (x coordinate).</param>
/// <param name="escape">The escape sequence to add.</param>
/// <exception cref="ArgumentNullException"><paramref name="escape"/> is <c>null</c>.</exception>
/// <remarks>
/// Multiple escapes at the same position are concatenated.
/// Empty strings are ignored (no-op).
/// </remarks>
public void AddZeroWidthEscape(int row, int col, string escape)
```

## Methods - Float Drawing

```csharp
/// <summary>
/// Queues a draw function to be executed during <see cref="DrawAllFloats"/>.
/// </summary>
/// <param name="zIndex">The z-index determining draw order (lower = first).</param>
/// <param name="drawFunc">The draw function to execute.</param>
/// <remarks>
/// Draw functions are executed in ascending z-index order.
/// </remarks>
public void DrawWithZIndex(int zIndex, Action drawFunc)

/// <summary>
/// Executes all queued float draw functions in z-index order.
/// </summary>
/// <remarks>
/// <para>
/// Functions are sorted by z-index and executed one at a time.
/// For equal z-index values, functions execute in FIFO order (the order queued).
/// If a draw function queues additional functions, they are processed
/// in the same loop (iterative, not recursive).
/// </para>
/// <para>
/// After completion, the draw function queue is empty.
/// </para>
/// <para>
/// If a draw function throws an exception, the queue is cleared and
/// the exception is re-thrown. Remaining functions are not executed.
/// </para>
/// </remarks>
public void DrawAllFloats()
```

## Methods - Style Operations

```csharp
/// <summary>
/// Appends a style string to all characters currently in the screen.
/// </summary>
/// <param name="styleStr">The style string to append.</param>
/// <remarks>
/// Iterates all stored characters and creates new Char instances with
/// the appended style. Uses character cache for efficiency.
/// </remarks>
public void AppendStyleToContent(string styleStr)

/// <summary>
/// Fills a rectangular region with a style.
/// </summary>
/// <param name="writePosition">The region to fill.</param>
/// <param name="style">The style to apply.</param>
/// <param name="after">
/// If <c>true</c>, appends style after existing style.
/// If <c>false</c> (default), prepends style before existing style.
/// </param>
/// <remarks>
/// <para>
/// If <paramref name="style"/> is empty or whitespace-only, no changes are made.
/// </para>
/// <para>
/// For each cell in the region, a new Char is created with the combined style.
/// This may cause cells that didn't exist to be created with the default character
/// plus the fill style.
/// </para>
/// </remarks>
public void FillArea(WritePosition writePosition, string style = "", bool after = false)
```

## Methods - Reset

```csharp
/// <summary>
/// Clears all screen content and resets to initial state.
/// </summary>
/// <remarks>
/// <para>
/// Clears: data buffer, zero-width escapes, cursor positions, menu positions,
/// draw queue, and visible windows.
/// </para>
/// <para>
/// Resets Width and Height to the constructor's initialWidth and initialHeight values.
/// </para>
/// <para>
/// Preserves: DefaultChar and ShowCursor.
/// </para>
/// </remarks>
public void Clear()
```

## Usage Examples

```csharp
// Create screen with default settings
var screen = new Screen();

// Create screen with custom default char
var customDefault = Char.Create(".", "class:background");
var screen2 = new Screen(customDefault, initialWidth: 80, initialHeight: 24);

// Write characters
screen[0, 0] = Char.Create("H", "class:keyword");
screen[0, 1] = Char.Create("i", "");

// Read characters (unset returns default)
var ch = screen[5, 5]; // DefaultChar (space with transparent style)

// Cursor tracking with mock window
IWindow window = new MockWindow();
screen.SetCursorPosition(window, new Point(10, 5));
var pos = screen.GetCursorPosition(window); // Point(10, 5)

// Menu position (falls back to cursor)
var menuPos = screen.GetMenuPosition(window); // Point(10, 5) (fallback)
screen.SetMenuPosition(window, new Point(10, 7));
menuPos = screen.GetMenuPosition(window); // Point(10, 7)

// Zero-width escapes
screen.AddZeroWidthEscape(0, 0, "\x1b]8;;http://example.com\x1b\\");
screen.AddZeroWidthEscape(0, 0, "more"); // Concatenated
var escapes = screen.GetZeroWidthEscapes(0, 0);
// "\x1b]8;;http://example.com\x1b\\more"

// Z-index drawing
int order = 0;
screen.DrawWithZIndex(10, () => Console.WriteLine($"{++order}: z=10"));
screen.DrawWithZIndex(5, () => Console.WriteLine($"{++order}: z=5"));
screen.DrawWithZIndex(8, () => Console.WriteLine($"{++order}: z=8"));
screen.DrawAllFloats();
// Output: "1: z=5", "2: z=8", "3: z=10"

// Fill area with style
var region = new WritePosition(0, 0, 10, 5);
screen.FillArea(region, "class:highlight");

// Append style to all content
screen.AppendStyleToContent("class:dim");

// Clear and reuse screen
screen.Clear();
// screen is now empty, Width/Height reset to initial values
// DefaultChar and ShowCursor are preserved
```

## Thread Safety Notes

All operations are protected by a single `Lock`:

```csharp
private readonly Lock _lock = new();

public Char this[int row, int col]
{
    get
    {
        using (_lock.EnterScope())
        {
            // ... read operation
        }
    }
    set
    {
        using (_lock.EnterScope())
        {
            // ... write operation
        }
    }
}
```

Compound operations (read-modify-write across multiple cells) require external synchronization if atomicity is needed.
