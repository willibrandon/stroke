using Stroke.Application;
using Xunit;

namespace Stroke.Tests.Application;

/// <summary>
/// Tests for ViFilters.ViInsertMultipleMode (T047).
/// </summary>
public class AppFiltersProcessorTests
{
    [Fact]
    public void ViInsertMultipleMode_FalseWithDummyApp()
    {
        // DummyApplication uses Emacs editing mode by default,
        // so ViInsertMultipleMode should return false
        Assert.False(ViFilters.ViInsertMultipleMode.Invoke());
    }

    [Fact]
    public void ViInsertMultipleMode_IsIFilter()
    {
        Assert.IsAssignableFrom<Stroke.Filters.IFilter>(ViFilters.ViInsertMultipleMode);
    }

    [Fact]
    public void ViInsertMultipleMode_NotNull()
    {
        Assert.NotNull(ViFilters.ViInsertMultipleMode);
    }
}
