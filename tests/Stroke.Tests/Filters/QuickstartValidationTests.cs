using Stroke.Filters;
using Xunit;

namespace Stroke.Tests.Filters;

/// <summary>
/// Validation tests for quickstart.md code examples.
/// Ensures all examples in quickstart.md compile and execute correctly.
/// </summary>
public sealed class QuickstartValidationTests
{
    #region Basic Filter Usage Examples

    [Fact]
    public void BasicUsage_DynamicCondition_Works()
    {
        // From quickstart.md: "Basic Filter Usage"
        var _state = new { IsActive = true };
        var isActive = new Condition(() => _state.IsActive);

        // Evaluate the filter
        bool active = isActive.Invoke();

        Assert.True(active);
    }

    [Fact]
    public void BasicUsage_ConstantFilters_Works()
    {
        // From quickstart.md: "Basic Filter Usage"
        bool always = Always.Instance.Invoke();
        bool never = Never.Instance.Invoke();

        Assert.True(always);
        Assert.False(never);
    }

    #endregion

    #region Combining Filters Examples

    [Fact]
    public void CombiningFilters_And_Works()
    {
        // From quickstart.md: "Combining Filters"
        var _hasFocus = true;
        var _isReadOnly = false;

        var canEdit = new Condition(() => _hasFocus) & new Condition(() => !_isReadOnly);

        Assert.True(canEdit.Invoke());
    }

    [Fact]
    public void CombiningFilters_Or_Works()
    {
        // From quickstart.md: "Combining Filters"
        var _isNewUser = false;
        var _helpRequested = true;

        var showHelp = new Condition(() => _isNewUser) | new Condition(() => _helpRequested);

        Assert.True(showHelp.Invoke());
    }

    [Fact]
    public void CombiningFilters_Negation_Works()
    {
        // From quickstart.md: "Combining Filters"
        var _isSearching = true;

        var notSearching = ~new Condition(() => _isSearching);

        Assert.False(notSearching.Invoke());
    }

    [Fact]
    public void CombiningFilters_Complex_Works()
    {
        // From quickstart.md: "Combining Filters"
        var _hasFocus = true;
        var _isReadOnly = false;
        var _isNewUser = true;
        var _helpRequested = false;
        var _isSearching = false;

        var canEdit = new Condition(() => _hasFocus) & new Condition(() => !_isReadOnly);
        var showHelp = new Condition(() => _isNewUser) | new Condition(() => _helpRequested);
        var notSearching = ~new Condition(() => _isSearching);

        // Need to cast to Filter since operators are on Filter, not IFilter
        var shouldActivate = (Filter)((Filter)canEdit | showHelp) & notSearching;

        Assert.True(shouldActivate.Invoke());
    }

    #endregion

    #region FilterOrBool API Examples

    [Fact]
    public void FilterOrBoolApi_WithBoolean_Works()
    {
        // From quickstart.md: "Using FilterOrBool in APIs"
        int callCount = 0;
        void SetVisibility(FilterOrBool visible)
        {
            var filter = FilterUtils.ToFilter(visible);
            callCount++;
            _ = filter.Invoke(); // Use the filter
        }

        // Call with boolean
        SetVisibility(true);
        SetVisibility(false);

        Assert.Equal(2, callCount);
    }

    [Fact]
    public void FilterOrBoolApi_WithFilter_Works()
    {
        // From quickstart.md: "Using FilterOrBool in APIs"
        IFilter? capturedFilter = null;
        void SetVisibility(FilterOrBool visible)
        {
            capturedFilter = FilterUtils.ToFilter(visible);
        }

        var _isVisible = true;

        // Call with filter
        SetVisibility(new Condition(() => _isVisible));

        Assert.NotNull(capturedFilter);
        Assert.True(capturedFilter!.Invoke());
    }

    [Fact]
    public void FilterOrBoolApi_WithAlways_Works()
    {
        // From quickstart.md: "Using FilterOrBool in APIs"
        IFilter? capturedFilter = null;
        void SetVisibility(FilterOrBool visible)
        {
            capturedFilter = FilterUtils.ToFilter(visible);
        }

        SetVisibility(Always.Instance);

        Assert.Same(Always.Instance, capturedFilter);
    }

    [Fact]
    public void FilterOrBoolApi_QuickEvaluation_Works()
    {
        // From quickstart.md: "Using FilterOrBool in APIs"
        FilterOrBool visible = new Condition(() => true);

        bool shouldShow = FilterUtils.IsTrue(visible);

        Assert.True(shouldShow);
    }

    #endregion

    #region Algebraic Properties Examples

    [Fact]
    public void AlgebraicProperties_AlwaysIsIdentityForAnd_Works()
    {
        // From quickstart.md: "Algebraic Properties"
        var x = new Condition(() => true);

        var result = Always.Instance & x;

        Assert.Same(x, result);
    }

    [Fact]
    public void AlgebraicProperties_NeverIsIdentityForOr_Works()
    {
        // From quickstart.md: "Algebraic Properties"
        var x = new Condition(() => true);

        var result = Never.Instance | x;

        Assert.Same(x, result);
    }

    [Fact]
    public void AlgebraicProperties_AlwaysIsAnnihilatorForOr_Works()
    {
        // From quickstart.md: "Algebraic Properties"
        var x = new Condition(() => false);

        var result = Always.Instance | x;

        Assert.Same(Always.Instance, result);
    }

    [Fact]
    public void AlgebraicProperties_NeverIsAnnihilatorForAnd_Works()
    {
        // From quickstart.md: "Algebraic Properties"
        var x = new Condition(() => true);

        var result = Never.Instance & x;

        Assert.Same(Never.Instance, result);
    }

    [Fact]
    public void AlgebraicProperties_DoubleNegation_Works()
    {
        // From quickstart.md: "Algebraic Properties"
        var x = new Condition(() => true);

        // ~~x.Invoke() == x.Invoke()
        var doubleInverted = x.Invert().Invert();

        Assert.Equal(x.Invoke(), doubleInverted.Invoke());
    }

    [Fact]
    public void AlgebraicProperties_NegationOfConstants_Works()
    {
        // From quickstart.md: "Algebraic Properties"
        // ~Always.Instance == Never.Instance
        Assert.Same(Never.Instance, Always.Instance.Invert());

        // ~Never.Instance == Always.Instance
        Assert.Same(Always.Instance, Never.Instance.Invert());
    }

    #endregion

    #region Common Patterns Examples

    [Fact]
    public void CommonPatterns_FeatureFlags_Works()
    {
        // From quickstart.md: "Common Patterns - Feature Flags"
        var config = new { EnableNewFeature = true };
        var featureEnabled = new Condition(() => config.EnableNewFeature);

        bool activated = false;
        if (featureEnabled.Invoke())
        {
            activated = true;
        }

        Assert.True(activated);
    }

    [Fact]
    public void CommonPatterns_CombiningApplicationState_Works()
    {
        // From quickstart.md: "Common Patterns - Combining Application State"
        var _document = new { IsDirty = true, IsReadOnly = false };
        var _hasWritePermission = true;

        // Chaining requires casting intermediate results since & returns IFilter
        var canSave =
            (Filter)(new Condition(() => _document.IsDirty) &
            new Condition(() => !_document.IsReadOnly)) &
            new Condition(() => _hasWritePermission);

        Assert.True(canSave.Invoke());
    }

    #endregion
}
