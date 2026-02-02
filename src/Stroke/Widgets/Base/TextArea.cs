using Stroke.AutoSuggest;
using Stroke.Completion;
using Stroke.Core;
using Stroke.Filters;
using Stroke.FormattedText;
using Stroke.Layout;
using Stroke.Layout.Containers;
using Stroke.Layout.Controls;
using Stroke.Layout.Margins;
using Stroke.Layout.Processors;
using Stroke.Layout.Windows;
using Stroke.Lexers;
using Stroke.Validation;
using Stroke.Widgets.Toolbars;

using Buffer = Stroke.Core.Buffer;

namespace Stroke.Widgets.Base;

/// <summary>
/// A simple input field. High-level abstraction over Buffer, BufferControl, and Window.
/// </summary>
/// <remarks>
/// <para>
/// This widget does have the most common options, but it does not intend to
/// cover every single use case. For more configuration options, you can
/// always build a text area manually, using a
/// <see cref="Core.Buffer"/>,
/// <see cref="BufferControl"/> and
/// <see cref="Window"/>.
/// </para>
/// <para>
/// Port of Python Prompt Toolkit's <c>TextArea</c> class from <c>widgets/base.py</c>.
/// </para>
/// </remarks>
public class TextArea : IMagicContainer
{
    // ════════════════════════════════════════════════════════════════════════
    // WRITABLE CONFIGURATION PROPERTIES
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Gets or sets the completer for auto completion.</summary>
    public ICompleter? Completer { get; set; }

    /// <summary>Gets or sets whether completion triggers while typing.</summary>
    public FilterOrBool CompleteWhileTyping { get; set; }

    /// <summary>Gets or sets the lexer for syntax highlighting.</summary>
    public ILexer? Lexer { get; set; }

    /// <summary>Gets or sets the auto-suggest provider.</summary>
    public IAutoSuggest? AutoSuggest { get; set; }

    /// <summary>Gets or sets whether the text area is read-only.</summary>
    public FilterOrBool ReadOnly { get; set; }

    /// <summary>Gets or sets whether to wrap lines.</summary>
    public FilterOrBool WrapLines { get; set; }

    /// <summary>Gets or sets the validator.</summary>
    public IValidator? Validator { get; set; }

    // ════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the buffer text.
    /// </summary>
    /// <remarks>
    /// Setting this property creates a new <see cref="Core.Document"/> with the cursor
    /// at position 0, bypassing read-only mode.
    /// </remarks>
    public string Text
    {
        get => Buffer.Text;
        set => Document = new Document(value ?? "", 0);
    }

    /// <summary>
    /// Gets or sets the buffer document (text + cursor position).
    /// </summary>
    /// <remarks>
    /// The setter uses <c>bypassReadonly: true</c> so the document can be changed
    /// programmatically even when the text area is in read-only mode.
    /// </remarks>
    public Document Document
    {
        get => Buffer.Document;
        set => Buffer.SetDocument(value, bypassReadonly: true);
    }

    /// <summary>Gets or sets the accept handler, called when the user accepts the input.</summary>
    public Func<Buffer, bool>? AcceptHandler
    {
        get => Buffer.AcceptHandler;
        set => Buffer.AcceptHandler = value;
    }

    // ════════════════════════════════════════════════════════════════════════
    // COMPONENT ACCESS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Gets the underlying buffer.</summary>
    public Buffer Buffer { get; }

    /// <summary>Gets the buffer control.</summary>
    public BufferControl Control { get; }

    /// <summary>Gets the underlying window.</summary>
    public Window Window { get; }

    // ════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="TextArea"/> class.
    /// </summary>
    /// <param name="text">The initial text.</param>
    /// <param name="multiline">If true, allow multiline input.</param>
    /// <param name="password">When true, display using asterisks.</param>
    /// <param name="lexer">Lexer for syntax highlighting.</param>
    /// <param name="autoSuggest">Auto-suggest provider for input suggestions.</param>
    /// <param name="completer">Completer for auto completion.</param>
    /// <param name="completeWhileTyping">Whether to complete while typing.</param>
    /// <param name="validator">Input validator.</param>
    /// <param name="acceptHandler">Called when Enter is pressed.</param>
    /// <param name="history">History instance.</param>
    /// <param name="focusable">When true, allow this widget to receive the focus.</param>
    /// <param name="focusOnClick">When true, focus after mouse click.</param>
    /// <param name="wrapLines">When true, don't scroll horizontally, but wrap lines.</param>
    /// <param name="readOnly">When true, the buffer is read-only.</param>
    /// <param name="width">Window width dimension.</param>
    /// <param name="height">Window height dimension.</param>
    /// <param name="dontExtendHeight">When true, don't take up more height than preferred.</param>
    /// <param name="dontExtendWidth">When true, don't take up more width than preferred.</param>
    /// <param name="lineNumbers">When true, display line numbers in a left margin.</param>
    /// <param name="getLinePrefix">Callable returning formatted text for line prefixes.</param>
    /// <param name="scrollbar">When true, display a scroll bar.</param>
    /// <param name="style">A style string.</param>
    /// <param name="searchField">An optional SearchToolbar for search integration.</param>
    /// <param name="previewSearch">When true, preview search matches while typing.</param>
    /// <param name="prompt">Formatted text to display before each line.</param>
    /// <param name="inputProcessors">Additional input processors.</param>
    /// <param name="name">Buffer name.</param>
    public TextArea(
        string text = "",
        FilterOrBool multiline = default,
        FilterOrBool password = default,
        ILexer? lexer = null,
        IAutoSuggest? autoSuggest = null,
        ICompleter? completer = null,
        FilterOrBool completeWhileTyping = default,
        IValidator? validator = null,
        Func<Buffer, bool>? acceptHandler = null,
        Stroke.History.IHistory? history = null,
        FilterOrBool focusable = default,
        FilterOrBool focusOnClick = default,
        FilterOrBool wrapLines = default,
        FilterOrBool readOnly = default,
        Dimension? width = null,
        Dimension? height = null,
        FilterOrBool dontExtendHeight = default,
        FilterOrBool dontExtendWidth = default,
        bool lineNumbers = false,
        GetLinePrefixCallable? getLinePrefix = null,
        bool scrollbar = false,
        string style = "",
        SearchToolbar? searchField = null,
        FilterOrBool previewSearch = default,
        AnyFormattedText prompt = default,
        IReadOnlyList<IProcessor>? inputProcessors = null,
        string name = "")
    {
        // Apply defaults matching Python Prompt Toolkit
        if (!multiline.HasValue)
            multiline = new FilterOrBool(true);
        if (!password.HasValue)
            password = new FilterOrBool(false);
        if (!completeWhileTyping.HasValue)
            completeWhileTyping = new FilterOrBool(true);
        if (!focusable.HasValue)
            focusable = new FilterOrBool(true);
        if (!focusOnClick.HasValue)
            focusOnClick = new FilterOrBool(false);
        if (!wrapLines.HasValue)
            wrapLines = new FilterOrBool(true);
        if (!readOnly.HasValue)
            readOnly = new FilterOrBool(false);
        if (!dontExtendHeight.HasValue)
            dontExtendHeight = new FilterOrBool(false);
        if (!dontExtendWidth.HasValue)
            dontExtendWidth = new FilterOrBool(false);
        if (!previewSearch.HasValue)
            previewSearch = new FilterOrBool(true);

        inputProcessors ??= [];

        // Extract SearchBufferControl from SearchToolbar
        SearchBufferControl? searchControl = searchField?.Control;

        // Store mutable config fields for runtime changes
        Completer = completer;
        CompleteWhileTyping = completeWhileTyping;
        Lexer = lexer;
        AutoSuggest = autoSuggest;
        ReadOnly = readOnly;
        WrapLines = wrapLines;
        Validator = validator;

        // Create Buffer with dynamic delegates
        Buffer = new Buffer(
            document: new Document(text, 0),
            multiline: () => FilterUtils.IsTrue(multiline),
            readOnly: () => FilterUtils.IsTrue(this.ReadOnly),
            completer: new DynamicCompleter(() => this.Completer),
            completeWhileTyping: () => FilterUtils.IsTrue(this.CompleteWhileTyping),
            validator: new DynamicValidator(() => this.Validator),
            autoSuggest: new DynamicAutoSuggest(() => this.AutoSuggest),
            acceptHandler: acceptHandler,
            history: history,
            name: name);

        // Build input processor pipeline
        var allProcessors = new List<IProcessor>
        {
            new ConditionalProcessor(
                new AppendAutoSuggestion(),
                new FilterOrBool(
                    (Filter)Application.AppFilters.HasFocus(Buffer)
                    & ~(Filter)Application.AppFilters.IsDone)),
            new ConditionalProcessor(
                new PasswordProcessor(),
                password),
            new BeforeInput(prompt, style: "class:text-area.prompt"),
        };
        allProcessors.AddRange(inputProcessors);

        // Create BufferControl
        Control = new BufferControl(
            buffer: Buffer,
            lexer: new DynamicLexer(() => this.Lexer),
            inputProcessors: allProcessors,
            searchBufferControl: searchControl,
            previewSearch: previewSearch,
            focusable: focusable,
            focusOnClick: focusOnClick);

        // Configure margins based on multiline/scrollbar/lineNumbers
        IReadOnlyList<IMargin> leftMargins;
        IReadOnlyList<IMargin> rightMargins;

        if (FilterUtils.IsTrue(multiline))
        {
            rightMargins = scrollbar
                ? [new ScrollbarMargin(displayArrows: new FilterOrBool(true))]
                : [];
            leftMargins = lineNumbers
                ? [new NumberedMargin()]
                : [];
        }
        else
        {
            height = Dimension.Exact(1);
            leftMargins = [];
            rightMargins = [];
        }

        style = "class:text-area " + style;

        // If no height was given, guarantee height of at least 1.
        height ??= new Dimension(min: 1);

        // Create Window
        Window = new Window(
            height: height,
            width: width,
            dontExtendHeight: dontExtendHeight,
            dontExtendWidth: dontExtendWidth,
            content: Control,
            style: style,
            wrapLines: new FilterOrBool(new Condition(() => FilterUtils.IsTrue(this.WrapLines))),
            leftMargins: leftMargins,
            rightMargins: rightMargins,
            getLinePrefix: getLinePrefix);
    }

    /// <inheritdoc/>
    public IContainer PtContainer() => Window;
}
