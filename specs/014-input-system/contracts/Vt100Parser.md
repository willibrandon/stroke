# Vt100Parser Class Contract

**Namespace**: `Stroke.Input.Vt100`
**Type**: Sealed class
**Python Equivalent**: `prompt_toolkit.input.vt100_parser.Vt100Parser`

## Summary

Parser for VT100/ANSI escape sequences. This parser converts raw terminal input into `KeyPress` events.

## Capabilities

- Regular character input
- Control characters (Ctrl+A through Ctrl+Z)
- Escape sequences (arrow keys, function keys, etc.)
- Mouse event sequences (X10, SGR, urxvt protocols)
- Cursor position report (CPR) responses
- Bracketed paste mode

## Thread Safety

This class is not thread-safe. Single-threaded access is assumed.

---

## Constructor

```csharp
public Vt100Parser(Action<KeyPress> feedKeyCallback);
```

Initializes a new instance of the Vt100Parser class.

**Parameters**:
- `feedKeyCallback`: A callback invoked for each parsed `KeyPress`.

**Exceptions**: `ArgumentNullException` thrown if `feedKeyCallback` is null.

---

## Methods

### Feed

```csharp
public void Feed(string data);
```

Feeds input data into the parser.

**Parameters**:
- `data`: The input characters to parse.

**Remarks**:
- Characters are processed sequentially
- Complete escape sequences result in callback invocations
- Incomplete sequences are buffered until more data arrives or `Flush` is called
- Bracketed paste mode markers (`\x1b[200~` to `\x1b[201~`) accumulate content and emit as single `Keys.BracketedPaste` key press

---

### Flush

```csharp
public void Flush();
```

Flushes any pending partial escape sequences.

**Remarks**:
- Critical for detecting standalone Escape key presses
- When ESC is received, the parser waits for additional characters that might form an escape sequence
- If a timeout occurs and no additional characters arrive, call this method to emit the buffered content as individual key presses
- Recommended timeout: 50-100ms after last input before calling flush

---

### FeedAndFlush

```csharp
public void FeedAndFlush(string data);
```

Feeds input data and immediately flushes.

**Parameters**:
- `data`: The input characters to parse.

**Remarks**: Convenience method equivalent to calling `Feed` followed by `Flush`. Useful when all input is available at once (e.g., in tests).

---

### Reset

```csharp
public void Reset(bool request = false);
```

Resets the parser to its initial state.

**Parameters**:
- `request`: If true, also clears any pending request state.

**Remarks**: Discards any buffered partial sequences without emitting them. Use this when recovering from an error or starting fresh.

---

## Parser State Machine

```
Ground ──(ESC)──► Escape ──([)──► CsiEntry ──(params)──► CsiParam ──(final)──► Emit
   │                │                                                            │
   │                └──(timeout/flush)──► Emit ESC ─────────────────────────────┘
   │                                                                             │
   └──(printable)──► Emit character ◄────────────────────────────────────────────┘
```

### States

| State | Description |
|-------|-------------|
| Ground | Normal state, processing characters |
| Escape | After ESC, waiting for sequence type |
| CsiEntry | After CSI (ESC[), waiting for parameters |
| CsiParam | Processing CSI parameters |
| CsiIntermediate | Processing CSI intermediates |
| OscString | Processing OSC string |
| SosPmApcString | Processing SOS/PM/APC string |

### Buffer Limits

- **Maximum buffer size**: 256 bytes for incomplete escape sequences
- **Overflow behavior**: If buffer exceeds limit, emit buffered content as `Keys.Escape` followed by literal characters, then reset to Ground state
- **Bracketed paste**: No buffer limit; content accumulated until end sequence (bounded only by available memory)

### Unrecognized Sequences

| Sequence Type | Behavior |
|--------------|----------|
| Unknown CSI sequence | Emit `Keys.Escape`, then literal characters including final byte |
| Unknown SS2/SS3 sequence | Emit `Keys.Escape`, then literal character |
| OSC string | Consume and discard (no key event emitted) |
| DCS string | Consume and discard (no key event emitted) |
| APC/PM/SOS string | Consume and discard (no key event emitted) |

### Malformed Bracketed Paste

- **Nested start sequences**: Inner `\x1b[200~` treated as literal paste content
- **Missing end sequence**: Content accumulates until end sequence or Close()
- **Interleaved escape sequences**: All content between start/end is literal (no parsing)

---

## Escape Sequence Examples

### Basic Navigation

| Input | Parsed Key |
|-------|------------|
| `\x1b[A` | `Keys.Up` |
| `\x1b[B` | `Keys.Down` |
| `\x1b[C` | `Keys.Right` |
| `\x1b[D` | `Keys.Left` |
| `\x1b[H` | `Keys.Home` |
| `\x1b[F` | `Keys.End` |
| `\x1b[5~` | `Keys.PageUp` |
| `\x1b[6~` | `Keys.PageDown` |
| `\x1b[2~` | `Keys.Insert` |
| `\x1b[3~` | `Keys.Delete` |

### Function Keys

| Input | Parsed Key |
|-------|------------|
| `\x1bOP` | `Keys.F1` |
| `\x1bOQ` | `Keys.F2` |
| `\x1bOR` | `Keys.F3` |
| `\x1bOS` | `Keys.F4` |
| `\x1b[15~` | `Keys.F5` |
| `\x1b[17~` | `Keys.F6` |
| `\x1b[18~` | `Keys.F7` |
| `\x1b[19~` | `Keys.F8` |
| `\x1b[20~` | `Keys.F9` |
| `\x1b[21~` | `Keys.F10` |
| `\x1b[23~` | `Keys.F11` |
| `\x1b[24~` | `Keys.F12` |

### Modifier Combinations

Modifiers are encoded in CSI sequences using the pattern `\x1b[1;{modifier}{final}` where modifier values are:
- 2 = Shift
- 3 = Alt
- 4 = Shift+Alt
- 5 = Ctrl
- 6 = Shift+Ctrl
- 7 = Alt+Ctrl
- 8 = Shift+Alt+Ctrl

| Input | Parsed Key |
|-------|------------|
| `\x1b[1;5A` | `Keys.ControlUp` |
| `\x1b[1;5B` | `Keys.ControlDown` |
| `\x1b[1;5C` | `Keys.ControlRight` |
| `\x1b[1;5D` | `Keys.ControlLeft` |
| `\x1b[1;2A` | `Keys.ShiftUp` |
| `\x1b[1;2B` | `Keys.ShiftDown` |
| `\x1b[1;3A` | `Keys.Escape` + `Keys.Up` (Alt+Up as escape prefix) |
| `\x1b[1;2P` | `Keys.ShiftF1` |
| `\x1b[1;5P` | `Keys.ControlF1` |

### Bracketed Paste and Mouse

| Input | Parsed Key |
|-------|------------|
| `\x1b[200~hello\x1b[201~` | `Keys.BracketedPaste` (data: "hello") |
| `\x1b[<0;10;20M` | `Keys.Vt100MouseEvent` (SGR mouse click) |
| `\x1b[<0;10;20m` | `Keys.Vt100MouseEvent` (SGR mouse release) |
| `\x1b[M !!` | `Keys.Vt100MouseEvent` (X10 mouse, button 0 at 1,1) |

---

## Usage Examples

### Basic Parsing

```csharp
var keys = new List<KeyPress>();
var parser = new Vt100Parser(keys.Add);

parser.FeedAndFlush("hello\x1b[A");

// keys contains: 'h', 'e', 'l', 'l', 'o', Keys.Up
```

### Standalone Escape Detection

```csharp
var keys = new List<KeyPress>();
var parser = new Vt100Parser(keys.Add);

parser.Feed("\x1b");  // ESC received
// Wait 100ms for potential sequence...
Thread.Sleep(100);
parser.Flush();  // No more data, emit ESC

// keys contains: Keys.Escape
```

### Bracketed Paste

```csharp
var keys = new List<KeyPress>();
var parser = new Vt100Parser(keys.Add);

parser.FeedAndFlush("\x1b[200~pasted text\x1b[201~");

// keys contains single KeyPress:
//   Key = Keys.BracketedPaste
//   Data = "pasted text"
```

### Incremental Input

```csharp
var keys = new List<KeyPress>();
var parser = new Vt100Parser(keys.Add);

// Bytes arrive in chunks
parser.Feed("\x1b");   // Nothing emitted yet
parser.Feed("[");      // Nothing emitted yet
parser.Feed("A");      // Emits Keys.Up

// keys contains: Keys.Up
```
