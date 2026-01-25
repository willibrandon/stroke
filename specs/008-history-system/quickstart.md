# Quickstart: History System

**Feature**: History System
**Date**: 2026-01-24

## Overview

The History System provides command history storage and retrieval for Stroke buffers. It mirrors Python Prompt Toolkit's `prompt_toolkit.history` module exactly.

## Quick Examples

### In-Memory History (Session Only)

```csharp
using Stroke.History;

// Create empty history
var history = new InMemoryHistory();

// Or pre-populate with existing entries
var history = new InMemoryHistory(["command1", "command2", "command3"]);

// Add entries
history.AppendString("echo hello");
history.AppendString("ls -la");

// Get all entries (oldest-first order)
IReadOnlyList<string> entries = history.GetStrings();
// entries = ["command1", "command2", "command3", "echo hello", "ls -la"]

// Load entries asynchronously (newest-first order)
await foreach (var entry in history.LoadAsync())
{
    Console.WriteLine(entry);
    // Prints: "ls -la", "echo hello", "command3", "command2", "command1"
}
```

### Persistent File History

```csharp
using Stroke.History;

// Create file-backed history
var history = new FileHistory("/home/user/.myapp_history");

// Add entries (automatically persisted to file)
history.AppendString("first command");
history.AppendString("second command");

// On next application start, previous entries are available
await foreach (var entry in history.LoadAsync())
{
    Console.WriteLine(entry);
}
```

### Background Loading with ThreadedHistory

```csharp
using Stroke.History;

// Wrap any history for background loading
var fileHistory = new FileHistory("/home/user/.large_history");
var threadedHistory = new ThreadedHistory(fileHistory);

// Start using immediately - loading happens in background
// First items available within 100ms, regardless of file size
await foreach (var entry in threadedHistory.LoadAsync())
{
    // Entries yielded as they load
    Console.WriteLine(entry);
}
```

### No History (Privacy Mode)

```csharp
using Stroke.History;

// Use DummyHistory for privacy-sensitive contexts
var history = new DummyHistory();

// Operations are no-ops
history.AppendString("this will not be stored");

// Returns empty
IReadOnlyList<string> entries = history.GetStrings();
// entries = []
```

## Integration with Buffer

```csharp
using Stroke.Core;
using Stroke.History;

// Create buffer with history
var history = new FileHistory("~/.myapp_history");
var buffer = new Buffer(history: history);

// Navigate history
buffer.HistoryBackward(); // Go to previous entry
buffer.HistoryForward();  // Go to next entry

// Append current text to history
buffer.AppendToHistory();
```

## File Format

FileHistory uses a specific format compatible with Python Prompt Toolkit:

```text
# 2026-01-24 10:30:15.123456
+echo hello

# 2026-01-24 10:31:00.789012
+cat <<EOF
+multi-line
+content
+EOF
```

- Lines starting with `#` are comments (timestamps)
- Lines starting with `+` are entry content
- Multi-line entries have each line prefixed with `+`

## Thread Safety

All history implementations are thread-safe:

```csharp
// Safe to use from multiple threads
var history = new InMemoryHistory();

Parallel.For(0, 1000, i =>
{
    history.AppendString($"command {i}");
});

// All entries will be present (order may vary)
var entries = history.GetStrings();
```

## API Reference

### IHistory Interface

| Method | Description |
|--------|-------------|
| `LoadHistoryStrings()` | Load entries from backend (newest-first) |
| `StoreString(string)` | Persist entry to backend |
| `AppendString(string)` | Add to cache and persist |
| `GetStrings()` | Get cached entries (oldest-first) |
| `LoadAsync()` | Async load (newest-first) |

### Implementations

| Class | Use Case |
|-------|----------|
| `InMemoryHistory` | Session-only storage |
| `FileHistory` | Persistent disk storage |
| `ThreadedHistory` | Background loading wrapper |
| `DummyHistory` | No-op (privacy mode) |
