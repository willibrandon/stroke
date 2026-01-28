# Feature Specification: Editing Modes and State

**Feature Branch**: `023-editing-modes-state`
**Created**: 2026-01-27
**Status**: Draft
**Input**: User description: "Implement the editing mode enums and state classes for Vi and Emacs modes, porting from Python Prompt Toolkit's enums.py, vi_state.py, and emacs_state.py"

## Constitution Compliance

This feature adheres to the following Constitutional principles:

| Principle | Compliance | Notes |
|-----------|------------|-------|
| I. Faithful Port | ✅ | 100% API fidelity with Python Prompt Toolkit; all enum values, classes, properties, and methods ported exactly |
| II. Immutability | ✅ | `CharacterFind` is a `sealed record` (immutable); `ViState`/`EmacsState` are mutable wrappers per Python design |
| III. Layered Architecture | ✅ | All types in `Stroke.KeyBinding` namespace; depends only on `Stroke.Input` and `Stroke.Clipboard` |
| VIII. Real-World Testing | ✅ | No mocks; tests use real implementations with xUnit |
| X. File Size | ✅ | Each type in separate file; all files well under 1,000 LOC |
| XI. Thread Safety | ✅ | `ViState` and `EmacsState` use `System.Threading.Lock` with `EnterScope()` pattern |
| XII. Contracts in Markdown | ✅ | API contracts defined in `contracts/editing-modes-state-api.md` |

## API Fidelity *(mandatory)*

This section specifies the exact 1:1 mapping from Python Prompt Toolkit to C#.

### Naming Convention Transformations

All Python names are transformed to C# naming conventions:
- `snake_case` → `PascalCase` for types, methods, and properties
- `UPPER_SNAKE_CASE` → `PascalCase` for constants (e.g., `SEARCH_BUFFER` → `SearchBuffer`)
- Python `__init__` parameters → C# constructor/record parameters

### EditingMode Enum (Python → C#)

| Python Value | C# Value | Description |
|--------------|----------|-------------|
| `EditingMode.VI` | `EditingMode.Vi` | Vi-style modal editing |
| `EditingMode.EMACS` | `EditingMode.Emacs` | Emacs-style chord-based editing |

**Source**: `prompt_toolkit/enums.py` lines 6-9
**Total Values**: 2 (MUST match Python source exactly)

### InputMode Enum (Python → C#)

| Python Value | C# Value | Description |
|--------------|----------|-------------|
| `InputMode.INSERT` | `InputMode.Insert` | Normal text insertion |
| `InputMode.INSERT_MULTIPLE` | `InputMode.InsertMultiple` | Insert mode for multiple cursors |
| `InputMode.NAVIGATION` | `InputMode.Navigation` | Vi normal mode |
| `InputMode.REPLACE` | `InputMode.Replace` | Overwrite mode (R command) |
| `InputMode.REPLACE_SINGLE` | `InputMode.ReplaceSingle` | Replace single character (r command) |

**Source**: `prompt_toolkit/key_binding/vi_state.py` lines 19-27
**Total Values**: 5 (MUST match Python source exactly)

### BufferNames Constants (Python → C#)

| Python Constant | Python Value | C# Constant | C# Value |
|-----------------|--------------|-------------|----------|
| `SEARCH_BUFFER` | `"SEARCH_BUFFER"` | `BufferNames.SearchBuffer` | `"SEARCH_BUFFER"` |
| `DEFAULT_BUFFER` | `"DEFAULT_BUFFER"` | `BufferNames.DefaultBuffer` | `"DEFAULT_BUFFER"` |
| `SYSTEM_BUFFER` | `"SYSTEM_BUFFER"` | `BufferNames.SystemBuffer` | `"SYSTEM_BUFFER"` |

**Source**: `prompt_toolkit/enums.py` lines 13-19
**Total Constants**: 3 (MUST match Python source exactly; values are identical)

### CharacterFind Class (Python → C#)

**Python signature** (`__init__`):
```python
def __init__(self, char: str, backwards: bool = False) -> None:
    self.char = char
    self.backwards = backwards
```

**C# signature** (sealed record):
```csharp
public sealed record CharacterFind(string Character, bool Backwards = false);
```

| Python Property | C# Property | Type |
|-----------------|-------------|------|
| `char` | `Character` | `string` |
| `backwards` | `Backwards` | `bool` |

**Source**: `prompt_toolkit/key_binding/vi_state.py` lines 29-32

### ViState Class (Python → C#)

| Python Property | C# Property | Type | Default |
|-----------------|-------------|------|---------|
| `input_mode` | `InputMode` | `InputMode` | `InputMode.Insert` |
| `last_character_find` | `LastCharacterFind` | `CharacterFind?` | `null` |
| `operator_func` | `OperatorFunc` | `OperatorFuncDelegate?` | `null` |
| `operator_arg` | `OperatorArg` | `int?` | `null` |
| `named_registers` | (via methods) | `Dictionary<string, ClipboardData>` | empty |
| `waiting_for_digraph` | `WaitingForDigraph` | `bool` | `false` |
| `digraph_symbol1` | `DigraphSymbol1` | `string?` | `null` |
| `tilde_operator` | `TildeOperator` | `bool` | `false` |
| `recording_register` | `RecordingRegister` | `string?` | `null` |
| `current_recording` | `CurrentRecording` | `string` | `""` |
| `temporary_navigation_mode` | `TemporaryNavigationMode` | `bool` | `false` |

**C# Methods** (not in Python but required for proper API):
- `GetNamedRegister(string name)` → `ClipboardData?`
- `SetNamedRegister(string name, ClipboardData data)` → `void`
- `ClearNamedRegister(string name)` → `bool`
- `GetNamedRegisterNames()` → `IReadOnlyCollection<string>`
- `Reset()` → `void`

**Source**: `prompt_toolkit/key_binding/vi_state.py` lines 35-107

### EmacsState Class (Python → C#)

| Python Property | C# Property | Type | Default |
|-----------------|-------------|------|---------|
| `macro` | `Macro` | `IReadOnlyList<KeyPress>` | empty list |
| `current_recording` | `CurrentRecording` | `IReadOnlyList<KeyPress>?` | `null` |

**Computed Properties**:
- `IsRecording` → `bool` (returns `CurrentRecording != null`)

**C# Methods**:
- `StartMacro()` → `void`
- `EndMacro()` → `void`
- `AppendToRecording(KeyPress keyPress)` → `void`
- `Reset()` → `void`

**Source**: `prompt_toolkit/key_binding/emacs_state.py` lines 10-36

### OperatorFuncDelegate Signature

**Python signature**:
```python
Callable[[KeyPressEvent, TextObject], None]
```

**C# signature**:
```csharp
public delegate NotImplementedOrNone OperatorFuncDelegate(KeyPressEvent e, object? textObject);
```

**Note**: `object?` is a placeholder for `ITextObject` until that interface is defined in a future feature. The return type uses `NotImplementedOrNone` to match the key binding system's event handling pattern.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Vi Mode State Management (Priority: P1)

A developer building a terminal application with Vi-style editing needs to track the current Vi input mode (insert, navigation, replace) and associated state such as pending operators, character finds, and macro recordings. The editing mode state allows the key binding system to respond appropriately based on the current modal context.

**Why this priority**: Vi mode is a complex modal editor with multiple states that affect key binding behavior. Without proper state management, Vi-style editing cannot function correctly. This is foundational for the entire Vi editing mode implementation.

**Independent Test**: Can be fully tested by creating a ViState instance, transitioning between input modes, and verifying state properties are correctly managed. Delivers the state management foundation for Vi key bindings.

**Acceptance Scenarios**:

1. **Given** a new ViState instance, **When** I check the initial state, **Then** all properties match initial values: [FR-005, CHK025]
   - `InputMode` = `InputMode.Insert`
   - `LastCharacterFind` = `null`
   - `OperatorFunc` = `null`
   - `OperatorArg` = `null`
   - `WaitingForDigraph` = `false`
   - `DigraphSymbol1` = `null`
   - `TildeOperator` = `false`
   - `RecordingRegister` = `null`
   - `CurrentRecording` = `""`
   - `TemporaryNavigationMode` = `false`
   - Named registers empty

2. **Given** ViState with pending operator (`OperatorFunc` set), **When** I set `InputMode` to `Navigation`, **Then** [FR-006, CHK022, CHK029]:
   - `OperatorFunc` becomes `null`
   - `OperatorArg` becomes `null`
   - `WaitingForDigraph` becomes `false`
   - `DigraphSymbol1` remains unchanged (NOT cleared by Navigation transition)

3. **Given** ViState with various state set (including `RecordingRegister`, `CurrentRecording`, operator state), **When** I call `Reset()`, **Then** [FR-015, CHK023]:
   - `InputMode` = `InputMode.Insert`
   - `OperatorFunc` = `null`
   - `OperatorArg` = `null`
   - `WaitingForDigraph` = `false`
   - `RecordingRegister` = `null`
   - `CurrentRecording` = `""`
   - **NOT cleared**: `LastCharacterFind`, `NamedRegisters`, `TildeOperator`, `TemporaryNavigationMode` [CHK030]

---

### User Story 2 - Emacs Mode State Management (Priority: P1)

A developer building a terminal application with Emacs-style editing needs to record and replay keyboard macros. The Emacs state tracks macro recording status and stores recorded key sequences for later playback.

**Why this priority**: Macro recording is a core Emacs feature. State management is required for the key binding system to know when to capture keys for recording versus normal processing.

**Independent Test**: Can be fully tested by creating an EmacsState instance, starting/ending macro recording, and verifying the recorded macro is preserved. Delivers macro recording foundation for Emacs key bindings.

**Acceptance Scenarios**:

1. **Given** a new EmacsState instance, **When** I check the initial state, **Then** all properties match initial values: [FR-016, FR-017, FR-018, CHK026]
   - `Macro` = empty list (not null)
   - `CurrentRecording` = `null`
   - `IsRecording` = `false`

2. **Given** EmacsState not recording, **When** I call `StartMacro()`, **Then** [FR-019]:
   - `CurrentRecording` becomes empty list (not null)
   - `IsRecording` = `true`

3. **Given** EmacsState recording a macro with key presses appended, **When** I call `EndMacro()`, **Then** [FR-020]:
   - `CurrentRecording` list is copied to `Macro`
   - `CurrentRecording` becomes `null`
   - `IsRecording` = `false`
   - `Macro` contains all previously appended key presses

4. **Given** EmacsState not recording, **When** I call `EndMacro()`, **Then** [CHK027]:
   - `Macro` is set to empty list (graceful handling; no exception)
   - `CurrentRecording` remains `null`
   - `IsRecording` = `false`

5. **Given** EmacsState recording a macro, **When** I call `Reset()`, **Then** [FR-021, CHK024]:
   - `CurrentRecording` becomes `null`
   - `IsRecording` = `false`
   - **NOT cleared**: `Macro` (preserves last recorded macro)

6. **Given** EmacsState recording, **When** I call `AppendToRecording(keyPress)`, **Then** [CHK043]:
   - Key press is added to `CurrentRecording` list

7. **Given** EmacsState not recording, **When** I call `AppendToRecording(keyPress)`, **Then** [CHK043]:
   - Nothing happens (no exception, no effect)

---

### User Story 3 - Editing Mode Selection (Priority: P1)

A developer configuring a terminal application needs to specify whether the application uses Vi or Emacs key bindings. The editing mode enum allows the application to select the appropriate set of key bindings.

**Why this priority**: The editing mode selection is required by the Application layer to determine which key binding set to activate. Without this, the application cannot be configured for different editing styles.

**Independent Test**: Can be fully tested by setting the editing mode to Vi or Emacs and verifying the correct enum value is stored. Delivers the configuration option for editing style selection.

**Acceptance Scenarios**:

1. **Given** an editing mode variable, **When** I set it to `EditingMode.Vi`, **Then** the value is `EditingMode.Vi` [FR-001]
2. **Given** an editing mode variable, **When** I set it to `EditingMode.Emacs`, **Then** the value is `EditingMode.Emacs` [FR-001]
3. **Given** EditingMode enum, **When** I enumerate all values, **Then** there are exactly 2 values: `Vi` and `Emacs` [CHK009]

---

### User Story 4 - Vi Character Find Operations (Priority: P2)

A developer implementing Vi key bindings needs to store and repeat character find operations (f, F, t, T commands). The CharacterFind class stores the target character and direction for use with the repeat (;) and reverse (,) commands.

**Why this priority**: Character find is a secondary Vi feature that enhances navigation. While important for full Vi fidelity, the core editing functionality works without it.

**Independent Test**: Can be fully tested by creating CharacterFind instances with various characters and directions, then verifying the properties. Delivers repeat-find capability for Vi navigation.

**Acceptance Scenarios**:

1. **Given** a forward character find for 'x', **When** I create `new CharacterFind("x")`, **Then** `Character` = `"x"` and `Backwards` = `false` [FR-004]
2. **Given** a backward character find for 'y', **When** I create `new CharacterFind("y", Backwards: true)`, **Then** `Character` = `"y"` and `Backwards` = `true` [FR-004]
3. **Given** two CharacterFind instances with same values, **When** I compare them, **Then** they are equal (record value semantics) [CHK051]
4. **Given** CharacterFind is declared as `sealed record`, **When** attempting to inherit, **Then** compilation fails [CHK051, Constitution II]

---

### User Story 5 - Vi Named Registers (Priority: P2)

A developer implementing Vi key bindings needs named registers (a-z) to store yanked/deleted text. The ViState provides methods for accessing named registers (dictionary pattern with thread safety).

**Why this priority**: Named registers extend Vi's clipboard functionality but are not required for basic yank/paste operations which use the default register.

**Independent Test**: Can be fully tested by storing and retrieving clipboard data from named registers. Delivers extended clipboard functionality for Vi power users.

**Acceptance Scenarios**:

1. **Given** a new ViState, **When** I call `GetNamedRegisterNames()`, **Then** it returns an empty collection [FR-010, CHK042]
2. **Given** ViState, **When** I call `SetNamedRegister("a", clipboardData)`, **Then** subsequent `GetNamedRegister("a")` returns that data [FR-010, CHK042]
3. **Given** ViState with register 'a' set, **When** I call `ClearNamedRegister("a")`, **Then** it returns `true` and `GetNamedRegister("a")` returns `null` [CHK042]
4. **Given** ViState with no register 'z' set, **When** I call `ClearNamedRegister("z")`, **Then** it returns `false` [CHK042]
5. **Given** ViState with no register 'x' set, **When** I call `GetNamedRegister("x")`, **Then** it returns `null` [FR-010]
6. **Given** a valid register name (any non-null string), **When** I call `SetNamedRegister`, **Then** it accepts any string key (no validation; follows Python behavior) [CHK055]

---

### User Story 6 - Vi Macro Recording (Priority: P2)

A developer implementing Vi key bindings needs to record macros into named registers (q command). The ViState tracks which register is recording and accumulates the key data as a string.

**Why this priority**: Macro recording extends Vi functionality but is not required for basic editing operations.

**Independent Test**: Can be fully tested by starting recording into a register, verifying RecordingRegister is set, and checking CurrentRecording accumulates data. Delivers macro recording for Vi automation.

**Acceptance Scenarios**:

1. **Given** a new ViState, **When** I check `RecordingRegister`, **Then** it is `null` [FR-013]
2. **Given** a new ViState, **When** I check `CurrentRecording`, **Then** it is `""` (empty string, not null) [FR-013]
3. **Given** ViState with `RecordingRegister` set to "a", **When** I set `CurrentRecording += "text"`, **Then** `CurrentRecording` contains "text" [FR-013]
4. **Given** ViState recording a macro, **When** `Reset()` is called, **Then** `RecordingRegister` = `null` and `CurrentRecording` = `""` [FR-015, CHK028]

---

### User Story 7 - Buffer Name Constants (Priority: P3)

A developer working with multiple buffers needs standard names to reference well-known buffers (default, search, system). The buffer name constants provide consistent identifiers across the codebase.

**Why this priority**: While important for multi-buffer scenarios, single-buffer applications can function without explicit buffer names.

**Independent Test**: Can be fully tested by verifying the constant values match expected strings. Delivers consistent buffer identification.

**Acceptance Scenarios**:

1. **Given** the BufferNames class, **When** I access `BufferNames.SearchBuffer`, **Then** it equals `"SEARCH_BUFFER"` [FR-002]
2. **Given** the BufferNames class, **When** I access `BufferNames.DefaultBuffer`, **Then** it equals `"DEFAULT_BUFFER"` [FR-002]
3. **Given** the BufferNames class, **When** I access `BufferNames.SystemBuffer`, **Then** it equals `"SYSTEM_BUFFER"` [FR-002]
4. **Given** BufferNames is a static class, **When** attempting to instantiate, **Then** compilation fails [FR-002]

---

### Edge Cases *(mandatory)*

This section explicitly specifies behavior for boundary conditions and exceptional scenarios.

#### CharacterFind Edge Cases [CHK054, CHK060]

| Scenario | Input | Expected Behavior |
|----------|-------|-------------------|
| Null character | `new CharacterFind(null!)` | Allowed (no validation; follows Python behavior where caller ensures valid input) |
| Empty string | `new CharacterFind("")` | Allowed (no validation) |
| Multi-character string | `new CharacterFind("abc")` | Allowed (no validation; Python accepts any string) |
| Unicode character | `new CharacterFind("日")` | Allowed; stores full string |

**Rationale**: Python Prompt Toolkit does not validate the `char` parameter in `CharacterFind.__init__`. The C# implementation follows the same permissive approach, delegating validation responsibility to the caller.

#### ViState Edge Cases

| Scenario | Trigger | Expected Behavior | Reference |
|----------|---------|-------------------|-----------|
| Set InputMode to Navigation during digraph | `WaitingForDigraph = true`, then `InputMode = Navigation` | `WaitingForDigraph` → `false`; `OperatorFunc` → `null`; `OperatorArg` → `null` | CHK029, FR-006 |
| Reset() during macro recording | `RecordingRegister = "a"`, `CurrentRecording = "data"`, then `Reset()` | `RecordingRegister` → `null`; `CurrentRecording` → `""` | CHK028, FR-015 |
| GetNamedRegister for nonexistent key | `GetNamedRegister("z")` when not set | Returns `null` | FR-010 |
| SetNamedRegister with null data | `SetNamedRegister("a", null!)` | Allowed (no validation; caller responsibility) | CHK055 |
| ClearNamedRegister for nonexistent key | `ClearNamedRegister("z")` when not set | Returns `false` | CHK042 |
| Property access during Reset() | Thread A calls `Reset()` while Thread B reads `InputMode` | Thread-safe; Lock ensures atomic operation | CHK057 |

#### EmacsState Edge Cases

| Scenario | Trigger | Expected Behavior | Reference |
|----------|---------|-------------------|-----------|
| EndMacro() when not recording | `CurrentRecording = null`, then `EndMacro()` | `Macro` → empty list (not null); `CurrentRecording` remains `null` | CHK027, FR-020 |
| AppendToRecording when not recording | `CurrentRecording = null`, then `AppendToRecording(kp)` | No effect; no exception thrown | CHK043 |
| StartMacro() when already recording | `CurrentRecording` already a list, then `StartMacro()` | Replaces with new empty list; previous recording lost | CHK056 |
| EndMacro() with empty recording | Recording started but no keys appended, then `EndMacro()` | `Macro` → empty list | CHK061 |
| Property access during EndMacro() | Thread A calls `EndMacro()` while Thread B reads `Macro` | Thread-safe; Lock ensures atomic operation | CHK058 |
| Reset() during recording | `CurrentRecording` active, then `Reset()` | `CurrentRecording` → `null`; `Macro` preserved | FR-021 |

#### OperatorFuncDelegate Edge Cases [CHK059]

| Scenario | Input | Expected Behavior |
|----------|-------|-------------------|
| Invocation with null textObject | `operatorFunc(e, null)` | Allowed; handler must handle null textObject |
| Delegate is null | `viState.OperatorFunc` is `null` | Caller must check before invoking |

## Requirements *(mandatory)*

### Functional Requirements

#### Enum Types

- **FR-001**: System MUST provide an `EditingMode` enum with exactly 2 values: `Vi` and `Emacs`, matching Python Prompt Toolkit's `EditingMode` enum
- **FR-002**: System MUST provide `BufferNames` static class with 3 constants matching Python exactly:
  - `SearchBuffer` = `"SEARCH_BUFFER"`
  - `DefaultBuffer` = `"DEFAULT_BUFFER"`
  - `SystemBuffer` = `"SYSTEM_BUFFER"`
- **FR-003**: System MUST provide an `InputMode` enum with exactly 5 values: `Insert`, `InsertMultiple`, `Navigation`, `Replace`, `ReplaceSingle`, matching Python Prompt Toolkit's `InputMode` enum

#### CharacterFind

- **FR-004**: System MUST provide `CharacterFind` as a `sealed record` with:
  - `Character` property (`string` type)
  - `Backwards` property (`bool` type, default `false`)
  - Immutable and inherently thread-safe

#### ViState Properties

- **FR-005**: `ViState` MUST initialize with all default values as specified in the API Fidelity section
- **FR-006**: `ViState` MUST clear `WaitingForDigraph`, `OperatorFunc`, and `OperatorArg` when `InputMode` is set to `Navigation`
- **FR-007**: `ViState` MUST provide `LastCharacterFind` property (`CharacterFind?` type)
- **FR-008**: `ViState` MUST provide `OperatorFunc` property (`OperatorFuncDelegate?` type)
- **FR-009**: `ViState` MUST provide `OperatorArg` property (`int?` type)
- **FR-010**: `ViState` MUST provide methods for named register access:
  - `GetNamedRegister(string name)` → `ClipboardData?`
  - `SetNamedRegister(string name, ClipboardData data)` → `void`
  - `ClearNamedRegister(string name)` → `bool`
  - `GetNamedRegisterNames()` → `IReadOnlyCollection<string>`
- **FR-011**: `ViState` MUST provide `WaitingForDigraph` (`bool`) and `DigraphSymbol1` (`string?`) properties
- **FR-012**: `ViState` MUST provide `TildeOperator` property (`bool` type)
- **FR-013**: `ViState` MUST provide `RecordingRegister` (`string?`) and `CurrentRecording` (`string`, default `""`) properties
- **FR-014**: `ViState` MUST provide `TemporaryNavigationMode` property (`bool` type)
- **FR-015**: `ViState.Reset()` MUST set:
  - `InputMode` → `InputMode.Insert`
  - `WaitingForDigraph` → `false`
  - `OperatorFunc` → `null`
  - `OperatorArg` → `null`
  - `RecordingRegister` → `null`
  - `CurrentRecording` → `""`
  - MUST NOT clear: `LastCharacterFind`, `NamedRegisters`, `TildeOperator`, `TemporaryNavigationMode`, `DigraphSymbol1`

#### EmacsState Properties and Methods

- **FR-016**: `EmacsState` MUST provide `Macro` property (`IReadOnlyList<KeyPress>`, default empty list, never null)
- **FR-017**: `EmacsState` MUST provide `CurrentRecording` property (`IReadOnlyList<KeyPress>?`, default `null`)
- **FR-018**: `EmacsState` MUST provide `IsRecording` computed property returning `CurrentRecording != null`
- **FR-019**: `EmacsState.StartMacro()` MUST set `CurrentRecording` to a new empty list
- **FR-020**: `EmacsState.EndMacro()` MUST:
  - If recording: copy `CurrentRecording` to `Macro`, set `CurrentRecording` to `null`
  - If not recording: set `Macro` to empty list
- **FR-021**: `EmacsState.Reset()` MUST set `CurrentRecording` to `null` (does NOT clear `Macro`)
- **FR-023**: `EmacsState.AppendToRecording(KeyPress)` MUST:
  - If recording: add key press to `CurrentRecording`
  - If not recording: do nothing (no exception)

#### Delegate Types

- **FR-024**: System MUST provide `OperatorFuncDelegate` delegate type:
  ```csharp
  public delegate NotImplementedOrNone OperatorFuncDelegate(KeyPressEvent e, object? textObject);
  ```

#### Thread Safety

- **FR-022**: All mutable state classes (`ViState`, `EmacsState`) MUST be thread-safe per Constitution XI
- **FR-025**: `ViState` MUST use `System.Threading.Lock` for synchronization
- **FR-026**: `EmacsState` MUST use `System.Threading.Lock` for synchronization
- **FR-027**: All property getters/setters MUST be atomic operations
- **FR-028**: Property getters returning collections (`Macro`, `CurrentRecording`) MUST return copies to ensure thread safety
- **FR-029**: `GetNamedRegisterNames()` MUST return a copy of the register name collection

#### Type Constraints

- **FR-030**: `ViState` MUST be declared as `sealed class`
- **FR-031**: `EmacsState` MUST be declared as `sealed class`
- **FR-032**: All 6 public types MUST be in the `Stroke.KeyBinding` namespace

#### Documentation

- **FR-033**: All public types and members MUST have XML documentation comments
- **FR-034**: Thread safety guarantees MUST be documented in class-level XML comments

### Key Entities

| Type | C# Kind | Mutability | Thread Safety | Description |
|------|---------|------------|---------------|-------------|
| `EditingMode` | `enum` | Immutable | Inherent | Active key binding set (Vi or Emacs) |
| `InputMode` | `enum` | Immutable | Inherent | Vi input states (Insert, Navigation, etc.) |
| `BufferNames` | `static class` | Immutable | Inherent | String constants for buffer identifiers |
| `CharacterFind` | `sealed record` | Immutable | Inherent | Character search target and direction |
| `ViState` | `sealed class` | Mutable | Lock-protected | Vi-specific navigation state |
| `EmacsState` | `sealed class` | Mutable | Lock-protected | Emacs-specific macro state |
| `OperatorFuncDelegate` | `delegate` | N/A | N/A | Callback for pending Vi operators |

### State Transitions

#### InputMode Transitions (ViState)

```
                    ┌─────────────┐
                    │   Insert    │ ← Initial state
                    └──────┬──────┘
                           │ Escape
                           ▼
                    ┌─────────────┐
         ┌─────────│ Navigation  │─────────┐
         │         └──────┬──────┘         │
         │ i,a,o,etc      │ R              │ r
         ▼                ▼                ▼
  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐
  │   Insert    │  │   Replace   │  │ReplaceSingle│
  └─────────────┘  └──────┬──────┘  └──────┬──────┘
                          │ Escape          │ char
                          ▼                 ▼
                   ┌─────────────┐   ┌─────────────┐
                   │ Navigation  │   │   Insert    │
                   └─────────────┘   └─────────────┘
```

**Side Effects on InputMode = Navigation**:
- `WaitingForDigraph` → `false`
- `OperatorFunc` → `null`
- `OperatorArg` → `null`

#### EmacsState Macro Recording State Machine

```
                    ┌─────────────┐
                    │ Not Recording│ ← Initial state
                    │ IsRecording=F│
                    └──────┬──────┘
                           │ StartMacro()
                           ▼
                    ┌─────────────┐
                    │  Recording  │
                    │ IsRecording=T│
                    └──────┬──────┘
         ┌─────────────────┼─────────────────┐
         │ EndMacro()      │ Reset()         │ StartMacro()
         ▼                 ▼                 ▼
  ┌─────────────┐   ┌─────────────┐   ┌─────────────┐
  │Not Recording│   │Not Recording│   │  Recording  │
  │Macro=saved  │   │Macro=kept   │   │(new empty)  │
  └─────────────┘   └─────────────┘   └─────────────┘
```

## Success Criteria *(mandatory)*

### Measurable Outcomes

| ID | Criterion | Measurement Method | Target |
|----|-----------|-------------------|--------|
| **SC-001** | API fidelity | Compare enum value counts and names against Python source | 100% match: EditingMode (2), InputMode (5), BufferNames (3) |
| **SC-002** | Test coverage | `dotnet test --collect:"XPlat Code Coverage"` | ≥80% line coverage for ViState, EmacsState, CharacterFind |
| **SC-003** | State transition correctness | Unit tests for all InputMode transitions, Reset(), and side effects | All 7 user story acceptance scenarios pass |
| **SC-004** | Thread safety | Concurrent stress tests with 10+ threads, 1000+ operations each | No data corruption, no deadlocks, no race conditions |
| **SC-005** | Integration | Tests using real `KeyPress` and `ClipboardData` types | All integration tests pass with actual dependency types |
| **SC-006** | Documentation | XML documentation present on all public types/members | 100% public API documented |
| **SC-007** | File organization | Each type in separate file | 6 source files, each <1000 LOC |

### Verification Procedures

**SC-001 Verification**:
```csharp
// Automated test to verify enum value counts
Assert.Equal(2, Enum.GetValues<EditingMode>().Length);
Assert.Equal(5, Enum.GetValues<InputMode>().Length);
// Verify specific values exist
Assert.True(Enum.IsDefined(typeof(EditingMode), EditingMode.Vi));
Assert.True(Enum.IsDefined(typeof(EditingMode), EditingMode.Emacs));
```

**SC-004 Verification**:
```csharp
// Concurrent stress test pattern
var viState = new ViState();
var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() => {
    for (int i = 0; i < 1000; i++) {
        viState.InputMode = InputMode.Navigation;
        viState.InputMode = InputMode.Insert;
        viState.SetNamedRegister("a", new ClipboardData("data"));
        _ = viState.GetNamedRegister("a");
    }
}));
await Task.WhenAll(tasks); // No exceptions = pass
```

## Assumptions

| ID | Assumption | Migration Strategy | Reference |
|----|------------|-------------------|-----------|
| **A-001** | `ITextObject` interface will be defined in a future feature | `OperatorFuncDelegate` uses `object?` as placeholder; when `ITextObject` is defined, update delegate signature | CHK062 |
| **A-002** | `KeyPress` type from Feature 14 is available | Verified: `Stroke.Input.KeyPress` exists | CHK063 |
| **A-003** | `ClipboardData` type from Feature 04 is available | Verified: `Stroke.Clipboard.ClipboardData` exists | CHK064 |
| **A-004** | `KeyPressEvent` type from Feature 22 is available | Verified: `Stroke.KeyBinding.KeyPressEvent` exists | CHK065 |
| **A-005** | `NotImplementedOrNone` type from Feature 22 is available | Verified: `Stroke.KeyBinding.NotImplementedOrNone` exists | CHK034 |

## Dependencies

| Dependency | Namespace | Feature | Used By | Purpose |
|------------|-----------|---------|---------|---------|
| `KeyPress` | `Stroke.Input` | 014-input-system | `EmacsState.Macro`, `EmacsState.CurrentRecording` | Macro key storage |
| `ClipboardData` | `Stroke.Clipboard` | 004-clipboard-system | `ViState` named registers | Register storage |
| `KeyPressEvent` | `Stroke.KeyBinding` | 022-key-bindings-system | `OperatorFuncDelegate` | Operator callback parameter |
| `NotImplementedOrNone` | `Stroke.KeyBinding` | 022-key-bindings-system | `OperatorFuncDelegate` | Operator callback return type |

### Future Dependencies (This Feature Used By)

| Future Feature | How Used |
|----------------|----------|
| Key binding handlers | Mode-conditional key binding behavior |
| Application layer | Editing mode configuration |
| Filter system | `InViMode`, `InEmacsMode`, `InNavigationMode` filters |
| Key processor | State-aware key processing |

## Out of Scope

The following are explicitly NOT part of this feature:

| Item | Rationale | Future Feature |
|------|-----------|----------------|
| Key binding implementations | Separate concern | Vi/Emacs key bindings feature |
| Text object implementations (`ITextObject`) | Complex feature | Vi text objects feature |
| Key processor integration | Orchestration layer | Key processor feature |
| Application-level mode switching | Higher layer | Application feature |
| Filter implementations (`InViMode`, etc.) | Depends on Filter system | Filter extensions feature |

## File Organization

Per Constitution X (File Size Limits) and III (Layered Architecture):

| File | Contents | Namespace | Max LOC |
|------|----------|-----------|---------|
| `src/Stroke/KeyBinding/EditingMode.cs` | `EditingMode` enum | `Stroke.KeyBinding` | <100 |
| `src/Stroke/KeyBinding/BufferNames.cs` | `BufferNames` static class | `Stroke.KeyBinding` | <100 |
| `src/Stroke/KeyBinding/InputMode.cs` | `InputMode` enum | `Stroke.KeyBinding` | <100 |
| `src/Stroke/KeyBinding/CharacterFind.cs` | `CharacterFind` record | `Stroke.KeyBinding` | <100 |
| `src/Stroke/KeyBinding/ViState.cs` | `ViState` class, `OperatorFuncDelegate` | `Stroke.KeyBinding` | <500 |
| `src/Stroke/KeyBinding/EmacsState.cs` | `EmacsState` class | `Stroke.KeyBinding` | <300 |

### Test File Organization

| File | Tests | Max LOC |
|------|-------|---------|
| `tests/Stroke.Tests/KeyBinding/EditingModeTests.cs` | Enum values, count validation | <100 |
| `tests/Stroke.Tests/KeyBinding/BufferNamesTests.cs` | Constant values | <100 |
| `tests/Stroke.Tests/KeyBinding/InputModeTests.cs` | Enum values, count validation | <100 |
| `tests/Stroke.Tests/KeyBinding/CharacterFindTests.cs` | Record equality, immutability | <200 |
| `tests/Stroke.Tests/KeyBinding/ViStateTests.cs` | State management, thread safety | <500 |
| `tests/Stroke.Tests/KeyBinding/EmacsStateTests.cs` | Macro recording, thread safety | <400 |

---

## Requirements Traceability Matrix

This matrix ensures all requirements are traceable to user stories, acceptance scenarios, and success criteria.

### Functional Requirements → User Stories

| Requirement | User Story | Acceptance Scenario(s) |
|-------------|------------|------------------------|
| FR-001 | US3 | US3.1, US3.2, US3.3 |
| FR-002 | US7 | US7.1, US7.2, US7.3, US7.4 |
| FR-003 | US1 | US1.1 (InputMode.Insert default) |
| FR-004 | US4 | US4.1, US4.2, US4.3, US4.4 |
| FR-005 | US1 | US1.1 |
| FR-006 | US1 | US1.2 |
| FR-007 | US4 | US4.1, US4.2 |
| FR-008 | US1 | US1.2 |
| FR-009 | US1 | US1.2 |
| FR-010 | US5 | US5.1, US5.2, US5.3, US5.4, US5.5, US5.6 |
| FR-011 | US1 | US1.2 |
| FR-012 | US1 | US1.1 (TildeOperator default) |
| FR-013 | US6 | US6.1, US6.2, US6.3, US6.4 |
| FR-014 | US1 | US1.1 (TemporaryNavigationMode default) |
| FR-015 | US1 | US1.3 |
| FR-016 | US2 | US2.1, US2.3 |
| FR-017 | US2 | US2.1, US2.3 |
| FR-018 | US2 | US2.1, US2.2 |
| FR-019 | US2 | US2.2 |
| FR-020 | US2 | US2.3, US2.4 |
| FR-021 | US2 | US2.5 |
| FR-022 | US1, US2 | Thread safety scenarios |
| FR-023 | US2 | US2.6, US2.7 |
| FR-024 | US1 | US1.2 (OperatorFunc) |
| FR-025 | US1 | Thread safety scenarios |
| FR-026 | US2 | Thread safety scenarios |
| FR-027 | US1, US2 | Thread safety scenarios |
| FR-028 | US2 | Thread safety scenarios |
| FR-029 | US5 | US5.1 |
| FR-030 | US1 | Type constraint validation |
| FR-031 | US2 | Type constraint validation |
| FR-032 | All | Namespace validation |
| FR-033 | All | Documentation validation |
| FR-034 | US1, US2 | Documentation validation |

### Success Criteria → Acceptance Tests

| Success Criterion | Validated By |
|-------------------|--------------|
| SC-001 | US3.3 (EditingMode count), InputMode count test, BufferNames value tests |
| SC-002 | Coverage report for ViState, EmacsState, CharacterFind |
| SC-003 | All user story acceptance scenarios |
| SC-004 | ViState/EmacsState concurrent stress tests (10 threads × 1000 ops) |
| SC-005 | Integration tests with real KeyPress and ClipboardData |
| SC-006 | XML documentation completeness check |
| SC-007 | File count and LOC verification |

### Type Coverage

| Public Type | Requirements | User Stories |
|-------------|--------------|--------------|
| `EditingMode` | FR-001 | US3 |
| `BufferNames` | FR-002 | US7 |
| `InputMode` | FR-003 | US1 |
| `CharacterFind` | FR-004 | US4 |
| `ViState` | FR-005 through FR-015, FR-022, FR-025, FR-027, FR-029, FR-030, FR-032-FR-034 | US1, US5, US6 |
| `EmacsState` | FR-016 through FR-021, FR-023, FR-022, FR-026-FR-028, FR-031-FR-034 | US2 |
| `OperatorFuncDelegate` | FR-024 | US1 |

---

## Revision History

| Date | Version | Changes |
|------|---------|---------|
| 2026-01-27 | 1.0 | Initial specification |
| 2026-01-27 | 1.1 | Added API Fidelity section with explicit enum/property mappings |
| 2026-01-27 | 1.2 | Enhanced user stories with detailed acceptance scenarios and requirement traceability |
| 2026-01-27 | 1.3 | Added comprehensive edge cases, state transition diagrams, thread safety requirements |
| 2026-01-27 | 1.4 | Added Requirements Traceability Matrix, Constitution Compliance section, file organization |
