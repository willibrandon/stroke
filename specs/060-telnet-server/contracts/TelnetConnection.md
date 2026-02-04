# Contract: TelnetConnection

**Namespace**: `Stroke.Contrib.Telnet`
**Python Source**: `prompt_toolkit.contrib.telnet.server.TelnetConnection`

## Class Signature

```csharp
namespace Stroke.Contrib.Telnet;

/// <summary>
/// Represents a single telnet client connection.
/// </summary>
/// <remarks>
/// <para>
/// This is a faithful port of Python Prompt Toolkit's <c>TelnetConnection</c> class.
/// Each connection has its own isolated input/output streams and can run a Stroke
/// application independently.
/// </para>
/// <para>
/// Thread safety: This class is thread-safe. Methods can be called from any thread,
/// but the connection's internal application context is associated with a specific
/// async context.
/// </para>
/// </remarks>
public sealed class TelnetConnection
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TelnetConnection"/> class.
    /// </summary>
    /// <param name="socket">The connected TCP socket.</param>
    /// <param name="remoteAddress">The remote client address.</param>
    /// <param name="interact">The interaction callback.</param>
    /// <param name="server">The parent server.</param>
    /// <param name="encoding">Character encoding.</param>
    /// <param name="style">Optional style for formatted text.</param>
    /// <param name="pipeInput">The pipe input for this connection.</param>
    /// <param name="enableCpr">Enable cursor position requests.</param>
    internal TelnetConnection(
        Socket socket,
        IPEndPoint remoteAddress,
        Func<TelnetConnection, Task> interact,
        TelnetServer server,
        Encoding encoding,
        IStyle? style,
        IPipeInput pipeInput,
        bool enableCpr);

    /// <summary>
    /// Gets the underlying TCP socket.
    /// </summary>
    public Socket Socket { get; }

    /// <summary>
    /// Gets the remote client address.
    /// </summary>
    public IPEndPoint RemoteAddress { get; }

    /// <summary>
    /// Gets the parent telnet server.
    /// </summary>
    public TelnetServer Server { get; }

    /// <summary>
    /// Gets the character encoding.
    /// </summary>
    public Encoding Encoding { get; }

    /// <summary>
    /// Gets the style for formatted text.
    /// </summary>
    public IStyle? Style { get; }

    /// <summary>
    /// Gets or sets the current terminal size.
    /// </summary>
    /// <remarks>
    /// Updated when NAWS data is received from the client.
    /// </remarks>
    public Size Size { get; }

    /// <summary>
    /// Gets whether this connection has been closed.
    /// </summary>
    public bool IsClosed { get; }

    /// <summary>
    /// Gets whether cursor position requests are enabled.
    /// </summary>
    public bool EnableCpr { get; }

    /// <summary>
    /// Sends formatted text to the client.
    /// </summary>
    /// <param name="formattedText">The formatted text to send.</param>
    /// <remarks>
    /// <para>
    /// The text is rendered using the connection's style and sent as ANSI escape
    /// sequences. This method is safe to call from any thread.
    /// </para>
    /// <para>
    /// If the connection is closed, this method is a no-op.
    /// </para>
    /// </remarks>
    public void Send(AnyFormattedText formattedText);

    /// <summary>
    /// Sends formatted text above the current prompt.
    /// </summary>
    /// <param name="formattedText">The formatted text to send.</param>
    /// <remarks>
    /// <para>
    /// This method uses the "run in terminal" pattern to print text above any
    /// active prompt without disrupting user input. Useful for notifications,
    /// chat messages, or alerts.
    /// </para>
    /// <para>
    /// Requires an active application context. If no application is running,
    /// throws <see cref="InvalidOperationException"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if called outside of an active application context.
    /// </exception>
    public void SendAbovePrompt(AnyFormattedText formattedText);

    /// <summary>
    /// Erases the screen and moves the cursor to the top-left position.
    /// </summary>
    /// <remarks>
    /// If the connection is closed, this method is a no-op.
    /// </remarks>
    public void EraseScreen();

    /// <summary>
    /// Closes this connection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Closes the socket, input, and output. After closing:
    /// <list type="bullet">
    /// <item><see cref="IsClosed"/> returns true</item>
    /// <item><see cref="Send"/> and other methods become no-ops</item>
    /// </list>
    /// </para>
    /// <para>
    /// This method is idempotent; multiple calls have no effect.
    /// </para>
    /// </remarks>
    public void Close();

    /// <summary>
    /// Feeds raw data to the telnet protocol parser.
    /// </summary>
    /// <param name="data">Raw bytes received from the socket.</param>
    /// <remarks>
    /// Internal use by TelnetServer. Parses telnet protocol sequences and
    /// forwards user data to the pipe input.
    /// </remarks>
    internal void Feed(ReadOnlySpan<byte> data);

    /// <summary>
    /// Runs the application for this connection.
    /// </summary>
    /// <returns>A task that completes when the connection ends.</returns>
    /// <remarks>
    /// Internal use by TelnetServer. Sets up the app session and invokes
    /// the interact callback.
    /// </remarks>
    internal Task RunApplicationAsync();
}
```

## Functional Requirements Coverage

| Requirement | Method/Property |
|-------------|-----------------|
| FR-007: Create isolated session | `RunApplicationAsync()` |
| FR-011: Clean up resources | `Close()` |
| FR-013: Send formatted text | `Send()` |
| FR-014: Send above prompt | `SendAbovePrompt()` |
| FR-015: Erase screen | `EraseScreen()` |
| FR-017: Notify on resize | `Size` property (updated via parser callback) |

## Python API Mapping

| Python | C# |
|--------|-----|
| `conn` | `Socket` |
| `addr` | `RemoteAddress` |
| `server` | `Server` |
| `encoding` | `Encoding` |
| `style` | `Style` |
| `size` | `Size` |
| `_closed` | `IsClosed` |
| `enable_cpr` | `EnableCpr` |
| `send(formatted_text)` | `Send(formattedText)` |
| `send_above_prompt(formatted_text)` | `SendAbovePrompt(formattedText)` |
| `erase_screen()` | `EraseScreen()` |
| `close()` | `Close()` |
| `feed(data)` | `Feed(data)` |
| `run_application()` | `RunApplicationAsync()` |
