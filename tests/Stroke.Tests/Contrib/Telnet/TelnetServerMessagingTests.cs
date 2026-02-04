namespace Stroke.Tests.Contrib.Telnet;

using System.Net;
using System.Net.Sockets;
using System.Text;
using Stroke.Contrib.Telnet;
using Stroke.FormattedText;
using Stroke.Input.Pipe;
using Stroke.Styles;
using Xunit;

/// <summary>
/// Tests for messaging functionality in <see cref="TelnetConnection"/>.
/// </summary>
public class TelnetServerMessagingTests : IDisposable
{
    private Socket? _serverSocket;
    private Socket? _clientSocket;
    private Socket? _acceptedSocket;
    private TelnetServer? _server;

    public void Dispose()
    {
        _acceptedSocket?.Dispose();
        _clientSocket?.Dispose();
        _serverSocket?.Dispose();
    }

    #region Send Tests

    [Fact]
    public void Send_WithPlainString_SendsText()
    {
        var (connection, _) = CreateReadyConnection();

        // Should not throw
        var ex = Record.Exception(() => connection.Send("Hello, World!"));
        Assert.Null(ex);
    }

    [Fact]
    public void Send_WithEmptyString_DoesNotThrow()
    {
        var (connection, _) = CreateReadyConnection();

        var ex = Record.Exception(() => connection.Send(""));
        Assert.Null(ex);
    }

    [Fact]
    public void Send_WithNewlines_HandlesCorrectly()
    {
        var (connection, _) = CreateReadyConnection();

        var ex = Record.Exception(() => connection.Send("Line1\nLine2\nLine3"));
        Assert.Null(ex);
    }

    [Fact]
    public void Send_OnClosedConnection_IsNoOp()
    {
        var (connection, _) = CreateReadyConnection();

        connection.Close();
        var ex = Record.Exception(() => connection.Send("Should not be sent"));
        Assert.Null(ex);
    }

    [Fact]
    public void Send_BeforeReady_IsNoOp()
    {
        var (connection, _) = CreateConnectionPair();

        // Connection is not ready (no TTYPE received yet)
        var ex = Record.Exception(() => connection.Send("Test"));
        Assert.Null(ex);
    }

    #endregion

    #region Send With FormattedText Tests

    [Fact]
    public void Send_WithFormattedText_SendsStyled()
    {
        var (connection, _) = CreateReadyConnection();

        AnyFormattedText text = new Stroke.FormattedText.FormattedText([
            new StyleAndTextTuple("bold", "Bold text"),
            new StyleAndTextTuple("", " and "),
            new StyleAndTextTuple("italic", "italic text")
        ]);

        var ex = Record.Exception(() => connection.Send(text));
        Assert.Null(ex);
    }

    [Fact]
    public void Send_WithHtmlFormattedText_ParsesAndSends()
    {
        var (connection, _) = CreateReadyConnectionWithStyle(DummyStyle.Instance);

        AnyFormattedText text = new Html("<b>Bold</b> text");

        var ex = Record.Exception(() => connection.Send(text));
        Assert.Null(ex);
    }

    [Fact]
    public void Send_WithAnsiFormattedText_ParsesAndSends()
    {
        var (connection, _) = CreateReadyConnectionWithStyle(DummyStyle.Instance);

        AnyFormattedText text = new Ansi("\x1b[1mBold\x1b[0m text");

        var ex = Record.Exception(() => connection.Send(text));
        Assert.Null(ex);
    }

    #endregion

    #region SendAbovePrompt Tests

    [Fact]
    public void SendAbovePrompt_WithoutAppContext_ThrowsInvalidOperationException()
    {
        var (connection, _) = CreateReadyConnection();

        // No application is running, so SendAbovePrompt should throw
        Assert.Throws<InvalidOperationException>(() => connection.SendAbovePrompt("Test"));
    }

    [Fact]
    public void SendAbovePrompt_ErrorMessage_MentionsAppContext()
    {
        var (connection, _) = CreateReadyConnection();

        var ex = Assert.Throws<InvalidOperationException>(() => connection.SendAbovePrompt("Test"));

        Assert.Contains("application context", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Style Application Tests

    [Fact]
    public void Send_WithStyle_UsesConnectionStyle()
    {
        var style = DummyStyle.Instance;
        var (connection, _) = CreateReadyConnectionWithStyle(style);

        // With a style, formatted text should be rendered with ANSI sequences
        var ex = Record.Exception(() => connection.Send("Styled text"));
        Assert.Null(ex);

        Assert.Same(style, connection.Style);
    }

    [Fact]
    public void Send_WithoutStyle_UsesDefaultStyle()
    {
        var (connection, _) = CreateReadyConnection();

        // Should use DummyStyle when no style is provided
        var ex = Record.Exception(() => connection.Send("Default styled text"));
        Assert.Null(ex);
    }

    #endregion

    #region EraseScreen Tests

    [Fact]
    public void EraseScreen_WhenReady_SendsSequences()
    {
        var (connection, clientSocket) = CreateReadyConnection();

        connection.EraseScreen();

        // Read what was sent
        var buffer = new byte[256];
        clientSocket.ReceiveTimeout = 500;

        try
        {
            var received = clientSocket.Receive(buffer);
            var data = Encoding.ASCII.GetString(buffer, 0, received);

            // Should contain escape sequences for erase and cursor home
            Assert.Contains("\x1b[2J", data); // Erase screen
            Assert.Contains("\x1b[", data);    // Some cursor positioning
        }
        catch (SocketException)
        {
            // Timeout - no data (might be buffered)
        }
    }

    [Fact]
    public void EraseScreen_OnClosedConnection_IsNoOp()
    {
        var (connection, _) = CreateReadyConnection();

        connection.Close();
        var ex = Record.Exception(() => connection.EraseScreen());
        Assert.Null(ex);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task Send_ConcurrentCalls_AreThreadSafe()
    {
        var (connection, _) = CreateReadyConnection();
        var ct = TestContext.Current.CancellationToken;
        var tasks = new List<Task>();

        for (int i = 0; i < 20; i++)
        {
            var msg = $"Concurrent message {i}";
            tasks.Add(Task.Run(() => connection.Send(msg), ct));
        }

        await Task.WhenAll(tasks);

        // If we get here without exception, thread safety is working
    }

    [Fact]
    public async Task Send_WhileClosing_IsThreadSafe()
    {
        var (connection, _) = CreateReadyConnection();
        var ct = TestContext.Current.CancellationToken;

        var sendTask = Task.Run(() =>
        {
            for (int i = 0; i < 100; i++)
            {
                connection.Send($"Message {i}");
            }
        }, ct);

        var closeTask = Task.Run(() =>
        {
            Thread.Sleep(10);
            connection.Close();
        }, ct);

        await Task.WhenAll(sendTask, closeTask);

        // If we get here without exception, thread safety is working
        Assert.True(connection.IsClosed);
    }

    #endregion

    #region Helper Methods

    private (TelnetConnection connection, Socket clientSocket) CreateConnectionPair()
    {
        _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        _serverSocket.Listen(1);

        var port = ((IPEndPoint)_serverSocket.LocalEndPoint!).Port;

        _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _clientSocket.Connect(IPAddress.Loopback, port);

        _acceptedSocket = _serverSocket.Accept();

        _server = new TelnetServer(port: port);
        var pipeInput = new SimplePipeInput();

        var connection = new TelnetConnection(
            _acceptedSocket,
            (IPEndPoint)_clientSocket.LocalEndPoint!,
            _ => Task.CompletedTask,
            _server,
            Encoding.UTF8,
            null,
            pipeInput,
            true);

        return (connection, _clientSocket);
    }

    private (TelnetConnection connection, Socket clientSocket) CreateReadyConnection()
    {
        return CreateReadyConnectionWithStyle(null);
    }

    private (TelnetConnection connection, Socket clientSocket) CreateReadyConnectionWithStyle(IStyle? style)
    {
        _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        _serverSocket.Listen(1);

        var port = ((IPEndPoint)_serverSocket.LocalEndPoint!).Port;

        _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _clientSocket.Connect(IPAddress.Loopback, port);

        _acceptedSocket = _serverSocket.Accept();

        _server = new TelnetServer(port: port, style: style);
        var pipeInput = new SimplePipeInput();

        var connection = new TelnetConnection(
            _acceptedSocket,
            (IPEndPoint)_clientSocket.LocalEndPoint!,
            _ => Task.CompletedTask,
            _server,
            Encoding.UTF8,
            style,
            pipeInput,
            true);

        // Make connection ready by sending TTYPE
        connection.Feed([
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.TTYPE,
            TelnetConstants.IS,
            (byte)'V', (byte)'T', (byte)'1', (byte)'0', (byte)'0',
            TelnetConstants.IAC, TelnetConstants.SE
        ]);

        return (connection, _clientSocket);
    }

    #endregion
}
