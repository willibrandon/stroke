using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for the <see cref="SimpleCache{TKey, TValue}"/> class.
/// </summary>
public sealed class SimpleCacheTests
{
    #region T005: Constructor Tests

    [Fact]
    public void Constructor_DefaultMaxSize_IsEight()
    {
        // Arrange & Act
        var cache = new SimpleCache<string, int>();

        // Assert
        Assert.Equal(8, cache.MaxSize);
    }

    [Fact]
    public void Constructor_CustomMaxSize_IsRespected()
    {
        // Arrange & Act
        var cache = new SimpleCache<string, int>(maxSize: 100);

        // Assert
        Assert.Equal(100, cache.MaxSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void Constructor_MaxSizeZeroOrNegative_ThrowsArgumentOutOfRangeException(int invalidMaxSize)
    {
        // Arrange, Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new SimpleCache<string, int>(maxSize: invalidMaxSize));
        Assert.Equal("maxSize", ex.ParamName);
    }

    #endregion

    #region T006: Get() Tests

    [Fact]
    public void Get_MissingKey_InvokesGetter()
    {
        // Arrange
        var cache = new SimpleCache<string, int>();
        int getterCallCount = 0;

        // Act
        var result = cache.Get("key1", () =>
        {
            getterCallCount++;
            return 42;
        });

        // Assert
        Assert.Equal(42, result);
        Assert.Equal(1, getterCallCount);
    }

    [Fact]
    public void Get_ExistingKey_ReturnsCachedValue()
    {
        // Arrange
        var cache = new SimpleCache<string, int>();
        int getterCallCount = 0;
        cache.Get("key1", () =>
        {
            getterCallCount++;
            return 42;
        });

        // Act
        var result = cache.Get("key1", () =>
        {
            getterCallCount++;
            return 999; // Different value - should not be returned
        });

        // Assert
        Assert.Equal(42, result);
        Assert.Equal(1, getterCallCount); // Getter should not have been called again
    }

    [Fact]
    public void Get_NullGetter_ThrowsArgumentNullException()
    {
        // Arrange
        var cache = new SimpleCache<string, int>();

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => cache.Get("key1", null!));
        Assert.Equal("getter", ex.ParamName);
    }

    [Fact]
    public void Get_DifferentKeys_InvokesGetterForEach()
    {
        // Arrange
        var cache = new SimpleCache<string, int>();
        int getterCallCount = 0;

        // Act
        cache.Get("key1", () => { getterCallCount++; return 1; });
        cache.Get("key2", () => { getterCallCount++; return 2; });
        cache.Get("key3", () => { getterCallCount++; return 3; });

        // Assert
        Assert.Equal(3, getterCallCount);
    }

    #endregion

    #region T007: FIFO Eviction Tests

    [Fact]
    public void Get_ExceedsMaxSize_EvictsOldestEntry()
    {
        // Arrange
        var cache = new SimpleCache<string, string>(maxSize: 3);
        cache.Get("first", () => "value1");
        cache.Get("second", () => "value2");
        cache.Get("third", () => "value3");

        // Act - add fourth entry, should evict "first"
        cache.Get("fourth", () => "value4");

        // Assert - "first" should be evicted, getter should be called again
        int getterCallCount = 0;
        var result = cache.Get("first", () =>
        {
            getterCallCount++;
            return "new_value1";
        });

        Assert.Equal("new_value1", result);
        Assert.Equal(1, getterCallCount); // Getter called because "first" was evicted
    }

    [Fact]
    public void Get_EvictionOrder_IsFIFO()
    {
        // Arrange
        var cache = new SimpleCache<int, int>(maxSize: 2);

        // Add two entries
        cache.Get(1, () => 100);
        cache.Get(2, () => 200);

        // Act - add third, should evict key 1 (oldest)
        cache.Get(3, () => 300);

        // Verify key 2 is still cached (getter won't be called)
        // Check key 2 FIRST to avoid re-adding key 1 which would evict key 2
        int callCount = 0;
        cache.Get(2, () => { callCount++; return 201; });
        Assert.Equal(0, callCount);

        // Verify key 3 is still cached
        callCount = 0;
        cache.Get(3, () => { callCount++; return 301; });
        Assert.Equal(0, callCount);

        // Verify key 1 was evicted (getter will be called)
        callCount = 0;
        cache.Get(1, () => { callCount++; return 101; });
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Get_EvictionTriggersWhenCountExceedsMaxSize()
    {
        // Arrange - Python uses > not >= for eviction check
        var cache = new SimpleCache<int, int>(maxSize: 3);

        // Add exactly maxSize entries
        cache.Get(1, () => 1);
        cache.Get(2, () => 2);
        cache.Get(3, () => 3);

        // Verify all three are still cached
        int callCount = 0;
        cache.Get(1, () => { callCount++; return 99; });
        cache.Get(2, () => { callCount++; return 99; });
        cache.Get(3, () => { callCount++; return 99; });
        Assert.Equal(0, callCount); // No getters called - all cached

        // Add fourth entry - now eviction should occur
        cache.Get(4, () => 4);

        // Key 1 (oldest) should be evicted
        callCount = 0;
        cache.Get(1, () => { callCount++; return 99; });
        Assert.Equal(1, callCount);
    }

    #endregion

    #region T008: Clear() Tests

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        // Arrange
        var cache = new SimpleCache<string, int>(maxSize: 10);
        cache.Get("key1", () => 1);
        cache.Get("key2", () => 2);
        cache.Get("key3", () => 3);

        // Act
        cache.Clear();

        // Assert - all keys should be evicted, getters should be called again
        int getterCallCount = 0;
        cache.Get("key1", () => { getterCallCount++; return 10; });
        cache.Get("key2", () => { getterCallCount++; return 20; });
        cache.Get("key3", () => { getterCallCount++; return 30; });

        Assert.Equal(3, getterCallCount);
    }

    [Fact]
    public void Clear_CacheCanBeReusedAfterClear()
    {
        // Arrange
        var cache = new SimpleCache<string, int>(maxSize: 10);
        cache.Get("key1", () => 1);
        cache.Clear();

        // Act
        var result = cache.Get("key1", () => 100);

        // Assert
        Assert.Equal(100, result);

        // And it should be cached now
        int callCount = 0;
        var result2 = cache.Get("key1", () => { callCount++; return 200; });
        Assert.Equal(100, result2);
        Assert.Equal(0, callCount);
    }

    [Fact]
    public void Clear_EmptyCache_DoesNotThrow()
    {
        // Arrange
        var cache = new SimpleCache<string, int>();

        // Act & Assert - should not throw
        var ex = Record.Exception(() => cache.Clear());
        Assert.Null(ex);
    }

    #endregion

    #region T009: Edge Case Tests

    [Fact]
    public void Get_MaxSizeOne_SingleEntryCache()
    {
        // Arrange
        var cache = new SimpleCache<string, int>(maxSize: 1);

        // Act
        cache.Get("first", () => 1);
        cache.Get("second", () => 2);

        // Assert - "second" should be the only entry in cache (first was evicted)
        int callCount = 0;
        cache.Get("second", () => { callCount++; return 20; });
        Assert.Equal(0, callCount); // Still cached

        // "first" should have been evicted
        callCount = 0;
        cache.Get("first", () => { callCount++; return 10; });
        Assert.Equal(1, callCount); // Had to re-compute
    }

    [Fact]
    public void Get_NullValue_IsCached()
    {
        // Arrange
        var cache = new SimpleCache<string, string?>();
        int getterCallCount = 0;

        // Act - cache a null value
        var result1 = cache.Get("key1", () =>
        {
            getterCallCount++;
            return null;
        });

        // Get it again - should return cached null
        var result2 = cache.Get("key1", () =>
        {
            getterCallCount++;
            return "not null";
        });

        // Assert
        Assert.Null(result1);
        Assert.Null(result2);
        Assert.Equal(1, getterCallCount); // Getter only called once
    }

    [Fact]
    public void Get_GetterThrowsException_PropagatesException()
    {
        // Arrange
        var cache = new SimpleCache<string, int>();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            cache.Get("key1", () => throw new InvalidOperationException("Test exception")));

        Assert.Equal("Test exception", ex.Message);
    }

    [Fact]
    public void Get_GetterThrowsException_CacheStateUnchanged()
    {
        // Arrange
        var cache = new SimpleCache<string, int>();

        // Act - try to cache with throwing getter
        try
        {
            cache.Get("key1", () => throw new InvalidOperationException());
        }
        catch
        {
            // Expected
        }

        // Assert - key should not be cached
        int callCount = 0;
        cache.Get("key1", () =>
        {
            callCount++;
            return 42;
        });
        Assert.Equal(1, callCount); // Getter was called because key wasn't cached
    }

    [Fact]
    public void Get_SameKeyMultipleTimes_ReturnsOriginalValue()
    {
        // Arrange
        var cache = new SimpleCache<string, int>();
        int callCount = 0;

        // Act - get same key multiple times
        var results = new List<int>();
        for (int i = 0; i < 10; i++)
        {
            results.Add(cache.Get("key", () =>
            {
                callCount++;
                return 42;
            }));
        }

        // Assert
        Assert.All(results, r => Assert.Equal(42, r));
        Assert.Equal(1, callCount);
    }

    #endregion

    #region T010: Concurrent Stress Tests

    [Fact]
    public async Task Get_ConcurrentAccess_NoExceptionsOrCorruption()
    {
        // Arrange
        var cache = new SimpleCache<int, int>(maxSize: 100);
        const int threadCount = 20;
        const int operationsPerThread = 500;
        var exceptions = new List<Exception>();
        var barrier = new Barrier(threadCount);

        // Act - run concurrent gets
        var tasks = Enumerable.Range(0, threadCount).Select(threadId => Task.Run(() =>
        {
            try
            {
                barrier.SignalAndWait(); // Synchronize thread start
                for (int i = 0; i < operationsPerThread; i++)
                {
                    // Each thread accesses a mix of shared and unique keys
                    int key = (threadId * 100 + i) % 200;
                    var value = cache.Get(key, () => key * 2);
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
    public async Task Get_ConcurrentAccessWithEviction_NoExceptionsOrCorruption()
    {
        // Arrange - small cache to force frequent evictions
        var cache = new SimpleCache<int, int>(maxSize: 10);
        const int threadCount = 15;
        const int operationsPerThread = 200;
        var exceptions = new List<Exception>();
        var barrier = new Barrier(threadCount);

        // Act - run concurrent gets that will cause many evictions
        var tasks = Enumerable.Range(0, threadCount).Select(threadId => Task.Run(() =>
        {
            try
            {
                barrier.SignalAndWait();
                var random = new Random(threadId);
                for (int i = 0; i < operationsPerThread; i++)
                {
                    int key = random.Next(1000); // Wide key range forces evictions
                    var value = cache.Get(key, () => key * 3);
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
    public async Task Clear_ConcurrentWithGet_NoExceptionsOrCorruption()
    {
        // Arrange
        var cache = new SimpleCache<int, int>(maxSize: 50);
        const int threadCount = 10;
        const int operationsPerThread = 300;
        var exceptions = new List<Exception>();
        var barrier = new Barrier(threadCount + 2); // +2 for clear threads

        // Act - run concurrent gets and clears
        var getTasks = Enumerable.Range(0, threadCount).Select(threadId => Task.Run(() =>
        {
            try
            {
                barrier.SignalAndWait();
                for (int i = 0; i < operationsPerThread; i++)
                {
                    int key = i % 100;
                    var value = cache.Get(key, () => key);
                    // Value might be from current or previous cache state - just verify no corruption
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

        var clearTasks = Enumerable.Range(0, 2).Select(_ => Task.Run(() =>
        {
            try
            {
                barrier.SignalAndWait();
                for (int i = 0; i < 50; i++)
                {
                    cache.Clear();
                    Thread.Sleep(1); // Small delay between clears
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

        await Task.WhenAll(getTasks.Concat(clearTasks).ToArray());

        // Assert
        Assert.Empty(exceptions);
    }

    #endregion
}
