using System.Reflection;
using Stroke.Application;
using Stroke.Application.Bindings;
using Stroke.AutoSuggest;
using Stroke.Core;
using Stroke.Input.Pipe;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Windows;
using Stroke.Output;
using Xunit;
using AppContext = Stroke.Application.AppContext;
using Buffer = Stroke.Core.Buffer;
using Keys = Stroke.Input.Keys;

namespace Stroke.Tests.Application.Bindings;

/// <summary>
/// Tests for <see cref="AutoSuggestBindings"/> handler functions and binding registration.
/// </summary>
public sealed class AutoSuggestBindingsTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public AutoSuggestBindingsTests()
    {
        _input = new SimplePipeInput();
        _output = new DummyOutput();
    }

    public void Dispose()
    {
        _input.Dispose();
    }

    /// <summary>Helper to set a private field on a buffer via reflection.</summary>
    private static void SetBufferField(Buffer buffer, string fieldName, object? value)
    {
        var field = typeof(Buffer).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);
        field!.SetValue(buffer, value);
    }

    /// <summary>
    /// Creates a minimal environment with a buffer, layout, application, and AppContext scope.
    /// </summary>
    private (Buffer Buffer, Application<object> App, IDisposable Scope)
        CreateEnvironment(
            string text = "",
            int cursorPosition = -1,
            EditingMode editingMode = EditingMode.Emacs)
    {
        // Default cursor position to end of text
        if (cursorPosition < 0) cursorPosition = text.Length;

        var buffer = new Buffer(document: new Document(text, cursorPosition: cursorPosition));
        var bc = new BufferControl(buffer: buffer);
        var window = new Window(content: bc);
        var container = new HSplit([window]);
        var layout = new Stroke.Layout.Layout(new AnyContainer(container));
        var app = new Application<object>(
            input: _input, output: _output, layout: layout, editingMode: editingMode);
        var scope = AppContext.SetApp(app);

        return (buffer, app, scope);
    }

    /// <summary>
    /// Creates a KeyPressEvent for testing binding handlers.
    /// </summary>
    private static KeyPressEvent CreateEvent(
        Buffer? buffer = null,
        IApplication? app = null)
    {
        return new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: [new KeyPress(Keys.Any)],
            previousKeySequence: [],
            isRepeat: false,
            app: app,
            currentBuffer: buffer);
    }

    // ═════════════════════════════════════════════════════════════════════
    // Phase 2: SuggestionAvailable Filter Tests (T005, T006)
    // ═════════════════════════════════════════════════════════════════════

    #region Filter Positive Tests (T005)

    [Fact]
    public void AcceptSuggestion_ActivatesWhenSuggestionAvailableAndCursorAtEnd()
    {
        var (buffer, app, scope) = CreateEnvironment("git");
        using (scope)
        {
            SetBufferField(buffer, "_suggestion", new Suggestion(" commit -m 'fix bug'"));
            var evt = CreateEvent(buffer: buffer, app: app);

            AutoSuggestBindings.AcceptSuggestion(evt);

            Assert.Equal("git commit -m 'fix bug'", buffer.Text);
        }
    }

    #endregion

    #region Filter Negative Tests (T006)

    [Fact]
    public void AcceptSuggestion_DoesNothing_WhenNoSuggestion()
    {
        var (buffer, _, scope) = CreateEnvironment("git");
        using (scope)
        {
            var evt = CreateEvent(buffer: buffer);

            var result = AutoSuggestBindings.AcceptSuggestion(evt);

            Assert.Null(result);
            Assert.Equal("git", buffer.Text);
        }
    }

    [Fact]
    public void AcceptSuggestion_DoesNothing_WhenSuggestionTextEmpty()
    {
        var (buffer, _, scope) = CreateEnvironment("git");
        using (scope)
        {
            SetBufferField(buffer, "_suggestion", new Suggestion(""));
            var evt = CreateEvent(buffer: buffer);

            var result = AutoSuggestBindings.AcceptSuggestion(evt);

            // Handler has null-guard on suggestion, but empty text is still non-null.
            // The handler inserts the empty string (no visible change).
            Assert.Null(result);
        }
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════
    // Phase 3: User Story 1 — AcceptSuggestion Tests (T008-T010, T013)
    // ═════════════════════════════════════════════════════════════════════

    #region AcceptSuggestion Handler Tests (T008-T010)

    [Fact]
    public void AcceptSuggestion_InsertsSuggestionText()
    {
        var (buffer, app, scope) = CreateEnvironment("git");
        using (scope)
        {
            SetBufferField(buffer, "_suggestion", new Suggestion(" commit -m 'fix bug'"));
            var evt = CreateEvent(buffer: buffer, app: app);

            AutoSuggestBindings.AcceptSuggestion(evt);

            Assert.Equal("git commit -m 'fix bug'", buffer.Text);
        }
    }

    [Fact]
    public void AcceptSuggestion_ReturnsNull_WhenSuggestionIsNull()
    {
        var (buffer, _, scope) = CreateEnvironment("git");
        using (scope)
        {
            // No suggestion set — Suggestion property is null
            var evt = CreateEvent(buffer: buffer);

            var result = AutoSuggestBindings.AcceptSuggestion(evt);

            Assert.Null(result);
            Assert.Equal("git", buffer.Text);
        }
    }

    [Fact]
    public void AcceptSuggestion_SingleCharacterSuggestion()
    {
        var (buffer, app, scope) = CreateEnvironment("a");
        using (scope)
        {
            SetBufferField(buffer, "_suggestion", new Suggestion("x"));
            var evt = CreateEvent(buffer: buffer, app: app);

            AutoSuggestBindings.AcceptSuggestion(evt);

            Assert.Equal("ax", buffer.Text);
        }
    }

    #endregion

    #region LoadAutoSuggestBindings Factory Tests (T013, T024)

    [Fact]
    public void LoadAutoSuggestBindings_ReturnsNonNullKeyBindings()
    {
        var kb = AutoSuggestBindings.LoadAutoSuggestBindings();

        Assert.NotNull(kb);
    }

    [Fact]
    public void LoadAutoSuggestBindings_ContainsFourBindings()
    {
        var kb = AutoSuggestBindings.LoadAutoSuggestBindings();

        Assert.Equal(4, kb.Bindings.Count);
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════
    // Phase 4: User Story 2 — AcceptPartialSuggestion Tests (T015-T021)
    // ═════════════════════════════════════════════════════════════════════

    #region AcceptPartialSuggestion Handler Tests (T015-T021)

    [Fact]
    public void AcceptPartialSuggestion_InsertsFirstWordSegment()
    {
        var (buffer, app, scope) = CreateEnvironment("git ");
        using (scope)
        {
            SetBufferField(buffer, "_suggestion", new Suggestion("commit -m 'message'"));
            var evt = CreateEvent(buffer: buffer, app: app);

            AutoSuggestBindings.AcceptPartialSuggestion(evt);

            Assert.Equal("git commit ", buffer.Text);
        }
    }

    [Fact]
    public void AcceptPartialSuggestion_InsertsPathSegment()
    {
        var (buffer, app, scope) = CreateEnvironment("cd ");
        using (scope)
        {
            SetBufferField(buffer, "_suggestion", new Suggestion("home/user/documents/"));
            var evt = CreateEvent(buffer: buffer, app: app);

            AutoSuggestBindings.AcceptPartialSuggestion(evt);

            Assert.Equal("cd home/", buffer.Text);
        }
    }

    [Fact]
    public void AcceptPartialSuggestion_LeadingSpaceSuggestion()
    {
        var (buffer, app, scope) = CreateEnvironment("git");
        using (scope)
        {
            SetBufferField(buffer, "_suggestion", new Suggestion(" commit -m 'fix'"));
            var evt = CreateEvent(buffer: buffer, app: app);

            AutoSuggestBindings.AcceptPartialSuggestion(evt);

            Assert.Equal("git ", buffer.Text);
        }
    }

    [Fact]
    public void AcceptPartialSuggestion_LeadingSlashSuggestion()
    {
        var (buffer, app, scope) = CreateEnvironment("cd ");
        using (scope)
        {
            SetBufferField(buffer, "_suggestion", new Suggestion("/home/user/"));
            var evt = CreateEvent(buffer: buffer, app: app);

            AutoSuggestBindings.AcceptPartialSuggestion(evt);

            Assert.Equal("cd /", buffer.Text);
        }
    }

    [Fact]
    public void AcceptPartialSuggestion_NoWordBoundary()
    {
        var (buffer, app, scope) = CreateEnvironment("git ");
        using (scope)
        {
            SetBufferField(buffer, "_suggestion", new Suggestion("abc"));
            var evt = CreateEvent(buffer: buffer, app: app);

            AutoSuggestBindings.AcceptPartialSuggestion(evt);

            Assert.Equal("git abc", buffer.Text);
        }
    }

    [Fact]
    public void AcceptPartialSuggestion_WhitespaceOnlySuggestion()
    {
        var (buffer, app, scope) = CreateEnvironment("git");
        using (scope)
        {
            SetBufferField(buffer, "_suggestion", new Suggestion("   "));
            var evt = CreateEvent(buffer: buffer, app: app);

            AutoSuggestBindings.AcceptPartialSuggestion(evt);

            Assert.Equal("git   ", buffer.Text);
        }
    }

    [Fact]
    public void AcceptPartialSuggestion_ReturnsNull_WhenSuggestionIsNull()
    {
        var (buffer, _, scope) = CreateEnvironment("git");
        using (scope)
        {
            var evt = CreateEvent(buffer: buffer);

            var result = AutoSuggestBindings.AcceptPartialSuggestion(evt);

            Assert.Null(result);
            Assert.Equal("git", buffer.Text);
        }
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════
    // Phase 5: User Story 3 — Bindings Inactive Tests (T026-T029)
    // ═════════════════════════════════════════════════════════════════════

    #region Binding Filter Integration Tests (T026-T029)

    [Fact]
    public void FullAcceptBinding_DoesNotActivate_WhenNoSuggestion()
    {
        var (buffer, app, scope) = CreateEnvironment("git");
        using (scope)
        {
            // No suggestion set
            var kb = AutoSuggestBindings.LoadAutoSuggestBindings();

            // Verify the Right arrow binding's filter evaluates to false
            // by checking that all bindings whose keys contain Right have inactive filters
            var rightBindings = kb.Bindings
                .Where(b => b.Keys.Any(k => k.IsKey && k.Key == Keys.Right));
            foreach (var binding in rightBindings)
            {
                Assert.False(binding.Filter.Invoke());
            }
        }
    }

    [Fact]
    public void FullAcceptBinding_DoesNotActivate_WhenSuggestionTextEmpty()
    {
        var (buffer, _, scope) = CreateEnvironment("git");
        using (scope)
        {
            SetBufferField(buffer, "_suggestion", new Suggestion(""));
            var kb = AutoSuggestBindings.LoadAutoSuggestBindings();

            var rightBindings = kb.Bindings
                .Where(b => b.Keys.Any(k => k.IsKey && k.Key == Keys.Right));
            foreach (var binding in rightBindings)
            {
                Assert.False(binding.Filter.Invoke());
            }
        }
    }

    [Fact]
    public void FullAcceptBinding_DoesNotActivate_WhenCursorNotAtEnd()
    {
        var (buffer, _, scope) = CreateEnvironment("git co", cursorPosition: 3);
        using (scope)
        {
            SetBufferField(buffer, "_suggestion", new Suggestion("mmit"));
            var kb = AutoSuggestBindings.LoadAutoSuggestBindings();

            var rightBindings = kb.Bindings
                .Where(b => b.Keys.Any(k => k.IsKey && k.Key == Keys.Right));
            foreach (var binding in rightBindings)
            {
                Assert.False(binding.Filter.Invoke());
            }
        }
    }

    [Fact]
    public void PartialAcceptBinding_DoesNotActivate_WhenNoSuggestion()
    {
        var (buffer, _, scope) = CreateEnvironment("git");
        using (scope)
        {
            var kb = AutoSuggestBindings.LoadAutoSuggestBindings();

            // The Escape+F binding is the 4th (index 3) — it has 2 keys
            var escFBindings = kb.Bindings
                .Where(b => b.Keys.Count == 2);
            foreach (var binding in escFBindings)
            {
                Assert.False(binding.Filter.Invoke());
            }
        }
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════
    // Phase 6: User Story 4 — Vi Override Tests (T030)
    // ═════════════════════════════════════════════════════════════════════

    #region Vi Override Tests (T030)

    [Fact]
    public void RightArrowBinding_ActivatesWhenSuggestionAvailable()
    {
        var (buffer, _, scope) = CreateEnvironment("git");
        using (scope)
        {
            SetBufferField(buffer, "_suggestion", new Suggestion(" commit"));
            var kb = AutoSuggestBindings.LoadAutoSuggestBindings();

            var rightBindings = kb.Bindings
                .Where(b => b.Keys.Any(k => k.IsKey && k.Key == Keys.Right));
            foreach (var binding in rightBindings)
            {
                Assert.True(binding.Filter.Invoke());
            }
        }
    }

    [Fact]
    public void RightArrowBinding_FallsThroughWhenNoSuggestion()
    {
        var (buffer, _, scope) = CreateEnvironment("git");
        using (scope)
        {
            // No suggestion — filter should be false, allowing Vi bindings to handle
            var kb = AutoSuggestBindings.LoadAutoSuggestBindings();

            var rightBindings = kb.Bindings
                .Where(b => b.Keys.Any(k => k.IsKey && k.Key == Keys.Right));
            foreach (var binding in rightBindings)
            {
                Assert.False(binding.Filter.Invoke());
            }
        }
    }

    #endregion
}
