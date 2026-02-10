using Stroke.Application;
using Stroke.Core;
using Stroke.FormattedText;
using Stroke.Input.Pipe;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Menus;
using Stroke.Output;
using Xunit;

using AppContext = Stroke.Application.AppContext;
using Buffer = Stroke.Core.Buffer;
using CompletionItem = Stroke.Completion.Completion;
using StrokeLayout = Stroke.Layout.Layout;

namespace Stroke.Tests.Layout.Menus;

/// <summary>
/// Tests for SelectedCompletionMetaControl (US7: meta information display).
/// </summary>
public sealed class SelectedCompletionMetaControlTests
{
    private static (Application<object?> app, IDisposable scope, Buffer buffer) CreateAppWithCompletions(
        IReadOnlyList<CompletionItem>? completions = null,
        int? selectIndex = null)
    {
        var buffer = new Buffer(document: new Document("hel", cursorPosition: 3));
        var control = new BufferControl(buffer: buffer);
        var window = new Window(content: control);
        var layout = new StrokeLayout(new AnyContainer(window));
        var input = new SimplePipeInput();
        var output = new DummyOutput();
        var app = new Application<object?>(layout: layout, input: input, output: output);
        var scope = AppContext.SetApp(app);

        if (completions is not null)
        {
            buffer.SetCompletions(completions);
            if (selectIndex is not null)
            {
                buffer.GoToCompletion(selectIndex.Value);
            }
        }

        return (app, scope, buffer);
    }

    #region IsFocusable Tests

    [Fact]
    public void IsFocusable_ReturnsFalse()
    {
        var control = new SelectedCompletionMetaControl();
        Assert.False(control.IsFocusable);
    }

    #endregion

    #region PreferredWidth Tests

    [Fact]
    public void PreferredWidth_NoCompletions_ReturnsZero()
    {
        var (_, scope, _) = CreateAppWithCompletions();
        using (scope)
        {
            var control = new SelectedCompletionMetaControl();
            var width = control.PreferredWidth(100);
            Assert.Equal(0, width);
        }
    }

    [Fact]
    public void PreferredWidth_WithMeta_ReturnsPositive()
    {
        AnyFormattedText meta = "some meta text";
        var completions = new List<CompletionItem>
        {
            new("hello", startPosition: -3, displayMeta: meta)
        };
        var (_, scope, _) = CreateAppWithCompletions(completions);
        using (scope)
        {
            var control = new SelectedCompletionMetaControl();
            var width = control.PreferredWidth(100);
            Assert.True(width > 0);
        }
    }

    [Fact]
    public void PreferredWidth_ManyCompletions_ReturnsMaxAvailable()
    {
        // Performance optimization: 30+ completions should return maxAvailableWidth
        AnyFormattedText meta = "meta";
        var completions = new List<CompletionItem>();
        for (int i = 0; i < 35; i++)
        {
            completions.Add(new CompletionItem($"item{i}", startPosition: -3, displayMeta: meta));
        }
        var (_, scope, _) = CreateAppWithCompletions(completions);
        using (scope)
        {
            var control = new SelectedCompletionMetaControl();
            var width = control.PreferredWidth(80);
            Assert.Equal(80, width);
        }
    }

    [Fact]
    public void PreferredWidth_FewCompletions_ReturnsSampledWidth()
    {
        AnyFormattedText meta = "ab";
        var completions = new List<CompletionItem>
        {
            new("hello", startPosition: -3, displayMeta: meta),
            new("world", startPosition: -3, displayMeta: meta)
        };
        var (_, scope, _) = CreateAppWithCompletions(completions);
        using (scope)
        {
            var control = new SelectedCompletionMetaControl();
            var width = control.PreferredWidth(100);
            // Should be 2 + max meta width (2) = 4
            Assert.Equal(4, width);
        }
    }

    #endregion

    #region PreferredHeight Tests

    [Fact]
    public void PreferredHeight_AlwaysReturnsOne()
    {
        var control = new SelectedCompletionMetaControl();
        var height = control.PreferredHeight(50, 20, false, null);
        Assert.Equal(1, height);
    }

    #endregion

    #region CreateContent Tests

    [Fact]
    public void CreateContent_NoCompletions_ReturnsEmptyContent()
    {
        var (_, scope, _) = CreateAppWithCompletions();
        using (scope)
        {
            var control = new SelectedCompletionMetaControl();
            var content = control.CreateContent(50, 1);
            Assert.Equal(0, content.LineCount);
        }
    }

    [Fact]
    public void CreateContent_SelectedCompletionWithMeta_ShowsMeta()
    {
        AnyFormattedText meta = "test meta";
        var completions = new List<CompletionItem>
        {
            new("hello", startPosition: -3, displayMeta: meta),
            new("world", startPosition: -3, displayMeta: meta)
        };
        var (_, scope, _) = CreateAppWithCompletions(completions, selectIndex: 0);
        using (scope)
        {
            var control = new SelectedCompletionMetaControl();
            var content = control.CreateContent(50, 1);

            Assert.Equal(1, content.LineCount);
            var line = content.GetLine(0);
            var text = string.Concat(line.Select(f => f.Text));
            Assert.Contains("test meta", text);
        }
    }

    [Fact]
    public void CreateContent_NoSelection_ReturnsEmptyContent()
    {
        AnyFormattedText meta = "meta";
        var completions = new List<CompletionItem>
        {
            new("hello", startPosition: -3, displayMeta: meta)
        };
        // No selection (selectIndex is null)
        var (_, scope, _) = CreateAppWithCompletions(completions);
        using (scope)
        {
            var control = new SelectedCompletionMetaControl();
            var content = control.CreateContent(50, 1);

            // CurrentCompletion is null when no selection, so empty content
            Assert.Equal(0, content.LineCount);
        }
    }

    [Fact]
    public void CreateContent_SelectedWithNoMeta_ReturnsEmptyContent()
    {
        var completions = new List<CompletionItem>
        {
            new("hello", startPosition: -3),
            new("world", startPosition: -3)
        };
        var (_, scope, _) = CreateAppWithCompletions(completions, selectIndex: 0);
        using (scope)
        {
            var control = new SelectedCompletionMetaControl();
            var content = control.CreateContent(50, 1);

            // No meta available, should be empty
            Assert.Equal(0, content.LineCount);
        }
    }

    [Fact]
    public void CreateContent_MetaWithStyle_AppliesMultiColumnMetaStyle()
    {
        AnyFormattedText meta = "styled meta";
        var completions = new List<CompletionItem>
        {
            new("hello", startPosition: -3, displayMeta: meta)
        };
        var (_, scope, _) = CreateAppWithCompletions(completions, selectIndex: 0);
        using (scope)
        {
            var control = new SelectedCompletionMetaControl();
            var content = control.CreateContent(50, 1);

            Assert.Equal(1, content.LineCount);
            var line = content.GetLine(0);
            var styles = line.Select(f => f.Style).ToList();
            // Should contain the multi-column-meta style class
            Assert.Contains(styles, s => s.Contains("class:completion-menu.multi-column-meta"));
        }
    }

    #endregion
}
