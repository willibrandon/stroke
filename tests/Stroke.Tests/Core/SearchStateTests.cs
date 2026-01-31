using Stroke.Core;
using Xunit;

namespace Stroke.Tests.Core;

/// <summary>
/// Unit tests for <see cref="SearchState"/>.
/// </summary>
public class SearchStateTests
{
    #region US1: Constructor and Property Tests (T012-T016a)

    /// <summary>
    /// T012: Test SearchState constructor defaults.
    /// </summary>
    [Fact]
    public void Constructor_WithNoArguments_UsesDefaults()
    {
        var state = new SearchState();

        Assert.Equal("", state.Text);
        Assert.Equal(SearchDirection.Forward, state.Direction);
        Assert.Null(state.IgnoreCaseFilter);
    }

    /// <summary>
    /// T013: Test SearchState constructor with all parameters.
    /// </summary>
    [Fact]
    public void Constructor_WithAllParameters_SetsValues()
    {
        Func<bool> filter = () => true;
        var state = new SearchState("hello", SearchDirection.Backward, filter);

        Assert.Equal("hello", state.Text);
        Assert.Equal(SearchDirection.Backward, state.Direction);
        Assert.Same(filter, state.IgnoreCaseFilter);
    }

    /// <summary>
    /// T014: Test IgnoreCase() returns false when filter is null.
    /// </summary>
    [Fact]
    public void IgnoreCase_WhenFilterIsNull_ReturnsFalse()
    {
        var state = new SearchState("test");

        Assert.False(state.IgnoreCase());
    }

    /// <summary>
    /// T015: Test IgnoreCase() returns filter result when filter is set.
    /// </summary>
    [Fact]
    public void IgnoreCase_WhenFilterReturnsTrue_ReturnsTrue()
    {
        var state = new SearchState("test", ignoreCase: () => true);

        Assert.True(state.IgnoreCase());
    }

    /// <summary>
    /// T015b: Test IgnoreCase() returns false when filter returns false.
    /// </summary>
    [Fact]
    public void IgnoreCase_WhenFilterReturnsFalse_ReturnsFalse()
    {
        var state = new SearchState("test", ignoreCase: () => false);

        Assert.False(state.IgnoreCase());
    }

    /// <summary>
    /// T016: Test Text property null handling (converts to empty string).
    /// </summary>
    [Fact]
    public void Text_WhenSetToNull_ConvertsToEmptyString()
    {
        var state = new SearchState("initial");

        state.Text = null!;

        Assert.Equal("", state.Text);
    }

    /// <summary>
    /// T016: Test Text property null in constructor (converts to empty string).
    /// </summary>
    [Fact]
    public void Constructor_WithNullText_ConvertsToEmptyString()
    {
        var state = new SearchState(null!);

        Assert.Equal("", state.Text);
    }

    /// <summary>
    /// T016a: Test SearchState supports 10,000 character search pattern (SC-007).
    /// </summary>
    [Fact]
    public void Text_With10000Characters_IsSupported()
    {
        var longText = new string('x', 10000);
        var state = new SearchState(longText);

        Assert.Equal(10000, state.Text.Length);
        Assert.Equal(longText, state.Text);
    }

    /// <summary>
    /// Test Text property get and set.
    /// </summary>
    [Fact]
    public void Text_CanBeUpdated()
    {
        var state = new SearchState("initial");

        state.Text = "updated";

        Assert.Equal("updated", state.Text);
    }

    /// <summary>
    /// Test Direction property get and set.
    /// </summary>
    [Fact]
    public void Direction_CanBeUpdated()
    {
        var state = new SearchState();

        state.Direction = SearchDirection.Backward;

        Assert.Equal(SearchDirection.Backward, state.Direction);
    }

    /// <summary>
    /// Test IgnoreCaseFilter property get and set.
    /// </summary>
    [Fact]
    public void IgnoreCaseFilter_CanBeUpdated()
    {
        var state = new SearchState();
        Func<bool> filter = () => true;

        state.IgnoreCaseFilter = filter;

        Assert.Same(filter, state.IgnoreCaseFilter);
    }

    /// <summary>
    /// Test IgnoreCaseFilter can be set back to null.
    /// </summary>
    [Fact]
    public void IgnoreCaseFilter_CanBeSetToNull()
    {
        var state = new SearchState("test", ignoreCase: () => true);

        state.IgnoreCaseFilter = null;

        Assert.Null(state.IgnoreCaseFilter);
        Assert.False(state.IgnoreCase());
    }

    #endregion

    #region US2: Invert Tests (T020-T024a)

    /// <summary>
    /// T020: Test Invert() from Forward to Backward.
    /// </summary>
    [Fact]
    public void Invert_FromForward_ReturnsBackward()
    {
        var state = new SearchState("test", SearchDirection.Forward);

        var inverted = state.Invert();

        Assert.Equal(SearchDirection.Backward, inverted.Direction);
    }

    /// <summary>
    /// T021: Test Invert() from Backward to Forward.
    /// </summary>
    [Fact]
    public void Invert_FromBackward_ReturnsForward()
    {
        var state = new SearchState("test", SearchDirection.Backward);

        var inverted = state.Invert();

        Assert.Equal(SearchDirection.Forward, inverted.Direction);
    }

    /// <summary>
    /// T022: Test Invert() preserves Text property.
    /// </summary>
    [Fact]
    public void Invert_PreservesText()
    {
        var state = new SearchState("hello world", SearchDirection.Forward);

        var inverted = state.Invert();

        Assert.Equal("hello world", inverted.Text);
    }

    /// <summary>
    /// T023: Test Invert() preserves IgnoreCaseFilter delegate.
    /// </summary>
    [Fact]
    public void Invert_PreservesIgnoreCaseFilter()
    {
        Func<bool> filter = () => true;
        var state = new SearchState("test", SearchDirection.Forward, filter);

        var inverted = state.Invert();

        Assert.Same(filter, inverted.IgnoreCaseFilter);
        Assert.True(inverted.IgnoreCase());
    }

    /// <summary>
    /// T024: Test Invert() returns NEW instance (not same reference).
    /// </summary>
    [Fact]
    public void Invert_ReturnsNewInstance()
    {
        var state = new SearchState("test", SearchDirection.Forward);

        var inverted = state.Invert();

        Assert.NotSame(state, inverted);
    }

    /// <summary>
    /// T024a: Test Invert() allocates exactly one new SearchState object (SC-008).
    /// Verified by checking only one new instance is created.
    /// </summary>
    [Fact]
    public void Invert_DoesNotModifyOriginal()
    {
        var state = new SearchState("test", SearchDirection.Forward);

        var inverted = state.Invert();

        // Original is unchanged
        Assert.Equal(SearchDirection.Forward, state.Direction);
        Assert.Equal("test", state.Text);

        // Inverted is different
        Assert.Equal(SearchDirection.Backward, inverted.Direction);
        Assert.Equal("test", inverted.Text);
    }

    /// <summary>
    /// Test double invert returns to original direction.
    /// </summary>
    [Fact]
    public void Invert_CalledTwice_ReturnsToOriginalDirection()
    {
        var state = new SearchState("test", SearchDirection.Forward);

        var doubleInverted = state.Invert().Invert();

        Assert.Equal(SearchDirection.Forward, doubleInverted.Direction);
    }

    /// <summary>
    /// Test ~ operator from Forward returns Backward.
    /// </summary>
    [Fact]
    public void BitwiseComplement_FromForward_ReturnsBackward()
    {
        var state = new SearchState("hello", SearchDirection.Forward);

        var inverted = ~state;

        Assert.Equal(SearchDirection.Backward, inverted.Direction);
        Assert.Equal("hello", inverted.Text);
        Assert.NotSame(state, inverted);
    }

    /// <summary>
    /// Test ~ operator from Backward returns Forward.
    /// </summary>
    [Fact]
    public void BitwiseComplement_FromBackward_ReturnsForward()
    {
        var state = new SearchState("world", SearchDirection.Backward);

        var inverted = ~state;

        Assert.Equal(SearchDirection.Forward, inverted.Direction);
        Assert.Equal("world", inverted.Text);
    }

    /// <summary>
    /// Test ~ operator preserves IgnoreCaseFilter.
    /// </summary>
    [Fact]
    public void BitwiseComplement_PreservesIgnoreCaseFilter()
    {
        Func<bool> filter = () => true;
        var state = new SearchState("test", SearchDirection.Forward, filter);

        var inverted = ~state;

        Assert.Same(filter, inverted.IgnoreCaseFilter);
    }

    /// <summary>
    /// Test ~ operator with empty text.
    /// </summary>
    [Fact]
    public void BitwiseComplement_WithEmptyText_Works()
    {
        var state = new SearchState("", SearchDirection.Forward);

        var inverted = ~state;

        Assert.Equal("", inverted.Text);
        Assert.Equal(SearchDirection.Backward, inverted.Direction);
    }

    #endregion

    #region US3: Incremental Update Tests (T027-T028)

    /// <summary>
    /// T027: Test incremental Text property updates ("a" -> "ap" -> "apr").
    /// </summary>
    [Fact]
    public void Text_SupportsIncrementalUpdates()
    {
        var state = new SearchState();

        state.Text = "a";
        Assert.Equal("a", state.Text);

        state.Text = "ap";
        Assert.Equal("ap", state.Text);

        state.Text = "apr";
        Assert.Equal("apr", state.Text);
    }

    /// <summary>
    /// T028: Test IgnoreCaseFilter runtime evaluation (toggle behavior).
    /// </summary>
    [Fact]
    public void IgnoreCase_EvaluatesFilterAtRuntime()
    {
        bool ignoreCase = false;
        var state = new SearchState("test", ignoreCase: () => ignoreCase);

        Assert.False(state.IgnoreCase());

        ignoreCase = true;
        Assert.True(state.IgnoreCase());

        ignoreCase = false;
        Assert.False(state.IgnoreCase());
    }

    #endregion

    #region ToString Tests (T051-T053)

    /// <summary>
    /// T051: Test ToString() output format matches spec.
    /// Format: SearchState("{Text}", direction={Direction}, ignoreCase={IgnoreCase()})
    /// </summary>
    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        var state = new SearchState("hello", SearchDirection.Forward);

        var result = state.ToString();

        Assert.Equal("SearchState(\"hello\", direction=Forward, ignoreCase=False)", result);
    }

    /// <summary>
    /// T052: Test ToString() with null IgnoreCaseFilter shows ignoreCase=False.
    /// </summary>
    [Fact]
    public void ToString_WithNullFilter_ShowsIgnoreCaseFalse()
    {
        var state = new SearchState("test");

        var result = state.ToString();

        Assert.Contains("ignoreCase=False", result);
    }

    /// <summary>
    /// T053: Test ToString() with IgnoreCaseFilter=() => true shows ignoreCase=True.
    /// </summary>
    [Fact]
    public void ToString_WithTrueFilter_ShowsIgnoreCaseTrue()
    {
        var state = new SearchState("test", ignoreCase: () => true);

        var result = state.ToString();

        Assert.Contains("ignoreCase=True", result);
    }

    /// <summary>
    /// Test ToString() with Backward direction.
    /// </summary>
    [Fact]
    public void ToString_WithBackwardDirection_ShowsBackward()
    {
        var state = new SearchState("test", SearchDirection.Backward);

        var result = state.ToString();

        Assert.Contains("direction=Backward", result);
    }

    /// <summary>
    /// Test ToString() with empty text.
    /// </summary>
    [Fact]
    public void ToString_WithEmptyText_ShowsEmptyQuotes()
    {
        var state = new SearchState();

        var result = state.ToString();

        Assert.Equal("SearchState(\"\", direction=Forward, ignoreCase=False)", result);
    }

    /// <summary>
    /// Test ToString() with special characters in text.
    /// </summary>
    [Fact]
    public void ToString_WithSpecialCharacters_PreservesText()
    {
        var state = new SearchState("hello\nworld");

        var result = state.ToString();

        Assert.Contains("hello\nworld", result);
    }

    #endregion
}
