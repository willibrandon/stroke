namespace Stroke.Tests.Contrib.RegularLanguages;

using Stroke.Contrib.RegularLanguages;
using Xunit;

/// <summary>
/// Tests for Match, Variables, and MatchVariable classes.
/// </summary>
public class MatchVariableTests
{
    [Fact]
    public void MatchVariable_Constructor_SetsProperties()
    {
        var mv = new MatchVariable("foo", "bar", 5, 8);

        Assert.Equal("foo", mv.VarName);
        Assert.Equal("bar", mv.Value);
        Assert.Equal(5, mv.Start);
        Assert.Equal(8, mv.Stop);
    }

    [Fact]
    public void MatchVariable_SliceTupleConstructor_SetsProperties()
    {
        var mv = new MatchVariable("foo", "bar", (10, 20));

        Assert.Equal("foo", mv.VarName);
        Assert.Equal("bar", mv.Value);
        Assert.Equal(10, mv.Start);
        Assert.Equal(20, mv.Stop);
    }

    [Fact]
    public void MatchVariable_Slice_ReturnsTuple()
    {
        var mv = new MatchVariable("x", "y", 3, 7);

        Assert.Equal((3, 7), mv.Slice);
    }

    [Fact]
    public void MatchVariable_ToString_FormatsCorrectly()
    {
        var mv = new MatchVariable("name", "value", 0, 5);

        Assert.Equal("MatchVariable('name', 'value')", mv.ToString());
    }

    [Fact]
    public void MatchVariable_NullVarName_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MatchVariable(null!, "value", 0, 5));
    }

    [Fact]
    public void MatchVariable_NullValue_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MatchVariable("name", null!, 0, 5));
    }
}

public class VariablesTests
{
    [Fact]
    public void Variables_Get_ReturnsFirstValue()
    {
        var tuples = new List<(string, string, int, int)>
        {
            ("name", "first", 0, 5),
            ("name", "second", 10, 16),
            ("other", "value", 20, 25)
        };
        var vars = new Variables(tuples);

        Assert.Equal("first", vars.Get("name"));
    }

    [Fact]
    public void Variables_Get_ReturnsNullForMissing()
    {
        var vars = new Variables([]);

        Assert.Null(vars.Get("missing"));
    }

    [Fact]
    public void Variables_GetWithDefault_ReturnsDefaultForMissing()
    {
        var vars = new Variables([]);

        Assert.Equal("default", vars.Get("missing", "default"));
    }

    [Fact]
    public void Variables_GetWithDefault_ReturnsValueWhenPresent()
    {
        var tuples = new List<(string, string, int, int)>
        {
            ("key", "value", 0, 5)
        };
        var vars = new Variables(tuples);

        Assert.Equal("value", vars.Get("key", "default"));
    }

    [Fact]
    public void Variables_GetAll_ReturnsAllValues()
    {
        var tuples = new List<(string, string, int, int)>
        {
            ("name", "first", 0, 5),
            ("name", "second", 10, 16),
            ("other", "value", 20, 25)
        };
        var vars = new Variables(tuples);

        var all = vars.GetAll("name");
        Assert.Equal(2, all.Count);
        Assert.Equal("first", all[0]);
        Assert.Equal("second", all[1]);
    }

    [Fact]
    public void Variables_GetAll_ReturnsEmptyForMissing()
    {
        var vars = new Variables([]);

        var all = vars.GetAll("missing");
        Assert.Empty(all);
    }

    [Fact]
    public void Variables_Indexer_ReturnsFirstValue()
    {
        var tuples = new List<(string, string, int, int)>
        {
            ("key", "value", 0, 5)
        };
        var vars = new Variables(tuples);

        Assert.Equal("value", vars["key"]);
    }

    [Fact]
    public void Variables_Indexer_ReturnsNullForMissing()
    {
        var vars = new Variables([]);

        Assert.Null(vars["missing"]);
    }

    [Fact]
    public void Variables_GetEnumerator_YieldsMatchVariables()
    {
        var tuples = new List<(string, string, int, int)>
        {
            ("a", "1", 0, 1),
            ("b", "2", 2, 3)
        };
        var vars = new Variables(tuples);

        var list = vars.ToList();
        Assert.Equal(2, list.Count);
        Assert.Equal("a", list[0].VarName);
        Assert.Equal("1", list[0].Value);
        Assert.Equal("b", list[1].VarName);
        Assert.Equal("2", list[1].Value);
    }

    [Fact]
    public void Variables_ToString_FormatsCorrectly()
    {
        var tuples = new List<(string, string, int, int)>
        {
            ("x", "1", 0, 1),
            ("y", "2", 2, 3)
        };
        var vars = new Variables(tuples);

        Assert.Equal("Variables(x='1', y='2')", vars.ToString());
    }

    [Fact]
    public void Variables_Empty_ToString()
    {
        var vars = new Variables([]);

        Assert.Equal("Variables()", vars.ToString());
    }
}

public class MatchTests
{
    [Fact]
    public void Match_Variables_ExtractsNamedGroups()
    {
        var grammar = Grammar.Compile(@"(?P<cmd>hello)\s+(?P<name>\w+)");
        var match = grammar.Match("hello world");

        Assert.NotNull(match);
        var vars = match.Variables();
        Assert.Equal("hello", vars["cmd"]);
        Assert.Equal("world", vars["name"]);
    }

    [Fact]
    public void Match_Variables_EmptyForNoGroups()
    {
        var grammar = Grammar.Compile(@"hello");
        var match = grammar.Match("hello");

        Assert.NotNull(match);
        var vars = match.Variables();
        Assert.Empty(vars);
    }

    [Fact]
    public void Match_EndNodes_ReturnsVariablesAtEnd()
    {
        var grammar = Grammar.Compile(@"(?P<cmd>hello)\s+(?P<arg>\w+)");
        var match = grammar.MatchPrefix("hello world");

        Assert.NotNull(match);
        var endNodes = match.EndNodes().ToList();
        Assert.NotEmpty(endNodes);
        Assert.Contains(endNodes, e => e.VarName == "arg");
    }

    [Fact]
    public void Match_TrailingInput_ReturnsNullWhenNoTrailing()
    {
        var grammar = Grammar.Compile(@"hello");
        var match = grammar.Match("hello");

        Assert.NotNull(match);
        Assert.Null(match.TrailingInput());
    }

    [Fact]
    public void Match_Input_ReturnsOriginalInput()
    {
        var grammar = Grammar.Compile(@"hello");
        var match = grammar.Match("hello");

        Assert.NotNull(match);
        Assert.Equal("hello", match.Input);
    }
}

/// <summary>
/// Tests for User Story 2: Variable extraction with positions and unescape.
/// </summary>
public class VariableExtractionTests
{
    [Fact]
    public void Variables_ContainsCorrectPositions()
    {
        var grammar = Grammar.Compile(@"cmd\s(?P<arg>\w+)");
        var match = grammar.Match("cmd test");

        Assert.NotNull(match);
        var vars = match.Variables().ToList();
        Assert.Single(vars);
        Assert.Equal("arg", vars[0].VarName);
        Assert.Equal("test", vars[0].Value);
        Assert.Equal(4, vars[0].Start);
        Assert.Equal(8, vars[0].Stop);
    }

    [Fact]
    public void Variables_MultipleVariables_ExtractsAll()
    {
        var grammar = Grammar.Compile(@"(?P<a>\d+)\+(?P<b>\d+)\=(?P<c>\d+)");
        var match = grammar.Match("12+34=46");

        Assert.NotNull(match);
        var vars = match.Variables();
        Assert.Equal("12", vars["a"]);
        Assert.Equal("34", vars["b"]);
        Assert.Equal("46", vars["c"]);
    }

    [Fact]
    public void Variables_GetAll_ReturnsAllMatches()
    {
        // With repeating groups, same variable may match multiple times
        var grammar = Grammar.Compile(@"(?P<item>\w+)");
        var match = grammar.Match("hello");

        Assert.NotNull(match);
        var all = match.Variables().GetAll("item");
        Assert.Single(all);
        Assert.Equal("hello", all[0]);
    }

    [Fact]
    public void Variables_WithUnescapeFunction_AppliesUnescape()
    {
        var unescapeFuncs = new Dictionary<string, Func<string, string>>
        {
            ["path"] = s => s.Replace(@"\ ", " ")
        };
        var grammar = Grammar.Compile(@"(?P<path>.+)", null, unescapeFuncs);
        var match = grammar.Match(@"hello\ world");

        Assert.NotNull(match);
        var vars = match.Variables();
        Assert.Equal("hello world", vars["path"]);
    }

    [Fact]
    public void Variables_NestedGroups_ExtractsBoth()
    {
        var grammar = Grammar.Compile(@"(?P<outer>(?P<inner>\d+))");
        var match = grammar.Match("123");

        Assert.NotNull(match);
        var vars = match.Variables();
        Assert.Equal("123", vars["outer"]);
        Assert.Equal("123", vars["inner"]);
    }

    [Fact]
    public void Variables_OptionalGroup_WhenMissing_ReturnsNull()
    {
        var grammar = Grammar.Compile(@"cmd(\s(?P<arg>\w+))?");
        var match = grammar.Match("cmd");

        Assert.NotNull(match);
        Assert.Null(match.Variables()["arg"]);
    }

    [Fact]
    public void Variables_Slice_ReturnsCorrectTuple()
    {
        var grammar = Grammar.Compile(@"(?P<word>\w+)");
        var match = grammar.Match("hello");

        Assert.NotNull(match);
        var vars = match.Variables().ToList();
        Assert.Single(vars);
        Assert.Equal((0, 5), vars[0].Slice);
    }

    [Fact]
    public void Variables_AmbiguousGrammar_ExtractsFromMatchingPath()
    {
        var grammar = Grammar.Compile(@"(a\s(?P<x>\w+)|b\s(?P<y>\w+))");

        var matchA = grammar.Match("a test");
        Assert.NotNull(matchA);
        Assert.Equal("test", matchA.Variables()["x"]);
        Assert.Null(matchA.Variables()["y"]);

        var matchB = grammar.Match("b test");
        Assert.NotNull(matchB);
        Assert.Null(matchB.Variables()["x"]);
        Assert.Equal("test", matchB.Variables()["y"]);
    }

    [Fact]
    public void Variables_PositionsAccountForUnicode()
    {
        var grammar = Grammar.Compile(@"(?P<emoji>.+)");
        var match = grammar.Match("🎉");

        Assert.NotNull(match);
        var vars = match.Variables().ToList();
        Assert.Single(vars);
        Assert.Equal("🎉", vars[0].Value);
        Assert.Equal(0, vars[0].Start);
        // C# strings use UTF-16, emoji is 2 chars (surrogate pair)
        Assert.Equal(2, vars[0].Stop);
    }
}

/// <summary>
/// Tests for User Story 6: Cursor position querying.
/// </summary>
public class VariableAtPositionTests
{
    [Fact]
    public void VariableAtPosition_CursorInVariable_ReturnsVariable()
    {
        var grammar = Grammar.Compile(@"cmd\s(?P<arg>\w+)");
        var match = grammar.Match("cmd test");

        Assert.NotNull(match);
        var varAtPos = match.VariableAtPosition(5); // Middle of "test"

        Assert.NotNull(varAtPos);
        Assert.Equal("arg", varAtPos.VarName);
        Assert.Equal("test", varAtPos.Value);
    }

    [Fact]
    public void VariableAtPosition_CursorAtStart_ReturnsVariable()
    {
        var grammar = Grammar.Compile(@"cmd\s(?P<arg>\w+)");
        var match = grammar.Match("cmd test");

        Assert.NotNull(match);
        var varAtPos = match.VariableAtPosition(4); // Start of "test"

        Assert.NotNull(varAtPos);
        Assert.Equal("arg", varAtPos.VarName);
    }

    [Fact]
    public void VariableAtPosition_CursorAtEnd_ReturnsNull()
    {
        var grammar = Grammar.Compile(@"cmd\s(?P<arg>\w+)");
        var match = grammar.Match("cmd test");

        Assert.NotNull(match);
        var varAtPos = match.VariableAtPosition(8); // After "test"

        Assert.Null(varAtPos); // At stop position, not inside
    }

    [Fact]
    public void VariableAtPosition_CursorInLiteralText_ReturnsNull()
    {
        var grammar = Grammar.Compile(@"cmd\s(?P<arg>\w+)");
        var match = grammar.Match("cmd test");

        Assert.NotNull(match);
        var varAtPos = match.VariableAtPosition(1); // In "cmd"

        Assert.Null(varAtPos);
    }

    [Fact]
    public void VariableAtPosition_CursorInWhitespace_ReturnsNull()
    {
        var grammar = Grammar.Compile(@"cmd\s(?P<arg>\w+)");
        var match = grammar.Match("cmd test");

        Assert.NotNull(match);
        var varAtPos = match.VariableAtPosition(3); // At the space

        Assert.Null(varAtPos);
    }

    [Fact]
    public void VariableAtPosition_NegativePosition_ReturnsNull()
    {
        var grammar = Grammar.Compile(@"(?P<arg>\w+)");
        var match = grammar.Match("test");

        Assert.NotNull(match);
        var varAtPos = match.VariableAtPosition(-1);

        Assert.Null(varAtPos);
    }

    [Fact]
    public void VariableAtPosition_PositionBeyondInput_ReturnsNull()
    {
        var grammar = Grammar.Compile(@"(?P<arg>\w+)");
        var match = grammar.Match("test");

        Assert.NotNull(match);
        var varAtPos = match.VariableAtPosition(100);

        Assert.Null(varAtPos);
    }

    [Fact]
    public void VariableAtPosition_MultipleVariables_ReturnsCorrectOne()
    {
        var grammar = Grammar.Compile(@"(?P<a>\w+)\s(?P<b>\w+)");
        var match = grammar.Match("hello world");

        Assert.NotNull(match);

        // In first variable
        var varA = match.VariableAtPosition(2);
        Assert.NotNull(varA);
        Assert.Equal("a", varA.VarName);

        // In second variable
        var varB = match.VariableAtPosition(8);
        Assert.NotNull(varB);
        Assert.Equal("b", varB.VarName);
    }

    [Fact]
    public void VariableAtPosition_NestedVariables_ReturnsFirst()
    {
        var grammar = Grammar.Compile(@"(?P<outer>(?P<inner>\w+))");
        var match = grammar.Match("hello");

        Assert.NotNull(match);
        var varAtPos = match.VariableAtPosition(2);

        Assert.NotNull(varAtPos);
        // Should return the first match found (outer contains inner)
        Assert.Contains(varAtPos.VarName, new[] { "outer", "inner" });
    }

    [Fact]
    public void VariableAtPosition_EmptyVariable_NotContained()
    {
        var grammar = Grammar.Compile(@"prefix(?P<opt>\w*)suffix");
        var match = grammar.Match("prefixsuffix");

        Assert.NotNull(match);
        // The empty variable has start == stop at position 6
        var varAtPos = match.VariableAtPosition(6);

        Assert.Null(varAtPos); // Empty range doesn't contain any position
    }

    [Fact]
    public void VariableAtPosition_WithPrefix_ReturnsVariable()
    {
        var grammar = Grammar.Compile(@"cmd\s(?P<arg>\w+)");
        var match = grammar.MatchPrefix("cmd te");

        Assert.NotNull(match);
        var varAtPos = match.VariableAtPosition(5); // In "te"

        Assert.NotNull(varAtPos);
        Assert.Equal("arg", varAtPos.VarName);
        Assert.Equal("te", varAtPos.Value);
    }
}
