using Stroke.KeyBinding;
using Xunit;

namespace Stroke.Tests.KeyBinding;

/// <summary>
/// Tests for the <see cref="BufferNames"/> static class.
/// </summary>
public class BufferNamesTests
{
    [Fact]
    public void BufferNames_Search_HasCorrectValue()
    {
        Assert.Equal("SEARCH_BUFFER", BufferNames.Search);
    }

    [Fact]
    public void BufferNames_Default_HasCorrectValue()
    {
        Assert.Equal("DEFAULT_BUFFER", BufferNames.Default);
    }

    [Fact]
    public void BufferNames_System_HasCorrectValue()
    {
        Assert.Equal("SYSTEM_BUFFER", BufferNames.System);
    }

    [Fact]
    public void BufferNames_IsStaticClass()
    {
        // Static classes cannot be instantiated
        // This test verifies the class is static by checking its type properties
        var type = typeof(BufferNames);

        Assert.True(type.IsAbstract, "Static class should be abstract");
        Assert.True(type.IsSealed, "Static class should be sealed");
    }

    [Fact]
    public void BufferNames_AllConstantsAreUnique()
    {
        var values = new[]
        {
            BufferNames.Search,
            BufferNames.Default,
            BufferNames.System
        };

        var uniqueCount = values.Distinct().Count();

        Assert.Equal(values.Length, uniqueCount);
    }

    [Fact]
    public void BufferNames_ConstantsAreNotNullOrEmpty()
    {
        Assert.False(string.IsNullOrEmpty(BufferNames.Search));
        Assert.False(string.IsNullOrEmpty(BufferNames.Default));
        Assert.False(string.IsNullOrEmpty(BufferNames.System));
    }

    [Fact]
    public void BufferNames_CanBeUsedAsDictionaryKeys()
    {
        var buffers = new Dictionary<string, object>
        {
            { BufferNames.Search, new object() },
            { BufferNames.Default, new object() },
            { BufferNames.System, new object() }
        };

        Assert.Equal(3, buffers.Count);
        Assert.True(buffers.ContainsKey(BufferNames.Search));
    }

    [Fact]
    public void BufferNames_ConstantsMatchPythonValues()
    {
        // Verify exact match with Python Prompt Toolkit values
        // From prompt_toolkit/enums.py
        Assert.Equal("SEARCH_BUFFER", BufferNames.Search);
        Assert.Equal("DEFAULT_BUFFER", BufferNames.Default);
        Assert.Equal("SYSTEM_BUFFER", BufferNames.System);
    }
}
