using Stroke.Input;
using Stroke.KeyBinding;
using Xunit;

// Use explicit alias to avoid ambiguity with Stroke.Input.KeyPress
using KeyPress = Stroke.KeyBinding.KeyPress;

namespace Stroke.Tests.KeyBinding;

/// <summary>
/// Tests for the <see cref="KeyPress"/> readonly record struct.
/// </summary>
public sealed class KeyPressTests
{
    #region Construction with Keys

    [Fact]
    public void Constructor_WithKeysOnly_UsesEnumNameAsData()
    {
        var kp = new KeyPress(Keys.ControlM);

        Assert.True(kp.Key.IsKey);
        Assert.Equal(Keys.ControlM, kp.Key.Key);
        Assert.Equal("ControlM", kp.Data);
    }

    [Fact]
    public void Constructor_WithKeysAndData_UsesProvidedData()
    {
        var kp = new KeyPress(Keys.ControlC, "\x03");

        Assert.Equal(Keys.ControlC, kp.Key.Key);
        Assert.Equal("\x03", kp.Data);
    }

    [Theory]
    [InlineData(Keys.ControlI, "ControlI")]
    [InlineData(Keys.Escape, "Escape")]
    [InlineData(Keys.ControlH, "ControlH")]
    [InlineData(Keys.F1, "F1")]
    [InlineData(Keys.ControlA, "ControlA")]
    public void Constructor_WithVariousKeys_UsesEnumNameAsDefaultData(Keys key, string expectedData)
    {
        var kp = new KeyPress(key);

        Assert.Equal(expectedData, kp.Data);
    }

    #endregion

    #region Construction with Char

    [Fact]
    public void Constructor_WithCharOnly_UsesCharAsData()
    {
        var kp = new KeyPress('a');

        Assert.True(kp.Key.IsChar);
        Assert.Equal('a', kp.Key.Char);
        Assert.Equal("a", kp.Data);
    }

    [Fact]
    public void Constructor_WithCharAndData_UsesProvidedData()
    {
        var kp = new KeyPress('x', "custom");

        Assert.Equal('x', kp.Key.Char);
        Assert.Equal("custom", kp.Data);
    }

    [Theory]
    [InlineData('a', "a")]
    [InlineData('Z', "Z")]
    [InlineData('0', "0")]
    [InlineData(' ', " ")]
    [InlineData('\n', "\n")]
    public void Constructor_WithVariousChars_UsesCharAsDefaultData(char c, string expectedData)
    {
        var kp = new KeyPress(c);

        Assert.Equal(expectedData, kp.Data);
    }

    [Fact]
    public void Constructor_WithUnicodeChar_UsesCharAsData()
    {
        var kp = new KeyPress('日');

        Assert.Equal('日', kp.Key.Char);
        Assert.Equal("日", kp.Data);
    }

    #endregion

    #region Construction with KeyOrChar

    [Fact]
    public void Constructor_WithKeyOrChar_KeyVariant()
    {
        KeyOrChar koc = Keys.F5;
        var kp = new KeyPress(koc);

        Assert.True(kp.Key.IsKey);
        Assert.Equal(Keys.F5, kp.Key.Key);
        Assert.Equal("F5", kp.Data);
    }

    [Fact]
    public void Constructor_WithKeyOrChar_CharVariant()
    {
        KeyOrChar koc = 'm';
        var kp = new KeyPress(koc);

        Assert.True(kp.Key.IsChar);
        Assert.Equal('m', kp.Key.Char);
        Assert.Equal("m", kp.Data);
    }

    [Fact]
    public void Constructor_WithKeyOrCharAndData()
    {
        KeyOrChar koc = Keys.ControlM;
        var kp = new KeyPress(koc, "\r\n");

        Assert.Equal(Keys.ControlM, kp.Key.Key);
        Assert.Equal("\r\n", kp.Data);
    }

    #endregion

    #region Value Equality

    [Fact]
    public void Equals_SameKeyAndData_ReturnsTrue()
    {
        var kp1 = new KeyPress(Keys.ControlM, "\r");
        var kp2 = new KeyPress(Keys.ControlM, "\r");

        Assert.Equal(kp1, kp2);
        Assert.True(kp1 == kp2);
        Assert.False(kp1 != kp2);
    }

    [Fact]
    public void Equals_SameKeyDifferentData_ReturnsFalse()
    {
        var kp1 = new KeyPress(Keys.ControlM, "\r");
        var kp2 = new KeyPress(Keys.ControlM, "\n");

        Assert.NotEqual(kp1, kp2);
        Assert.False(kp1 == kp2);
        Assert.True(kp1 != kp2);
    }

    [Fact]
    public void Equals_DifferentKeysSameData_ReturnsFalse()
    {
        var kp1 = new KeyPress(Keys.ControlI, "\t");
        var kp2 = new KeyPress(Keys.ControlM, "\t");

        Assert.NotEqual(kp1, kp2);
    }

    [Fact]
    public void Equals_SameCharAndData_ReturnsTrue()
    {
        var kp1 = new KeyPress('x');
        var kp2 = new KeyPress('x');

        Assert.Equal(kp1, kp2);
        Assert.True(kp1 == kp2);
    }

    [Fact]
    public void Equals_DifferentChars_ReturnsFalse()
    {
        var kp1 = new KeyPress('x');
        var kp2 = new KeyPress('y');

        Assert.NotEqual(kp1, kp2);
    }

    [Fact]
    public void GetHashCode_SameKeyAndData_ReturnsSameValue()
    {
        var kp1 = new KeyPress(Keys.ControlC, "\x03");
        var kp2 = new KeyPress(Keys.ControlC, "\x03");

        Assert.Equal(kp1.GetHashCode(), kp2.GetHashCode());
    }

    #endregion

    #region ToString

    [Fact]
    public void ToString_WithKey_IncludesKeyAndData()
    {
        var kp = new KeyPress(Keys.ControlC);

        var result = kp.ToString();

        Assert.Contains("ControlC", result);
    }

    [Fact]
    public void ToString_WithChar_IncludesCharAndData()
    {
        var kp = new KeyPress('a');

        var result = kp.ToString();

        Assert.Contains("a", result);
    }

    #endregion

    #region Data Property Behavior per FR-047

    [Fact]
    public void Data_ForKeyPress_DefaultsToEnumName()
    {
        // FR-047: For Keys enum values, data defaults to enum name
        var kp = new KeyPress(Keys.ControlH);

        Assert.Equal("ControlH", kp.Data);
    }

    [Fact]
    public void Data_ForCharPress_DefaultsToCharString()
    {
        // FR-047: For char values, data defaults to char.ToString()
        var kp = new KeyPress('q');

        Assert.Equal("q", kp.Data);
    }

    [Fact]
    public void Data_CanBeOverridden_ForKeys()
    {
        var kp = new KeyPress(Keys.ControlI, "\t");

        Assert.Equal("\t", kp.Data);
    }

    [Fact]
    public void Data_CanBeOverridden_ForChar()
    {
        var kp = new KeyPress('a', "special");

        Assert.Equal("special", kp.Data);
    }

    #endregion

    #region Collection Usage

    [Fact]
    public void List_CanStoreMultipleKeyPresses()
    {
        List<KeyPress> sequence =
        [
            new KeyPress(Keys.ControlX),
            new KeyPress(Keys.ControlC)
        ];

        Assert.Equal(2, sequence.Count);
        Assert.Equal(Keys.ControlX, sequence[0].Key.Key);
        Assert.Equal(Keys.ControlC, sequence[1].Key.Key);
    }

    [Fact]
    public void Array_CanStoreMixedKeyPresses()
    {
        KeyPress[] sequence =
        [
            new KeyPress(Keys.Escape),
            new KeyPress('['),
            new KeyPress('A')
        ];

        Assert.Equal(3, sequence.Length);
        Assert.True(sequence[0].Key.IsKey);
        Assert.True(sequence[1].Key.IsChar);
        Assert.True(sequence[2].Key.IsChar);
    }

    #endregion

    #region Implicit Conversions

    [Fact]
    public void ImplicitConversion_FromKeys_CreatesKeyPress()
    {
        KeyPress kp = Keys.Delete;

        Assert.True(kp.Key.IsKey);
        Assert.Equal(Keys.Delete, kp.Key.Key);
        Assert.Equal("Delete", kp.Data);
    }

    [Fact]
    public void ImplicitConversion_FromChar_CreatesKeyPress()
    {
        KeyPress kp = 'z';

        Assert.True(kp.Key.IsChar);
        Assert.Equal('z', kp.Key.Char);
        Assert.Equal("z", kp.Data);
    }

    [Fact]
    public void ImplicitConversion_FromKeyOrChar_CreatesKeyPress()
    {
        KeyOrChar koc = Keys.Home;
        KeyPress kp = koc;

        Assert.Equal(Keys.Home, kp.Key.Key);
        Assert.Equal("Home", kp.Data);
    }

    #endregion

    #region Default Value

    [Fact]
    public void Default_HasDefaultKeyOrChar()
    {
        KeyPress kp = default;

        // Default struct: Key is default KeyOrChar, Data is null
        Assert.False(kp.Key.IsKey);
        Assert.False(kp.Key.IsChar);
        Assert.Null(kp.Data);
    }

    #endregion
}
