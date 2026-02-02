# Contract: StdoutPatching

**Namespace**: `Stroke.Application`
**Python Source**: `prompt_toolkit.patch_stdout.patch_stdout`

## Class Definition

```csharp
/// <summary>
/// Static entry point for patching <see cref="Console.Out"/> and <see cref="Console.Error"/>
/// with a <see cref="StdoutProxy"/> that routes output above the current prompt.
/// </summary>
/// <remarks>
/// <para>
/// Port of Python Prompt Toolkit's <c>patch_stdout</c> context manager.
/// </para>
/// </remarks>
public static class StdoutPatching
```

## Methods

```csharp
/// <summary>
/// Replaces <see cref="Console.Out"/> and <see cref="Console.Error"/> with a
/// <see cref="StdoutProxy"/> instance. Returns an <see cref="IDisposable"/> that
/// restores the original streams when disposed.
/// </summary>
/// <param name="raw">
/// When <c>true</c>, VT100 escape sequences are passed through unmodified.
/// Default: <c>false</c>.
/// </param>
/// <returns>
/// An <see cref="IDisposable"/> that, when disposed, restores the original
/// <see cref="Console.Out"/> and <see cref="Console.Error"/> streams and
/// disposes the proxy.
/// </returns>
/// <example>
/// <code>
/// using (StdoutPatching.PatchStdout())
/// {
///     // All Console.Write/WriteLine calls are routed above the prompt
///     Console.WriteLine("This appears above the prompt!");
/// }
/// // Console.Out and Console.Error are restored
/// </code>
/// </example>
public static IDisposable PatchStdout(bool raw = false);
```

## Python Correspondence

| Python | C# | Notes |
|--------|-----|-------|
| `patch_stdout(raw=False)` | `StdoutPatching.PatchStdout(raw)` | Context manager → IDisposable |
| `@contextmanager` | `IDisposable` pattern | Python yields → C# using block |
| `sys.stdout = proxy` | `Console.SetOut(proxy)` | |
| `sys.stderr = proxy` | `Console.SetError(proxy)` | |
| `sys.stdout = original` | `Console.SetOut(original)` | On dispose |
| `sys.stderr = original` | `Console.SetError(original)` | On dispose |

## Internal: FlushItem (Sealed Record Hierarchy)

```csharp
/// <summary>
/// Discriminated union for items in the StdoutProxy flush queue.
/// </summary>
internal abstract record FlushItem
{
    /// <summary>
    /// Text to be written to the terminal output.
    /// </summary>
    internal sealed record Text(string Value) : FlushItem;

    /// <summary>
    /// Sentinel value signaling the flush thread to terminate.
    /// </summary>
    internal sealed record Done() : FlushItem;
}
```
