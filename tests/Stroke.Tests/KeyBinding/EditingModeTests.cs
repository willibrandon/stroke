using Stroke.KeyBinding;
using Xunit;

namespace Stroke.Tests.KeyBinding;

/// <summary>
/// Tests for the <see cref="EditingMode"/> enum.
/// </summary>
public class EditingModeTests
{
    [Fact]
    public void EditingMode_HasExactlyTwoValues()
    {
        // Per SC-001: EditingMode must have exactly 2 values
        var values = Enum.GetValues<EditingMode>();

        Assert.Equal(2, values.Length);
    }

    [Fact]
    public void EditingMode_ContainsViValue()
    {
        // Per US3.1: Vi value must exist
        Assert.True(Enum.IsDefined(typeof(EditingMode), EditingMode.Vi));
    }

    [Fact]
    public void EditingMode_ContainsEmacsValue()
    {
        // Per US3.2: Emacs value must exist
        Assert.True(Enum.IsDefined(typeof(EditingMode), EditingMode.Emacs));
    }

    [Fact]
    public void EditingMode_ViAndEmacs_AreDifferentValues()
    {
        // Ensure Vi and Emacs are distinct
        Assert.NotEqual(EditingMode.Vi, EditingMode.Emacs);
    }

    [Fact]
    public void EditingMode_CanAssignToVariable()
    {
        // Basic usage test
        EditingMode mode = EditingMode.Vi;
        Assert.Equal(EditingMode.Vi, mode);

        mode = EditingMode.Emacs;
        Assert.Equal(EditingMode.Emacs, mode);
    }

    [Fact]
    public void EditingMode_CanUseInSwitchExpression()
    {
        // Verify enum can be used in switch expressions
        static string GetModeDescription(EditingMode mode) => mode switch
        {
            EditingMode.Vi => "Vi-style modal editing",
            EditingMode.Emacs => "Emacs-style chord editing",
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };

        Assert.Equal("Vi-style modal editing", GetModeDescription(EditingMode.Vi));
        Assert.Equal("Emacs-style chord editing", GetModeDescription(EditingMode.Emacs));
    }
}
