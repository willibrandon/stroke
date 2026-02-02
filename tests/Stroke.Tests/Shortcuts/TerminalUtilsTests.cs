using Stroke.Output;
using Stroke.Shortcuts;
using Xunit;

namespace Stroke.Tests.Shortcuts;

public class TerminalUtilsTests
{
    [Fact]
    public void Clear_EmitsEraseScreenAndCursorHomeSequences()
    {
        // US4-AS1, SC-006: Clear() emits erase screen + cursor home + flush
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        // Create a session with our Vt100Output so TerminalUtils.Clear() uses it
        using var session = Stroke.Application.AppContext.CreateAppSession(output: output);

        TerminalUtils.Clear();
        output.Flush();

        var raw = writer.ToString();

        // Verify erase screen sequence: ESC [ 2 J
        Assert.Contains("\x1b[2J", raw);
    }

    [Fact]
    public void SetTitle_EmitsTitleEscapeSequence()
    {
        // US4-AS2, SC-007: SetTitle emits title-setting escape sequence
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        using var session = Stroke.Application.AppContext.CreateAppSession(output: output);

        TerminalUtils.SetTitle("My App");
        output.Flush();

        var raw = writer.ToString();

        // Verify title-setting escape sequence: ESC ] 2 ; <title> BEL
        Assert.Contains("\x1b]2;My App\x07", raw);
    }

    [Fact]
    public void ClearTitle_EmitsEmptyTitleSequence()
    {
        // US4-AS3, SC-007: ClearTitle calls SetTitle("")
        var writer = new StringWriter();
        var output = Vt100Output.FromPty(writer);

        using var session = Stroke.Application.AppContext.CreateAppSession(output: output);

        TerminalUtils.ClearTitle();
        output.Flush();

        var raw = writer.ToString();

        // Verify empty title sequence: ESC ] 2 ; BEL
        Assert.Contains("\x1b]2;\x07", raw);
    }
}
