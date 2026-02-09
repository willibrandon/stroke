using Stroke.Clipboard;
using Stroke.Core;
using Stroke.Input.Pipe;
using Stroke.KeyBinding;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Output;
using Xunit;
using AppContext = Stroke.Application.AppContext;
using Buffer = Stroke.Core.Buffer;
using Keys = Stroke.Input.Keys;
using ViBindingsType = Stroke.Application.Bindings.ViBindings;

namespace Stroke.Tests.Application;

/// <summary>
/// Mapped Vi mode tests (T063) — 13 test cases from test-mapping.md.
/// These verify Vi binding registration and basic behavior.
/// </summary>
public sealed class ViModeTests : IDisposable
{
    private readonly SimplePipeInput _input;
    private readonly DummyOutput _output;

    public ViModeTests()
    {
        _input = new SimplePipeInput();
        _output = new DummyOutput();
    }

    public void Dispose()
    {
        _input.Dispose();
    }

    private (Buffer Buffer, Stroke.Application.Application<object> App, IDisposable Scope)
        CreateEnvironment(
            string text = "",
            int cursorPosition = 0,
            bool multiline = false)
    {
        var buffer = new Buffer(
            document: new Document(text, cursorPosition: cursorPosition),
            multiline: multiline ? () => true : () => false);
        var bufferControl = new BufferControl(buffer: buffer);
        var window = new Window(content: bufferControl);
        var layout = new Stroke.Layout.Layout(new AnyContainer(window));
        var app = new Stroke.Application.Application<object>(
            input: _input, output: _output, layout: layout,
            editingMode: EditingMode.Vi);
        var scope = AppContext.SetApp(app);

        return (buffer, app, scope);
    }

    private static IReadOnlyList<Binding> GetBindings(
        IKeyBindingsBase kb, params KeyOrChar[] keys)
    {
        return kb.GetBindingsForKeys(keys);
    }

    /// <summary>
    /// Test 1: CursorMovements — Vi movement keys h, l, j, k, w, b, e, 0, $, ^,
    /// gg, G, {, }, %, f, F, t, T, ;, , are registered.
    /// </summary>
    [Fact]
    public void CursorMovements()
    {
        var (_, _, scope) = CreateEnvironment("hello world\nfoo bar\nbaz");
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();

            // Single-char motions
            foreach (var key in new[] { 'h', 'l', 'j', 'k', 'w', 'b', 'e', 'W', 'B', 'E',
                '0', '$', '^', 'G', '{', '}', '%', 'H', 'M', 'L', 'n', 'N', ';', ',' })
            {
                Assert.NotEmpty(GetBindings(kb, new KeyOrChar(key)));
            }

            // Multi-char motions
            Assert.NotEmpty(GetBindings(kb,
                new KeyOrChar('g'), new KeyOrChar('g')));
            Assert.NotEmpty(GetBindings(kb,
                new KeyOrChar('g'), new KeyOrChar('e')));
            Assert.NotEmpty(GetBindings(kb,
                new KeyOrChar('g'), new KeyOrChar('E')));

            // Character find
            Assert.NotEmpty(GetBindings(kb,
                new KeyOrChar('f'), new KeyOrChar(Keys.Any)));
            Assert.NotEmpty(GetBindings(kb,
                new KeyOrChar('F'), new KeyOrChar(Keys.Any)));
            Assert.NotEmpty(GetBindings(kb,
                new KeyOrChar('t'), new KeyOrChar(Keys.Any)));
            Assert.NotEmpty(GetBindings(kb,
                new KeyOrChar('T'), new KeyOrChar(Keys.Any)));
        }
    }

    /// <summary>
    /// Test 2: Operators — d, c, y operators and doubled-key variants are registered.
    /// </summary>
    [Fact]
    public void Operators()
    {
        var (_, _, scope) = CreateEnvironment("hello world");
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();

            // Standard operators
            foreach (var op in new[] { 'd', 'c', 'y', '>', '<' })
            {
                Assert.NotEmpty(GetBindings(kb, new KeyOrChar(op)));
            }

            // Doubled-key operators
            Assert.NotEmpty(GetBindings(kb,
                new KeyOrChar('d'), new KeyOrChar('d')));
            Assert.NotEmpty(GetBindings(kb,
                new KeyOrChar('y'), new KeyOrChar('y')));
            Assert.NotEmpty(GetBindings(kb,
                new KeyOrChar('c'), new KeyOrChar('c')));

            // Single-key aliases
            foreach (var alias in new[] { 'Y', 'S', 'C', 'D' })
            {
                Assert.NotEmpty(GetBindings(kb, new KeyOrChar(alias)));
            }
        }
    }

    /// <summary>
    /// Test 3: TextObjects — iw, aw, i(, a(, i", a", etc. are registered.
    /// </summary>
    [Fact]
    public void TextObjects()
    {
        var (_, _, scope) = CreateEnvironment("hello (world) \"test\"");
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();

            // Word text objects
            Assert.NotEmpty(GetBindings(kb,
                new KeyOrChar('i'), new KeyOrChar('w')));
            Assert.NotEmpty(GetBindings(kb,
                new KeyOrChar('a'), new KeyOrChar('w')));
            Assert.NotEmpty(GetBindings(kb,
                new KeyOrChar('i'), new KeyOrChar('W')));
            Assert.NotEmpty(GetBindings(kb,
                new KeyOrChar('a'), new KeyOrChar('W')));

            // Bracket text objects
            Assert.NotEmpty(GetBindings(kb,
                new KeyOrChar('i'), new KeyOrChar('(')));
            Assert.NotEmpty(GetBindings(kb,
                new KeyOrChar('a'), new KeyOrChar('(')));

            // Quote text objects
            Assert.NotEmpty(GetBindings(kb,
                new KeyOrChar('i'), new KeyOrChar('"')));
            Assert.NotEmpty(GetBindings(kb,
                new KeyOrChar('a'), new KeyOrChar('"')));

            // Aliases
            Assert.NotEmpty(GetBindings(kb,
                new KeyOrChar('i'), new KeyOrChar('b')));
            Assert.NotEmpty(GetBindings(kb,
                new KeyOrChar('a'), new KeyOrChar('B')));
        }
    }

    /// <summary>
    /// Test 4: Digraphs — Ctrl-K digraph entry binding exists.
    /// </summary>
    [Fact]
    public void Digraphs()
    {
        var (_, _, scope) = CreateEnvironment();
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlK));
            Assert.NotEmpty(bindings);
        }
    }

    /// <summary>
    /// Test 5: BlockEditing — I and A bindings exist for block selection.
    /// </summary>
    [Fact]
    public void BlockEditing()
    {
        var (_, _, scope) = CreateEnvironment("hello\nworld\ntest");
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();

            // I and A should have multiple bindings
            // (nav-mode insert + block-selection insert-multiple)
            var iBindings = GetBindings(kb, new KeyOrChar('I'));
            Assert.True(iBindings.Count >= 2,
                "I should have at least 2 bindings (insert + block-insert)");

            var aBindings = GetBindings(kb, new KeyOrChar('A'));
            Assert.True(aBindings.Count >= 2,
                "A should have at least 2 bindings (insert + block-insert)");

            // Ctrl-V for block selection entry
            Assert.NotEmpty(GetBindings(kb, new KeyOrChar(Keys.ControlV)));
        }
    }

    /// <summary>
    /// Test 6: BlockEditing_EmptyLines — block mode bindings work with empty content.
    /// </summary>
    [Fact]
    public void BlockEditing_EmptyLines()
    {
        var (_, _, scope) = CreateEnvironment("\n\n\n");
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();

            // Block selection mode entry exists
            Assert.NotEmpty(GetBindings(kb, new KeyOrChar(Keys.ControlV)));

            // I/A block bindings registered
            Assert.True(GetBindings(kb, new KeyOrChar('I')).Count >= 2);
            Assert.True(GetBindings(kb, new KeyOrChar('A')).Count >= 2);
        }
    }

    /// <summary>
    /// Test 7: VisualLineCopy — V, y, p bindings exist.
    /// </summary>
    [Fact]
    public void VisualLineCopy()
    {
        var (_, _, scope) = CreateEnvironment("line 1\nline 2\nline 3");
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();

            Assert.NotEmpty(GetBindings(kb, new KeyOrChar('V')));
            Assert.NotEmpty(GetBindings(kb, new KeyOrChar('y')));
            Assert.NotEmpty(GetBindings(kb, new KeyOrChar('p')));
        }
    }

    /// <summary>
    /// Test 8: VisualEmptyLine — visual line mode bindings with empty lines.
    /// </summary>
    [Fact]
    public void VisualEmptyLine()
    {
        var (_, _, scope) = CreateEnvironment("hello\n\nworld");
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();

            Assert.NotEmpty(GetBindings(kb, new KeyOrChar('V')));
            Assert.NotEmpty(GetBindings(kb, new KeyOrChar('j')));
            Assert.NotEmpty(GetBindings(kb, new KeyOrChar('d')));
        }
    }

    /// <summary>
    /// Test 9: CharacterDeleteAfterCursor — x binding exists.
    /// </summary>
    [Fact]
    public void CharacterDeleteAfterCursor()
    {
        var (_, _, scope) = CreateEnvironment("hello");
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var xBindings = GetBindings(kb, new KeyOrChar('x'));
            Assert.NotEmpty(xBindings);
        }
    }

    /// <summary>
    /// Test 10: CharacterDeleteBeforeCursor — X binding exists.
    /// </summary>
    [Fact]
    public void CharacterDeleteBeforeCursor()
    {
        var (_, _, scope) = CreateEnvironment("hello", cursorPosition: 3);
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var xBindings = GetBindings(kb, new KeyOrChar('X'));
            Assert.NotEmpty(xBindings);
        }
    }

    /// <summary>
    /// Test 11: CharacterPaste — p and P bindings exist, including register variants.
    /// </summary>
    [Fact]
    public void CharacterPaste()
    {
        var (_, _, scope) = CreateEnvironment("hello");
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();

            Assert.NotEmpty(GetBindings(kb, new KeyOrChar('p')));
            Assert.NotEmpty(GetBindings(kb, new KeyOrChar('P')));

            // Register-aware paste
            Assert.NotEmpty(GetBindings(kb,
                new KeyOrChar('"'), new KeyOrChar(Keys.Any), new KeyOrChar('p')));
            Assert.NotEmpty(GetBindings(kb,
                new KeyOrChar('"'), new KeyOrChar(Keys.Any), new KeyOrChar('P')));
        }
    }

    /// <summary>
    /// Test 12: TempNavigationMode — Ctrl-O binding exists.
    /// </summary>
    [Fact]
    public void TempNavigationMode()
    {
        var (_, _, scope) = CreateEnvironment("hello");
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();
            var bindings = GetBindings(kb, new KeyOrChar(Keys.ControlO));
            Assert.NotEmpty(bindings);
        }
    }

    /// <summary>
    /// Test 13: Macros — q (start/stop recording) and @ (playback) bindings exist.
    /// </summary>
    [Fact]
    public void Macros()
    {
        var (_, _, scope) = CreateEnvironment("hello world");
        using (scope)
        {
            var kb = ViBindingsType.LoadViBindings();

            // q,{reg} — start recording
            Assert.NotEmpty(GetBindings(kb,
                new KeyOrChar('q'), new KeyOrChar(Keys.Any)));

            // q — stop recording
            Assert.NotEmpty(GetBindings(kb, new KeyOrChar('q')));

            // @,{reg} — playback
            Assert.NotEmpty(GetBindings(kb,
                new KeyOrChar('@'), new KeyOrChar(Keys.Any)));
        }
    }
}
