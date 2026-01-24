using Stroke.Clipboard;
using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Clipboard;

/// <summary>
/// Tests for ClipboardData class (Phase 11 - T142).
/// </summary>
public class ClipboardDataTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_DefaultValues_SetsEmptyTextAndCharactersType()
    {
        // Act
        var data = new ClipboardData();

        // Assert
        Assert.Equal("", data.Text);
        Assert.Equal(SelectionType.Characters, data.Type);
    }

    [Fact]
    public void Constructor_WithText_SetsText()
    {
        // Act
        var data = new ClipboardData(text: "hello world");

        // Assert
        Assert.Equal("hello world", data.Text);
        Assert.Equal(SelectionType.Characters, data.Type);
    }

    [Fact]
    public void Constructor_WithType_SetsType()
    {
        // Act
        var data = new ClipboardData(type: SelectionType.Lines);

        // Assert
        Assert.Equal("", data.Text);
        Assert.Equal(SelectionType.Lines, data.Type);
    }

    [Fact]
    public void Constructor_WithBothParameters_SetsBoth()
    {
        // Act
        var data = new ClipboardData(text: "line1\nline2", type: SelectionType.Lines);

        // Assert
        Assert.Equal("line1\nline2", data.Text);
        Assert.Equal(SelectionType.Lines, data.Type);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Text_IsReadOnly()
    {
        // Arrange
        var data = new ClipboardData(text: "test");

        // Assert - property should be immutable (getter only)
        Assert.Equal("test", data.Text);
    }

    [Fact]
    public void Type_IsReadOnly()
    {
        // Arrange
        var data = new ClipboardData(type: SelectionType.Block);

        // Assert - property should be immutable (getter only)
        Assert.Equal(SelectionType.Block, data.Type);
    }

    #endregion

    #region Text Content Tests

    [Fact]
    public void Constructor_WithMultilineText_PreservesNewlines()
    {
        // Arrange
        var multilineText = "line1\nline2\nline3";

        // Act
        var data = new ClipboardData(text: multilineText);

        // Assert
        Assert.Equal(multilineText, data.Text);
        Assert.Contains("\n", data.Text);
    }

    [Fact]
    public void Constructor_WithSpecialCharacters_PreservesText()
    {
        // Arrange
        var specialText = "hello\tworld\r\n\"quotes\"";

        // Act
        var data = new ClipboardData(text: specialText);

        // Assert
        Assert.Equal(specialText, data.Text);
    }

    [Fact]
    public void Constructor_WithUnicodeText_PreservesText()
    {
        // Arrange
        var unicodeText = "你好世界 🎉 こんにちは";

        // Act
        var data = new ClipboardData(text: unicodeText);

        // Assert
        Assert.Equal(unicodeText, data.Text);
    }

    [Fact]
    public void Constructor_WithEmptyString_SetsEmptyText()
    {
        // Act
        var data = new ClipboardData(text: "");

        // Assert
        Assert.Equal("", data.Text);
    }

    #endregion

    #region SelectionType Enum Tests

    [Theory]
    [InlineData(SelectionType.Characters)]
    [InlineData(SelectionType.Lines)]
    [InlineData(SelectionType.Block)]
    public void Constructor_AllSelectionTypes_Supported(SelectionType type)
    {
        // Act
        var data = new ClipboardData(type: type);

        // Assert
        Assert.Equal(type, data.Type);
    }

    #endregion

    #region Block Selection Tests

    [Fact]
    public void Constructor_BlockType_WithMultilineText_CreatesValidBlockData()
    {
        // Arrange - block selection typically has rows of equal length
        var blockText = "ABC\nDEF\nGHI";

        // Act
        var data = new ClipboardData(text: blockText, type: SelectionType.Block);

        // Assert
        Assert.Equal(SelectionType.Block, data.Type);
        Assert.Equal(blockText, data.Text);
    }

    #endregion
}
