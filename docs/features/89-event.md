# Feature 89: Event System

## Overview

Implement a simple event system for subscribing to and firing events within the library.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/utils.py`

## Public API

### Event Class

```csharp
namespace Stroke.Utils;

/// <summary>
/// Simple event to which handlers can be attached.
/// Supports += and -= operators for subscribing/unsubscribing.
/// </summary>
/// <typeparam name="TSender">Type of the event sender.</typeparam>
public sealed class Event<TSender>
{
    /// <summary>
    /// The sender object passed to handlers.
    /// </summary>
    public TSender Sender { get; }

    /// <summary>
    /// Create an event.
    /// </summary>
    /// <param name="sender">The sender to pass to handlers.</param>
    /// <param name="handler">Optional initial handler.</param>
    public Event(TSender sender, Action<TSender>? handler = null);

    /// <summary>
    /// Fire the event, calling all handlers.
    /// </summary>
    public void Fire();

    /// <summary>
    /// Add a handler to this event.
    /// </summary>
    /// <param name="handler">Handler that receives the sender.</param>
    public void AddHandler(Action<TSender> handler);

    /// <summary>
    /// Remove a handler from this event.
    /// </summary>
    /// <param name="handler">Handler to remove.</param>
    public void RemoveHandler(Action<TSender> handler);

    /// <summary>
    /// Add handler using += operator.
    /// </summary>
    public static Event<TSender> operator +(Event<TSender> e, Action<TSender> handler);

    /// <summary>
    /// Remove handler using -= operator.
    /// </summary>
    public static Event<TSender> operator -(Event<TSender> e, Action<TSender> handler);
}
```

### DummyContext

```csharp
namespace Stroke.Utils;

/// <summary>
/// A no-op disposable for use as a dummy context.
/// </summary>
public sealed class DummyContext : IDisposable
{
    /// <summary>
    /// Singleton instance.
    /// </summary>
    public static DummyContext Instance { get; } = new();

    private DummyContext() { }

    public void Dispose() { }
}
```

## Project Structure

```
src/Stroke/
└── Utils/
    ├── Event.cs
    └── DummyContext.cs
tests/Stroke.Tests/
└── Utils/
    └── EventTests.cs
```

## Implementation Notes

### Event Implementation

```csharp
public sealed class Event<TSender>
{
    private readonly List<Action<TSender>> _handlers = new();

    public TSender Sender { get; }

    public Event(TSender sender, Action<TSender>? handler = null)
    {
        Sender = sender;
        if (handler != null)
            AddHandler(handler);
    }

    public void Fire()
    {
        foreach (var handler in _handlers.ToList())
            handler(Sender);
    }

    public void AddHandler(Action<TSender> handler)
    {
        _handlers.Add(handler);
    }

    public void RemoveHandler(Action<TSender> handler)
    {
        _handlers.Remove(handler);
    }

    public static Event<TSender> operator +(Event<TSender> e, Action<TSender> handler)
    {
        e.AddHandler(handler);
        return e;
    }

    public static Event<TSender> operator -(Event<TSender> e, Action<TSender> handler)
    {
        e.RemoveHandler(handler);
        return e;
    }
}
```

### Usage Examples

```csharp
// Define an event in a class
public class Buffer
{
    public Event<Buffer> OnTextChanged { get; }
    public Event<Buffer> OnCursorPositionChanged { get; }

    public Buffer()
    {
        OnTextChanged = new Event<Buffer>(this);
        OnCursorPositionChanged = new Event<Buffer>(this);
    }

    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                OnTextChanged.Fire();
            }
        }
    }
}

// Subscribe to event
var buffer = new Buffer();
buffer.OnTextChanged += (sender) =>
{
    Console.WriteLine($"Text changed: {sender.Text}");
};

// Fire event
buffer.Text = "Hello";  // Triggers handler
```

### Application Events

```csharp
public class Application
{
    public Event<Application> OnInitialized { get; }
    public Event<Application> OnRendered { get; }
    public Event<Application> OnInvalidated { get; }
    public Event<Application> OnExit { get; }

    public Application()
    {
        OnInitialized = new Event<Application>(this);
        OnRendered = new Event<Application>(this);
        OnInvalidated = new Event<Application>(this);
        OnExit = new Event<Application>(this);
    }
}
```

### DummyContext Usage

```csharp
// Use DummyContext when a context is optional
public IDisposable GetOptionalContext()
{
    if (ShouldCreateContext)
        return new SomeContext();
    return DummyContext.Instance;
}

// Caller doesn't need to check for null
using (GetOptionalContext())
{
    // Do work
}
```

## Dependencies

None (utility module).

## Implementation Tasks

1. Implement `Event<TSender>` class
2. Implement `+=` and `-=` operators
3. Implement `Fire()` method
4. Implement `DummyContext` singleton
5. Add events to Buffer
6. Add events to Application
7. Write unit tests

## Acceptance Criteria

- [ ] Event stores sender reference
- [ ] AddHandler adds handlers to list
- [ ] RemoveHandler removes handlers
- [ ] Fire() calls all handlers with sender
- [ ] += operator adds handler
- [ ] -= operator removes handler
- [ ] DummyContext is a no-op disposable
- [ ] Multiple handlers can be attached
- [ ] Unit tests achieve 80% coverage
