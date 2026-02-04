# Quickstart: Async Generator Utilities

**Feature**: 059-async-generator-utils
**Date**: 2026-02-03

## Overview

The `AsyncGeneratorUtils` class provides two utilities for working with async generators:

1. **`Aclosing<T>`** - Ensures async generators are properly cleaned up
2. **`GeneratorToAsyncGenerator<T>`** - Converts sync sequences to async with backpressure

---

## Installation

Add a reference to the Stroke library (these utilities are in `Stroke.EventLoop`):

```csharp
using Stroke.EventLoop;
```

---

## Basic Usage

### Aclosing - Safe Async Generator Cleanup

Use `Aclosing` to ensure an async generator is properly disposed, even when you break out of the loop early or an exception occurs:

```csharp
async Task ProcessWithCleanup()
{
    await using var wrapper = AsyncGeneratorUtils.Aclosing(GetCompletionsAsync());

    await foreach (var completion in wrapper.Value)
    {
        Console.WriteLine(completion);

        if (completion.StartsWith("done"))
            break; // Generator is properly disposed!
    }
}
```

**Why use Aclosing?**
- Without `Aclosing`, breaking out of `await foreach` may leave the generator in a partially consumed state
- `Aclosing` guarantees `DisposeAsync()` is called on the underlying enumerator
- Essential for generators that hold resources (file handles, network connections, etc.)

### GeneratorToAsyncGenerator - Sync to Async Conversion

Use `GeneratorToAsyncGenerator` to consume a synchronous sequence asynchronously without blocking:

```csharp
async Task ConsumeCompletionsAsync()
{
    // Convert sync completer to async
    var asyncCompletions = AsyncGeneratorUtils.GeneratorToAsyncGenerator(
        () => myCompleter.GetCompletions(document, position));

    await foreach (var completion in asyncCompletions)
    {
        // UI thread remains responsive while processing
        await DisplayCompletionAsync(completion);
    }
}
```

**Why use GeneratorToAsyncGenerator?**
- The sync producer runs in a background thread
- The async consumer can `await` between items without blocking
- Perfect for integrating sync completion providers with async UI

---

## Advanced Usage

### Custom Buffer Size

For large datasets, tune the buffer size for optimal throughput:

```csharp
// Larger buffer for high-throughput scenarios
var asyncItems = AsyncGeneratorUtils.GeneratorToAsyncGenerator(
    () => ReadLargeDataset(),
    bufferSize: 5000);

// Smaller buffer for memory-constrained scenarios
var asyncItems = AsyncGeneratorUtils.GeneratorToAsyncGenerator(
    () => GetItems(),
    bufferSize: 100);
```

**Buffer size guidelines**:
- Default (1000): Good balance for most use cases
- Larger: Higher throughput but more memory usage
- Smaller: Lower memory but may reduce throughput

### Combining Aclosing with GeneratorToAsyncGenerator

For full lifecycle control:

```csharp
async Task SafeAsyncConversion()
{
    var asyncSequence = AsyncGeneratorUtils.GeneratorToAsyncGenerator(
        () => GetSlowItems());

    await using var wrapper = AsyncGeneratorUtils.Aclosing(asyncSequence);

    await foreach (var item in wrapper.Value)
    {
        if (IsCancelled)
            break; // Both producer thread AND enumerator are cleaned up
    }
}
```

### Error Handling

Exceptions from the sync producer propagate to the async consumer:

```csharp
try
{
    var asyncItems = AsyncGeneratorUtils.GeneratorToAsyncGenerator(
        () => ThrowingEnumerable());

    await foreach (var item in asyncItems)
    {
        // This will receive the exception
    }
}
catch (InvalidOperationException ex)
{
    // Handle exception from producer
}
```

---

## Common Patterns

### Integration with Completion System

```csharp
public class AsyncCompleterWrapper : ICompleter
{
    private readonly ICompleter _syncCompleter;

    public IAsyncEnumerable<Completion> GetCompletionsAsync(
        Document document,
        CompleteEvent completeEvent)
    {
        return AsyncGeneratorUtils.GeneratorToAsyncGenerator(
            () => _syncCompleter.GetCompletions(document, completeEvent));
    }
}
```

### Cancellation via Disposal

```csharp
async Task WithTimeout(CancellationToken ct)
{
    var asyncItems = AsyncGeneratorUtils.GeneratorToAsyncGenerator(() => SlowItems());

    await using var wrapper = AsyncGeneratorUtils.Aclosing(asyncItems);

    await foreach (var item in wrapper.Value.WithCancellation(ct))
    {
        // Process item
    }
    // When ct is cancelled or loop exits, producer thread is stopped
}
```

---

## Performance Notes

1. **Background thread overhead**: `GeneratorToAsyncGenerator` spawns a thread per enumeration. Avoid for trivial sequences.

2. **Buffer sizing**: The default 1000-item buffer is optimized for completion scenarios (50k+ items). Adjust based on your item size and throughput needs.

3. **Cleanup timing**: Producer thread terminates within 2 seconds of disposal in the worst case (1 second timeout per blocking operation).

---

## API Reference

See [contracts/async-generator-utils.md](contracts/async-generator-utils.md) for full API documentation.
