using Stroke.Layout;
using Xunit;

namespace Stroke.Tests.Layout;

public class CharacterDisplayMappingsTests
{
    [Fact]
    public void Mappings_HasExactly66Entries()
    {
        // 32 C0 + 1 DEL + 32 C1 + 1 NBSP = 66
        Assert.Equal(66, CharacterDisplayMappings.Mappings.Count);
    }

    [Theory]
    [InlineData('\x00', "^@")]  // NUL
    [InlineData('\x01', "^A")]  // SOH (Ctrl+A)
    [InlineData('\x02', "^B")]  // STX
    [InlineData('\x07', "^G")]  // BEL
    [InlineData('\x08', "^H")]  // BS
    [InlineData('\x09', "^I")]  // TAB
    [InlineData('\x0A', "^J")]  // LF
    [InlineData('\x0D', "^M")]  // CR
    [InlineData('\x1B', "^[")]  // ESC
    [InlineData('\x1F', "^_")]  // US (last C0)
    public void Mappings_C0Controls_AreCaretNotation(char input, string expected)
    {
        Assert.True(CharacterDisplayMappings.Mappings.TryGetValue(input, out var display));
        Assert.Equal(expected, display);
    }

    [Fact]
    public void Mappings_DEL_IsCaretQuestion()
    {
        Assert.True(CharacterDisplayMappings.Mappings.TryGetValue('\x7F', out var display));
        Assert.Equal("^?", display);
    }

    [Theory]
    [InlineData('\x80', "<80>")]
    [InlineData('\x81', "<81>")]
    [InlineData('\x8F', "<8f>")]
    [InlineData('\x90', "<90>")]
    [InlineData('\x9F', "<9f>")]
    public void Mappings_C1Controls_AreHexNotation(char input, string expected)
    {
        Assert.True(CharacterDisplayMappings.Mappings.TryGetValue(input, out var display));
        Assert.Equal(expected, display);
    }

    [Fact]
    public void Mappings_NonBreakingSpace_IsSpace()
    {
        Assert.True(CharacterDisplayMappings.Mappings.TryGetValue('\xA0', out var display));
        Assert.Equal(" ", display);
    }

    [Theory]
    [InlineData('A')]  // Normal ASCII
    [InlineData(' ')]  // Space
    [InlineData('~')]  // Last printable ASCII
    [InlineData('\xA1')]  // Character after NBSP
    [InlineData('\xFF')]  // High byte outside C1
    public void Mappings_NormalCharacters_NotMapped(char c)
    {
        Assert.False(CharacterDisplayMappings.Mappings.ContainsKey(c));
    }

    [Theory]
    [InlineData('\x01', "^A")]
    [InlineData('\x7F', "^?")]
    [InlineData('\x80', "<80>")]
    [InlineData('\xA0', " ")]
    public void TryGetDisplay_ReturnsCorrectValue(char c, string expected)
    {
        Assert.True(CharacterDisplayMappings.TryGetDisplay(c, out var display));
        Assert.Equal(expected, display);
    }

    [Fact]
    public void TryGetDisplay_NormalChar_ReturnsFalse()
    {
        Assert.False(CharacterDisplayMappings.TryGetDisplay('A', out _));
    }

    [Theory]
    [InlineData('\x01', "^A")]
    [InlineData('\x7F', "^?")]
    [InlineData('A', "A")]
    [InlineData(' ', " ")]
    public void GetDisplayOrDefault_ReturnsCorrectValue(char c, string expected)
    {
        Assert.Equal(expected, CharacterDisplayMappings.GetDisplayOrDefault(c));
    }

    [Theory]
    [InlineData('\x00', true)]   // NUL
    [InlineData('\x1F', true)]   // Last C0
    [InlineData('\x20', false)]  // Space
    [InlineData('\x7E', false)]  // Tilde
    [InlineData('\x7F', true)]   // DEL
    [InlineData('\x80', true)]   // First C1
    [InlineData('\x9F', true)]   // Last C1
    [InlineData('\xA0', false)]  // NBSP (not a control char per spec)
    [InlineData('\xA1', false)]  // After NBSP
    public void IsControlCharacter_CorrectlyIdentifies(char c, bool expected)
    {
        Assert.Equal(expected, CharacterDisplayMappings.IsControlCharacter(c));
    }

    [Theory]
    [InlineData('\xA0', true)]
    [InlineData(' ', false)]
    [InlineData('\x00', false)]
    [InlineData('\xA1', false)]
    public void IsNonBreakingSpace_CorrectlyIdentifies(char c, bool expected)
    {
        Assert.Equal(expected, CharacterDisplayMappings.IsNonBreakingSpace(c));
    }

    [Fact]
    public void Mappings_AllC0Controls_ArePresent()
    {
        // C0: 0x00-0x1F (32 characters)
        for (int i = 0; i <= 0x1F; i++)
        {
            Assert.True(CharacterDisplayMappings.Mappings.ContainsKey((char)i),
                $"Missing mapping for C0 control 0x{i:X2}");
        }
    }

    [Fact]
    public void Mappings_AllC1Controls_ArePresent()
    {
        // C1: 0x80-0x9F (32 characters)
        for (int i = 0x80; i <= 0x9F; i++)
        {
            Assert.True(CharacterDisplayMappings.Mappings.ContainsKey((char)i),
                $"Missing mapping for C1 control 0x{i:X2}");
        }
    }
}
