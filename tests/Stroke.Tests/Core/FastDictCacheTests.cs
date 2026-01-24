// Copyright (c) 2024 Stroke Contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for the <see cref="FastDictCache{TKey, TValue}"/> class.
/// </summary>
public sealed class FastDictCacheTests
{
    #region T019: Constructor Tests

    [Fact]
    public void Constructor_DefaultSize_IsOneMillion()
    {
        // Arrange & Act
        var cache = new FastDictCache<string, int>(key => key.Length);

        // Assert
        Assert.Equal(1_000_000, cache.Size);
    }

    [Fact]
    public void Constructor_CustomSize_IsRespected()
    {
        // Arrange & Act
        var cache = new FastDictCache<string, int>(key => key.Length, size: 500);

        // Assert
        Assert.Equal(500, cache.Size);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void Constructor_SizeZeroOrNegative_ThrowsArgumentOutOfRangeException(int invalidSize)
    {
        // Arrange, Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => new FastDictCache<string, int>(key => key.Length, size: invalidSize));
        Assert.Equal("size", ex.ParamName);
    }

    [Fact]
    public void Constructor_NullGetValue_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(
            () => new FastDictCache<string, int>(null!));
        Assert.Equal("getValue", ex.ParamName);
    }

    #endregion

    #region T020: Indexer Tests

    [Fact]
    public void Indexer_MissingKey_InvokesFactory()
    {
        // Arrange
        int factoryCallCount = 0;
        var cache = new FastDictCache<string, int>(key =>
        {
            factoryCallCount++;
            return key.Length * 10;
        });

        // Act
        var result = cache["hello"];

        // Assert
        Assert.Equal(50, result);
        Assert.Equal(1, factoryCallCount);
    }

    [Fact]
    public void Indexer_ExistingKey_ReturnsCachedValue()
    {
        // Arrange
        int factoryCallCount = 0;
        var cache = new FastDictCache<string, int>(key =>
        {
            factoryCallCount++;
            return key.Length * 10;
        });

        // First access creates the entry
        _ = cache["hello"];

        // Act - second access should use cache
        var result = cache["hello"];

        // Assert
        Assert.Equal(50, result);
        Assert.Equal(1, factoryCallCount); // Factory not called again
    }

    [Fact]
    public void Indexer_FactoryReceivesCorrectKey()
    {
        // Arrange
        string? receivedKey = null;
        var cache = new FastDictCache<string, int>(key =>
        {
            receivedKey = key;
            return 42;
        });

        // Act
        _ = cache["test-key"];

        // Assert
        Assert.Equal("test-key", receivedKey);
    }

    [Fact]
    public void Indexer_DifferentKeys_InvokesFactoryForEach()
    {
        // Arrange
        int factoryCallCount = 0;
        var cache = new FastDictCache<int, int>(key =>
        {
            factoryCallCount++;
            return key * 2;
        });

        // Act
        _ = cache[1];
        _ = cache[2];
        _ = cache[3];

        // Assert
        Assert.Equal(3, factoryCallCount);
    }

    #endregion

    #region T021: FIFO Eviction Tests

    [Fact]
    public void Indexer_ExceedsSize_EvictsOldestEntry()
    {
        // Arrange - Python uses > not >= for eviction, so with size=3 we can hold 4 items
        // before eviction kicks in
        var cache = new FastDictCache<int, int>(key => key * 10, size: 3);
        _ = cache[1]; // first - count=1
        _ = cache[2]; // second - count=2
        _ = cache[3]; // third - count=3
        _ = cache[4]; // fourth - count=4, 4 > 3 is false at check time, so no eviction

        // At this point we have 4 items (count can exceed size by 1)
        // Adding fifth should evict "1"
        _ = cache[5];

        // Assert - key 1 should be evicted (oldest)
        Assert.False(cache.ContainsKey(1));
        Assert.True(cache.ContainsKey(2));
        Assert.True(cache.ContainsKey(3));
        Assert.True(cache.ContainsKey(4));
        Assert.True(cache.ContainsKey(5));
    }

    [Fact]
    public void Indexer_EvictionOrder_IsFIFO()
    {
        // Arrange - Python uses > not >= for eviction
        // With size=2, cache can hold 3 items before eviction
        var cache = new FastDictCache<int, int>(key => key * 10, size: 2);
        _ = cache[1]; // first - count=1
        _ = cache[2]; // second - count=2
        _ = cache[3]; // third - count=3, 3 > 2 is false at check time

        // Act - add fourth, now 3 > 2 is true, evict 1 (oldest)
        _ = cache[4];

        // Assert - 1 should be evicted, 2, 3, 4 should remain
        Assert.False(cache.ContainsKey(1));
        Assert.True(cache.ContainsKey(2));
        Assert.True(cache.ContainsKey(3));
        Assert.True(cache.ContainsKey(4));
    }

    [Fact]
    public void Indexer_EvictsBeforeAddingNewEntry()
    {
        // Python's FastDictCache uses > not >= for eviction check
        // This means we can hold size+1 items before eviction kicks in

        // Arrange - small cache with size=2
        var cache = new FastDictCache<int, int>(key => key, size: 2);
        _ = cache[1]; // count=1
        _ = cache[2]; // count=2

        Assert.Equal(2, cache.Count);

        // Act - add third (count becomes 3, which is > 2, but check happens BEFORE add)
        // At check time count=2, 2 > 2 is false, so no eviction
        _ = cache[3];

        // Count is now 3 (can exceed size by 1 per Python semantics)
        Assert.Equal(3, cache.Count);

        // Act - add fourth (count=3, 3 > 2 is true at check time, so evict first)
        _ = cache[4];

        // Count should be back to 3 (evicted one, added one)
        Assert.Equal(3, cache.Count);
    }

    #endregion

    #region T022: ContainsKey Tests

    [Fact]
    public void ContainsKey_CachedKey_ReturnsTrue()
    {
        // Arrange
        var cache = new FastDictCache<string, int>(key => 42);
        _ = cache["exists"];

        // Act & Assert
        Assert.True(cache.ContainsKey("exists"));
    }

    [Fact]
    public void ContainsKey_MissingKey_ReturnsFalse()
    {
        // Arrange
        var cache = new FastDictCache<string, int>(key => 42);

        // Act & Assert
        Assert.False(cache.ContainsKey("missing"));
    }

    [Fact]
    public void ContainsKey_DoesNotInvokeFactory()
    {
        // Arrange
        int factoryCallCount = 0;
        var cache = new FastDictCache<string, int>(key =>
        {
            factoryCallCount++;
            return 42;
        });

        // Act
        _ = cache.ContainsKey("key");

        // Assert - factory should NOT be called
        Assert.Equal(0, factoryCallCount);
    }

    #endregion

    #region T023: TryGetValue Tests

    [Fact]
    public void TryGetValue_CachedKey_ReturnsTrueAndValue()
    {
        // Arrange
        var cache = new FastDictCache<string, int>(key => key.Length * 10);
        _ = cache["hello"]; // Cache the value

        // Act
        var found = cache.TryGetValue("hello", out var value);

        // Assert
        Assert.True(found);
        Assert.Equal(50, value);
    }

    [Fact]
    public void TryGetValue_MissingKey_ReturnsFalse()
    {
        // Arrange
        var cache = new FastDictCache<string, int>(key => 42);

        // Act
        var found = cache.TryGetValue("missing", out var value);

        // Assert
        Assert.False(found);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGetValue_DoesNotInvokeFactory()
    {
        // Arrange
        int factoryCallCount = 0;
        var cache = new FastDictCache<string, int>(key =>
        {
            factoryCallCount++;
            return 42;
        });

        // Act
        _ = cache.TryGetValue("key", out _);

        // Assert - factory should NOT be called
        Assert.Equal(0, factoryCallCount);
    }

    #endregion

    #region T024: Property Tests

    [Fact]
    public void Size_ReturnsConfiguredMaximum()
    {
        // Arrange & Act
        var cache = new FastDictCache<int, int>(key => key, size: 123);

        // Assert
        Assert.Equal(123, cache.Size);
    }

    [Fact]
    public void Count_InitiallyZero()
    {
        // Arrange & Act
        var cache = new FastDictCache<int, int>(key => key);

        // Assert
        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public void Count_ReflectsActualEntryCount()
    {
        // Arrange
        var cache = new FastDictCache<int, int>(key => key, size: 100);

        // Act & Assert
        Assert.Equal(0, cache.Count);
        _ = cache[1];
        Assert.Equal(1, cache.Count);
        _ = cache[2];
        Assert.Equal(2, cache.Count);
        _ = cache[3];
        Assert.Equal(3, cache.Count);
    }

    [Fact]
    public void Count_DoesNotExceedSizePlusOne()
    {
        // Python uses > not >= for eviction, so count can be size+1
        // Arrange
        var cache = new FastDictCache<int, int>(key => key, size: 3);

        // Act - add more than size
        for (int i = 0; i < 10; i++)
        {
            _ = cache[i];
        }

        // Assert - count should not exceed size+1 per Python semantics
        Assert.True(cache.Count <= 4);
    }

    #endregion

    #region T025: Edge Case Tests

    [Fact]
    public void Indexer_SizeOne_SingleEntryCache()
    {
        // Arrange - Python uses > not >= so with size=1 we can hold 2 items
        var cache = new FastDictCache<int, int>(key => key * 10, size: 1);

        // Act
        _ = cache[1]; // count=1
        _ = cache[2]; // count=2, 1 > 1 is false at check time

        // Both keys are still present (size+1 allowed)
        Assert.True(cache.ContainsKey(1));
        Assert.True(cache.ContainsKey(2));
        Assert.Equal(2, cache.Count);

        // Add third - now 2 > 1 is true, evict key 1
        _ = cache[3];

        // Assert - key 1 should now be evicted
        Assert.False(cache.ContainsKey(1));
        Assert.True(cache.ContainsKey(2));
        Assert.True(cache.ContainsKey(3));
        Assert.Equal(2, cache.Count);
    }

    [Fact]
    public void Indexer_NullValueFromFactory_IsCached()
    {
        // Arrange
        int factoryCallCount = 0;
        var cache = new FastDictCache<string, string?>(key =>
        {
            factoryCallCount++;
            return null;
        });

        // Act - get null value twice
        var result1 = cache["key"];
        var result2 = cache["key"];

        // Assert
        Assert.Null(result1);
        Assert.Null(result2);
        Assert.Equal(1, factoryCallCount); // Null was cached
    }

    [Fact]
    public void Indexer_FactoryThrowsException_PropagatesException()
    {
        // Arrange
        var cache = new FastDictCache<string, int>(key =>
            throw new InvalidOperationException("Test exception"));

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => cache["key"]);
        Assert.Equal("Test exception", ex.Message);
    }

    [Fact]
    public void Indexer_FactoryThrowsException_CacheStateUnchanged()
    {
        // Arrange
        bool shouldThrow = true;
        var cache = new FastDictCache<string, int>(key =>
        {
            if (shouldThrow)
                throw new InvalidOperationException();
            return 42;
        }, size: 10);

        // Act - try to cache with throwing factory
        try
        {
            _ = cache["key"];
        }
        catch
        {
            // Expected
        }

        // Assert - key should not be cached
        Assert.False(cache.ContainsKey("key"));
        Assert.Equal(0, cache.Count);
    }

    #endregion

    #region T026: Concurrent Stress Tests

    [Fact]
    public async Task Indexer_ConcurrentAccess_NoExceptionsOrCorruption()
    {
        // Arrange
        var cache = new FastDictCache<int, int>(key => key * 2, size: 100);
        const int threadCount = 20;
        const int operationsPerThread = 500;
        var exceptions = new List<Exception>();
        var barrier = new Barrier(threadCount);

        // Act - run concurrent accesses
        var tasks = Enumerable.Range(0, threadCount).Select(threadId => Task.Run(() =>
        {
            try
            {
                barrier.SignalAndWait();
                for (int i = 0; i < operationsPerThread; i++)
                {
                    int key = (threadId * 100 + i) % 200;
                    var value = cache[key];
                    if (value != key * 2)
                    {
                        throw new Exception($"Unexpected value: expected {key * 2}, got {value}");
                    }
                }
            }
            catch (Exception ex)
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                }
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Empty(exceptions);
    }

    [Fact]
    public async Task Indexer_ConcurrentAccessWithEviction_NoExceptionsOrCorruption()
    {
        // Arrange - small cache to force frequent evictions
        var cache = new FastDictCache<int, int>(key => key * 3, size: 10);
        const int threadCount = 15;
        const int operationsPerThread = 200;
        var exceptions = new List<Exception>();
        var barrier = new Barrier(threadCount);

        // Act - run concurrent accesses that will cause many evictions
        var tasks = Enumerable.Range(0, threadCount).Select(threadId => Task.Run(() =>
        {
            try
            {
                barrier.SignalAndWait();
                var random = new Random(threadId);
                for (int i = 0; i < operationsPerThread; i++)
                {
                    int key = random.Next(1000);
                    var value = cache[key];
                    if (value != key * 3)
                    {
                        throw new Exception($"Unexpected value for key {key}: expected {key * 3}, got {value}");
                    }
                }
            }
            catch (Exception ex)
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                }
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Empty(exceptions);
    }

    [Fact]
    public async Task ContainsKeyAndTryGetValue_ConcurrentWithIndexer_NoExceptionsOrCorruption()
    {
        // Arrange
        var cache = new FastDictCache<int, int>(key => key, size: 50);
        const int threadCount = 10;
        const int operationsPerThread = 300;
        var exceptions = new List<Exception>();
        var barrier = new Barrier(threadCount * 2);

        // Act - run concurrent indexer accesses and lookups
        var indexerTasks = Enumerable.Range(0, threadCount).Select(threadId => Task.Run(() =>
        {
            try
            {
                barrier.SignalAndWait();
                for (int i = 0; i < operationsPerThread; i++)
                {
                    int key = i % 100;
                    var value = cache[key];
                    if (value != key)
                    {
                        throw new Exception($"Value mismatch: expected {key}, got {value}");
                    }
                }
            }
            catch (Exception ex)
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                }
            }
        })).ToArray();

        var lookupTasks = Enumerable.Range(0, threadCount).Select(_ => Task.Run(() =>
        {
            try
            {
                barrier.SignalAndWait();
                for (int i = 0; i < operationsPerThread; i++)
                {
                    int key = i % 100;
                    cache.ContainsKey(key);
                    cache.TryGetValue(key, out _);
                }
            }
            catch (Exception ex)
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                }
            }
        })).ToArray();

        await Task.WhenAll(indexerTasks.Concat(lookupTasks).ToArray());

        // Assert
        Assert.Empty(exceptions);
    }

    #endregion
}
