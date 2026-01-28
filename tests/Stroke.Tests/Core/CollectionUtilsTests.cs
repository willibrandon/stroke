namespace Stroke.Tests.Core;

using Stroke.Core;
using Xunit;

/// <summary>
/// Tests for <see cref="CollectionUtils"/> class.
/// </summary>
public class CollectionUtilsTests
{
    #region Distribution Tests

    [Fact]
    public void TakeUsingWeights_WithWeights124_DistributesProportionally()
    {
        var items = new[] { "A", "B", "C" };
        var weights = new[] { 1, 2, 4 };

        // Take 700 items to get good distribution statistics
        var results = CollectionUtils.TakeUsingWeights(items, weights).Take(700).ToList();

        var aCount = results.Count(x => x == "A");
        var bCount = results.Count(x => x == "B");
        var cCount = results.Count(x => x == "C");

        // With weights [1, 2, 4], total weight = 7
        // Expected: A ~100, B ~200, C ~400
        // Allow 5% tolerance (±35 items)
        Assert.InRange(aCount, 65, 135);   // ~100 ± 35
        Assert.InRange(bCount, 165, 235);  // ~200 ± 35
        Assert.InRange(cCount, 365, 435);  // ~400 ± 35

        // Also verify we got exactly 700 items
        Assert.Equal(700, results.Count);
    }

    [Fact]
    public void TakeUsingWeights_SingleItemPositiveWeight_YieldsInfinitely()
    {
        var items = new[] { "Only" };
        var weights = new[] { 5 };

        var results = CollectionUtils.TakeUsingWeights(items, weights).Take(100).ToList();

        Assert.Equal(100, results.Count);
        Assert.All(results, item => Assert.Equal("Only", item));
    }

    [Fact]
    public void TakeUsingWeights_EqualWeights_RoundRobinDistribution()
    {
        var items = new[] { "A", "B", "C" };
        var weights = new[] { 1, 1, 1 };

        // With equal weights, each item should appear roughly equally
        var results = CollectionUtils.TakeUsingWeights(items, weights).Take(99).ToList();

        var aCount = results.Count(x => x == "A");
        var bCount = results.Count(x => x == "B");
        var cCount = results.Count(x => x == "C");

        // With equal weights and 99 items, expect 33 of each
        Assert.Equal(33, aCount);
        Assert.Equal(33, bCount);
        Assert.Equal(33, cCount);
    }

    #endregion

    #region Zero Weight Filtering Tests

    [Fact]
    public void TakeUsingWeights_ZeroWeightItems_FilteredOut()
    {
        var items = new[] { "A", "B", "C" };
        var weights = new[] { 1, 0, 2 };

        var results = CollectionUtils.TakeUsingWeights(items, weights).Take(30).ToList();

        // B should never appear (weight = 0)
        Assert.DoesNotContain("B", results);

        // Only A and C should appear
        Assert.Contains("A", results);
        Assert.Contains("C", results);
    }

    [Fact]
    public void TakeUsingWeights_NegativeWeights_TreatedAsZero()
    {
        var items = new[] { "A", "B", "C" };
        var weights = new[] { 1, -5, 2 };

        var results = CollectionUtils.TakeUsingWeights(items, weights).Take(30).ToList();

        // B should never appear (negative weight treated as 0)
        Assert.DoesNotContain("B", results);

        // Only A and C should appear
        Assert.Contains("A", results);
        Assert.Contains("C", results);
    }

    [Fact]
    public void TakeUsingWeights_MixedPositiveZeroNegative_OnlyPositiveUsed()
    {
        var items = new[] { "A", "B", "C", "D" };
        var weights = new[] { 2, 0, -1, 3 };

        var results = CollectionUtils.TakeUsingWeights(items, weights).Take(50).ToList();

        // Only A and D should appear
        Assert.Contains("A", results);
        Assert.Contains("D", results);
        Assert.DoesNotContain("B", results);
        Assert.DoesNotContain("C", results);
    }

    #endregion

    #region Validation Exception Tests

    [Fact]
    public void TakeUsingWeights_EmptyItemsList_ThrowsArgumentException()
    {
        var items = Array.Empty<string>();
        var weights = Array.Empty<int>();

        // Should throw when trying to enumerate (lazy validation)
        Assert.Throws<ArgumentException>(() =>
            CollectionUtils.TakeUsingWeights(items, weights).First());
    }

    [Fact]
    public void TakeUsingWeights_NullItems_ThrowsArgumentNullException()
    {
        IReadOnlyList<string>? items = null;
        var weights = new[] { 1 };

        Assert.Throws<ArgumentNullException>(() =>
            CollectionUtils.TakeUsingWeights(items!, weights));
    }

    [Fact]
    public void TakeUsingWeights_NullWeights_ThrowsArgumentNullException()
    {
        var items = new[] { "A" };
        IReadOnlyList<int>? weights = null;

        Assert.Throws<ArgumentNullException>(() =>
            CollectionUtils.TakeUsingWeights(items, weights!));
    }

    [Fact]
    public void TakeUsingWeights_MismatchedLengths_ThrowsArgumentException()
    {
        var items = new[] { "A", "B", "C" };
        var weights = new[] { 1, 2 };

        // Should throw when trying to enumerate (lazy validation)
        Assert.Throws<ArgumentException>(() =>
            CollectionUtils.TakeUsingWeights(items, weights).First());
    }

    [Fact]
    public void TakeUsingWeights_AllZeroWeights_ThrowsArgumentException()
    {
        var items = new[] { "A", "B", "C" };
        var weights = new[] { 0, 0, 0 };

        // Should throw when trying to enumerate (lazy validation)
        Assert.Throws<ArgumentException>(() =>
            CollectionUtils.TakeUsingWeights(items, weights).First());
    }

    [Fact]
    public void TakeUsingWeights_AllNegativeWeights_ThrowsArgumentException()
    {
        var items = new[] { "A", "B" };
        var weights = new[] { -1, -5 };

        // Negative weights are treated as zero, so this should throw
        Assert.Throws<ArgumentException>(() =>
            CollectionUtils.TakeUsingWeights(items, weights).First());
    }

    #endregion

    #region Infinite Sequence Tests

    [Fact]
    public void TakeUsingWeights_IsInfinite_CanTakeAnyNumber()
    {
        var items = new[] { "A", "B" };
        var weights = new[] { 1, 1 };

        // Should be able to take any number without throwing
        var result1000 = CollectionUtils.TakeUsingWeights(items, weights).Take(1000).ToList();
        Assert.Equal(1000, result1000.Count);

        var result10000 = CollectionUtils.TakeUsingWeights(items, weights).Take(10000).ToList();
        Assert.Equal(10000, result10000.Count);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public void TakeUsingWeights_ReturnsNewIterator_PerCall()
    {
        var items = new[] { "A", "B" };
        var weights = new[] { 1, 1 };

        // Each call should return a new independent enumerable
        var enum1 = CollectionUtils.TakeUsingWeights(items, weights);
        var enum2 = CollectionUtils.TakeUsingWeights(items, weights);

        // Taking from one should not affect the other
        var results1 = enum1.Take(5).ToList();
        var results2 = enum2.Take(5).ToList();

        Assert.Equal(5, results1.Count);
        Assert.Equal(5, results2.Count);
    }

    #endregion

    #region Generic Type Tests

    [Fact]
    public void TakeUsingWeights_WorksWithDifferentTypes()
    {
        // Test with integers
        var intItems = new[] { 1, 2, 3 };
        var intWeights = new[] { 1, 2, 3 };
        var intResults = CollectionUtils.TakeUsingWeights(intItems, intWeights).Take(10).ToList();
        Assert.Equal(10, intResults.Count);

        // Test with custom objects
        var objItems = new[] { new TestItem("A"), new TestItem("B") };
        var objWeights = new[] { 1, 2 };
        var objResults = CollectionUtils.TakeUsingWeights(objItems, objWeights).Take(10).ToList();
        Assert.Equal(10, objResults.Count);
    }

    #endregion

    #region Test Helpers

    private sealed class TestItem(string name)
    {
        public string Name { get; } = name;
    }

    #endregion
}
