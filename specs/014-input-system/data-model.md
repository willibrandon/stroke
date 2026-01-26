# Data Model: Input System

**Feature**: 014-input-system
**Date**: 2026-01-25

## Overview

This document defines the data structures and entities for the Input System. The model follows Python Prompt Toolkit's design with C#/.NET adaptations.

---

## Core Entities

### KeyPress

Represents a single key press event with both the logical key identity and raw input data.

| Field | Type | Description |
|-------|------|-------------|
| Key | `Keys` | The logical key identity (enum value or character) |
| Data | `string?` | The raw input data (escape sequences, characters) |

**Implementation**: `readonly record struct` (immutable, value semantics)

**Relationships**:
- Referenced by `IInput.ReadKeys()` return value
- Referenced by `Vt100Parser` callback parameter
- Referenced by `TypeaheadBuffer` storage

**Validation Rules**:
- `Key` must be a valid `Keys` enum value
- `Data` defaults to key's string representation if null

**Python Equivalent**: `prompt_toolkit.key_binding.KeyPress`

```csharp
public readonly record struct KeyPress(Keys Key, string? Data = null)
{
    public string Data { get; } = Data ?? KeyToString(Key);
}
```

---

### IInput

Abstract interface representing any input source.

| Member | Type | Description |
|--------|------|-------------|
| Closed | `bool` (property) | Whether the input stream is closed |
| ReadKeys() | `IReadOnlyList<KeyPress>` | Read and parse available key presses |
| FlushKeys() | `IReadOnlyList<KeyPress>` | Flush pending partial sequences |
| RawMode() | `IDisposable` | Enter raw terminal mode |
| CookedMode() | `IDisposable` | Enter cooked terminal mode |
| Attach(callback) | `IDisposable` | Attach to event loop |
| Detach() | `IDisposable` | Detach from event loop |
| FileNo() | `nint` | Native file descriptor/handle |
| TypeaheadHash() | `string` | Unique identifier for typeahead storage |
| Close() | `void` | Close the input |

**Implementation**: Interface with abstract members

**Relationships**:
- Implemented by `DummyInput`, `Vt100Input`, `Win32Input`
- Extended by `IPipeInput`
- Created by `InputFactory`
- Attached to event loops

**Python Equivalent**: `prompt_toolkit.input.Input`

---

### IPipeInput

Extended interface for pipe-based input (testing).

| Member | Type | Description |
|--------|------|-------------|
| SendBytes(data) | `void` | Feed raw bytes into the pipe |
| SendText(data) | `void` | Feed text (UTF-8) into the pipe |

**Implementation**: Interface extending `IInput`

**Relationships**:
- Extends `IInput`
- Implemented by `PosixPipeInput`, `Win32PipeInput`

**Python Equivalent**: `prompt_toolkit.input.PipeInput`

---

### DummyInput

No-op input that immediately signals EOF.

| Field | Type | Description |
|-------|------|-------------|
| _id | `int` | Unique instance identifier |

**State**:
- `Closed` always returns `true`
- `ReadKeys()` returns empty list
- Mode methods return no-op disposables

**Implementation**: Sealed class implementing `IInput`

**Use Cases**:
- Non-terminal scenarios (redirected stdin)
- Unit tests that don't need input
- Placeholder when no real terminal available

**Python Equivalent**: `prompt_toolkit.input.DummyInput`

---

### Vt100Parser

State machine for parsing VT100/ANSI escape sequences.

| Field | Type | Description |
|-------|------|-------------|
| _feedKeyCallback | `Action<KeyPress>` | Callback for parsed keys |
| _state | `ParserState` | Current parser state |
| _buffer | `StringBuilder` | Accumulated partial sequence |
| _inBracketedPaste | `bool` | Whether in bracketed paste mode |
| _pasteBuffer | `StringBuilder` | Accumulated paste content |

**State Transitions**:
```
Ground → (ESC) → Escape → (CSI indicator) → CsiEntry → ... → Ground
Ground → (printable) → Ground (emit character)
Escape → (timeout/flush) → Ground (emit ESC)
```

**Methods**:
| Method | Description |
|--------|-------------|
| Feed(data) | Feed input characters |
| Flush() | Flush pending sequences |
| FeedAndFlush(data) | Combined feed and flush |
| Reset() | Reset parser state |

**Implementation**: Sealed class with internal state

**Relationships**:
- Used by `Vt100Input` and `Vt100ConsoleInputReader`
- References `AnsiSequences` for lookups
- Produces `KeyPress` via callback

**Python Equivalent**: `prompt_toolkit.input.vt100_parser.Vt100Parser`

---

### AnsiSequences

Static dictionary of ANSI escape sequences to key mappings.

| Member | Type | Description |
|--------|------|-------------|
| Sequences | `FrozenDictionary<string, Keys>` | Sequence → Key mapping |
| ReverseSequences | `FrozenDictionary<Keys, string>` | Key → Sequence mapping |
| ValidPrefixes | `FrozenSet<string>` | Prefixes that could match longer sequences |

**Implementation**: Static class with frozen collections

**Key Sequences** (partial list):
| Sequence | Key |
|----------|-----|
| `\x1b[A` | `Keys.Up` |
| `\x1b[B` | `Keys.Down` |
| `\x1b[C` | `Keys.Right` |
| `\x1b[D` | `Keys.Left` |
| `\x1bOP` | `Keys.F1` |
| `\x1bOQ` | `Keys.F2` |
| `\x1b[200~` | `Keys.BracketedPaste` (start) |
| `\x1b[201~` | `Keys.BracketedPaste` (end) |

**Python Equivalent**: `prompt_toolkit.input.vt100_parser.ANSI_SEQUENCES`

---

### Vt100Input

POSIX input implementation using VT100 parsing.

| Field | Type | Description |
|-------|------|-------------|
| _stdin | `Stream` | Input stream |
| _fileno | `int` | Cached file descriptor |
| _buffer | `List<KeyPress>` | Parsed key buffer |
| _stdinReader | `PosixStdinReader` | Low-level reader |
| _vt100Parser | `Vt100Parser` | Escape sequence parser |

**Implementation**: Sealed class implementing `IInput`

**Relationships**:
- Uses `PosixStdinReader` for I/O
- Uses `Vt100Parser` for parsing
- Creates `RawModeContext` / `CookedModeContext`

**Python Equivalent**: `prompt_toolkit.input.vt100.Vt100Input`

---

### Win32Input

Windows console input implementation.

| Field | Type | Description |
|-------|------|-------------|
| _useVirtualTerminalInput | `bool` | Whether VT100 mode is enabled |
| _consoleInputReader | `IConsoleInputReader` | Platform-specific reader |

**Implementation**: Sealed class implementing `IInput`

**Relationships**:
- Uses `ConsoleInputReader` (legacy) or `Vt100ConsoleInputReader` (Win10+)
- Creates `Win32RawMode` contexts

**Python Equivalent**: `prompt_toolkit.input.win32.Win32Input`

---

### PosixStdinReader

Non-blocking reader for POSIX stdin.

| Field | Type | Description |
|-------|------|-------------|
| _stdinFd | `int` | File descriptor |
| _encoding | `Encoding` | Character encoding |
| _closed | `bool` | Whether stream is closed |
| _decoder | `Decoder` | Incremental UTF-8 decoder |

**Implementation**: Internal sealed class

**Relationships**:
- Used by `Vt100Input`
- Uses P/Invoke to `libc` for `select()` and `read()`

**Python Equivalent**: `prompt_toolkit.input.posix_utils.PosixStdinReader`

---

### TypeaheadBuffer

Global storage for typeahead key presses.

| Field | Type | Description |
|-------|------|-------------|
| _buffer | `ConcurrentDictionary<string, ConcurrentQueue<KeyPress>>` | Hash → keys mapping |

**Methods**:
| Method | Description |
|--------|-------------|
| Store(input, keys) | Store excess keys for later |
| Get(input) | Retrieve and clear stored keys |
| Clear(input) | Clear stored keys |

**Implementation**: Thread-safe static class

**Relationships**:
- Keyed by `IInput.TypeaheadHash()`
- Stores overflow from one prompt to next

**Python Equivalent**: `prompt_toolkit.input.typeahead` module

---

### RawModeContext / CookedModeContext

Disposable context managers for terminal mode control.

| Field | Type | Description |
|-------|------|-------------|
| _fileno | `int` | File descriptor |
| _attrsBefore | `Termios?` | Saved terminal attributes |

**Implementation**: Internal sealed classes implementing `IDisposable`

**Terminal Flags Modified (POSIX)**:
| Flag | Raw Mode | Cooked Mode |
|------|----------|-------------|
| ECHO | Clear | Set |
| ICANON | Clear | Set |
| ISIG | Clear | Set |
| IEXTEN | Clear | Set |
| IXON | Clear | - |
| IXOFF | Clear | - |
| ICRNL | Clear | Set |

**Python Equivalent**: `prompt_toolkit.input.vt100.raw_mode`, `cooked_mode`

---

## Enums

### ParserState

Internal state for VT100 parser.

| Value | Description |
|-------|-------------|
| Ground | Normal state, processing characters |
| Escape | After ESC, waiting for sequence type |
| CsiEntry | After CSI (ESC[), waiting for parameters |
| CsiParam | Processing CSI parameters |
| CsiIntermediate | Processing CSI intermediates |
| OscString | Processing OSC string |
| SosPmApcString | Processing SOS/PM/APC string |

---

## Relationships Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        InputFactory                              │
│  Create() → IInput (platform-appropriate)                        │
│  CreatePipe() → IPipeInput                                       │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                          IInput                                  │
│  ReadKeys() → IReadOnlyList<KeyPress>                           │
│  FlushKeys() → IReadOnlyList<KeyPress>                          │
│  RawMode() → IDisposable                                         │
│  Attach(callback) → IDisposable                                  │
└─────────────────────────────────────────────────────────────────┘
         │                    │                    │
         ▼                    ▼                    ▼
┌─────────────┐     ┌─────────────────┐    ┌────────────────┐
│ DummyInput  │     │   Vt100Input    │    │   Win32Input   │
│ (no-op)     │     │   (POSIX)       │    │   (Windows)    │
└─────────────┘     └─────────────────┘    └────────────────┘
                           │                       │
                           ▼                       ▼
                    ┌─────────────┐         ┌────────────────┐
                    │ Vt100Parser │◄────────│ Vt100Console   │
                    │             │         │ InputReader    │
                    └─────────────┘         └────────────────┘
                           │
                           ▼
                    ┌─────────────┐
                    │AnsiSequences│
                    │ (lookups)   │
                    └─────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                        IPipeInput                                │
│  extends IInput                                                  │
│  SendBytes(data)                                                 │
│  SendText(data)                                                  │
└─────────────────────────────────────────────────────────────────┘
         │                              │
         ▼                              ▼
┌─────────────────┐            ┌─────────────────┐
│ PosixPipeInput  │            │ Win32PipeInput  │
│ (uses pipe())   │            │ (uses events)   │
└─────────────────┘            └─────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                      TypeaheadBuffer                             │
│  Store(input, keys) / Get(input) / Clear(input)                 │
│  Thread-safe global storage                                      │
└─────────────────────────────────────────────────────────────────┘
```

---

## State Transitions

### VT100 Parser State Machine

```
                    ┌─────────────────┐
                    │     Ground      │◄───────────────────────────┐
                    └────────┬────────┘                            │
                             │ ESC                                 │
                             ▼                                     │
                    ┌─────────────────┐     timeout/flush          │
                    │     Escape      │─────────────────────────────┤
                    └────────┬────────┘                            │
                             │ [                                   │
                             ▼                                     │
                    ┌─────────────────┐                            │
                    │    CsiEntry     │                            │
                    └────────┬────────┘                            │
                             │ 0-9;                                │
                             ▼                                     │
                    ┌─────────────────┐                            │
                    │    CsiParam     │                            │
                    └────────┬────────┘                            │
                             │ final byte (A-Z, a-z, ~)            │
                             ▼                                     │
                    ┌─────────────────┐                            │
                    │  Emit KeyPress  │────────────────────────────┘
                    └─────────────────┘
```

### Bracketed Paste Mode

```
Ground ──(\x1b[200~)──► BracketedPaste ──(chars)──► BracketedPaste
                                          │
                                          │ \x1b[201~
                                          ▼
                                   Emit KeyPress(BracketedPaste, content)
                                          │
                                          ▼
                                       Ground
```
