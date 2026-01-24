# Quickstart: Buffer (Mutable Text Container)

**Date**: 2026-01-24
**Feature**: 007-mutable-buffer

## Overview

The Buffer class is the mutable text container that wraps an immutable Document, providing all the editing operations needed for interactive text input. It manages:

- **Text editing**: Insert, delete, transform text
- **Cursor navigation**: Move through single and multiline text
- **Undo/redo**: Track and restore previous states
- **History**: Navigate through command history
- **Selection**: Select, copy, cut, paste text
- **Completion**: Manage autocompletion state
- **Validation**: Validate input content
- **Auto-suggest**: Show suggestions as user types

## Quick Example

```csharp
using Stroke.Core;

// Create a buffer
var buffer = new Buffer();

// Insert text
buffer.InsertText("Hello ");
buffer.InsertText("World");
Console.WriteLine(buffer.Text);  // "Hello World"

// Access the immutable document
Document doc = buffer.Document;
Console.WriteLine(doc.TextBeforeCursor);  // "Hello World"

// Undo
buffer.SaveToUndoStack();
buffer.InsertText("!");
Console.WriteLine(buffer.Text);  // "Hello World!"

buffer.Undo();
Console.WriteLine(buffer.Text);  // "Hello World"
```

## Core Concepts

### 1. Document Property

The `Document` property provides access to the immutable document that the Buffer wraps. Each modification creates a new Document instance (cached for performance).

```csharp
var buffer = new Buffer(document: new Document("Initial text", cursorPosition: 7));

// Access document properties
Document doc = buffer.Document;
Console.WriteLine(doc.Text);              // "Initial text"
Console.WriteLine(doc.CursorPosition);    // 7
Console.WriteLine(doc.TextBeforeCursor);  // "Initial"
Console.WriteLine(doc.TextAfterCursor);   // " text"
```

### 2. Text Editing

```csharp
var buffer = new Buffer();

// Insert at cursor
buffer.InsertText("Hello");

// Insert with overwrite mode
buffer.InsertText("X", overwrite: true);  // Replaces character at cursor

// Delete forward
buffer.Delete(count: 3);

// Delete backward (like backspace)
buffer.DeleteBeforeCursor(count: 1);

// Transform lines
buffer.TransformLines(
    Enumerable.Range(0, 5),
    line => line.ToUpperInvariant()
);

// Transform region
buffer.TransformRegion(from: 0, to: 5, text => text.ToLower());
```

### 3. Cursor Navigation

```csharp
var buffer = new Buffer(document: new Document("Line 1\nLine 2\nLine 3", 0));

// Basic movement
buffer.CursorRight(count: 3);
buffer.CursorLeft(count: 1);

// Multiline movement
buffer.CursorDown(count: 1);  // Move to Line 2
buffer.CursorUp(count: 1);    // Move back to Line 1

// Smart navigation (handles completion, cursor, and history)
buffer.AutoUp();   // Goes to previous completion, line, or history entry
buffer.AutoDown(); // Goes to next completion, line, or history entry
```

### 4. Undo/Redo

```csharp
var buffer = new Buffer();

buffer.InsertText("First");
buffer.SaveToUndoStack();  // Save state

buffer.InsertText(" Second");
buffer.SaveToUndoStack();

buffer.InsertText(" Third");

buffer.Undo();  // Text: "First Second"
buffer.Undo();  // Text: "First"
buffer.Redo();  // Text: "First Second"
```

### 5. History Navigation

```csharp
// Create buffer with history
var history = new InMemoryHistory();
await history.AppendAsync("ls -la");
await history.AppendAsync("cd /home");
await history.AppendAsync("pwd");

var buffer = new Buffer(history: history);
buffer.LoadHistoryIfNotYetLoaded();

// Navigate history
buffer.HistoryBackward();  // Shows "pwd"
buffer.HistoryBackward();  // Shows "cd /home"
buffer.HistoryBackward();  // Shows "ls -la"
buffer.HistoryForward();   // Shows "cd /home"

// With prefix search (enable_history_search)
var searchBuffer = new Buffer(
    history: history,
    enableHistorySearch: () => true
);
searchBuffer.InsertText("cd");
searchBuffer.HistoryBackward();  // Shows "cd /home" (matches prefix)
```

### 6. Selection and Clipboard

```csharp
var buffer = new Buffer(document: new Document("Hello World", 0));

// Start selection
buffer.StartSelection(SelectionType.Characters);
buffer.CursorRight(5);

// Copy selection
ClipboardData copied = buffer.CopySelection();
Console.WriteLine(copied.Text);  // "Hello"

// Or cut selection
ClipboardData cut = buffer.CutSelection();

// Paste
buffer.CursorPosition = buffer.Text.Length;
buffer.PasteClipboardData(copied);

// Paste modes
buffer.PasteClipboardData(data, PasteMode.Emacs);     // Insert at cursor
buffer.PasteClipboardData(data, PasteMode.ViBefore);  // Insert before line
buffer.PasteClipboardData(data, PasteMode.ViAfter);   // Insert after line

// Paste multiple times
buffer.PasteClipboardData(data, count: 3);  // Insert 3 times
```

### 7. Completion

```csharp
var completer = new WordCompleter(new[] { "hello", "help", "helmet" });
var buffer = new Buffer(completer: completer);

buffer.InsertText("hel");

// Start completion
buffer.StartCompletion();

// Navigate completions
buffer.CompleteNext();
buffer.CompletePrevious();
buffer.GoToCompletion(2);

// Apply selected completion
if (buffer.CompleteState?.CurrentCompletion is { } completion)
{
    buffer.ApplyCompletion(completion);
}

// Or cancel
buffer.CancelCompletion();
```

### 8. Validation

```csharp
var validator = new NonEmptyValidator();
var buffer = new Buffer(
    validator: validator,
    validateWhileTyping: () => true
);

// Synchronous validation
bool isValid = buffer.Validate();
if (!isValid && buffer.ValidationError is { } error)
{
    Console.WriteLine($"Error at {error.CursorPosition}: {error.Message}");
}

// Validation state
Console.WriteLine(buffer.ValidationState);  // Valid, Invalid, or Unknown
```

### 9. Read-Only Mode

```csharp
// Static read-only
var readOnlyBuffer = new Buffer(readOnly: () => true);
try
{
    readOnlyBuffer.InsertText("test");
}
catch (EditReadOnlyBufferException)
{
    Console.WriteLine("Cannot edit read-only buffer");
}

// Bypass for programmatic updates
readOnlyBuffer.SetDocument(new Document("new content"), bypassReadonly: true);
```

### 10. Events

```csharp
var buffer = new Buffer(
    onTextChanged: b => Console.WriteLine($"Text changed: {b.Text}"),
    onTextInsert: b => Console.WriteLine($"Text inserted"),
    onCursorPositionChanged: b => Console.WriteLine($"Cursor: {b.CursorPosition}"),
    onCompletionsChanged: b => Console.WriteLine($"Completions updated"),
    onSuggestionSet: b => Console.WriteLine($"Suggestion: {b.Suggestion?.Text}")
);

// Or subscribe after creation
buffer.OnTextChanged += b => { /* handle */ };
```

## Thread Safety

Buffer is thread-safe. All mutable state is protected by synchronization:

```csharp
var buffer = new Buffer();

// Safe to call from multiple threads
Parallel.For(0, 100, i =>
{
    buffer.InsertText($"Item {i} ");
});
```

## Common Patterns

### REPL-Style Input

```csharp
var history = new InMemoryHistory();
var buffer = new Buffer(
    history: history,
    enableHistorySearch: () => true,
    multiline: () => false
);

// User types command
buffer.InsertText("git status");

// On Enter: validate and accept
if (buffer.Validate())
{
    buffer.AppendToHistory();
    string command = buffer.Text;
    buffer.Reset();  // Clear for next input

    // Execute command...
}
```

### Multiline Editor

```csharp
var buffer = new Buffer(
    multiline: () => true,
    completeWhileTyping: () => true,
    validateWhileTyping: () => true
);

// Insert newlines
buffer.InsertText("def hello():");
buffer.Newline(copyMargin: true);  // Copies indentation
buffer.InsertText("    pass");

// Vi-style line insertion
buffer.InsertLineAbove(copyMargin: true);
buffer.InsertLineBelow(copyMargin: true);
```

### External Editor

```csharp
var buffer = new Buffer(
    tempfileSuffix: ".py",  // For syntax highlighting
    readOnly: () => false
);

buffer.InsertText("# Edit in your favorite editor\nprint('hello')");

// Open in $EDITOR or $VISUAL
await buffer.OpenInEditorAsync(validateAndHandle: true);

// Buffer now contains edited content
```

## BufferOperations

Static helper methods for common operations:

```csharp
// Indent lines (4 spaces per count)
BufferOperations.Indent(buffer, fromRow: 0, toRow: 5, count: 1);

// Unindent lines
BufferOperations.Unindent(buffer, fromRow: 0, toRow: 5, count: 1);

// Reshape text (Vi 'gq' operator)
// Wraps text at buffer.TextWidth (default: 80)
BufferOperations.ReshapeText(buffer, fromRow: 0, toRow: 5);
```
