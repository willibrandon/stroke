using Stroke.Input;
using Stroke.Input.Vt100;
using Xunit;

namespace Stroke.Tests.Input;

public class AnsiSequencesTests
{
    [Theory]
    [InlineData("\x1b[A", Keys.Up)]
    [InlineData("\x1b[B", Keys.Down)]
    [InlineData("\x1b[C", Keys.Right)]
    [InlineData("\x1b[D", Keys.Left)]
    [InlineData("\x1b[H", Keys.Home)]
    [InlineData("\x1b[F", Keys.End)]
    [InlineData("\x1b[2~", Keys.Insert)]
    [InlineData("\x1b[3~", Keys.Delete)]
    [InlineData("\x1b[5~", Keys.PageUp)]
    [InlineData("\x1b[6~", Keys.PageDown)]
    public void Sequences_NavigationKeys_MappedCorrectly(string sequence, Keys expectedKey)
    {
        Assert.True(AnsiSequences.TryGetKey(sequence, out var key));
        Assert.Equal(expectedKey, key);
    }

    [Theory]
    [InlineData("\x1b[1;5A", Keys.ControlUp)]
    [InlineData("\x1b[1;5B", Keys.ControlDown)]
    [InlineData("\x1b[1;5C", Keys.ControlRight)]
    [InlineData("\x1b[1;5D", Keys.ControlLeft)]
    [InlineData("\x1b[1;2A", Keys.ShiftUp)]
    [InlineData("\x1b[1;2B", Keys.ShiftDown)]
    [InlineData("\x1b[1;2C", Keys.ShiftRight)]
    [InlineData("\x1b[1;2D", Keys.ShiftLeft)]
    [InlineData("\x1b[1;6A", Keys.ControlShiftUp)]
    [InlineData("\x1b[1;6B", Keys.ControlShiftDown)]
    [InlineData("\x1b[1;6C", Keys.ControlShiftRight)]
    [InlineData("\x1b[1;6D", Keys.ControlShiftLeft)]
    public void Sequences_ModifiedNavigationKeys_MappedCorrectly(string sequence, Keys expectedKey)
    {
        Assert.True(AnsiSequences.TryGetKey(sequence, out var key));
        Assert.Equal(expectedKey, key);
    }

    [Theory]
    [InlineData("\x1bOP", Keys.F1)]
    [InlineData("\x1bOQ", Keys.F2)]
    [InlineData("\x1bOR", Keys.F3)]
    [InlineData("\x1bOS", Keys.F4)]
    [InlineData("\x1b[15~", Keys.F5)]
    [InlineData("\x1b[17~", Keys.F6)]
    [InlineData("\x1b[18~", Keys.F7)]
    [InlineData("\x1b[19~", Keys.F8)]
    [InlineData("\x1b[20~", Keys.F9)]
    [InlineData("\x1b[21~", Keys.F10)]
    [InlineData("\x1b[23~", Keys.F11)]
    [InlineData("\x1b[24~", Keys.F12)]
    public void Sequences_FunctionKeys_MappedCorrectly(string sequence, Keys expectedKey)
    {
        Assert.True(AnsiSequences.TryGetKey(sequence, out var key));
        Assert.Equal(expectedKey, key);
    }

    [Theory]
    [InlineData("\x1b[Z", Keys.BackTab)]
    [InlineData("\x1b[200~", Keys.BracketedPaste)]
    [InlineData("\x1b[201~", Keys.BracketedPaste)]
    public void Sequences_SpecialKeys_MappedCorrectly(string sequence, Keys expectedKey)
    {
        Assert.True(AnsiSequences.TryGetKey(sequence, out var key));
        Assert.Equal(expectedKey, key);
    }

    [Fact]
    public void TryGetKey_UnknownSequence_ReturnsFalse()
    {
        Assert.False(AnsiSequences.TryGetKey("\x1b[INVALID", out _));
    }

    [Theory]
    [InlineData(Keys.Up, "\x1b[A")]
    [InlineData(Keys.Down, "\x1b[B")]
    [InlineData(Keys.Right, "\x1b[C")]
    [InlineData(Keys.Left, "\x1b[D")]
    [InlineData(Keys.Home, "\x1b[H")]
    [InlineData(Keys.End, "\x1b[F")]
    public void ReverseSequences_NavigationKeys_MappedCorrectly(Keys key, string expectedSequence)
    {
        Assert.True(AnsiSequences.TryGetSequence(key, out var sequence));
        Assert.Equal(expectedSequence, sequence);
    }

    [Theory]
    [InlineData(Keys.F1, "\x1bOP")]
    [InlineData(Keys.F2, "\x1bOQ")]
    [InlineData(Keys.F3, "\x1bOR")]
    [InlineData(Keys.F4, "\x1bOS")]
    public void ReverseSequences_FunctionKeys_MappedCorrectly(Keys key, string expectedSequence)
    {
        Assert.True(AnsiSequences.TryGetSequence(key, out var sequence));
        Assert.Equal(expectedSequence, sequence);
    }

    [Theory]
    [InlineData(Keys.ControlA, "\x01")]
    [InlineData(Keys.ControlC, "\x03")]
    [InlineData(Keys.ControlD, "\x04")]
    [InlineData(Keys.Escape, "\x1b")]
    public void ReverseSequences_ControlCharacters_MappedCorrectly(Keys key, string expectedSequence)
    {
        Assert.True(AnsiSequences.TryGetSequence(key, out var sequence));
        Assert.Equal(expectedSequence, sequence);
    }

    [Fact]
    public void TryGetSequence_UnmappedKey_ReturnsFalse()
    {
        // Keys.Any doesn't have a reverse sequence
        Assert.False(AnsiSequences.TryGetSequence(Keys.Any, out _));
    }

    [Theory]
    [InlineData("\x1b")]
    [InlineData("\x1b[")]
    [InlineData("\x1b[1")]
    [InlineData("\x1b[1;")]
    [InlineData("\x1b[1;5")]
    [InlineData("\x1bO")]
    public void IsPrefixOfLongerSequence_ValidPrefixes_ReturnsTrue(string prefix)
    {
        Assert.True(AnsiSequences.IsPrefixOfLongerSequence(prefix));
    }

    [Fact]
    public void IsPrefixOfLongerSequence_CompleteSequence_ReturnsFalse()
    {
        // A complete sequence is not a prefix of a longer one
        Assert.False(AnsiSequences.IsPrefixOfLongerSequence("\x1b[A"));
    }

    [Fact]
    public void IsPrefixOfLongerSequence_InvalidPrefix_ReturnsFalse()
    {
        Assert.False(AnsiSequences.IsPrefixOfLongerSequence("abc"));
    }

    [Fact]
    public void Sequences_IsFrozenDictionary()
    {
        // Verify the type is a frozen dictionary (optimized for read)
        Assert.IsAssignableFrom<System.Collections.Frozen.FrozenDictionary<string, Keys>>(
            AnsiSequences.Sequences);
    }

    [Fact]
    public void ReverseSequences_IsFrozenDictionary()
    {
        Assert.IsAssignableFrom<System.Collections.Frozen.FrozenDictionary<Keys, string>>(
            AnsiSequences.ReverseSequences);
    }

    [Fact]
    public void ValidPrefixes_IsFrozenSet()
    {
        Assert.IsAssignableFrom<System.Collections.Frozen.FrozenSet<string>>(
            AnsiSequences.ValidPrefixes);
    }

    [Fact]
    public void Sequences_ContainsAlternativeHomeEndSequences()
    {
        // Some terminals use different sequences for Home/End
        Assert.True(AnsiSequences.TryGetKey("\x1b[1~", out var home1));
        Assert.Equal(Keys.Home, home1);

        Assert.True(AnsiSequences.TryGetKey("\x1b[4~", out var end1));
        Assert.Equal(Keys.End, end1);

        Assert.True(AnsiSequences.TryGetKey("\x1bOH", out var home2));
        Assert.Equal(Keys.Home, home2);

        Assert.True(AnsiSequences.TryGetKey("\x1bOF", out var end2));
        Assert.Equal(Keys.End, end2);
    }

    [Fact]
    public void Sequences_ContainsAlternativeFunctionKeySequences()
    {
        // Some terminals use CSI sequences for F1-F4
        Assert.True(AnsiSequences.TryGetKey("\x1b[11~", out var f1));
        Assert.Equal(Keys.F1, f1);

        Assert.True(AnsiSequences.TryGetKey("\x1b[12~", out var f2));
        Assert.Equal(Keys.F2, f2);
    }

    [Fact]
    public void ValidPrefixes_ContainsMouseEventPrefixes()
    {
        // X10 mouse: \x1b[M followed by 3 bytes
        Assert.True(AnsiSequences.IsPrefixOfLongerSequence("\x1b[M"));

        // SGR mouse: \x1b[< followed by digits and semicolons
        Assert.True(AnsiSequences.IsPrefixOfLongerSequence("\x1b[<"));
    }
}
