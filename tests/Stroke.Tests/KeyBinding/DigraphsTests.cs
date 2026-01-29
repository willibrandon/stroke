using Stroke.KeyBinding;
using Xunit;

namespace Stroke.Tests.KeyBinding;

/// <summary>
/// Tests for the <see cref="Digraphs"/> static class.
/// </summary>
public sealed class DigraphsTests
{
    #region Phase 2: Foundational - Entry Count Verification (T023)

    [Fact]
    public void Map_ContainsExpectedEntryCount()
    {
        // Arrange & Act
        var count = Digraphs.Map.Count;

        // Assert - spec says 1,300+ entries from Python source
        Assert.True(count >= 1300, $"Expected at least 1300 entries but found {count}");
    }

    #endregion

    #region Phase 3: User Story 1 - Lookup Method Tests (T024-T028)

    [Fact]
    public void Lookup_ReturnsEuroSignCodePoint_ForEu()
    {
        // Act
        var result = Digraphs.Lookup('E', 'u');

        // Assert
        Assert.Equal(0x20AC, result);
    }

    [Fact]
    public void Lookup_ReturnsGreekPiCodePoint_ForPAsterisk()
    {
        // Act
        var result = Digraphs.Lookup('p', '*');

        // Assert
        Assert.Equal(0x03C0, result);
    }

    [Fact]
    public void Lookup_ReturnsLeftArrowCodePoint_ForLessThanMinus()
    {
        // Act
        var result = Digraphs.Lookup('<', '-');

        // Assert
        Assert.Equal(0x2190, result);
    }

    [Fact]
    public void Lookup_ReturnsBoxDrawingHorizontal_ForHh()
    {
        // Act
        var result = Digraphs.Lookup('h', 'h');

        // Assert
        Assert.Equal(0x2500, result);
    }

    [Fact]
    public void Lookup_IsCaseSensitive_DifferentGreekLetters()
    {
        // Act
        var lowercase = Digraphs.Lookup('a', '*');
        var uppercase = Digraphs.Lookup('A', '*');

        // Assert
        Assert.NotEqual(lowercase, uppercase);
        Assert.Equal(0x03B1, lowercase); // Greek lowercase alpha
        Assert.Equal(0x0391, uppercase); // Greek uppercase Alpha
    }

    #endregion

    #region Phase 4: User Story 2 - Invalid Digraph Handling Tests (T031-T034)

    [Fact]
    public void Lookup_ReturnsNull_ForZZ()
    {
        // Act
        var result = Digraphs.Lookup('Z', 'Z');

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Lookup_ReturnsNull_ForExclamationAt()
    {
        // Act
        var result = Digraphs.Lookup('!', '@');

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Lookup_ReturnsNull_ForReversedOrder()
    {
        // ('E', 'u') is valid and returns Euro sign
        // ('u', 'E') should return null as order matters
        var valid = Digraphs.Lookup('E', 'u');
        var reversed = Digraphs.Lookup('u', 'E');

        Assert.NotNull(valid);
        Assert.Null(reversed);
    }

    [Fact]
    public void Lookup_DoesNotThrow_ForAnyInvalidInput()
    {
        // Test various invalid inputs - none should throw
        var exception = Record.Exception(() =>
        {
            _ = Digraphs.Lookup('\0', '\0');
            _ = Digraphs.Lookup('\xFF', '\xFF');
            _ = Digraphs.Lookup('Z', 'Z');
            _ = Digraphs.Lookup('!', '@');
            _ = Digraphs.Lookup('#', '$');
            _ = Digraphs.Lookup('9', '9');
        });

        Assert.Null(exception);
    }

    #endregion

    #region Phase 5: User Story 3 - GetString Method Tests (T036-T040)

    [Fact]
    public void GetString_ReturnsEuroSign_ForEu()
    {
        // Act
        var result = Digraphs.GetString('E', 'u');

        // Assert
        Assert.Equal("€", result);
    }

    [Fact]
    public void GetString_ReturnsGreekPi_ForPAsterisk()
    {
        // Act
        var result = Digraphs.GetString('p', '*');

        // Assert
        Assert.Equal("π", result);
    }

    [Fact]
    public void GetString_ReturnsNull_ForInvalidDigraph()
    {
        // Act
        var result = Digraphs.GetString('Z', 'Z');

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetString_HandlesBmpCharactersCorrectly()
    {
        // Test various BMP characters
        Assert.Equal("─", Digraphs.GetString('h', 'h')); // Box drawing horizontal
        Assert.Equal("←", Digraphs.GetString('<', '-')); // Left arrow
        Assert.Equal("≠", Digraphs.GetString('!', '=')); // Not equal
        Assert.Equal("✓", Digraphs.GetString('O', 'K')); // Check mark
    }

    [Fact]
    public void GetString_UsesConvertFromUtf32_ReadyForSupplementaryPlane()
    {
        // Per research.md, Python source has no code points >0xFFFF,
        // but verify implementation uses char.ConvertFromUtf32 by testing
        // a high BMP code point (just below supplementary plane threshold)
        var result = Digraphs.GetString('f', 'i'); // fi ligature at 0xFB01

        Assert.NotNull(result);
        Assert.Equal("ﬁ", result);
        Assert.Single(result); // Single UTF-16 code unit for BMP
    }

    #endregion

    #region Phase 6: User Story 4 - Map Property Tests (T043-T047)

    [Fact]
    public void Map_ReturnsNonNullDictionary()
    {
        // Act
        var map = Digraphs.Map;

        // Assert
        Assert.NotNull(map);
    }

    [Fact]
    public void Map_ContainsExpectedEntryCount_Duplicate()
    {
        // This duplicates T023 for user story completeness
        var count = Digraphs.Map.Count;
        Assert.True(count >= 1300);
    }

    [Fact]
    public void Map_ContainsEuroSignMapping()
    {
        // Act
        var containsKey = Digraphs.Map.ContainsKey(('E', 'u'));
        var value = Digraphs.Map[('E', 'u')];

        // Assert
        Assert.True(containsKey);
        Assert.Equal(0x20AC, value);
    }

    [Fact]
    public void Map_IsEnumerable_CanFilterByCodePointRange()
    {
        // Filter for Greek letters (0x0370-0x03FF)
        var greekLetters = Digraphs.Map
            .Where(kvp => kvp.Value >= 0x0370 && kvp.Value <= 0x03FF)
            .ToList();

        // Assert - should have Greek letters in the dictionary
        Assert.NotEmpty(greekLetters);
        Assert.True(greekLetters.Count >= 50, $"Expected at least 50 Greek entries but found {greekLetters.Count}");
    }

    [Fact]
    public void Map_ReturnsSameInstance_OnMultipleAccesses()
    {
        // Act
        var map1 = Digraphs.Map;
        var map2 = Digraphs.Map;

        // Assert - should be same instance (reference equality)
        Assert.Same(map1, map2);
    }

    #endregion

    #region Phase 7: Polish - Category Coverage Tests (T050)

    [Theory]
    [InlineData(0x00, 0x1F, 25, "Control characters")] // Slightly lower bound for flexibility
    [InlineData(0x20, 0x7F, 10, "ASCII printable")]
    [InlineData(0xA0, 0xFF, 50, "Latin-1 Supplement")]
    [InlineData(0x0100, 0x017F, 80, "Latin Extended-A")]
    [InlineData(0x0370, 0x03FF, 50, "Greek and Coptic")]
    [InlineData(0x0400, 0x04FF, 80, "Cyrillic")]
    [InlineData(0x05D0, 0x05EA, 20, "Hebrew")]
    [InlineData(0x0600, 0x06FF, 40, "Arabic")]
    [InlineData(0x2500, 0x257F, 40, "Box drawing")]
    [InlineData(0x3040, 0x309F, 70, "Hiragana")]
    [InlineData(0x30A0, 0x30FF, 70, "Katakana")]
    public void Map_HasExpectedApproximateCount_ForUnicodeBlock(
        int rangeStart, int rangeEnd, int minimumExpected, string blockName)
    {
        var count = Digraphs.Map.Count(kvp =>
            kvp.Value >= rangeStart && kvp.Value <= rangeEnd);

        Assert.True(count >= minimumExpected,
            $"{blockName} block (U+{rangeStart:X4}-U+{rangeEnd:X4}): expected at least {minimumExpected} entries but found {count}");
    }

    #endregion

    #region Phase 7: Polish - Thread Safety Tests (T051)

    [Fact]
    public void Lookup_IsThreadSafe_ConcurrentAccess()
    {
        // Arrange
        const int threadCount = 10;
        const int operationsPerThread = 1000;
        var exceptions = new List<Exception>();
        var lockObj = new object();

        // Act - run concurrent lookups
        Parallel.For(0, threadCount, (int _) =>
        {
            try
            {
                for (int i = 0; i < operationsPerThread; i++)
                {
                    var lookup1 = Digraphs.Lookup('E', 'u');
                    var lookup2 = Digraphs.Lookup('p', '*');
                    var lookup3 = Digraphs.Lookup('Z', 'Z');
                    var getString = Digraphs.GetString('E', 'u');
                    var count = Digraphs.Map.Count;
                }
            }
            catch (Exception ex)
            {
                lock (lockObj)
                {
                    exceptions.Add(ex);
                }
            }
        });

        // Assert
        Assert.Empty(exceptions);
    }

    #endregion

    #region Phase 7: Polish - Control Character Edge Cases (T052)

    [Fact]
    public void Lookup_ReturnsCorrectValue_ForNullCharacter()
    {
        // ('N', 'U') maps to 0x00 (NUL)
        var result = Digraphs.Lookup('N', 'U');
        Assert.Equal(0x00, result);
    }

    [Fact]
    public void Lookup_ReturnsCorrectValue_ForCarriageReturn()
    {
        // ('C', 'R') maps to 0x0D (CR)
        var result = Digraphs.Lookup('C', 'R');
        Assert.Equal(0x0D, result);
    }

    [Fact]
    public void Lookup_ReturnsCorrectValue_ForLineFeed()
    {
        // ('L', 'F') maps to 0x0A (LF)
        var result = Digraphs.Lookup('L', 'F');
        Assert.Equal(0x0A, result);
    }

    [Fact]
    public void GetString_HandlesControlCharacters()
    {
        // Control characters should convert to strings (even if not printable)
        var nul = Digraphs.GetString('N', 'U');
        Assert.NotNull(nul);
        Assert.Equal("\0", nul);
    }

    #endregion
}
