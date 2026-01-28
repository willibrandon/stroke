# Data Model: Key Bindings System

**Feature**: 022-key-bindings-system
**Date**: 2026-01-27
**Status**: Complete

## Entity Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                         IKeyBindingsBase                         │
│                          (interface)                             │
├─────────────────────────────────────────────────────────────────┤
│ + Version: object                                                │
│ + Bindings: IReadOnlyList<Binding>                              │
│ + GetBindingsForKeys(keys): IReadOnlyList<Binding>              │
│ + GetBindingsStartingWithKeys(keys): IReadOnlyList<Binding>     │
└─────────────────────────────────────────────────────────────────┘
                              ▲
                              │ implements
       ┌──────────────────────┼──────────────────────┐
       │                      │                      │
┌──────┴──────┐       ┌───────┴──────┐      ┌───────┴───────┐
│ KeyBindings │       │KeyBindingsProxy│      │    (other     │
│  (concrete) │       │   (abstract)  │      │  wrappers)    │
└─────────────┘       └───────┬───────┘      └───────────────┘
                              │
          ┌───────────────────┼───────────────────┐
          │                   │                   │
┌─────────┴─────┐  ┌──────────┴────────┐  ┌──────┴──────────┐
│ Conditional   │  │   Merged          │  │   Dynamic       │
│ KeyBindings   │  │   KeyBindings     │  │   KeyBindings   │
└───────────────┘  └───────────────────┘  └─────────────────┘
                            │
                   ┌────────┴────────┐
                   │  GlobalOnly     │
                   │  KeyBindings    │
                   └─────────────────┘
```

---

## Entity: KeyOrChar

**Purpose**: Union type representing either a `Keys` enum value or a single character.

**Type**: `readonly record struct`

| Field | Type | Description |
|-------|------|-------------|
| `_key` | `Keys?` | The Keys enum value (if this is a key) |
| `_char` | `char?` | The character value (if this is a char) |

**Properties**:
| Property | Type | Description |
|----------|------|-------------|
| `IsKey` | `bool` | True if this represents a Keys enum value |
| `IsChar` | `bool` | True if this represents a character |
| `Key` | `Keys` | The key value (throws if IsChar) |
| `Char` | `char` | The character value (throws if IsKey) |

**Invariants**:
- Exactly one of `_key` or `_char` has a value (mutually exclusive)
- Value is immutable after construction

**Implicit Conversions**:
- `Keys` → `KeyOrChar`
- `char` → `KeyOrChar`
- `string` (single char) → `KeyOrChar`

---

## Entity: KeyPress

**Purpose**: Represents a single key press with optional raw data.

**Type**: `readonly record struct`

| Field | Type | Description |
|-------|------|-------------|
| `Key` | `KeyOrChar` | The key that was pressed |
| `Data` | `string` | Raw terminal data (escape sequence or character) |

**Validation Rules**:
- If `Key.IsChar`, `Data` defaults to that character as string
- If `Key.IsKey`, `Data` defaults to the key's enum name (e.g., "ControlA", "Enter")

**Source Reference**: Python `KeyPress` class in `key_processor.py:36-61`

---

## Entity: Binding

**Purpose**: Immutable association between a key sequence and a handler.

**Type**: `sealed class` (immutable)

| Field | Type | Description |
|-------|------|-------------|
| `Keys` | `IReadOnlyList<KeyOrChar>` | The key sequence to match |
| `Handler` | `KeyHandlerCallable` | The handler function |
| `Filter` | `IFilter` | Condition for when binding is active |
| `Eager` | `IFilter` | Condition for eager matching |
| `IsGlobal` | `IFilter` | Condition for global binding |
| `SaveBefore` | `Func<KeyPressEvent, bool>` | Whether to save buffer before execution |
| `RecordInMacro` | `IFilter` | Whether to record in macro |

**Validation Rules**:
- `Keys` must not be empty
- `Handler` must not be null
- All filters default to `Always` or `Never` as appropriate

**Methods**:
| Method | Return | Description |
|--------|--------|-------------|
| `Call(event)` | `void` | Invokes handler, creates background task if async |

**Source Reference**: Python `Binding` class in `key_bindings.py:99-146`

---

## Entity: IKeyBindingsBase

**Purpose**: Interface defining the contract for all key binding registries.

**Type**: `interface`

| Property | Type | Description |
|----------|------|-------------|
| `Version` | `object` | Cache invalidation version (hashable) |
| `Bindings` | `IReadOnlyList<Binding>` | All bindings in this registry |

| Method | Return | Description |
|--------|--------|-------------|
| `GetBindingsForKeys(keys)` | `IReadOnlyList<Binding>` | Exact matches for key sequence |
| `GetBindingsStartingWithKeys(keys)` | `IReadOnlyList<Binding>` | Prefixes of longer sequences |

**Source Reference**: Python `KeyBindingsBase` class in `key_bindings.py:153-199`

---

## Entity: KeyBindings

**Purpose**: Concrete mutable registry with add/remove and caching.

**Type**: `sealed class` implementing `IKeyBindingsBase`

| Field | Type | Access | Description |
|-------|------|--------|-------------|
| `_lock` | `Lock` | private | Thread synchronization |
| `_bindings` | `List<Binding>` | private | Stored bindings |
| `_version` | `int` | private | Version counter |
| `_forKeysCache` | `SimpleCache` | private | Exact match cache (10,000) |
| `_startingCache` | `SimpleCache` | private | Prefix match cache (1,000) |

**Methods**:
| Method | Return | Description |
|--------|--------|-------------|
| `Add(keys, filter, eager, ...)` | `Func<T,T>` | Decorator-style add |
| `Remove(handler)` | `void` | Remove by handler reference |
| `Remove(keys...)` | `void` | Remove by key sequence |

**State Transitions**:
```
Initial → [Add] → HasBindings → [Add] → HasBindings
HasBindings → [Remove] → HasBindings | Initial
Any → [Add/Remove] → Version incremented, Cache cleared
```

**Thread Safety**: All public methods are atomic via `Lock.EnterScope()`

**Source Reference**: Python `KeyBindings` class in `key_bindings.py:206-429`

---

## Entity: KeyBindingsProxy

**Purpose**: Base class for wrapper types that delegate to another registry.

**Type**: `abstract class` implementing `IKeyBindingsBase`

| Field | Type | Description |
|-------|------|-------------|
| `_bindings2` | `IKeyBindingsBase` | Cached delegate registry |
| `_lastVersion` | `object` | Last seen version |

**Methods**:
| Method | Description |
|--------|-------------|
| `UpdateCache()` | Abstract: Update `_bindings2` if version changed |

**Subclasses**: ConditionalKeyBindings, MergedKeyBindings, DynamicKeyBindings, GlobalOnlyKeyBindings

**Source Reference**: Python `_Proxy` class in `key_bindings.py:494-529`

---

## Entity: ConditionalKeyBindings

**Purpose**: Wrapper that applies an additional filter to all contained bindings.

**Type**: `sealed class` extending `KeyBindingsProxy`

| Field | Type | Description |
|-------|------|-------------|
| `KeyBindings` | `IKeyBindingsBase` | Wrapped registry |
| `Filter` | `IFilter` | Additional condition |

**Behavior**:
- When cache updated, copies all bindings with `filter = this.Filter & binding.Filter`
- Preserves eager, is_global, save_before, record_in_macro unchanged

**Source Reference**: Python `ConditionalKeyBindings` class in `key_bindings.py:532-580`

---

## Entity: MergedKeyBindings

**Purpose**: Combines multiple registries into a single view.

**Type**: `sealed class` extending `KeyBindingsProxy`

| Field | Type | Description |
|-------|------|-------------|
| `Registries` | `IReadOnlyList<IKeyBindingsBase>` | Child registries |

**Behavior**:
- Version is tuple of all child versions
- Bindings are concatenated in registry order
- Last binding for a key sequence wins (registration order)

**Source Reference**: Python `_MergedKeyBindings` class in `key_bindings.py:583-612`

---

## Entity: DynamicKeyBindings

**Purpose**: Delegates to a callable that returns a registry at runtime.

**Type**: `sealed class` extending `KeyBindingsProxy`

| Field | Type | Description |
|-------|------|-------------|
| `GetKeyBindings` | `Func<IKeyBindingsBase?>` | Provider callable |
| `_dummy` | `KeyBindings` | Empty fallback |

**Behavior**:
- Calls provider on every access
- Returns empty bindings if provider returns null
- Version tracks both identity and version of returned registry

**Source Reference**: Python `DynamicKeyBindings` class in `key_bindings.py:625-645`

---

## Entity: GlobalOnlyKeyBindings

**Purpose**: Filters to expose only global bindings from a wrapped registry.

**Type**: `sealed class` extending `KeyBindingsProxy`

| Field | Type | Description |
|-------|------|-------------|
| `KeyBindings` | `IKeyBindingsBase` | Wrapped registry |

**Behavior**:
- Only includes bindings where `binding.IsGlobal()` returns true
- Filter evaluated at cache update time

**Source Reference**: Python `GlobalOnlyKeyBindings` class in `key_bindings.py:647-673`

---

## Entity: KeyPressEvent

**Purpose**: Event data passed to key binding handlers.

**Type**: `class`

| Property | Type | Description |
|----------|------|-------------|
| `KeyProcessor` | `KeyProcessor` | Reference to processor (weak) |
| `KeySequence` | `IReadOnlyList<KeyPress>` | Keys that triggered this event |
| `PreviousKeySequence` | `IReadOnlyList<KeyPress>` | Previous key sequence |
| `IsRepeat` | `bool` | True if same handler as previous |
| `Arg` | `int` | Repetition argument (default 1) |
| `ArgPresent` | `bool` | True if arg was explicitly provided |
| `App` | `Application` | Current application |
| `CurrentBuffer` | `Buffer` | Current buffer |
| `Data` | `string` | Raw data of last key in sequence |

**Methods**:
| Method | Description |
|--------|-------------|
| `AppendToArgCount(digit)` | Adds digit to repetition argument |

**Source Reference**: Python `KeyPressEvent` class in `key_processor.py:424-527`

---

## Relationships

```
KeyBindings ────────────────────┬── contains many ──→ Binding
                                │
ConditionalKeyBindings ─────────┼── wraps ──→ IKeyBindingsBase
                                │              references ──→ IFilter
                                │
MergedKeyBindings ──────────────┼── aggregates ──→ IKeyBindingsBase[]
                                │
DynamicKeyBindings ─────────────┼── delegates to ──→ Func<IKeyBindingsBase?>
                                │
GlobalOnlyKeyBindings ──────────┴── filters ──→ IKeyBindingsBase

Binding ─────────────────────────── contains ──→ KeyOrChar[]
                                    references ──→ IFilter (filter, eager, is_global)
                                    references ──→ KeyHandlerCallable

KeyPressEvent ───────────────────── contains ──→ KeyPress[]
                                    references ──→ KeyProcessor (weak)
                                    references ──→ Application
                                    references ──→ Buffer
```

---

## Cache Key Types

For `SimpleCache` usage:

| Cache | Key Type | Value Type | Max Size |
|-------|----------|------------|----------|
| GetBindingsForKeys | `KeysTuple` (tuple of KeyOrChar) | `IReadOnlyList<Binding>` | 10,000 |
| GetBindingsStartingWithKeys | `KeysTuple` | `IReadOnlyList<Binding>` | 1,000 |

**KeysTuple Implementation**:
```csharp
// Use ImmutableArray<KeyOrChar> for tuple-like key with proper equality
using KeysTuple = System.Collections.Immutable.ImmutableArray<KeyOrChar>;
```

---

## Thread Safety Summary

| Entity | Thread Safe | Notes |
|--------|-------------|-------|
| KeyOrChar | Yes | Immutable struct |
| KeyPress | Yes | Immutable struct |
| Binding | Yes | Immutable class |
| IKeyBindingsBase | N/A | Interface |
| KeyBindings | Yes | Lock-protected |
| KeyBindingsProxy | Yes | Lock-protected |
| ConditionalKeyBindings | Yes | Inherits from proxy |
| MergedKeyBindings | Yes | Inherits from proxy |
| DynamicKeyBindings | Yes | Inherits from proxy |
| GlobalOnlyKeyBindings | Yes | Inherits from proxy |
| KeyPressEvent | Partial | Properties are safe, buffer access is external |
