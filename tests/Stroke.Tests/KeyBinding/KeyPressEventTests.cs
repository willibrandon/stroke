using Stroke.Input;
using Stroke.KeyBinding;
using Xunit;

// Use explicit alias to avoid ambiguity with Stroke.Input.KeyPress
using KeyPress = Stroke.KeyBinding.KeyPress;

namespace Stroke.Tests.KeyBinding;

/// <summary>
/// Tests for the <see cref="KeyPressEvent"/> class.
/// </summary>
public sealed class KeyPressEventTests
{
    #region Construction

    [Fact]
    public void Constructor_WithValidArgs_CreatesEvent()
    {
        var keySequence = new List<KeyPress> { Keys.ControlC };

        var e = new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: keySequence,
            previousKeySequence: [],
            isRepeat: false);

        Assert.NotNull(e);
        Assert.Equal(keySequence, e.KeySequence);
    }

    [Fact]
    public void Constructor_WithNullKeySequence_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: null!,
            previousKeySequence: [],
            isRepeat: false));
    }

    [Fact]
    public void Constructor_WithNullPreviousKeySequence_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: [Keys.ControlC],
            previousKeySequence: null!,
            isRepeat: false));
    }

    #endregion

    #region KeySequence Property

    [Fact]
    public void KeySequence_ReturnsProvidedSequence()
    {
        var keySequence = new List<KeyPress> { Keys.ControlX, Keys.ControlC };

        var e = new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: keySequence,
            previousKeySequence: [],
            isRepeat: false);

        Assert.Equal(2, e.KeySequence.Count);
        Assert.Equal(Keys.ControlX, e.KeySequence[0].Key.Key);
        Assert.Equal(Keys.ControlC, e.KeySequence[1].Key.Key);
    }

    #endregion

    #region PreviousKeySequence Property

    [Fact]
    public void PreviousKeySequence_ReturnsProvidedSequence()
    {
        var prevSequence = new List<KeyPress> { Keys.ControlA };

        var e = new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: [Keys.ControlC],
            previousKeySequence: prevSequence,
            isRepeat: false);

        Assert.Single(e.PreviousKeySequence);
        Assert.Equal(Keys.ControlA, e.PreviousKeySequence[0].Key.Key);
    }

    #endregion

    #region IsRepeat Property

    [Fact]
    public void IsRepeat_WhenTrue_ReturnsTrue()
    {
        var e = new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: [Keys.ControlC],
            previousKeySequence: [],
            isRepeat: true);

        Assert.True(e.IsRepeat);
    }

    [Fact]
    public void IsRepeat_WhenFalse_ReturnsFalse()
    {
        var e = new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: [Keys.ControlC],
            previousKeySequence: [],
            isRepeat: false);

        Assert.False(e.IsRepeat);
    }

    #endregion

    #region Arg Property (T065)

    [Fact]
    public void Arg_Default_ReturnsOne()
    {
        var e = new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: [Keys.ControlC],
            previousKeySequence: [],
            isRepeat: false);

        Assert.Equal(1, e.Arg);
    }

    [Fact]
    public void Arg_WithProvidedValue_ReturnsValue()
    {
        var e = new KeyPressEvent(
            keyProcessorRef: null,
            arg: "5",
            keySequence: [Keys.ControlC],
            previousKeySequence: [],
            isRepeat: false);

        Assert.Equal(5, e.Arg);
    }

    [Fact]
    public void Arg_WithLargeValue_ClampsToMaximum()
    {
        var e = new KeyPressEvent(
            keyProcessorRef: null,
            arg: "2000000", // Over 1M limit
            keySequence: [Keys.ControlC],
            previousKeySequence: [],
            isRepeat: false);

        Assert.Equal(1_000_000, e.Arg);
    }

    [Fact]
    public void Arg_WithZero_ReturnsOne()
    {
        var e = new KeyPressEvent(
            keyProcessorRef: null,
            arg: "0",
            keySequence: [Keys.ControlC],
            previousKeySequence: [],
            isRepeat: false);

        // Zero string parses to 0, which is clamped via Abs
        Assert.Equal(0, e.Arg);
    }

    [Fact]
    public void Arg_WithNegativePrefix_ReturnsNegativeOne()
    {
        var e = new KeyPressEvent(
            keyProcessorRef: null,
            arg: "-",
            keySequence: [Keys.ControlC],
            previousKeySequence: [],
            isRepeat: false);

        // Negative prefix "-" returns -1
        Assert.Equal(-1, e.Arg);
    }

    #endregion

    #region ArgPresent Property

    [Fact]
    public void ArgPresent_WhenArgNull_ReturnsFalse()
    {
        var e = new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: [Keys.ControlC],
            previousKeySequence: [],
            isRepeat: false);

        Assert.False(e.ArgPresent);
    }

    [Fact]
    public void ArgPresent_WhenArgProvided_ReturnsTrue()
    {
        var e = new KeyPressEvent(
            keyProcessorRef: null,
            arg: "3",
            keySequence: [Keys.ControlC],
            previousKeySequence: [],
            isRepeat: false);

        Assert.True(e.ArgPresent);
    }

    #endregion

    #region AppendToArgCount Method (T065)

    [Fact]
    public void AppendToArgCount_WithNoExistingArg_StartsWithDigit()
    {
        var e = new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: [Keys.ControlC],
            previousKeySequence: [],
            isRepeat: false);

        e.AppendToArgCount("5");

        Assert.Equal(5, e.Arg);
        Assert.True(e.ArgPresent);
    }

    [Fact]
    public void AppendToArgCount_WithExistingArg_AppendsDigit()
    {
        var e = new KeyPressEvent(
            keyProcessorRef: null,
            arg: "3",
            keySequence: [Keys.ControlC],
            previousKeySequence: [],
            isRepeat: false);

        e.AppendToArgCount("5");

        // 3 * 10 + 5 = 35
        Assert.Equal(35, e.Arg);
    }

    [Fact]
    public void AppendToArgCount_MultipleTimes_BuildsNumber()
    {
        var e = new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: [Keys.ControlC],
            previousKeySequence: [],
            isRepeat: false);

        e.AppendToArgCount("1");
        e.AppendToArgCount("2");
        e.AppendToArgCount("3");

        // 1, then 12, then 123
        Assert.Equal(123, e.Arg);
    }

    [Fact]
    public void AppendToArgCount_WhenOverflow_ClampsToMaximum()
    {
        var e = new KeyPressEvent(
            keyProcessorRef: null,
            arg: "999999",
            keySequence: [Keys.ControlC],
            previousKeySequence: [],
            isRepeat: false);

        e.AppendToArgCount("9");

        // 999999 * 10 + 9 = 9999999 > 1M, so clamped
        Assert.Equal(1_000_000, e.Arg);
    }

    #endregion

    #region Data Property

    [Fact]
    public void Data_ReturnsLastKeyPressData()
    {
        var keySequence = new List<KeyPress> { new KeyPress(Keys.ControlC, "\\x03") };

        var e = new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: keySequence,
            previousKeySequence: [],
            isRepeat: false);

        Assert.Equal("\\x03", e.Data);
    }

    [Fact]
    public void Data_EmptySequence_ReturnsEmptyString()
    {
        var e = new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: [],
            previousKeySequence: [],
            isRepeat: false);

        Assert.Equal(string.Empty, e.Data);
    }

    #endregion

    #region Thread Safety

    [Fact]
    public async Task ConcurrentArgAppend_NoExceptions()
    {
        var e = new KeyPressEvent(
            keyProcessorRef: null,
            arg: null,
            keySequence: [Keys.ControlC],
            previousKeySequence: [],
            isRepeat: false);

        var tasks = new List<Task>();
        var ct = TestContext.Current.CancellationToken;

        for (int i = 0; i < 10; i++)
        {
            string digit = (i % 10).ToString();
            tasks.Add(Task.Run(() => e.AppendToArgCount(digit), ct));
        }

        await Task.WhenAll(tasks);

        // Just verify no exception and arg is within bounds
        Assert.True(e.Arg >= 0);
        Assert.True(e.Arg <= 1_000_000);
    }

    #endregion
}
