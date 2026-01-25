// Copyright (c) 2025 Brandon Pugh. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Stroke.Input;
using Xunit;

namespace Stroke.Tests.Input;

/// <summary>
/// Tests for the <see cref="AllKeys"/> static class.
/// </summary>
public class AllKeysTests
{
    // T048: Values contains 151 entries (matching Python Prompt Toolkit)
    [Fact]
    public void Values_Contains151Entries()
    {
        Assert.Equal(151, AllKeys.Values.Count);
    }

    // T049: Values contains all canonical key strings (spot check)
    [Fact]
    public void Values_ContainsAllCanonicalKeyStrings()
    {
        var values = AllKeys.Values;

        // Escape keys
        Assert.Contains("escape", values);
        Assert.Contains("s-escape", values);

        // Control characters
        Assert.Contains("c-@", values);
        Assert.Contains("c-a", values);
        Assert.Contains("c-z", values);
        Assert.Contains("c-\\", values);
        Assert.Contains("c-]", values);
        Assert.Contains("c-^", values);
        Assert.Contains("c-_", values);

        // Control + Numbers
        Assert.Contains("c-0", values);
        Assert.Contains("c-9", values);

        // Control + Shift + Numbers
        Assert.Contains("c-s-0", values);
        Assert.Contains("c-s-9", values);

        // Navigation
        Assert.Contains("left", values);
        Assert.Contains("right", values);
        Assert.Contains("up", values);
        Assert.Contains("down", values);
        Assert.Contains("home", values);
        Assert.Contains("end", values);
        Assert.Contains("insert", values);
        Assert.Contains("delete", values);
        Assert.Contains("pageup", values);
        Assert.Contains("pagedown", values);

        // Control + Navigation
        Assert.Contains("c-left", values);
        Assert.Contains("c-pagedown", values);

        // Shift + Navigation
        Assert.Contains("s-left", values);
        Assert.Contains("s-pagedown", values);

        // Control + Shift + Navigation
        Assert.Contains("c-s-left", values);
        Assert.Contains("c-s-pagedown", values);

        // BackTab
        Assert.Contains("s-tab", values);

        // Function keys
        Assert.Contains("f1", values);
        Assert.Contains("f24", values);

        // Control + Function keys
        Assert.Contains("c-f1", values);
        Assert.Contains("c-f24", values);

        // Special keys
        Assert.Contains("<any>", values);
        Assert.Contains("<scroll-up>", values);
        Assert.Contains("<scroll-down>", values);
        Assert.Contains("<cursor-position-response>", values);
        Assert.Contains("<vt100-mouse-event>", values);
        Assert.Contains("<windows-mouse-event>", values);
        Assert.Contains("<bracketed-paste>", values);
        Assert.Contains("<sigint>", values);
        Assert.Contains("<ignore>", values);
    }

    // T050: Values count matches Keys enum count
    [Fact]
    public void Values_MatchesKeysEnumCount()
    {
        var enumCount = Enum.GetValues<Keys>().Length;
        var valuesCount = AllKeys.Values.Count;

        Assert.Equal(enumCount, valuesCount);
    }

    // T051: All strings exist in ToKeyString output
    [Fact]
    public void Values_AllStringsExistInToKeyStringOutput()
    {
        var toKeyStringOutputs = Enum.GetValues<Keys>()
            .Select(k => k.ToKeyString())
            .ToHashSet();

        foreach (var keyString in AllKeys.Values)
        {
            Assert.Contains(keyString, toKeyStringOutputs);
        }
    }

    // T052: Values is read-only
    [Fact]
    public void Values_IsReadOnly()
    {
        var values = AllKeys.Values;

        // IReadOnlyList doesn't have mutating methods, so verify the type
        Assert.IsAssignableFrom<IReadOnlyList<string>>(values);

        // Verify we get the same instance (immutable singleton)
        var values2 = AllKeys.Values;
        Assert.Same(values, values2);
    }

    // Additional: Verify no duplicates
    [Fact]
    public void Values_NoDuplicates()
    {
        var values = AllKeys.Values;
        var distinctCount = values.Distinct().Count();

        Assert.Equal(values.Count, distinctCount);
    }

    // Additional: Verify alphabetical ordering is NOT required (order matches enum definition)
    [Fact]
    public void Values_FirstIsEscape_LastIsIgnore()
    {
        var values = AllKeys.Values;

        Assert.Equal("escape", values[0]);
        Assert.Equal("<ignore>", values[^1]);
    }
}
