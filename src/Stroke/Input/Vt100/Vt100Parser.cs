using System.Text;
using System.Text.RegularExpressions;

namespace Stroke.Input.Vt100;

/// <summary>
/// Parser for VT100/ANSI escape sequences.
/// </summary>
/// <remarks>
/// <para>
/// This parser converts raw terminal input into <see cref="KeyPress"/> events.
/// It handles regular characters, control characters, escape sequences, mouse events,
/// cursor position reports, and bracketed paste mode.
/// </para>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's
/// <c>prompt_toolkit.input.vt100_parser.Vt100Parser</c>.
/// </para>
/// <para>
/// Thread safety: This class is not thread-safe. Single-threaded access is assumed.
/// </para>
/// </remarks>
public sealed class Vt100Parser
{
    private const int MaxBufferSize = 256;
    private const string BracketedPasteStart = "\x1b[200~";
    private const string BracketedPasteEnd = "\x1b[201~";

    private static readonly Regex X10MousePattern = new(@"^\x1b\[M...$", RegexOptions.Compiled);
    private static readonly Regex SgrMousePattern = new(@"^\x1b\[<[\d;]+[mM]$", RegexOptions.Compiled);
    private static readonly Regex UrxvtMousePattern = new(@"^\x1b\[\d+;\d+;\d+M$", RegexOptions.Compiled);
    private static readonly Regex CprPattern = new(@"^\x1b\[\d+;\d+R$", RegexOptions.Compiled);

    private readonly Action<KeyPress> _feedKeyCallback;
    private readonly StringBuilder _buffer = new();
    private readonly StringBuilder _pasteBuffer = new();
    private ParserState _state = ParserState.Ground;
    private bool _inBracketedPaste;

    /// <summary>
    /// Initializes a new instance of the <see cref="Vt100Parser"/> class.
    /// </summary>
    /// <param name="feedKeyCallback">A callback invoked for each parsed <see cref="KeyPress"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="feedKeyCallback"/> is null.</exception>
    public Vt100Parser(Action<KeyPress> feedKeyCallback)
    {
        _feedKeyCallback = feedKeyCallback ?? throw new ArgumentNullException(nameof(feedKeyCallback));
    }

    /// <summary>
    /// Feeds input data into the parser.
    /// </summary>
    /// <param name="data">The input characters to parse.</param>
    /// <remarks>
    /// <para>
    /// Characters are processed sequentially. Complete escape sequences result in
    /// callback invocations. Incomplete sequences are buffered until more data
    /// arrives or <see cref="Flush"/> is called.
    /// </para>
    /// <para>
    /// Bracketed paste mode markers accumulate content and emit as a single
    /// <see cref="Keys.BracketedPaste"/> key press.
    /// </para>
    /// </remarks>
    public void Feed(string data)
    {
        ArgumentNullException.ThrowIfNull(data);

        foreach (char c in data)
        {
            ProcessChar(c);
        }
    }

    /// <summary>
    /// Flushes any pending partial escape sequences.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Critical for detecting standalone Escape key presses. When ESC is received,
    /// the parser waits for additional characters that might form an escape sequence.
    /// If a timeout occurs and no additional characters arrive, call this method to
    /// emit the buffered content as individual key presses.
    /// </para>
    /// <para>
    /// Recommended timeout: 50-100ms after last input before calling flush.
    /// </para>
    /// </remarks>
    public void Flush()
    {
        if (_buffer.Length == 0)
            return;

        // If we're in the middle of an escape sequence, emit buffered content
        var buffered = _buffer.ToString();
        _buffer.Clear();
        _state = ParserState.Ground;

        // Emit each character as either escape or literal
        if (buffered.StartsWith('\x1b'))
        {
            // First character is ESC
            _feedKeyCallback(new KeyPress(Keys.Escape, "\x1b"));

            // Remaining characters are literal
            for (int i = 1; i < buffered.Length; i++)
            {
                EmitCharacter(buffered[i]);
            }
        }
        else
        {
            // All characters are literal
            foreach (char c in buffered)
            {
                EmitCharacter(c);
            }
        }
    }

    /// <summary>
    /// Feeds input data and immediately flushes.
    /// </summary>
    /// <param name="data">The input characters to parse.</param>
    /// <remarks>
    /// Convenience method equivalent to calling <see cref="Feed"/> followed by
    /// <see cref="Flush"/>. Useful when all input is available at once (e.g., in tests).
    /// </remarks>
    public void FeedAndFlush(string data)
    {
        Feed(data);
        Flush();
    }

    /// <summary>
    /// Resets the parser to its initial state.
    /// </summary>
    /// <param name="request">If true, also clears any pending request state.</param>
    /// <remarks>
    /// Discards any buffered partial sequences without emitting them.
    /// Use this when recovering from an error or starting fresh.
    /// </remarks>
    public void Reset(bool request = false)
    {
        _buffer.Clear();
        _state = ParserState.Ground;

        if (request)
        {
            _pasteBuffer.Clear();
            _inBracketedPaste = false;
        }
    }

    private void ProcessChar(char c)
    {
        // Handle bracketed paste mode specially
        if (_inBracketedPaste)
        {
            ProcessBracketedPasteChar(c);
            return;
        }

        // Check buffer overflow
        if (_buffer.Length >= MaxBufferSize)
        {
            // Emit buffered content and reset
            Flush();
        }

        switch (_state)
        {
            case ParserState.Ground:
                ProcessGroundState(c);
                break;

            case ParserState.Escape:
                ProcessEscapeState(c);
                break;

            case ParserState.CsiEntry:
            case ParserState.CsiParam:
            case ParserState.CsiIntermediate:
                ProcessCsiState(c);
                break;

            case ParserState.Ss3:
                ProcessSs3State(c);
                break;

            case ParserState.OscString:
                ProcessOscState(c);
                break;

            case ParserState.SosPmApcString:
                ProcessSosPmApcState(c);
                break;

            case ParserState.X10Mouse:
                ProcessX10MouseState(c);
                break;
        }
    }

    private void ProcessGroundState(char c)
    {
        if (c == '\x1b')
        {
            // Start of escape sequence
            _buffer.Append(c);
            _state = ParserState.Escape;
        }
        else if (c < 32)
        {
            // Control character
            EmitControlCharacter(c);
        }
        else
        {
            // Regular printable character
            EmitCharacter(c);
        }
    }

    private void ProcessEscapeState(char c)
    {
        _buffer.Append(c);

        switch (c)
        {
            case '[':
                // CSI sequence
                _state = ParserState.CsiEntry;
                break;

            case 'O':
                // SS3 sequence (F1-F4, keypad)
                _state = ParserState.Ss3;
                break;

            case ']':
                // OSC sequence (Operating System Command)
                _state = ParserState.OscString;
                break;

            case 'P':
            case 'X':
            case '^':
            case '_':
                // DCS, SOS, PM, APC strings
                _state = ParserState.SosPmApcString;
                break;

            default:
                // Unknown escape sequence or two-character sequence
                // Try to match as complete sequence
                var sequence = _buffer.ToString();
                if (AnsiSequences.TryGetKey(sequence, out var key))
                {
                    _feedKeyCallback(new KeyPress(key, sequence));
                    _buffer.Clear();
                    _state = ParserState.Ground;
                }
                else if (!AnsiSequences.IsPrefixOfLongerSequence(sequence))
                {
                    // Not a valid prefix, emit ESC and literal char
                    Flush();
                }
                // else: valid prefix, keep waiting
                break;
        }
    }

    private void ProcessCsiState(char c)
    {
        _buffer.Append(c);
        var sequence = _buffer.ToString();

        // Check for final byte (0x40-0x7E)
        if (c >= 0x40 && c <= 0x7E)
        {
            // Special case: X10 mouse protocol is \x1b[M followed by 3 data bytes
            // When we see exactly \x1b[M, we need to wait for 3 more bytes
            if (sequence == "\x1b[M")
            {
                _state = ParserState.X10Mouse;
                return;
            }

            // End of CSI sequence
            if (TryEmitCsiSequence(sequence))
            {
                _buffer.Clear();
                _state = ParserState.Ground;
            }
            else
            {
                // Unknown CSI sequence
                Flush();
            }
        }
        else if (c >= 0x30 && c <= 0x3F)
        {
            // Parameter byte (0-9, ;, <, =, >, ?)
            _state = ParserState.CsiParam;
        }
        else if (c >= 0x20 && c <= 0x2F)
        {
            // Intermediate byte
            _state = ParserState.CsiIntermediate;
        }
        else if (c < 0x20)
        {
            // Control character in middle of sequence - invalid
            Flush();
            ProcessGroundState(c);
        }
    }

    private void ProcessSs3State(char c)
    {
        _buffer.Append(c);
        var sequence = _buffer.ToString();

        // SS3 sequences are \x1bO followed by a single character
        if (AnsiSequences.TryGetKey(sequence, out var key))
        {
            _feedKeyCallback(new KeyPress(key, sequence));
            _buffer.Clear();
            _state = ParserState.Ground;
        }
        else
        {
            // Unknown SS3 sequence
            Flush();
        }
    }

    private void ProcessOscState(char c)
    {
        _buffer.Append(c);

        // OSC sequences end with BEL (\x07) or ST (\x1b\\)
        if (c == '\x07')
        {
            // Discard OSC string (no key event)
            _buffer.Clear();
            _state = ParserState.Ground;
        }
        else if (_buffer.Length >= 2 && _buffer[^2] == '\x1b' && c == '\\')
        {
            // String Terminator (ST)
            _buffer.Clear();
            _state = ParserState.Ground;
        }
    }

    private void ProcessSosPmApcState(char c)
    {
        _buffer.Append(c);

        // These strings end with ST (\x1b\\)
        if (_buffer.Length >= 2 && _buffer[^2] == '\x1b' && c == '\\')
        {
            // Discard string (no key event)
            _buffer.Clear();
            _state = ParserState.Ground;
        }
    }

    private void ProcessX10MouseState(char c)
    {
        _buffer.Append(c);

        // X10 mouse format: \x1b[M followed by 3 data bytes (button, x, y)
        // Total sequence length is 6 bytes
        if (_buffer.Length >= 6)
        {
            var sequence = _buffer.ToString();
            _feedKeyCallback(new KeyPress(Keys.Vt100MouseEvent, sequence));
            _buffer.Clear();
            _state = ParserState.Ground;
        }
    }

    private void ProcessBracketedPasteChar(char c)
    {
        _pasteBuffer.Append(c);

        // Check for end sequence
        if (_pasteBuffer.Length >= BracketedPasteEnd.Length)
        {
            var end = _pasteBuffer.ToString()[^BracketedPasteEnd.Length..];
            if (end == BracketedPasteEnd)
            {
                // End of paste
                var content = _pasteBuffer.ToString()[..^BracketedPasteEnd.Length];
                _feedKeyCallback(new KeyPress(Keys.BracketedPaste, content));
                _pasteBuffer.Clear();
                _inBracketedPaste = false;
            }
        }
    }

    private bool TryEmitCsiSequence(string sequence)
    {
        // Check for bracketed paste start/end first
        if (sequence == BracketedPasteStart)
        {
            _feedKeyCallback(new KeyPress(Keys.BracketedPaste, sequence));
            _inBracketedPaste = true;
            _pasteBuffer.Clear();
            return true;
        }

        if (sequence == BracketedPasteEnd)
        {
            _feedKeyCallback(new KeyPress(Keys.BracketedPaste, sequence));
            return true;
        }

        // Check for known sequence
        if (AnsiSequences.TryGetKey(sequence, out var key))
        {
            _feedKeyCallback(new KeyPress(key, sequence));
            return true;
        }

        // Check for mouse events (variable coordinates)
        if (TryParseMouseEvent(sequence))
        {
            return true;
        }

        // Check for CPR response
        if (TryParseCprResponse(sequence))
        {
            return true;
        }

        // Check for modifier combinations not in dictionary
        if (TryParseModifierSequence(sequence))
        {
            return true;
        }

        return false;
    }

    private bool TryParseMouseEvent(string sequence)
    {
        if (X10MousePattern.IsMatch(sequence) ||
            SgrMousePattern.IsMatch(sequence) ||
            UrxvtMousePattern.IsMatch(sequence))
        {
            _feedKeyCallback(new KeyPress(Keys.Vt100MouseEvent, sequence));
            return true;
        }

        return false;
    }

    private bool TryParseCprResponse(string sequence)
    {
        if (CprPattern.IsMatch(sequence))
        {
            _feedKeyCallback(new KeyPress(Keys.CPRResponse, sequence));
            return true;
        }

        return false;
    }

    private bool TryParseModifierSequence(string sequence)
    {
        // Handle modifier combinations: \x1b[1;{modifier}{final}
        // Modifiers: 2=Shift, 3=Alt, 4=Shift+Alt, 5=Ctrl, 6=Ctrl+Shift, 7=Alt+Ctrl, 8=All
        if (sequence.Length >= 6 && sequence.StartsWith("\x1b[1;"))
        {
            // Already handled in AnsiSequences, but catch any we missed
            // For now, return false and let Flush handle it
        }

        return false;
    }

    private void EmitControlCharacter(char c)
    {
        var key = c switch
        {
            '\x00' => Keys.ControlAt,
            '\x01' => Keys.ControlA,
            '\x02' => Keys.ControlB,
            '\x03' => Keys.ControlC,
            '\x04' => Keys.ControlD,
            '\x05' => Keys.ControlE,
            '\x06' => Keys.ControlF,
            '\x07' => Keys.ControlG,
            '\x08' => Keys.ControlH,
            '\x09' => Keys.ControlI,
            '\x0a' => Keys.ControlJ,
            '\x0b' => Keys.ControlK,
            '\x0c' => Keys.ControlL,
            '\x0d' => Keys.ControlM,
            '\x0e' => Keys.ControlN,
            '\x0f' => Keys.ControlO,
            '\x10' => Keys.ControlP,
            '\x11' => Keys.ControlQ,
            '\x12' => Keys.ControlR,
            '\x13' => Keys.ControlS,
            '\x14' => Keys.ControlT,
            '\x15' => Keys.ControlU,
            '\x16' => Keys.ControlV,
            '\x17' => Keys.ControlW,
            '\x18' => Keys.ControlX,
            '\x19' => Keys.ControlY,
            '\x1a' => Keys.ControlZ,
            '\x1b' => Keys.Escape,
            '\x1c' => Keys.ControlBackslash,
            '\x1d' => Keys.ControlSquareClose,
            '\x1e' => Keys.ControlCircumflex,
            '\x1f' => Keys.ControlUnderscore,
            _ => Keys.Any
        };

        _feedKeyCallback(new KeyPress(key, c.ToString()));
    }

    private void EmitCharacter(char c)
    {
        _feedKeyCallback(new KeyPress(Keys.Any, c.ToString()));
    }

    /// <summary>
    /// Internal parser state enumeration.
    /// </summary>
    private enum ParserState
    {
        /// <summary>Normal state, processing characters.</summary>
        Ground,

        /// <summary>After ESC, waiting for sequence type.</summary>
        Escape,

        /// <summary>After CSI (ESC[), waiting for parameters.</summary>
        CsiEntry,

        /// <summary>Processing CSI parameters.</summary>
        CsiParam,

        /// <summary>Processing CSI intermediates.</summary>
        CsiIntermediate,

        /// <summary>After SS3 (ESC O), waiting for final character.</summary>
        Ss3,

        /// <summary>Processing OSC string.</summary>
        OscString,

        /// <summary>Processing SOS/PM/APC string.</summary>
        SosPmApcString,

        /// <summary>After CSI M (X10 mouse), waiting for 3 data bytes.</summary>
        X10Mouse
    }
}
