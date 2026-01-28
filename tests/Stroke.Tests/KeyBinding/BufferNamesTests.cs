using Stroke.KeyBinding;
using Xunit;

namespace Stroke.Tests.KeyBinding;

/// <summary>
/// Tests for the <see cref="BufferNames"/> static class.
/// </summary>
public class BufferNamesTests
{
    [Fact]
    public void BufferNames_SearchBuffer_HasCorrectValue()
    {
        Assert.Equal("SEARCH_BUFFER", BufferNames.SearchBuffer);
    }

    [Fact]
    public void BufferNames_DefaultBuffer_HasCorrectValue()
    {
        Assert.Equal("DEFAULT_BUFFER", BufferNames.DefaultBuffer);
    }

    [Fact]
    public void BufferNames_SystemBuffer_HasCorrectValue()
    {
        Assert.Equal("SYSTEM_BUFFER", BufferNames.SystemBuffer);
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
            BufferNames.SearchBuffer,
            BufferNames.DefaultBuffer,
            BufferNames.SystemBuffer
        };

        var uniqueCount = values.Distinct().Count();

        Assert.Equal(values.Length, uniqueCount);
    }

    [Fact]
    public void BufferNames_ConstantsAreNotNullOrEmpty()
    {
        Assert.False(string.IsNullOrEmpty(BufferNames.SearchBuffer));
        Assert.False(string.IsNullOrEmpty(BufferNames.DefaultBuffer));
        Assert.False(string.IsNullOrEmpty(BufferNames.SystemBuffer));
    }

    [Fact]
    public void BufferNames_CanBeUsedAsDictionaryKeys()
    {
        var buffers = new Dictionary<string, object>
        {
            { BufferNames.SearchBuffer, new object() },
            { BufferNames.DefaultBuffer, new object() },
            { BufferNames.SystemBuffer, new object() }
        };

        Assert.Equal(3, buffers.Count);
        Assert.True(buffers.ContainsKey(BufferNames.SearchBuffer));
    }

    [Fact]
    public void BufferNames_ConstantsMatchPythonValues()
    {
        // Verify exact match with Python Prompt Toolkit values
        // From prompt_toolkit/enums.py
        Assert.Equal("SEARCH_BUFFER", BufferNames.SearchBuffer);
        Assert.Equal("DEFAULT_BUFFER", BufferNames.DefaultBuffer);
        Assert.Equal("SYSTEM_BUFFER", BufferNames.SystemBuffer);
    }
}
