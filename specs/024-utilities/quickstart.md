# Quickstart: Utilities

**Feature**: 024-utilities
**Date**: 2026-01-28

## Overview

The Utilities feature provides foundational helper classes for the Stroke library:

- **Event<TSender>**: Pub/sub event system with += and -= operators
- **UnicodeWidth**: Terminal display width calculation for strings and characters
- **PlatformUtils**: Cross-platform OS and environment detection
- **ConversionUtils**: Lazy value (callable) to concrete value conversion
- **CollectionUtils**: Weight-based item distribution
- **DummyContext**: No-op disposable for optional context scenarios

## Quick Examples

### Event-Based Communication

```csharp
using Stroke.Core;

// Define a class with an event
public class Counter
{
    public Event<Counter> Changed { get; }
    private int _value;

    public Counter()
    {
        Changed = new Event<Counter>(this);
    }

    public int Value
    {
        get => _value;
        set
        {
            _value = value;
            Changed.Fire();
        }
    }
}

// Usage
var counter = new Counter();

// Subscribe with +=
counter.Changed += sender => Console.WriteLine($"Counter changed to {sender.Value}");

// Fire event by changing value
counter.Value = 42;  // Prints: "Counter changed to 42"

// Unsubscribe with -=
Action<Counter> handler = sender => Console.WriteLine("Handler 2");
counter.Changed += handler;
counter.Changed -= handler;
```

### Unicode Width Calculation

```csharp
using Stroke.Core;

// ASCII characters: width 1
int width1 = UnicodeWidth.GetWidth('A');  // 1

// CJK characters: width 2
int width2 = UnicodeWidth.GetWidth('中');  // 2

// String width
int width3 = UnicodeWidth.GetWidth("Hello世界");  // 5 + 2*2 = 9

// Control characters: width 0
int width4 = UnicodeWidth.GetWidth('\x1b');  // 0

// Empty string
int width5 = UnicodeWidth.GetWidth("");  // 0
```

### Platform Detection

```csharp
using Stroke.Core;

// OS detection
if (PlatformUtils.IsWindows)
{
    // Windows-specific logic
}
else if (PlatformUtils.IsMacOS)
{
    // macOS-specific logic
}
else if (PlatformUtils.IsLinux)
{
    // Linux-specific logic
}

// Feature detection
if (PlatformUtils.SuspendToBackgroundSupported)
{
    // Unix: can handle Ctrl+Z
}

// Terminal detection
string term = PlatformUtils.GetTermEnvironmentVariable();
if (PlatformUtils.IsDumbTerminal())
{
    // Fall back to simple line editing
}

// ConEmu on Windows
if (PlatformUtils.IsConEmuAnsi)
{
    // ANSI sequences supported in ConEmu
}
```

### Lazy Value Conversion

```csharp
using Stroke.Core;

// Direct values
string s1 = ConversionUtils.ToStr("hello");  // "hello"
int i1 = ConversionUtils.ToInt(42);          // 42
double d1 = ConversionUtils.ToFloat(3.14);   // 3.14

// Callable values (lazy evaluation)
string s2 = ConversionUtils.ToStr(() => "world");  // "world"
int i2 = ConversionUtils.ToInt(() => GetDynamicValue());

// Nested callables (recursive unwrapping)
string s3 = ConversionUtils.ToStr(() => () => "nested");  // "nested"

// Null handling
string s4 = ConversionUtils.ToStr(null);  // ""
int i3 = ConversionUtils.ToInt(null);     // 0

// AnyFloat for flexible APIs
AnyFloat f1 = 3.14;                         // From double
Func<double> getter = () => 2.71;
AnyFloat f2 = getter;                       // From callable
double result = f1.Value + f2.Value;
```

### Weight-Based Distribution

```csharp
using Stroke.Core;

// Items with weights
var items = new[] { "A", "B", "C" };
var weights = new[] { 1, 2, 4 };  // A=14%, B=29%, C=57%

// Take proportionally
var results = CollectionUtils.TakeUsingWeights(items, weights)
    .Take(70)
    .ToList();

// Results: approximately 10 A's, 20 B's, 40 C's

// Zero weights are filtered
var weights2 = new[] { 0, 1, 2 };
// Only B and C will be yielded
```

### DummyContext

```csharp
using Stroke.Core;

// Use when IDisposable is optional
IDisposable context = needsCleanup
    ? GetRealContext()
    : DummyContext.Instance;

using (context)
{
    // Do work
    // If using DummyContext, Dispose() is a no-op
}
```

## Common Patterns

### Event Handler Cleanup

```csharp
class MyComponent : IDisposable
{
    private readonly Buffer _buffer;
    private readonly Action<Buffer> _handler;

    public MyComponent(Buffer buffer)
    {
        _buffer = buffer;
        _handler = OnBufferChanged;
        _buffer.TextChanged += _handler;
    }

    private void OnBufferChanged(Buffer sender) { /* ... */ }

    public void Dispose()
    {
        _buffer.TextChanged -= _handler;
    }
}
```

### Cross-Platform Terminal Setup

```csharp
IOutput CreateOutput()
{
    if (PlatformUtils.IsWindows)
    {
        if (PlatformUtils.IsConEmuAnsi || /* Windows 10 VT100 check */)
            return new Vt100Output(Console.Out);
        return new WindowsConsoleOutput();
    }
    return new Vt100Output(Console.Out);
}
```

### Width-Aware Text Formatting

```csharp
void PrintAligned(string label, string value, int totalWidth)
{
    int labelWidth = UnicodeWidth.GetWidth(label);
    int valueWidth = UnicodeWidth.GetWidth(value);
    int padding = totalWidth - labelWidth - valueWidth;

    Console.Write(label);
    Console.Write(new string(' ', Math.Max(1, padding)));
    Console.WriteLine(value);
}

// Works correctly with CJK:
PrintAligned("Name:", "田中太郎", 20);  // Aligns properly
```

## Error Handling

```csharp
// TakeUsingWeights validation
try
{
    // Mismatched lengths
    CollectionUtils.TakeUsingWeights(new[] { 1, 2 }, new[] { 1 });
}
catch (ArgumentException) { /* "Items and weights must have the same length" */ }

try
{
    // No positive weights
    CollectionUtils.TakeUsingWeights(new[] { 1, 2 }, new[] { 0, 0 });
}
catch (ArgumentException) { /* "At least one item must have a positive weight" */ }

// Event handler exceptions propagate
var evt = new Event<string>("sender");
evt += _ => throw new InvalidOperationException("Handler error");
try
{
    evt.Fire();
}
catch (InvalidOperationException) { /* Exception propagates */ }
```

## Testing Tips

1. **Event tests**: Verify handler order, multiple handlers, removal behavior
2. **UnicodeWidth tests**: Cover ASCII, CJK, control chars, combining marks
3. **Platform tests**: Use runtime checks (can't mock RuntimeInformation)
4. **TakeUsingWeights tests**: Verify distribution within 5% tolerance for 100+ items
5. **ConversionUtils tests**: Test null handling, nested callables

