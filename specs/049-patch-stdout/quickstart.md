# Quickstart: Patch Stdout

**Feature**: 049-patch-stdout

## Basic Usage — Patch Console Output

The most common use case: prevent `Console.Write`/`Console.WriteLine` from corrupting an active prompt.

```csharp
using Stroke.Application;

// Wrap your application run in PatchStdout
using (StdoutPatching.PatchStdout())
{
    // Background tasks can safely write to Console
    _ = Task.Run(async () =>
    {
        while (true)
        {
            await Task.Delay(1000);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Background tick");
        }
    });

    // Run your Stroke application — output appears above the prompt
    await app.RunAsync();
}
// Console.Out and Console.Error are automatically restored
```

## Standalone StdoutProxy

Use `StdoutProxy` directly for custom logging integration:

```csharp
using Stroke.Application;

using var proxy = new StdoutProxy(sleepBetweenWrites: TimeSpan.FromMilliseconds(100));

// Use as a TextWriter target for a logging handler
proxy.Write("Processing item 1...");
proxy.Write(" done.\n");  // Flushes on newline

proxy.Flush();  // Force flush even without newline
```

## Raw VT100 Mode

Pass pre-formatted ANSI output through without escaping:

```csharp
using (StdoutPatching.PatchStdout(raw: true))
{
    // ANSI escape sequences pass through unmodified
    Console.Write("\x1b[31mRed text\x1b[0m from a library\n");

    await app.RunAsync();
}
```

## Verification

After implementing, verify:

1. `Console.WriteLine("test")` during an active prompt renders above the prompt
2. Rapid writes (10+ in 50ms) are batched into ≤2 terminal repaints
3. `Console.Out` and `Console.Error` are fully restored after `using` block exits
4. Multi-threaded writes produce no corruption or deadlocks
