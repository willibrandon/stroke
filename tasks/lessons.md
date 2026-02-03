# Lessons Learned

## 2026-02-03: Key Binding Priority Sort Order Bug

**Problem**: Enter key wasn't accepting input - SelfInsert (wildcard binding) was called instead of the specific Enter handler.

**Root Cause**: In `KeyBindings.GetBindingsForKeys()`, bindings were sorted by `AnyCount` in **ascending** order:
```csharp
matches.Sort((a, b) => a.AnyCount.CompareTo(b.AnyCount));  // WRONG
```

But `KeyProcessor.ProcessSingleKey()` calls `matches[^1]` (the **last** element). This meant wildcard bindings (higher AnyCount) ended up last and "won" the selection.

Python uses `sorted(result, key=lambda item: -item[0])` which sorts **descending** by AnyCount, so the last element has the **fewest** wildcards (most specific match).

**Fix**: Change sort to descending order:
```csharp
matches.Sort((a, b) => b.AnyCount.CompareTo(a.AnyCount));  // CORRECT
```

**Lesson**: When porting Python code that uses negative sort keys (e.g., `-item[0]`), remember that C# `CompareTo` returns ascending order by default. To match Python's descending sort, swap the comparison operands (`b.CompareTo(a)` instead of `a.CompareTo(b)`).

**Files Changed**: `src/Stroke/KeyBinding/KeyBindings.cs:237`

---

## 2026-02-03: C# Generic Invariance with `Unsafe.As`

**Problem**: `AppContext.GetApp()` returned `DummyApplication` even when a real `Application<string>` was set.

**Root Cause**: C# generics are invariant - `Application<string>` is NOT an `Application<object?>`. Pattern matching like `session.App is Application<object?> app` fails at runtime.

**Fix**: Check the generic type definition explicitly, then use `Unsafe.As` to reinterpret the reference:
```csharp
if (app is not null)
{
    var appType = app.GetType();
    if (appType.IsGenericType && appType.GetGenericTypeDefinition() == typeof(Application<>))
    {
        return System.Runtime.CompilerServices.Unsafe.As<Application<object?>>(app);
    }
}
```

**Lesson**: Generic invariance in C# prevents direct casting between `Foo<Derived>` and `Foo<Base>`. Use `GetGenericTypeDefinition()` to check if something is "any Foo<T>", then `Unsafe.As` if needed.

**Files Changed**: `src/Stroke/Application/AppContext.cs`
