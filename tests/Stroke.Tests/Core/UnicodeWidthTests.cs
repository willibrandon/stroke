namespace Stroke.Tests.Core;

using Stroke.Core;
using Xunit;

/// <summary>
/// Tests for <see cref="UnicodeWidth"/> class.
/// </summary>
public class UnicodeWidthTests
{
    #region GetWidth(char) Tests

    [Fact]
    public void GetWidth_Char_AsciiLetter_ReturnsOne()
    {
        Assert.Equal(1, UnicodeWidth.GetWidth('A'));
        Assert.Equal(1, UnicodeWidth.GetWidth('z'));
        Assert.Equal(1, UnicodeWidth.GetWidth('5'));
    }

    [Fact]
    public void GetWidth_Char_CjkCharacter_ReturnsTwo()
    {
        // Chinese
        Assert.Equal(2, UnicodeWidth.GetWidth('中'));
        Assert.Equal(2, UnicodeWidth.GetWidth('文'));
        // Japanese Hiragana
        Assert.Equal(2, UnicodeWidth.GetWidth('あ'));
        // Korean Hangul
        Assert.Equal(2, UnicodeWidth.GetWidth('한'));
    }

    [Fact]
    public void GetWidth_Char_ControlCharacter_ReturnsZero()
    {
        // Escape character
        Assert.Equal(0, UnicodeWidth.GetWidth('\x1b'));
        // Bell
        Assert.Equal(0, UnicodeWidth.GetWidth('\x07'));
        // Carriage return
        Assert.Equal(0, UnicodeWidth.GetWidth('\r'));
        // Line feed
        Assert.Equal(0, UnicodeWidth.GetWidth('\n'));
        // Tab
        Assert.Equal(0, UnicodeWidth.GetWidth('\t'));
    }

    [Fact]
    public void GetWidth_Char_NullCharacter_ReturnsZero()
    {
        Assert.Equal(0, UnicodeWidth.GetWidth('\0'));
    }

    [Fact]
    public void GetWidth_Char_CombiningMark_ReturnsZero()
    {
        // Combining acute accent
        Assert.Equal(0, UnicodeWidth.GetWidth('\u0301'));
        // Combining grave accent
        Assert.Equal(0, UnicodeWidth.GetWidth('\u0300'));
    }

    [Fact]
    public void GetWidth_Char_Space_ReturnsOne()
    {
        Assert.Equal(1, UnicodeWidth.GetWidth(' '));
    }

    #endregion

    #region GetWidth(string) Tests

    [Fact]
    public void GetWidth_String_EmptyString_ReturnsZero()
    {
        Assert.Equal(0, UnicodeWidth.GetWidth(""));
    }

    [Fact]
    public void GetWidth_String_NullString_ReturnsZero()
    {
        Assert.Equal(0, UnicodeWidth.GetWidth(null));
    }

    [Fact]
    public void GetWidth_String_AsciiOnly_ReturnsLength()
    {
        Assert.Equal(5, UnicodeWidth.GetWidth("Hello"));
        Assert.Equal(11, UnicodeWidth.GetWidth("Hello World"));
        Assert.Equal(3, UnicodeWidth.GetWidth("123"));
    }

    [Fact]
    public void GetWidth_String_MixedAsciiCjk_SumsCorrectly()
    {
        // "Hello世界" = 5 (Hello) + 4 (世界 = 2 + 2) = 9
        Assert.Equal(9, UnicodeWidth.GetWidth("Hello世界"));

        // "A中B" = 1 + 2 + 1 = 4
        Assert.Equal(4, UnicodeWidth.GetWidth("A中B"));
    }

    [Fact]
    public void GetWidth_String_WithControlCharacters_ExcludesControlChars()
    {
        // "A\x1bB" = 1 + 0 + 1 = 2
        Assert.Equal(2, UnicodeWidth.GetWidth("A\x1bB"));
    }

    [Fact]
    public void GetWidth_String_WithCombiningMarks_ExcludesCombiningMarks()
    {
        // "e\u0301" (e + combining acute accent) = 1 + 0 = 1
        Assert.Equal(1, UnicodeWidth.GetWidth("e\u0301"));
    }

    #endregion

    #region Caching Tests

    [Fact]
    public void GetWidth_String_CachedString_ReturnsSameResult()
    {
        var text = "Cached string test";
        var width1 = UnicodeWidth.GetWidth(text);
        var width2 = UnicodeWidth.GetWidth(text);

        Assert.Equal(width1, width2);
    }

    [Fact]
    public void GetWidth_String_Exactly64Characters_CachedAsShortString()
    {
        // A string of exactly 64 characters should be treated as a short string
        var text = new string('A', 64);
        var width1 = UnicodeWidth.GetWidth(text);
        var width2 = UnicodeWidth.GetWidth(text);

        Assert.Equal(64, width1);
        Assert.Equal(width1, width2);
    }

    [Fact]
    public void GetWidth_String_65Characters_ClassifiedAsLongString()
    {
        // A string of 65 characters should be treated as a long string
        var text = new string('A', 65);
        var width = UnicodeWidth.GetWidth(text);

        Assert.Equal(65, width);
    }

    [Fact]
    public void GetWidth_String_17thLongString_EvictsOldest()
    {
        // Create 17 unique long strings (> 64 chars) to trigger FIFO eviction
        // The cache should evict the oldest when adding the 17th
        var longStrings = new List<string>();
        for (int i = 0; i < 17; i++)
        {
            // Each string is unique and > 64 characters
            var text = new string((char)('A' + i), 65);
            longStrings.Add(text);
            UnicodeWidth.GetWidth(text);
        }

        // Verify the 17th string still calculates correctly
        // (This test mainly verifies no exceptions and correct width)
        Assert.Equal(65, UnicodeWidth.GetWidth(longStrings[16]));
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task GetWidth_ConcurrentCalls_SucceedWithoutErrors()
    {
        var tasks = new List<Task<int>>();
        var testStrings = new[]
        {
            "Hello",
            "World",
            "中文",
            "日本語",
            new string('A', 100),
            new string('B', 100)
        };

        // Launch many concurrent tasks
        for (int i = 0; i < 100; i++)
        {
            var text = testStrings[i % testStrings.Length];
            tasks.Add(Task.Run(() => UnicodeWidth.GetWidth(text)));
        }

        // All tasks should complete without exception
        var exception = await Record.ExceptionAsync(async () => await Task.WhenAll(tasks));
        Assert.Null(exception);
    }

    [Fact]
    public void GetWidth_ConcurrentAccess_ReturnsConsistentResults()
    {
        var text = "Concurrent test 中文混合";
        var expectedWidth = UnicodeWidth.GetWidth(text);

        var results = new int[50];
        var threads = new Thread[50];

        for (int i = 0; i < 50; i++)
        {
            var index = i;
            threads[i] = new Thread(() =>
            {
                results[index] = UnicodeWidth.GetWidth(text);
            });
        }

        // Start all threads
        foreach (var thread in threads)
        {
            thread.Start();
        }

        // Wait for all threads
        foreach (var thread in threads)
        {
            thread.Join();
        }

        // All results should be the same
        Assert.All(results, w => Assert.Equal(expectedWidth, w));
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void GetWidth_String_FullWidthDigits_ReturnsTwo()
    {
        // Full-width numbers are typically width 2
        Assert.Equal(2, UnicodeWidth.GetWidth('\uFF10')); // ０ (full-width zero)
    }

    [Fact]
    public void GetWidth_String_MultipleControlCharacters_ExcludesControlChars()
    {
        // Multiple escape characters (just the escape chars, not full ANSI sequences)
        // Note: "\x1b[31m" = ESC (0) + [ (1) + 3 (1) + 1 (1) + m (1) = 4
        // Only the escape character itself has width 0
        Assert.Equal(4, UnicodeWidth.GetWidth("\x1b[31m"));

        // Just escape characters
        Assert.Equal(0, UnicodeWidth.GetWidth("\x1b\x1b\x1b"));
    }

    [Fact]
    public void GetWidth_String_OnlySpaces_ReturnsLength()
    {
        Assert.Equal(5, UnicodeWidth.GetWidth("     "));
    }

    #endregion
}
