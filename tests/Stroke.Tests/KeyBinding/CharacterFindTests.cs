using Stroke.KeyBinding;
using Xunit;

namespace Stroke.Tests.KeyBinding;

/// <summary>
/// Tests for the <see cref="CharacterFind"/> sealed record.
/// </summary>
public class CharacterFindTests
{
    #region Basic Property Tests (US4.1, US4.2)

    [Fact]
    public void CharacterFind_ForwardFind_BackwardsIsFalse()
    {
        // Default Backwards value is false (forward search)
        var find = new CharacterFind("x");

        Assert.Equal("x", find.Character);
        Assert.False(find.Backwards);
    }

    [Fact]
    public void CharacterFind_BackwardFind_BackwardsIsTrue()
    {
        var find = new CharacterFind("y", Backwards: true);

        Assert.Equal("y", find.Character);
        Assert.True(find.Backwards);
    }

    #endregion

    #region Equality Tests (US4.3, US4.4)

    [Fact]
    public void CharacterFind_SameValues_AreEqual()
    {
        var find1 = new CharacterFind("x", false);
        var find2 = new CharacterFind("x", false);

        // Record value semantics
        Assert.Equal(find1, find2);
        Assert.True(find1 == find2);
    }

    [Fact]
    public void CharacterFind_DifferentCharacter_AreNotEqual()
    {
        var find1 = new CharacterFind("x", false);
        var find2 = new CharacterFind("y", false);

        Assert.NotEqual(find1, find2);
        Assert.True(find1 != find2);
    }

    [Fact]
    public void CharacterFind_DifferentBackwards_AreNotEqual()
    {
        var find1 = new CharacterFind("x", false);
        var find2 = new CharacterFind("x", true);

        Assert.NotEqual(find1, find2);
    }

    [Fact]
    public void CharacterFind_IsImmutable()
    {
        // CharacterFind is a sealed record, which is inherently immutable
        // Verify it's a sealed record by checking it implements IEquatable<CharacterFind>
        // and that we can't inherit from it (this is a compile-time check)
        var find = new CharacterFind("x");

        // If this compiles, the type is correctly defined
        Assert.IsType<CharacterFind>(find);

        // Records have value equality
        var find2 = find with { Character = "y" };
        Assert.NotEqual(find, find2);
        Assert.Equal("x", find.Character); // Original unchanged
    }

    #endregion

    #region Edge Case Tests (CHK054, CHK060)

    [Fact]
    public void CharacterFind_NullCharacter_Allowed()
    {
        // Python allows any value, including None
        // No validation per Python behavior
        var find = new CharacterFind(null!);

        Assert.Null(find.Character);
    }

    [Fact]
    public void CharacterFind_EmptyString_Allowed()
    {
        var find = new CharacterFind("");

        Assert.Equal("", find.Character);
    }

    [Fact]
    public void CharacterFind_MultiCharacterString_Allowed()
    {
        // Python accepts any string, not just single characters
        var find = new CharacterFind("abc");

        Assert.Equal("abc", find.Character);
    }

    [Fact]
    public void CharacterFind_UnicodeCharacter_Allowed()
    {
        // Should handle Unicode characters correctly
        var find1 = new CharacterFind("é");
        var find2 = new CharacterFind("中");
        var find3 = new CharacterFind("🔥");

        Assert.Equal("é", find1.Character);
        Assert.Equal("中", find2.Character);
        Assert.Equal("🔥", find3.Character);
    }

    #endregion

    #region Hash Code Tests

    [Fact]
    public void CharacterFind_EqualRecords_HaveSameHashCode()
    {
        var find1 = new CharacterFind("x", true);
        var find2 = new CharacterFind("x", true);

        Assert.Equal(find1.GetHashCode(), find2.GetHashCode());
    }

    [Fact]
    public void CharacterFind_CanBeUsedInHashSet()
    {
        var set = new HashSet<CharacterFind>
        {
            new("x", false),
            new("x", false), // Duplicate
            new("x", true),   // Different Backwards
            new("y", false)   // Different Character
        };

        Assert.Equal(3, set.Count);
    }

    [Fact]
    public void CharacterFind_CanBeUsedAsDictionaryKey()
    {
        var dict = new Dictionary<CharacterFind, string>
        {
            { new CharacterFind("x", false), "forward-x" },
            { new CharacterFind("x", true), "backward-x" }
        };

        Assert.Equal("forward-x", dict[new CharacterFind("x", false)]);
        Assert.Equal("backward-x", dict[new CharacterFind("x", true)]);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void CharacterFind_ToString_ContainsPropertyValues()
    {
        var find = new CharacterFind("x", true);
        var str = find.ToString();

        // Record ToString includes property values
        Assert.Contains("x", str);
        Assert.Contains("True", str);
    }

    #endregion

    #region With Expression Tests

    [Fact]
    public void CharacterFind_WithExpression_CreatesNewInstance()
    {
        var find1 = new CharacterFind("x", false);
        var find2 = find1 with { Backwards = true };

        Assert.NotSame(find1, find2);
        Assert.Equal("x", find2.Character);
        Assert.True(find2.Backwards);
    }

    [Fact]
    public void CharacterFind_WithExpression_DoesNotModifyOriginal()
    {
        var original = new CharacterFind("a", false);
        var modified = original with { Character = "b", Backwards = true };

        Assert.Equal("a", original.Character);
        Assert.False(original.Backwards);
        Assert.Equal("b", modified.Character);
        Assert.True(modified.Backwards);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public void CharacterFind_IsInherentlyThreadSafe()
    {
        // Immutable records are inherently thread-safe
        var find = new CharacterFind("x", true);
        var exceptions = new List<Exception>();
        const int threadCount = 10;
        const int operationsPerThread = 1000;

        var threads = new Thread[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            threads[i] = new Thread(() =>
            {
                try
                {
                    for (int j = 0; j < operationsPerThread; j++)
                    {
                        _ = find.Character;
                        _ = find.Backwards;
                        _ = find.GetHashCode();
                        _ = find.ToString();
                        _ = find == new CharacterFind("x", true);
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            });
        }

        foreach (var thread in threads)
            thread.Start();

        foreach (var thread in threads)
            thread.Join();

        Assert.Empty(exceptions);
    }

    #endregion
}
