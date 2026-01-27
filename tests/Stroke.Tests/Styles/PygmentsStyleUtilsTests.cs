using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Styles;

/// <summary>
/// Tests for the PygmentsStyleUtils class.
/// </summary>
public class PygmentsStyleUtilsTests
{
    #region PygmentsTokenToClassName Tests

    [Fact]
    public void PygmentsTokenToClassName_EmptyParts_ReturnsPygments()
    {
        var result = PygmentsStyleUtils.PygmentsTokenToClassName([]);
        Assert.Equal("pygments", result);
    }

    [Fact]
    public void PygmentsTokenToClassName_SinglePart_ReturnsPygmentsDotPart()
    {
        var result = PygmentsStyleUtils.PygmentsTokenToClassName(["Name"]);
        Assert.Equal("pygments.name", result);
    }

    [Fact]
    public void PygmentsTokenToClassName_MultipleParts_ReturnsJoined()
    {
        var result = PygmentsStyleUtils.PygmentsTokenToClassName(["Name", "Exception"]);
        Assert.Equal("pygments.name.exception", result);
    }

    [Fact]
    public void PygmentsTokenToClassName_ConvertsToPascalCaseToLowercase()
    {
        var result = PygmentsStyleUtils.PygmentsTokenToClassName(["Literal", "String", "Doc"]);
        Assert.Equal("pygments.literal.string.doc", result);
    }

    [Fact]
    public void PygmentsTokenToClassName_MixedCase_AllLowercase()
    {
        var result = PygmentsStyleUtils.PygmentsTokenToClassName(["KEYWORD", "Type"]);
        Assert.Equal("pygments.keyword.type", result);
    }

    [Fact]
    public void PygmentsTokenToClassName_ThrowsForNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            PygmentsStyleUtils.PygmentsTokenToClassName((IEnumerable<string>)null!));
    }

    [Fact]
    public void PygmentsTokenToClassName_ParamsOverload_Works()
    {
        var result = PygmentsStyleUtils.PygmentsTokenToClassName("Name", "Function");
        Assert.Equal("pygments.name.function", result);
    }

    [Fact]
    public void PygmentsTokenToClassName_ParamsOverload_NoParams()
    {
        var result = PygmentsStyleUtils.PygmentsTokenToClassName();
        Assert.Equal("pygments", result);
    }

    #endregion

    #region StyleFromPygmentsDict Tests

    [Fact]
    public void StyleFromPygmentsDict_EmptyDict_ReturnsEmptyStyle()
    {
        var dict = new Dictionary<IEnumerable<string>, string>();
        var style = PygmentsStyleUtils.StyleFromPygmentsDict(dict);

        Assert.NotNull(style);
        Assert.Empty(style.StyleRules);
    }

    [Fact]
    public void StyleFromPygmentsDict_SingleRule_ConvertsCorrectly()
    {
        var dict = new Dictionary<IEnumerable<string>, string>
        {
            { new[] { "Name", "Function" }, "bold #0000ff" }
        };

        var style = PygmentsStyleUtils.StyleFromPygmentsDict(dict);

        Assert.Single(style.StyleRules);
        Assert.Equal(("pygments.name.function", "bold #0000ff"), style.StyleRules[0]);
    }

    [Fact]
    public void StyleFromPygmentsDict_MultipleRules_ConvertsAll()
    {
        var dict = new Dictionary<IEnumerable<string>, string>
        {
            { new[] { "Keyword" }, "bold #008000" },
            { new[] { "Comment" }, "italic #408080" },
            { new[] { "Name", "Class" }, "bold #0000ff" }
        };

        var style = PygmentsStyleUtils.StyleFromPygmentsDict(dict);

        Assert.Equal(3, style.StyleRules.Count);

        // Verify the style can be used
        var keywordAttrs = style.GetAttrsForStyleStr("class:pygments.keyword");
        Assert.True(keywordAttrs.Bold);
        Assert.Equal("008000", keywordAttrs.Color);
    }

    [Fact]
    public void StyleFromPygmentsDict_ThrowsForNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            PygmentsStyleUtils.StyleFromPygmentsDict((IEnumerable<KeyValuePair<IEnumerable<string>, string>>)null!));
    }

    [Fact]
    public void StyleFromPygmentsDict_StringKeys_EmptyDict()
    {
        var dict = new Dictionary<string, string>();
        var style = PygmentsStyleUtils.StyleFromPygmentsDict(dict);

        Assert.NotNull(style);
        Assert.Empty(style.StyleRules);
    }

    [Fact]
    public void StyleFromPygmentsDict_StringKeys_SingleRule()
    {
        var dict = new Dictionary<string, string>
        {
            { "Name.Function", "bold #0000ff" }
        };

        var style = PygmentsStyleUtils.StyleFromPygmentsDict(dict);

        Assert.Single(style.StyleRules);
        Assert.Equal(("pygments.name.function", "bold #0000ff"), style.StyleRules[0]);
    }

    [Fact]
    public void StyleFromPygmentsDict_StringKeys_MultipleRules()
    {
        var dict = new Dictionary<string, string>
        {
            { "Keyword", "bold #008000" },
            { "Comment.Single", "italic #408080" },
            { "Name.Class", "bold #0000ff" }
        };

        var style = PygmentsStyleUtils.StyleFromPygmentsDict(dict);

        Assert.Equal(3, style.StyleRules.Count);

        // Verify class name conversion
        var commentAttrs = style.GetAttrsForStyleStr("class:pygments.comment.single");
        Assert.True(commentAttrs.Italic);
    }

    [Fact]
    public void StyleFromPygmentsDict_StringKeys_ThrowsForNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            PygmentsStyleUtils.StyleFromPygmentsDict((IReadOnlyDictionary<string, string>)null!));
    }

    [Fact]
    public void StyleFromPygmentsDict_StringKeys_SinglePartToken()
    {
        var dict = new Dictionary<string, string>
        {
            { "Error", "#ff0000" }
        };

        var style = PygmentsStyleUtils.StyleFromPygmentsDict(dict);

        var errorAttrs = style.GetAttrsForStyleStr("class:pygments.error");
        Assert.Equal("ff0000", errorAttrs.Color);
    }

    #endregion

    #region StyleFromPygmentsClass Tests

    [Fact]
    public void StyleFromPygmentsClass_ThrowsForNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            PygmentsStyleUtils.StyleFromPygmentsClass(null!));
    }

    [Fact]
    public void StyleFromPygmentsClass_EmptyStyles()
    {
        var style = PygmentsStyleUtils.StyleFromPygmentsClass(
            () => new Dictionary<string, string>());

        Assert.NotNull(style);
        Assert.Empty(style.StyleRules);
    }

    [Fact]
    public void StyleFromPygmentsClass_WithStyles()
    {
        var style = PygmentsStyleUtils.StyleFromPygmentsClass(
            () => new Dictionary<string, string>
            {
                { "Keyword", "bold" },
                { "String", "#ba2121" }
            });

        Assert.Equal(2, style.StyleRules.Count);

        var keywordAttrs = style.GetAttrsForStyleStr("class:pygments.keyword");
        Assert.True(keywordAttrs.Bold);
    }

    [Fact]
    public void StyleFromPygmentsClass_GetterIsInvokedOnCall()
    {
        var callCount = 0;
        var style = PygmentsStyleUtils.StyleFromPygmentsClass(() =>
        {
            callCount++;
            return new Dictionary<string, string> { { "Test", "bold" } };
        });

        Assert.Equal(1, callCount);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void StyleFromPygmentsDict_WorksWithDefaultPygmentsStyle()
    {
        // The default Pygments style is already in class name format,
        // but this tests that we can create styles from similar dictionaries
        var style = Style.FromDict(DefaultStyles.PygmentsDefaultStyle);

        var keywordAttrs = style.GetAttrsForStyleStr("class:pygments.keyword");
        Assert.True(keywordAttrs.Bold);
        Assert.Equal("008000", keywordAttrs.Color);
    }

    [Fact]
    public void PygmentsTokenToClassName_MatchesPythonBehavior()
    {
        // Python: pygments_token_to_classname(Token.Name.Exception) -> 'pygments.name.exception'
        // Token.Name.Exception in Python is essentially ("Name", "Exception") tuple
        var result = PygmentsStyleUtils.PygmentsTokenToClassName("Name", "Exception");
        Assert.Equal("pygments.name.exception", result);
    }

    [Fact]
    public void StyleFromPygmentsDict_MatchesPythonBehavior()
    {
        // Simulating Python's style_from_pygments_dict behavior
        var pygmentsDict = new Dictionary<IEnumerable<string>, string>
        {
            { new[] { "Name", "Builtin" }, "#008000" },
            { new[] { "Name", "Function" }, "#0000ff" }
        };

        var style = PygmentsStyleUtils.StyleFromPygmentsDict(pygmentsDict);

        // Should be able to get attrs using the class: prefix
        var builtinAttrs = style.GetAttrsForStyleStr("class:pygments.name.builtin");
        Assert.Equal("008000", builtinAttrs.Color);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task PygmentsStyleUtils_ThreadSafe_ConcurrentCalls()
    {
        var exceptions = new List<Exception>();
        var tasks = new Task[10];

        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                try
                {
                    for (int j = 0; j < 100; j++)
                    {
                        // Concurrent token conversions
                        var className = PygmentsStyleUtils.PygmentsTokenToClassName("Name", "Function");
                        Assert.Equal("pygments.name.function", className);

                        // Concurrent style creation
                        var style = PygmentsStyleUtils.StyleFromPygmentsDict(
                            new Dictionary<string, string>
                            {
                                { "Keyword", "bold" }
                            });
                        Assert.Single(style.StyleRules);
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            }, TestContext.Current.CancellationToken);
        }

        await Task.WhenAll(tasks);
        Assert.Empty(exceptions);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void PygmentsTokenToClassName_EmptyString_InParts()
    {
        var result = PygmentsStyleUtils.PygmentsTokenToClassName("");
        Assert.Equal("pygments.", result);
    }

    [Fact]
    public void PygmentsTokenToClassName_WhitespaceInParts()
    {
        var result = PygmentsStyleUtils.PygmentsTokenToClassName("Name ", " Function");
        Assert.Equal("pygments.name . function", result);
    }

    [Fact]
    public void StyleFromPygmentsDict_StringKeys_AlreadyLowercase()
    {
        var dict = new Dictionary<string, string>
        {
            { "keyword", "bold" }
        };

        var style = PygmentsStyleUtils.StyleFromPygmentsDict(dict);

        var attrs = style.GetAttrsForStyleStr("class:pygments.keyword");
        Assert.True(attrs.Bold);
    }

    #endregion
}
