using Stroke.Input;
using Stroke.Input.Pipe;
using Stroke.Input.Typeahead;
using Xunit;

namespace Stroke.Tests.Input;

/// <summary>
/// Tests for TypeaheadBuffer static class.
/// Tests use SimplePipeInput as real IInput per Constitution VIII (no mocks).
/// </summary>
public class TypeaheadBufferTests : IDisposable
{
    public TypeaheadBufferTests()
    {
        // Clear all typeahead before each test
        TypeaheadBuffer.ClearAll();
    }

    public void Dispose()
    {
        // Clean up after each test
        TypeaheadBuffer.ClearAll();
    }

    #region T095: Basic Store/Get/Clear Tests

    [Fact]
    public void Store_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            TypeaheadBuffer.Store(null!, new List<KeyPress>()));
    }

    [Fact]
    public void Store_NullKeyPresses_ThrowsArgumentNullException()
    {
        using var input = new SimplePipeInput();

        Assert.Throws<ArgumentNullException>(() =>
            TypeaheadBuffer.Store(input, null!));
    }

    [Fact]
    public void Get_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            TypeaheadBuffer.Get(null!));
    }

    [Fact]
    public void Clear_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            TypeaheadBuffer.Clear(null!));
    }

    [Fact]
    public void HasTypeahead_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            TypeaheadBuffer.HasTypeahead(null!));
    }

    [Fact]
    public void Store_ThenGet_ReturnsStoredKeyPresses()
    {
        using var input = new SimplePipeInput();
        var keyPresses = new List<KeyPress>
        {
            new(Keys.Any, "a"),
            new(Keys.Any, "b"),
            new(Keys.Any, "c")
        };

        TypeaheadBuffer.Store(input, keyPresses);
        var result = TypeaheadBuffer.Get(input);

        Assert.Equal(3, result.Count);
        Assert.Equal("a", result[0].Data);
        Assert.Equal("b", result[1].Data);
        Assert.Equal("c", result[2].Data);
    }

    [Fact]
    public void Get_ClearsBuffer()
    {
        using var input = new SimplePipeInput();
        var keyPresses = new List<KeyPress> { new(Keys.Any, "x") };

        TypeaheadBuffer.Store(input, keyPresses);
        var first = TypeaheadBuffer.Get(input);
        var second = TypeaheadBuffer.Get(input);

        Assert.Single(first);
        Assert.Empty(second);
    }

    [Fact]
    public void Get_WhenEmpty_ReturnsEmptyList()
    {
        using var input = new SimplePipeInput();

        var result = TypeaheadBuffer.Get(input);

        Assert.Empty(result);
    }

    [Fact]
    public void Clear_RemovesStoredKeyPresses()
    {
        using var input = new SimplePipeInput();
        var keyPresses = new List<KeyPress> { new(Keys.Any, "x") };

        TypeaheadBuffer.Store(input, keyPresses);
        TypeaheadBuffer.Clear(input);
        var result = TypeaheadBuffer.Get(input);

        Assert.Empty(result);
    }

    [Fact]
    public void Clear_WhenEmpty_DoesNotThrow()
    {
        using var input = new SimplePipeInput();

        TypeaheadBuffer.Clear(input);

        Assert.Empty(TypeaheadBuffer.Get(input));
    }

    [Fact]
    public void Store_EmptyList_DoesNotStore()
    {
        using var input = new SimplePipeInput();

        TypeaheadBuffer.Store(input, new List<KeyPress>());

        Assert.False(TypeaheadBuffer.HasTypeahead(input));
    }

    #endregion

    #region T096: Multiple Store Calls Append

    [Fact]
    public void Store_MultipleCalls_AppendsKeyPresses()
    {
        using var input = new SimplePipeInput();

        TypeaheadBuffer.Store(input, new List<KeyPress> { new(Keys.Any, "a") });
        TypeaheadBuffer.Store(input, new List<KeyPress> { new(Keys.Any, "b") });
        TypeaheadBuffer.Store(input, new List<KeyPress> { new(Keys.Any, "c") });

        var result = TypeaheadBuffer.Get(input);

        Assert.Equal(3, result.Count);
        Assert.Equal("a", result[0].Data);
        Assert.Equal("b", result[1].Data);
        Assert.Equal("c", result[2].Data);
    }

    [Fact]
    public void Store_MultipleInputs_SeparateBuffers()
    {
        using var input1 = new SimplePipeInput();
        using var input2 = new SimplePipeInput();

        TypeaheadBuffer.Store(input1, new List<KeyPress> { new(Keys.Any, "1") });
        TypeaheadBuffer.Store(input2, new List<KeyPress> { new(Keys.Any, "2") });

        var result1 = TypeaheadBuffer.Get(input1);
        var result2 = TypeaheadBuffer.Get(input2);

        Assert.Single(result1);
        Assert.Equal("1", result1[0].Data);
        Assert.Single(result2);
        Assert.Equal("2", result2[0].Data);
    }

    [Fact]
    public void Clear_OnlyAffectsSpecifiedInput()
    {
        using var input1 = new SimplePipeInput();
        using var input2 = new SimplePipeInput();

        TypeaheadBuffer.Store(input1, new List<KeyPress> { new(Keys.Any, "1") });
        TypeaheadBuffer.Store(input2, new List<KeyPress> { new(Keys.Any, "2") });

        TypeaheadBuffer.Clear(input1);

        Assert.Empty(TypeaheadBuffer.Get(input1));
        Assert.Single(TypeaheadBuffer.Get(input2));
    }

    #endregion

    #region T096: HasTypeahead Tests

    [Fact]
    public void HasTypeahead_WhenEmpty_ReturnsFalse()
    {
        using var input = new SimplePipeInput();

        Assert.False(TypeaheadBuffer.HasTypeahead(input));
    }

    [Fact]
    public void HasTypeahead_AfterStore_ReturnsTrue()
    {
        using var input = new SimplePipeInput();

        TypeaheadBuffer.Store(input, new List<KeyPress> { new(Keys.Any, "x") });

        Assert.True(TypeaheadBuffer.HasTypeahead(input));
    }

    [Fact]
    public void HasTypeahead_AfterGet_ReturnsFalse()
    {
        using var input = new SimplePipeInput();

        TypeaheadBuffer.Store(input, new List<KeyPress> { new(Keys.Any, "x") });
        TypeaheadBuffer.Get(input);

        Assert.False(TypeaheadBuffer.HasTypeahead(input));
    }

    [Fact]
    public void HasTypeahead_AfterClear_ReturnsFalse()
    {
        using var input = new SimplePipeInput();

        TypeaheadBuffer.Store(input, new List<KeyPress> { new(Keys.Any, "x") });
        TypeaheadBuffer.Clear(input);

        Assert.False(TypeaheadBuffer.HasTypeahead(input));
    }

    #endregion

    #region T096: ClearAll Tests

    [Fact]
    public void ClearAll_RemovesAllBuffers()
    {
        using var input1 = new SimplePipeInput();
        using var input2 = new SimplePipeInput();

        TypeaheadBuffer.Store(input1, new List<KeyPress> { new(Keys.Any, "1") });
        TypeaheadBuffer.Store(input2, new List<KeyPress> { new(Keys.Any, "2") });

        TypeaheadBuffer.ClearAll();

        Assert.Empty(TypeaheadBuffer.Get(input1));
        Assert.Empty(TypeaheadBuffer.Get(input2));
    }

    #endregion

    #region T096: Thread Safety Tests

    [Fact]
    public void ThreadSafety_ConcurrentStoreAndGet()
    {
        using var input = new SimplePipeInput();
        var iterations = 1000;
        var stored = 0;
        var retrieved = new List<KeyPress>();
        var lockObj = new object();

        Parallel.For(0, iterations, i =>
        {
            if (i % 2 == 0)
            {
                TypeaheadBuffer.Store(input, new List<KeyPress> { new(Keys.Any, i.ToString()) });
                Interlocked.Increment(ref stored);
            }
            else
            {
                var result = TypeaheadBuffer.Get(input);
                lock (lockObj)
                {
                    retrieved.AddRange(result);
                }
            }
        });

        // Final get to collect any remaining
        var remaining = TypeaheadBuffer.Get(input);
        retrieved.AddRange(remaining);

        // All stored items should be retrievable
        Assert.Equal(iterations / 2, retrieved.Count);
    }

    [Fact]
    public void ThreadSafety_ConcurrentClear()
    {
        using var input = new SimplePipeInput();

        Parallel.For(0, 100, i =>
        {
            TypeaheadBuffer.Store(input, new List<KeyPress> { new(Keys.Any, i.ToString()) });
            TypeaheadBuffer.Clear(input);
        });

        // Should not throw and should be empty at the end
        Assert.False(TypeaheadBuffer.HasTypeahead(input));
    }

    [Fact]
    public void ThreadSafety_MultipleInputsConcurrent()
    {
        var inputs = Enumerable.Range(0, 10).Select(_ => new SimplePipeInput()).ToList();

        try
        {
            Parallel.ForEach(inputs, input =>
            {
                for (var i = 0; i < 100; i++)
                {
                    TypeaheadBuffer.Store(input, new List<KeyPress> { new(Keys.Any, i.ToString()) });
                }
            });

            // Each input should have 100 key presses
            foreach (var input in inputs)
            {
                var result = TypeaheadBuffer.Get(input);
                Assert.Equal(100, result.Count);
            }
        }
        finally
        {
            foreach (var input in inputs)
            {
                input.Dispose();
            }
        }
    }

    #endregion

    #region T096: Different Key Types

    [Fact]
    public void Store_DifferentKeyTypes_PreservesAll()
    {
        using var input = new SimplePipeInput();
        var keyPresses = new List<KeyPress>
        {
            new(Keys.ControlA, "\x01"),
            new(Keys.Escape, "\x1b"),
            new(Keys.Up, "\x1b[A"),
            new(Keys.F1, "\x1bOP"),
            new(Keys.Any, "x"),
            new(Keys.BracketedPaste, "pasted text")
        };

        TypeaheadBuffer.Store(input, keyPresses);
        var result = TypeaheadBuffer.Get(input);

        Assert.Equal(6, result.Count);
        Assert.Equal(Keys.ControlA, result[0].Key);
        Assert.Equal(Keys.Escape, result[1].Key);
        Assert.Equal(Keys.Up, result[2].Key);
        Assert.Equal(Keys.F1, result[3].Key);
        Assert.Equal(Keys.Any, result[4].Key);
        Assert.Equal(Keys.BracketedPaste, result[5].Key);
    }

    #endregion

    #region T097: TypeaheadHash Tests

    [Fact]
    public void TypeaheadHash_SameInput_ReturnsConsistentHash()
    {
        using var input = new SimplePipeInput();

        var hash1 = input.TypeaheadHash();
        var hash2 = input.TypeaheadHash();

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void TypeaheadHash_DifferentInputs_ReturnDifferentHashes()
    {
        using var input1 = new SimplePipeInput();
        using var input2 = new SimplePipeInput();

        var hash1 = input1.TypeaheadHash();
        var hash2 = input2.TypeaheadHash();

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void TypeaheadHash_DummyInput_ReturnsConsistentHash()
    {
        using var input = new DummyInput();

        var hash1 = input.TypeaheadHash();
        var hash2 = input.TypeaheadHash();

        Assert.Equal(hash1, hash2);
        Assert.Contains("DummyInput", hash1);
    }

    #endregion
}
