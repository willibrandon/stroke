using Stroke.Core;
using Stroke.Filters;
using Stroke.Input;
using Stroke.KeyBinding;
using Stroke.KeyBinding.Bindings;
using Stroke.Layout.Containers;

namespace Stroke.Application.Bindings;

public static partial class ViBindings
{
    /// <summary>
    /// Registers all Vi text object bindings.
    /// </summary>
    static partial void RegisterTextObjects(KeyBindings kb)
    {
        RegisterWordMotions(kb);
        RegisterLineMotions(kb);
        RegisterDocumentScreenMotions(kb);
        RegisterCharacterFindMotions(kb);
        RegisterSearchMotions(kb);
        RegisterCursorMotions(kb);
        RegisterInnerAroundTextObjects(kb);
        RegisterBracketQuoteTextObjects(kb);
    }

    // ============================================================
    // Word motions (T014)
    // ============================================================

    private static void RegisterWordMotions(KeyBindings kb)
    {
        // w — word forward
        RegisterTextObject(kb, [new KeyOrChar('w')], (@event) =>
        {
            var doc = @event.CurrentBuffer!.Document;
            return new TextObject(
                doc.FindNextWordBeginning(count: @event.Arg)
                ?? doc.GetEndOfDocumentPosition());
        });

        // W — WORD forward
        RegisterTextObject(kb, [new KeyOrChar('W')], (@event) =>
        {
            var doc = @event.CurrentBuffer!.Document;
            return new TextObject(
                doc.FindNextWordBeginning(count: @event.Arg, WORD: true)
                ?? doc.GetEndOfDocumentPosition());
        });

        // b — word backward
        RegisterTextObject(kb, [new KeyOrChar('b')], (@event) =>
        {
            return new TextObject(
                @event.CurrentBuffer!.Document.FindStartOfPreviousWord(count: @event.Arg)
                ?? 0);
        });

        // B — WORD backward
        RegisterTextObject(kb, [new KeyOrChar('B')], (@event) =>
        {
            return new TextObject(
                @event.CurrentBuffer!.Document.FindStartOfPreviousWord(
                    count: @event.Arg, WORD: true)
                ?? 0);
        });

        // e — end of word
        RegisterTextObject(kb, [new KeyOrChar('e')], (@event) =>
        {
            var end = @event.CurrentBuffer!.Document.FindNextWordEnding(count: @event.Arg);
            return new TextObject(
                end.HasValue ? end.Value - 1 : 0,
                type: TextObjectType.Inclusive);
        });

        // E — end of WORD
        RegisterTextObject(kb, [new KeyOrChar('E')], (@event) =>
        {
            var end = @event.CurrentBuffer!.Document.FindNextWordEnding(
                count: @event.Arg, WORD: true);
            return new TextObject(
                end.HasValue ? end.Value - 1 : 0,
                type: TextObjectType.Inclusive);
        });

        // ge — end of previous word
        RegisterTextObject(kb, [new KeyOrChar('g'), new KeyOrChar('e')], (@event) =>
        {
            var prevEnd = @event.CurrentBuffer!.Document.FindPreviousWordEnding(
                count: @event.Arg);
            return new TextObject(
                prevEnd.HasValue ? prevEnd.Value - 1 : 0,
                type: TextObjectType.Inclusive);
        });

        // gE — end of previous WORD
        RegisterTextObject(kb, [new KeyOrChar('g'), new KeyOrChar('E')], (@event) =>
        {
            var prevEnd = @event.CurrentBuffer!.Document.FindPreviousWordEnding(
                count: @event.Arg, WORD: true);
            return new TextObject(
                prevEnd.HasValue ? prevEnd.Value - 1 : 0,
                type: TextObjectType.Inclusive);
        });
    }

    // ============================================================
    // Line motions (T015)
    // ============================================================

    private static void RegisterLineMotions(KeyBindings kb)
    {
        // 0 — hard start of line
        RegisterTextObject(kb, [new KeyOrChar('0')], (@event) =>
        {
            return new TextObject(
                @event.CurrentBuffer!.Document.GetStartOfLinePosition(afterWhitespace: false));
        });

        // ^ — soft start of line (after whitespace)
        RegisterTextObject(kb, [new KeyOrChar('^')], (@event) =>
        {
            return new TextObject(
                @event.CurrentBuffer!.Document.GetStartOfLinePosition(afterWhitespace: true));
        });

        // $ — end of line
        RegisterTextObject(kb, [new KeyOrChar('$')], (@event) =>
        {
            return new TextObject(
                @event.CurrentBuffer!.Document.GetEndOfLinePosition());
        });

        // | — go to column
        RegisterTextObject(kb, [new KeyOrChar('|')], (@event) =>
        {
            return new TextObject(
                @event.CurrentBuffer!.Document.GetColumnCursorPosition(@event.Arg - 1));
        });

        // gm — middle of line
        RegisterTextObject(kb, [new KeyOrChar('g'), new KeyOrChar('m')], (@event) =>
        {
            var w = @event.GetApp().Layout.CurrentWindow;
            var buff = @event.CurrentBuffer!;

            if (w is not null && w.RenderInfo is not null)
            {
                var width = w.RenderInfo.WindowWidth;
                var start = buff.Document.GetStartOfLinePosition(afterWhitespace: false);
                start += (int)Math.Min(width / 2.0, buff.Document.CurrentLine.Length);
                return new TextObject(start, type: TextObjectType.Inclusive);
            }
            return new TextObject(0);
        });

        // g_ — last non-blank of line
        RegisterTextObject(kb, [new KeyOrChar('g'), new KeyOrChar('_')], (@event) =>
        {
            return new TextObject(
                @event.CurrentBuffer!.Document.LastNonBlankOfCurrentLinePosition(),
                type: TextObjectType.Inclusive);
        });
    }

    // ============================================================
    // Document/screen motions (T016)
    // ============================================================

    private static void RegisterDocumentScreenMotions(KeyBindings kb)
    {
        // gg — go to first line (or given line with arg)
        RegisterTextObject(kb, [new KeyOrChar('g'), new KeyOrChar('g')], (@event) =>
        {
            var d = @event.CurrentBuffer!.Document;

            if (@event.ArgPresent)
            {
                // Move to the given line.
                return new TextObject(
                    d.TranslateRowColToIndex(@event.Arg - 1, 0) - d.CursorPosition,
                    type: TextObjectType.Linewise);
            }
            else
            {
                // Move to the top of the input.
                return new TextObject(
                    d.GetStartOfDocumentPosition(),
                    type: TextObjectType.Linewise);
            }
        });

        // G — go to last line
        RegisterTextObject(kb, [new KeyOrChar('G')], (@event) =>
        {
            var buf = @event.CurrentBuffer!;
            return new TextObject(
                buf.Document.TranslateRowColToIndex(buf.Document.LineCount - 1, 0)
                    - buf.CursorPosition,
                type: TextObjectType.Linewise);
        });

        // { — previous paragraph
        RegisterTextObject(kb, [new KeyOrChar('{')], (@event) =>
        {
            return new TextObject(
                @event.CurrentBuffer!.Document.StartOfParagraph(
                    count: @event.Arg, before: true));
        });

        // } — next paragraph
        RegisterTextObject(kb, [new KeyOrChar('}')], (@event) =>
        {
            return new TextObject(
                @event.CurrentBuffer!.Document.EndOfParagraph(
                    count: @event.Arg, after: true));
        });

        // % — matching bracket (or go to percentage with arg)
        RegisterTextObject(kb, [new KeyOrChar('%')], (@event) =>
        {
            var buffer = @event.CurrentBuffer!;

            if (@event.ArgPresent)
            {
                if (@event.Arg > 0 && @event.Arg <= 100)
                {
                    var absoluteIndex = buffer.Document.TranslateRowColToIndex(
                        (int)((@event.Arg * buffer.Document.LineCount - 1) / 100.0), 0);
                    return new TextObject(
                        absoluteIndex - buffer.Document.CursorPosition,
                        type: TextObjectType.Linewise);
                }
                else
                {
                    return new TextObject(0);
                }
            }
            else
            {
                var match = buffer.Document.FindMatchingBracketPosition();
                if (match != 0)
                {
                    return new TextObject(match, type: TextObjectType.Inclusive);
                }
                else
                {
                    return new TextObject(0);
                }
            }
        });

        // H — top of screen
        RegisterTextObject(kb, [new KeyOrChar('H')], (@event) =>
        {
            var w = @event.GetApp().Layout.CurrentWindow;
            var b = @event.CurrentBuffer!;

            if (w is not null && w.RenderInfo is not null)
            {
                var pos = b.Document.TranslateRowColToIndex(
                    w.RenderInfo.FirstVisibleLine(afterScrollOffset: true), 0)
                    - b.CursorPosition;
                return new TextObject(pos, type: TextObjectType.Linewise);
            }
            else
            {
                return new TextObject(
                    -b.Document.TextBeforeCursor.Length,
                    type: TextObjectType.Linewise);
            }
        });

        // M — middle of screen
        RegisterTextObject(kb, [new KeyOrChar('M')], (@event) =>
        {
            var w = @event.GetApp().Layout.CurrentWindow;
            var b = @event.CurrentBuffer!;

            if (w is not null && w.RenderInfo is not null)
            {
                var pos = b.Document.TranslateRowColToIndex(
                    w.RenderInfo.CenterVisibleLine(), 0)
                    - b.CursorPosition;
                return new TextObject(pos, type: TextObjectType.Linewise);
            }
            else
            {
                return new TextObject(
                    -b.Document.TextBeforeCursor.Length,
                    type: TextObjectType.Linewise);
            }
        });

        // L — bottom of screen
        RegisterTextObject(kb, [new KeyOrChar('L')], (@event) =>
        {
            var w = @event.GetApp().Layout.CurrentWindow;
            var b = @event.CurrentBuffer!;

            if (w is not null && w.RenderInfo is not null)
            {
                var pos = b.Document.TranslateRowColToIndex(
                    w.RenderInfo.LastVisibleLine(beforeScrollOffset: true), 0)
                    - b.CursorPosition;
                return new TextObject(pos, type: TextObjectType.Linewise);
            }
            else
            {
                return new TextObject(
                    b.Document.TextAfterCursor.Length,
                    type: TextObjectType.Linewise);
            }
        });
    }

    // ============================================================
    // Character find motions (T017)
    // ============================================================

    private static void RegisterCharacterFindMotions(KeyBindings kb)
    {
        // f,{char} — find next occurrence
        RegisterTextObject(kb,
            [new KeyOrChar('f'), new KeyOrChar(Keys.Any)],
            (@event) =>
            {
                @event.GetApp().ViState.LastCharacterFind =
                    new CharacterFind(@event.Data, Backwards: false);
                var match = @event.CurrentBuffer!.Document.Find(
                    @event.Data, inCurrentLine: true, count: @event.Arg);
                return match.HasValue
                    ? new TextObject(match.Value, type: TextObjectType.Inclusive)
                    : new TextObject(0);
            });

        // F,{char} — find previous occurrence
        RegisterTextObject(kb,
            [new KeyOrChar('F'), new KeyOrChar(Keys.Any)],
            (@event) =>
            {
                @event.GetApp().ViState.LastCharacterFind =
                    new CharacterFind(@event.Data, Backwards: true);
                return new TextObject(
                    @event.CurrentBuffer!.Document.FindBackwards(
                        @event.Data, inCurrentLine: true, count: @event.Arg) ?? 0);
            });

        // t,{char} — till next occurrence
        RegisterTextObject(kb,
            [new KeyOrChar('t'), new KeyOrChar(Keys.Any)],
            (@event) =>
            {
                @event.GetApp().ViState.LastCharacterFind =
                    new CharacterFind(@event.Data, Backwards: false);
                var match = @event.CurrentBuffer!.Document.Find(
                    @event.Data, inCurrentLine: true, count: @event.Arg);
                return match.HasValue
                    ? new TextObject(match.Value - 1, type: TextObjectType.Inclusive)
                    : new TextObject(0);
            });

        // T,{char} — till previous occurrence
        RegisterTextObject(kb,
            [new KeyOrChar('T'), new KeyOrChar(Keys.Any)],
            (@event) =>
            {
                @event.GetApp().ViState.LastCharacterFind =
                    new CharacterFind(@event.Data, Backwards: true);
                var match = @event.CurrentBuffer!.Document.FindBackwards(
                    @event.Data, inCurrentLine: true, count: @event.Arg);
                return new TextObject(match.HasValue ? match.Value + 1 : 0);
            });

        // ; — repeat last find
        RegisterRepeatFind(kb, reverse: false);

        // , — reverse last find
        RegisterRepeatFind(kb, reverse: true);
    }

    private static void RegisterRepeatFind(KeyBindings kb, bool reverse)
    {
        var key = reverse ? ',' : ';';

        RegisterTextObject(kb, [new KeyOrChar(key)], (@event) =>
        {
            int? pos = 0;
            var viState = @event.GetApp().ViState;
            var type = TextObjectType.Exclusive;

            if (viState.LastCharacterFind is not null)
            {
                var chr = viState.LastCharacterFind.Character;
                var backwards = viState.LastCharacterFind.Backwards;

                if (reverse)
                    backwards = !backwards;

                if (backwards)
                {
                    pos = @event.CurrentBuffer!.Document.FindBackwards(
                        chr, inCurrentLine: true, count: @event.Arg);
                }
                else
                {
                    pos = @event.CurrentBuffer!.Document.Find(
                        chr, inCurrentLine: true, count: @event.Arg);
                    type = TextObjectType.Inclusive;
                }
            }

            return pos.HasValue && pos.Value != 0
                ? new TextObject(pos.Value, type: type)
                : new TextObject(0);
        });
    }

    // ============================================================
    // Search motions (T018)
    // ============================================================

    private static void RegisterSearchMotions(KeyBindings kb)
    {
        // n — search next (text object for operator-pending mode)
        RegisterTextObject(kb,
            [new KeyOrChar('n')],
            (@event) =>
            {
                var buff = @event.CurrentBuffer!;
                var searchState = @event.GetApp().CurrentSearchState;

                var cursorPosition = buff.GetSearchPosition(
                    searchState, includeCurrentPosition: false, count: @event.Arg);
                return new TextObject(cursorPosition - buff.CursorPosition);
            },
            noMoveHandler: true);

        // n — search next (navigation mode: apply search through history)
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('n')],
            filter: new FilterOrBool(ViFilters.ViNavigationMode))(
            (@event) =>
            {
                var searchState = @event.GetApp().CurrentSearchState;
                @event.CurrentBuffer!.ApplySearch(
                    searchState, includeCurrentPosition: false, count: @event.Arg);
                return null;
            });

        // N — search previous (text object for operator-pending mode)
        RegisterTextObject(kb,
            [new KeyOrChar('N')],
            (@event) =>
            {
                var buff = @event.CurrentBuffer!;
                var searchState = ~@event.GetApp().CurrentSearchState;

                var cursorPosition = buff.GetSearchPosition(
                    searchState, includeCurrentPosition: false, count: @event.Arg);
                return new TextObject(cursorPosition - buff.CursorPosition);
            },
            noMoveHandler: true);

        // N — search previous (navigation mode: apply search through history)
        kb.Add<KeyHandlerCallable>(
            [new KeyOrChar('N')],
            filter: new FilterOrBool(ViFilters.ViNavigationMode))(
            (@event) =>
            {
                var searchState = ~@event.GetApp().CurrentSearchState;
                @event.CurrentBuffer!.ApplySearch(
                    searchState, includeCurrentPosition: false, count: @event.Arg);
                return null;
            });
    }

    // ============================================================
    // Cursor motions h/l (T019)
    // ============================================================

    private static void RegisterCursorMotions(KeyBindings kb)
    {
        // h — move left
        RegisterTextObject(kb, [new KeyOrChar('h')], (@event) =>
        {
            return new TextObject(
                @event.CurrentBuffer!.Document.GetCursorLeftPosition(count: @event.Arg));
        });

        // Left — move left
        RegisterTextObject(kb, [new KeyOrChar(Keys.Left)], (@event) =>
        {
            return new TextObject(
                @event.CurrentBuffer!.Document.GetCursorLeftPosition(count: @event.Arg));
        });

        // l — move right
        RegisterTextObject(kb, [new KeyOrChar('l')], (@event) =>
        {
            return new TextObject(
                @event.CurrentBuffer!.Document.GetCursorRightPosition(count: @event.Arg));
        });

        // Space — move right
        RegisterTextObject(kb, [new KeyOrChar(' ')], (@event) =>
        {
            return new TextObject(
                @event.CurrentBuffer!.Document.GetCursorRightPosition(count: @event.Arg));
        });

        // Right — move right
        RegisterTextObject(kb, [new KeyOrChar(Keys.Right)], (@event) =>
        {
            return new TextObject(
                @event.CurrentBuffer!.Document.GetCursorRightPosition(count: @event.Arg));
        });

        // j — down (text object, Linewise, noMoveHandler=true, noSelectionHandler=true)
        RegisterTextObject(kb, [new KeyOrChar('j')], (@event) =>
        {
            return new TextObject(
                @event.CurrentBuffer!.Document.GetCursorDownPosition(count: @event.Arg),
                type: TextObjectType.Linewise);
        }, noMoveHandler: true, noSelectionHandler: true);

        // k — up (text object, Linewise, noMoveHandler=true, noSelectionHandler=true)
        RegisterTextObject(kb, [new KeyOrChar('k')], (@event) =>
        {
            return new TextObject(
                @event.CurrentBuffer!.Document.GetCursorUpPosition(count: @event.Arg),
                type: TextObjectType.Linewise);
        }, noMoveHandler: true, noSelectionHandler: true);
    }

    // ============================================================
    // Inner/Around text objects — word, WORD, paragraph (T031)
    // ============================================================

    private static void RegisterInnerAroundTextObjects(KeyBindings kb)
    {
        // iw — inner word
        RegisterTextObject(kb,
            [new KeyOrChar('i'), new KeyOrChar('w')],
            (@event) =>
            {
                var (start, end) = @event.CurrentBuffer!.Document
                    .FindBoundariesOfCurrentWord();
                return new TextObject(start, end);
            },
            noMoveHandler: true);

        // aw — a word (with trailing whitespace)
        RegisterTextObject(kb,
            [new KeyOrChar('a'), new KeyOrChar('w')],
            (@event) =>
            {
                var (start, end) = @event.CurrentBuffer!.Document
                    .FindBoundariesOfCurrentWord(includeTrailingWhitespace: true);
                return new TextObject(start, end);
            },
            noMoveHandler: true);

        // iW — inner WORD
        RegisterTextObject(kb,
            [new KeyOrChar('i'), new KeyOrChar('W')],
            (@event) =>
            {
                var (start, end) = @event.CurrentBuffer!.Document
                    .FindBoundariesOfCurrentWord(WORD: true);
                return new TextObject(start, end);
            },
            noMoveHandler: true);

        // aW — a WORD (with trailing whitespace)
        RegisterTextObject(kb,
            [new KeyOrChar('a'), new KeyOrChar('W')],
            (@event) =>
            {
                var (start, end) = @event.CurrentBuffer!.Document
                    .FindBoundariesOfCurrentWord(WORD: true, includeTrailingWhitespace: true);
                return new TextObject(start, end);
            },
            noMoveHandler: true);

        // ap — a paragraph
        RegisterTextObject(kb,
            [new KeyOrChar('a'), new KeyOrChar('p')],
            (@event) =>
            {
                var start = @event.CurrentBuffer!.Document.StartOfParagraph();
                var end = @event.CurrentBuffer!.Document.EndOfParagraph(count: @event.Arg);
                return new TextObject(start, end);
            },
            noMoveHandler: true);
    }

    // ============================================================
    // Bracket/quote text objects — ci", da(, etc. (T032)
    // ============================================================

    private static void RegisterBracketQuoteTextObjects(KeyBindings kb)
    {
        foreach (var inner in new[] { true, false })
        {
            // Quotes: ", ', `
            foreach (var quote in new[] { '"', '\'', '`' })
            {
                CreateCiCaHandles(kb, quote, quote, inner);
            }

            // Brackets: [], <>, {}, ()
            foreach (var (ciStart, ciEnd) in new[] { ('[', ']'), ('<', '>'), ('{', '}'), ('(', ')') })
            {
                CreateCiCaHandles(kb, ciStart, ciEnd, inner);
            }

            // Aliases: b = (, B = {
            CreateCiCaHandles(kb, '(', ')', inner, key: 'b');
            CreateCiCaHandles(kb, '{', '}', inner, key: 'B');
        }
    }

    private static void CreateCiCaHandles(
        KeyBindings kb, char ciStart, char ciEnd, bool inner, char? key = null)
    {
        Func<KeyPressEvent, TextObject> handler = (@event) =>
        {
            int? start, end;

            if (ciStart == ciEnd)
            {
                // Quotes: search backward and forward.
                start = @event.CurrentBuffer!.Document.FindBackwards(
                    ciStart.ToString(), inCurrentLine: false);
                end = @event.CurrentBuffer!.Document.Find(
                    ciEnd.ToString(), inCurrentLine: false);
            }
            else
            {
                // Brackets: use enclosing bracket search.
                start = @event.CurrentBuffer!.Document.FindEnclosingBracketLeft(
                    ciStart, ciEnd);
                end = @event.CurrentBuffer!.Document.FindEnclosingBracketRight(
                    ciStart, ciEnd);
            }

            if (start.HasValue && end.HasValue)
            {
                var offset = inner ? 0 : 1;
                return new TextObject(start.Value + 1 - offset, end.Value + offset);
            }
            else
            {
                return new TextObject(0);
            }
        };

        var prefix = inner ? 'i' : 'a';

        if (key.HasValue)
        {
            RegisterTextObject(kb,
                [new KeyOrChar(prefix), new KeyOrChar(key.Value)],
                handler, noMoveHandler: true);
        }
        else
        {
            RegisterTextObject(kb,
                [new KeyOrChar(prefix), new KeyOrChar(ciStart)],
                handler, noMoveHandler: true);

            if (ciStart != ciEnd)
            {
                RegisterTextObject(kb,
                    [new KeyOrChar(prefix), new KeyOrChar(ciEnd)],
                    handler, noMoveHandler: true);
            }
        }
    }
}
