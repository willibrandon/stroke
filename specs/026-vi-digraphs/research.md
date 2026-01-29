# Research: Vi Digraphs

**Feature**: 026-vi-digraphs
**Date**: 2026-01-28

## Research Summary

This feature has minimal unknowns as it's a direct data port from Python Prompt Toolkit.

## Research Items

### R-001: Dictionary Data Type Selection

**Question**: What is the optimal dictionary type for a static, read-only, thread-safe dictionary of 1,300+ entries?

**Decision**: Use `FrozenDictionary<(char, char), int>` (.NET 8+)

**Rationale**:
- `FrozenDictionary` is optimized for read-only scenarios with static data
- Automatically immutable; no synchronization needed
- Optimized hash table for fast lookups after freezing
- Available in .NET 8+ (project targets .NET 10)

**Alternatives Considered**:
- `ImmutableDictionary<K,V>`: Slightly slower lookups; designed for snapshot scenarios with modifications
- `Dictionary<K,V>` + `AsReadOnly()`: Not truly immutable; wrapper only prevents modifications through interface
- `IReadOnlyDictionary<K,V>` backed by regular Dictionary: Same as above

### R-002: Key Type Design

**Question**: How should the two-character digraph key be represented?

**Decision**: Use `(char, char)` value tuple as dictionary key

**Rationale**:
- Matches Python's `tuple[str, str]` key semantics
- Value tuples have built-in equality and hash code implementations
- Zero allocation for lookups (value type)
- Case-sensitive by default (matching RFC1345 requirement)

**Alternatives Considered**:
- `string` (concatenated): Would require string allocation for lookups; less efficient
- Custom struct: Unnecessary complexity; tuples provide everything needed
- Two separate parameters in methods: Can use tuple internally anyway

### R-003: Unicode Handling for Supplementary Planes

**Question**: How should code points above 0xFFFF (supplementary planes) be handled?

**Decision**: Provide both `GetCodePoint` (returns `int?`) and `GetString` (returns `string?`) methods

**Rationale**:
- `int` represents Unicode code points (0x0000 to 0x10FFFF)
- `char` cannot represent code points > 0xFFFF (it's a UTF-16 code unit)
- `char.ConvertFromUtf32(codePoint)` correctly handles surrogate pairs for high code points
- Callers who need the actual character should use `GetString`

**Alternatives Considered**:
- Only provide `GetString`: Loses code point information for callers who need it
- Provide `GetChar` that throws for high code points: Surprising failure mode
- Use `Rune` (.NET Core 3.0+): Good option but spec already mentions `GetCharacter` method name

### R-004: Null vs Exception for Invalid Lookups

**Question**: What should be returned for non-existent digraphs?

**Decision**: Return `null` (nullable return types) for both `Lookup` and `GetString` methods

**Rationale**:
- Matches FR-002 requirement: "returns null if not found"
- Matches SC-005: "Invalid digraph lookups consistently return null without throwing exceptions"
- Consistent with Python returning `None` via dictionary `.get()` semantics
- Allows caller to decide how to handle (ignore, beep, insert literal characters)

**Alternatives Considered**:
- Throw `KeyNotFoundException`: Violates spec requirements; exceptions for control flow is anti-pattern
- Return `default`: Ambiguous for value types; 0 is a valid code point (NUL character)

### R-005: Map Property Type

**Question**: What type should the public `Map` property expose?

**Decision**: `IReadOnlyDictionary<(char, char), int>`

**Rationale**:
- Matches FR-005: "expose the complete digraph dictionary as a read-only collection for enumeration"
- `FrozenDictionary<K,V>` implements `IReadOnlyDictionary<K,V>`
- Hides implementation detail while providing required functionality
- Allows enumeration, count, key/value access

**Alternatives Considered**:
- Expose `FrozenDictionary` directly: Exposes implementation detail
- Return copy of dictionary: Unnecessary allocation; underlying is already immutable

### R-006: Naming Conventions

**Question**: What naming should be used for the C# API?

**Decision**: Follow api-mapping.md conventions

| Python | C# |
|--------|-----|
| `DIGRAPHS` (module-level dict) | `Digraphs.Map` (static property) |
| `DIGRAPHS.get((c1, c2))` | `Digraphs.Lookup(char, char)` |
| N/A | `Digraphs.GetString(char, char)` |

**Rationale**:
- `UPPER_CASE` constants become `PascalCase` properties
- Module-level dict becomes static class property
- `Lookup` is clearer than `Get` for dictionary operation
- `GetString` is added convenience method (spec FR-004)

### R-007: Static Class vs Singleton

**Question**: Should this be a static class or a singleton instance?

**Decision**: Static class

**Rationale**:
- Python source uses module-level dictionary (equivalent to static in C#)
- No instance state needed; all data is static
- Simpler API (`Digraphs.Lookup(...)` vs `Digraphs.Instance.Lookup(...)`)
- Thread-safe by design (no instance to synchronize)

**Alternatives Considered**:
- Singleton: Unnecessary indirection; adds complexity without benefit
- Instance with DI: Overkill for static data; spec says "static dictionary"

## Conclusion

All research items resolved. The implementation is straightforward:
- Single `Digraphs` static class
- `FrozenDictionary<(char, char), int>` for storage
- Three public members: `Map` property, `Lookup` method, `GetString` method
- Null returns for invalid lookups
- Thread-safe via immutability
