# Contract: CollectionUtils

**Namespace**: `Stroke.Core`
**File**: `src/Stroke/Core/CollectionUtils.cs`

## API Contract

```csharp
namespace Stroke.Core;

/// <summary>
/// Collection manipulation utilities.
/// </summary>
/// <remarks>
/// Port of Python Prompt Toolkit's collection-related functions from <c>utils.py</c>.
/// </remarks>
public static class CollectionUtils
{
    /// <summary>
    /// Generates an infinite sequence of items, yielding each item in proportion to its weight.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="items">The items to yield.</param>
    /// <param name="weights">
    /// The weight for each item. Must have the same length as <paramref name="items"/>.
    /// Items with weight 0 are filtered out.
    /// </param>
    /// <returns>
    /// An infinite enumerable that yields items proportionally. For example, with items
    /// ['A', 'B', 'C'] and weights [1, 2, 4], taking 70 items would yield approximately
    /// 10 A's, 20 B's, and 40 C's.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="items"/> or <paramref name="weights"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="items"/> and <paramref name="weights"/> have different lengths.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when no items have a positive weight.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The generator uses a fill-based algorithm where each iteration fills items proportionally
    /// based on their weight relative to the maximum weight. This ensures fair distribution
    /// over time.
    /// </para>
    /// <para>
    /// Port of Python Prompt Toolkit's <c>take_using_weights</c> function from <c>utils.py</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var items = new[] { "A", "B", "C" };
    /// var weights = new[] { 1, 2, 4 };
    ///
    /// // Take first 70 items - approximately 10 A's, 20 B's, 40 C's
    /// var results = CollectionUtils.TakeUsingWeights(items, weights).Take(70).ToList();
    ///
    /// // Distribution should be roughly proportional to weights
    /// var aCount = results.Count(x => x == "A"); // ~10
    /// var bCount = results.Count(x => x == "B"); // ~20
    /// var cCount = results.Count(x => x == "C"); // ~40
    /// </code>
    /// </example>
    public static IEnumerable<T> TakeUsingWeights<T>(IReadOnlyList<T> items, IReadOnlyList<int> weights);
}
```

## Functional Requirements Coverage

| Requirement | Method |
|-------------|--------|
| FR-021 | `TakeUsingWeights<T>()` |
| FR-022 | Zero-weight items filtered out |
| FR-023 | `ArgumentException` for mismatched lengths |
| FR-024 | `ArgumentException` for no positive weights |
| FR-029 | Negative weights treated as zero (filtered out) |

## Algorithm Description

The algorithm from Python Prompt Toolkit:

1. **Filter**: Remove items with weight ≤ 0
2. **Validate**: Ensure at least one item with positive weight
3. **Initialize**: Track `alreadyTaken[i]` for each item, starting at 0
4. **Loop forever**:
   - For each iteration `i` (starting at 0):
   - For each item with index `itemIndex`:
     - If `alreadyTaken[itemIndex] < i * weight[itemIndex] / maxWeight`:
       - Yield the item
       - Increment `alreadyTaken[itemIndex]`
   - Continue until no items were yielded in this sub-loop
   - Increment `i`

## Edge Cases

| Scenario | Behavior |
|----------|----------|
| items or weights is null | Throws `ArgumentNullException` |
| Different lengths | Throws `ArgumentException` |
| All weights are 0 | Throws `ArgumentException` |
| All weights are negative | Throws `ArgumentException` (treated as zero) |
| Some weights are 0 | Those items filtered out, others used |
| Some weights are negative | Treated as zero, filtered out |
| Single item | Yields that item infinitely |
| Equal weights | Round-robin distribution |
| Empty items list | Throws `ArgumentException` (no positive weights) |
| Mixed positive/zero/negative weights | Only items with weight > 0 used |

## Distribution Accuracy

Per SC-004 from spec: Distribution should be within 5% of expected proportions when taking 100+ items.

For items with weights [1, 2, 4] (total 7):
- Taking 700 items: expect ~100 A's, ~200 B's, ~400 C's (±5% = ±35 items)

