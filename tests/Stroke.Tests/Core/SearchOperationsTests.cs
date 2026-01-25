using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for <see cref="SearchOperations"/> stub behavior.
/// </summary>
public class SearchOperationsTests
{
    /// <summary>
    /// T031: Test StartSearch throws NotImplementedException.
    /// </summary>
    [Fact]
    public void StartSearch_ThrowsNotImplementedException()
    {
        var ex = Assert.Throws<NotImplementedException>(() =>
            SearchOperations.StartSearch());

        Assert.Contains("Feature 20", ex.Message);
        Assert.Contains("Feature 35", ex.Message);
    }

    /// <summary>
    /// T031b: Test StartSearch with direction parameter throws NotImplementedException.
    /// </summary>
    [Fact]
    public void StartSearch_WithDirection_ThrowsNotImplementedException()
    {
        var ex = Assert.Throws<NotImplementedException>(() =>
            SearchOperations.StartSearch(SearchDirection.Backward));

        Assert.Contains("Feature 20", ex.Message);
    }

    /// <summary>
    /// T032: Test StopSearch throws NotImplementedException.
    /// </summary>
    [Fact]
    public void StopSearch_ThrowsNotImplementedException()
    {
        var ex = Assert.Throws<NotImplementedException>(() =>
            SearchOperations.StopSearch());

        Assert.Contains("Feature 20", ex.Message);
        Assert.Contains("Feature 35", ex.Message);
    }

    /// <summary>
    /// T033: Test DoIncrementalSearch throws NotImplementedException.
    /// </summary>
    [Fact]
    public void DoIncrementalSearch_ThrowsNotImplementedException()
    {
        var ex = Assert.Throws<NotImplementedException>(() =>
            SearchOperations.DoIncrementalSearch(SearchDirection.Forward));

        Assert.Contains("Feature 12", ex.Message);
        Assert.Contains("Feature 20", ex.Message);
    }

    /// <summary>
    /// T033b: Test DoIncrementalSearch with count parameter throws NotImplementedException.
    /// </summary>
    [Fact]
    public void DoIncrementalSearch_WithCount_ThrowsNotImplementedException()
    {
        var ex = Assert.Throws<NotImplementedException>(() =>
            SearchOperations.DoIncrementalSearch(SearchDirection.Backward, 5));

        Assert.Contains("Feature 12", ex.Message);
    }

    /// <summary>
    /// T034: Test AcceptSearch throws NotImplementedException.
    /// </summary>
    [Fact]
    public void AcceptSearch_ThrowsNotImplementedException()
    {
        var ex = Assert.Throws<NotImplementedException>(() =>
            SearchOperations.AcceptSearch());

        Assert.Contains("Feature 20", ex.Message);
    }
}
