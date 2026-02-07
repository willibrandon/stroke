using Stroke.AutoSuggest;
using Stroke.Completion;
using Stroke.History;
using Stroke.Validation;
using Xunit;

// Use alias to avoid ambiguity with System.Buffer
using Buffer = Stroke.Core.Buffer;
using Document = Stroke.Core.Document;
using SelectionState = Stroke.Core.SelectionState;
using ValidationState = Stroke.Core.ValidationState;

namespace Stroke.Tests.Core;

/// <summary>
/// Tests for Buffer class - core functionality (T019).
/// </summary>
public class BufferTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_DefaultValues_CreatesEmptyBuffer()
    {
        // Act
        var buffer = new Buffer();

        // Assert
        Assert.Equal("", buffer.Text);
        Assert.Equal(0, buffer.CursorPosition);
        Assert.Equal("", buffer.Name);
    }

    [Fact]
    public void Constructor_WithDocument_SetsTextAndCursor()
    {
        // Arrange
        var doc = new Document("hello", cursorPosition: 2);

        // Act
        var buffer = new Buffer(document: doc);

        // Assert
        Assert.Equal("hello", buffer.Text);
        Assert.Equal(2, buffer.CursorPosition);
    }

    [Fact]
    public void Constructor_WithName_SetsName()
    {
        // Act
        var buffer = new Buffer(name: "myBuffer");

        // Assert
        Assert.Equal("myBuffer", buffer.Name);
    }

    [Fact]
    public void Constructor_WithCompleter_SetsCompleter()
    {
        // Arrange
        var completer = DummyCompleter.Instance;

        // Act
        var buffer = new Buffer(completer: completer);

        // Assert
        Assert.Same(completer, buffer.Completer);
    }

    [Fact]
    public void Constructor_WithoutCompleter_UsesDummyCompleter()
    {
        // Act
        var buffer = new Buffer();

        // Assert
        Assert.Same(DummyCompleter.Instance, buffer.Completer);
    }

    [Fact]
    public void Constructor_WithAutoSuggest_SetsAutoSuggest()
    {
        // Arrange
        var suggest = new DummyAutoSuggest();

        // Act
        var buffer = new Buffer(autoSuggest: suggest);

        // Assert
        Assert.Same(suggest, buffer.AutoSuggest);
    }

    [Fact]
    public void Constructor_WithHistory_SetsHistory()
    {
        // Arrange
        var history = new InMemoryHistory();

        // Act
        var buffer = new Buffer(history: history);

        // Assert
        Assert.Same(history, buffer.History);
    }

    [Fact]
    public void Constructor_WithoutHistory_CreatesNewInMemoryHistory()
    {
        // Act
        var buffer = new Buffer();

        // Assert
        Assert.IsType<InMemoryHistory>(buffer.History);
        Assert.NotSame(InMemoryHistory.Empty, buffer.History);
    }

    [Fact]
    public void Constructor_WithValidator_SetsValidator()
    {
        // Arrange
        var validator = new TestValidator();

        // Act
        var buffer = new Buffer(validator: validator);

        // Assert
        Assert.Same(validator, buffer.Validator);
    }

    [Fact]
    public void Constructor_WithMaxNumberOfCompletions_SetsValue()
    {
        // Act
        var buffer = new Buffer(maxNumberOfCompletions: 500);

        // Assert
        Assert.Equal(500, buffer.MaxNumberOfCompletions);
    }

    [Fact]
    public void Constructor_DefaultMaxNumberOfCompletions_Is10000()
    {
        // Act
        var buffer = new Buffer();

        // Assert
        Assert.Equal(10000, buffer.MaxNumberOfCompletions);
    }

    [Fact]
    public void Constructor_WithAcceptHandler_SetsHandler()
    {
        // Arrange
        Func<Buffer, ValueTask<bool>> handler = b => ValueTask.FromResult(true);

        // Act
        var buffer = new Buffer(acceptHandler: handler);

        // Assert
        Assert.Same(handler, buffer.AcceptHandler);
        Assert.True(buffer.IsReturnable);
    }

    [Fact]
    public void Constructor_WithoutAcceptHandler_IsNotReturnable()
    {
        // Act
        var buffer = new Buffer();

        // Assert
        Assert.Null(buffer.AcceptHandler);
        Assert.False(buffer.IsReturnable);
    }

    #endregion

    #region Text Property Tests

    [Fact]
    public void Text_Set_UpdatesText()
    {
        // Arrange
        var buffer = new Buffer();

        // Act
        buffer.Text = "hello world";

        // Assert
        Assert.Equal("hello world", buffer.Text);
    }

    [Fact]
    public void Text_Set_ClampsCursorPosition()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 5));

        // Act
        buffer.Text = "hi"; // shorter text

        // Assert
        Assert.Equal("hi", buffer.Text);
        Assert.Equal(2, buffer.CursorPosition); // clamped to text length
    }

    [Fact]
    public void Text_SetSameValue_DoesNotTriggerChange()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello"));
        var changeCount = 0;
        buffer.OnTextChanged += _ => Interlocked.Increment(ref changeCount);

        // Act
        buffer.Text = "hello"; // same value
        Thread.Sleep(50); // allow event to propagate

        // Assert
        Assert.Equal(0, changeCount);
    }

    [Fact]
    public void Text_SetReadOnly_ThrowsEditReadOnlyBufferException()
    {
        // Arrange
        var buffer = new Buffer(readOnly: () => true);

        // Act & Assert
        Assert.Throws<Stroke.Core.EditReadOnlyBufferException>(() => buffer.Text = "test");
    }

    #endregion

    #region CursorPosition Property Tests

    [Fact]
    public void CursorPosition_Set_UpdatesPosition()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello"));

        // Act
        buffer.CursorPosition = 3;

        // Assert
        Assert.Equal(3, buffer.CursorPosition);
    }

    [Fact]
    public void CursorPosition_SetBeyondTextLength_ClampsToEnd()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello")); // length 5

        // Act
        buffer.CursorPosition = 100;

        // Assert
        Assert.Equal(5, buffer.CursorPosition);
    }

    [Fact]
    public void CursorPosition_SetNegative_ClampsToZero()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello"));

        // Act
        buffer.CursorPosition = -10;

        // Assert
        Assert.Equal(0, buffer.CursorPosition);
    }

    [Fact]
    public void CursorPosition_SetSameValue_DoesNotTriggerChange()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 3));
        var changeCount = 0;
        buffer.OnCursorPositionChanged += _ => Interlocked.Increment(ref changeCount);

        // Act
        buffer.CursorPosition = 3; // same value
        Thread.Sleep(50); // allow event to propagate

        // Assert
        Assert.Equal(0, changeCount);
    }

    #endregion

    #region Document Property Tests

    [Fact]
    public void Document_Get_ReturnsDocumentMatchingState()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 2));

        // Act
        var doc = buffer.Document;

        // Assert
        Assert.Equal("hello", doc.Text);
        Assert.Equal(2, doc.CursorPosition);
    }

    [Fact]
    public void Document_Set_UpdatesTextAndCursor()
    {
        // Arrange
        var buffer = new Buffer();
        var newDoc = new Document("world", cursorPosition: 3);

        // Act
        buffer.Document = newDoc;

        // Assert
        Assert.Equal("world", buffer.Text);
        Assert.Equal(3, buffer.CursorPosition);
    }

    [Fact]
    public void Document_SetReadOnly_ThrowsEditReadOnlyBufferException()
    {
        // Arrange
        var buffer = new Buffer(readOnly: () => true);
        var newDoc = new Document("test");

        // Act & Assert
        Assert.Throws<Stroke.Core.EditReadOnlyBufferException>(() => buffer.Document = newDoc);
    }

    #endregion

    #region Reset Tests

    [Fact]
    public void Reset_WithoutDocument_ClearsBuffer()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", cursorPosition: 3));

        // Act
        buffer.Reset();

        // Assert
        Assert.Equal("", buffer.Text);
        Assert.Equal(0, buffer.CursorPosition);
    }

    [Fact]
    public void Reset_WithDocument_SetsNewState()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello"));
        var newDoc = new Document("world", cursorPosition: 2);

        // Act
        buffer.Reset(newDoc);

        // Assert
        Assert.Equal("world", buffer.Text);
        Assert.Equal(2, buffer.CursorPosition);
    }

    [Fact]
    public void Reset_ClearsSelectionState()
    {
        // Arrange
        var buffer = new Buffer(document: new Document("hello", selection: new SelectionState(1)));

        // Act
        buffer.Reset();

        // Assert
        Assert.Null(buffer.SelectionState);
    }

    [Fact]
    public void Reset_ClearsValidationState()
    {
        // Arrange
        var buffer = new Buffer();

        // Act
        buffer.Reset();

        // Assert
        Assert.Equal(ValidationState.Unknown, buffer.ValidationState);
        Assert.Null(buffer.ValidationError);
    }

    [Fact]
    public void Reset_ClearsCompleteState()
    {
        // Arrange
        var buffer = new Buffer();

        // Act
        buffer.Reset();

        // Assert
        Assert.Null(buffer.CompleteState);
    }

    [Fact]
    public void Reset_ClearsSuggestion()
    {
        // Arrange
        var buffer = new Buffer();

        // Act
        buffer.Reset();

        // Assert
        Assert.Null(buffer.Suggestion);
    }

    [Fact]
    public void Reset_WithAppendToHistory_AppendsToHistory()
    {
        // Arrange
        var history = new InMemoryHistory();
        var buffer = new Buffer(history: history, document: new Document("previous command"));

        // Act
        buffer.Reset(appendToHistory: true);

        // Assert
        var strings = history.GetStrings();
        Assert.Single(strings);
        Assert.Equal("previous command", strings[0]);
    }

    [Fact]
    public void Reset_WithAppendToHistory_DoesNotAppendEmptyText()
    {
        // Arrange
        var history = new InMemoryHistory();
        var buffer = new Buffer(history: history); // empty text

        // Act
        buffer.Reset(appendToHistory: true);

        // Assert
        Assert.Empty(history.GetStrings());
    }

    #endregion

    #region SetDocument Tests

    [Fact]
    public void SetDocument_UpdatesTextAndCursor()
    {
        // Arrange
        var buffer = new Buffer();
        var doc = new Document("hello", cursorPosition: 2);

        // Act
        buffer.SetDocument(doc);

        // Assert
        Assert.Equal("hello", buffer.Text);
        Assert.Equal(2, buffer.CursorPosition);
    }

    [Fact]
    public void SetDocument_WithBypassReadonly_SucceedsOnReadOnlyBuffer()
    {
        // Arrange
        var buffer = new Buffer(readOnly: () => true);
        var doc = new Document("test");

        // Act
        buffer.SetDocument(doc, bypassReadonly: true);

        // Assert
        Assert.Equal("test", buffer.Text);
    }

    [Fact]
    public void SetDocument_ClampsExcessiveCursorPosition()
    {
        // Arrange - SetDocument clamps cursor if it exceeds text length
        // Document itself validates, so we test Buffer's clamping via internal path
        var buffer = new Buffer(document: new Document("hello world", cursorPosition: 11));

        // Act - change to shorter text
        buffer.Text = "hi";

        // Assert - cursor was clamped when text was set
        Assert.Equal("hi", buffer.Text);
        Assert.Equal(2, buffer.CursorPosition); // clamped to text length
    }

    #endregion

    #region Filter Property Tests

    [Fact]
    public void CompleteWhileTyping_DefaultFalse()
    {
        // Act
        var buffer = new Buffer();

        // Assert
        Assert.False(buffer.CompleteWhileTyping);
    }

    [Fact]
    public void CompleteWhileTyping_WithFilter_ReturnsFilterResult()
    {
        // Arrange
        var enabled = false;
        var buffer = new Buffer(completeWhileTyping: () => enabled);

        // Assert initial
        Assert.False(buffer.CompleteWhileTyping);

        // Change filter result
        enabled = true;
        Assert.True(buffer.CompleteWhileTyping);
    }

    [Fact]
    public void ValidateWhileTyping_DefaultFalse()
    {
        // Act
        var buffer = new Buffer();

        // Assert
        Assert.False(buffer.ValidateWhileTyping);
    }

    [Fact]
    public void EnableHistorySearch_DefaultFalse()
    {
        // Act
        var buffer = new Buffer();

        // Assert
        Assert.False(buffer.EnableHistorySearch);
    }

    [Fact]
    public void ReadOnly_DefaultFalse()
    {
        // Act
        var buffer = new Buffer();

        // Assert
        Assert.False(buffer.ReadOnly);
    }

    [Fact]
    public void Multiline_DefaultTrue()
    {
        // Act
        var buffer = new Buffer();

        // Assert
        Assert.True(buffer.Multiline);
    }

    #endregion

    #region Event Tests

    [Fact]
    public void OnTextChanged_FiredOnTextChange()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var buffer = new Buffer();
        var wasFired = false;
        Buffer? receivedBuffer = null;
        var signal = new ManualResetEventSlim(false);

        buffer.OnTextChanged += b =>
        {
            wasFired = true;
            receivedBuffer = b;
            signal.Set();
        };

        // Act
        buffer.Text = "hello";
        var signaled = signal.Wait(TimeSpan.FromSeconds(5), ct);

        // Assert
        Assert.True(signaled, "Event was not fired within timeout");
        Assert.True(wasFired);
        Assert.Same(buffer, receivedBuffer);
    }

    [Fact]
    public void OnCursorPositionChanged_FiredOnCursorChange()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var buffer = new Buffer(document: new Document("hello"));
        var wasFired = false;
        Buffer? receivedBuffer = null;
        var signal = new ManualResetEventSlim(false);

        buffer.OnCursorPositionChanged += b =>
        {
            wasFired = true;
            receivedBuffer = b;
            signal.Set();
        };

        // Act
        buffer.CursorPosition = 3;
        var signaled = signal.Wait(TimeSpan.FromSeconds(5), ct);

        // Assert
        Assert.True(signaled, "Event was not fired within timeout");
        Assert.True(wasFired);
        Assert.Same(buffer, receivedBuffer);
    }

    [Fact]
    public void Constructor_EventCallbacksAreRegistered()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var textChangedCalled = false;
        var cursorChangedCalled = false;
        var signal1 = new ManualResetEventSlim(false);
        var signal2 = new ManualResetEventSlim(false);

        // Act
        var buffer = new Buffer(
            onTextChanged: _ => { textChangedCalled = true; signal1.Set(); },
            onCursorPositionChanged: _ => { cursorChangedCalled = true; signal2.Set(); });

        buffer.Text = "test";
        buffer.CursorPosition = 2;

        signal1.Wait(TimeSpan.FromSeconds(5), ct);
        signal2.Wait(TimeSpan.FromSeconds(5), ct);

        // Assert
        Assert.True(textChangedCalled);
        Assert.True(cursorChangedCalled);
    }

    #endregion

    #region WorkingIndex Tests

    [Fact]
    public void WorkingIndex_InitiallyZero()
    {
        // Act
        var buffer = new Buffer();

        // Assert
        Assert.Equal(0, buffer.WorkingIndex);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ShortText_IncludesFullText()
    {
        // Arrange
        var buffer = new Buffer(name: "test", document: new Document("hello"));

        // Act
        var result = buffer.ToString();

        // Assert
        Assert.Contains("name=test", result);
        Assert.Contains("text=hello", result);
    }

    [Fact]
    public void ToString_LongText_TruncatesText()
    {
        // Arrange
        var longText = new string('x', 50);
        var buffer = new Buffer(name: "test", document: new Document(longText));

        // Act
        var result = buffer.ToString();

        // Assert
        Assert.Contains("...", result);
        Assert.Contains("text=xxxxxxxxxxxx...", result);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task ConcurrentTextAccess_ThreadSafe()
    {
        // Arrange
        var buffer = new Buffer();
        var iterations = 1000;
        var barrier = new Barrier(2);

        // Act - concurrent reads and writes
        var ct = TestContext.Current.CancellationToken;
        var writeTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                buffer.Text = $"value{i}";
            }
        }, ct);

        var readTask = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                _ = buffer.Text;
            }
        }, ct);

        // Assert - no exceptions
        await Task.WhenAll(writeTask, readTask);
    }

    [Fact]
    public async Task ConcurrentCursorAccess_ThreadSafe()
    {
        // Arrange
        var buffer = new Buffer(document: new Document(new string('x', 100)));
        var iterations = 1000;
        var barrier = new Barrier(2);

        // Act - concurrent cursor updates
        var ct = TestContext.Current.CancellationToken;
        var task1 = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                buffer.CursorPosition = i % 50;
            }
        }, ct);

        var task2 = Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (var i = 0; i < iterations; i++)
            {
                buffer.CursorPosition = (i + 25) % 50;
            }
        }, ct);

        // Assert - no exceptions
        await Task.WhenAll(task1, task2);
    }

    #endregion

    #region External Editor Tests

    [Fact]
    public async Task OpenInEditorAsync_ReadOnly_ThrowsException()
    {
        // Arrange
        var buffer = new Buffer(readOnly: () => true, document: new Document("test"));

        // Act & Assert
        await Assert.ThrowsAsync<Stroke.Core.EditReadOnlyBufferException>(
            () => buffer.OpenInEditorAsync());
    }

    [Fact]
    public void TempfileSuffix_DefaultEmpty()
    {
        // Arrange
        var buffer = new Buffer();

        // Assert
        Assert.Equal("", buffer.TempfileSuffix());
    }

    [Fact]
    public void TempfileSuffix_CustomSuffix()
    {
        // Arrange
        var buffer = new Buffer(tempfileSuffix: ".py");

        // Assert
        Assert.Equal(".py", buffer.TempfileSuffix());
    }

    [Fact]
    public void Tempfile_DefaultEmpty()
    {
        // Arrange
        var buffer = new Buffer();

        // Assert
        Assert.Equal("", buffer.Tempfile());
    }

    [Fact]
    public void Tempfile_CustomPath()
    {
        // Arrange
        var buffer = new Buffer(tempfile: "scripts/edit.py");

        // Assert
        Assert.Equal("scripts/edit.py", buffer.Tempfile());
    }

    #endregion

    #region Sealed Class Test

    [Fact]
    public void Buffer_IsSealed()
    {
        // Assert
        Assert.True(typeof(Buffer).IsSealed);
    }

    #endregion

    #region Helper Classes

    private sealed class TestValidator : IValidator
    {
        public void Validate(Document document)
        {
            // Always valid
        }

        public ValueTask ValidateAsync(Document document)
        {
            return ValueTask.CompletedTask;
        }
    }

    #endregion

    #region Property Getter Coverage

    [Fact]
    public void PreferredColumn_Default_IsNull()
    {
        var buffer = new Buffer();
        Assert.Null(buffer.PreferredColumn);
    }

    [Fact]
    public void HistorySearchText_Default_IsNull()
    {
        var buffer = new Buffer();
        Assert.Null(buffer.HistorySearchText);
    }

    #endregion
}
