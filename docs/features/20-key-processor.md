# Feature 20: Key Processor

## Overview

Implement the key processor that receives key presses and dispatches them to the appropriate key binding handlers based on the current state.

## Python Prompt Toolkit Reference

**Source:** `/Users/brandon/src/python-prompt-toolkit/src/prompt_toolkit/key_binding/key_processor.py`

## Public API

### KeyProcessor Class

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// State machine that receives KeyPress instances and according to the
/// key bindings, calls the matching handlers.
/// </summary>
public sealed class KeyProcessor
{
    /// <summary>
    /// Creates a key processor.
    /// </summary>
    /// <param name="keyBindings">The key bindings to use.</param>
    public KeyProcessor(IKeyBindingsBase keyBindings);

    /// <summary>
    /// Event fired before a key press is processed.
    /// </summary>
    public event EventHandler? BeforeKeyPress;

    /// <summary>
    /// Event fired after a key press is processed.
    /// </summary>
    public event EventHandler? AfterKeyPress;

    /// <summary>
    /// The queue of keys not yet sent to the process generator.
    /// </summary>
    public Queue<KeyPress> InputQueue { get; }

    /// <summary>
    /// The key buffer that is matched in the generator state machine.
    /// </summary>
    public IReadOnlyList<KeyPress> KeyBuffer { get; }

    /// <summary>
    /// Readline argument (for repetition of commands).
    /// </summary>
    public string? Arg { get; set; }

    /// <summary>
    /// Reset the processor state.
    /// </summary>
    public void Reset();

    /// <summary>
    /// Add a new KeyPress to the input queue.
    /// </summary>
    /// <param name="keyPress">The key press to add.</param>
    /// <param name="first">If true, insert before everything else.</param>
    public void Feed(KeyPress keyPress, bool first = false);

    /// <summary>
    /// Add multiple KeyPress objects to the input queue.
    /// </summary>
    /// <param name="keyPresses">The key presses to add.</param>
    /// <param name="first">If true, insert before everything else.</param>
    public void FeedMultiple(IEnumerable<KeyPress> keyPresses, bool first = false);

    /// <summary>
    /// Process all the keys in the input queue.
    /// </summary>
    public void ProcessKeys();

    /// <summary>
    /// Empty the input queue and return the unprocessed input.
    /// </summary>
    public IReadOnlyList<KeyPress> EmptyQueue();

    /// <summary>
    /// Send SIGINT. Immediately call the SIGINT key handler.
    /// </summary>
    public void SendSigint();
}
```

### KeyPressEvent Class

```csharp
namespace Stroke.KeyBinding;

/// <summary>
/// Key press event, delivered to key bindings.
/// </summary>
public sealed class KeyPressEvent
{
    /// <summary>
    /// Creates a key press event.
    /// </summary>
    internal KeyPressEvent(
        KeyProcessor keyProcessor,
        string? arg,
        IReadOnlyList<KeyPress> keySequence,
        IReadOnlyList<KeyPress> previousKeySequence,
        bool isRepeat);

    /// <summary>
    /// The key sequence that triggered this event.
    /// </summary>
    public IReadOnlyList<KeyPress> KeySequence { get; }

    /// <summary>
    /// The previous key sequence.
    /// </summary>
    public IReadOnlyList<KeyPress> PreviousKeySequence { get; }

    /// <summary>
    /// True when the previous key sequence was handled by the same handler.
    /// </summary>
    public bool IsRepeat { get; }

    /// <summary>
    /// The data from the last key in the sequence.
    /// </summary>
    public string Data { get; }

    /// <summary>
    /// The key processor.
    /// </summary>
    public KeyProcessor KeyProcessor { get; }

    /// <summary>
    /// The current Application object.
    /// </summary>
    public Application<object> App { get; }

    /// <summary>
    /// The current buffer.
    /// </summary>
    public IBuffer CurrentBuffer { get; }

    /// <summary>
    /// Repetition argument.
    /// </summary>
    public int Arg { get; }

    /// <summary>
    /// True if repetition argument was explicitly provided.
    /// </summary>
    public bool ArgPresent { get; }

    /// <summary>
    /// Add digit to the input argument.
    /// </summary>
    /// <param name="data">The typed digit as string ("-" or "0"-"9").</param>
    public void AppendToArgCount(string data);
}
```

## Project Structure

```
src/Stroke/
└── KeyBinding/
    ├── KeyProcessor.cs
    └── KeyPressEvent.cs
tests/Stroke.Tests/
└── KeyBinding/
    ├── KeyProcessorTests.cs
    └── KeyPressEventTests.cs
```

## Implementation Notes

### Processing State Machine

The key processor uses a coroutine-like state machine:
1. Receive key presses via `Feed()`
2. Buffer keys until a match is found
3. Check for exact matches and prefix matches
4. If eager match found, call handler immediately
5. If exact match and no longer prefix matches, call handler
6. If no match, shift buffer and retry

### Flush Mechanism

A special `_Flush` key press is used internally to force processing of buffered keys after a timeout (like Vim's `timeoutlen`).

### Vi Cursor Fix

After every command in Vi navigation mode, the cursor is prevented from going past the end of the line.

### Temporary Navigation Mode

In Vi mode, pressing Ctrl+O temporarily switches to navigation mode for one command, then returns to insert/replace mode.

### Macro Recording

Key sequences are recorded in macros when:
- `RecordInMacro` filter returns true
- Emacs or Vi macro recording is active

### CPR Response Handling

Cursor Position Report (CPR) responses are processed even when the application is done, to prevent them from appearing as typeahead.

### Timeout Handling

After processing keys, a timeout is started. If no more keys arrive within the timeout and there are buffered keys, a flush is triggered.

## Dependencies

- `Stroke.KeyBinding.Binding` (Feature 19) - Key binding class
- `Stroke.Input.KeyPress` (Feature 16) - Key press class
- `Stroke.Input.Keys` (Feature 10) - Keys enum
- `Stroke.Application.Application<TResult>` (Feature 31) - Application class

## Implementation Tasks

1. Implement `KeyProcessor` class with state machine
2. Implement `KeyPressEvent` class
3. Implement flush mechanism
4. Implement Vi cursor fix
5. Implement temporary navigation mode handling
6. Implement macro recording
7. Implement CPR response handling
8. Write comprehensive unit tests

## Acceptance Criteria

- [ ] Key processor matches Python Prompt Toolkit semantics
- [ ] State machine handles all key sequences correctly
- [ ] Flush mechanism works correctly
- [ ] Vi cursor fix works correctly
- [ ] Macro recording works correctly
- [ ] Unit tests achieve 80% coverage
