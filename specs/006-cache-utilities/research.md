# Research: Cache Utilities

**Feature**: 006-cache-utilities
**Date**: 2026-01-23

## Research Tasks

### 1. Python Prompt Toolkit cache.py Analysis

**Task**: Analyze the reference implementation to ensure faithful port.

**Findings**:

1. **SimpleCache Implementation**:
   - Uses `dict[_T, _U]` for storage (`_data`)
   - Uses `deque[_T]` for tracking insertion order (`_keys`)
   - Default `maxsize = 8`
   - `get(key, getter_func)` method: returns cached value or calls getter and caches result
   - FIFO eviction: when `len(_data) > maxsize`, popleft from `_keys` and delete from `_data`
   - `clear()` method: reinitializes both `_data` and `_keys`
   - Key insight: eviction check uses `>` not `>=`, so actual size can be `maxsize + 1` momentarily during `get`

2. **FastDictCache Implementation**:
   - Inherits from `Dict[_K, _V]` (dictionary subclass)
   - Uses `deque[_K]` for tracking insertion order (`_keys`)
   - Default `size = 1,000,000`
   - `get_value` callable stored as instance attribute
   - `__missing__(key)` method: auto-populates on missing key access via indexer
   - FIFO eviction: checks `len(self) > size` before adding new entry
   - Key insight: eviction happens BEFORE adding new entry (check uses `>`)
   - The key type `_K` is `TypeVar("_K", bound=Tuple[Hashable, ...])` - tuple of hashables
   - Factory called with `get_value(*key)` - unpacks tuple as arguments

3. **memoized Decorator**:
   - Returns a decorator factory with configurable `maxsize` (default: 1024)
   - Uses `SimpleCache` internally for caching
   - Cache key construction: `(a, tuple(sorted(kw.items())))` - args tuple + sorted kwargs tuple
   - Uses `functools.wraps` to preserve function metadata
   - Uses `cast` to maintain type signature

**Decision**: Port all three components exactly as implemented in Python.

**Alternatives Considered**:
- Using `ConcurrentDictionary` for thread safety - rejected in favor of `Lock` with `EnterScope()` pattern per Constitution XI
- Using `LinkedList` instead of `Queue` for deque - unnecessary complexity
- Adding LRU semantics - not in original, would violate Principle I

---

### 2. C# Deque Equivalent

**Task**: Determine C# equivalent for Python's `collections.deque`.

**Findings**:
- Python `deque` provides O(1) append and popleft operations
- C# `Queue<T>` provides equivalent O(1) Enqueue (append) and Dequeue (popleft)
- Both maintain FIFO order
- `Queue<T>` is the idiomatic C# equivalent

**Decision**: Use `Queue<TKey>` for tracking insertion order.

**Alternatives Considered**:
- `LinkedList<T>`: More overhead, unnecessary for FIFO-only operations
- Custom deque implementation: Over-engineering for simple FIFO use case

---

### 3. Dictionary Inheritance in C#

**Task**: Determine how to port Python's `Dict` inheritance for FastDictCache.

**Findings**:
- Python `FastDictCache(Dict[_K, _V])` inherits from dict and overrides `__missing__`
- C# `Dictionary<TKey, TValue>` is sealed - cannot inherit
- C# options for auto-populating dictionary behavior:
  1. Implement `IDictionary<TKey, TValue>` with internal Dictionary (composition)
  2. Use indexer with custom logic
  3. Inherit from non-sealed collection types

**Decision**: Use composition - implement the indexer to provide auto-population on access. This matches Python semantics while using idiomatic C# patterns.

**Alternatives Considered**:
- Inherit from `KeyedCollection<TKey, TValue>`: Wrong abstraction
- Implement `IDictionary<TKey, TValue>` fully: Over-engineering; only need indexer, ContainsKey, TryGetValue, Count

---

### 4. Memoization in C#

**Task**: Determine how to port Python's `@memoized` decorator to C#.

**Findings**:
- Python uses decorator pattern with `@memoized(maxsize)` syntax
- C# options:
  1. **Attribute + source generator**: Complex, requires Roslyn
  2. **Static factory methods**: `Memoize.Create(func, maxSize)` returns wrapped Func
  3. **Extension methods on delegates**: Less discoverable

**Decision**: Use static factory methods in `Memoization` class per api-mapping.md suggestion. Provide overloads for 1, 2, and 3 argument functions per FR-012.

**Alternatives Considered**:
- Attribute approach: Requires source generators, too complex for this use case
- Extension methods: Less discoverable, clutters delegate namespace

---

### 5. Cache Key Construction for Memoization

**Task**: Determine how to create cache keys from function arguments in C#.

**Findings**:
- Python uses `(a, tuple(sorted(kw.items())))` as cache key
- C# Func delegates don't have named parameters at runtime
- For positional arguments only:
  - Single arg: Use arg directly as key
  - Multiple args: Use `ValueTuple` as key (provides GetHashCode/Equals)

**Decision**: Use `ValueTuple` for multi-argument keys. ValueTuples provide structural equality by default, making them ideal cache keys.

**Alternatives Considered**:
- Anonymous types: Cannot be used as generic type parameters
- Custom key records: Over-engineering; ValueTuple is simpler and efficient

---

## Summary

All research items resolved. No NEEDS CLARIFICATION items remain. The implementation approach:

| Component | C# Implementation |
|-----------|-------------------|
| SimpleCache | `Dictionary<TKey, TValue>` + `Queue<TKey>` for FIFO tracking + `Lock` for thread safety |
| FastDictCache | Composition with `Dictionary<TKey, TValue>` + `Queue<TKey>`, custom indexer + `Lock` for thread safety |
| Memoization | Static class with `Memoize<T1, TResult>()` overloads using ValueTuple keys (thread-safe via SimpleCache) |
| Key tracking | `Queue<TKey>` (C# equivalent of Python deque) |
| Thread safety | `System.Threading.Lock` with `EnterScope()` pattern (per Constitution XI) |
