using Stroke.Input;
using Stroke.KeyBinding;
using Xunit;

namespace Stroke.Tests.KeyBinding;

/// <summary>
/// Tests for the <see cref="KeyOrChar"/> readonly record struct.
/// </summary>
public sealed class KeyOrCharTests
{
    #region Construction from Keys

    [Fact]
    public void Constructor_WithKeys_SetsKeyValue()
    {
        var koc = new KeyOrChar(Keys.ControlC);

        Assert.True(koc.IsKey);
        Assert.False(koc.IsChar);
        Assert.Equal(Keys.ControlC, koc.Key);
    }

    [Fact]
    public void Constructor_WithKeysAny_SetsKeyValue()
    {
        var koc = new KeyOrChar(Keys.Any);

        Assert.True(koc.IsKey);
        Assert.Equal(Keys.Any, koc.Key);
    }

    [Theory]
    [InlineData(Keys.ControlM)]
    [InlineData(Keys.ControlI)]
    [InlineData(Keys.Escape)]
    [InlineData(Keys.ControlH)]
    [InlineData(Keys.ControlA)]
    [InlineData(Keys.F1)]
    public void Constructor_WithVariousKeys_SetsCorrectly(Keys key)
    {
        var koc = new KeyOrChar(key);

        Assert.True(koc.IsKey);
        Assert.Equal(key, koc.Key);
    }

    #endregion

    #region Construction from Char

    [Fact]
    public void Constructor_WithChar_SetsCharValue()
    {
        var koc = new KeyOrChar('a');

        Assert.False(koc.IsKey);
        Assert.True(koc.IsChar);
        Assert.Equal('a', koc.Char);
    }

    [Theory]
    [InlineData('a')]
    [InlineData('Z')]
    [InlineData('0')]
    [InlineData(' ')]
    [InlineData('\n')]
    [InlineData('\t')]
    public void Constructor_WithVariousChars_SetsCorrectly(char c)
    {
        var koc = new KeyOrChar(c);

        Assert.True(koc.IsChar);
        Assert.Equal(c, koc.Char);
    }

    [Fact]
    public void Constructor_WithUnicodeChar_SetsCorrectly()
    {
        var koc = new KeyOrChar('日');

        Assert.True(koc.IsChar);
        Assert.Equal('日', koc.Char);
    }

    #endregion

    #region Key/Char Accessor Exceptions

    [Fact]
    public void Key_WhenIsChar_ThrowsInvalidOperationException()
    {
        var koc = new KeyOrChar('a');

        var ex = Assert.Throws<InvalidOperationException>(() => koc.Key);
        Assert.Contains("IsKey", ex.Message);
    }

    [Fact]
    public void Char_WhenIsKey_ThrowsInvalidOperationException()
    {
        var koc = new KeyOrChar(Keys.ControlM);

        var ex = Assert.Throws<InvalidOperationException>(() => koc.Char);
        Assert.Contains("IsChar", ex.Message);
    }

    #endregion

    #region Implicit Conversions

    [Fact]
    public void ImplicitConversion_FromKeys_CreatesKeyOrChar()
    {
        KeyOrChar koc = Keys.ControlX;

        Assert.True(koc.IsKey);
        Assert.Equal(Keys.ControlX, koc.Key);
    }

    [Fact]
    public void ImplicitConversion_FromChar_CreatesKeyOrChar()
    {
        KeyOrChar koc = 'x';

        Assert.True(koc.IsChar);
        Assert.Equal('x', koc.Char);
    }

    [Fact]
    public void ImplicitConversion_FromSingleCharString_CreatesKeyOrChar()
    {
        KeyOrChar koc = "x";

        Assert.True(koc.IsChar);
        Assert.Equal('x', koc.Char);
    }

    [Fact]
    public void ImplicitConversion_FromString_ThrowsForMultipleChars()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            KeyOrChar koc = "abc";
        });
    }

    [Fact]
    public void ImplicitConversion_FromString_ThrowsForEmptyString()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            KeyOrChar koc = "";
        });
    }

    [Fact]
    public void ImplicitConversion_FromNullString_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            KeyOrChar koc = (string)null!;
        });
    }

    #endregion

    #region Equality

    [Fact]
    public void Equals_SameKey_ReturnsTrue()
    {
        var koc1 = new KeyOrChar(Keys.ControlM);
        var koc2 = new KeyOrChar(Keys.ControlM);

        Assert.Equal(koc1, koc2);
        Assert.True(koc1 == koc2);
        Assert.False(koc1 != koc2);
    }

    [Fact]
    public void Equals_DifferentKey_ReturnsFalse()
    {
        var koc1 = new KeyOrChar(Keys.ControlM);
        var koc2 = new KeyOrChar(Keys.ControlI);

        Assert.NotEqual(koc1, koc2);
        Assert.False(koc1 == koc2);
        Assert.True(koc1 != koc2);
    }

    [Fact]
    public void Equals_SameChar_ReturnsTrue()
    {
        var koc1 = new KeyOrChar('a');
        var koc2 = new KeyOrChar('a');

        Assert.Equal(koc1, koc2);
        Assert.True(koc1 == koc2);
    }

    [Fact]
    public void Equals_DifferentChar_ReturnsFalse()
    {
        var koc1 = new KeyOrChar('a');
        var koc2 = new KeyOrChar('b');

        Assert.NotEqual(koc1, koc2);
        Assert.False(koc1 == koc2);
    }

    [Fact]
    public void Equals_KeyAndChar_ReturnsFalse()
    {
        var koc1 = new KeyOrChar(Keys.ControlM);
        var koc2 = new KeyOrChar('\n');

        Assert.NotEqual(koc1, koc2);
        Assert.False(koc1 == koc2);
    }

    [Fact]
    public void GetHashCode_SameKey_ReturnsSameValue()
    {
        var koc1 = new KeyOrChar(Keys.ControlC);
        var koc2 = new KeyOrChar(Keys.ControlC);

        Assert.Equal(koc1.GetHashCode(), koc2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_SameChar_ReturnsSameValue()
    {
        var koc1 = new KeyOrChar('a');
        var koc2 = new KeyOrChar('a');

        Assert.Equal(koc1.GetHashCode(), koc2.GetHashCode());
    }

    #endregion

    #region ToString

    [Fact]
    public void ToString_WithKey_ReturnsKeyName()
    {
        var koc = new KeyOrChar(Keys.ControlC);

        var result = koc.ToString();

        Assert.Contains("ControlC", result);
    }

    [Fact]
    public void ToString_WithChar_ReturnsCharValue()
    {
        var koc = new KeyOrChar('x');

        var result = koc.ToString();

        Assert.Contains("x", result);
    }

    #endregion

    #region Array/Collection Usage

    [Fact]
    public void Array_CanContainMixedKeyAndChar()
    {
        KeyOrChar[] sequence = [Keys.ControlX, 'c'];

        Assert.Equal(2, sequence.Length);
        Assert.True(sequence[0].IsKey);
        Assert.True(sequence[1].IsChar);
        Assert.Equal(Keys.ControlX, sequence[0].Key);
        Assert.Equal('c', sequence[1].Char);
    }

    [Fact]
    public void List_CanContainMixedKeyAndChar()
    {
        List<KeyOrChar> sequence = [Keys.Escape, '[', 'A'];

        Assert.Equal(3, sequence.Count);
        Assert.True(sequence[0].IsKey);
        Assert.True(sequence[1].IsChar);
        Assert.True(sequence[2].IsChar);
    }

    #endregion

    #region Default Value

    [Fact]
    public void Default_IsNotKeyNorChar()
    {
        KeyOrChar koc = default;

        // Default struct value should behave consistently
        Assert.False(koc.IsKey);
        Assert.False(koc.IsChar);
    }

    [Fact]
    public void Default_Key_ThrowsInvalidOperationException()
    {
        KeyOrChar koc = default;

        Assert.Throws<InvalidOperationException>(() => koc.Key);
    }

    [Fact]
    public void Default_Char_ThrowsInvalidOperationException()
    {
        KeyOrChar koc = default;

        Assert.Throws<InvalidOperationException>(() => koc.Char);
    }

    #endregion
}
