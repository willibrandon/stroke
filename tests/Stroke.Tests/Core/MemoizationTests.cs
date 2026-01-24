// Copyright (c) 2024 Stroke Contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for the <see cref="Memoization"/> static class.
/// </summary>
public sealed class MemoizationTests
{
    #region T036: Validation Tests

    [Fact]
    public void Memoize_SingleArg_NullFunc_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(
            () => Memoization.Memoize<int, int>(null!));
        Assert.Equal("func", ex.ParamName);
    }

    [Fact]
    public void Memoize_TwoArg_NullFunc_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(
            () => Memoization.Memoize<int, int, int>(null!));
        Assert.Equal("func", ex.ParamName);
    }

    [Fact]
    public void Memoize_ThreeArg_NullFunc_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(
            () => Memoization.Memoize<int, int, int, int>(null!));
        Assert.Equal("func", ex.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Memoize_SingleArg_MaxSizeZeroOrNegative_ThrowsArgumentOutOfRangeException(int invalidMaxSize)
    {
        // Arrange, Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => Memoization.Memoize<int, int>(x => x, maxSize: invalidMaxSize));
        Assert.Equal("maxSize", ex.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Memoize_TwoArg_MaxSizeZeroOrNegative_ThrowsArgumentOutOfRangeException(int invalidMaxSize)
    {
        // Arrange, Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => Memoization.Memoize<int, int, int>((a, b) => a + b, maxSize: invalidMaxSize));
        Assert.Equal("maxSize", ex.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Memoize_ThreeArg_MaxSizeZeroOrNegative_ThrowsArgumentOutOfRangeException(int invalidMaxSize)
    {
        // Arrange, Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => Memoization.Memoize<int, int, int, int>((a, b, c) => a + b + c, maxSize: invalidMaxSize));
        Assert.Equal("maxSize", ex.ParamName);
    }

    #endregion

    #region T037: Single-Arg Memoize Tests

    [Fact]
    public void Memoize_SingleArg_FirstCall_ExecutesFunction()
    {
        // Arrange
        int callCount = 0;
        var memoized = Memoization.Memoize<int, int>(x =>
        {
            callCount++;
            return x * 2;
        });

        // Act
        var result = memoized(5);

        // Assert
        Assert.Equal(10, result);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Memoize_SingleArg_RepeatCall_ReturnsCachedValue()
    {
        // Arrange
        int callCount = 0;
        var memoized = Memoization.Memoize<int, int>(x =>
        {
            callCount++;
            return x * 2;
        });

        // Act
        memoized(5); // First call
        var result = memoized(5); // Repeat call

        // Assert
        Assert.Equal(10, result);
        Assert.Equal(1, callCount); // Function only called once
    }

    [Fact]
    public void Memoize_SingleArg_DifferentArgs_EachCachedSeparately()
    {
        // Arrange
        int callCount = 0;
        var memoized = Memoization.Memoize<int, int>(x =>
        {
            callCount++;
            return x * 2;
        });

        // Act
        var result1 = memoized(1);
        var result2 = memoized(2);
        var result3 = memoized(3);

        // Assert
        Assert.Equal(2, result1);
        Assert.Equal(4, result2);
        Assert.Equal(6, result3);
        Assert.Equal(3, callCount);

        // Verify caching
        callCount = 0;
        memoized(1);
        memoized(2);
        memoized(3);
        Assert.Equal(0, callCount);
    }

    [Fact]
    public void Memoize_SingleArg_DefaultMaxSize_Is1024()
    {
        // Arrange
        int callCount = 0;
        var memoized = Memoization.Memoize<int, int>(x =>
        {
            callCount++;
            return x;
        });

        // Act - fill cache with 1024 entries
        for (int i = 0; i < 1024; i++)
        {
            memoized(i);
        }

        Assert.Equal(1024, callCount);

        // All should be cached
        callCount = 0;
        for (int i = 0; i < 1024; i++)
        {
            memoized(i);
        }

        Assert.Equal(0, callCount);
    }

    #endregion

    #region T038: Two-Arg Memoize Tests

    [Fact]
    public void Memoize_TwoArg_FirstCall_ExecutesFunction()
    {
        // Arrange
        int callCount = 0;
        var memoized = Memoization.Memoize<int, int, int>((a, b) =>
        {
            callCount++;
            return a + b;
        });

        // Act
        var result = memoized(2, 3);

        // Assert
        Assert.Equal(5, result);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Memoize_TwoArg_RepeatCall_ReturnsCachedValue()
    {
        // Arrange
        int callCount = 0;
        var memoized = Memoization.Memoize<int, int, int>((a, b) =>
        {
            callCount++;
            return a + b;
        });

        // Act
        memoized(2, 3); // First call
        var result = memoized(2, 3); // Repeat call

        // Assert
        Assert.Equal(5, result);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Memoize_TwoArg_DifferentArgs_EachCachedSeparately()
    {
        // Arrange
        int callCount = 0;
        var memoized = Memoization.Memoize<int, int, int>((a, b) =>
        {
            callCount++;
            return a * b;
        });

        // Act
        var result1 = memoized(2, 3);
        var result2 = memoized(3, 4);
        var result3 = memoized(2, 3); // Repeat

        // Assert
        Assert.Equal(6, result1);
        Assert.Equal(12, result2);
        Assert.Equal(6, result3);
        Assert.Equal(2, callCount); // Only 2 calls, third was cached
    }

    [Fact]
    public void Memoize_TwoArg_ValueTupleKeyEquality()
    {
        // ValueTuple provides structural equality
        // Arrange
        int callCount = 0;
        var memoized = Memoization.Memoize<int, int, string>((a, b) =>
        {
            callCount++;
            return $"{a},{b}";
        });

        // Act - same values should hit cache
        memoized(1, 2);
        memoized(1, 2);

        // Assert
        Assert.Equal(1, callCount);
    }

    #endregion

    #region T039: Three-Arg Memoize Tests

    [Fact]
    public void Memoize_ThreeArg_FirstCall_ExecutesFunction()
    {
        // Arrange
        int callCount = 0;
        var memoized = Memoization.Memoize<int, int, int, int>((a, b, c) =>
        {
            callCount++;
            return a + b + c;
        });

        // Act
        var result = memoized(1, 2, 3);

        // Assert
        Assert.Equal(6, result);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Memoize_ThreeArg_RepeatCall_ReturnsCachedValue()
    {
        // Arrange
        int callCount = 0;
        var memoized = Memoization.Memoize<int, int, int, int>((a, b, c) =>
        {
            callCount++;
            return a + b + c;
        });

        // Act
        memoized(1, 2, 3); // First call
        var result = memoized(1, 2, 3); // Repeat call

        // Assert
        Assert.Equal(6, result);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Memoize_ThreeArg_DifferentArgs_EachCachedSeparately()
    {
        // Arrange
        int callCount = 0;
        var memoized = Memoization.Memoize<int, int, int, int>((a, b, c) =>
        {
            callCount++;
            return a * b * c;
        });

        // Act
        var result1 = memoized(1, 2, 3);
        var result2 = memoized(4, 5, 6);
        var result3 = memoized(1, 2, 3); // Repeat

        // Assert
        Assert.Equal(6, result1);
        Assert.Equal(120, result2);
        Assert.Equal(6, result3);
        Assert.Equal(2, callCount);
    }

    [Fact]
    public void Memoize_ThreeArg_ValueTupleKeyEquality()
    {
        // ValueTuple provides structural equality
        // Arrange
        int callCount = 0;
        var memoized = Memoization.Memoize<int, int, int, string>((a, b, c) =>
        {
            callCount++;
            return $"{a},{b},{c}";
        });

        // Act - same values should hit cache
        memoized(1, 2, 3);
        memoized(1, 2, 3);

        // Assert
        Assert.Equal(1, callCount);
    }

    #endregion

    #region T040: Eviction Tests

    [Fact]
    public void Memoize_SingleArg_MaxSizeExceeded_EvictsOldest()
    {
        // Arrange
        int callCount = 0;
        var memoized = Memoization.Memoize<int, int>(x =>
        {
            callCount++;
            return x;
        }, maxSize: 3);

        // Act - fill cache and exceed
        memoized(1); // count=1
        memoized(2); // count=2
        memoized(3); // count=3
        memoized(4); // count=4, triggers eviction of 1

        Assert.Equal(4, callCount);

        // Key 1 should be evicted, calling again should re-compute
        callCount = 0;
        memoized(1);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Memoize_TwoArg_MaxSizeExceeded_EvictsOldest()
    {
        // Arrange
        int callCount = 0;
        var memoized = Memoization.Memoize<int, int, int>((a, b) =>
        {
            callCount++;
            return a + b;
        }, maxSize: 2);

        // Act - fill cache and exceed
        memoized(1, 1); // count=1
        memoized(2, 2); // count=2
        memoized(3, 3); // count=3, triggers eviction of (1,1)

        Assert.Equal(3, callCount);

        // (1,1) should be evicted
        callCount = 0;
        memoized(1, 1);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Memoize_ThreeArg_MaxSizeExceeded_EvictsOldest()
    {
        // Arrange
        int callCount = 0;
        var memoized = Memoization.Memoize<int, int, int, int>((a, b, c) =>
        {
            callCount++;
            return a + b + c;
        }, maxSize: 2);

        // Act - fill cache and exceed
        memoized(1, 1, 1); // count=1
        memoized(2, 2, 2); // count=2
        memoized(3, 3, 3); // count=3, triggers eviction of (1,1,1)

        Assert.Equal(3, callCount);

        // (1,1,1) should be evicted
        callCount = 0;
        memoized(1, 1, 1);
        Assert.Equal(1, callCount);
    }

    #endregion

    #region T041: Equivalence Tests (SC-004)

    [Fact]
    public void Memoize_SingleArg_ReturnsIdenticalResults()
    {
        // Arrange
        Func<int, int> original = x => x * x;
        var memoized = Memoization.Memoize(original);

        // Act & Assert - compare results for various inputs
        for (int i = -100; i <= 100; i++)
        {
            Assert.Equal(original(i), memoized(i));
        }
    }

    [Fact]
    public void Memoize_TwoArg_ReturnsIdenticalResults()
    {
        // Arrange
        Func<int, int, int> original = (a, b) => a * b + a - b;
        var memoized = Memoization.Memoize(original);

        // Act & Assert
        for (int a = -10; a <= 10; a++)
        {
            for (int b = -10; b <= 10; b++)
            {
                Assert.Equal(original(a, b), memoized(a, b));
            }
        }
    }

    [Fact]
    public void Memoize_ThreeArg_ReturnsIdenticalResults()
    {
        // Arrange
        Func<int, int, int, int> original = (a, b, c) => a * b * c + a + b + c;
        var memoized = Memoization.Memoize(original);

        // Act & Assert
        for (int a = -5; a <= 5; a++)
        {
            for (int b = -5; b <= 5; b++)
            {
                for (int c = -5; c <= 5; c++)
                {
                    Assert.Equal(original(a, b, c), memoized(a, b, c));
                }
            }
        }
    }

    #endregion

    #region T042: Edge Case Tests

    [Fact]
    public void Memoize_SingleArg_NullReturnValue_IsCached()
    {
        // Arrange
        int callCount = 0;
        var memoized = Memoization.Memoize<int, string?>(x =>
        {
            callCount++;
            return null;
        });

        // Act
        var result1 = memoized(1);
        var result2 = memoized(1);

        // Assert
        Assert.Null(result1);
        Assert.Null(result2);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Memoize_TwoArg_NullReturnValue_IsCached()
    {
        // Arrange
        int callCount = 0;
        var memoized = Memoization.Memoize<int, int, string?>((a, b) =>
        {
            callCount++;
            return null;
        });

        // Act
        var result1 = memoized(1, 2);
        var result2 = memoized(1, 2);

        // Assert
        Assert.Null(result1);
        Assert.Null(result2);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Memoize_ThreeArg_NullReturnValue_IsCached()
    {
        // Arrange
        int callCount = 0;
        var memoized = Memoization.Memoize<int, int, int, string?>((a, b, c) =>
        {
            callCount++;
            return null;
        });

        // Act
        var result1 = memoized(1, 2, 3);
        var result2 = memoized(1, 2, 3);

        // Assert
        Assert.Null(result1);
        Assert.Null(result2);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Memoize_ReferenceTypeArgument_UsesReferenceEquality()
    {
        // By default, reference types use reference equality
        // Two different string instances with same content should be treated as same key
        // because strings are interned

        // Arrange
        int callCount = 0;
        var memoized = Memoization.Memoize<string, int>(s =>
        {
            callCount++;
            return s.Length;
        });

        // Act - use same string content
        var result1 = memoized("hello");
        var result2 = memoized("hello");

        // Assert - strings are interned, so same reference
        Assert.Equal(5, result1);
        Assert.Equal(5, result2);
        Assert.Equal(1, callCount);
    }

    #endregion

    #region T043: Concurrent Stress Tests

    [Fact]
    public async Task Memoize_SingleArg_ConcurrentAccess_NoExceptionsOrCorruption()
    {
        // Arrange
        var memoized = Memoization.Memoize<int, int>(x => x * 2, maxSize: 100);
        const int threadCount = 20;
        const int operationsPerThread = 500;
        var exceptions = new List<Exception>();
        var barrier = new Barrier(threadCount);

        // Act
        var tasks = Enumerable.Range(0, threadCount).Select(threadId => Task.Run(() =>
        {
            try
            {
                barrier.SignalAndWait();
                for (int i = 0; i < operationsPerThread; i++)
                {
                    int key = (threadId * 100 + i) % 200;
                    var result = memoized(key);
                    if (result != key * 2)
                    {
                        throw new Exception($"Unexpected result: expected {key * 2}, got {result}");
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
    public async Task Memoize_TwoArg_ConcurrentAccess_NoExceptionsOrCorruption()
    {
        // Arrange
        var memoized = Memoization.Memoize<int, int, int>((a, b) => a + b, maxSize: 50);
        const int threadCount = 15;
        const int operationsPerThread = 300;
        var exceptions = new List<Exception>();
        var barrier = new Barrier(threadCount);

        // Act
        var tasks = Enumerable.Range(0, threadCount).Select(threadId => Task.Run(() =>
        {
            try
            {
                barrier.SignalAndWait();
                var random = new Random(threadId);
                for (int i = 0; i < operationsPerThread; i++)
                {
                    int a = random.Next(100);
                    int b = random.Next(100);
                    var result = memoized(a, b);
                    if (result != a + b)
                    {
                        throw new Exception($"Unexpected result for ({a}, {b}): expected {a + b}, got {result}");
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
    public async Task Memoize_ThreeArg_ConcurrentAccess_NoExceptionsOrCorruption()
    {
        // Arrange
        var memoized = Memoization.Memoize<int, int, int, int>((a, b, c) => a + b + c, maxSize: 30);
        const int threadCount = 10;
        const int operationsPerThread = 200;
        var exceptions = new List<Exception>();
        var barrier = new Barrier(threadCount);

        // Act
        var tasks = Enumerable.Range(0, threadCount).Select(threadId => Task.Run(() =>
        {
            try
            {
                barrier.SignalAndWait();
                var random = new Random(threadId);
                for (int i = 0; i < operationsPerThread; i++)
                {
                    int a = random.Next(50);
                    int b = random.Next(50);
                    int c = random.Next(50);
                    var result = memoized(a, b, c);
                    if (result != a + b + c)
                    {
                        throw new Exception($"Unexpected result for ({a}, {b}, {c}): expected {a + b + c}, got {result}");
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

    #endregion
}
