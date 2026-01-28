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
    /// Items with weight ≤ 0 are filtered out.
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
    public static IEnumerable<T> TakeUsingWeights<T>(IReadOnlyList<T> items, IReadOnlyList<int> weights)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(weights);

        return TakeUsingWeightsIterator(items, weights);
    }

    private static IEnumerable<T> TakeUsingWeightsIterator<T>(IReadOnlyList<T> items, IReadOnlyList<int> weights)
    {
        if (items.Count != weights.Count)
        {
            throw new ArgumentException("Items and weights must have the same length.", nameof(weights));
        }

        // Filter to items with positive weights
        var filtered = new List<(T Item, int Weight)>();
        for (int i = 0; i < items.Count; i++)
        {
            if (weights[i] > 0)
            {
                filtered.Add((items[i], weights[i]));
            }
        }

        if (filtered.Count == 0)
        {
            throw new ArgumentException("At least one item must have a positive weight.", nameof(weights));
        }

        // Find max weight for normalization
        var maxWeight = filtered.Max(x => x.Weight);

        // Track how many times each item has been yielded
        var alreadyTaken = new int[filtered.Count];

        // Infinite loop
        var iteration = 0;
        while (true)
        {
            iteration++;

            // For each item, check if it should be yielded this iteration
            for (int itemIndex = 0; itemIndex < filtered.Count; itemIndex++)
            {
                // Calculate expected count: iteration * weight / maxWeight
                // Item should be yielded if alreadyTaken < expected
                var (item, weight) = filtered[itemIndex];
                var expected = (double)iteration * weight / maxWeight;

                while (alreadyTaken[itemIndex] < expected)
                {
                    alreadyTaken[itemIndex]++;
                    yield return item;
                }
            }
        }
    }
}
