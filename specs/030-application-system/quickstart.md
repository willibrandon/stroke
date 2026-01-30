# Quickstart: Application System

**Feature**: 030-application-system

## Getting Started

### Minimal Application

```csharp
using Stroke.Application;
using Stroke.Layout;

// Create and run a minimal application (uses dummy layout with ENTER to quit)
var app = new Application<object?>();
app.Run();
```

### Application with Custom Layout

```csharp
using Stroke.Application;
using Stroke.Layout;
using Stroke.Layout.Controls;
using Stroke.Layout.Containers;

// Create a control and layout
var control = new FormattedTextControl("Hello, Stroke!");
var window = new Window(content: control);
var layout = new Layout(container: window);

// Create and run the application
var app = new Application<string>(layout: layout);
string result = await app.RunAsync();
```

### Exiting with a Result

```csharp
using Stroke.Application;
using Stroke.KeyBinding;
using Stroke.Input;

var kb = new KeyBindings();
kb.Add(Keys.Enter, (KeyPressEvent e) =>
{
    e.App.Exit(result: "done");
});

var app = new Application<string>(keyBindings: kb);
string result = app.Run(); // Returns "done" when ENTER is pressed
```

### Application Context

```csharp
using Stroke.Application;

// Access the current application from anywhere
var app = AppContext.GetApp();        // Returns DummyApplication if none running
var appOrNull = AppContext.GetAppOrNull(); // Returns null if none running
var session = AppContext.GetAppSession();  // Always returns a session

// Create isolated sessions (e.g., Telnet server)
using var session = AppContext.CreateAppSession(input: myInput, output: myOutput);
```

### Background Tasks

```csharp
// Inside a key binding handler or other application code:
app.CreateBackgroundTask(async (ct) =>
{
    while (!ct.IsCancellationRequested)
    {
        await Task.Delay(1000, ct);
        app.Invalidate(); // Thread-safe UI refresh
    }
});
// Background tasks are automatically cancelled when the application exits.
```

### Run In Terminal

```csharp
// Temporarily suspend the UI to run terminal commands
await RunInTerminal.RunAsync(() =>
{
    Console.WriteLine("This prints above the application UI");
});

// Or use the async context:
await using (RunInTerminal.InTerminal())
{
    Console.WriteLine("Direct terminal access");
    await SomeAsyncOperation();
}
```

## Key Concepts

1. **Application\<TResult\>** — The orchestrator. Generic over the result type returned by `Exit()`.
2. **Layout** — Manages the container hierarchy and focus. Required for rendering.
3. **Renderer** — Converts the layout tree to screen output using differential updates.
4. **KeyProcessor** — Dispatches key presses to matching handlers from the CombinedRegistry.
5. **AppContext** — Static access to the current application/session via `AsyncLocal<T>`.
6. **AppSession** — Groups an input/output pair for one terminal connection.

## File Organization

```
src/Stroke/Application/
├── Application.cs              # Application<TResult> class (constructor, properties)
├── Application.RunAsync.cs     # RunAsync/Run methods (partial class)
├── Application.Lifecycle.cs    # Reset, Exit, Invalidate, Redraw (partial class)
├── Application.BackgroundTasks.cs # CreateBackgroundTask, Cancel (partial class)
├── Application.SystemCommands.cs  # RunSystemCommand, SuspendToBackground, PrintText (partial class)
├── AppSession.cs               # AppSession class
├── AppContext.cs                # Static context utilities
├── DummyApplication.cs         # No-op fallback
├── RunInTerminal.cs            # RunInTerminal static class
├── CombinedRegistry.cs         # Internal key bindings aggregator
├── ColorDepthOption.cs         # ColorDepth option union type
├── InputHook.cs                # InputHook delegate and context
├── DefaultKeyBindings.cs       # LoadKeyBindings, LoadPageNavigation stubs
├── AppFilters.cs               # Application-aware filter functions
└── DummyLayout.cs              # CreateDummyLayout utility

src/Stroke/Rendering/
├── Renderer.cs                 # Renderer class
├── Renderer.Diff.cs            # Screen diff algorithm (partial class)
├── RendererUtils.cs            # PrintFormattedText utility

src/Stroke/KeyBinding/
├── KeyProcessor.cs             # KeyProcessor state machine

src/Stroke/Layout/
├── Layout.cs                   # Layout focus/parent management class
├── LayoutUtils.cs              # Walk utility function
├── FocusableElement.cs         # FocusableElement union type
├── InvalidLayoutException.cs   # Layout validation exception
```
