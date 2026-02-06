using Stroke.Lexers;
using Xunit;

namespace Stroke.Tests.Lexers;

/// <summary>
/// Tests for <see cref="TextMateScopeMapper"/>.
/// </summary>
public class TextMateScopeMapperTests
{
    // ════════════════════════════════════════════════════════════════════════
    // KEYWORD SCOPE MAPPING
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void MapScopes_KeywordControl_MapsToKeyword()
    {
        var scopes = new List<string> { "source.cs", "keyword.control.cs" };
        var result = TextMateScopeMapper.MapScopes(scopes);
        Assert.Equal(["Keyword"], result);
    }

    [Fact]
    public void MapScopes_KeywordOperator_MapsToOperator()
    {
        var scopes = new List<string> { "source.cs", "keyword.operator.cs" };
        var result = TextMateScopeMapper.MapScopes(scopes);
        Assert.Equal(["Operator"], result);
    }

    [Fact]
    public void MapScopes_StorageType_MapsToKeywordType()
    {
        var scopes = new List<string> { "source.cs", "storage.type.cs" };
        var result = TextMateScopeMapper.MapScopes(scopes);
        Assert.Equal(["Keyword", "Type"], result);
    }

    [Fact]
    public void MapScopes_StorageModifier_MapsToKeywordDeclaration()
    {
        var scopes = new List<string> { "source.cs", "storage.modifier.cs" };
        var result = TextMateScopeMapper.MapScopes(scopes);
        Assert.Equal(["Keyword", "Declaration"], result);
    }

    // ════════════════════════════════════════════════════════════════════════
    // STRING SCOPE MAPPING
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void MapScopes_StringQuotedDouble_MapsToStringDouble()
    {
        var scopes = new List<string> { "source.cs", "string.quoted.double.cs" };
        var result = TextMateScopeMapper.MapScopes(scopes);
        Assert.Equal(["String", "Double"], result);
    }

    [Fact]
    public void MapScopes_StringQuotedSingle_MapsToStringSingle()
    {
        var scopes = new List<string> { "source.python", "string.quoted.single.python" };
        var result = TextMateScopeMapper.MapScopes(scopes);
        Assert.Equal(["String", "Single"], result);
    }

    // ════════════════════════════════════════════════════════════════════════
    // COMMENT SCOPE MAPPING
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void MapScopes_CommentLine_MapsToCommentSingle()
    {
        var scopes = new List<string> { "source.cs", "comment.line.double-slash.cs" };
        var result = TextMateScopeMapper.MapScopes(scopes);
        Assert.Equal(["Comment", "Single"], result);
    }

    [Fact]
    public void MapScopes_CommentBlock_MapsToCommentMultiline()
    {
        var scopes = new List<string> { "source.cs", "comment.block.cs" };
        var result = TextMateScopeMapper.MapScopes(scopes);
        Assert.Equal(["Comment", "Multiline"], result);
    }

    // ════════════════════════════════════════════════════════════════════════
    // NAME SCOPE MAPPING
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void MapScopes_EntityNameFunction_MapsToNameFunction()
    {
        var scopes = new List<string> { "source.cs", "entity.name.function.cs" };
        var result = TextMateScopeMapper.MapScopes(scopes);
        Assert.Equal(["Name", "Function"], result);
    }

    [Fact]
    public void MapScopes_EntityNameTypeClass_MapsToNameClass()
    {
        var scopes = new List<string> { "source.cs", "entity.name.type.class.cs" };
        var result = TextMateScopeMapper.MapScopes(scopes);
        Assert.Equal(["Name", "Class"], result);
    }

    [Fact]
    public void MapScopes_Variable_MapsToNameVariable()
    {
        var scopes = new List<string> { "source.cs", "variable.other.cs" };
        var result = TextMateScopeMapper.MapScopes(scopes);
        Assert.Equal(["Name", "Variable"], result);
    }

    [Fact]
    public void MapScopes_SupportFunction_MapsToNameBuiltin()
    {
        var scopes = new List<string> { "source.cs", "support.function.cs" };
        var result = TextMateScopeMapper.MapScopes(scopes);
        Assert.Equal(["Name", "Builtin"], result);
    }

    // ════════════════════════════════════════════════════════════════════════
    // NUMBER/CONSTANT SCOPE MAPPING
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void MapScopes_ConstantNumeric_MapsToNumber()
    {
        var scopes = new List<string> { "source.cs", "constant.numeric.cs" };
        var result = TextMateScopeMapper.MapScopes(scopes);
        Assert.Equal(["Number"], result);
    }

    [Fact]
    public void MapScopes_ConstantLanguageBoolean_MapsToKeywordConstant()
    {
        var scopes = new List<string> { "source.cs", "constant.language.boolean.true.cs" };
        var result = TextMateScopeMapper.MapScopes(scopes);
        Assert.Equal(["Keyword", "Constant"], result);
    }

    // ════════════════════════════════════════════════════════════════════════
    // PUNCTUATION AND OTHER
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void MapScopes_Punctuation_MapsToPunctuation()
    {
        var scopes = new List<string> { "source.cs", "punctuation.separator.cs" };
        var result = TextMateScopeMapper.MapScopes(scopes);
        Assert.Equal(["Punctuation"], result);
    }

    [Fact]
    public void MapScopes_Invalid_MapsToError()
    {
        var scopes = new List<string> { "source.cs", "invalid.illegal.cs" };
        var result = TextMateScopeMapper.MapScopes(scopes);
        Assert.Equal(["Error"], result);
    }

    // ════════════════════════════════════════════════════════════════════════
    // LANGUAGE SUFFIX STRIPPING
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void MapScope_StripsKnownCsSuffix()
    {
        var result = TextMateScopeMapper.MapScope("keyword.control.cs");
        Assert.Equal(["Keyword"], result);
    }

    [Fact]
    public void MapScope_StripsPythonSuffix()
    {
        var result = TextMateScopeMapper.MapScope("keyword.control.python");
        Assert.Equal(["Keyword"], result);
    }

    [Fact]
    public void MapScope_StripsFsharpSuffix()
    {
        var result = TextMateScopeMapper.MapScope("keyword.control.fsharp");
        Assert.Equal(["Keyword"], result);
    }

    [Fact]
    public void MapScope_StripsVbSuffix()
    {
        var result = TextMateScopeMapper.MapScope("keyword.control.vb");
        Assert.Equal(["Keyword"], result);
    }

    [Fact]
    public void MapScope_DoesNotStripUnknownSuffix()
    {
        // "exotic" is not a known language suffix, so it stays
        var result = TextMateScopeMapper.MapScope("keyword.exotic");
        // Falls through to PascalCase passthrough
        Assert.Equal(["Keyword"], result);
    }

    // ════════════════════════════════════════════════════════════════════════
    // EDGE CASES
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void MapScopes_EmptyList_ReturnsDefaultToken()
    {
        var result = TextMateScopeMapper.MapScopes([]);
        Assert.Equal(["Token"], result);
    }

    [Fact]
    public void MapScopes_SingleRootScope_MapsToToken()
    {
        var result = TextMateScopeMapper.MapScopes(["source.cs"]);
        Assert.Equal(["Token"], result);
    }

    [Fact]
    public void MapScope_UnmappedScope_PassesThroughAsPascalCase()
    {
        var result = TextMateScopeMapper.MapScope("something.completely.unknown");
        // Passes through as PascalCase
        Assert.Equal("Something", result[0]);
        Assert.Equal("Completely", result[1]);
        Assert.Equal("Unknown", result[2]);
    }

    [Fact]
    public void MapScopes_UsesLastScope()
    {
        // When multiple non-root scopes exist, use the last (most specific)
        var scopes = new List<string> { "source.cs", "meta.class.cs", "entity.name.type.class.cs" };
        var result = TextMateScopeMapper.MapScopes(scopes);
        Assert.Equal(["Name", "Class"], result);
    }
}
