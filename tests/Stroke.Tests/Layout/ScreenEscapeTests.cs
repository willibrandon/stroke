using Stroke.Layout;
using Xunit;

namespace Stroke.Tests.Layout;

/// <summary>
/// Tests for Screen zero-width escape sequences (User Story 4).
/// </summary>
public class ScreenEscapeTests
{
    [Fact]
    public void AddZeroWidthEscape_SingleEscape_StoredCorrectly()
    {
        var screen = new Screen();
        var escape = "\x1b]8;;https://example.com\x1b\\";

        screen.AddZeroWidthEscape(5, 10, escape);

        Assert.Equal(escape, screen.GetZeroWidthEscapes(5, 10));
    }

    [Fact]
    public void AddZeroWidthEscape_MultipleAtSamePosition_Concatenated()
    {
        var screen = new Screen();
        var escape1 = "\x1b]8;;https://example.com\x1b\\";
        var escape2 = "\x1b]8;;\x1b\\";

        screen.AddZeroWidthEscape(5, 10, escape1);
        screen.AddZeroWidthEscape(5, 10, escape2);

        Assert.Equal(escape1 + escape2, screen.GetZeroWidthEscapes(5, 10));
    }

    [Fact]
    public void GetZeroWidthEscapes_UnsetPosition_ReturnsEmptyString()
    {
        var screen = new Screen();

        Assert.Equal(string.Empty, screen.GetZeroWidthEscapes(5, 10));
    }

    [Fact]
    public void AddZeroWidthEscape_EmptyString_Ignored()
    {
        var screen = new Screen();

        screen.AddZeroWidthEscape(5, 10, "");

        Assert.Equal(string.Empty, screen.GetZeroWidthEscapes(5, 10));
    }

    [Fact]
    public void AddZeroWidthEscape_NullString_ThrowsArgumentNullException()
    {
        var screen = new Screen();

        Assert.Throws<ArgumentNullException>(() =>
            screen.AddZeroWidthEscape(5, 10, null!));
    }

    [Fact]
    public void AddZeroWidthEscape_DifferentPositions_IndependentStorage()
    {
        var screen = new Screen();
        var escape1 = "escape1";
        var escape2 = "escape2";

        screen.AddZeroWidthEscape(0, 0, escape1);
        screen.AddZeroWidthEscape(1, 1, escape2);

        Assert.Equal(escape1, screen.GetZeroWidthEscapes(0, 0));
        Assert.Equal(escape2, screen.GetZeroWidthEscapes(1, 1));
        Assert.Equal(string.Empty, screen.GetZeroWidthEscapes(0, 1));
    }

    [Fact]
    public void AddZeroWidthEscape_NegativeCoords_Valid()
    {
        var screen = new Screen();
        var escape = "escape";

        screen.AddZeroWidthEscape(-5, -10, escape);

        Assert.Equal(escape, screen.GetZeroWidthEscapes(-5, -10));
    }

    [Fact]
    public void AddZeroWidthEscape_DoesNotAffectCharacter()
    {
        var screen = new Screen();
        var ch = Stroke.Layout.Char.Create("A", "class:test");
        var escape = "\x1b]8;;link\x1b\\";

        screen[5, 10] = ch;
        screen.AddZeroWidthEscape(5, 10, escape);

        // Character should be unchanged
        Assert.Equal(ch, screen[5, 10]);
        // Escape should be stored separately
        Assert.Equal(escape, screen.GetZeroWidthEscapes(5, 10));
    }

    [Fact]
    public void AddZeroWidthEscape_MultipleConcat_PreservesOrder()
    {
        var screen = new Screen();

        screen.AddZeroWidthEscape(0, 0, "first");
        screen.AddZeroWidthEscape(0, 0, "second");
        screen.AddZeroWidthEscape(0, 0, "third");

        Assert.Equal("firstsecondthird", screen.GetZeroWidthEscapes(0, 0));
    }
}
